using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Dynamic;

namespace Synapse.RestClient.User
{

    public interface ISynapseUserApiClient
    {
        Task<CreateUserResponse> CreateUserAsync(CreateUserRequest req);
        Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest req);

        Task<UpdateUserResponse> UpdateUser(UpdateUserRequest req);

        Task<AddKycResponse> AddKycAsync(AddKycRequest req);
        Task<AddDocResponse> AddDocAsync(AddDocRequest req);
        Task<VerifyKYCInfoResponse> VerifyKYCInfo(VerifyKYCInfoRequest req);

        Task<ShowUsersResponse> ShowUsersAsync(ShowUsersRequest msg);

        event RequestEventHandler OnAfterRequest;
    }

    public class SynapseUserApiClient : ISynapseUserApiClient
    {
        SynapseApiCredentials _creds;
        IRestClient _api;
        public event RequestEventHandler OnAfterRequest = delegate { };
        public SynapseUserApiClient(SynapseApiCredentials creds, string baseUrl) : this(creds, new RestSharp.RestClient(new Uri(baseUrl, UriKind.Absolute)))
        {

        }

        public SynapseUserApiClient(SynapseApiCredentials creds, IRestClient client)
        {

            this._creds = creds;
            this._api = client;
        }


        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest msg)
        {
            var req = new RestRequest("user/create", Method.POST);
            var body = new
            {
                client = new
                {
                    client_id = this._creds.ClientId,
                    client_secret = this._creds.ClientSecret
                },
                logins = new[] {
                    new {
                            email = msg.EmailAddress,
                            read_only = false
                        }
                },
                legal_names = new[]
                {
                    String.Format("{0} {1}", msg.FirstName, msg.LastName)
                },
                phone_numbers = new[] { msg.PhoneNumber },
                fingerprints = new[] {
                    new {
                        fingerprint =  msg.Fingerprint
                    }
                },
                ips = new[] { msg.IpAddress },
                extra = new
                {
                    supp_id = msg.LocalId,
                    is_business = false

                }
            };
            req.AddJsonBody(body);

            var resp = await this._api.ExecuteTaskAsync(req);
            RaiseOnAfterRequest(body, req, resp);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);

            if (resp.IsHttpOk() && data.success)
            {
                var oauth = data.oauth;
                return new CreateUserResponse
                {
                    Success = true,
                    Message = ApiHelper.TryGetMessage(data),
                    SynapseOId = data.user._id["$oid"],
                    SynapseClientId = data.user.client.id.ToString(),
                    OAuth = new SynapseUserOAuth
                    {
                        Key = oauth.oauth_key,
                        RefreshToken = oauth.refresh_token,
                        ExpirationUtc = DateTime.UtcNow.AddSeconds(Convert.ToInt64(oauth.expires_in))
                    },
                    Permission = ParsePermission(data.user.permission)
                };
            }
            else
            {
                return new CreateUserResponse
                {
                    Message = ApiHelper.TryGetError(data),
                    Success = false
                };
            };

        }

        public async Task<AddKycResponse> AddKycAsync(AddKycRequest msg)
        {
            var req = new RestRequest("user/doc/add", Method.POST);
            var body = new
            {
                login = new
                {
                    oauth_key = msg.OAuth.Key
                },
                user = new
                {
                    doc = new
                    {
                        birth_day = msg.DateOfBirth.Day,
                        birth_month = msg.DateOfBirth.Month,
                        birth_year = msg.DateOfBirth.Year,
                        name_first = msg.FirstName,
                        name_last = msg.LastName,
                        address_street1 = msg.Address1,
                        address_postal_code = msg.PostalCode,
                        address_country_code = msg.CountryCode,
                        document_value = msg.DocumentValue,
                        document_type = ToString(msg.DocumentType)
                    },
                    fingerprint = msg.Fingerprint
                }
            };
            req.AddJsonBody(body);

            var resp = await this._api.ExecuteTaskAsync(req);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);
            RaiseOnAfterRequest(body, req, resp);
            if (resp.IsHttpOk() && data.success)
            {
                return new AddKycResponse
                {
                    Success = true,
                    Message = ApiHelper.TryGetMessage(data),
                    Permission = ParsePermission(data.user.permission),
                    HasKBAQuestions = false
                };
            }
            else
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.Accepted && data.success)
                {
                    if (data.error_code == "10" && ApiHelper.PropertyExists(data, "question_set")) //Magic number for "need KBA"
                    {
                        var questions = new List<Question>();

                        foreach (dynamic q in data.question_set.questions)
                        {
                            var answers = new List<Answer>();
                            var question = new Question
                            {
                                Id = Convert.ToInt32(q.id),
                                Text = q.question
                            };

                            foreach (dynamic a in q.answers)
                            {
                                answers.Add(new Answer
                                {
                                    Id = Convert.ToInt32(a.id),
                                    Text = a.answer
                                });
                            }

                            question.Answers = answers.OrderBy(c => c.Id).ToArray();
                            questions.Add(question);
                        }
                        var response = new AddKycResponse
                        {
                            HasKBAQuestions = true,
                            KBAQuestionSet = new QuestionSet
                            {
                                Id = data.question_set.id,
                                Questions = questions.ToArray(),
                                CreatedAtUtc = DateTime.UtcNow //TODO: there is a timestamp from synapse, might be important                               
                            },
                            Success = true,
                            Message = ApiHelper.TryGetMessage(data),
                            Permission = SynapsePermission.Unverified

                        };
                        return response;
                    }
                }
                return new AddKycResponse
                {
                    Success = false,
                    Message = ApiHelper.TryGetError(data)
                };
            }
        }

        public async Task<VerifyKYCInfoResponse> VerifyKYCInfo(VerifyKYCInfoRequest msg)
        {
            var req = new RestRequest("user/doc/verify", Method.POST);
            var body = new
            {
                login = new
                {
                    oauth_key = msg.OAuth.Key,
                },
                user = new
                {
                    doc = new
                    {
                        question_set_id = msg.QuestionSetId,
                        answers = msg.Answers.Select(c => new { question_id = c.QuestionId, answer_id = c.AnswerId }).ToArray()
                    },
                    fingerprint = msg.Fingerprint
                },

            };
            req.AddJsonBody(body);

            var resp = await this._api.ExecuteTaskAsync(req);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);
            RaiseOnAfterRequest(body, req, resp);

            if (resp.IsHttpOk() && data.success)
            {
                return new VerifyKYCInfoResponse
                {
                    Success = true,
                    Permission = ParsePermission(data.user.permission),
                    Message = ApiHelper.TryGetMessage(data)
                };
            }
            else
            {
                return new VerifyKYCInfoResponse
                {
                    Success = false,
                    Message = ApiHelper.TryGetError(data)
                };
            }

        }

        public async Task<AddDocResponse> AddDocAsync(AddDocRequest msg)
        {
            var req = new RestRequest("user/doc/attachments/add", Method.POST);
            var body = new
            {
                login = new
                {
                    oauth_key = msg.OAuth.Key,
                },
                user = new
                {
                    doc = new
                    {
                        attachment = msg.Attachment
                    },
                    fingerprint = msg.Fingerprint
                },

            };
            req.AddJsonBody(body);

            var resp = await this._api.ExecuteTaskAsync(req);
            RaiseOnAfterRequest(body, req, resp);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);

            if (resp.IsHttpOk() && data.success)
            {
                return new AddDocResponse
                {
                    Success = true,
                    Permission = ParsePermission(data.user.permission),
                    Message = ApiHelper.TryGetMessage(data)
                };
            }
            else
            {
                return new AddDocResponse
                {
                    Success = false,
                    Message = ApiHelper.TryGetError(data)
                };
            }
        }


        public async Task<ShowUsersResponse> ShowUsersAsync(ShowUsersRequest msg)
        {
            var req = new RestRequest("user/client/users", Method.POST);
            dynamic body = new ExpandoObject();

            body.client = new
            {
                client_id = this._creds.ClientId,
                client_secret = this._creds.ClientSecret
            };
            if (msg != null && msg.Filter != null)
            {
                body.filter = new
                {
                    page = msg.Filter.Page,
                    query = msg.Filter.Query
                };
            }
            req.AddJsonBody(body);

            var resp = await this._api.ExecuteTaskAsync(req);
            RaiseOnAfterRequest(body, req, resp);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);

            if (resp.IsHttpOk() && data.success)
            {
                var list = new List<UserRecord>();
                var users = data.users;
                var r = new ShowUsersResponse
                {
                    Page = Convert.ToInt32(data.page),
                    PageCount = Convert.ToInt32(data.page_count),
                    Users = list,
                    Success = true
                };
                foreach (dynamic user in users)
                {
                    list.Add(new UserRecord
                    {
                        OId = user._id["$oid"],
                        DateJoinedUtc = ApiHelper.UnixTimestampInMillisecondsToUtc(Convert.ToInt64(user.extra.date_joined["$date"])),
                        Permission = ParsePermission(user.permission),
                        RefreshToken = user.refresh_token,
                        SupplementalId = ApiHelper.PropertyExists(user.extra, "supp_id") ? user.extra.supp_id : String.Empty,
                    });
                }
                return r;

            }
            else
            {
                return new ShowUsersResponse
                {
                    Message = ApiHelper.TryGetError(data),
                    Success = false
                };
            };

        }

        public async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest msg)
        {
            var req = new RestRequest("user/signin", Method.POST);

            dynamic id = new Dictionary<string, object>() { { "$oid", msg.UserOId } };
            var update = new Dictionary<string, object>();
            if(!String.IsNullOrEmpty(msg.NewPhoneNumber))
            {
                update.Add("phone_number", msg.NewPhoneNumber);
            }
            if(!String.IsNullOrEmpty(msg.NewEmail))
            {
                update.Add("login", new
                {
                    email = msg.NewEmail
                });
            }
            if(!String.IsNullOrEmpty(msg.NewLegalName))
            {
                update.Add("legal_name", msg.NewLegalName);
            }
            if(!String.IsNullOrEmpty(msg.RemovePhoneNumber))
            {
                update.Add("remove_phone_number", msg.RemovePhoneNumber);
            }
            var body = new
            {
                client = new
                {
                    client_id = _creds.ClientId,
                    client_secret = _creds.ClientSecret
                },
                login = new 
                {
                    email = msg.Email
                },
                user = new
                {
                    _id = id,
                    fingerprint = msg.Fingerprint,
                    ip = String.IsNullOrEmpty(msg.IpAddress) ? "10.0.0.1" : msg.IpAddress,
                    update = update
                },

            };
            req.AddJsonBody(body);

            var resp = await this._api.ExecuteTaskAsync(req);
            RaiseOnAfterRequest(body, req, resp);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);

            if (resp.IsHttpOk() && data.success)
            {
                return new UpdateUserResponse
                {
                    Success = true,
                    Permission = ParsePermission(data.user.permission),
                    Message = ApiHelper.TryGetMessage(data)
                };
            }
            else
            {
                return new UpdateUserResponse
                {
                    Success = false,
                    Message = ApiHelper.TryGetError(data)
                };
            }

        }

        public async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest msg)
        {
            var req = new RestRequest("user/signin", Method.POST);
            var _id = new Dictionary<string, string>()
            {
                { "$oid", msg.SynapseOId }
            };

            dynamic body = new
            {
                client = new
                {
                    client_id = this._creds.ClientId,
                    client_secret = this._creds.ClientSecret
                },
                login = new
                {
                    refresh_token = msg.RefreshToken,
                },
                user = new
                {
                    _id = _id,
                    ip = String.IsNullOrEmpty(msg.IPAddress) ? "10.0.0.1" : msg.IPAddress,
                    fingerprint = msg.Fingerprint
                },

            };

            req.AddJsonBody(body);

            var resp = await this._api.ExecuteTaskAsync(req);
            RaiseOnAfterRequest(body, req, resp);
            dynamic data = SimpleJson.DeserializeObject(resp.Content);

            if (resp.IsHttpOk() && data.success)
            {
                var oauth = data.oauth;
                return new RefreshTokenResponse
                {
                    Success = true,
                    OAuth = new SynapseUserOAuth
                    {
                        Key = oauth.oauth_key,
                        RefreshToken = oauth.refresh_token,
                        ExpirationUtc = DateTime.UtcNow.AddSeconds(Convert.ToInt64(oauth.expires_in))
                    },
                    Message = ApiHelper.TryGetMessage(data)
                };
            }
            else
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = ApiHelper.TryGetError(data)
                };
            }
        }

        private void RaiseOnAfterRequest(object body, IRestRequest req, IRestResponse resp)
        {
            string uri = "<empty>";
            string content = "<empt>";
            var code = System.Net.HttpStatusCode.Unused;
            if(resp != null)
            {
                uri = resp.ResponseUri == null ? "<unknown>" : resp.ResponseUri.ToString();
                code = resp.StatusCode;
                content = resp.Content;
                if(resp.ErrorException != null && String.IsNullOrEmpty(content))
                {
                    content = resp.ErrorException.ToString();
                }
            }
            OnAfterRequest?.Invoke(uri, code, SimpleJson.SerializeObject(body), content);
        }

        private static string ToString(SynapseDocumentType docType)
        {
            if(docType == SynapseDocumentType.SSN)
            {
                return "SSN";
            }
			else if(docType == SynapseDocumentType.Passport)
            {
                return "GOVT_ID_INT";
            }
			else if(docType == SynapseDocumentType.PersonalIdentification)
            {
                return "PERSONAL_IDENTIFICATION";
            }
			else if (docType == SynapseDocumentType.DriversLicense)
            {
                return "GOVT_ID";
            }
			else if(docType == SynapseDocumentType.None)
            {
                return "NONE";
            }
            throw new ArgumentOutOfRangeException("docType", docType, "This value is not supported");
        }
        private static SynapsePermission ParsePermission(string permission)
        {
            if (permission == "UNVERIFIED") return SynapsePermission.Unverified;
            else if (permission == "RECEIVE") return SynapsePermission.ReceiveOnly;
            else if (permission == "SEND-AND-RECEIVE") return SynapsePermission.SendAndReceive;
            else if (permission == "LOCKED") return SynapsePermission.Locked;
            throw new InvalidOperationException(String.Format("Unknown permission type {0}", permission));
        }
    }
}