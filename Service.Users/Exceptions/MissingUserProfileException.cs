namespace Service.Users.Exceptions;

/// <summary>Indicates that an authenticated user has no local profile required for an owned subscription.</summary>
public sealed class MissingUserProfileException() : Exception("Create a user profile before managing an owned calendar subscription.");
