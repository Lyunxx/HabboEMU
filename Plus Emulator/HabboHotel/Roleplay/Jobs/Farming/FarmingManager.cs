using System;
using System.Data;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.Configuration;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.PathFinding;

namespace Plus.HabboHotel.Roleplay.Jobs.Farming
{
    internal class farmingManager
    {
        /// <summary>
        /// Thread-safe dictionary containing farming spots
        /// </summary>
        public static ConcurrentDictionary<uint, FarmingSpot> FarmingSpots = new ConcurrentDictionary<uint, FarmingSpot>();
        
        /// <summary>
        /// Boolean representing if the farming spots have been initialized
        /// </summary>
        public static bool initiated = false;

        /// <summary>
        /// Caches farming spots
        /// </summary>
        public static void init()
        {
            FarmingSpots.Clear();

            string Query = "SELECT * FROM rp_farming";

            try
            {
                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(Query);
                    DataTable farmingTable = dbClient.GetTable();

                    foreach (DataRow row in farmingTable.Rows)
                    {
                        uint Id = Convert.ToUInt32(row["id"]);
                        uint RoomId = Convert.ToUInt32(row["roomid"]);

                        int X = Convert.ToInt32(row["x"]);
                        int Y = Convert.ToInt32(row["y"]);
                        
                        string PlantType = row["type"].ToString();

                        var farmingspot = new FarmingSpot(Id, RoomId, X, Y, PlantType);
                        FarmingSpots.TryAdd(Id, farmingspot);
                    }
                }
            }
            catch (Exception ex) { Logging.LogQueryError(ex, Query); }
        }

        public static bool isUserNearFarmingSpot(FarmingSpot farmingspot, RoomUser user)
        {
            if (farmingspot != null && user != null)
            {
                try
                {
                    if (RoleplayManager.Distance(new Vector2D(farmingspot.x, farmingspot.y), new Vector2D(user.X, user.Y)) <= 1 && user.RoomId == farmingspot.roomid)
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

        public static FarmingSpot getFarmingSpotByItem(RoomItem item)
        {
            if (item != null)
            {
                try
                {
                    FarmingSpot theFarmingSpot = null;

                    foreach (FarmingSpot farmingspot in FarmingSpots.Values)
                    {
                        if (farmingspot.FarmingSpotItem == item)
                        {
                            return farmingspot;
                        }
                    }

                    return theFarmingSpot;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public static void updateFarmingSpot(FarmingSpot thefarmingspot, GameClient Session, Room room)
        {
            if (thefarmingspot != null && Session != null)
            {
                try
                {
                    var item = thefarmingspot.FarmingSpotItem;

                    #region Plant Type
                    uint thefarmingspottype = 0;
                    double thefarmingspotheight = 0;

                    if (thefarmingspot.type == "weed")
                    {
                        thefarmingspottype = 6699;
                        thefarmingspotheight = 0;
                    }
                    else if (thefarmingspot.type == "carrot")
                    {
                        thefarmingspottype = 2946;
                        thefarmingspotheight = 0.01;
                    }
                    else
                    {
                        thefarmingspottype = 6699;
                        thefarmingspotheight = 0;
                    }
                    #endregion

                    RoleplayManager.PlaceItemToCord(Session, thefarmingspottype, thefarmingspot.x, thefarmingspot.y, thefarmingspotheight, 0, false);
                }
                catch
                {
                }
            }
        }

        public static void removeFarmingSpot(FarmingSpot thefarmingspot, GameClient Session, Room room)
        {
            if (thefarmingspot != null && Session != null)
            {
                try
                {
                    FarmingSpot expiredFarmingSpot;
                    var item = thefarmingspot.FarmingSpotItem;

                    room.GetRoomItemHandler().RemoveFurniture(Session, item.Id, true);
                    var currentRoom = Plus.GetGame().GetRoomManager().GetRoom(thefarmingspot.roomid);
                    foreach (var Item in currentRoom.GetRoomItemHandler().FloorItems.Values)
                    {
                        if (Item.X == thefarmingspot.x && Item.Y == thefarmingspot.y && Item.RoomId == thefarmingspot.roomid && thefarmingspot.type == "carrot" && Item.BaseItem == 2946)
                        {
                            Misc.RoleplayManager.PickFarmingSpot(Item, thefarmingspot.roomid);
                        }
                        if (Item.X == thefarmingspot.x && Item.Y == thefarmingspot.y && Item.RoomId == thefarmingspot.roomid && thefarmingspot.type == "weed" && Item.BaseItem == 6699)
                        {
                            Misc.RoleplayManager.PickFarmingSpot(Item, thefarmingspot.roomid);
                        }
                    }

                    FarmingSpots.TryRemove(thefarmingspot.id, out expiredFarmingSpot);
                    expiredFarmingSpot = null;
                }
                catch
                {
                }
            }
        }

    }
}