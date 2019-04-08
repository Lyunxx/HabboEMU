using System;
using Plus.HabboHotel.Items;


namespace Plus.HabboHotel.Roleplay.Casino.Slots
{
    public class Tile
    {
        public RoomItem Tile1;
        public RoomItem Tile2;

        int TileFurni = 2072;

        int SlotId;
        int TileId;
        int RoomId;
        int X1;
        int X2;
        int Y1;
        int Y2;
        int Z1;
        int Z2;
        int Rot;

        public Tile(int SlotId, int RoomId, int TileId, int X1, int X2, int Y1, int Y2, int Z1, int Z2, int Rot)
        {
            this.SlotId = SlotId;
            this.TileId = TileId;
            this.RoomId = RoomId;
            this.X1 = X1;
            this.X2 = X2;
            this.Y1 = Y1;
            this.Y2 = Y2;
            this.Z1 = Z1;
            this.Z2 = Z2;
            this.Rot = Rot;

            removeOldPrizes();
            removeSameTiles();
            createTiles();
        }

        public void createTiles()
        {
            try
            {
                Tile1 = Misc.RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(TileFurni), X1, Y1, Z1, Rot, false, Convert.ToUInt32(RoomId));
                Tile1.ExtraData = "3";
                Tile1.UpdateState();

                Tile2 = Misc.RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(TileFurni), X2, Y2, Z2, Rot, false, Convert.ToUInt32(RoomId));
                Tile2.ExtraData = "6";
                Tile2.UpdateState();
            }
            catch
            {
            }
        }

        public void removeSameTiles()
        {
            try
            {
                var currentRoom = Plus.GetGame().GetRoomManager().GetRoom(Convert.ToUInt32(RoomId));

                foreach (var Item in currentRoom.GetRoomItemHandler().FloorItems.Values)
                {
                    if (Item.BaseItem == TileFurni)
                    {
                        if (Item.X == X1 && Item.Y == Y1 && Item.RoomId == Convert.ToUInt32(RoomId))
                        {
                            Misc.RoleplayManager.PickSlotTilePrize(Item, Convert.ToUInt32(RoomId));
                        }
                        if (Item.X == X2 && Item.Y == Y2 && Item.RoomId == Convert.ToUInt32(RoomId))
                        {
                            Misc.RoleplayManager.PickSlotTilePrize(Item, Convert.ToUInt32(RoomId));
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void removeOldPrizes()
        {
            try
            {
                var currentRoom = Plus.GetGame().GetRoomManager().GetRoom(Convert.ToUInt32(RoomId));

                foreach (var Item in currentRoom.GetRoomItemHandler().FloorItems.Values)
                {
                    if (Item.BaseItem == 761 || Item.BaseItem == 6877 || Item.BaseItem == 6876 || Item.BaseItem == 3855 || Item.BaseItem == 6878 || Item.BaseItem == 55005)
                    {
                        if (Item.X == X1 && Item.Y == Y1 && Item.RoomId == Convert.ToUInt32(RoomId))
                        {
                            Misc.RoleplayManager.PickSlotTilePrize(Item, Convert.ToUInt32(RoomId));
                        }
                        if (Item.X == X2 && Item.Y == Y2 && Item.RoomId == Convert.ToUInt32(RoomId))
                        {
                            Misc.RoleplayManager.PickSlotTilePrize(Item, Convert.ToUInt32(RoomId));
                        }
                    }
                }
            }
            catch
            {
            }
        }

    }
}