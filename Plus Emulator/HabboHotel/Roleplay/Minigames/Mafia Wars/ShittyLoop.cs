using System;
using System.Threading;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Roleplay.Misc;

namespace Plus.HabboHotel.Roleplay.Minigames.Mafia_Wars
{
    /// <summary>
    /// This will do a countdown before the match starts
    /// </summary>
    public class MWLoop
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
        public MWLoop(Room Room)
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
                    if (!Plus.GetGame().MafiaWars.Started)
                    {

                        // Inform the user of time left
                        messageCunts("[MAFIA WARS] Green team: " + Plus.GetGame().MafiaWars.MWTeams["Green"].Players.Count + "/10 | Blue team: " + Plus.GetGame().MafiaWars.MWTeams["Blue"].Players.Count + "/10");

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
                        messageCunts("[MAFIA WARS] There is currently a game in progress!");         
                    }

                    Timer.Change(30000, Timeout.Infinite);
                }
            }
            catch { }
        }

        public void messageCunts(string message)
        {
            Room FuckingRoom = Room;
            lock (FuckingRoom.GetRoomUserManager().GetRoomUsers())
            {
                foreach (RoomUser User in FuckingRoom.GetRoomUserManager().GetRoomUsers())
                {
                    if (User == null)
                        continue;

                    GameClient me = User.GetClient();
                    me.SendWhisper(message);
                }
            }
        }
    }
}
