using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Rooms.RoomInvokedItems;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.RoomBots;
using Plus.HabboHotel.Pets;
using Plus.Messages;
using Plus.Messages.Parsers;
using Plus.Database;
using System.Drawing;
using Plus.Util;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay;
using System.Text.RegularExpressions;
using System.Net;
using Plus.Database.Manager.Database.Session_Details.Interfaces;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class deadTimer
    {
        Timer timer;

        GameClient Session;
        int timeLeft; // 10 minutes (milliseconds)

        public deadTimer(GameClient Session)
        {
            this.Session = Session;

            int time = Session.GetRoleplay().DeadTimer;
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

                if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("HOSP"))
                {
                    return;
                }

                if (timeLeft > 0)
                {
                    if (!Session.GetRoleplay().Dead)
                    {
                        timeLeft = -1;
                    }

                    if(Session.GetRoleplay().DeadTimer <= 0)
                    {
                        timeLeft = -1;

                    }
                        int minutesRemaining = timeLeft / 60000;
                        Session.GetRoleplay().DeadTimer--;
                        Session.GetRoleplay().SaveStatusComponents("dead");
                        Session.SendWhisper("Você tem " + Session.GetRoleplay().DeadTimer + " minutos restantes para ser levado ao hospital.");
                        timer.Change(60000, Timeout.Infinite);
                        return;
                }
                #endregion
                
                #region Restore
                Session.GetRoleplay().DeadTimer = 0;
                RoomUser RoomUser = Session.GetHabbo().GetRoomUser();

                    if (RoomUser == null)
                        return;

                    RoomUser.Frozen = false;
                    RoleplayManager.Shout(Session, "*Regains their consciousness*", 0);
                    Session.GetRoleplay().DeadTimer = 0;
                    Session.GetRoleplay().Dead = false;
                    Session.GetRoleplay().SaveStatusComponents("dead");

                    if (Session.GetRoleplay().FigBeforeSpecial != null && Session.GetRoleplay().MottBeforeSpecial != null)
                    {
                        Session.GetHabbo().Look = Session.GetRoleplay().FigBeforeSpecial;
                        Session.GetHabbo().Motto = Session.GetRoleplay().MottBeforeSpecial;
                    }
                    else
                    {
                        DataRow User = null;

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT look,motto FROM users WHERE id = '" + Session.GetHabbo().Id + "'");
                            User = dbClient.GetRow();
                        }

                        Session.GetHabbo().Look = Convert.ToString(User["look"]);
                        Session.GetHabbo().Motto = Convert.ToString(User["motto"]);
                    }

                    Session.GetRoleplay().RefreshVals();
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
