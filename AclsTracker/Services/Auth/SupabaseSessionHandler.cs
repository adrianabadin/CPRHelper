using Supabase.Gotrue.Interfaces;
using Supabase.Gotrue;
using Microsoft.Maui.Authentication;
using System.Text.Json;

namespace AclsTracker.Services.Auth;

/// <summary>
/// Session persistence handler for Supabase Auth using MAUI SecureStorage.
/// Encrypts and stores JWT tokens securely on device.
/// </summary>
public class SupabaseSessionHandler : IGotrueSessionPersistence<Session>
{
    private const string SessionKey = "supabase_session";
    
    /// <summary>
    /// JSON serializer options for Session serialization.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Save session to secure storage.
    /// Uses MainThread for async fire-and-forget to ensure UI thread safety.
    /// </summary>
    public void SaveSession(Session session)
    {
        if (session == null) return;

        var json = JsonSerializer.Serialize(session, JsonOptions);
        
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await SecureStorage.Default.SetAsync(SessionKey, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SupabaseSessionHandler] Failed to save session: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// Load session from secure storage synchronously.
    /// Uses GetAwaiter().GetResult() to bridge async SecureStorage with sync interface.
    /// </summary>
    public Session? LoadSession()
    {
        try
        {
            var json = SecureStorage.Default.GetAsync(SessionKey).GetAwaiter().GetResult();
            
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonSerializer.Deserialize<Session>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SupabaseSessionHandler] Failed to load session: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Destroy session from secure storage.
    /// </summary>
    public void DestroySession()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                SecureStorage.Default.Remove(SessionKey);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SupabaseSessionHandler] Failed to destroy session: {ex.Message}");
        }
    }
}
