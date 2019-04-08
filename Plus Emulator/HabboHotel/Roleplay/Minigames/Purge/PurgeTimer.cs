using System;
using System.Threading;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Roleplay.Minigames.Purge
{
    /// <summary>
    /// This will do a countdown before the match starts
    /// </summary>
    public class PurgeTimer
    {
        /// <summary>
        /// Timer for our operation
        /// </summary>
        private Timer Timer;

        public bool On = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public PurgeTimer()
        {
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

                if (PurgeManager.Running)
                {
                        foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            try
                            {
                                if (client == null)
                                {
                                    continue;
                                }

                                client.SendWhisper("[Automatic Event Alert]: The hotel is currently under Purge Mode. All crime is legal.");

                            }
                            catch (Exception e)
                            {

                            }
                        }           

                    Timer.Change(30000, Timeout.Infinite);
                }
                else
                {

                    return;
                }
            }
            catch { }
        }
    }
}
