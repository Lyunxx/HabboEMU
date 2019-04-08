using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.UserDataManagement;

namespace Plus.HabboHotel.Users.Inventory
{
    /// <summary>
    /// Class InventoryGlobal.
    /// </summary>
    internal class InventoryGlobal
    {
        /// <summary>
        /// Gets the inventory.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="client">The client.</param>
        /// <param name="data">The data.</param>
        /// <returns>InventoryComponent.</returns>
        internal static InventoryComponent GetInventory(uint userId, GameClient client, UserData data)
        {
            return new InventoryComponent(userId, client, data);
        }
    }
}