using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Roleplay.Components
{
    public class ProcessNuke
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 5 minutes (milliseconds)

        public ProcessNuke(GameClient Session)
        {
            this.Session = Session;

            int time = 10;
            timeLeft = time * 60000;

            startTimer();
        }

        public void startTimer()
        {
            TimerCallback timerFinished = timerDone;

            timer = new Timer(timerFinished, null, 1000, Timeout.Infinite);
        }

        public void timerDone(object info)
        {
            try
            {
                timeLeft -= 60000;

                #region Conditions
                if (Session == null)
                { stopTimer(); return; }

                
                if (timeLeft > 0)
                {
                    int minutesRemaining = timeLeft / 60000;

                    lock (Plus.GetGame().GetClientManager().Clients.Values)
                    {
                        foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                        {

                            if (client == null)
                                continue;
                            if (client.GetHabbo() == null)
                                continue;
                            if (client.GetHabbo().CurrentRoom == null)
                                continue;
                            if (client.GetConnection() == null)
                                continue;

                            client.SendWhisper("The nuke is about to go off, hurry to the hospital! (" + minutesRemaining + " seconds)!");
                        }

                        Session.SendWhisper("The nuke will go off in: (" + minutesRemaining + " seconds)!");
                    }

                    timer.Change(1000, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute

                int kills = 0;

                RoleplayManager.Shout(Session, "*Successfully breaks into the nuke system, causing the whole hotel to nuke*");

                lock (Plus.GetGame().GetClientManager().Clients.Values)
                {
                    foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                    {

                        if (client == null)
                            continue;
                        if (client.GetHabbo() == null)
                            continue;
                        if (client.GetHabbo().CurrentRoom == null)
                            continue;
                        if (client.GetConnection() == null)
                            continue;

                        if (RoleplayManager.BypassRights(client))
                            continue;
                        if (client.GetRoleplay().inColourWars)
                            continue;
                        if (client.GetRoleplay().InMafiaWars)
                            continue;
                        if (client.GetRoleplay().inBrawl)
                            continue;

                        if (client.GetHabbo().CurrentRoom.RoomData.Description.Contains("NPA_INSIDE"))
                        {
                            client.SendWhisper("The nuke exploded, but you found refuge in the NPA room! [+50 Points]");
                        }
                        else if (client.GetHabbo().CurrentRoom.RoomData.Description.Contains("HOSP"))
                        {
                            client.SendWhisper("The nuke exploded, but you found refuge in the hospital! [+50 Points]");
                        }
                        else if (client.GetHabbo().CurrentRoom.RoomData.Description.Contains("JAIL"))
                        {
                            client.SendWhisper("The nuke exploded, but you found refuge in prison! [+50 Points]");
                        }
                        else
                        {
                            kills++;

                            client.GetRoleplay().DeadFigSet = false;
                            client.GetRoleplay().DeadSeconds = 60;
                            client.GetRoleplay().DeadTimer = 5;
                            client.GetRoleplay().Dead = true;
                            client.GetRoleplay().SaveStatusComponents("dead");
                            RoleplayManager.HandleDeath(client);
                            client.SendNotif("The nuke exploded, the radioactivity killed you!");
                        }
                    }

                    Session.GetRoleplay().Kills += kills;
                    Session.GetRoleplay().UpdateStats++;

                    Session.GetRoleplay().NPA = false;
                    RoomItem Itemm = null;

                    foreach (RoomItem Item in Session.GetHabbo().CurrentRoom.GetRoomItemHandler().FloorItems.Values)
                    {
                        if (1 == 1)
                        {
                            Itemm = Item;
                        }
                    }

                    Itemm.OnNPAUsing = false;
                }

                stopTimer();
                #endregion
            }
            catch { stopTimer(); }
        }

        public int getTime()
        {
            int minutesRemaining = timeLeft / 60000;
            return minutesRemaining;
        }

        public void stopTimer()
        {
            timer.Dispose();
        }
    }
}
