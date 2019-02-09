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
                EmailAddress = "we@wero.com",
				FirstName = "Wero",
                LastName = "We",
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
				Address1 = "1829 E Gemini Dr",
				Address2 = "#4",
				City = "Tempe",
				State = "Arizona",
				PostalCode = "85283",
				CountryCode = "US",
				DateOfBirth = DateTime.Parse("10/19/1979").Date,
                DocumentType = SynapseDocumentType.SSN,
                DocumentValue = this.Person.DocumentValue,
                Fingerprint = Fingerprint,
                FirstName = this.Person.FirstName,
                LastName = this.Person.LastName
            };
        }
		protected AddDocsRequest CreateAddDocsRequest(SynapseUserOAuth oauth)
		{
			return new AddDocsRequest
			{
				OAuth = oauth,
				FirstName = "Wero",
				LastName = "We",
				EmailAddress = "we@wero.com",
				IpAddress = IpAddress,
				PhoneNumber = "555-123-1233",
				Address1 = "1829 E Gemini Dr",
				Address2 = "#4",
				City = "Tempe",
				State = "Arizona",
				PostalCode = "85283",
				CountryCode = "US",
				DateOfBirth = DateTime.Parse("10/19/1979").Date,
				VirtualDocumentType = SynapseDocumentType.SSN,
				VirtualDocumentValue = this.Person.DocumentValue,
				PhysicalDocumentType = SynapseDocumentType.GovtId,
				PhysicalDocumentValue = "data:text/csv;base64,SUQs==",//GetTextResource("Base64Attachment.txt"),
				Fingerprint = Fingerprint,
				
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
