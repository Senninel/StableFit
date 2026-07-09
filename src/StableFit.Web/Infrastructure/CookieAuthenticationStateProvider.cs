using Microsoft.AspNetCore.Components.Authorization;
using StableFit.Web.Models;
using System.Net.Http.Json;
using System.Security.Claims;

namespace StableFit.Web.Infrastructure;

public class CookieAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _http;
    private bool _initialized;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public CookieAuthenticationStateProvider(IHttpClientFactory factory)
        => _http = factory.CreateClient("API");

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_initialized)
        {
            try
            {
                var info = await _http.GetFromJsonAsync<UserInfo>("api/auth/me");
                if (info is not null)
                    SetUser(info.Username ?? info.Email ?? "User");
            }
            catch { /* 401 or network error → stays anonymous */ }
            _initialized = true;
        }
        return new AuthenticationState(_currentUser);
    }

    public void MarkLoggedIn(string username)
    {
        SetUser(username);
        _initialized = true;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public void MarkLoggedOut()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        _initialized = true;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    private void SetUser(string username)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, username)], "cookie");
        _currentUser = new ClaimsPrincipal(identity);
    }
}
