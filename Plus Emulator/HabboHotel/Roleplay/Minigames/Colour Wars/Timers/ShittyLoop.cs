using System;
using System.Threading;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Roleplay.Misc;

namespace Plus.HabboHotel.Roleplay.Minigames.Colour_Wars
{
    /// <summary>
    /// This will do a countdown before the match starts
    /// </summary>
    public class ShittyLoopForYing
    {
        /// <summary>
        /// Timer for our operation
        /// </summary>
        private Timer Timer;

        public Room Room;
        public bool On = false;
        public bool PreparingMsg = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public ShittyLoopForYing(Room Room)
        {
            this.Room = Room;
            // Method to call when completed
            TimerCallback TimerCallback = Ticked;

            // Create a new instance of timer
            Timer = new Timer(TimerCallback, null, 30000, Timeout.Infinite);
        }

        /// <summary>
        /// Method is call when timer is finished
        /// </summary>
        /// <param name="info">The information</param>
        public void Ticked(object info)
        {
            try
            {

                if (On)
                {
                    // If there is still more to do
                    if (ColourManager.Started == false)
                    {

                        // Inform the user of time left
                        messageCunts("[COLOR WARS] Pink team: " + ColourManager.Teams["pink"].Players.Count + "/6 | Blue team: " + ColourManager.Teams["blue"].Players.Count +
                            "/6 | Yellow team: " + ColourManager.Teams["yellow"].Players.Count + "/6 | Green team: " + ColourManager.Teams["green"].Players.Count + "/6");

                        try
                        {
                            // For every team in teams

                            // For every player in team.players
                            
                        }
                        catch (Exception ex) { }
                     
                    }
                    else
                    {
                        // lallaa
                        messageCunts("[COLOR WARS] There is currently a game in progress!");         
                    }

                    Timer.Change(30000, Timeout.Infinite);
                }
            }
            catch { }
        }

        public void messageCunts(string message)
        {
            Room FuckingRoom = RoleplayManager.GenerateRoom(ColourManager.GameLobby);
            lock (FuckingRoom.GetRoomUserManager().GetRoomUsers())
            {
                foreach (RoomUser User in FuckingRoom.GetRoomUserManager().GetRoomUsers())
                {
                    GameClient me = User.GetClient();
                    me.SendWhisper(message);
                }
            }
        }
    }
}
