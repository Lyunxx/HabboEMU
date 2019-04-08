using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.Rooms;
using System;
using System.Data;

namespace Plus.HabboHotel.Items
{
    /// <summary>
    /// Class TeleHandler.
    /// </summary>
    internal static class TeleHandler
    {
        /// <summary>
        /// Gets the linked tele.
        /// </summary>
        /// <param name="teleId">The tele identifier.</param>
        /// <param name="pRoom">The p room.</param>
        /// <returns>System.UInt32.</returns>
        internal static UInt32 GetLinkedTele(UInt32 TeleId, Room pRoom)
        {
            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT tele_two_id FROM items_teleports WHERE tele_one_id = " + TeleId);
                DataRow Row = dbClient.GetRow();

                if (Row == null)
                {
                    return 0;
                }

                return Convert.ToUInt32(Row[0]);
            }
        }

        /// <summary>
        /// Gets the tele room identifier.
        /// </summary>
        /// <param name="teleId">The tele identifier.</param>
        /// <param name="pRoom">The p room.</param>
        /// <returns>System.UInt32.</returns>
        internal static UInt32 GetTeleRoomId(UInt32 TeleId, Room pRoom)
        {
            if (pRoom.GetRoomItemHandler().GetItem(TeleId) != null)
                return pRoom.RoomId;

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT room_id FROM items_rooms WHERE id = " + TeleId + " LIMIT 1");
                DataRow Row = dbClient.GetRow();

                if (Row == null)
                {
                    return 0;
                }

                return Convert.ToUInt32(Row[0]);
            }
        }

        /// <summary>
        /// Determines whether [is tele linked] [the specified tele identifier].
        /// </summary>
        /// <param name="teleId">The tele identifier.</param>
        /// <param name="pRoom">The p room.</param>
        /// <returns><c>true</c> if [is tele linked] [the specified tele identifier]; otherwise, <c>false</c>.</returns>
        internal static bool IsTeleLinked(uint teleId, Room pRoom)
        {
            var linkedTele = GetLinkedTele(teleId, pRoom);
            if (linkedTele == 0u)
                return false;
            var item = pRoom.GetRoomItemHandler().GetItem(linkedTele);
            return (item != null && (item.GetBaseItem().InteractionType == Interaction.Teleport || item.GetBaseItem().InteractionType == Interaction.QuickTeleport)) || GetTeleRoomId(linkedTele, pRoom) != 0u;
        }
    }
}