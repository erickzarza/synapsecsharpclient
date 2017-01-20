using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.RestClient.Node
{
    using User;
    public enum SynapseNodeType
    {
        Business,
        Personal        
    }
    public enum SynapseNodeClass
    {
        Savings,
        Checking
    }
    public enum SynapseNodePermission
    {
        None = 0,
        Credit,
        CreditAndDebit,
        Locked
    }
    public class AddACHNodeRequest
    {
        public SynapseUserOAuth OAuth { get; set; }
        public string LocalId { get; set; }
        public string Fingerprint { get; set; }
        public string Nickname { get; set; }
        public string NameOnAccount { get; set; }
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public SynapseNodeType AccountType { get; set; }
        public SynapseNodeClass AccountClass { get; set; }
        
    }

    public class AddACHNodeResponse
    {
        public string SynapseNodeOId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsActive { get; set; }
        public SynapseNodePermission Permission { get; set; }
    }

    public class VerifyACHNodeRequest
    {
        public SynapseUserOAuth OAuth { get; set; }
        public string SynapseNodeOId { get; set; }
        public string Fingerprint { get; set; }
        public decimal[] Deposits { get; set; } 
    }

    public class VerifyACHNodeResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string SynapseNodeOId { get; set; }
        public SynapseNodePermission Permission { get; set; }
        public bool IsActive { get; set; }
    }

    public class ACHNodeRequest
    {
        public string SynapseNodeId { get; set; }
        public SynapseUserOAuth OAuth { get; set; }
        public string Fingerprint { get; set; }
    }

    public class ACHNodeResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public SynapseNodePermission Permission { get; set; }
        public bool IsActive { get; set; }
    }

}
