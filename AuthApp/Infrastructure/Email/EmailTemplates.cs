using System;

namespace AuthApp.Infrastructure.Email;

public static class EmailTemplates
{
    private static string Layout(string title, string body, string actionText, string link) =>
        $"""
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>{title}</title>
</head>
<body style="
    margin:0;
    padding:0;
    background-color:#0f172a;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif;
    color:#e5e7eb;
">
<table width="100%" cellpadding="0" cellspacing="0">
<tr>
<td align="center" style="padding:40px 16px;">
    <table width="100%" cellpadding="0" cellspacing="0" style="
        max-width:520px;
        background-color:#020617;
        border-radius:14px;
        padding:32px;
        box-shadow:0 10px 30px rgba(0,0,0,0.4);
    ">
        <tr>
            <td>
                <h1 style="
                    margin:0 0 12px 0;
                    font-size:22px;
                    font-weight:600;
                    color:#f8fafc;
                ">
                    {title}
                </h1>

                <p style="
                    margin:0 0 24px 0;
                    font-size:15px;
                    line-height:1.6;
                    color:#cbd5f5;
                ">
                    {body}
                </p>

                <a href="{link}" style="
                    display:inline-block;
                    padding:14px 22px;
                    background-color:#6366f1;
                    color:#ffffff;
                    text-decoration:none;
                    font-weight:600;
                    border-radius:10px;
                    font-size:15px;
                ">
                    {actionText}
                </a>

                <p style="
                    margin-top:28px;
                    font-size:13px;
                    color:#94a3b8;
                ">
                    If the button doesn't work, copy and paste this link into your browser:
                </p>

                <p style="
                    word-break:break-all;
                    font-size:12px;
                    color:#64748b;
                ">
                    {link}
                </p>

                <hr style="
                    border:none;
                    border-top:1px solid #1e293b;
                    margin:32px 0;
                " />

                <p style="
                    margin:0;
                    font-size:12px;
                    color:#64748b;
                ">
                    If you didn’t request this, you can safely ignore this email.
                </p>
            </td>
        </tr>
    </table>

    <p style="
        margin-top:16px;
        font-size:12px;
        color:#475569;
    ">
        © {DateTime.UtcNow.Year} AuthApp. All rights reserved.
    </p>
</td>
</tr>
</table>
</body>
</html>
""";

    public static string VerifyEmail(string link) =>
        Layout(
            "Verify your email",
            "Thanks for signing up. Please confirm your email address to activate your account.",
            "Verify Email",
            link
        );

    public static string ResetPassword(string link) =>
        Layout(
            "Reset your password",
            "We received a request to reset your password. Click the button below to continue.",
            "Reset Password",
            link
        );
}
