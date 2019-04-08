using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class broteinTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 10 seconds (milliseconds)

        public broteinTimer(GameClient Session)
        {
            this.Session = Session;
            Session.GetRoleplay().savedSTR = 5;
            int time = 300;
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
                    if (timeLeft == 20000)
                    {
                        Misc.RoleplayManager.Shout(Session, "*Brotein begins to wear off [-1 STR]*", 4);
                        Session.GetRoleplay().savedSTR -= 1;
                    }
                    else if (timeLeft == 12000)
                    {
                        Misc.RoleplayManager.Shout(Session, "*Adrenaline begins to fade away [-1 STR]*", 4);
                        Session.GetRoleplay().savedSTR -= 1;
                    }
                    else if (timeLeft == 6000)
                    {
                        Misc.RoleplayManager.Shout(Session, "*Brotein has 2 strength power left  [-1 STR]*", 4);
                        Session.GetRoleplay().savedSTR -= 1;
                    }
                    else if (timeLeft == 2000)
                    {
                        Misc.RoleplayManager.Shout(Session, "*Begins to go back to their puny self strength [-1 STR]*", 4);
                        Session.GetRoleplay().savedSTR -= 1;
                    }
                    timer.Change(1000, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute
                Session.GetRoleplay().savedSTR = 0;
                Session.GetRoleplay().StrBonus = false;
                int intTotalStr = Session.GetRoleplay().Strength + Session.GetRoleplay().savedSTR;
                Session.Shout("*Brotein Power wears off and gets their original strength of " + intTotalStr + " [-1 STR]*", 6, false);
                stopTimer();
                #endregion
            }
            catch { 
            stopTimer();
            
          }
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
