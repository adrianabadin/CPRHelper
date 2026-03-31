namespace AclsTracker.Constants;

/// <summary>
/// Supabase configuration constants.
/// These values are safe to embed in the client application.
/// The anon key is public by design and only allows limited operations.
/// Replace placeholder values with your actual Supabase project credentials.
/// </summary>
public static class SupabaseConfig
{
    /// <summary>
    /// Your Supabase project URL.
    /// Found in: Supabase Dashboard → Settings → API → Project URL
    /// </summary>
    public const string Url = "YOUR_SUPABASE_URL";

    /// <summary>
    /// Your Supabase anonymous (public) key.
    /// Found in: Supabase Dashboard → Settings → API → Project API keys → anon public
    /// </summary>
    public const string AnonKey = "YOUR_SUPABASE_ANON_KEY";

    /// <summary>
    /// OAuth redirect URI registered in Supabase dashboard.
    /// Must match exactly: Supabase Dashboard → Authentication → URL Configuration → Redirect URLs
    /// </summary>
    public const string RedirectUri = "aclstracker://callback";
}
