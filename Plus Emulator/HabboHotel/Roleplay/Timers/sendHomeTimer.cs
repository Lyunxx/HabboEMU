using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class sendHomeTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // Based on whoever senthome the user (milliseconds)

        public sendHomeTimer(GameClient Session)
        {
            this.Session = Session;

            int time = Session.GetRoleplay().SendHomeTimer;
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
                    Session.GetRoleplay().SendHomeTimer = minutesRemaining;
                    Session.GetRoleplay().SaveQuickStat("sendhome_timer", "" + Session.GetRoleplay().SendHomeTimer);
                    Session.SendWhisper("You have " + minutesRemaining + " minute(s) left until you can get back to working!");
                    timer.Change(60000, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute

                Session.SendWhisper("You have completed your sendhome punishment. You can resume working now");
                Session.GetRoleplay().SendHomeTimer = 0;
                Session.GetRoleplay().SaveQuickStat("sendhome_timer", "" + Session.GetRoleplay().SendHomeTimer);
                Session.GetRoleplay().SentHome = false;

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
