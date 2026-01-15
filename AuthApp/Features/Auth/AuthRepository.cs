using System.Security.Cryptography;
using System.Text;
using StackExchange.Redis;

namespace AuthApp.Features.Auth;

public class AuthRepository(IConnectionMultiplexer redis)
{
    private readonly IDatabase _redis = redis.GetDatabase();

    private static string ResetPasswordKey(string hashedToken) => $"auth:forgot_pwd:{hashedToken}";

    private static string ResetPasswordEmailCooldownKey(string email)
    {
        var emailHash = HashEmail(email);
        return $"auth:forgot_pwd:cooldown:email:{emailHash}";
    }

    public async Task<bool> TrySetEmailCooldown(string email)
    {
        var key = ResetPasswordEmailCooldownKey(email);

        //SET NX
        return await _redis.StringSetAsync(
            key,
            "1",
            TimeSpan.FromMinutes(15),
            when: When.NotExists
        );
    }

    private static string HashEmail(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToBase64String(bytes);
    }

    public async Task StoreResetToken(Guid userId, string rawToken)
    {
        var hashedToken = HashResetToken(rawToken);

        await _redis.StringSetAsync(
            ResetPasswordKey(hashedToken),
            userId.ToString(),
            TimeSpan.FromMinutes(15)
        );
    }

    public string GenerateResetToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    public string HashResetToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }

    public async Task DeleteResetToken(string hashedToken)
    {
        await _redis.KeyDeleteAsync(ResetPasswordKey(hashedToken));
    }

    public async Task<Guid?> GetUserIdByResetToken(string rawToken)
    {
        var hashedToken = HashResetToken(rawToken);

        var value = await _redis.StringGetAsync(ResetPasswordKey(hashedToken));

        if (value.IsNullOrEmpty)
            return null;

        if (!Guid.TryParse(value.ToString(), out var userId))
            return null;

        return userId;
    }

    public string GenerateEmailVerificationCode()
    {
        return RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
    }
}
