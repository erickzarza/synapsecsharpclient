using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.RestClient.Node
{
    public interface ISynapseNodeApiClient
    {
        Task<AddACHNodeResponse> AddACHNodeAsync(AddACHNodeRequest req);
        Task<VerifyACHNodeResponse> VerifyACHNodeAsync(VerifyACHNodeRequest req);
        Task<ACHNodeResponse> FindNodeAsync(ACHNodeRequest req);
        event RequestEventHandler OnAfterRequest;
    }

    public class SynapseNodeApiClient : ISynapseNodeApiClient
    {
        SynapseApiCredentials _creds;
        IRestClient _api;
        public event RequestEventHandler OnAfterRequest = delegate { };

        public SynapseNodeApiClient(SynapseApiCredentials creds, string baseUrl) : this(creds, new RestSharp.RestClient(new Uri(baseUrl, UriKind.Absolute)))
        {

        }

        public SynapseNodeApiClient(SynapseApiCredentials creds, IRestClient client)
        {

            this._creds = creds;
            this._api = client;
        }

        public async Task<ACHNodeResponse> FindNodeAsync(ACHNodeRequest msg)
        {
            var req = new RestRequest($"node/show", Method.POST);
            var body = new
            {
                login = new
                {
                    oauth_key = msg.OAuth.Key
                },
                user = new
                {
                    fingerprint = msg.Fingerprint
                }
            };

            req.AddJsonBody(body);
            var resp = await this._api.ExecuteTaskAsync(req);
            RaiseOnAfterRequest(body, req, resp);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);

            if (resp.IsHttpOk() && data.success)
            {
                foreach (var node in data.nodes)
                {
                    if (Convert.ToString(node._id["$oid"]) == msg.SynapseNodeId)
                    {
                        return new ACHNodeResponse
                        {
                            Success = true,
                            IsActive = node.is_active,
                            Permission = ParseNodePermission(node.allowed)
                        };
                    }
                }
            }
            return new ACHNodeResponse
            {
                Success = false,
                Message = ApiHelper.TryGetMessage(data)
            };

        }
        public async Task<AddACHNodeResponse> AddACHNodeAsync(AddACHNodeRequest msg)
        {
            var req = new RestRequest("node/add", Method.POST);
            var body = new
            {
                login = new
                {
                    oauth_key = msg.OAuth.Key
                },
                user = new
                {
                    fingerprint = msg.Fingerprint
                },
                node = new
                {
                    type = "ACH-US",
                    info = new
                    {
                        nickname = msg.Nickname,
                        name_on_account = msg.NameOnAccount,
                        account_num = msg.AccountNumber,
                        routing_num = msg.RoutingNumber,
                        type = msg.AccountType.ToString().ToUpper(),
                        @class = msg.AccountClass.ToString().ToUpper()
                    },
                    extra = new
                    {
                        supp_id = msg.LocalId
                    }
                }
            };
            req.AddJsonBody(body);
            var resp = await this._api.ExecuteTaskAsync(req);
            RaiseOnAfterRequest(body, req, resp);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);
            if (resp.IsHttpOk() && data.success)
            {
                var node = data.nodes[0];
                var info = node.info;
                return new AddACHNodeResponse
                {
                    Success = true,
                    IsActive = node.is_active,
                    Permission = ParseNodePermission(node.allowed),
                    Message = ApiHelper.TryGetMessage(data),
                    SynapseNodeOId = node._id["$oid"]
                };
            }
            else
            {
                return new AddACHNodeResponse
                {
                    Success = false,
                    Message = ApiHelper.TryGetError(data)
                };
            }
        }
        

        public async Task<VerifyACHNodeResponse> VerifyACHNodeAsync(VerifyACHNodeRequest msg)
        {
            var req = new RestRequest("node/verify", Method.POST);
            dynamic id = new Dictionary<string, object>() { { "$oid", msg.SynapseNodeOId } };

            dynamic body = new
            {
                login = new
                {
                    oauth_key = msg.OAuth.Key
                },
                user = new
                {
                    fingerprint = msg.Fingerprint
                },
                node = new
                {
                    _id = id,                    
                    verify = new 
                    {
                        micro = msg.Deposits
                    }
                }
            };
            req.AddJsonBody(body);
            var resp = await this._api.ExecuteTaskAsync(req);
            RaiseOnAfterRequest(body, req, resp);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);
            if (resp.IsHttpOk())
            {
                if (data == null) throw new InvalidOperationException("Expected the response to contain json data");
                if (data.node == null) throw new InvalidOperationException("Expected a node field in response");
                var node = data.node;
                return new VerifyACHNodeResponse
                {
                    Success = true,
                    IsActive = node.is_active,
                    Permission = ParseNodePermission(node.allowed),
                    Message = ApiHelper.TryGetMessage(data),
                    SynapseNodeOId = node._id["$oid"]
                };
            }
            else
            {
                return new VerifyACHNodeResponse
                {
                    Success = false,
                    Message = ApiHelper.TryGetError(data)
                };
            }
        }

        private void RaiseOnAfterRequest(object body, IRestRequest req, IRestResponse resp)
        {
            OnAfterRequest(resp.ResponseUri.ToString(), resp.StatusCode, SimpleJson.SerializeObject(body), resp.Content);
        }
        private static SynapseNodePermission ParseNodePermission(string allowed)
        {
            if (allowed == "CREDIT") return SynapseNodePermission.Credit;
            else if (allowed == "CREDIT-AND-DEBIT") return SynapseNodePermission.CreditAndDebit;
            else if (allowed == "LOCKED") return SynapseNodePermission.Locked;
            else throw new ArgumentOutOfRangeException(String.Format("Unknown node permission value {0}", allowed));
        }
    }
}
