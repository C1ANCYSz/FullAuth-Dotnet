using System;

namespace AuthApp.Common.Utils;

public static class PasswordUtil
{
    public static string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty");

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public static bool IsHashed(string value)
    {

        return value.StartsWith("$2");
    }

    public static bool PasswordsAreEqual(string password1, string password2)
    {
        return password1 == password2;
    }
}
