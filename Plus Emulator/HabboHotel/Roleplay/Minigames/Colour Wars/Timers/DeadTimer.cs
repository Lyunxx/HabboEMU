using System;
using System.Threading;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Roleplay.Minigames.Colour_Wars
{
    /// <summary>
    /// This will do a countdown before the match starts
    /// </summary>
    public class DeadTimer
    {
        /// <summary>
        /// Timer for our operation
        /// </summary>
        private Timer Timer;

        /// <summary>
        /// The session we are operating on
        /// </summary>
        private GameClient Me;

        /// <summary>
        /// Constructor
        /// </summary>
        public DeadTimer(GameClient Me)
        {
            // Set variables
            this.Me = Me;

            // Alert the user of what has happened
            Me.SendNotif("You were knocked out and have been transported to the graveyard, you cannot move for 1 minute!");

            // Get the room user
            RoomUser MyRoomUser = Me.GetHabbo().GetRoomUser();

            // Set the user as knocked out
            Me.GetRoleplay().KnockedOut = true;

            // Method to call when completed
            TimerCallback TimerCallback = Ticked;

            // Create a new instance of timer
            Timer = new Timer(TimerCallback, null, 60000, Timeout.Infinite);
        }

        /// <summary>
        /// Method is call when timer is finished
        /// </summary>
        /// <param name="info">The information</param>
        public void Ticked(object info)
        {
            try
            {
                // If the user exists
                if (Me != null && Me.GetRoleplay() != null)
                {
                    // Get the room user
                    RoomUser MyRoomUser = Me.GetHabbo().GetRoomUser();
                    if (MyRoomUser != null)
                    {
                        // Let the user walk again
                        MyRoomUser.CanWalk = true;
                        MyRoomUser.Frozen = false;
                        MyRoomUser.ApplyEffect(0);
                    }

                    // Set the user as alive
                    Me.GetRoleplay().KnockedOut = false;

                    // Tell the user they are free to move again
                    Me.SendWhisper("[COLOR WARS] You are now conscious and can move!");



                    // Dispose of me, I have served mi purpose!!
                    Timer = null;
                    Me = null;
                }
            }
            catch { }
        }
    }
}
