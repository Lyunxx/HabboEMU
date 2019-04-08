using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class learningTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 10 minutes (milliseconds)

        public learningTimer(GameClient Session)
        {
            this.Session = Session;

            int time = 10;
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

                if (timeLeft > 0)
                {
                    int minutesRemaining = timeLeft / 60000;
                    Session.GetRoleplay().SaveStatusComponents("learning");
                    Session.SendWhisper("You have " + minutesRemaining + " minutes left until you learn a new subject.");
                    timer.Change(60000, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute

                Session.GetRoleplay().Intelligence += 1;
                Session.GetRoleplay().SaveQuickStat("intelligence", "" + Session.GetRoleplay().Intelligence);
                RoleplayManager.Shout(Session, "*Finishes learning [+1 Intelligence]*");
                Session.SendWhisper("Your intelligence is now: " + Session.GetRoleplay().Intelligence + "");
                Session.GetRoleplay().Learning = false;
                Session.GetRoleplay().SaveStatusComponents("learning");
                Session.GetHabbo().GetRoomUser().ApplyEffect(0);
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
