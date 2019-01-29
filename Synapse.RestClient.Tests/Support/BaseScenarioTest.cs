using Synapse.RestClient.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.RestClient
{
    using User;
    using Node;
    public abstract class BaseScenarioTest : BaseTest
    {
        protected Person Person { get; set; }

        public override void Init()
        {
            base.Init();
            this.Person = this.CreatePerson(SynapseTestDocumentValues.PassValidationNoVerificationRequired);
        }

        protected virtual Person CreatePerson(string ssn)
        {
            return Person.CreateRandom(ssn);
        }

        protected CreateUserRequest CreateUserRequest()
        {
            return new CreateUserRequest
            {
                EmailAddress = this.Person.EmailAddress,
                FirstName = this.Person.FirstName,
                LastName = this.Person.EmailAddress,
                PhoneNumber = "555-123-1233",
                IpAddress = IpAddress,
                LocalId = "LocalId",
                Fingerprint = Fingerprint
            };
        }

        protected AddKycRequest CreateKycRequest(SynapseUserOAuth oauth)
        {
            return new AddKycRequest
            {
                OAuth = oauth,
                Address1 = "1428 Elm Street",
                Address2 = "#4",
                City = "Brooklyn",
                State = "New York",
                PostalCode = "11215",
                CountryCode = "US",
                DateOfBirth = DateTime.Parse("10/19/1979").Date,
                DocumentType = SynapseDocumentType.SSN,
                DocumentValue = this.Person.DocumentValue,
                Fingerprint = Fingerprint,
                FirstName = this.Person.FirstName,
                LastName = this.Person.LastName
            };
        }

        protected AddDocRequest CreateAddDocRequest(SynapseUserOAuth oauth)
        {
            return new AddDocRequest
            {
                OAuth = oauth,
                Attachment = GetTextResource("Base64Attachment.txt"),
                Fingerprint = Fingerprint
            };
        }
        protected AddACHNodeRequest CreateAddACHNodeRequest(SynapseUserOAuth oauth)
        {
            return new AddACHNodeRequest
            {
                OAuth = oauth,
                AccountClass = SynapseNodeClass.Checking,
                AccountNumber = "1234",
                RoutingNumber = "021000021", //Chase NYC
                AccountType = SynapseNodeType.Personal,
                Fingerprint = Fingerprint,
                LocalId = "1234",
                NameOnAccount = "Freddy Krueger Jr.",
                Nickname = "Freddy's Chase Checking"
            };
        }
    }

}
