using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.RestClient.User
{
    public class CreateUserRequest
    {
        public string LocalId { get; set; }
        public string EmailAddress { get; set; }
        public string Fingerprint { get; set; }
        public string IpAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
		public string Country { get; set; }
	}

    public class SynapseUserOAuth
    {
        public string Key { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpirationUtc { get; set; }
    }

    public class CreateUserResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string SynapseClientId { get; set; }
        public string SynapseOId { get; set; }
        public SynapseUserOAuth OAuth { get; set; }
        public SynapsePermission Permission { get; set; }
    }

    public class AddKycRequest
    {
        public SynapseUserOAuth OAuth { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public DateTime DateOfBirth { get; set; }
        public SynapseDocumentType VirtualDocumentType { get; set; }
        public string VirtualDocumentValue { get; set; }
        public string Fingerprint { get; set; }
    }

    public class AddKycResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool HasKBAQuestions { get; set; }
        public QuestionSet KBAQuestionSet { get; set; }
        public SynapsePermission Permission { get; set; }
    }

    public class VerifyKYCInfoRequest
    {
        public SynapseUserOAuth OAuth { get; set; }
        public string QuestionSetId { get; set; }
        public VerifyKYCInfoAnswer[] Answers { get; set; }
        public string Fingerprint { get; set; }
    }

    public class VerifyKYCInfoAnswer
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
    }
    public class VerifyKYCInfoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public SynapsePermission Permission { get; set; }
    }




    public class AddDocRequest
    {
        public SynapseUserOAuth OAuth { get; set; }
        public string Attachment { get; set; }
        public string Fingerprint { get; set; }
    }

    public class AddDocResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public SynapsePermission Permission { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
        public string SynapseOId { get; set; }
        public string IPAddress { get; set; }
        public string Fingerprint { get; set; }

    }

    public class RefreshTokenResponse
    {
        public SynapseUserOAuth OAuth { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public enum SynapseDocumentType
    {
        None = 0,
        SSN = 1,
        Passport = 2,
        PersonalIdentification = 3,
        DriversLicense = 4
    }
    public enum SynapsePermission
    {
        Unverified,
        SendAndReceive,
        ReceiveOnly,
        Locked
    }

    public class QuestionSet
    {
        public string Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Question[] Questions { get; set; }
    }

    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public Answer[] Answers { get; set; }
    }

    public class Answer
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }

    public class UserFilter
    {
        public int Page { get; set; }
        public string Query { get; set; }
    }

    public class ShowUsersRequest
    {
        public UserFilter Filter { get; set; }
    }
    public class ShowUsersResponse
    {
        public int Page { get; set; }
        public int PageCount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public IReadOnlyCollection<UserRecord> Users { get; set; }
    }

    public class UserRecord
    {
        public string OId { get; set; }
        public DateTime DateJoinedUtc { get; set; }
        public string SupplementalId { get; set; }
        public SynapsePermission Permission { get; set; }
        public string RefreshToken { get; set; }
    }

    public class UpdateUserRequest
    {
        public string UserOId { get; set; }
        public string Email { get; set; }
        public SynapseUserOAuth OAuth { get; set; }
        public string NewPhoneNumber { get; set; }
        public string RemovePhoneNumber { get; set; }
        public string NewLegalName { get; set; }
        public string NewEmail { get; set; }
        public string Fingerprint { get; set; }
        public string IpAddress { get; set; }
    }
    public class UpdateUserResponse
    {
        public bool Success { get; set; }
        public SynapsePermission Permission { get; set; }
        public string Message { get; set; }
    }


}