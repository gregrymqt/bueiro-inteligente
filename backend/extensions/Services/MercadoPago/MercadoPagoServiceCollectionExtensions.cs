using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions.Services.MercadoPago;

public static class MercadoPagoServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteMercadoPago(
        this IServiceCollection services
    )
    {
        // Aqui configuraremos futuramente o HttpClient do Mercado Pago
        // ou a injeção do SDK oficial e dos seus Services/Repositories de pagamento.
        // Exemplo: services.AddScoped<IMercadoPagoService, MercadoPagoService>();

        return services;
    }
}
