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

namespace Plus.HabboHotel.Roleplay.Components
{
    /// <summary>
    /// This will do a countdown before the match starts
    /// </summary>
    public class ProcessLogout
    {
        /// <summary>
        /// Timer for our operation
        /// </summary>
        private Timer Timer;
        public Room Room;
        public bool On = false;
        public GameClient Session;
        public int LogoutSeconds;
        public int LogoutSecondsElapsed;
        public bool LoggedOut;
        public bool SaidLogout;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessLogout(GameClient Session)
        {
            if (Session.LoggingOut)
                return;

            this.Session = Session;
            Session.LoggingOut = true;
            this.LogoutSeconds = Convert.ToInt32(RoleplayData.Data["roleplay.logout.seconds"]);
            this.LogoutSecondsElapsed = 0;
            this.LoggedOut = false;
            this.SaidLogout = false;

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
                if (Session == null)
                {
                    Logout(Session);
                    stopTimer("");
                    return;
                }

                if (Session.GetHabbo() == null)
                {
                    Logout(Session);
                    stopTimer("");
                    return;
                }

                if (!LoggedOut)
                {
                    if (SaidLogout)
                    {
                        if (LogoutSecondsElapsed < LogoutSeconds)
                        {
                            LogoutSecondsElapsed++;
                        }
                        else
                        {
                            LoggedOut = true;
                        }
                    }
                    else
                    {
                        if (Session != null)
                        {
                            if (Session.GetHabbo() != null)
                            {
                                if (Session.GetHabbo().CurrentRoom != null)
                                {
                                    Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(13);
                                    Session.Shout("*Desconectado - Desconectando em " + LogoutSeconds + " segundos*");
                                }
                            }
                        }
                        SaidLogout = true;
                    }
                }
                else
                {
                    Logout(Session);
                    stopTimer("");
                    return;
                }

                Timer.Change(1000, Timeout.Infinite);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString() + "|" + e.StackTrace);
            }
        }

        public void Logout(GameClient client)
        {
            try
            {
                if (client != null)
                {
                    client.LoggingOut = false;
                    client.Stop();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString() + "|" + e.StackTrace);
            }
        }

        public void stopTimer(string error)
        {
            Session = null;
            Timer.Dispose();
            return;
        }

    }
}
