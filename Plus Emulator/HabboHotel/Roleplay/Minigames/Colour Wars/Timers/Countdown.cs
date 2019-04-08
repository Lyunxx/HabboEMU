using System;
using System.Threading;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Roleplay.Minigames.Colour_Wars
{
    /// <summary>
    /// This will do a countdown before the match starts
    /// </summary>
    public class Countdown
    {
        /// <summary>
        /// Timer for our operation
        /// </summary>
        private Timer Timer;

        /// <summary>
        /// Integer representing the time left of our operation
        /// </summary>
        private int TimeLeft = 10000;

        /// <summary>
        /// Constructor
        /// </summary>
        public Countdown()
        {
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
                /*
                if (!ColourManager.LobbyLoop.PreparingMsg)
                {
                    ColourManager.MessageAllPlayers("A color wars game is preparing to start!");

                    ColourManager.LobbyLoop.PreparingMsg = true;
                    Timer.Change(5000, Timeout.Infinite);
                    return;
                }*/
                // Take away 1000 milliseconds from the time left
                TimeLeft -= 1000;

                // If there is still more to do
                if (TimeLeft > 0)
                {
                    // Convert the milliseconds to seconds
                    int Seconds = TimeLeft / 1000;

                    // Inform the user of time left
                    ColourManager.MessageAllPlayers("MATCH STARTING IN " + Seconds + " SECONDS!");

                    // Restart the timer! :D
                    Timer.Change(1000, Timeout.Infinite);
                    // Stop proceeding
                    return;
                }

                // Now start the game
                ColourManager.Start();
            }
            catch { }
        }
    }
}
