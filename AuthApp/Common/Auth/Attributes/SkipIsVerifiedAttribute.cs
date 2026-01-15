namespace AuthApp.Common.Auth.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipIsVerifiedAttribute : Attribute { }
