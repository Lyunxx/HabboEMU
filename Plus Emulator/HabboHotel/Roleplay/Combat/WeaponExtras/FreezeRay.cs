using System;
using System.Linq;
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
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Jobs;
using Plus.HabboHotel.Roleplay.Gangs;
using Plus.HabboHotel.Roleplay.Combat;
using Plus.HabboHotel.Roleplay.Apartments;
using Plus.HabboHotel.Roleplay;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Roleplay.Radio;
using System.Net;
using System.Diagnostics;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.Roleplay.Timers;
using Plus.HabboHotel.Roleplay.Combat.WeaponExtras;
using Plus.Configuration;
using Plus.Messages.Parsers;
using Plus.HabboHotel.Roleplay.Minigames.Purge;
using Plus.HabboHotel.Roleplay.Jobs.Space;
using Plus.HabboHotel.Roleplay.Jobs.Cutting;
using System.Collections.Specialized;
using Plus.HabboHotel.Roleplay.Minigames.Colour_Wars;

namespace Plus.HabboHotel.Roleplay.Combat.WeaponExtras
{
    /// <summary>
    /// This will do a countdown before the match starts
    /// </summary>
    public class FreezeRay
    {
        /// <summary>
        /// Timer for our operation
        /// </summary>
        private Timer Timer;

        /// <summary>
        /// Room for operation
        /// </summary>
        public Room Room;

        /// <summary>
        /// Starts and stops the timer
        /// </summary>
        public bool On = false;

        /// <summary>
        /// Prepares to stop the timer
        /// </summary>
        public bool PreparingMsg = false;

        /// <summary>
        /// The person using the freeze ray
        /// </summary>
        public GameClient Wielder;

       
        /// <summary>
        /// Freeze Ray Charging bool true, for yes, false for no
        /// </summary>
        public bool Charging = false;

        /// <summary>
        /// Minutes left til finished charging freeze ray
        /// </summary>
        public int ChargingMins = Convert.ToInt32(RoleplayData.Data["freeze.ray.default.chargetime"]);

        /// <summary>
        /// Seconds left til finished charging freeze ray
        /// </summary>
        public int ChargingSeconds = 60;

        /// <summary>
        /// Tells us wether the freeze ray is charged or not
        /// </summary>
        public bool Charged = false;

        /// <summary>
        /// Super blast release cooldown minutes left
        /// </summary>
        public int ReleaseCdMins = Convert.ToInt32(RoleplayData.Data["freeze.ray.default.cdreleasetime"]);

        /// <summary>
        /// Super blast release cooldown seconds left
        /// </summary>
        public int ReleaseCdSecs = 60;

        /// <summary>
        /// Tells us wether the user is currently using the super blast releaser
        /// </summary>
        public bool Releasing = false;

        /// <summary>
        /// Tells us the duration for the super blast releaser
        /// </summary>
        public int ReleasingSeconds = Convert.ToInt32(RoleplayData.Data["freeze.ray.default.releasetime"]);

        /// <summary>
        /// Constructor
        /// </summary>
        public FreezeRay(GameClient Wielder)
        {

            // Set default Release Mins to 0
            this.ReleaseCdMins = 0;

            // Set the person using the gun
            this.Wielder = Wielder;

            // Method to call when completed
            TimerCallback TimerCallback = Ticked;

            // Create a new instance of timer
            Timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);

        }

        /// <summary>
        /// Checks if the targeted user is within radius of being frozen
        /// </summary>
        /// <param name="SprayPoints">Freezable Points</param>
        /// <param name="User">User to check</param>
        /// <returns>true or false</returns>
        internal bool OnSprayPoint(List<Vector2D> SprayPoints, RoomUser User)
        {
            lock (SprayPoints)
            {
                foreach (Vector2D Point in SprayPoints)
                {
                    if (User.X == Point.X && User.Y == Point.Y)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieves The list of users who can be frozen near you
        /// </summary>
        /// <param name="surrounding">If it is true, it checks for people surrounding you, false for people within your line</param>
        /// <returns></returns>
        internal List<RoomUser> RetrieveFontalUsers(bool surrounding = false)
        {
            
            List<RoomUser> Frontal = new List<RoomUser>();
            List<Vector2D> SprayPoints = new List<Vector2D>();
            RoomUser Me = Wielder.GetHabbo().GetRoomUser();

            lock(Wielder.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
            {
                Vector2D fPoint = new Vector2D(Me.SquareInFront.X, Me.SquareInFront.Y);

                lock (SprayPoints)
                {
                    for (int i = 0; i < WeaponManager.WeaponsData[Wielder.GetRoleplay().Equiped].Range; i++)
                    {
                        SprayPoints.Add(new Vector2D(fPoint.X, fPoint.Y+i));
                    }

                    for (int i = 0; i > -WeaponManager.WeaponsData[Wielder.GetRoleplay().Equiped].Range; i--)
                    {
                        SprayPoints.Add(new Vector2D(fPoint.X + i, fPoint.Y));
                    }

                    foreach (RoomUser User in Wielder.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                    {

                        #region Checks
                        if (User == null)
                            continue;

                        if (User.GetClient() == null)
                            continue;

                        if (User.GetClient().GetHabbo() == null)
                            continue;

                        if (User.GetClient().GetHabbo().GetRoomUser() == null)
                            continue;

                        if (User.GetClient().GetRoleplay() == null)
                            continue;

                        if (User.GetClient().GetRoleplay().StaffDuty)
                            continue;

                        if (User == Me)
                            continue;

                        if (User.GetClient().GetRoleplay().RayFrozen)
                            continue;

                        #endregion

                        if (!surrounding)
                        {
                            if (OnSprayPoint(SprayPoints, User))
                            {
                                Frontal.Add(User);
                            }
                        }
                        else
                        {
                            if (RoleplayManager.Distance(new Vector2D(Me.X, Me.Y), new Vector2D(User.X, User.Y)) > WeaponManager.WeaponsData[Wielder.GetRoleplay().Equiped].Spread)
                                continue;

                            Frontal.Add(User);
                        }
                    }
                }
            }

            return Frontal;
        }

        /// <summary>
        /// Freezes all users upon freezeray deactivation
        /// </summary>
        internal void FreezeSurrounding()
        {
            lock (RetrieveFontalUsers(true))
            {
                foreach (RoomUser User in RetrieveFontalUsers(true))
                {
                    User.GetClient().SendWhisper("Você foi atingido por " + Wielder.GetHabbo().UserName + "'s freeze ray recuo explosão e foi congelado!");
                   
                    User.GetClient().GetRoleplay().Freeze(Wielder);
                }
            }
        }

        /// <summary>
        /// Handles freezing the users within range
        /// </summary>
        internal void ReleaseMassFreeze()
        {
            Random Rand = new Random();
            int RandInt = Rand.Next(1, 5);
            int str = Wielder.GetRoleplay().Strength + Wielder.GetRoleplay().savedSTR;
            int finalint = str - 3;
            if(finalint <= 0)
            {
                finalint = 3;
            }

            int FreezeTime = Rand.Next(2,finalint);

            if (Wielder == null)
                return;
            if (Wielder.GetHabbo() == null)
                return;
            if (Wielder.GetRoleplay() == null)
                return;
            if (Wielder.GetHabbo().CurrentRoom == null)
                return;
            if (Wielder.GetHabbo().CurrentRoom.GetRoomUserManager() == null)
                return;
            if(Wielder.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers() == null)
                return;
          
            lock(RetrieveFontalUsers())
            {
                foreach(RoomUser User in RetrieveFontalUsers())
                {
                    User.GetClient().SendWhisper("You were hit by " + Wielder.GetHabbo().UserName + "'s freeze ray blast and was frozen!");
                    User.GetClient().GetRoleplay().Freeze(Wielder);
                    User.GetClient().GetRoleplay().RayFrozen = true;
                }
            }
        }

        /// <summary>
        /// General Attack handler for freeze ray
        /// </summary>
        /// <param name="TargetSession"></param>
        internal void HandleGeneralAttack(GameClient TargetSession)
        {

            string freezingthem = "";

            Random Rand = new Random();
            int RandInt = Rand.Next(1, 5);
            int str = Wielder.GetRoleplay().Strength + Wielder.GetRoleplay().savedSTR;
            int finalint = str - 3;
            if(finalint <= 0)
            {
                finalint = 3;
            }

            int FreezeTime = Rand.Next(2,finalint);
            if(RandInt == 3)
            {
                TargetSession.GetRoleplay().Freeze(Wielder);
                freezingthem = ", freezing them!";
                Wielder.Shout("*Emits a glowing freeze blast towards " + TargetSession.GetHabbo().UserName + "'s direction " + freezingthem + "*");                   
            }
            else
            {
                Wielder.Shout("*Emits a glowing freeze blast towards " + TargetSession.GetHabbo().UserName + "'s direction but fails to hit them!*"); 
            }
            
        }

        /// <summary>
        /// Release a massive destructive freeze blast
        /// </summary>
        internal void ReleaseDestructiveFreeze()
        {

        }

        /// <summary>
        /// Callback for time elapsed (1 second)
        /// </summary>
        /// <param name="info"></param>
        public void Ticked(object info)
        {
            try
            {


                {

                    if (On)
                    {


                        if (RoleplayData.Data["freeze.debug.show.msgs"] == "true")
                        {
                            Wielder.SendWhisper("Values:::::: Freezecount " + RetrieveFontalUsers().Count + " || Releaseseconds: " + ReleasingSeconds + " || Releasing: " + Releasing.ToString() + " || Charging: " +Charging.ToString() + " || Charging Seconds: " + ChargingSeconds + " || Charging Mins: " + ChargingMins + " || ReleaseCooldown: " + ReleaseCdMins + " || releasecooldown secs" + ReleaseCdSecs);
                        }

                        #region Conditions

                        if (Wielder == null)
                        {
                            stopTimer();
                            return;
                        }
                        if (Wielder.GetHabbo() == null)
                        {
                            stopTimer();
                            return;
                        }
                        if (Wielder.GetRoleplay() == null)
                        { 
                            stopTimer();
                            return;
                        }
                       /* if (Wielder.GetHabbo().GetRoomUser() == null)
                            stopTimer();*/
                        if (Wielder.GetRoleplay().Dead)
                            On = false;
                        if (Wielder.GetRoleplay().Jailed)
                            On = false;
                        if (Wielder.GetRoleplay().WorkingOut)
                            On = false;
                        if (Wielder.GetRoleplay().WeightLifting)
                            On = false;
                        if (Wielder.GetRoleplay().Working)
                            On = false;
                        if (Wielder.GetRoleplay().Learning)
                            On = false;


                        #endregion

                        //Do stuff here

                        #region Release CD

                        if(ReleaseCdMins > 0)
                        {
                            if(ReleaseCdSecs -1 > 0)
                            {
                                ReleaseCdSecs--;
                            }
                            else
                            {
                                ReleaseCdMins--;
                                ReleaseCdSecs = 60;
                            }

                            Timer.Change(1000, Timeout.Infinite);
                            //return;
                        }

                        #endregion

                        #region Charging Super Blast Timer
                        if (Charging)
                        {
                            if (ChargingMins > 0 && !(RoleplayManager.BypassRights(Wielder)) )
                            {
                                if (ChargingSeconds - 1 > 0)
                                {
                                    ChargingSeconds--;
                                    if(ChargingSeconds < 6)
                                    {
                                        Wielder.SendWhisper(ChargingSeconds.ToString());
                                    }
                                }
                                else
                                {
                                    ChargingMins -= 1;
                                    if (ChargingMins > 0)
                                    {
                                        Wielder.SendWhisper("Your freeze ray blast will finish charging in " + ChargingMins + " minutes");
                                        ChargingSeconds = 60;
                                    }
                                }
                            }
                            else
                            {
                                //Stop charging
                                Wielder.SendWhisper("Your freeze ray mega blast is now ready to launch! Type :freezerelease to unleash it!");
                                Charged = true;
                                Charging = false;
                            }

                            Timer.Change(1000, Timeout.Infinite);
                            return;
                        }
                        #endregion

                        #region Releasing Super Blast Timer

                        if (Releasing && Charged)
                        {
                            
                            if (ReleasingSeconds > 0)
                            {
                                if (RoleplayData.Data["freeze.debug.show.msgs"] == "true")
                                {
                                    Wielder.SendWhisper("Freeze User Radius Count: " + RetrieveFontalUsers().Count + " || Releasing Secnds Left: " + ReleasingSeconds + "");
                                }
                                ReleaseMassFreeze();
                                ReleasingSeconds--;
                            }
                            else
                            {

                                //Stop releasing freeze blast
                                FreezeSurrounding();
                                Wielder.Shout("*Deactivates their freeze emitting a strong wave blasting everybody surrounding them*");
                                Releasing = false;
                                Charged = false;
                               ReleaseCdMins = Convert.ToInt32(RoleplayData.Data["freeze.ray.default.cdreleasetime"]);
                            }


                            Timer.Change(1000, Timeout.Infinite);
                            return;
                        }
                        #endregion


                    }
                    else
                    {
                        
                    }

                    if (!PreparingMsg)
                    {

                        Timer.Change(1000, Timeout.Infinite);
                    }
                }

                    
            }
            catch(Exception e) {

                if (RoleplayData.Data["freeze.debug.show.msgs"] == "true")
                {
                    Console.WriteLine("FreezeRay:::: " + e.ToString() + "||" + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Force stops the timer
        /// </summary>
        public void stopTimer()
        {
            if(RoleplayData.Data["freeze.debug.show.msgs"] == "true")
            {
                Console.WriteLine("The Timer has stopped!");
            }

            Timer.Dispose();
            PreparingMsg = true;
            return;
        }

    }
}
