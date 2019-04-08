using System;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Roleplay.Casino.Slots
{
    public class Slot
    {
        public RoomItem Slots;

        int SlotsFurni = 2192;

        public uint Id;
        public uint RoomId;
        public int X;
        public int Y;
        public int Z;
        public int Rot;

        public bool beingRolled = false;

        public Slot(uint Id, uint RoomId, int X, int Y, int Z, int Rot)
        {
            this.Id = Id;
            this.RoomId = RoomId;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.Rot = Rot;

            removeSameSlots();
            createSlots();
        }

        public void createSlots()
        {
            try
            {
                Slots = Misc.RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(SlotsFurni), X, Y, Z, Rot, false, RoomId);
            }
            catch
            {
            }
        }

        public void removeSameSlots()
        {
            try
            {
                var currentRoom = Plus.GetGame().GetRoomManager().GetRoom(RoomId);

                foreach (var Item in currentRoom.GetRoomItemHandler().FloorItems.Values)
                {
                    if (Item.X == X && Item.Y == Y && Item.Z == Z && Item.Rot == Rot && Item.RoomId == RoomId && Item.BaseItem == SlotsFurni)
                    {
                        Misc.RoleplayManager.PickFarmingSpot(Item, RoomId);
                    }
                }
            }
            catch
            {
            }
        }
    }
}