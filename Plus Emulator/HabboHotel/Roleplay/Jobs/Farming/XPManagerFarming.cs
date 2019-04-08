using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading;
using Plus.Connection.ServerManager;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Roleplay.Jobs.Farming
{
    public static class xpManagerFarming
    {
        public static readonly Dictionary<int, int> levels = new Dictionary<int, int>
        {
            {1, 0}, // Do not change this, feel free 2 change all the others
            {2, 10000},
            {3, 17500},
            {4, 30000},
        };

        public static void addXP(GameClient Session, int amount = 15)
        {
            try
            {
                if (Session != null)
                {
                    Session.GetRoleplay().FarmingXP += amount;
                    Session.GetRoleplay().UpdateStats = 10;

                    if (levelUp(Session))
                    {
                        Session.GetRoleplay().FarmingLevel += 1;
                        Session.GetRoleplay().UpdateStats = 10;
                        RoleplayManager.Shout(Session, "*Níveis até um nível " + Session.GetRoleplay().FarmingLevel + " agricultura!*");
                    }
                    else
                    {
                        Session.SendWhisper("Você recebeu XP por plantar! [" + Session.GetRoleplay().FarmingXP + " / " + levels[Session.GetRoleplay().FarmingLevel + 1] + "]");
                    }
                }
            }
            catch { }
        }

        public static bool levelUp(GameClient Session)
        {
            try
            {
                if (Session != null)
                {
                    int farmingXP = Session.GetRoleplay().FarmingXP;
                    int farmingLevel = Session.GetRoleplay().FarmingLevel + 1;

                    if (farmingLevel <= 5)
                    {
                        if (farmingXP >= levels[farmingLevel])
                        {
                            return true;
                        }

                    }
                    return false;
                }
                return false;
            }
            catch { return false; }   
        }
    }
}
