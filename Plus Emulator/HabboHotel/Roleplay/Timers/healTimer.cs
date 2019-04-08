using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class healTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 10 seconds (milliseconds)

        public healTimer(GameClient Session)
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
                int PlusHealth = Rand.Next(5, 30);

                if (Session.GetRoleplay().CurHealth + PlusHealth >= Session.GetRoleplay().MaxHealth)
                {
                    Session.GetRoleplay().CurHealth = Session.GetRoleplay().MaxHealth;
                    Session.GetRoleplay().SaveQuickStat("curhealth", "" + Session.GetRoleplay().CurHealth);
                    Session.SendWhisper("You are fully healed!");
                    Session.GetRoleplay().BeingHealed = false;
                }
                else
                {
                    Session.GetRoleplay().CurHealth += PlusHealth;
                    Session.GetRoleplay().SaveQuickStat("curhealth", "" + Session.GetRoleplay().CurHealth);
                    Session.SendWhisper("Your health is now " + Session.GetRoleplay().CurHealth + "! Stay for more healing!");
                }
                if (Session.GetRoleplay().CurHealth != Session.GetRoleplay().MaxHealth && Session.GetRoleplay().BeingHealed)
                {
                    Session.GetRoleplay().healTimer = new healTimer(Session);
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
