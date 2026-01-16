using System;

namespace AuthApp.Common.Auth;

public sealed record OAuthIdentity(string ProviderUserId, string Email);
