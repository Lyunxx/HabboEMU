using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class nukeTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 5 minutes (milliseconds)

        public nukeTimer(GameClient Session)
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

                if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NPA_INSIDE"))
                {
                    RoleplayManager.Shout(Session, "*Stops the process in nuking the hotel*");
                    Session.GetRoleplay().NPA = false;
                    RoomItem Itemm = null;

                    foreach (RoomItem Item in Session.GetHabbo().CurrentRoom.GetRoomItemHandler().FloorItems.Values)
                    {
                        if (1 == 1)
                        {
                            Itemm = Item;
                        }
                    }

                    Itemm.OnNPAUsing = false;
                    stopTimer();
                    return;
                }
                if (timeLeft > 0)
                {
                    int minutesRemaining = timeLeft / 60000;
                    Session.SendWhisper("You have " + minutesRemaining + " minutes until you nuke the hotel!");
                    timer.Change(60000, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute

                Roleplay.Components.ProcessNuke nukeProcess = new Roleplay.Components.ProcessNuke(Session);

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
