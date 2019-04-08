using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Roleplay.Misc
{
    class Bounties
    {
        public static ConcurrentDictionary<string, int> BountyUsers;

        public static void LoadBounty()
        {
            BountyUsers = new ConcurrentDictionary<string, int>();
            BountyUsers.Clear();
            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT user, amount FROM rp_bounties");

                foreach (DataRow Row in dbClient.GetTable().Rows)
                {
                    BountyUsers.TryAdd((string)Row["user"], (int)Row["amount"]);
                }
            }

        }

        public static void SaveBounty()
        {
            if (BountyUsers.Count > 0)
            {
                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery("TRUNCATE TABLE rp_bounties;");
                    foreach (string username in BountyUsers.Keys)
                    {
                        dbClient.RunFastQuery("INSERT INTO rp_bounties (user, amount) VALUES ('" + username + "', '" + BountyUsers[username] + "');");
                    }
                }
            }
        }

        public static int SetBounty(string username, int amount)
        {
            if (BountyUsers.ContainsKey(username))
            {
                BountyUsers[username] += amount;
            }
            else
            {
                BountyUsers.TryAdd(username, amount);
            }
            return BountyUsers[username];
        }

        public static void CheckBounty(GameClient user, string username)
        {
            if (BountyUsers.ContainsKey(username))
            {
                int junkBounty = BountyUsers[username];

                user.Shout("*Claims $" + BountyUsers[username] + " from " + username + "'s bounty*");
                user.SendWhisper("You have claimed $" + BountyUsers[username] + " from the bounty!");
                RoleplayManager.GiveMoney(user, +BountyUsers[username]);
                BountyUsers.TryRemove(username, out junkBounty);
            }
        }
    }
}
