using System.Security.Cryptography;
using System.Text;
using AuthApp.Common.RateLimit;
using StackExchange.Redis;

namespace AuthApp.Features.Auth;

public class AuthRepository(IConnectionMultiplexer redis)
{
    private readonly IDatabase _db = redis.GetDatabase();

    private const int ResetTokenTtlMinutes = 15;
    private const int VerifyCodeTtlMinutes = 10;
    private const int CooldownTtlMinutes = 3;
    private const int VerifyMaxAttempts = 5;

    private static string ResetTokenKey(string hash) => $"auth:reset:{hash}";

    private static string VerifyCodeKey(Guid userId) => $"auth:verify:{userId}";

    private static string VerifyAttemptsKey(Guid userId) => $"auth:verify:attempts:{userId}";

    private static string CooldownKey(string emailHash) => $"auth:cooldown:{emailHash}";

    public async Task StoreResetToken(Guid userId, string rawToken)
    {
        var hash = Hash(rawToken);

        await _db.StringSetAsync(
            ResetTokenKey(hash),
            userId.ToString(),
            TimeSpan.FromMinutes(ResetTokenTtlMinutes)
        );
    }

    public async Task<Guid?> ConsumeResetToken(string rawToken)
    {
        var hash = Hash(rawToken);
        var key = ResetTokenKey(hash);

        var result = await _db.ScriptEvaluateAsync(RedisScripts.ConsumeValue, [key]);

        if (result.IsNull)
            return null;

        var value = (string?)result;
        if (value is null)
            return null;

        return Guid.Parse(value);
    }

    public async Task<string> CreateEmailVerificationCode(Guid userId)
    {
        var code = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

        await _db.StringSetAsync(
            VerifyCodeKey(userId),
            Hash(code),
            TimeSpan.FromMinutes(VerifyCodeTtlMinutes)
        );

        return code;
    }

    public async Task<bool> VerifyEmailCode(Guid userId, string code)
    {
        var attemptsKey = VerifyAttemptsKey(userId);
        var codeKey = VerifyCodeKey(userId);
        var hashedCode = Hash(code);

        var attempts = (int)
            await _db.ScriptEvaluateAsync(
                RedisScripts.IncrementWithLimit,
                [attemptsKey],
                [VerifyMaxAttempts, TimeSpan.FromMinutes(VerifyCodeTtlMinutes).TotalSeconds]
            );

        if (attempts == -1)
            return false;

        var verified = (int)
            await _db.ScriptEvaluateAsync(RedisScripts.ConsumeIfEquals, [codeKey], [hashedCode]);

        if (verified == 1)
        {
            await _db.KeyDeleteAsync(attemptsKey);
            return true;
        }

        return false;
    }

    public async Task<bool> TrySetEmailCooldown(string email)
    {
        var key = CooldownKey(Hash(Normalize(email)));

        return await _db.StringSetAsync(
            key,
            "1",
            TimeSpan.FromMinutes(CooldownTtlMinutes),
            When.NotExists
        );
    }

    public string GenerateSecureToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static string Hash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash);
    }
}
