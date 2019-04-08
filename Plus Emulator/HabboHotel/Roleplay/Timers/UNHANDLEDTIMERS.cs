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
    public class UNHANDLEDTIMERS
    {
        /// <summary>
        /// Timer for our operation
        /// </summary>
        private Timer Timer;

        public Room Room;
        public bool On = false;
        public GameClient Session;
        public string Username;

        /// <summary>
        /// Constructor
        /// </summary>
        public UNHANDLEDTIMERS(GameClient Session, string Username)
        {

            if (RoleplayData.Data["debug.show.stop.timer.msg"] == "true")
            {
                Console.WriteLine("Started timer for " + Username);
            }

            this.Session = Session;
            this.Username = Username;

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


                    RoomUser roomUser = Session.GetHabbo().GetRoomUser();


                    roleplayTimers.HandleTimer(Session, "rp", roomUser);

                    if (Session.GetRoleplay().WorkingOut)
                    {
                        roleplayTimers.HandleTimer(Session, "workout", roomUser);
                    }

                    if (Session.GetRoleplay().WeightLifting)
                    {
                        roleplayTimers.HandleTimer(Session, "weightlift", roomUser);
                    }

                    if (Session.GetRoleplay().ATMRobbery)
                    {
                        roleplayTimers.HandleTimer(Session, "atm_robbery", roomUser);
                    }

                    if (Session.GetRoleplay().Learning)
                    {
                        roleplayTimers.HandleTimer(Session, "learning", roomUser);
                    }

                    if (Session.GetRoleplay().Dead)
                    {
                        roleplayTimers.HandleTimer(Session, "dead", roomUser);
                    }

                    if (Session.GetRoleplay().Jailed)
                    {
                        roleplayTimers.HandleTimer(Session, "jail", roomUser);
                    }

                    if (Session.GetRoleplay().SentHome)
                    {
                        roleplayTimers.HandleTimer(Session, "sendhome", roomUser);
                    }

                    if (Session.GetRoleplay().BeingHealed)
                    {
                        roleplayTimers.HandleTimer(Session, "heal", roomUser);
                    }

                    if (Session.GetRoleplay().GangCapturing)
                    {
                        roleplayTimers.HandleTimer(Session, "gang_capture", roomUser);
                    }

                    if (Session.GetRoleplay().Withdraw_Via_Phone)
                    {
                        roleplayTimers.HandleTimer(Session, "quick", roomUser);
                    }

                    Timer.Change(1000, Timeout.Infinite);
                }


            }
            catch { }
        }

        public void stopTimer(string error)
        {
            if (RoleplayData.Data["debug.show.stop.timer.msg"] == "true")
            {
                Console.WriteLine("Destroyed timer for " + Username + " reason: " + error);
            }

            Timer.Dispose();
            return;
        }

    }
}
