namespace QimErp.Shared.Common.ExceptionHandlers;

/// <summary>
/// A tracked row was updated or deleted while its company lies outside the caller's company scope.
/// </summary>
/// <remarks>Deliberately not a <see cref="DomainException"/> — validation helpers swallow those.</remarks>
public sealed class CrossCompanyWriteException(string message) : Exception(message);
