﻿using System;
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
    }

    public class SynapseUserOAuth
    {
        public string Key { get; set; }
        public string RefreshToken { get; set; }
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
        public SynapseDocumentType DocumentType { get; set; }
        public string DocumentValue { get; set; }
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

    public enum SynapseDocumentType
    {
        None = 0,
        SSN = 1
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

}
