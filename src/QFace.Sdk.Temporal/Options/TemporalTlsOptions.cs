namespace QFace.Sdk.Temporal.Options;

/// <summary>
/// Advanced TLS options for Temporal connections.
/// Only needed when EnableTls is true and the default TLS settings
/// (system CA, SNI from Address host) are not sufficient.
/// Typical for Railway/cloud deployments with custom CA certificates.
/// </summary>
public sealed class TemporalTlsOptions
{
    /// <summary>
    /// PEM-encoded server root CA certificate.
    /// Set when the Temporal server uses a certificate not in the system trust store.
    /// </summary>
    public string? ServerRootCaCert { get; set; }

    /// <summary>
    /// SNI domain override for TLS handshake.
    /// Defaults to the host portion of TemporalOptions.Address when null.
    /// Set when the TLS certificate CN/SAN differs from the connection hostname.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// PEM-encoded client certificate for mutual TLS (mTLS).
    /// Optional — only required when the Temporal server demands client auth.
    /// </summary>
    public string? ClientCert { get; set; }

    /// <summary>
    /// PEM-encoded client private key for mutual TLS (mTLS).
    /// Must be provided alongside ClientCert.
    /// </summary>
    public string? ClientPrivateKey { get; set; }
}
