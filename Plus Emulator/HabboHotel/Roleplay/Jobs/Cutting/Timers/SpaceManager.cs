#region Librarys

using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Data;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.Rooms;

#endregion

namespace Plus.HabboHotel.Roleplay.Jobs.Space
{
    internal class spaceManager
    {
        #region Declares

        public static ConcurrentDictionary<uint, Rock> Rocks = new ConcurrentDictionary<uint, Rock>();
        public static bool initiated = false;

        private static readonly int[] sellPoint = new int[3];

        #endregion

        #region Methods

        public static void initSpace()
        {
            Rocks.Clear();
            try
            {
                using (var dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM rp_space");
                    var spaceTable = dbClient.GetTable();
                    dbClient.SetQuery("SELECT * FROM rp_settings WHERE type = 'SPACE'");
                    var spaceSettings = dbClient.GetRow();

                    foreach (DataRow row in spaceTable.Rows)
                    {
                        var rock = new Rock(Convert.ToUInt32(row["id"]), Convert.ToUInt32(row["roomid"]),
                            Convert.ToInt32(row["x"]), Convert.ToInt32(row["y"]), row["data"].ToString());
                        Rocks.TryAdd(Convert.ToUInt32(row["id"]), rock);
                    }

                    var dataSettings = spaceSettings["data"].ToString().Split(':');

                    sellPoint[0] = Convert.ToInt32(dataSettings[0]); // X
                    sellPoint[1] = Convert.ToInt32(dataSettings[1]); // Y
                    sellPoint[2] = Convert.ToInt32(dataSettings[2]); // Room ID

                    oxygenTimer oxygenTimer = new oxygenTimer(Convert.ToUInt32(sellPoint[2]));
                    oxygenTimer.startTimer();
                }
            }
            catch
            {
            }
        }

        public static bool removeInventoryRocks(GameClient session, HybridDictionary invRocks)
        {
            if (session != null)
            {

                try
                {
                    foreach (UserItem item in invRocks.Values)
                    {
                        session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id, false);

                        using (var dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunFastQuery("DELETE FROM items_rooms WHERE id = '" + item.Id + "'");
                        }
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool isUserNearRock(Rock rock, RoomUser user)
        {
            if (rock != null && user != null)
            {
                try
                {
                    if (RoleplayManager.Distance(new Vector2D(rock.x, rock.y), new Vector2D(user.X, user.Y)) <= 1 && user.RoomId == rock.roomid)
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

        public static Rock getRockByItem(RoomItem item)
        {
            if (item != null)
            {
                try
                {
                    Rock theRock = null;

                    foreach (Rock rock in Rocks.Values)
                    {
                        if (rock.rockItem == item)
                        {
                            return rock;
                        }
                    }

                    return theRock;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public static void removeRock(Rock therock, GameClient Session, Room room)
        {
            if (therock != null && Session != null)
            {
                try
                {
                    Rock expiredRock;
                    var item = therock.rockItem;

                    room.GetRoomItemHandler().RemoveFurniture(Session, item.Id, true);
                    Session.GetHabbo()
                        .GetInventoryComponent()
                        .AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, 0, 0, "");
                    Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

                    Rocks.TryRemove(therock.id, out expiredRock);
                    expiredRock = null;
                }
                catch
                {
                }
            }
        }

        public static bool userOnSellPoint(RoomUser user)
        {
            try
            {
                if (user.X == sellPoint[0] && user.Y == sellPoint[1] && user.RoomId == Convert.ToUInt32(sellPoint[2]))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}