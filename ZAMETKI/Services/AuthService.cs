using System.Net;
using ZAMETKI.Api;
using ZAMETKI.Api.Contracts;

namespace ZAMETKI.Services;

public class AuthService
{
    private const string TokenKey = "auth.token";

    private readonly ApiClient _api;

    public AuthService(ApiClient api) => _api = api;

    public string? Token { get; private set; }

    public UserDto? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public event EventHandler? AuthStateChanged;

    public async Task<bool> RestoreAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey).ConfigureAwait(false);
        if (string.IsNullOrEmpty(token)) return false;

        _api.Token = token;
        try
        {
            var me = await _api.GetAsync<UserDto>("/api/auth/me").ConfigureAwait(false);
            Token = token;
            CurrentUser = me;
            RaiseStateChanged();
            return true;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            await ClearAsync().ConfigureAwait(false);
            return false;
        }
    }

    public async Task LoginAsync(string fullName, string password)
    {
        _api.Token = null;
        var resp = await _api.PostAsync<AuthResponse>("/api/auth/login", new LoginRequest(fullName, password)).ConfigureAwait(false);
        await ApplyAuthAsync(resp).ConfigureAwait(false);
    }

    public async Task RegisterStudentAsync(string groupName, string fullName, string password)
    {
        _api.Token = null;
        var resp = await _api.PostAsync<AuthResponse>("/api/auth/register/student",
            new RegisterStudentRequest(groupName, fullName, password)).ConfigureAwait(false);
        await ApplyAuthAsync(resp).ConfigureAwait(false);
    }

    public async Task RegisterTeacherAsync(string fullName, string password)
    {
        _api.Token = null;
        var resp = await _api.PostAsync<AuthResponse>("/api/auth/register/teacher",
            new RegisterTeacherRequest(fullName, password)).ConfigureAwait(false);
        await ApplyAuthAsync(resp).ConfigureAwait(false);
    }

    public async Task LogoutAsync()
    {
        if (!string.IsNullOrEmpty(Token))
        {
            try { await _api.PostAsync("/api/auth/logout", null).ConfigureAwait(false); }
            catch (ApiException) { }
        }
        await ClearAsync().ConfigureAwait(false);
    }

    private async Task ApplyAuthAsync(AuthResponse resp)
    {
        Token = resp.Token;
        CurrentUser = resp.User;
        _api.Token = resp.Token;
        await SecureStorage.Default.SetAsync(TokenKey, resp.Token).ConfigureAwait(false);
        RaiseStateChanged();
    }

    private async Task ClearAsync()
    {
        Token = null;
        CurrentUser = null;
        _api.Token = null;
        try { SecureStorage.Default.Remove(TokenKey); } catch { }
        RaiseStateChanged();
        await Task.CompletedTask;
    }

    private void RaiseStateChanged() => AuthStateChanged?.Invoke(this, EventArgs.Empty);
}
