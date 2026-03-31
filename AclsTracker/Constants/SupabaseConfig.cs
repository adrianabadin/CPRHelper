namespace AclsTracker.Constants;

/// <summary>
/// Supabase configuration loaded from environment variables.
/// In development: set via terminal (see commands below).
/// In production: set via app store console or CI/CD pipeline.
/// The anon key is public by design - safe to expose in the client.
/// </summary>
public static class SupabaseConfig
{
    /// <summary>
    /// Your Supabase project URL.
    /// 
    /// Development - Windows PowerShell:
    ///   $env:SUPABASE_URL = "https://tu-proyecto.supabase.co"
    /// 
    /// Development - Windows CMD:
    ///   set SUPABASE_URL=https://tu-proyecto.supabase.co
    /// 
    /// Found in: Supabase Dashboard → Settings → API → Project URL
    /// </summary>
    public static string Url => 
        Environment.GetEnvironmentVariable("SUPABASE_URL") ?? "YOUR_SUPABASE_URL";

    /// <summary>
    /// Your Supabase anonymous (public) key.
    /// 
    /// Development - Windows PowerShell:
    ///   $env:SUPABASE_ANONKEY = "eyJh..."
    /// 
    /// Development - Windows CMD:
    ///   set SUPABASE_ANONKEY=eyJh...
    /// 
    /// Found in: Supabase Dashboard → Settings → API → Project API keys → anon public
    /// </summary>
    public static string AnonKey => 
        Environment.GetEnvironmentVariable("SUPABASE_ANONKEY") ?? "YOUR_SUPABASE_ANONKEY";

    /// <summary>
    /// OAuth redirect URI registered in Supabase dashboard.
    /// Must match exactly: Supabase Dashboard → Authentication → URL Configuration → Redirect URLs
    /// </summary>
    public const string RedirectUri = "aclstracker://callback";
}
