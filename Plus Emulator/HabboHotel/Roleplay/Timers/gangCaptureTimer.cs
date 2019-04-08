using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Gangs;
using Plus.Database.Manager.Database.Session_Details.Interfaces;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class gangCaptureTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 5 minutes (milliseconds)

        public gangCaptureTimer(GameClient Session)
        {
            this.Session = Session;

            int time = 5;
            timeLeft = time * 60000;

            startTimer();
        }

        public void startTimer()
        {
            TimerCallback timerFinished = timerDone;

            timer = new Timer(timerFinished, null, 60000, Timeout.Infinite);
        }

        public void timerDone(object info)
        {
            try
            {
                timeLeft -= 60000;

                #region Conditions
                if (Session == null)
                { stopTimer(); return; }

                if (timeLeft == 3 * 60000)
                {
                    if (GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId > 0)
                    {
                        RoleplayManager.AlertGang("Your turf is slowly being captured in RoomID " + Session.GetHabbo().CurrentRoomId + "! They're getting close!", GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId);
                    }
                }
                else if (timeLeft == 1 * 60000)
                {
                    if (GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId > 0)
                    {
                        RoleplayManager.AlertGang("Your turf is nearly captured in RoomID " + Session.GetHabbo().CurrentRoomId + "! Hurry Hurry!", GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId);
                    }
                }

                if (timeLeft > 0)
                {
                    int minutesRemaining = timeLeft / 60000;
                    Session.SendWhisper("You have " + minutesRemaining + " minute(s) left until you capture the turf!");
                    timer.Change(60000, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute
                RoleplayManager.Shout(Session, "*Successfully captures gang turf*");
                GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId = Session.GetRoleplay().GangId;
                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery("UPDATE rp_gangs_turfs SET gang_id = '" + Session.GetRoleplay().GangId + "' WHERE turf_id = '" + Session.GetHabbo().CurrentRoomId + "'");
                }
                GangManager.initTurfs();

                if (GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId > 0)
                {
                    RoleplayManager.AlertGang("Your turf just got captured in RoomID " + Session.GetHabbo().CurrentRoomId + "! It's too late!", GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId);
                }

                Session.GetRoleplay().GangCapturing = false;
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
