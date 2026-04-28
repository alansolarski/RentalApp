namespace RentalApp.Database.Services;

public class TokenStore
{
    public string? Token { get; private set; }
    public int UserId { get; private set; }

    public void SetToken(string? token, int userId = 0)
    {
        Token = token;
        UserId = userId;
    }
}