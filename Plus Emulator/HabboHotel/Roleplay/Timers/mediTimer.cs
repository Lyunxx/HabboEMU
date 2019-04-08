using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class mediTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 10 seconds (milliseconds)

        public mediTimer(GameClient Session)
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
                int PlusHealth = Rand.Next(20, 60);

                if (Session.GetRoleplay().CurHealth + PlusHealth >= Session.GetRoleplay().MaxHealth)
                {
                    Session.GetRoleplay().CurHealth = Session.GetRoleplay().MaxHealth;
                    Session.GetRoleplay().SaveQuickStat("curhealth", "" + Session.GetRoleplay().CurHealth);
                    Session.SendWhisper("You are fully healed!");
                    Misc.RoleplayManager.Shout(Session, "*[" + +Session.GetRoleplay().CurHealth + "/" + Session.GetRoleplay().MaxHealth + "]HP*", 6);
                    Session.GetRoleplay().UsingMedkit = false;
                }
                else
                {
                    Session.GetRoleplay().CurHealth += PlusHealth;
                    Session.GetRoleplay().SaveQuickStat("curhealth", "" + Session.GetRoleplay().CurHealth);
                    Misc.RoleplayManager.Shout(Session, "*[" + +Session.GetRoleplay().CurHealth + "/" + Session.GetRoleplay().MaxHealth  + "]HP*", 4);
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
