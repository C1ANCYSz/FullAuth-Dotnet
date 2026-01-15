using System;

namespace AuthApp.Infrastructure.Email;

public static class EmailTemplates
{
    public static string VerifyEmail(string link) =>
        $"""
            <h2>Verify your email</h2>
            <p>Click the link below:</p>
            <a href="{link}">Verify Email</a>
            """;

    public static string ResetPassword(string link) =>
        $"""
            <h2>Reset password</h2>
            <p>Click below to reset:</p>
            <a href="{link}">Reset Password</a>
            """;
}
