using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;

namespace Synapse.RestClient.User
{
    using User;
    using Node;
    [TestClass]
    public class UserApiTests : BaseScenarioTest
    {
        ISynapseUserApiClient _user;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            this._user = new SynapseUserApiClient(this.Credentials, this.BaseUrl);
        }


        [TestMethod]
        public async Task CreatesUser()
        {
            var result = await this._user.CreateUserAsync(this.CreateUserRequest());

            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
            result.OAuth.ShouldNotBeNull();
            result.OAuth.Key.ShouldNotBeEmpty();
            result.OAuth.RefreshToken.ShouldNotBeEmpty();
            result.Permission.ShouldEqual(SynapsePermission.Unverified);
        }

        [TestMethod]
        public async Task AddsKycSuccessfully()
        {
            var user = await this._user.CreateUserAsync(this.CreateUserRequest());
            var result = await this._user.AddKycAsync(this.CreateKycRequest(user.OAuth));

            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
            //result.Permission.ShouldEqual(SynapsePermission.ReceiveOnly); //TODO: Discuss
            result.HasKBAQuestions.ShouldBeFalse();
        }

		[TestMethod]
		public async Task AddsDocsSuccessfully()
		{
			var user = await this._user.CreateUserAsync(this.CreateUserRequest());
			var result = await this._user.AddDocsAsync(this.CreateAddDocsRequest(user.OAuth));

			result.ShouldNotBeNull();
			result.Success.ShouldBeTrue();
			//result.Permission.ShouldEqual(SynapsePermission.ReceiveOnly); //TODO: Discuss		
		}

		[TestMethod]
        public async Task AddsDocumentation()
        {
            var user = await this._user.CreateUserAsync(this.CreateUserRequest());
            var kyc = await this._user.AddKycAsync(this.CreateKycRequest(user.OAuth));
            var result = await this._user.AddDocAsync(this.CreateAddDocRequest(user.OAuth));
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
            //result.Permission.ShouldEqual(SynapsePermission.SendAndReceive);  //TODO: Discuss
        }


        [TestMethod]
        public async Task GetsKBAQuestionSet()
        {
            this.Person = this.CreatePerson(SynapseTestDocumentValues.PassValidationButVerificationRequired);
            var user = await this._user.CreateUserAsync(this.CreateUserRequest());
            var kyc = await this._user.AddKycAsync(this.CreateKycRequest(user.OAuth));
            kyc.ShouldNotBeNull();
            kyc.Success.ShouldBeTrue();
            kyc.HasKBAQuestions.ShouldBeTrue();
            kyc.KBAQuestionSet.ShouldNotBeNull();
            kyc.KBAQuestionSet.Questions.Length.ShouldEqual(5);
            foreach (var q in kyc.KBAQuestionSet.Questions)
            {
                q.Id.ShouldBeGreaterThan(0);
                q.Text.ShouldNotBeEmpty();
                q.Answers.Length.ShouldEqual(5);
                foreach (var a in q.Answers)
                {
                    a.Id.ShouldBeGreaterThan(0);
                    a.Text.ShouldNotBeEmpty();
                }
            }
            //result.Permission.ShouldEqual(SynapsePermission.SendAndReceive);  //TODO: Discuss
        }
        [TestMethod]
        public async Task AnswerKBAFully()
        {
            this.Person = this.CreatePerson(SynapseTestDocumentValues.PassValidationButVerificationRequired);
            var user = await this._user.CreateUserAsync(this.CreateUserRequest());
            var kyc = await this._user.AddKycAsync(this.CreateKycRequest(user.OAuth));
            var answ = await this._user.VerifyKYCInfo(new VerifyKYCInfoRequest
            {
                Fingerprint = Fingerprint,
                OAuth = user.OAuth,
                QuestionSetId = kyc.KBAQuestionSet.Id,
                Answers = new VerifyKYCInfoAnswer[]
                 {
                     new VerifyKYCInfoAnswer { AnswerId = 5, QuestionId =1 },
                     new VerifyKYCInfoAnswer { AnswerId = 5, QuestionId =2 },
                     new VerifyKYCInfoAnswer { AnswerId = 5, QuestionId =3 },
                     new VerifyKYCInfoAnswer { AnswerId = 5, QuestionId =4 },
                     new VerifyKYCInfoAnswer { AnswerId = 5, QuestionId =5 },
                     new VerifyKYCInfoAnswer { AnswerId = 5, QuestionId =6 }
                 }
            });
            answ.ShouldNotBeNull();
            answ.Success.ShouldBeTrue();

            //result.Permission.ShouldEqual(SynapsePermission.SendAndReceive);  //TODO: Discuss
        }

        [TestMethod]

        public async Task RefreshToken()
        {
            var token = await this._user.RefreshToken(new RefreshTokenRequest
            {
                Fingerprint = "c1c4622e155d1ba135f161a6228b2c2c",
                SynapseOId = "562d50fb86c27374bdd58257",
                RefreshToken = "refresh-691a462f-bc9c-4cc9-af5a-68dc2c7afbda",
                IPAddress = IpAddress
            });

            token.Success.ShouldBeTrue();
            token.OAuth.ShouldNotBeNull();
            token.OAuth.Key.ShouldNotBeNull();
            token.OAuth.RefreshToken.ShouldNotBeNull();
            token.OAuth.ExpirationUtc.ShouldBeGreaterThan(DateTime.UtcNow);
        }

        [TestMethod]
        public async Task ShowExistingUser()
        {
            var user = await this._user.CreateUserAsync(this.CreateUserRequest());
            var show = await this._user.ShowUsersAsync(new ShowUsersRequest
            {
                Filter = new UserFilter
                {
                    Page = 1,
                    Query = this.Person.EmailAddress
                }
            });

            show.Success.ShouldBeTrue();
            show.Page.ShouldEqual(1);
            show.Page.ShouldEqual(1);
            show.Users.ShouldNotBeNull();
            show.Users.Count.ShouldEqual(1);

            var found = show.Users.First();
            found.OId.ShouldEqual(user.SynapseOId);
            found.Permission.ShouldEqual(user.Permission);
            found.DateJoinedUtc.ShouldBeGreaterThan(DateTime.UtcNow.AddSeconds(-2)); //TODO: Hacky
        }

        [TestMethod]
        public async Task UpdateUser()
        {
            var user = await this._user.CreateUserAsync(this.CreateUserRequest());
            var update = await this._user.UpdateUser(new UpdateUserRequest
            {
                UserOId = user.SynapseOId,
                OAuth = user.OAuth,
                Fingerprint = Fingerprint,
                IpAddress = IpAddress,
                Email = Person.EmailAddress,
                NewEmail = "somebodyyyy@someemailthatdoesntreallyexist.org"
            });
            //TODO: check with synapse why this is failing.
            //update.Success.ShouldBeTrue();

        }

        [TestMethod]
        public async Task ShowNonExistingUser()
        {
            var show = await this._user.ShowUsersAsync(new ShowUsersRequest
            {
                Filter = new UserFilter
                {
                    Page = 1,
                    Query = this.Person.EmailAddress
                }
            });
            show.Success.ShouldBeTrue();
            show.Users.ShouldNotBeNull();
            show.Users.Count.ShouldEqual(0);

        }

    }
}