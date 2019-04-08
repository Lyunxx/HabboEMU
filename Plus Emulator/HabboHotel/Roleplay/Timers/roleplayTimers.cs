using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.Misc;
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
using Plus.HabboHotel.Roleplay.Jobs;
using Plus.HabboHotel.Roleplay.Gangs;
using System.Net;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.Configuration;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class roleplayTimers
    {
        public static void HandleTimer(GameClient Session, string Timer, RoomUser RUser = null)
        {
            try {


                if (RUser == null)
                    return;

                /*if (Session != null)
                {

                    if (Session.GetConnection() != null)
                    {

                        if (Session.GetRoleplay() != null)
                        {

                            if (RUser != null)
                            {*/

                                switch (Timer)
                                {

                                    #region Main Timers (4) 

                                    #region Dead Timer
                                    case "dead":
                                        {
                                            if (Session.GetRoleplay().Dead)
                                            {
                                                RoomUser RoomUser = null;
                                                RoomUser = RUser;

                                                if (RoomUser != null)
                                                {
                                                    if (!RoomUser.Frozen)
                                                    {
                                                        RoomUser.Frozen = true;
                                                    }
                                                }

                                                if (Session.GetRoleplay().DeadTimer > 0)
                                                {
                                                    if (Session.GetRoleplay().DeadSeconds > 0)
                                                    {
                                                        Session.GetRoleplay().DeadSeconds--;
                                                    }
                                                    else
                                                    {
                                                        Session.GetRoleplay().DeadTimer--;
                                                        Session.GetRoleplay().SaveStatusComponents("dead");
                                                        if (Session.GetRoleplay().DeadTimer > 0)
                                                        {
                                                            Session.SendWhisper("You have " + Session.GetRoleplay().DeadTimer + " minutes left until you are discharged from the hospital.");
                                                        }
                                                        Session.GetRoleplay().DeadSeconds = 60;
                                                    }
                                                }
                                                else
                                                {
                                                    //Discharge from hospital
                                                    RoomUser.SetPos(9, 26, 0);
                                                    RoomUser.ClearMovement();
                                                    RoomUser.Frozen = false;
                                                    RoleplayManager.Shout(Session, "*Volta a consiencia*", 0);
                                                    Session.GetRoleplay().DeadTimer = 0;
                                                    Session.GetRoleplay().Dead = false;
                                                    Session.GetRoleplay().SaveStatusComponents("dead");

                                                    if (Session.GetRoleplay().FigBeforeSpecial != null && Session.GetRoleplay().MottBeforeSpecial != null)
                                                    {
                                                        Session.GetHabbo().Look = Session.GetRoleplay().FigBeforeSpecial;
                                                        Session.GetHabbo().Motto = Session.GetRoleplay().MottBeforeSpecial;
                                                    }
                                                    else
                                                    {
                                                        DataRow User = null;

                                                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                                                        {
                                                            dbClient.SetQuery("SELECT look,motto FROM users WHERE id = '" + Session.GetHabbo().Id + "'");
                                                            User = dbClient.GetRow();
                                                        }

                                                        Session.GetHabbo().Look = Convert.ToString(User["look"]);
                                                        Session.GetHabbo().Motto = Convert.ToString(User["motto"]);
                                                    }

                                                    Session.GetRoleplay().RefreshVals();
                                                }
                                            }

                                            break;
                                        }
                                    #endregion

                                    #region Jailed Timer
                                    case "jail":
                                        {
                                            if (Session.GetRoleplay().Jailed)
                                            {
                                                if (RUser.Frozen)
                                                {
                                                    RUser.Frozen = false;
                                                }
                                                if (Session.GetRoleplay().JailTimer > 0)
                                                {
                                                    if (Session.GetRoleplay().JailedSeconds > 0)
                                                    {
                                                        Session.GetRoleplay().JailedSeconds--;
                                                    }
                                                    else
                                                    {
                                                        Session.GetRoleplay().JailTimer--;
                                                        Session.GetRoleplay().SaveStatusComponents("jailed");
                                                        if (Session.GetRoleplay().JailTimer > 0)
                                                        {
                                                            Session.SendWhisper("Você tem " + Session.GetRoleplay().JailTimer + " minutos restantes para sair da prisão");
                                                        }
                                                        Session.GetRoleplay().JailedSeconds = 60;
                                                    }
                                                }
                                                else
                                                {
                                                    //Released from jail
                                                    RUser.SetPos(23, 16, 0.2);
                                                    RUser.ClearMovement();
                                                    RoleplayManager.Shout(Session, "*Cumpre sua pena*", 0);
                                                    Session.GetRoleplay().JailTimer = 0;
                                                    Session.GetRoleplay().Jailed = false;
                                                    Session.GetRoleplay().SaveStatusComponents("jailed");
                                                    if (Session.GetRoleplay().FigBeforeSpecial.Length != null && Session.GetRoleplay().MottBeforeSpecial != null)
                                                    {
                                                        Session.GetHabbo().Look = Session.GetRoleplay().FigBeforeSpecial;
                                                        Session.GetHabbo().Motto = Session.GetRoleplay().MottBeforeSpecial;
                                                    }
                                                    else
                                                    {
                                                        DataRow User = null;

                                                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                                                        {
                                                            dbClient.SetQuery("SELECT look,motto FROM users WHERE id = '" + Session.GetHabbo().Id + "'");
                                                            User = dbClient.GetRow();
                                                        }

                                                        Session.GetHabbo().Look = Convert.ToString(User["look"]);
                                                        Session.GetHabbo().Motto = Convert.ToString(User["motto"]);
                                                    }
                                                    Session.GetRoleplay().RefreshVals();
                                                }
                                            }

                                            break;
                                        }
                                    #endregion

                                    #region WorkingOut Timer
                                    case "workout":
                                        {
                                            if (Session.GetRoleplay().WorkingOut)
                                            {
                                                bool StoppedWorkingOut = false;

                                                if (Session.GetRoleplay().RequestedTaxi || !Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("GYM") || !Session.GetRoleplay().NearItem("sf_roller", 0))
                                                {
                                                    Session.GetRoleplay().WorkingOut = false;
                                                    RoleplayManager.Shout(Session, "*Para de trabalhar*");
                                                    StoppedWorkingOut = true;
                                                }

                                                if (RUser.IsAsleep)
                                                {
                                                    Session.GetRoleplay().WorkingOut = false;
                                                    RoleplayManager.Shout(Session, "*Para de trabalhar [AFK]*");
                                                    StoppedWorkingOut = true;
                                                }

                                                if (!StoppedWorkingOut)
                                                {
                                                    if (Session.GetRoleplay().WorkoutTimer_Done < Session.GetRoleplay().WorkoutTimer_ToDo)
                                                    {
                                                        if (Session.GetRoleplay().WorkoutSeconds > 0)
                                                        {
                                                            Session.GetRoleplay().WorkoutSeconds--;
                                                        }
                                                        else
                                                        {
                                                            //Finished 1min
                                                            if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("GYM") || !Session.GetRoleplay().NearItem("sf_roller", 0))
                                                            {
                                                                RoleplayManager.Shout(Session, "*Para de trabalhar*");
                                                                StoppedWorkingOut = true;
                                                            }
                                                            else
                                                            {
                                                                Session.GetRoleplay().WorkoutTimer_Done++;
                                                                Session.GetRoleplay().SaveQuickStat("workout_cur_timer", "" + Session.GetRoleplay().WorkoutTimer_Done);
                                                                Session.SendWhisper("Working Out: " + Session.GetRoleplay().WorkoutTimer_Done + "/" + Session.GetRoleplay().WorkoutTimer_ToDo);
                                                                Session.GetRoleplay().WorkoutSeconds = 40;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //Increment strength by one
                                                        if (Session.GetRoleplay().Strength >= 20)
                                                        {
                                                            Session.SendWhisper("You cannot go past level 20.");
                                                            Session.GetRoleplay().WorkingOut = false;
                                                            RoleplayManager.Shout(Session, "*Stops working out*");
                                                            StoppedWorkingOut = true;
                                                        }
                                                        else
                                                        {
                                                            Session.GetRoleplay().Strength += 1;
                                                            Session.GetRoleplay().SaveQuickStat("strength", "" + Session.GetRoleplay().Strength);
                                                            Session.GetRoleplay().CalculateWorkoutTimer();
                                                            RoleplayManager.Shout(Session, "*Feels stronger*");
                                                            Session.SendWhisper("You have earned +1 Strength. Your strength is now: " + Session.GetRoleplay().Strength + " with the bonus of: " + Session.GetRoleplay().savedSTR);
                                                            Session.GetRoleplay().WorkoutSeconds = 40;
                                                        }
                                                    }
                                                }
                                            }

                                            break;
                                        }
                    #endregion

                                    #region WeightLifting Timer
                    case "weightlift":
                        {
                            if (Session.GetRoleplay().WeightLifting)
                            {
                                bool StoppedWeightLifting = false;

                                if (Session == null)
                                    return;

                                if (Session.GetRoleplay() == null)
                                    return;

                                if (Session.GetHabbo() == null)
                                    return;

                                if (Session.GetHabbo().CurrentRoom == null)
                                    return;

                                if (Session.GetHabbo().CurrentRoom.RoomData == null)
                                    return;

                                if (Session.GetRoleplay().RequestedTaxi || !Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("GYM") || !Session.GetRoleplay().NearItem("uni_wobench", 0))
                                {
                                    Session.GetRoleplay().WeightLifting = false;
                                    RoleplayManager.Shout(Session, "*Stops lifting weights*");
                                    StoppedWeightLifting = true;
                                }

                                if (RUser.IsAsleep)
                                {
                                    Session.GetRoleplay().WeightLifting = false;
                                    RoleplayManager.Shout(Session, "*Stops lifting weights [AFK]*");
                                    StoppedWeightLifting = true;
                                }

                                if (!StoppedWeightLifting)
                                {
                                    if (Session.GetRoleplay().WeightLiftTimer_Done < Session.GetRoleplay().WeightLiftTimer_ToDo)
                                    {
                                        if (Session.GetRoleplay().WeightLiftSeconds > 0)
                                        {
                                            Session.GetRoleplay().WeightLiftSeconds--;
                                        }
                                        else
                                        {
                                            //Finished 1min
                                            if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("GYM") || !Session.GetRoleplay().NearItem("uni_wobench", 0))
                                            {
                                                RoleplayManager.Shout(Session, "*Stops lifting weights*");
                                                StoppedWeightLifting = true;
                                            }
                                            else
                                            {
                                                Session.GetRoleplay().WeightLiftTimer_Done++;
                                                Session.GetRoleplay().SaveQuickStat("weightlift_cur_timer", "" + Session.GetRoleplay().WeightLiftTimer_Done);
                                                Session.SendWhisper("Lifting Weights: " + Session.GetRoleplay().WeightLiftTimer_Done + "/" + Session.GetRoleplay().WeightLiftTimer_ToDo);
                                                Session.GetRoleplay().WeightLiftSeconds = 40;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Increment constitution by one and maxhp by 10
                                        if (Session.GetRoleplay().Constitution >= 10)
                                        {
                                            Session.SendWhisper("You cannot go past level 10.");
                                            Session.GetRoleplay().WeightLifting = false;
                                            RoleplayManager.Shout(Session, "*Stops lifting weights*");
                                            StoppedWeightLifting = true;
                                        }
                                        else
                                        {
                                            Session.GetRoleplay().Constitution += 1;
                                            Session.GetRoleplay().SaveQuickStat("constitution", "" + Session.GetRoleplay().Constitution);
                                            Session.GetRoleplay().MaxHealth += 10;
                                            Session.GetRoleplay().SaveQuickStat("maxhealth", "" + Session.GetRoleplay().MaxHealth);
                                            Session.GetRoleplay().CalculateWeightLiftTimer();
                                            RoleplayManager.Shout(Session, "*Feels healthier*");
                                            Session.SendWhisper("You have earned +1 Constitution. Your constitution is now: " + Session.GetRoleplay().Constitution);
                                            Session.SendWhisper("You have earned +10 Max Health. Your max health is now: " + Session.GetRoleplay().MaxHealth);
                                            Session.GetRoleplay().WeightLiftSeconds = 40;
                                        }
                                    }
                                }
                            }

                            break;
                        }
                    #endregion


                                    #endregion

                                    #region Misc Timers (2)

                                    #region Crafting (DISABLED)

                                    #endregion

                                    #region Gathering (DISABLED)
                                   /* case "gather":
                                        {
                                            if (Session.GetRoleplay().Gathering)
                                            {
                                                bool Continue_Gather = true;

                                                if (Session.GetRoleplay().GatheringItem == null)
                                                {
                                                    Misc.Shout(Session, "*Stops gathering*");
                                                    Continue_Gather = false;
                                                    return;
                                                }

                                                if (Continue_Gather)
                                                {

                                                    if (!Session.GetRoleplay().NearItem(Session.GetRoleplay().GatheringItem.GetBaseItem().Name, 1))
                                                    {
                                                        Misc.Shout(Session, "*Stops gathering*");
                                                        Continue_Gather = false;
                                                        return;
                                                    }

                                                    if (Session.GetRoleplay().GatheringSeconds > 0)
                                                    {
                                                        Session.GetRoleplay().GatheringSeconds--;
                                                        RUser.ApplyEffect(8);
                                                    }
                                                    else
                                                    {
                                                        Session.GetRoleplay().Gathering = false;
                                                        RUser.ApplyEffect(0);
                                                    }

                                                }
                                                else
                                                    Session.GetRoleplay().Gathering = false;
                                            }

                                            break;
                                        }*/
                                    #endregion
                                        
                                    #endregion
                                        
                                    #region Continuous Timers (9)
                                    case "rp":
                                        {

                                            #region Hygiene Decrement (DISABLED)

                                            #region Inside Shower

                                          /*  if (Session.GetRoleplay().InShower)
                                            {
                                                if (Session.GetRoleplay().ShowerSeconds <= 0)
                                                {
                                                    if (Session.GetRoleplay().Hygiene + 2 <= 100)
                                                    {
                                                        Session.GetRoleplay().Hygiene += 5;
                                                    }
                                                    else
                                                    {
                                                        Session.GetRoleplay().Hygiene = 100;
                                                        Session.GetRoleplay().InShower = false;
                                                        RUser.MoveTo(RUser.X, RUser.Y - 1);
                                                        Session.SendWhisper("You are now fully clean");
                                                        Session.Shout("*Stops bathing*");
                                                        Session.GetRoleplay().Shower.ExtraData = "0";
                                                        Session.GetRoleplay().Shower.UpdateNeeded = true;
                                                        Session.GetRoleplay().Shower.UpdateState();
                                                        Session.GetRoleplay().InShower = false;
                                                        return;
                                                    }
                                                    Session.GetRoleplay().ShowerSeconds = 1;
                                                }
                                                else
                                                {
                                                    //   Console.WriteLine(Session.GetRoleplay().ShowerSeconds.ToString());
                                                    Session.GetRoleplay().ShowerSeconds--;

                                                    if (Misc.Distance(new Vector2D(Session.GetRoleplay().Shower.X, Session.GetRoleplay().Shower.Y), new Vector2D(RUser.X, RUser.Y)) >= 1)
                                                    {
                                                        Session.Shout("*Stops bathing*");
                                                        Session.GetRoleplay().Shower.ExtraData = "";
                                                        Session.GetRoleplay().Shower.UpdateNeeded = true;
                                                        Session.GetRoleplay().Shower.UpdateState();
                                                        Session.GetRoleplay().InShower = false;
                                                        return;
                                                    }
                                                    else
                                                    {

                                                        Session.GetRoleplay().Shower.ExtraData = "1";
                                                        Session.GetRoleplay().Shower.UpdateState();
                                                    }

                                                }
                                            }*/

                                            #endregion

                                            #region Decrease Hygiene
                                            /* if (Session.GetRoleplay().HygieneDecrementSeconds <= 0)
                                            {


                                                if (Session.GetRoleplay().Hygiene - 2 > 0)
                                                {
                                                    Session.GetRoleplay().Hygiene -= 2;
                                                    Session.GetRoleplay().UpdateStats++;
                                                }
                                                else
                                                {
                                                    Session.GetRoleplay().Hygiene = 0;
                                                }


                                                Session.GetRoleplay().HygieneDecrementSeconds = 480;

                                            }
                                            else
                                            {

                                                if (Session.GetRoleplay().Hygiene <= 10 && RUser.CurrentEffect <= 0)
                                                {
                                                    RUser.ApplyEffect(10);
                                                }
                                                else if (RUser.CurrentEffect == 10 && Session.GetRoleplay().Hygiene > 10)
                                                {
                                                    RUser.ApplyEffect(0);
                                                }

                                                Session.GetRoleplay().HygieneDecrementSeconds--;
                                            }
                                            */
                                            #endregion

                                            #endregion

                                            #region Hunger Decrement (DISABLED)
                                            /*
                                            if (Session.GetRoleplay().HungerDecrementSeconds <= 0)
                                            {

                                                if (Session.GetRoleplay().Hunger + 2 <= 100)
                                                {
                                                    Session.GetRoleplay().UpdateStats++;
                                                    Session.GetRoleplay().Hunger += 2;
                                                }
                                                else
                                                {
                                                    Session.GetRoleplay().Hunger = 100;
                                                }

                                                if (Session.GetRoleplay().Hunger >= 88 && Session.GetRoleplay().Hunger < 100)
                                                {
                                                    Session.SendWhisper("Your hunger is running very low, get some food before you start to lose HP!");
                                                }
                                                else if (Session.GetRoleplay().Hunger >= 100)
                                                {

                                                    Random rand = new Random();
                                                    int Randhealth = rand.Next(5, 9);
                                                    int Randenergy = rand.Next(5, 10);
                                                    if (Session.GetRoleplay().CurHealth - Randhealth > 0)
                                                    {
                                                        Session.GetRoleplay().CurHealth -= Randhealth;
                                                        if (Session.GetRoleplay().Energy - Randenergy >= 0) { Session.GetRoleplay().Energy -= Randenergy; }
                                                        Session.SendWhisper("You have lost some energy and health from lack of food, get some quick before you die!");
                                                        Session.GetRoleplay().CurHealth -= 10;
                                                        Session.GetRoleplay().UpdateStats++;
                                                        Misc.MakeLay(RUser);
                                                    }
                                                    else
                                                    {
                                                        Session.GetRoleplay().UpdateStats++;
                                                        Session.SendNotif("You died from starvation!");
                                                        if (Session.GetRoleplay().Working)
                                                        {
                                                            Session.GetRoleplay().StopWork();
                                                        }
                                                        Session.GetRoleplay().Hunger = 0;
                                                        Session.GetRoleplay().DeadFigSet = false;
                                                        Session.GetRoleplay().DeadSeconds = 60;
                                                        Session.GetRoleplay().DeadTimer = 2;
                                                        Session.GetRoleplay().Dead = true;
                                                        Misc.HandleDeath(Session);
                                                    }
                                                }

                                                Session.GetRoleplay().HungerDecrementSeconds = 480;
                                            }
                                            else
                                            {
                                                Session.GetRoleplay().HungerDecrementSeconds--;
                                            }
                                            */
                                            #endregion                                        

                                            #region Gunshots (DISABLED)
                                            //if (Session.GetRoleplay().GunShots > 0)
                                            //{
                                            //  Session.GetRoleplay().GunShots--;
                                            //}
                                            #endregion

                                            #region Effect Timer

                                            #region Check if Certain Effects Enabled
                                            if (!Session.GetRoleplay().Gathering)
                                            {
                                                if (RUser != null)
                                                {
                                                    if (RUser.CurrentEffect == 65 || RUser.CurrentEffect == 9 || RUser.CurrentEffect == 4 || RUser.CurrentEffect == 25 || RUser.CurrentEffect == 8)
                                                    {
                                                        if (Session.GetRoleplay().EffectSeconds > 0)
                                                        {
                                                            Session.GetRoleplay().EffectSeconds--;
                                                        }
                                                        else
                                                        {
                                                            RUser.ApplyEffect(0);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region Frozen/Walk Timer
                                                if (RUser != null)
                                                {
                                                    if (Session.GetRoleplay().StunnedSeconds > 0)
                                                    {
                                                        Session.GetRoleplay().StunnedSeconds--;
                                                    }
                                                    else
                                                    {
                                                        if (RUser.CanWalk == false)
                                                        {
                                                            RUser.CanWalk = true;
                                                        }

                                                        if (RUser.Frozen == true)
                                                        {
                                                            RUser.Frozen = false;
                                                        }
                                                    }
                                                }
                                            #endregion

                                            #region Gun Enable Effect
                                                if (Session.GetRoleplay().Equiped != null)
                                            {
                                                if (RUser == null)
                                                    return;

                                                if (Session == null)
                                                    return;

                                                if (Session.GetRoleplay() == null)
                                                    return;

                                                if (HabboHotel.Roleplay.Combat.WeaponManager.WeaponsData == null)
                                                    return;

                                                if (Session.GetRoleplay().Equiped == null)
                                                    return;

                                                if (!HabboHotel.Roleplay.Combat.WeaponManager.WeaponsData.ContainsKey(Session.GetRoleplay().Equiped))
                                                    return;

                                                if (HabboHotel.Roleplay.Combat.WeaponManager.WeaponsData[Session.GetRoleplay().Equiped] == null)
                                                    return;

                                                if (RUser.CurrentEffect != HabboHotel.Roleplay.Combat.WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Effect_Id)
                                                {
                                                    RUser.ApplyEffect(HabboHotel.Roleplay.Combat.WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Effect_Id);
                                                }

                                                if (HabboHotel.Roleplay.Combat.WeaponManager.WeaponsData.ContainsKey(Session.GetRoleplay().Equiped.ToLower()))
                                                {
                                                    RUser.CarryItem(HabboHotel.Roleplay.Combat.WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].HandItem);
                                                }
                                            }
                                            #endregion

                                            #endregion

                                            #region Armored Timer (DISABLED)

                                            /*if (Session.GetRoleplay().Armored)
                                            {
                                                if (Session.GetRoleplay().Armor <= 0)
                                                {
                                                    DataRow User = null;

                                                    using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                                                    {
                                                        dbClient.SetQuery("SELECT look,motto FROM users WHERE id = '" + Session.GetHabbo().Id + "'");
                                                        User = dbClient.GetRow();
                                                    }

                                                    Session.GetRoleplay().Armor = 0;
                                                    Session.GetRoleplay().Armored = false;
                                                    Session.GetRoleplay().ArmoredFigSet = false;

                                                    Session.GetHabbo().Look = Convert.ToString(User["look"]);
                                                    Session.GetHabbo().Motto = Convert.ToString(User["motto"]);

                                                    Session.GetRoleplay().RefreshVals();
                                                }

                                                Session.GetRoleplay().ApplySpecialStatus("armored");

                                            }
                                            if (Session.GetRoleplay().Armor >= 1 && Session.GetRoleplay().Armored != true)
                                            {
                                                Session.GetRoleplay().Armored = true;
                                            }*/
                                            
                                            #endregion

                                            #region Mute Timer
                                            if (Session.GetRoleplay().SpamMuted)
                                            {
                                                if (Session.GetRoleplay().MuteSeconds <= 0)
                                                {
                                                    Session.GetRoleplay().SpamMuted = false;
                                                    Session.GetRoleplay().L_Message1 = "";
                                                    Session.GetRoleplay().L_Message2 = "";
                                                    Session.GetRoleplay().L_Message3 = "";
                                                }
                                            }
                                            if (Session.GetRoleplay().MuteSeconds > 0)
                                            {
                                                Session.GetRoleplay().MuteSeconds--;
                                            }
                                            if (Session.GetRoleplay().L_MessageTimer > 0)
                                            {
                                                Session.GetRoleplay().L_MessageTimer--;
                                            }
                                            #endregion

                                            #region Stun Timer
                                            if (RUser.CurrentEffect == 53)
                                            {
                                                if (!RUser.Frozen)
                                                {
                                                    RUser.ApplyEffect(0);
                                                }
                                            }
                                            #endregion

                                            #region Check Timers [EXTRA]

                                            #region Check if in Shower

                                            if (Session.GetRoleplay().InShower)
                                            {
                                                if (Session.GetRoleplay().ShowerSeconds <= 0)
                                                {
                                                    if (Session.GetRoleplay().Hygiene + 2 <= 100)
                                                    {
                                                        Session.GetRoleplay().Hygiene += 5;
                                                    }
                                                    else
                                                    {
                                                        Session.GetRoleplay().Hygiene = 100;
                                                        Session.GetRoleplay().InShower = false;
                                                        RUser.MoveTo(RUser.X, RUser.Y - 1);
                                                        Session.SendWhisper("You are now fully clean");
                                                        Session.Shout("*Stops bathing*");
                                                        Session.GetRoleplay().Shower.ExtraData = "0";
                                                        Session.GetRoleplay().Shower.UpdateNeeded = true;
                                                        Session.GetRoleplay().Shower.UpdateState();
                                                        Session.GetRoleplay().InShower = false;
                                                        return;
                                                    }
                                                    Session.GetRoleplay().ShowerSeconds = 1;
                                                }
                                                else
                                                {
                                                    Session.GetRoleplay().ShowerSeconds--;

                                                    if (RoleplayManager.Distance(new Vector2D(Session.GetRoleplay().Shower.X, Session.GetRoleplay().Shower.Y), new Vector2D(RUser.X, RUser.Y)) >= 1)
                                                    {
                                                        Session.Shout("*Stops bathing*");
                                                        Session.GetRoleplay().Shower.ExtraData = "";
                                                        Session.GetRoleplay().Shower.UpdateNeeded = true;
                                                        Session.GetRoleplay().Shower.UpdateState();
                                                        Session.GetRoleplay().InShower = false;
                                                        return;
                                                    }
                                                    else
                                                    {

                                                        Session.GetRoleplay().Shower.ExtraData = "1";
                                                        Session.GetRoleplay().Shower.UpdateState();
                                                    }

                                                }
                                            }

                                            #endregion

                                            #region Check if Robbing/Learning


                                            #region Check if Working
                                            if (Session.GetRoleplay().Working)
                                            {
                                                if (Session.GetRoleplay().WorkingOut || Session.GetRoleplay().WeightLifting || Session.GetRoleplay().Learning || Session.GetRoleplay().Robbery || Session.GetRoleplay().ATMRobbery || Session.GetRoleplay().Dead || Session.GetRoleplay().Jailed || Session.GetRoleplay().SentHome)
                                                {
                                                    RoleplayManager.Shout(Session, "*Stops Working*");
                                                    Session.GetRoleplay().StopWork();
                                                }
                                                if (JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                                                {
                                                    if (!JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].isWorkRoom(Session.GetHabbo().CurrentRoomId) && !JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].WorkRooms.Contains("*"))
                                                    {
                                                        RoleplayManager.Shout(Session, "*Stops Working as they left their workroom!*");
                                                        Session.GetRoleplay().StopWork();
                                                    }
                                                    if (JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].hasRights("police") || JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].hasRights("swat"))
                                                    {
                                                        if (RoleplayManager.PurgeTime)
                                                        {
                                                            RoleplayManager.Shout(Session, "*Stops Working as an officer due to the purge!*");
                                                            Session.GetRoleplay().StopWork();
                                                        }
                                                        if (Session.GetRoleplay().IsNoob)
                                                        {
                                                            RoleplayManager.Shout(Session, "*Stops Working as an officer due to god protection!*");
                                                            Session.GetRoleplay().StopWork();
                                                        }
                                                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                                                        {
                                                            RoleplayManager.Shout(Session, "*Stops Working as an officer due to being in a NOCOP zone!*");
                                                            Session.GetRoleplay().StopWork();
                                                        }
                                                    }
                                                }
                                                if (Session.GetHabbo().GetRoomUser() == null)
                                                    return;
                                                RoomUser User = Session.GetHabbo().GetRoomUser();
                                                if (User != null)
                                                {
                                                    if (User.IsAsleep)
                                                    {
                                                        RoleplayManager.Shout(Session, "*Stops Working [AFK]*");
                                                        Session.GetRoleplay().StopWork();
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region ATM Robbery Check
                                            if (Session.GetRoleplay().ATMRobbery == true)
                                            {
                                                RoomUser User = null;
                                                if (Session.GetRoleplay().RequestedTaxi || !Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("ATMROB") || !Session.GetRoleplay().NearItem("atm_moneymachine", 1))
                                                {
                                                    RoleplayManager.Shout(Session, "*Stops robbing the ATM*");
                                                    Session.GetRoleplay().SaveStatusComponents("atm_robbery");
                                                    Session.GetRoleplay().ATMRobbery = false;
                                                    Session.GetRoleplay().ATMRobTimer.stopTimer();
                                                }
                                                User = RUser;
                                                if (User != null)
                                                {
                                                    if (User.IsAsleep)
                                                    {
                                                        RoleplayManager.Shout(Session, "*Stops robbing the ATM [AFK]*");
                                                        Session.GetRoleplay().SaveStatusComponents("atm_robbery");
                                                        Session.GetRoleplay().ATMRobbery = false;
                                                        Session.GetRoleplay().ATMRobTimer.stopTimer();
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region Bank Robbery Check
                                            if (Session.GetRoleplay().Robbery == true)
                                            {
                                                RoomUser User = null;
                                                if (Session.GetRoleplay().RequestedTaxi || !Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("VAULT"))
                                                {
                                                    RoleplayManager.Shout(Session, "*Stops robbing the bank*");
                                                    Session.GetRoleplay().SaveStatusComponents("robbery");
                                                    Session.GetRoleplay().Robbery = false;
                                                    Session.GetRoleplay().bankRobTimer.stopTimer();
                                                }
                                                /*if (Session.GetHabbo().GetRoomUser().Z != 0)
                                                {
                                                    RoleplayManager.Shout(Session, "*Stops robbing the bank*");
                                                    Session.GetRoleplay().SaveStatusComponents("robbery");
                                                    Session.GetRoleplay().Robbery = false;
                                                    Session.GetRoleplay().bankRobTimer.stopTimer();
                                                }*/
                                                User = RUser;
                                                if (User != null)
                                                {
                                                    if (User.IsAsleep)
                                                    {
                                                        RoleplayManager.Shout(Session, "*Stops robbing the bank [AFK]*");
                                                        Session.GetRoleplay().SaveStatusComponents("robbery");
                                                        Session.GetRoleplay().Robbery = false;
                                                        Session.GetRoleplay().bankRobTimer.stopTimer();
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region Learning Check
                                            if (Session.GetRoleplay().Learning)
                                            {
                                                RoomUser User = null;
                                                if (Session.GetRoleplay().RequestedTaxi || !Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SCHOOL"))
                                                {
                                                    RoleplayManager.Shout(Session, "*Stops Learning*");
                                                    Session.GetRoleplay().SaveStatusComponents("learning");
                                                    Session.GetRoleplay().Learning = false;
                                                    Session.GetRoleplay().learningTimer.stopTimer();
                                                }
                                                User = RUser;
                                                if (User != null)
                                                {
                                                    if (User.IsAsleep)
                                                    {
                                                        RoleplayManager.Shout(Session, "*Stops Learning [AFK]*");
                                                        Session.GetRoleplay().SaveStatusComponents("learning");
                                                        Session.GetRoleplay().Learning = false;
                                                        Session.GetRoleplay().learningTimer.stopTimer();
                                                    }
                                                }
                                            }
                                            #endregion

                                            #endregion

                                            #region Check if Hunger Needed
                                            if (!Session.GetRoleplay().HungerDecrement)
                                            {
                                                Session.GetRoleplay().HungerDecrement = true;
                                                Session.GetRoleplay().hungerTimer = new hungerTimer(Session);
                                            }
                                            #endregion

                                            #region Check if Hygiene Needed
                                            if (!Session.GetRoleplay().HygieneDecrement)
                                            {
                                                Session.GetRoleplay().HygieneDecrement = true;
                                                Session.GetRoleplay().hygieneTimer = new hygieneTimer(Session);
                                            }
                                            #endregion

                                            #region Check if SentHome
                                            if (Session.GetRoleplay().SendHomeTimer >= 1 && Session.GetRoleplay().SentHome == false)
                                            {
                                                Session.GetRoleplay().SentHome = true;
                                                Session.GetRoleplay().SaveQuickStat("sendhome_timer", "" + Session.GetRoleplay().SendHomeTimer);
                                                Session.GetRoleplay().sendHomeTimer = new sendHomeTimer(Session);
                                                Session.SendWhisper("You have been senthome from work for " + Session.GetRoleplay().SendHomeTimer + " minutes!");
                                            }
                                            #endregion

                                            #region Check if Being Healed
                                            if (Session.GetRoleplay().CurHealth == Session.GetRoleplay().MaxHealth && RUser.CurrentEffect == 23 && Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("HOSP"))
                                            {
                                                RUser.ApplyEffect(0);
                                            }
                                            if (Session.GetRoleplay().BeingHealed)
                                            {
                                                if (RUser.CurrentEffect != 23)
                                                {
                                                    RUser.ApplyEffect(23);
                                                }
                                                if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("HOSP") || Session.GetRoleplay().RequestedTaxi)
                                                {
                                                    RUser.ApplyEffect(0);
                                                    Session.GetRoleplay().BeingHealed = false;
                                                    Session.GetRoleplay().healTimer.stopTimer();
                                                    Session.SendWhisper("You left the HOSPITAL in the middle of being healed!");
                                                }
                                            }
                                            #endregion

                                            #region Check if Being Massaged
                                            if (Session.GetRoleplay().Energy == 100 && RUser.CurrentEffect == 23 && Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SPA"))
                                            {
                                                RUser.ApplyEffect(0);
                                            }
                                            if (Session.GetRoleplay().BeingMassaged)
                                            {
                                                if (RUser.CurrentEffect != 23)
                                                {
                                                    RUser.ApplyEffect(23);
                                                }
                                                if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SPA") || Session.GetRoleplay().RequestedTaxi)
                                                {
                                                    RUser.ApplyEffect(0);
                                                    Session.GetRoleplay().BeingMassaged = false;
                                                    Session.GetRoleplay().massageTimer.stopTimer();
                                                    Session.SendWhisper("You left the SPA in the middle of a massage!");
                                                }
                                            }
                                            #endregion

                                            #region Check if Relaxing
                                            if (Session.GetRoleplay().Energy == 100 && RUser.CurrentEffect == 23 && Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SPA"))
                                            {
                                                RUser.ApplyEffect(0);
                                            }
                                            if (Session.GetRoleplay().Relaxing)
                                            {
                                                if (RUser.CurrentEffect != 23)
                                                {
                                                    RUser.ApplyEffect(23);
                                                }
                                                if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SPA") || Session.GetRoleplay().RequestedTaxi)
                                                {
                                                    RUser.ApplyEffect(0);
                                                    Session.GetRoleplay().Relaxing = false;
                                                    Session.GetRoleplay().relaxTimer.stopTimer();
                                                    Session.SendWhisper("You left the SPA in the middle of a nice relaxing session!");
                                                }
                                                if (!Session.GetRoleplay().NearItem("val14_recchair", 0))
                                                {
                                                    RUser.ApplyEffect(0);
                                                    Session.GetRoleplay().Relaxing = false;
                                                    Session.GetRoleplay().relaxTimer.stopTimer();
                                                    Session.SendWhisper("You got off the Treatment Chair in the middle of a nice relaxing session!");
                                                }
                                            }
                                            #endregion

                                            #region Check if Gang Capturing
                                            if (Session.GetRoleplay().GangCapturing)
                                            {
                                                RoomUser User = null;
                                                User = RUser;
                                                if (User != null)
                                                {
                                                    if (Session.GetRoleplay().RequestedTaxi || User.IsWalking || !GangManager.IsTurfSpot(Convert.ToInt32(Session.GetHabbo().CurrentRoomId), User.X, User.Y))
                                                    {
                                                        User.ApplyEffect(0);
                                                        Session.GetRoleplay().GangCapturing = false;
                                                        RoleplayManager.Shout(Session, "*Stops capturing the gang turf*", 3);
                                                        Session.GetRoleplay().gangCaptureTimer.stopTimer();

                                                        if (!GangManager.TurfData.ContainsKey(Convert.ToInt32(Session.GetHabbo().CurrentRoomId)))
                                                        {
                                                            Session.GetRoleplay().GangCapturing = false;
                                                            RoleplayManager.Shout(Session, "*Stops capturing the gang turf*", 3);
                                                            Session.GetRoleplay().gangCaptureTimer.stopTimer();
                                                            return;
                                                        }
                                                        else
                                                        if (GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId > 0)
                                                        {
                                                            RoleplayManager.AlertGang("Whoever was capturing your turf in RoomID " + Session.GetHabbo().CurrentRoomId + " ran away! It's safe now!", GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId);
                                                        }
                                                    }
                                                    if (User.IsAsleep)
                                                    {
                                                        User.ApplyEffect(0);
                                                        Session.GetRoleplay().GangCapturing = false;
                                                        RoleplayManager.Shout(Session, "*Stops capturing the turf zone [AFK]*", 3);
                                                        Session.GetRoleplay().gangCaptureTimer.stopTimer();
                                                        if (GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId > 0)
                                                        {
                                                            RoleplayManager.AlertGang("Whoever was capturing your turf in RoomID " + Session.GetHabbo().CurrentRoomId + " fell asleep! It's safe now!", GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId);
                                                        }
                                                    }
                                                }
                                            }
                                            if (!Session.GetRoleplay().GangCapturing && !Session.GetRoleplay().inColourWars && RUser.CurrentEffect == 59)
                                            {
                                                RUser.ApplyEffect(0);
                                            }
                                            #endregion

                                            #region Check if Using a Vehicle

                                            #region Check if Driving Car
                                            if (Session.GetRoleplay().usingCar == true)
                                            {
                                                if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("DRIVE"))
                                                {
                                                    RUser.FastWalking = true;
                                                    Session.GetRoleplay().usingCar = true;
                                                    RUser.ApplyEffect(Session.GetRoleplay().Car);
                                                }
                                                else if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("DRIVE"))
                                                {
                                                    RoleplayManager.Shout(Session, "*Removes keys from ignition and stops driving their car*");
                                                    RUser.FastWalking = false;
                                                    Session.GetRoleplay().usingCar = false;
                                                    RUser.ApplyEffect(0);
                                                }
                                            }
                                            #endregion

                                            #region Check if Flying Plane
                                            if (Session.GetRoleplay().planeUsing == 1)
                                            {
                                                if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("PLANE"))
                                                {
                                                    Session.GetRoleplay().planeUsing = 1;
                                                    RUser.ApplyEffect(175);
                                                    RUser.AllowOverride = true;
                                                    Session.GetRoleplay().usingPlane = true;
                                                }
                                                else if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("PLANE"))
                                                {
                                                    Session.GetRoleplay().usingPlane = false;
                                                    RUser.ApplyEffect(0);
                                                    RUser.AllowOverride = false;
                                                    Session.GetRoleplay().planeUsing = 0;

                                                    RoleplayManager.Shout(Session, "*Hops off their plane and stops navigating*");
                                                }
                                            }
                                            #endregion

                                            #endregion

                                            #region Check if In Brawl
                                            if (Session != null)
                                            {
                                                if (Session.GetHabbo() != null)
                                                {
                                                    if (Session.GetHabbo().CurrentRoom != null)
                                                    {
                                                        if (Session.GetRoleplay() != null)
                                                        {
                                                            if (Session.GetRoleplay().inBrawl)
                                                            {
                                                                if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("BRAWL"))
                                                                {
                                                                    Session.GetRoleplay().Brawl = false;
                                                                    Session.GetRoleplay().inBrawl = false;
                                                                    Session.SendWhisper("You have been signed off from the Brawl since you left the room!");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region Check if Staff On Duty
                                            if (Session.GetRoleplay().StaffDuty == true)
                                            {
                                                // Ambassador Check
                                                if (Session.GetHabbo().Rank == 3)
                                                {
                                                    RUser.ApplyEffect(178);
                                                }
                                                // Other Ranks
                                                else
                                                {
                                                    RUser.ApplyEffect(102);
                                                }
                                            }
                                            #endregion

                                            #region Check if Frozen

                                            if (Session.GetRoleplay().Frozen)
                                            {

                                                if (Session.GetRoleplay().FrozenSeconds <= 0)
                                                {
                                                    RUser.Frozen = false;
                                                    RUser.ApplyEffect(0);
                                                    Session.GetRoleplay().Frozen = false;
                                                    return;
                                                }

                                                RUser.Frozen = true;
                                                RUser.ApplyEffect(12);

                                            }
                                            #endregion

                                            #endregion
                                            break;
                                        }
                                    #endregion

                                    #region Quick Timer (1)
                                    case "quick":
                                        {

                                            #region Withdraw Money
                                            if (Session.GetRoleplay().Withdraw_Via_Phone)
                                            {
                                                if (Session.GetRoleplay().WithdrawDelay > 0)
                                                {
                                                    Session.GetRoleplay().WithdrawDelay--;
                                                }
                                                else
                                                {
                                                    if (Session.GetRoleplay().Bank < Session.GetRoleplay().AtmSetAmount)
                                                    {
                                                        Session.SendWhisper("Ocorreu um erro. Você não tem fundos suficientes no banco!");
                                                        Session.GetRoleplay().Withdraw_Via_Phone = false;
                                                        return;
                                                    }

                                                    RoleplayManager.Shout(Session, "*Retira $" + Session.GetRoleplay().AtmSetAmount + " da sua conta do banco*");
                                                    RoleplayManager.GiveMoney(Session, +Session.GetRoleplay().AtmSetAmount);
                                                    Session.GetRoleplay().SaveQuickStat("bank", "" + (Session.GetRoleplay().Bank - Session.GetRoleplay().AtmSetAmount));
                                                    Session.GetRoleplay().Bank -= Session.GetRoleplay().AtmSetAmount;
                                                    Session.GetRoleplay().AtmSetAmount = 0;
                                                    Session.GetRoleplay().WithdrawDelay = 0;
                                                    Session.GetRoleplay().Withdraw_Via_Phone = false;
                                                    RoomUser RoomUser = null;
                                                    RoomUser = RUser;
                                                    RoomUser.ApplyEffect(0);
                                                }
                                            }
                                            #endregion

                                            break;
                                        }
                                    #endregion
                                }
                            /*}
                        }
                    }
                }*/
            }
            catch(Exception ex)
            {
                Logging.LogException(ex.StackTrace);
            }
        }
    }
}
