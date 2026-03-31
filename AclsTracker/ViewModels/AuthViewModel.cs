using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AclsTracker.Models;
using AclsTracker.Services.Auth;
using AclsTracker.Services.Sync;
using Microsoft.Extensions.Logging;
using Supabase.Gotrue.Exceptions;

namespace AclsTracker.ViewModels;

/// <summary>
/// ViewModel for all authentication UI: LoginPage, RegisterPage, and ProfilePage.
/// Manages auth state, form fields, and delegates all operations to IAuthService.
/// Uses CommunityToolkit.Mvvm with ObservableProperty and RelayCommand patterns.
/// </summary>
public partial class AuthViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ISessionSyncService _syncService;
    private readonly ILogger<AuthViewModel>? _logger;

    // ============ Login Form Fields ============

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    // ============ Register Form Fields ============

    [ObservableProperty]
    private string _registerEmail = string.Empty;

    [ObservableProperty]
    private string _registerPassword = string.Empty;

    [ObservableProperty]
    private string _nombre = string.Empty;

    [ObservableProperty]
    private string _apellido = string.Empty;

    [ObservableProperty]
    private string _telefono = string.Empty;

    // ============ Auth State ============

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private string _userDisplayName = string.Empty;

    [ObservableProperty]
    private string? _userAvatarUrl;

    // ============ UI State ============

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private UserProfile? _currentProfile;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isGoogleAvailable;

    [ObservableProperty]
    private bool _isAppleAvailable;

    public AuthViewModel(
        IAuthService authService,
        ISessionSyncService syncService,
        ILogger<AuthViewModel>? logger = null)
    {
        _authService = authService;
        _syncService = syncService;
        _logger = logger;

        // Subscribe to auth state changes
        _authService.AuthStateChanged += OnAuthStateChanged;

        // Check initial auth state
        UpdateAuthState();

        // Platform availability for OAuth providers
#if WINDOWS
        IsGoogleAvailable = false;
        IsAppleAvailable = false;
#elif ANDROID
        IsGoogleAvailable = true;
        IsAppleAvailable = false; // Apple Sign-In on Android uses web flow, available but less common
#elif IOS || MACCATALYST
        IsGoogleAvailable = true;
        IsAppleAvailable = true;
#else
        IsGoogleAvailable = true;
        IsAppleAvailable = true;
#endif
    }

    private void UpdateAuthState()
    {
        IsLoggedIn = _authService.IsLoggedIn;
        if (_authService.IsLoggedIn)
        {
            UserDisplayName = _authService.CurrentUserEmail ?? "Usuario";
            // Kick off a background load so the avatar populates once the profile is fetched.
            // Do not await — this is called from the constructor where async is not available.
            _ = Task.Run(async () =>
            {
                var profile = await _authService.GetProfileAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    CurrentProfile = profile;
                    UserAvatarUrl = profile?.AvatarUrl ?? _authService.CurrentUserAvatarUrl;
                });
            });
        }
    }

    private void OnAuthStateChanged(object? sender, bool isLoggedIn)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            IsLoggedIn = isLoggedIn;
            if (isLoggedIn)
            {
                UserDisplayName = _authService.CurrentUserEmail ?? "Usuario";
                // Populate avatar from profile; CurrentUserAvatarUrl only has data after
                // GetProfileAsync() populates _cachedAvatarUrl (or from OAuth metadata).
                var profile = await _authService.GetProfileAsync();
                CurrentProfile = profile;
                UserAvatarUrl = profile?.AvatarUrl ?? _authService.CurrentUserAvatarUrl;
            }
            else
            {
                UserDisplayName = string.Empty;
                UserAvatarUrl = null;
                CurrentProfile = null;
            }
        });
    }

    // ============ Email/Password Commands ============

    [RelayCommand]
    private async Task SignInWithEmailAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor complete email y contrasena";
            Toast.Make("Por favor complete email y contrasena").Show();
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var success = await _authService.SignInWithEmailAsync(Email, Password);
            if (success)
            {
                Email = string.Empty;
                Password = string.Empty;
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ErrorMessage = "Error al iniciar sesion. Verifique sus credenciales.";
                Toast.Make("Error al iniciar sesion").Show();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SignInWithEmail failed");
            ErrorMessage = "Error al iniciar sesion. Intente nuevamente.";
            Toast.Make("Error al iniciar sesion").Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SignUpAsync()
    {
        if (string.IsNullOrWhiteSpace(RegisterEmail) || string.IsNullOrWhiteSpace(RegisterPassword))
        {
            ErrorMessage = "Por favor complete email y contrasena";
            Toast.Make("Por favor complete email y contrasena").Show();
            return;
        }

        if (string.IsNullOrWhiteSpace(Nombre))
        {
            ErrorMessage = "Por favor ingrese su nombre";
            Toast.Make("Por favor ingrese su nombre").Show();
            return;
        }

        if (string.IsNullOrWhiteSpace(Apellido))
        {
            ErrorMessage = "Por favor ingrese su apellido";
            Toast.Make("Por favor ingrese su apellido").Show();
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var success = await _authService.SignUpWithEmailAsync(
                RegisterEmail,
                RegisterPassword,
                Nombre,
                Apellido,
                Telefono);

            if (success)
            {
                if (_authService.IsLoggedIn)
                {
                    // Auto-confirmed: user is already logged in, go to main tab
                    Toast.Make("Registro exitoso. Bienvenido!").Show();
                    ClearRegisterForm();
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    // Email verification pending
                    Toast.Make("Revisa tu email para verificar tu cuenta").Show();
                    ClearRegisterForm();
                    await Shell.Current.GoToAsync("LoginPage");
                }
            }
            else
            {
                ErrorMessage = "Error al registrarse. Intente nuevamente.";
                Toast.Make("Error al registrarse").Show();
            }
        }
        catch (GotrueException ex) when (ex.StatusCode == 429)
        {
            _logger?.LogWarning(ex, "SignUp rate limited by Supabase (429)");
            ErrorMessage = "Demasiados intentos de registro. Espera unos minutos e intenta nuevamente.";
            Toast.Make("Limite de intentos alcanzado. Espera unos minutos.").Show();
        }
        catch (GotrueException ex)
        {
            _logger?.LogError(ex, "SignUp GotrueException: {StatusCode}", ex.StatusCode);
            ErrorMessage = "Error al registrarse. Intente nuevamente.";
            Toast.Make("Error al registrarse").Show();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SignUp failed");
            ErrorMessage = "Error al registrarse. Intente nuevamente.";
            Toast.Make("Error al registrarse").Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            Toast.Make("Por favor ingrese su email").Show();
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var success = await _authService.ResetPasswordAsync(Email);
            if (success)
            {
                Toast.Make("Email de recuperacion enviado").Show();
            }
            else
            {
                Toast.Make("No se pudo enviar el email de recuperacion").Show();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ResetPassword failed");
            Toast.Make("Error al enviar email de recuperacion").Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ============ OAuth Commands ============

    [RelayCommand]
    private async Task SignInWithGoogleAsync()
    {
        if (!IsGoogleAvailable) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var success = await _authService.SignInWithGoogleAsync();
            if (success)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ErrorMessage = "Error al iniciar sesion con Google.";
                Toast.Make("Error al iniciar sesion con Google").Show();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SignInWithGoogle failed");
            ErrorMessage = "Error al iniciar sesion con Google.";
            Toast.Make("Error al iniciar sesion con Google").Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SignInWithAppleAsync()
    {
        if (!IsAppleAvailable) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var success = await _authService.SignInWithAppleAsync();
            if (success)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ErrorMessage = "Error al iniciar sesion con Apple.";
                Toast.Make("Error al iniciar sesion con Apple").Show();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SignInWithApple failed");
            ErrorMessage = "Error al iniciar sesion con Apple.";
            Toast.Make("Error al iniciar sesion con Apple").Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ============ Session Management ============

    [RelayCommand]
    private async Task SignOutAsync()
    {
        IsBusy = true;

        try
        {
            // Delete user's local sessions BEFORE signing out (CurrentUserId will be null after)
            var userId = _authService.CurrentUserId;
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    await _syncService.DeleteLocalUserSessionsAsync(userId);
                }
                catch (Exception cleanupEx)
                {
                    _logger?.LogError(cleanupEx, "Failed to delete local sessions on logout");
                    // Continue with logout even if cleanup fails
                }
            }

            await _authService.SignOutAsync();
            CurrentProfile = null;
            UserDisplayName = string.Empty;
            UserAvatarUrl = null;
            Toast.Make("Sesion cerrada").Show();
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SignOut failed");
            Toast.Make("Error al cerrar sesion").Show();
            // Navigate anyway — sign-out fires AuthStateChanged(false) even on exception
            await Shell.Current.GoToAsync("//MainPage");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ============ Profile Commands ============

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        if (!_authService.IsLoggedIn) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            CurrentProfile = await _authService.GetProfileAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "LoadProfile failed");
            ErrorMessage = "Error al cargar perfil.";
            Toast.Make("Error al cargar perfil").Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (CurrentProfile == null) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var success = await _authService.UpdateProfileAsync(CurrentProfile);
            if (success)
            {
                Toast.Make("Perfil actualizado").Show();
            }
            else
            {
                ErrorMessage = "Error al guardar cambios.";
                Toast.Make("Error al guardar cambios").Show();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SaveProfile failed");
            ErrorMessage = "Error al guardar cambios.";
            Toast.Make("Error al guardar cambios").Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UploadAvatarAsync()
    {
        if (!_authService.IsLoggedIn) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result != null)
            {
                var avatarUrl = await _authService.UploadAvatarAsync(result.FullPath);
                if (avatarUrl != null)
                {
                    UserAvatarUrl = avatarUrl;
                    if (CurrentProfile != null)
                    {
                        CurrentProfile.AvatarUrl = avatarUrl;
                    }
                    Toast.Make("Foto de perfil actualizada").Show();
                }
                else
                {
                    Toast.Make("Error al subir foto").Show();
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UploadAvatar failed");
            Toast.Make("Error al subir foto").Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ============ Navigation Commands ============

    [RelayCommand]
    private async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("LoginPage");
    }

    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await Shell.Current.GoToAsync("RegisterPage");
    }

    [RelayCommand]
    private async Task NavigateToProfileAsync()
    {
        await Shell.Current.GoToAsync("ProfilePage");
    }

    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    // ============ Helper Methods ============

    private void ClearRegisterForm()
    {
        RegisterEmail = string.Empty;
        RegisterPassword = string.Empty;
        Nombre = string.Empty;
        Apellido = string.Empty;
        Telefono = string.Empty;
    }
}
