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
    public class weedTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 2 minutes or 120 seconds (milliseconds)

        public weedTimer(GameClient Session)
        {
            this.Session = Session;

            int time = 120;
            timeLeft = time * 1000;

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
                timeLeft -= 1000;

                #region Conditions
                if (Session == null)
                { stopTimer(); return; }

                if (timeLeft == 90 * 1000)
                {
                    Session.SendWhisper("You have 90 seconds until your high runs out!");
                }
                else if (timeLeft == 60 * 1000)
                {
                    Session.SendWhisper("You have 60 seconds until your high runs out!");
                }
                else if (timeLeft == 30 * 1000)
                {
                    Session.SendWhisper("You have 30 seconds until your high runs out!");
                }
                else if (timeLeft == 10 * 1000)
                {
                    Session.SendWhisper("Your high is almost completely out!");
                }
                if (timeLeft > 0)
                {
                    timer.Change(1000, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute

                Session.GetRoleplay().UsingWeed = false;
                Session.GetRoleplay().UsingWeed_Bonus = 0;
                Session.SendWhisper("Your high ran out, your stomach feels full again");

                stopTimer();
                #endregion
            }
            catch { stopTimer(); }
        }

        public int getTime()
        {
            int minutesRemaining = timeLeft / 1000;
            return minutesRemaining;
        }

        public void stopTimer()
        {
            timer.Dispose();
        }
    }
}
