using System;
using System.Threading;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Roleplay;

namespace Plus.HabboHotel.Roleplay.Combat.WeaponExtras
{
    /// <summary>
    /// This will do a countdown before the match starts
    /// </summary>
    public class FrozenTimer
    {
        /// <summary>
        /// Timer for our operation
        /// </summary>
        private Timer Timer;

        public Room Room;
        public bool On = false;
        public GameClient Session;

        /// <summary>
        /// Constructor
        /// </summary>
        public FrozenTimer(GameClient Session)
        {

            this.Session = Session;

            // Method to call when completed
            TimerCallback TimerCallback = Ticked;

            // Create a new instance of timer
            Timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);
        }

        /// <summary>
        /// Method is call when timer is finished
        /// </summary>
        /// <param name="info">The information</param>
        public void Ticked(object info)
        {
            try
            {


                {

                    if (Session == null)
                    { stopTimer(); return; }

                    if (Session.GetRoleplay().RayFrozen)
                    {

                        if (Session.GetRoleplay().RayFrozenSeconds - 1 > 0)
                        {
                            Session.GetRoleplay().RayFrozenSeconds--;
                        }
                        else
                        {
                            //Unfreeze
                            Session.GetRoleplay().UnFreeze();
                        }

                    }
                    else
                    {
                        stopTimer();
                        return;
                    }

                    
                    Timer.Change(1000, Timeout.Infinite);
                }


            }
            catch { }
        }

        public void stopTimer()
        {
            //running = false;
            Timer.Dispose();
            return;
        }

    }
}
