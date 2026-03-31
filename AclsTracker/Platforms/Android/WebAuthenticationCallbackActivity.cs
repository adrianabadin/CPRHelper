using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Authentication;

namespace AclsTracker.Platforms.Android;

/// <summary>
/// Activity that handles OAuth callback URLs for the aclstracker scheme.
/// This enables Supabase OAuth authentication to return to the app
/// after the user authenticates in the system browser.
/// </summary>
[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "aclstracker")]
public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
{
}
