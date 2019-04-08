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
    public class bankRobTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 10 minutes (milliseconds)

        public bankRobTimer(GameClient Session)
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
                    Session.GetRoleplay().SaveStatusComponents("robbery");
                    Session.SendWhisper("You have " + minutesRemaining + " minute(s) left until you complete your robbery!");
                    timer.Change(60000, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute
                Random rnd = new Random();
                int maxrob = (RoleplayManager.VaultRobbery < 666) ? (RoleplayManager.VaultRobbery / 2) : 666;
                int money = rnd.Next(0, maxrob);

                RoleplayManager.VaultRobbery -= money;

                if (RoleplayManager.VaultRobbery <= 0)
                {
                    Session.SendWhisper("The vault has $0 left, meaning it was completely robbed! Therefore, you didn't get any cash for robbing the vault!");
                    Session.GetRoleplay().Robbery = false;
                    Session.GetRoleplay().SaveStatusComponents("robbery");
                    stopTimer();
                    return;
                }

                RoleplayManager.Shout(Session, "*Finishes robbing the bank [+$" + money + "]*");
                RoleplayManager.GiveMoney(Session, +money);
                Session.SendWhisper("Vault cash remaining: $" + RoleplayManager.VaultRobbery + "!");
                Session.GetRoleplay().Robbery = false;
                Session.GetRoleplay().SaveStatusComponents("robbery");

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
