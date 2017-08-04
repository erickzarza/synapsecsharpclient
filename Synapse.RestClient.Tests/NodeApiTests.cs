using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;

namespace Synapse.RestClient
{
    using User;
    using Node;
    [TestClass]
    public class NodeApiTests : BaseScenarioTest
    {
        ISynapseNodeApiClient _node;
        ISynapseUserApiClient _user;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            this._node = this.Factory.CreateNodeClient();
            this._user = this.Factory.CreateUserClient();
        }


        [TestMethod]
        public async Task AddsNode()
        {
            var user = await this._user.CreateUserAsync(this.CreateUserRequest());
            await this._user.AddKycAsync(this.CreateKycRequest(user.OAuth));
            await this._user.AddDocAsync(this.CreateAddDocRequest(user.OAuth));
            var result = await this._node.AddACHNodeAsync(new AddACHNodeRequest
            {
                OAuth = user.OAuth,
                AccountClass = SynapseNodeClass.Checking,
                AccountNumber = "12345678",
                RoutingNumber = "021000021", //Chase NYC
                AccountType = SynapseNodeType.Personal,
                Fingerprint = Fingerprint,
                LocalId = "1234",
                NameOnAccount = "Freddy Krueger Jr.",
                Nickname = "Freddy's Chase Checking"
            });
            result.Success.ShouldBeTrue();
            result.SynapseNodeOId.ShouldNotBeEmpty();

        }

        [TestMethod]
        public async Task InvalidRoutingNumber()
        {
            var user = await this._user.CreateUserAsync(this.CreateUserRequest());
            await this._user.AddKycAsync(this.CreateKycRequest(user.OAuth));
            await this._user.AddDocAsync(this.CreateAddDocRequest(user.OAuth));
            var node = await this._node.AddACHNodeAsync(new AddACHNodeRequest
            {
                OAuth = user.OAuth,
                AccountClass = SynapseNodeClass.Checking,
                AccountNumber = "12345678",
                RoutingNumber = "123456",
                AccountType = SynapseNodeType.Personal,
                Fingerprint = Fingerprint,
                LocalId = "1234",
                NameOnAccount = "Freddy Krueger Jr.",
                Nickname = "Freddy's Chase Checking"
            });
            node.Success.ShouldBeFalse();
            node.Permission.ShouldEqual(SynapseNodePermission.None);
            

        }
        [TestMethod]
        public async Task VerifiesNode()
        {
            var user = await this._user.CreateUserAsync(this.CreateUserRequest());
            await this._user.AddKycAsync(this.CreateKycRequest(user.OAuth));
            await this._user.AddDocAsync(this.CreateAddDocRequest(user.OAuth));
            var node = await this._node.AddACHNodeAsync(new AddACHNodeRequest
            {
                OAuth = user.OAuth,
                AccountClass = SynapseNodeClass.Checking,
                AccountNumber = "12345678",
                RoutingNumber = "021000021", //Chase NYC
                AccountType = SynapseNodeType.Personal,
                Fingerprint = Fingerprint,
                LocalId = "1234",
                NameOnAccount = "Freddy Krueger Jr.",
                Nickname = "Freddy's Chase Checking"
            });
            node.Success.ShouldBeTrue();
            var result = await this._node.VerifyACHNodeAsync(new VerifyACHNodeRequest
            {
                OAuth = user.OAuth,
                SynapseNodeOId = node.SynapseNodeOId,
                Fingerprint = Fingerprint,
                Deposits = new[] { 0.10m, 0.10m }
            });
            result.Success.ShouldBeTrue();
            result.SynapseNodeOId.ShouldNotBeEmpty();
            result.IsActive.ShouldBeTrue();
            result.Permission.ShouldEqual(SynapseNodePermission.CreditAndDebit);

        }
    }
}
