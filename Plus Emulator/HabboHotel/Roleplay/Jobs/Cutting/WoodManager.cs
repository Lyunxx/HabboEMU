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

namespace Plus.HabboHotel.Roleplay.Jobs.Cutting
{
    internal class woodManager
    {
        #region Declares

        public static ConcurrentDictionary<uint, Tree> Trees = new ConcurrentDictionary<uint, Tree>();
        public static bool initiated = false;

        private static readonly int[] sellPoint = new int[3];

        #endregion

        #region Methods

        public static void initTrees()
        {
            Trees.Clear();
            try
            {
                using (var dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM rp_trees");
                    var woodTable = dbClient.GetTable();
                    dbClient.SetQuery("SELECT * FROM rp_settings WHERE type = 'TREES'");
                    var woodSettings = dbClient.GetRow();

                    foreach (DataRow row in woodTable.Rows)
                    {
                        var rock = new Tree(Convert.ToUInt32(row["id"]), Convert.ToUInt32(row["roomid"]),
                            Convert.ToInt32(row["x"]), Convert.ToInt32(row["y"]), row["data"].ToString());
                        Trees.TryAdd(Convert.ToUInt32(row["id"]), rock);
                    }

                    var dataSettings = woodSettings["data"].ToString().Split(':');

                    sellPoint[0] = Convert.ToInt32(dataSettings[0]); // X
                    sellPoint[1] = Convert.ToInt32(dataSettings[1]); // Y
                    sellPoint[2] = Convert.ToInt32(dataSettings[2]); // Room ID

                }
            }
            catch
            {
            }
        }

        public static bool removeInventoryTrees(GameClient session, HybridDictionary invTrees)
        {
            if (session != null)
            {

                try
                {
                    foreach (UserItem item in invTrees.Values)
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

        public static bool isUserNearTree(Tree tree, RoomUser user)
        {
            if (tree != null && user != null)
            {
                try
                {
                    if (RoleplayManager.Distance(new Vector2D(tree.x, tree.y), new Vector2D(user.X, user.Y)) <= 2 && user.RoomId == tree.roomid)
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

        public static Tree getTreeByItem(RoomItem item)
        {
            if (item != null)
            {
                try
                {
                    Tree theTree = null;

                    foreach (Tree tree in Trees.Values)
                    {
                        if (tree.TreeItem == item)
                        {
                            return tree;
                        }
                    }

                    return theTree;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public static void removeTree(Tree thetree, GameClient Session, Room room)
        {
            if (thetree != null && Session != null)
            {
                try
                {
                    Tree expiredRock;
                    var item = thetree.TreeItem;
                    room.GetRoomItemHandler().RemoveFurniture(Session, item.Id, true);
                    Session.GetHabbo()
                        .GetInventoryComponent()
                        .AddNewItem(item.Id, 2694, "", 0, true, true, 0, 0, "");
                    Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

                    Trees.TryRemove(thetree.id, out expiredRock);
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