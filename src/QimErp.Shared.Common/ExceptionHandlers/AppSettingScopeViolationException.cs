namespace QimErp.Shared.Common.ExceptionHandlers;

/// <remarks>Deliberately not a <see cref="DomainException"/> — validation helpers swallow those.</remarks>
public sealed class AppSettingScopeViolationException(string message) : Exception(message);
