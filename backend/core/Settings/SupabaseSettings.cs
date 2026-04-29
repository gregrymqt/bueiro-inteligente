namespace backend.Core.Settings;

/// <summary>
/// Configurações de integração com Supabase Storage.
/// Seção do appsettings.json: "Supabase"
/// </summary>
public sealed class SupabaseSettings
{
    public const string SectionName = "Supabase";

    /// <summary>
    /// URL do projeto Supabase (ex: https://seu-projeto.supabase.co)
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Chave de acesso público do Supabase (anon key)
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Indica se o sistema deve usar Supabase Storage como destino principal dos uploads.
    /// Quando false, mantém o fallback para armazenamento local em wwwroot/uploads.
    /// </summary>
    public bool UseStorage { get; set; }
}
