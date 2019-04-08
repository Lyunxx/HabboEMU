#region Librarys

using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading;
using Plus.Connection.ServerManager;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Rooms;

#endregion


namespace Plus.HabboHotel.Roleplay.Jobs.Cutting
{
    public static class xpManagerCutting
    {
        public static readonly Dictionary<int, int> levels = new Dictionary<int, int>
        {
            {1, 0}, // Do not change this, feel free 2 change all the others u fuckin fag
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
                    Session.GetRoleplay().WoodXP += amount;
                    Session.GetRoleplay().UpdateStats = 10;

                    if (levelUp(Session))
                    {
                        Session.GetRoleplay().WoodLevel += 1;
                        Session.GetRoleplay().UpdateStats = 10;
                        RoleplayManager.Shout(Session, "*Níveis até um nível " + Session.GetRoleplay().WoodLevel + " Lenhador * ");
                    }
                    else
                    {
                        Session.SendWhisper("Você recebeu XP por corte de madeira! [" + Session.GetRoleplay().WoodXP + " / " + levels[Session.GetRoleplay().WoodLevel + 1] + "]");
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
                    int woodXP = Session.GetRoleplay().WoodXP;
                    int woodLevel = Session.GetRoleplay().WoodLevel + 1;

                    if (woodLevel <= 5)
                    {
                        if (woodXP >= levels[woodLevel])
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
