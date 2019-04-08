using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Misc;
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
using Plus.HabboHotel.Roleplay.Combat;
using System.Net;
using Plus.HabboHotel.Achievements.Composer;
using Plus.Messages.Parsers;
using Plus.Database.Manager.Database.Session_Details.Interfaces;

namespace Plus.HabboHotel.Roleplay.Combat
{
    public class MeleeCombat
    {
        public static bool CanExecuteAttack(GameClient Session, GameClient TargetSession)
        {
            if (Session == null)
                return false;
            if (Session.GetHabbo() == null)
                return false;
            if (Session.GetHabbo().CurrentRoom == null)
                return false;
            if (Session.GetHabbo().GetRoomUser() == null)
                return false;

            if(Session.GetRoleplay().Equiped == null)
            {
                Session.SendWhisper("You have not equiped a weapon!");
                return false;
            }
            if (TargetSession == null)
            {
                Session.SendWhisper("This user was not found in this room!");
                return false;
            }
            if(TargetSession.GetHabbo() == null)
            {
                Session.SendWhisper("This user was not found in this room!");
                return false;
            }
            if(TargetSession.GetHabbo().GetRoomUser() == null)
            {
                Session.SendWhisper("This user was not found in this room!");
                return false;
            }
            if(TargetSession.GetHabbo().CurrentRoom == null)
            {
                Session.SendWhisper("This user was not found in this room!");
                return false;
            }
            if (TargetSession.GetRoleplay() == null)
            {
                return false;
            }
            if(TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("This user was not found in this room!");
                return false;
            }

            Room Room = Session.GetHabbo().CurrentRoom;
            RoomUser RoomUser = Session.GetHabbo().GetRoomUser();
            RoomUser Target = TargetSession.GetHabbo().GetRoomUser();

            Vector2D Pos1 = new Vector2D(RoomUser.X, RoomUser.Y);
            Vector2D Pos2 = new Vector2D(Target.X, Target.Y);

            #region Distance
            if (Room.RoomData.Description.Contains("NOMELEE") && RoleplayManager.PurgeTime == false)
            {
                Session.SendWhisper("Sorry, but this is a no melee zone!");
                return false;
            }
            if (Room.RoomData.Description.Contains("SAFEZONE"))
            {
                Session.SendWhisper("Sorry, but this is a safe zone!");
                return false;
            }
            if (Session.GetRoleplay().Equiped == null)
            {
                Session.SendWhisper("You do not have a weapon equipped!");
                return false;
            }
            if (TargetSession.GetRoleplay().usingPlane)
            {
                RoleplayManager.Shout(Session, "*Attempts to melee " + TargetSession.GetHabbo().UserName + ", but misses as they are in a plane*");
                Session.GetRoleplay().CoolDown = 3;
                return false;
            }
            if (Session.GetRoleplay().Intelligence < WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Min_Intel)
            {
                Session.SendWhisper("You must have an intelligence level of at least " + WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Min_Intel + " to use this weapon!");
                return false;
            }
            if ((Session.GetRoleplay().Strength + Session.GetRoleplay().savedSTR) < WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Need_Str)
            {
                Session.SendWhisper("You must have a strength level of at least " + WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Need_Str + " to use this weapon!");
                return false;
            }
            if (Session.GetRoleplay().CoolDown > 0)
            {
                Session.SendWhisper("You are cooling down! [" + Session.GetRoleplay().CoolDown + "/3]");
                return false;
            }
            if (RoleplayManager.Distance(Pos1, Pos2) > WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Range)
            {
               Session.GetHabbo().GetRoomUser().LastBubble = 3;

                Session.Shout("*Swings their " + WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Name + " at " + TargetSession.GetHabbo().UserName + ", but misses*");

               Session.GetHabbo().GetRoomUser().LastBubble = 0;
                Session.GetRoleplay().CoolDown = 3;
                return false;
            }
            #endregion

            #region Status Conditions
            if (Session.GetRoleplay().Energy < WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Need_Energy)
            {
                Session.SendWhisper("You do not have enough energy to do this!");
                return false;
            }
            if (Session.GetRoleplay().inBrawl == true)
            {
                Session.SendWhisper("Unequip your melee weapon for the brawl!");
                return false;
            }

            if (Session.GetRoleplay().Dead)
            {
                Session.SendWhisper("Cannot complete this action while you are dead!");
                return false;
            }

            if (Session.GetRoleplay().Jailed)
            {
                Session.SendWhisper("Cannot complete this action while you are jailed!");
                return false;
            }

            if (TargetSession.GetRoleplay().Dead)
            {
                Session.SendWhisper("Cannot complete this action as this user is dead!");
                return false;
            }

            if (TargetSession.GetRoleplay().Jailed)
            {
                Session.SendWhisper("Cannot complete this action as this user is jailed!");
                return false;
            }

            if (RoomUser.Stunned)
            {
                Session.SendWhisper("Cannot complete this action while you are stunned!");
                return false;
            }
            if (Target.IsAsleep)
            {
                Session.SendWhisper("Cannot complete this action as this user is AFK!");
                return false;
            }
            if (RoomUser.IsAsleep)
            {
                Session.SendWhisper("Cannot complete this action as this user is AFK!");
                return false;
            }
            if (Session.GetRoleplay().IsNoob)
            {
                if (!Session.GetRoleplay().NoobWarned)
                {
                    Session.SendNotif("If you choose to do this again your temporary God Protection will be disabled!");
                    Session.GetRoleplay().NoobWarned = true;
                    return false;
                }
                else
                {
                    using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunFastQuery("UPDATE rp_stats SET is_noob = 0 WHERE id = '" + Session.GetHabbo().Id + "'");
                    }

                    Session.SendWhisper("Your god protection has been disabled!");
                    Session.GetRoleplay().IsNoob = false;
                    Session.GetRoleplay().SaveQuickStat("is_noob", "0");

                }
            }

            if (TargetSession.GetRoleplay().IsNoob)
            {
                Session.SendWhisper("Cannot complete this action as this user is under God Protection!");
                return false;
            }
            #endregion

            return true;
        }

        public static int CalculateDamage(GameClient Session, GameClient TargetSession)
        {
            int Damage = 0;
            string WeaponName = "";
            double WeaponLevelKills = Session.GetRoleplay().MeleeKills / 250;
            int WeaponLevel = (int)Math.Round(WeaponLevelKills, 0);
            int WeaponDamage = WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Max_Damage;

            foreach (KeyValuePair<string, Weapon> Weapon in Session.GetRoleplay().Weapons)
            {
                string WeaponData = Weapon.Key;
                WeaponName = WeaponManager.GetWeaponName(WeaponData);
            }

            int Damage2 = new Random().Next(WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Min_Damage, WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Max_Damage);
            Damage = new Random().Next((Session.GetRoleplay().Strength + Session.GetRoleplay().savedSTR), Damage2 + WeaponLevel * 3);
            return Damage;
        }

        public static void ExecuteAttack(GameClient Session, GameClient TargetSession)
        {
            {
                int Damage = MeleeCombat.CalculateDamage(Session, TargetSession);
                string WeaponName = WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Name;
                if (TargetSession.GetRoleplay().Armor >= 1)
                {
                    TargetSession.GetRoleplay().Armor -= Damage;
                    #region Armor Broken?
                    if (TargetSession.GetRoleplay().Armor <= 0 && TargetSession.GetRoleplay().Armored == true)
                    {
                        TargetSession.GetRoleplay().Armored = false;
                        TargetSession.GetRoleplay().ArmoredFigSet = false;
                       TargetSession.GetHabbo().GetRoomUser().LastBubble = 3;
                        Misc.RoleplayManager.Shout(TargetSession, "*Body-armor shatters!*");
                       TargetSession.GetHabbo().GetRoomUser().LastBubble = 0;
                    }
                    #endregion
                }
                else
                {
                    TargetSession.GetRoleplay().CurHealth -= Damage;
                }
                Session.GetRoleplay().Energy -= WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Need_Energy;

                if (TargetSession.GetRoleplay().CurHealth <= 0)
                {
                    #region Gang Rewards

                    if (Session.GetRoleplay().GangId > 0 && HabboHotel.Roleplay.Gangs.GangManager.validGang(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank))
                    {

                        Random _s = new Random();
                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            bool HasGang = false;
                            int gangpts = _s.Next((TargetSession.GetRoleplay().Strength + TargetSession.GetRoleplay().savedSTR) * 1 - (int)Math.Round((double)(TargetSession.GetRoleplay().Strength + TargetSession.GetRoleplay().savedSTR) / 3, 0), (TargetSession.GetRoleplay().Strength + TargetSession.GetRoleplay().savedSTR) * 2);
                            HabboHotel.Roleplay.Gangs.GangManager.GangData[Session.GetRoleplay().GangId].Kills++;
                            HabboHotel.Roleplay.Gangs.GangManager.GangData[Session.GetRoleplay().GangId].Points += gangpts;

                            if (TargetSession.GetRoleplay().GangId > 0 && HabboHotel.Roleplay.Gangs.GangManager.validGang(TargetSession.GetRoleplay().GangId, TargetSession.GetRoleplay().GangRank))
                            {
                                HasGang = true;
                            }

                            if (HasGang)
                            {
                                if (!Session.GetHabbo().HasFuse("fuse_owner"))
                                {
                                    HabboHotel.Roleplay.Gangs.GangManager.GangData[TargetSession.GetRoleplay().GangId].Deaths++;
                                }
                                dbClient.RunFastQuery("UPDATE rp_gangs SET deaths = " + HabboHotel.Roleplay.Gangs.GangManager.GangData[TargetSession.GetRoleplay().GangId].Deaths + " WHERE id = " + TargetSession.GetRoleplay().GangId + "");
                            }

                            dbClient.RunFastQuery("UPDATE rp_gangs SET kills = " + HabboHotel.Roleplay.Gangs.GangManager.GangData[Session.GetRoleplay().GangId].Kills + ", points = " + HabboHotel.Roleplay.Gangs.GangManager.GangData[Session.GetRoleplay().GangId].Points + " WHERE id = " + Session.GetRoleplay().GangId + "");

                        }
                    }

                    #endregion

                   Session.GetHabbo().GetRoomUser().LastBubble = 3;
                    /*if (Session.GetRoleplay().LastKilled != TargetSession.GetHabbo().UserName)
                    {*/
                        int score = new Random().Next(0, 20);
                        Session.GetHabbo().AchievementPoints += score;
                        Session.GetHabbo().UpdateActivityPointsBalance();
                        Session.GetRoleplay().Kills++;
                        Session.GetRoleplay().MeleeKills++;
                        Session.SendMessage(AchievementScoreUpdateComposer.Compose(Session.GetHabbo().AchievementPoints));
                        RoleplayManager.Shout(Session, "*Swings their " + WeaponName + " at " + TargetSession.GetHabbo().UserName + ", causing " + Damage + " damage and killing them! [-" + WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Need_Energy + " Energy] [+" + score + " PTS]*");
                    /*}
                    if (Session.GetRoleplay().LastKilled == TargetSession.GetHabbo().UserName)
                    {
                        Misc.Shout(Session, "*Swings their " + WeaponName + " at " + TargetSession.GetHabbo().UserName + ", causing " + Damage + " damage and killing them! [-" + weaponManager.WeaponsData[Session.GetRoleplay().Equiped].Need_Energy + " Energy]*");
                        Session.SendWhisper("The last person you killed is this same person!");
                    }*/
                   Session.GetHabbo().GetRoomUser().LastBubble = 0;
                    if (!Session.GetHabbo().HasFuse("fuse_owner"))
                    {
                        TargetSession.GetRoleplay().Deaths++;
                    }
                    TargetSession.SendNotif("You were meleed to death by " + Session.GetHabbo().UserName + ", and are being transported to the hospital.");

                    if (TargetSession.GetRoleplay().Working)
                    {
                        TargetSession.GetRoleplay().StopWork();
                    }

                    if (!Session.GetRoleplay().Equiped.Contains("police"))
                    {

                        #region Handle Death & Robbery
                        TargetSession.GetRoleplay().DeadFigSet = false;
                        TargetSession.GetRoleplay().DeadSeconds = 60;
                        TargetSession.GetRoleplay().DeadTimer = 2;
                        TargetSession.GetRoleplay().Dead = true;
                        if (TargetSession.GetRoleplay().usingCar == true)
                        {
                            TargetSession.GetRoleplay().usingCar = false;
                           TargetSession.GetHabbo().GetRoomUser().FastWalking = false;
                        }

                        RoleplayManager.HandleDeath(TargetSession);

                        int lol = new Random().Next(1, 25);
                        if (TargetSession.GetHabbo().Credits > lol && Session.GetRoleplay().LastKilled != TargetSession.GetHabbo().UserName)
                        {
                            RoleplayManager.GiveMoney(Session, +lol);
                            RoleplayManager.Shout(Session, "*Steals $" + lol + " from " + TargetSession.GetHabbo().UserName + "'s wallet*");
                            RoleplayManager.GiveMoney(TargetSession, -lol);
                            TargetSession.SendNotif(Session.GetHabbo().UserName + " stole $" + lol + " from you");
                        }

                        #endregion

                        Misc.Bounties.CheckBounty(Session, TargetSession.GetHabbo().UserName);
                        Plus.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_Fighter", 1);
                        Plus.GetGame().GetAchievementManager().ProgressUserAchievement(TargetSession, "ACH_Death", 1);

                    }
                    else
                    {

                        #region Handle Arrest
                        int Time = 5;
                        if (RoleplayManager.WantedListData.ContainsKey(TargetSession.GetHabbo().UserName.ToLower()))
                        {
                            string Data = RoleplayManager.WantedListData[TargetSession.GetHabbo().UserName.ToLower()];
                            foreach (KeyValuePair<string, string> User in RoleplayManager.WantedListData)
                            {

                                string Name = User.Key;

                                if (Name != TargetSession.GetHabbo().UserName.ToLower())
                                {
                                    continue;

                                }

                                string[] Split = User.Value.Split('|');

                                Time = Convert.ToInt32(Split[0]);
                            }
                        }
                        else
                        {
                            Time = 10;
                        }

                        TargetSession.SendNotif("You have been arrested by " + Session.GetHabbo().UserName + " for " + Time + " minute(s)");
                        TargetSession.GetRoleplay().JailFigSet = false;
                        TargetSession.GetRoleplay().JailedSeconds = 60;
                        TargetSession.GetRoleplay().JailTimer = Time;
                        TargetSession.GetRoleplay().Jailed = true;
                        TargetSession.GetRoleplay().Arrested++;
                        TargetSession.GetRoleplay().UpdateStats++;

                        Session.GetRoleplay().Arrests++;
                        Session.GetRoleplay().UpdateStats++;
                        #endregion

                    }
                }
                else
                {
                   Session.GetHabbo().GetRoomUser().LastBubble = 3;
                   TargetSession.GetHabbo().GetRoomUser().LastBubble = 3;

                    if (WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Speech == null || WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Speech == "")
                    {
                        RoleplayManager.Shout(Session, "*Swings their " + WeaponName + " at " + TargetSession.GetHabbo().UserName + ", causing " + Damage + " damage [-" + WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Need_Energy + " Energy]*");
                    }
                    else
                    {
                        string finalspeech = WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Speech.Replace("%energy%", Convert.ToString(WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Need_Energy));
                        Session.Shout(finalspeech);
                    }

                    if (TargetSession.GetRoleplay().Armor >= 1)
                    {
                        Misc.RoleplayManager.commandShout(TargetSession, "*[" + TargetSession.GetRoleplay().Armor + "AP Left!]*");
                    }
                    else
                    {
                        Misc.RoleplayManager.commandShout(TargetSession, "*[" + TargetSession.GetRoleplay().CurHealth + "/" + TargetSession.GetRoleplay().MaxHealth + "]*");
                    }
                   Session.GetHabbo().GetRoomUser().LastBubble = 0;
                   TargetSession.GetHabbo().GetRoomUser().LastBubble = 0;
                }

                Session.GetRoleplay().CoolDown = 3;
                Session.GetRoleplay().LastKilled = TargetSession.GetHabbo().UserName;
                TargetSession.GetRoleplay().UpdateStats++;
                Session.GetRoleplay().UpdateStats++;
            }

        }
    }
}
