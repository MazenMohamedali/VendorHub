namespace VendorHub.Models
{
    [Flags]
    public enum PermissionType
    {
        None = 0,
        CanUploadProducts    = 1 << 0,
        CanEditProducts      = 1 << 1,
        CanDeleteProducts    = 1 << 2,
        CanViewOrders        = 1 << 3,
        CanUpdateOrderStatus = 1 << 4,
        CanCancelOrders      = 1 << 5,
        CanViewAnalytics     = 1 << 6,
        CanViewProducts      = 1 << 7,
        CanManageInventory   = 1 << 8,

        VendorAdmin = CanUploadProducts | CanEditProducts | CanDeleteProducts |
                      CanViewOrders | CanUpdateOrderStatus | CanCancelOrders |
                      CanViewAnalytics | CanViewProducts | CanManageInventory,

        VendorStaff = CanViewProducts | CanViewOrders | CanUpdateOrderStatus | CanManageInventory
    }
}
