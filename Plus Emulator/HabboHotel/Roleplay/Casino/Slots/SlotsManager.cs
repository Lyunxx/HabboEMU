using System;
using System.Data;
using Plus.Collections;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using System.Collections.Concurrent;
using Plus.HabboHotel.PathFinding;

namespace Plus.HabboHotel.Roleplay.Casino.Slots
{
    public class SlotsManager
    {
        /// <summary>
        /// Thread-safe dictionary containing slots
        /// </summary>
        public static ConcurrentDictionary<uint, Slot> SlotsData = new ConcurrentDictionary<uint, Slot>();

        /// <summary>
        /// Thread-safe dictionary containing tiles for each slot
        /// </summary>
        public static MultiKeyDictionary<int, int, Tile> SlotsTilesData = new MultiKeyDictionary<int, int, Tile>();

        /// <summary>
        /// Boolean representing if the farming spots have been initialized
        /// </summary>
        public static bool initiated = false;

        /// <summary>
        /// Caches slots
        /// </summary>
        public static void init()
        {
            DataTable Slots = null;
            DataTable SlotsTiles = null;
            DataRow SlotsCost = null;
            DataRow SlotsPrizeDivider = null;
            DataRow SlotsJackpotDivider = null;

            SlotsData.Clear();
            SlotsTilesData.Clear();

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rp_slots");
                Slots = dbClient.GetTable();

                dbClient.SetQuery("SELECT * FROM rp_settings WHERE type = 'SLOTSCOST'");
                SlotsCost = dbClient.GetRow();

                dbClient.SetQuery("SELECT * FROM rp_settings WHERE type = 'SLOTSPRIZE'");
                SlotsPrizeDivider = dbClient.GetRow();

                dbClient.SetQuery("SELECT * FROM rp_settings WHERE type = 'SLOTSJACKPOT'");
                SlotsJackpotDivider = dbClient.GetRow();

                RoleplayManager.SlotsMachineCost = Convert.ToInt32(SlotsCost["data"]);
                RoleplayManager.SlotsMachinePrizeDivider = Convert.ToInt32(SlotsPrizeDivider["data"]);
                RoleplayManager.SlotsMachineJackpotDivider = Convert.ToInt32(SlotsJackpotDivider["data"]);

                foreach (DataRow slot in Slots.Rows)
                {
                    dbClient.SetQuery("SELECT * FROM rp_slots_tiles WHERE slot_id = " + Convert.ToInt32(slot["id"]) + "");

                    SlotsTiles = dbClient.GetTable();

                    foreach (DataRow tile in SlotsTiles.Rows)
                    {
                        int SlotId = Convert.ToInt32(tile["slot_id"]);
                        int TileId = Convert.ToInt32(tile["tile_id"]);
                        int RoomId = Convert.ToInt32(tile["room_id"]);

                        int x1 = Convert.ToInt32(tile["x1"]);
                        int x2 = Convert.ToInt32(tile["x2"]);
                        int y1 = Convert.ToInt32(tile["y1"]);
                        int y2 = Convert.ToInt32(tile["y2"]);
                        int z1 = Convert.ToInt32(tile["z1"]);
                        int z2 = Convert.ToInt32(tile["z2"]);
                        int rot = Convert.ToInt32(tile["rot"]);

                        var newtile = new Tile(SlotId, RoomId, TileId, x1, x2, y1, y2, z1, z2, rot);
                        SlotsTilesData.Add(SlotId, TileId, newtile);
                    }

                    uint Id = Convert.ToUInt32(slot["id"]);
                    uint Roomid = Convert.ToUInt32(slot["roomid"]);

                    int X = Convert.ToInt32(slot["x"]);
                    int Y = Convert.ToInt32(slot["y"]);
                    int Z = Convert.ToInt32(slot["z"]);
                    int Rot = Convert.ToInt32(slot["rot"]);

                    var newslot = new Slot(Id, Roomid, X, Y, Z, Rot);
                    SlotsData.TryAdd(Id, newslot);
                }
            }
        }

        /// <summary>
        /// Checks if user is near the slot machine
        /// </summary>
        public static bool isUserNearMachine(Slot slot, RoomUser user)
        {
            if (slot != null && user != null)
            {
                try
                {
                    if (RoleplayManager.Distance(new Vector2D(slot.X, slot.Y), new Vector2D(user.X, user.Y)) <= 1 && user.RoomId == slot.RoomId)
                    {
                        return true;
                    }

                    return false;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the slot machine information based on the roomitem
        /// </summary>
        public static Slot getSlotbyItem(RoomItem item)
        {
            if (item != null)
            {
                try
                {
                    Slot theSlot = null;

                    foreach (Slot slot in SlotsData.Values)
                    {
                        if (slot.Slots == item)
                        {
                            return slot;
                        }
                    }

                    return theSlot;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

    }
}
