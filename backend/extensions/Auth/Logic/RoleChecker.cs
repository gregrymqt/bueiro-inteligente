using backend.Core;
using backend.Extensions.Auth.Abstractions;
using backend.Extensions.Auth.Models;

namespace backend.Extensions.Auth.Logic;

public sealed class RoleChecker
{
    private readonly HashSet<string> _allowedRoles;
    private readonly bool _strictDbCheck;
    private readonly IUserRoleProvider? _roleProvider;

    public RoleChecker(
        IEnumerable<string> allowedRoles,
        bool strictDbCheck = false,
        IUserRoleProvider? roleProvider = null
    )
    {
        if (allowedRoles is null)
        {
            throw new ArgumentNullException(nameof(allowedRoles));
        }

        _allowedRoles = new HashSet<string>(allowedRoles, StringComparer.OrdinalIgnoreCase);
        _strictDbCheck = strictDbCheck;
        _roleProvider = roleProvider;
    }

    public async Task<UserTokenData> AuthorizeAsync(
        UserTokenData currentUser,
        CancellationToken cancellationToken = default
    )
    {
        if (currentUser is null)
        {
            throw LogicException.NullValue(nameof(currentUser));
        }

        string userRole = currentUser.Role;

        if (_strictDbCheck)
        {
            if (_roleProvider is null)
            {
                throw new InvalidOperationException(
                    "strictDbCheck foi habilitado, mas nenhum IUserRoleProvider foi registrado."
                );
            }

            string? freshRole = await _roleProvider
                .GetRoleByEmailAsync(currentUser.Email, cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(freshRole))
            {
                throw new UnauthorizedAccessException("Usuário ou role não encontrados no sistema.");
            }

            userRole = freshRole;
            currentUser.Role = freshRole;
        }

        if (!_allowedRoles.Contains(userRole))
        {
            string allowedRolesText = string.Join(", ", _allowedRoles);

            throw new UnauthorizedAccessException(
                $"Acesso negado: este recurso exige uma das roles [{allowedRolesText}], mas você possui a role '{userRole}'."
            );
        }

        return currentUser;
    }
}
