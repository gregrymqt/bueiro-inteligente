using backend.Core;
using backend.Extensions.Auth.Models;
using backend.extensions.Services.Auth.Abstractions;
using backend.extensions.Services.Auth.Models;

namespace backend.extensions.Services.Auth.Logic;

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

        IReadOnlyList<string> userRoles = currentUser.Roles;

        if (_strictDbCheck)
        {
            if (_roleProvider is null)
            {
                throw new InvalidOperationException(
                    "strictDbCheck foi habilitado, mas nenhum IUserRoleProvider foi registrado."
                );
            }

            IReadOnlyList<string>? freshRoles = await _roleProvider
                .GetRolesByEmailAsync(currentUser.Email, cancellationToken)
                .ConfigureAwait(false);

            if (freshRoles is null || freshRoles.Count == 0)
            {
                throw new UnauthorizedAccessException(
                    "Usuário ou role não encontrados no sistema."
                );
            }

            userRoles = freshRoles;
            currentUser.Roles = freshRoles;
        }

        if (!userRoles.Any(role => _allowedRoles.Contains(role)))
        {
            string allowedRolesText = string.Join(", ", _allowedRoles);

            throw new UnauthorizedAccessException(
                $"Acesso negado: este recurso exige uma das roles [{allowedRolesText}], mas você possui as roles [{string.Join(", ", userRoles)}]."
            );
        }

        return currentUser;
    }
}
