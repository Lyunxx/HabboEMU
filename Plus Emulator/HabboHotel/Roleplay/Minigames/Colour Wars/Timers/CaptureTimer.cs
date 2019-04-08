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
    public class CaptureTimer
    {
        /// <summary>
        /// Timer for our operation
        /// </summary>
        private Timer Timer;

        /// <summary>
        /// 2nd Timer for our operation
        /// </summary>
        private Timer Timer2;

        /// <summary>
        /// The team being raided
        /// </summary>
        private Team Team;

        /// <summary>
        /// The user capturing the base
        /// </summary>
        private GameClient Me;

        /// <summary>
        /// Integer representing the time left of our operation
        /// </summary>
        private int TimeLeft = 180000; // 3 minutes

        /// <summary>
        /// Constructor
        /// </summary>
        public CaptureTimer(GameClient Me, Team Team)
        {
            this.Me = Me;
            this.Team = Team;

            // Method to call when completed
            TimerCallback TimerCallback = Ticked;

            // Create a new instance of timer
            Timer = new Timer(TimerCallback, null, 2000, Timeout.Infinite);

            // Method to call when completed
            TimerCallback TimerCallback2 = OneMinute;

            // Create a new instance of timer
            Timer2 = new Timer(TimerCallback2, null, 60000, Timeout.Infinite);
        }

        public void OneMinute(object info)
        {
            try
            {
                if (Me != null)
                {
                    // Get the roomuser
                    RoomUser RoomUser = Me.GetHabbo().GetRoomUser();
                    // Get the team
                    Team TheTeam = ColourManager.GetTeamByBase(RoomUser);

                    // Check if the user is not on the base point
                    if (TheTeam == null)
                    {
                        Team.BeingCaptured = false;

                        Timer = null;
                        Timer2 = null;
                        TheTeam = null;
                        Me = null;

                        return;
                    }

                    // If there is still more to do
                    if (TimeLeft > 0)
                    {
                        // Convert the milliseconds to minutes
                        int Minutes = TimeLeft / 60000;

                        if (Minutes == 0)
                        {
                            // Inform the user of time left
                            if(Me.GetRoleplay() != null)
                            {
                                Me.GetRoleplay().GiveColorWarPoints(ColourManager.CapturePoints);
                            }

                            RoleplayManager.Shout(Me, "*Successfully captures the " + Team.Colour.ToLower() + "'s base, knocking them out of the round [+ " + ColourManager.CapturePoints + " Color Wars Pts]*");
                            RoomUser.ApplyEffect(0);
                        }
                        else
                        {
                            RoleplayManager.Shout(Me, "*" + Minutes + " minutes until the base is captured!*");
                        }

                        // Restart the timer! :D
                        Timer2.Change(60000, Timeout.Infinite);
                        // Stop proceeding
                        return;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Method is call when timer is finished
        /// </summary>
        /// <param name="info">The information</param>
        public void Ticked(object info)
        {
            try
            {
                if (Me != null && Me.GetHabbo() != null && Me.GetRoleplay() != null)
                {
                    // Take away 2 seconds from the time left
                    TimeLeft -= 2000;

                    // Get the roomuser
                    RoomUser RoomUser = Me.GetHabbo().GetRoomUser();
                    // Get the team
                    Team TheTeam = ColourManager.GetTeamByBase(RoomUser);

                    // Check if the user is not on the base point
                    if (TheTeam == null || Team.KnockedOut)
                    {
                        RoleplayManager.Shout(Me, "*Stops capturing the base*");

                        Team.BeingCaptured = false;

                        Timer = null;
                        Timer2 = null;
                        TheTeam = null;
                        Me = null;

                        RoomUser.ApplyEffect(0);

                        return;
                    }

                    // If there is still more to do
                    if (TimeLeft > 0)
                    {
                        Timer.Change(2000, Timeout.Infinite);
                        // Stop proceeding
                        return;
                    }

                    Team.BeingCaptured = false;
                    RoomUser.ApplyEffect(0);
                    ColourManager.EliminateTeam(Team);
                }
                else
                {
                    Team.BeingCaptured = false;
                }
            }
            catch { Team.BeingCaptured = false; }
        }
    }
}
