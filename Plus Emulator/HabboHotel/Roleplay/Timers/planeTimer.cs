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
    public class planeTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 3 minutes (milliseconds)

        public planeTimer(GameClient Session)
        {
            this.Session = Session;

            int time = 1;
            timeLeft = time * 500;

            startTimer();
        }

        public void startTimer()
        {
            TimerCallback timerFinished = timerDone;

            timer = new Timer(timerFinished, null, 500, Timeout.Infinite);
        }

        public void timerDone(object info)
        {
            try
            {
                timeLeft -= 500;

                #region Conditions
                if (Session == null)
                { stopTimer(); return; }

                if (timeLeft > 0)
                {
                    if (!Session.GetRoleplay().usingPlane)
                    {
                        stopTimer();
                    }
                    timer.Change(500, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute

                if (Session.GetRoleplay().Fuel > 0)
                {
                    Session.GetRoleplay().Fuel--;
                }
                else
                {
                    Session.Shout("*My plane plummets to the ground due to no fuel left*");
                    Session.GetRoleplay().MultiCoolDown["plane_cooldown"] = 300;
                    Session.GetRoleplay().CheckingMultiCooldown = true;
                    Session.GetRoleplay().usingPlane = false;
                    Session.GetRoleplay().planeUsing = 0;
                    Session.GetRoleplay().SaveQuickStat("fuel", 0);
                    stopTimer();
                }
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
