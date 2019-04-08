using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.RoomInvokedItems;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.RoomBots;
using Plus.HabboHotel.Pets;
using Plus.Messages;
using System.Drawing;
using Plus.Util;
using System.Threading;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay;
using System.Net;

namespace Plus.HabboHotel.Roleplay.Timers
{
    /// <summary>
    /// This will do a countdown before the match starts
    /// </summary>
    public class ShitTimer
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
        public ShitTimer(GameClient Session)
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
                    { stopTimer("SESSION WAS NULL"); return; }


                    Timer.Change(1000, Timeout.Infinite);
                }


            }
            catch { }
        }

        public void stopTimer(string error)
        {

            Timer.Dispose();
            return;
        }

    }
}
