using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class massageTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 10 seconds (milliseconds)

        public massageTimer(GameClient Session)
        {
            this.Session = Session;

            int time = 5;
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

                if (timeLeft > 0)
                {
                    timer.Change(1000, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute
                Random Rand = new Random();
                int PlusEnergy = Rand.Next(5, 30);

                if (Session.GetRoleplay().Energy + PlusEnergy >= 100)
                {
                    Session.GetRoleplay().Energy = 100;
                    Session.GetRoleplay().SaveQuickStat("energy", "" + Session.GetRoleplay().Energy);
                    Session.SendWhisper("You are fully masaged!");
                    Session.GetRoleplay().BeingMassaged = false;
                }
                else
                {
                    Session.GetRoleplay().Energy += PlusEnergy;
                    Session.GetRoleplay().SaveQuickStat("energy", "" + Session.GetRoleplay().Energy);
                    Session.SendWhisper("Your energy is now " + Session.GetRoleplay().Energy + "! Stay for more massaging!");
                }
                if (Session.GetRoleplay().Energy != 100 && Session.GetRoleplay().BeingMassaged)
                {
                    Session.GetRoleplay().massageTimer = new massageTimer(Session);
                }
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