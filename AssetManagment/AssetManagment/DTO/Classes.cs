using Microsoft.VisualBasic;

namespace AssetManagment.DTO
{
    public class Asset
    {
        public int AssetID { get; set; }
        public string AssetDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime AssetExpiry { get; set; }
        public int MaxAllocation { get; set; }
        public int MinApproval { get; set; }
        public List<AssetApprovers> Approver { get; set; }
    }

    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
    }

    public class AssetApprovers
    {
        public int AssetID { get; set; } // Foreign Key to Asset
        public int ApproverID { get; set; } // Foreign Key to User

     
    }

    public class Approvals
    {public int RequestID { get; set; }
        public int AssetID { get; set; } // Foreign Key to Asset
        public int ApproverID { get; set; } // Foreign Key to User
        public DateTime ApprovedOn { get; set; }


    }

    public class AllocatedAsset
    {
        public int AssetID { get; set; } // Foreign Key to Asset
        public int UserID { get; set; } // Foreign Key to User

    }

    public class AssetAllocationRequest
    {
        public int RequestID { get; set; } // Unique identifier for the request
        public int AssetID { get; set; } // Foreign Key to Asset
        public int UserID { get; set; } // Foreign Key to User
        public DateTime RequestDate { get; set; }

       
    }

    public class AssetAllocationLog
    {
        public int RequestID { get; set; } // Foreign Key to AssetAllocation
        public int ApproverID { get; set; } // Foreign Key to User
        public DateTime ApprovalDate { get; set; }

   
    }
    public class CreateAssetRequest
    {
        public Asset Asset { get; set; }    
        public List<int> ApproverIds { get; set; }
      
    }
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpiryInMinutes { get; set; }
    }

}