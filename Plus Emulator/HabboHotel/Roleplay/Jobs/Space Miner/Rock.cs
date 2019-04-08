#region Librarys

using System;
using Plus.HabboHotel.Items;

#endregion

namespace Plus.HabboHotel.Roleplay.Jobs.Space
{
    public class Rock
    {
        #region Constructor

        public Rock(uint id, uint roomid, int x, int y, string data)
        {
            this.id = id;
            this.roomid = roomid;
            this.x = x;
            this.y = y;

            this.data = data;

            removeSameRocks();
            createRock();
        }

        #endregion

        #region Declares

        public RoomItem rockItem;

        public uint id;
        public uint roomid;
        public int x;
        public int y;
        public string data;
        public bool beingMined = false;

        #endregion

        #region Methods

        public void removeSameRocks()
        {
            try
            {
                var currentRoom = Plus.GetGame().GetRoomManager().GetRoom(roomid);

                foreach (var Item in currentRoom.GetRoomItemHandler().FloorItems.Values)
                {
                    if (Item.X == x && Item.Y == y && Item.RoomId == roomid && Item.BaseItem == 1943)
                    {
                        Misc.RoleplayManager.PickRock(Item, roomid);
                    }
                }
            }
            catch
            {
            }
        }

        public void createRock()
        {
            try
            {
                rockItem = Misc.RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(1943), x, y, 0, 0, false, roomid);
            }
            catch
            {
            }
        }

        #endregion
    }
}