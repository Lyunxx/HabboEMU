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
    public class hygieneTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 3 minutes (milliseconds)

        public hygieneTimer(GameClient Session)
        {
            this.Session = Session;

            int time = 5;
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
                    if (Session.GetRoleplay().Hygiene <= 10 && Session.GetHabbo().GetRoomUser().CurrentEffect <= 0)
                    {
                        Session.GetHabbo().GetRoomUser().ApplyEffect(10);
                    }
                    else if (Session.GetHabbo().GetRoomUser().CurrentEffect == 10 && Session.GetRoleplay().Hygiene > 10)
                    {
                        Session.GetHabbo().GetRoomUser().ApplyEffect(0);
                    }

                    timer.Change(60000, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute

                if (Session.GetRoleplay().Hygiene > 0)
                {
                    if (Session.GetRoleplay().Hygiene - 2 >= 0)
                    {
                        Session.GetRoleplay().Hygiene -= 2;
                        Session.GetRoleplay().UpdateStats++;
                    }
                    else
                    {
                        Session.GetRoleplay().Hygiene = 0;
                    }
                }
                Session.GetRoleplay().SaveQuickStat("hygiene", "" + Session.GetRoleplay().Hygiene);

                stopTimer();

                Session.GetRoleplay().HygieneDecrement = false;
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