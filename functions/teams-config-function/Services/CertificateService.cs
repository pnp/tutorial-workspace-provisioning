using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TeamsConfiguration.Services
{
    public class CertificateService
    {
        private const string certificateThumbprintKey = "AUTH_CLIENT_SECRET_CERTIFICATE_THUMBPRINT";
        public CertificateService(ConfigurationService configurationService)
        {
            if (configurationService == null)
                throw new ArgumentNullException(nameof(configurationService));
            AppCertificate = new Lazy<X509Certificate2>(() =>
            {
                var configuration = configurationService.Configuration.Value;
                var storeName = StoreName.My;
                var storeLocation = StoreLocation.CurrentUser;

                var certStore = new X509Store(storeName, storeLocation);
                certStore.Open(OpenFlags.ReadOnly);
                var thumbPrint = configuration[certificateThumbprintKey];

                if (string.IsNullOrEmpty(thumbPrint))
                    return null;
                else
                {
                    var certCollection = certStore.Certificates.Find(
                                X509FindType.FindByThumbprint,
                                thumbPrint,
                                false);


                    var certificate = certCollection.Count > 0 ? certCollection[0] : null;
                    certStore.Close();
                    return certificate;
                }
            });
        }
        public Lazy<X509Certificate2> AppCertificate { get; private set; }
    }
}
