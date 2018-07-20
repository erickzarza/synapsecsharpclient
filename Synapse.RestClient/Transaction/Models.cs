using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.RestClient.Transaction
{
    using User;
    public class AddTransactionRequest
    {
        public SynapseUserOAuth OAuth { get; set; }
        public string IpAddress { get; set; }
        public string LocalId { get; set; }
        public string Fingerprint { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public SynapseNodeTransactionType FromNodeType { get; set; }
        public string FromNodeId { get; set; }
        public SynapseNodeTransactionType ToNodeType { get; set; }
        public string ToNodeId { get; set; }
        public uint ProcessOn {get; set;}

        public string Note { get; set; }
        public string WebhookUrl { get; set; }

        public decimal Fee {get; set;}
        public string FeeNote {get; set;}

        public string FeeNodeId {get; set;}
		public bool SameDay { get; set; }
	}

    public class AddTransactionResponse
    {
        public bool Success {get; set;}
        public string Message {get; set;}
        public string TransactionOId {get; set;}
        public SynapseTransactionStatus Status {get; set;}
    }

    
    public class SynapseTransactionStatus
    {
        public DateTime OnUtc {get; set;}
        public string Note {get; set;}
        public string StatusDescription {get; set;}
        public SynapseTransactionStatusCode Status {get; set;}

    }

    public class CancelTransactionRequest
    {
        public SynapseUserOAuth OAuth {get; set;}
        public string Fingerprint { get; set; }
        public string TransactionOId {get; set;}
    }

    public class CancelTransactionResponse
    {
        public bool Success {get; set;}
        public string Message { get; set; }
    }

    public enum SynapseTransactionStatusCode
    {
        QueuedBySynapse = -1,
        QueuedByClient = 0,
        Created = 1,
        ProcessingDebit = 2,
        ProcesingCredit = 3,
        Settled = 4,
        Cancelled = 5,
        Returned = 6
    }

    public class SynapseCurrencies
    {
        public const string USD = "USD";
    }
    public enum SynapseNodeTransactionType
    {
        ACHUS,
        SYNAPSEUS
    }

    public class Transaction
    {
        public string OId { get; set; }
        public decimal AmountUsd { get; set; }
        public bool IsDebit { get; set; }
        public bool IsCredit { get { return !IsDebit; } }
        public string FromUserOId { get; set; }
        public string ToUserOId { get; set; }
        public SynapseTransactionStatusCode LastStatus { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    
        public string SuppId { get; internal set; }

        public IList<SynapseTransactionStatus> History { get; set; }
    }
    
    public class TransactionsResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public IList<Transaction> Transactions { get; set; }
    }

    public class TransactionsRequest
    {
        public SynapseUserOAuth OAuth { get; set; }
        public string NodeOId { get; set; }
        public string Fingerprint { get; set; }
    }
}
