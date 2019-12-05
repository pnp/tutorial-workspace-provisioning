using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamsConfiguration.Services
{
    public class GraphServiceClientFactory
    {
        private const string clientIdKey = "WEBSITE_AUTH_CLIENT_ID";
        private const string issuerIdKey = "WEBSITE_AUTH_OPENID_ISSUER";
        private GraphServiceClientFactory(ConfigurationService configurationService, CertificateService certificateService)
        {
            if (configurationService == null)
                throw new ArgumentNullException(nameof(configurationService));
            if (certificateService == null)
                throw new ArgumentNullException(nameof(certificateService));
            var configuration = configurationService.Configuration.Value;
            Client = new Lazy<GraphServiceClient>(() =>
            {
                var confidentialClientApplication = ConfidentialClientApplicationBuilder
                                                                            .Create(configuration[clientIdKey])
                                                                            .WithTenantId(GetTenantIdFromIssuer(configuration[issuerIdKey]))
                                                                            .WithCertificate(certificateService.AppCertificate.Value)
                                                                            .Build();
                var authProvider = new ClientCredentialProvider(confidentialClientApplication);
                var client = new GraphServiceClient(authProvider);
                return client;
            });
        }
        public Lazy<GraphServiceClient> Client { get; private set; }
        private string GetTenantIdFromIssuer(string issuer) => issuer?.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)?.LastOrDefault();

        private static GraphServiceClientFactory _instance;
        public static GraphServiceClientFactory GetInstance(string appDirectory)
        {
            if (_instance == null)
            {
                var configurationService = ConfigurationService.GetInstance(appDirectory);
                _instance = new GraphServiceClientFactory(configurationService, new CertificateService(configurationService));
            }
            return _instance;
        }
    }
}
