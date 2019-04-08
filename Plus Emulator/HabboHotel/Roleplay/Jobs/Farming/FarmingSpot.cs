using System;
using Plus.HabboHotel.Items;


namespace Plus.HabboHotel.Roleplay.Jobs.Farming
{
    public class FarmingSpot
    {
        #region Constructor

        public FarmingSpot(uint id, uint roomid, int x, int y, string type)
        {
            this.id = id;
            this.roomid = roomid;
            this.x = x;
            this.y = y;

            this.type = type;

            removeSameFarmingSpots();
            createFarmingSpot();
        }

        #endregion

        #region Declares

        public RoomItem FarmingSpotItem;

        public uint id;
        public uint roomid;
        public int x;
        public int y;
        public string type;
        public bool beingFarmed = false;
        public bool Part1Complete = false;
        public bool beingFarmed2 = false;

        #endregion

        #region Methods

        public void removeSameFarmingSpots()
        {
            try
            {
                var currentRoom = Plus.GetGame().GetRoomManager().GetRoom(roomid);

                foreach (var Item in currentRoom.GetRoomItemHandler().FloorItems.Values)
                {
                    if (Item.X == x && Item.Y == y && Item.RoomId == roomid && Item.BaseItem == 1737)
                    {
                        Misc.RoleplayManager.PickFarmingSpot(Item, roomid);
                    }
                    if (Item.X == x && Item.Y == y && Item.RoomId == roomid && Item.BaseItem == 2946)
                    {
                        Misc.RoleplayManager.PickFarmingSpot(Item, roomid);
                    }
                    if (Item.X == x && Item.Y == y && Item.RoomId == roomid && Item.BaseItem == 6699)
                    {
                        Misc.RoleplayManager.PickFarmingSpot(Item, roomid);
                    }
                }
            }
            catch
            {
            }
        }

        public void createFarmingSpot()
        {
            try
            {
                FarmingSpotItem = Misc.RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(1737), x, y, 0, 0, false, roomid);
            }
            catch
            {
            }
        }

        #endregion
    }
}