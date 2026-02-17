namespace Admin.Services;

public class AuthTokenStore
{
    private const string TokenKey = "auth_token";
    private const string UserIdKey = "auth_user_id";
    private const string UserNameKey = "auth_user_name";
    private const string UserEmailKey = "auth_user_email";

    public Task SaveTokenAsync(string token)
    {
        Preferences.Default.Set(TokenKey, token);
        return Task.CompletedTask;
    }

    public Task<string?> GetTokenAsync()
    {
        var value = Preferences.Default.Get<string?>(TokenKey, null);
        return Task.FromResult(value);
    }

    public Task SaveUserInfoAsync(int id, string name, string email)
    {
        Preferences.Default.Set(UserIdKey, id.ToString());
        Preferences.Default.Set(UserNameKey, name);
        Preferences.Default.Set(UserEmailKey, email);
        return Task.CompletedTask;
    }

    public Task<(int Id, string Name, string Email)?> GetUserInfoAsync()
    {
        var idStr = Preferences.Default.Get<string?>(UserIdKey, null);
        var name = Preferences.Default.Get<string?>(UserNameKey, null);
        var email = Preferences.Default.Get<string?>(UserEmailKey, null);

        if (idStr is not null && name is not null && email is not null && int.TryParse(idStr, out var id))
            return Task.FromResult<(int, string, string)?>(( id, name, email ));

        return Task.FromResult<(int, string, string)?>(null);
    }

    public void Clear()
    {
        Preferences.Default.Remove(TokenKey);
        Preferences.Default.Remove(UserIdKey);
        Preferences.Default.Remove(UserNameKey);
        Preferences.Default.Remove(UserEmailKey);
    }

    public Task<bool> HasTokenAsync()
    {
        var token = Preferences.Default.Get<string?>(TokenKey, null);
        return Task.FromResult(!string.IsNullOrEmpty(token));
    }
}
