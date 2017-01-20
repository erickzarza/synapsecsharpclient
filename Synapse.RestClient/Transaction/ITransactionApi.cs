using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.RestClient.Transaction
{
    public interface ISynapseTransactionApiClient
    {
        Task<AddTransactionResponse> AddTransactionAsync(AddTransactionRequest msg);

        Task<CancelTransactionResponse> CancelTransactionAsync(CancelTransactionRequest msg);
        Task<TransactionsResponse> FindTransactionsAsync(TransactionsRequest msg, int page);

        event RequestEventHandler OnAfterRequest;
    }

    public class SynapseTransactionApiClient : ISynapseTransactionApiClient
    {
        public event RequestEventHandler OnAfterRequest = delegate { };
        
        SynapseApiCredentials _creds;
        IRestClient _api;
        public SynapseTransactionApiClient(SynapseApiCredentials creds, string baseUrl) : this(creds, new RestSharp.RestClient(new Uri(baseUrl, UriKind.Absolute)))
        {

        }

        public SynapseTransactionApiClient(SynapseApiCredentials creds, IRestClient client)
        {

            this._creds = creds;
            this._api = client;
        }

        public async Task<AddTransactionResponse> AddTransactionAsync(AddTransactionRequest msg)
        {
            var req = new RestRequest("trans/add", Method.POST);
            dynamic body = new
            {
                login = new {
                    oauth_key = msg.OAuth.Key,
                },
                user = new {
                    fingerprint = msg.Fingerprint
                },
                trans = new
                {
                    from = new
                    {
                        type = Translate(msg.FromNodeType),
                        id = msg.FromNodeId
                    },
                    to = new
                    {
                        type = Translate(msg.ToNodeType),
                        id = msg.ToNodeId
                    },
                    amount = new
                    {
                        amount = msg.Amount,
                        currency = msg.Currency.ToString(),
                    },
                    extra = new
                    {
                        supp_id = msg.LocalId,
                        note = msg.Note,
                        ip = msg.IpAddress,
                        process_on = msg.ProcessOn
                    }
                }
                
            };
            if(msg.Fee > 0)
            {
                body.fees = new[] {new {fee = msg.Fee, note = msg.Note, to = new {id = msg.FeeNodeId}}};
            }
            req.AddJsonBody(body);

            var resp = await this._api.ExecuteTaskAsync(req);
            RaiseOnAfterRequest(body, req, resp);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);

            if(resp.IsHttpOk() && data.success)
            {
                var oauth = data.oauth;
                var trans = data.trans;
                var status = trans.recent_status;
                return new AddTransactionResponse
                {
                    Success = true,
                    Message = ApiHelper.TryGetMessage(data),
                    TransactionOId = trans._id["$oid"],
                    Status =
                        new SynapseTransactionStatus()
                        {
                            OnUtc = ApiHelper.UnixTimestampInMillisecondsToUtc(Convert.ToInt64(status.date["$date"])),
                            Note = status.note,
                            StatusDescription = status.status,
                            Status = Enum.Parse(typeof(SynapseTransactionStatusCode), Convert.ToInt32(status.status_id).ToString())
                        }
                };
            }
            else
            {
                return new AddTransactionResponse() {Success = false, Message = ApiHelper.TryGetError(data)};
            }
        }

        public async Task<TransactionsResponse> FindTransactionsAsync(TransactionsRequest msg, int page)
        {
            var req = new RestRequest($"trans/show", Method.POST);
            dynamic body = new
            {
                login = new
                {
                    oauth_key = msg.OAuth.Key,
                },
                user = new
                {
                    fingerprint = msg.Fingerprint
                },
                filter = new
                {
                    page = page
                }
            };
            req.AddJsonBody(body);
            var resp = await this._api.ExecuteTaskAsync(req);
            RaiseOnAfterRequest(body, req, resp);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);
            var list = new List<Transaction>();
            if(resp.IsHttpOk() && data.success)
            {
                foreach(var trans in data.trans)
                {
                    var t = new Transaction
                    {
                        OId = trans._id["$oid"],
                        Notes = trans.extra.note,
                        SuppId = trans.extra.supp_id,
                        FromUserOId = trans.from.user._id["$oid"],
                        ToUserOId = trans.to.user._id["$oid"],
                        AmountUsd = Convert.ToDecimal(trans.amount["amount"]),
                        IsDebit = Convert.ToString(trans.from.id["$oid"]) == msg.NodeOId,
                        CreatedOnUtc = ApiHelper.UnixTimestampInMillisecondsToUtc(Convert.ToInt64(trans.extra.created_on["$date"])),
                        LastStatus = Enum.Parse(typeof(SynapseTransactionStatusCode), Convert.ToInt32(trans.recent_status.status_id).ToString()),
                        History = new List<SynapseTransactionStatus>()
                    };

                    list.Add(t);

                    foreach(var status in trans.timeline)
                    {
                        t.History.Add(new SynapseTransactionStatus()
                        {
                            OnUtc = ApiHelper.UnixTimestampInMillisecondsToUtc(Convert.ToInt64(status.date["$date"])),
                            Note = status.note,
                            StatusDescription = status.status,
                            Status = Enum.Parse(typeof(SynapseTransactionStatusCode), Convert.ToInt32(status.status_id).ToString())
                        });
                    }
                }
                return new RestClient.Transaction.TransactionsResponse
                {
                    IsSuccess = true,
                    Transactions = list
                };
            }
            return new RestClient.Transaction.TransactionsResponse
            {
                IsSuccess = false,
                Message = ApiHelper.TryGetError(data)
            };
        }
        

        public async Task<CancelTransactionResponse> CancelTransactionAsync(CancelTransactionRequest msg)
        {
            var req = new RestRequest("trans/cancel", Method.POST);
            var _id = new Dictionary<string, string>()
            {
                { "$oid", msg.TransactionOId }
            };  
            dynamic body = new
            {
                login = new {
                    oauth_key = msg.OAuth.Key,
                },
                user = new {
                    fingerprint = msg.Fingerprint
                },
                trans = new
                {
                    _id = _id,
                }
                
            };
            req.AddJsonBody(body);

            var resp = await this._api.ExecuteTaskAsync(req);
            RaiseOnAfterRequest(body, req, resp);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);

            if(resp.IsHttpOk() && data.success)
            {
                var oauth = data.oauth;
                var trans = data.trans;
                var status = trans.recent_status;
                return new CancelTransactionResponse
                {
                    Success = true,
                    Message = ApiHelper.TryGetMessage(data),
                };
            }
            else
            {
                return new CancelTransactionResponse { Success = false, Message = ApiHelper.TryGetError(data) };
            }
        }

        private string Translate(SynapseNodeTransactionType nodeType)
        {
            if(nodeType == SynapseNodeTransactionType.ACHUS)
                return "ACH-US";
            else if(nodeType == SynapseNodeTransactionType.SYNAPSEUS)
                return "SYNAPSE-US";
            throw new InvalidOperationException(String.Format("SynapseNodeTransactionType {0} unknown", nodeType));
        }
        private void RaiseOnAfterRequest(object body, IRestRequest req, IRestResponse resp)
        {
            OnAfterRequest(resp.ResponseUri.ToString(), resp.StatusCode, SimpleJson.SerializeObject(body), resp.Content);
        }
    }
}
