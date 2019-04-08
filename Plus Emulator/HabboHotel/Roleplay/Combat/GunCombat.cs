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
using Plus.HabboHotel.Roleplay.Misc;
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
using Plus.HabboHotel.Roleplay.Jobs;
using System.Net;
using Plus.HabboHotel.Achievements.Composer;
using Plus.Messages.Parsers;
using Plus.Database.Manager.Database.Session_Details.Interfaces;

namespace Plus.HabboHotel.Roleplay.Combat
{
    public class GunCombat
    {
        public static void ExecuteAttack(GameClient Session, GameClient TargetSession, Room Room, RoomUser RoomUser, RoomUser Target)
        {

            #region Global Variables
            string WeaponName = WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].DisplayName;
            int Damage = CombatManager.DamageCalculator(Session, true);
            #endregion

            #region RPG
            if (Session.GetRoleplay().Equiped.Contains("rpg"))
            {
                if (!Target.CurrentEffect.Equals(175) && !Target.CurrentEffect.Equals(176))
                {
                    Session.SendWhisper("This user is not using a plane!");
                    return;
                }
                else
                {
                    Session.Shout("*Fires their RPG at " + TargetSession.GetHabbo().UserName + "'s plane, causing it to plummet to the ground*");
                    TargetSession.GetRoleplay().MultiCoolDown["plane_cooldown"] = 300;
                    TargetSession.GetRoleplay().CheckingMultiCooldown = true;
                    TargetSession.GetRoleplay().usingPlane = false;
                    TargetSession.GetRoleplay().planeUsing = 0;
                    Target.ApplyEffect(25);
                    return;
                }
            }
            #endregion

            else
            {

                #region Increment / Decrement Values
                if (TargetSession.GetRoleplay().Armor >= 1)
                {
                    TargetSession.GetRoleplay().Armor -= Damage;
                    #region Armor Broken?
                    if (TargetSession.GetRoleplay().Armor <= 0 && TargetSession.GetRoleplay().Armored == true)
                    {
                        TargetSession.GetRoleplay().Armored = false;
                        TargetSession.GetRoleplay().ArmoredFigSet = false;
                        Target.LastBubble = 3;
                        TargetSession.Shout("*Body-armor shatters*");
                        Target.LastBubble = 0;
                    }
                    #endregion
                }
                else
                {
                    TargetSession.GetRoleplay().CurHealth -= Damage;
                }


                Session.GetRoleplay().Energy -= WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Need_Energy;
                Session.GetRoleplay().Bullets -= 1;
                Session.GetRoleplay().SaveQuickStat("bullets", "" + Session.GetRoleplay().Bullets);
                Session.GetRoleplay().GunShots++;
                #endregion

                if (TargetSession.GetRoleplay().CurHealth <= 0)
                {

                    int score = new Random().Next(0, 20);

                    if (!WeaponManager.WeaponsData.ContainsKey(Session.GetRoleplay().Equiped.ToLower()))
                    {
                        return;
                    }

                        Session.Shout("*Shoots their " + WeaponName + " at " + TargetSession.GetHabbo().UserName + ", causing " + Damage + " damage and killing them! [-" + WeaponManager.WeaponsData[Session.GetRoleplay().Equiped.ToLower()].Need_Energy + " Energy, +" + score + " PTS]*");
                    

                    #region Gang Rewards

                    if (Session.GetRoleplay().GangId > 0 && HabboHotel.Roleplay.Gangs.GangManager.validGang(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank))
                    {

                        Random _s = new Random();
                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            bool HasGang = false;
                            int gangpts = _s.Next((TargetSession.GetRoleplay().Strength + TargetSession.GetRoleplay().savedSTR) * 1 - (int)Math.Round((double)(TargetSession.GetRoleplay().Strength + TargetSession.GetRoleplay().savedSTR) / 3, 0), (TargetSession.GetRoleplay().Strength + TargetSession.GetRoleplay().savedSTR)* 2);
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

                    RoomUser.LastBubble = 3;
                    /*if (Session.GetRoleplay().LastKilled != TargetSession.GetHabbo().UserName)
                    {*/
                    Session.GetHabbo().AchievementPoints += score;
                    Session.GetHabbo().UpdateActivityPointsBalance();
                    Session.GetRoleplay().Kills++;
                    Session.GetRoleplay().GunKills++;
                    Session.SendMessage(AchievementScoreUpdateComposer.Compose(Session.GetHabbo().AchievementPoints));
                    /*}
                    if (Session.GetRoleplay().LastKilled == TargetSession.GetHabbo().UserName)
                    {
                        Misc.Shout(Session, "*Shoots their " + WeaponName + " at " + TargetSession.GetHabbo().UserName + ", causing " + Damage + " damage and killing them! [-" + weaponManager.WeaponsData[Session.GetRoleplay().Equiped].Need_Energy + " Energy]*");
                        Session.SendWhisper("The last person you killed is this same person!");
                    }*/
                    RoomUser.LastBubble = 0;
                    if (!Session.GetHabbo().HasFuse("fuse_owner"))
                    {
                        TargetSession.GetRoleplay().Deaths++;
                    }
                    TargetSession.SendNotif("You were shot dead by " + Session.GetHabbo().UserName + ", and are being transported to the hospital.");

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
                            Target.FastWalking = false;
                        }

                        RoleplayManager.HandleDeath(TargetSession);

                        int lol = new Random().Next(1, 25);
                        if (TargetSession.GetHabbo().Credits > lol && Session.GetRoleplay().LastKilled != TargetSession.GetHabbo().UserName)
                        {
                            RoleplayManager.GiveMoney(Session, +lol);
                            Session.Shout("*Steals $" + lol + " from " + TargetSession.GetHabbo().UserName + "'s wallet*");
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

                    CombatManager.HandleGun(Session, TargetSession, Room, RoomUser, Target, WeaponName, Damage);

                    #region Increment Values / Cooldown
                    Session.GetRoleplay().CoolDown = 3;
                    Session.GetRoleplay().LastKilled = TargetSession.GetHabbo().UserName;
                    TargetSession.GetRoleplay().UpdateStats++;
                    Session.GetRoleplay().UpdateStats++;
                    #endregion
                }

            }
        }
    }
}
