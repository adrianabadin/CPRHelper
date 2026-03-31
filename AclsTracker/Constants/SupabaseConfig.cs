using Microsoft.Extensions.Configuration;

namespace AclsTracker.Constants;

/// <summary>
/// Supabase configuration loaded from .NET User Secrets (dev) or environment variables (prod).
/// The anon key is public by design - safe to expose in the client.
/// 
/// Setup:
///   cd AclsTracker
///   dotnet user-secrets init
///   dotnet user-secrets set "Supabase:Url" "https://tu-proyecto.supabase.co"
///   dotnet user-secrets set "Supabase:AnonKey" "eyJh..."
/// </summary>
public static class SupabaseConfig
{
    private static IConfiguration? _configuration;

    /// <summary>
    /// Initialize configuration with User Secrets support.
    /// Called from MauiProgram.cs before accessing Url or AnonKey.
    /// </summary>
    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Your Supabase project URL.
    /// Found in: Supabase Dashboard → Settings → API → Project URL
    /// </summary>
    public static string Url => 
        _configuration?["Supabase:Url"] 
        ?? Environment.GetEnvironmentVariable("SUPABASE_URL") 
        ?? "YOUR_SUPABASE_URL";

    /// <summary>
    /// Your Supabase anonymous (public) key.
    /// Found in: Supabase Dashboard → Settings → API → Project API keys → anon public
    /// </summary>
    public static string AnonKey => 
        _configuration?["Supabase:AnonKey"] 
        ?? Environment.GetEnvironmentVariable("SUPABASE_ANONKEY") 
        ?? "YOUR_SUPABASE_ANONKEY";

    /// <summary>
    /// OAuth redirect URI registered in Supabase dashboard.
    /// Must match exactly: Supabase Dashboard → Authentication → URL Configuration → Redirect URLs
    /// </summary>
    public const string RedirectUri = "aclstracker://callback";
}
