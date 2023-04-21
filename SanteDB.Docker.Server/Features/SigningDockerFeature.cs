using SanteDB.Core.Configuration;
using SanteDB.Core.Security.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using SanteDB;
using SanteDB.Core.Security;
using SanteDB.Security.Certs.BouncyCastle;
using System.Security.Cryptography.X509Certificates;

namespace SanteDB.Docker.Core.Features
{
    /// <summary>
    /// Docker feature that adds additional security configuration
    /// </summary>
    public class SigningDockerFeature : IDockerFeature
    {
        /// <summary>
        /// Use RS256 Secrets
        /// </summary>
        public const string SIGN_ALG_SETTING = "ALG";
        /// <summary>
        /// Generate certificate for me
        /// </summary>
        public const string GEN_RSA_CERT_SETTING = "RS256_GEN";
        /// <summary>
        /// Certificate setting
        /// </summary>
        public const string CERT_SETTING = "R256_CERT";
        /// <summary>
        /// HMAC secret
        /// </summary>
        public const string HS256_SECRET_SETTING = "HS256_SECRET";

        /// <summary>
        /// Keys to change
        /// </summary>
        private readonly string[] KEYS_TO_CHANGE =
        {
            "default", "jwsdefault"
        };

        /// <inheritdoc/>
        public string Id => "SIGN";

        /// <inheritdoc/>
        public IEnumerable<string> Settings => new String[]
        {
            SIGN_ALG_SETTING,
            GEN_RSA_CERT_SETTING,
            CERT_SETTING,
            HS256_SECRET_SETTING
        };


        /// <inheritdoc/>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {
            var securitySection = configuration.GetSection<SecurityConfigurationSection>();

            // First - is there a signing setting?
            if (settings.TryGetValue(SIGN_ALG_SETTING, out var signAlg))
            {
                switch (signAlg.ToLowerInvariant())
                {
                    case "HS256":
                        if (!settings.TryGetValue(HS256_SECRET_SETTING, out var secret))
                        {
                            throw new InvalidOperationException($"HS256 requires {HS256_SECRET_SETTING} setting");
                        }
                        Array.ForEach(this.KEYS_TO_CHANGE, keyName =>
                        {
                            var key = this.GetOrCreateSignatureKey(securitySection, keyName);
                            key.Algorithm = SignatureAlgorithm.HS256;
                            key.HmacSecret = secret;
                        });
                        break;
                    case "RS256":
                        if (!settings.TryGetValue(GEN_RSA_CERT_SETTING, out var genRsa) & !settings.TryGetValue(CERT_SETTING, out var certFind))
                        {
                            throw new InvalidOperationException($"RS256 requires either {GEN_RSA_CERT_SETTING} or {CERT_SETTING} setting");
                        }
                        else if(!String.IsNullOrEmpty(genRsa))
                        {
                            var bcGenerator = new BouncyCastleCertificateGenerator();
                            var keyPair = bcGenerator.CreateKeyPair(2048);
                            var keySubject = $"CN={genRsa}, OID.2.5.6.11=SanteDB.Docker.Program";
                            var certificate = X509CertificateUtils.FindCertificate(X509FindType.FindBySubjectName, StoreLocation.CurrentUser, StoreName.My, keySubject);
                            if (certificate == null) {
                                certificate = bcGenerator.CreateSelfSignedCertificate(keyPair, new X500DistinguishedName(keySubject), new TimeSpan(365, 0, 0, 0), X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyAgreement);
                                X509CertificateUtils.InstallCertificate(StoreName.My, certificate);
                            }
                            certFind = certificate.Thumbprint;
                        }

                        Array.ForEach(this.KEYS_TO_CHANGE, keyName =>
                        {
                            var key = this.GetOrCreateSignatureKey(securitySection, keyName);
                            key.FindValue = certFind;
                            key.FindType = System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint;
                            key.StoreName = System.Security.Cryptography.X509Certificates.StoreName.My;
                            key.StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser;
                            key.Algorithm = SignatureAlgorithm.RS256;
                        });
                        break;
                }

            }
        }

        private SecuritySignatureConfiguration GetOrCreateSignatureKey(SecurityConfigurationSection securitySection, string keyName)
        {
            var key = securitySection.Signatures.Find(o => o.KeyName == keyName);
            if (key == null)
            {
                key = new SecuritySignatureConfiguration();
                securitySection.Signatures.Add(key);
            }
            return key;
        }
    }
}