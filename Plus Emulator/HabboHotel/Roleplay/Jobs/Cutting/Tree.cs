#region Librarys

using System;
using Plus.HabboHotel.Items;

#endregion

namespace Plus.HabboHotel.Roleplay.Jobs.Cutting
{
    public class Tree
    
    {
        #region Constructor

        public Tree(uint id, uint roomid, int x, int y, string data)
        {
            this.id = id;
            this.roomid = roomid;
            this.x = x;
            this.y = y;

            this.data = data;

            removeSameTrees();
            createTree();
        }

        #endregion

        #region Declares

        public RoomItem TreeItem;

        public uint id;
        public uint roomid;
        public int x;
        public int y;
        public string data;
        public bool beingMined = false;

        #endregion

        #region Methods

        public void removeSameTrees()
        {
            try
            {
                var currentRoom = Plus.GetGame().GetRoomManager().GetRoom(roomid);

                foreach (var Item in currentRoom.GetRoomItemHandler().FloorItems.Values)
                {
                    if (Item.X == x && Item.Y == y && Item.RoomId == roomid && Item.BaseItem == 7024)
                    {
                        Misc.RoleplayManager.PickTree(Item, roomid);
                    }
                }
            }
            catch
            {
            }
        }

        public void createTree()
        {
            try
            {
                TreeItem = Misc.RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(7024), x, y, 0, 0, false, roomid);
            }
            catch
            {
            }
        }

        #endregion
    }
}