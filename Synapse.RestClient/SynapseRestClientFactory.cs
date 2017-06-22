using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Synapse.RestClient.Transaction;

namespace Synapse.RestClient
{
    using User;
    using Node;
    using System.Net;

    public class SynapseRestClientFactory
    {
        private SynapseApiCredentials _creds;
        private string _baseUrl;
        public SynapseRestClientFactory(SynapseApiCredentials credentials, string baseUrl)
        {
            this._creds = credentials;
            this._baseUrl = baseUrl;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        }

        public ISynapseUserApiClient CreateUserClient()
        {
            return new SynapseUserApiClient(this._creds, this._baseUrl);
        }

        public ISynapseNodeApiClient CreateNodeClient()
        {
            return new SynapseNodeApiClient(this._creds, this._baseUrl);
        }

        public ISynapseTransactionApiClient CreateTransactionClient()
        {
            return new SynapseTransactionApiClient(this._creds, this._baseUrl);
        }
    }
}
