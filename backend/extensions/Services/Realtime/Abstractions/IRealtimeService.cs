namespace backend.extensions.Services.Realtime.Abstractions;

public interface IRealtimeService
{
    // Envia dados para todos os usuários conectados em um tópico específico
    Task PublishAsync(string eventName, object data);

    // Envia dados apenas para um usuário específico (útil para pagamentos)
    Task PublishToUserAsync(string userId, string eventName, object data);
}