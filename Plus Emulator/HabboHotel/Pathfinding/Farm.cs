using System;
using System.Data;
using System.Collections.Concurrent;
using Azure.HabboHotel.Roleplay.Jobs.Farming.Objects;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Items;
using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Roleplay.Jobs.Farming
{
    /// <summary>
    /// A new instance which is setup upon room initialization
    /// </summary>
    public class Farm
    {
        /// <summary>
        /// Room ID of the farm
        /// </summary>
        public uint MyRID;

        /// <summary>
        /// Room instance of the farm
        /// </summary>
        public Room MyRoomInstance;

        /// <summary>
        /// Containing the items on the map to gather
        /// </summary>
        public ConcurrentDictionary<uint, object> FarmItems;

        /// <summary>
        /// Starts the farm for the room
        /// </summary>
        public Farm(uint RID, Room RoomInstance)
        {
            this.MyRID = RID;
            this.MyRoomInstance = RoomInstance;

            FarmItems = new ConcurrentDictionary<uint, object>();
            LoadItems();
        }

        /// <summary>
        /// Loads items
        /// </summary>
        public void LoadItems()
        {
            foreach (DataRow Row in FarmManager.RP_Farms.Rows)
            {
                uint DB_RID = Convert.ToUInt32(Row["room_id"]);

                if (DB_RID == MyRID)
                {
                    int DB_X = Convert.ToInt32(Row["x"]);
                    int DB_Y = Convert.ToInt32(Row["y"]);

                    string DB_TYPE = Convert.ToString(Row["typeobject"]);
                    FarmManager.TypeOfObject ObjectType = FarmManager.GetObjectType(DB_TYPE);

                    uint ID = Convert.ToUInt32(FarmItems.Count + 1);
                    object Object = FarmManager.CreateObjectInstance(ID, ObjectType, this, DB_X, DB_Y);
                   
                    if (Object != null)
                        FarmItems.TryAdd(ID, Object);
                }
            }
        }

        public void PlaceInUserInventory(GameClient Session, RoomItem item)
        {
            
            MyRoomInstance.GetRoomItemHandler().RemoveFurniture(Session, item.Id, true);
            Session.GetHabbo()
                .GetInventoryComponent()
                .AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, 0, 0, "");
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
        }
    }
}