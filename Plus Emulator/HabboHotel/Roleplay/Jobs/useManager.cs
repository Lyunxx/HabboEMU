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

namespace Plus.HabboHotel.Roleplay.Jobs
{
    internal class useManager
    {

        #region Methods


        public static bool removeInventoryBrotein(GameClient session, HybridDictionary invBrotein)
        {
            if (session != null)
            {

                try
                {
                    bool lol = false;
                    foreach (UserItem item in invBrotein.Values)
                    {
                        if (lol == false)
                        {
                            session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id, false);

                            using (var dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunFastQuery("DELETE FROM items_rooms WHERE id = '" + item.Id + "'");
                            }
                            lol = true;
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
        public static bool removeInventorykits(GameClient session, HybridDictionary invMedi)
        {
            if (session != null)
            {

                try
                {

                    bool lol1 = false;
                    foreach (UserItem item in invMedi.Values)
                    {
                        if (lol1 == false)
                        { 
                        session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id, false);

                        using (var dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunFastQuery("DELETE FROM items_rooms WHERE id = '" + item.Id + "'");
                        }
                            lol1 = true;
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

        public static bool removeInventoryDrinks(GameClient session, HybridDictionary invedrinks)
        {



            if (session != null)
            {

                try
                {
                    bool lol2 = false;
                    foreach (UserItem item in invedrinks.Values)
                    {
                        if (lol2 == false)
                        {
                            session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id, false);
                            using (var dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunFastQuery("DELETE FROM items_rooms WHERE id = '" + item.Id + "'");
                            }
                            lol2 = true;
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
        #endregion
    }

}