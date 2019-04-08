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
    public class taxiTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // Depends on type of taxi (milliseconds)

        public taxiTimer(GameClient Session)
        {
            this.Session = Session;

            int time = Session.GetRoleplay().RequestedTaxi_WaitTime;
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
                    int secondsRemaining = timeLeft / 1000;
                    timer.Change(1000, Timeout.Infinite);
                    return;
                }

                if (!Session.GetHabbo().GetRoomUser().CanWalk)
                {
                    Session.SendWhisper("The taxi driver has cancelled your destination request due to the police catching you!");
                    stopTimer();
                    return;
                }

                #endregion

                #region Execute

                if (!Session.GetRoleplay().HideTaxiMsg)
                {
                    Session.Shout("*Entra no taxi e vai para seu destino*");
                }

                Session.GetMessageHandler().PrepareRoomForUser(Session.GetRoleplay().RequestedTaxiDestination.RoomId, "");
                Session.GetRoleplay().RequestedTaxi_Arrived = true;
                Session.GetRoleplay().HideTaxiMsg = false;

                stopTimer();
                #endregion
            }
            catch { stopTimer(); }
        }

        public int getTime()
        {
            int secondsRemaining = timeLeft / 1000;
            return secondsRemaining;
        }

        public void stopTimer()
        {
            timer.Dispose();
        }
    }
}
