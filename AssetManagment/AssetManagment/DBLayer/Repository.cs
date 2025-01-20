using AssetManagment.DTO;

namespace AssetManagment.DBLayer
{
    public class Repository
    {
        public static List<Asset> Assets = new();
        public static List<User> Users = new();
        public static List<AssetApprovers> AssetApprovers = new();
        public static List<AssetAllocationRequest> AssetAllocationsRequest = new();
        public static List<AssetAllocationLog> AllocationLogs = new();
        public static List<AllocatedAsset> Allocations = new();
        public static List<Approvals> Approvals = new();
    }
}
