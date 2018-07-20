using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;
using Synapse.RestClient.Node;
using Synapse.RestClient.Transaction;
using Synapse.RestClient.User;

namespace Synapse.RestClient
{
    [TestClass]
    public class TransactionApiTests : BaseScenarioTest
    {
        private CreateUserResponse Sender;
        private AddACHNodeResponse SenderNode;

        private ISynapseTransactionApiClient _api;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
        }

        private async Task InitializeAsync()
        {
            this.Person = this.CreatePerson(SynapseTestDocumentValues.PassValidationNoVerificationRequired);
            this._api = Factory.CreateTransactionClient();
            var userApi = this.Factory.CreateUserClient();
            var nodeApi = this.Factory.CreateNodeClient();
            var user = await userApi.CreateUserAsync(this.CreateUserRequest());
            var kyc = await userApi.AddKycAsync(this.CreateKycRequest(user.OAuth));
            var doc = await userApi.AddDocAsync(this.CreateAddDocRequest(user.OAuth));
            this.Sender = user;
            this.SenderNode = await nodeApi.AddACHNodeAsync(new AddACHNodeRequest
            {
                OAuth = user.OAuth,
                AccountClass = SynapseNodeClass.Checking,
                AccountNumber = "123456786",
                RoutingNumber = "021000021", //Chase NYC
                AccountType = SynapseNodeType.Personal,
                Fingerprint = Fingerprint,
                LocalId = "1234",
                NameOnAccount = "Freddy Krueger Jr.",
                Nickname = "Freddy's Chase Checking"
            });
            
        }

        [TestMethod]
        public async Task AddsTransaction()
        {
            await InitializeAsync();
            var trans =
                await
                    this._api.AddTransactionAsync(new AddTransactionRequest()
                    {
                        Amount = 109.50m,
                        Currency = SynapseCurrencies.USD,
                        Fingerprint = Fingerprint,
                        FromNodeType = SynapseNodeTransactionType.ACHUS,
                        FromNodeId = SenderNode.SynapseNodeOId,
                        ToNodeType = SynapseNodeTransactionType.SYNAPSEUS,
                        ToNodeId = "55e9da8a86c2733ede53dac8",
                        LocalId = "1234",
                        IpAddress = "10.0.0.1",
                        OAuth = this.Sender.OAuth,
                        ProcessOn = 0,
                        Note = "Offer ALKJ-JA98",
						SameDay = false
                    });

            trans.Success.ShouldBeTrue();
            trans.Status.ShouldNotBeNull();
            trans.Status.Status.ShouldEqual(SynapseTransactionStatusCode.Created);
            trans.TransactionOId.ShouldNotBeEmpty();
        }
    }
}
