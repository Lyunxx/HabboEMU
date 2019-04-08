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
using Plus.HabboHotel.Roleplay.Misc;
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
    public class BombCombat
    {
        public static bool CanExecuteAttack(GameClient Session, GameClient TargetSession)
        {
            if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
            {
                Session.SendWhisper("Este usuário não foi encontrado nesta sala!");
                return false;
            }

            Room Room = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
            RoomUser RoomUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().UserName);
            RoomUser Target = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetSession.GetHabbo().UserName);

            Vector2D Pos1 = new Vector2D(RoomUser.X, RoomUser.Y);
            Vector2D Pos2 = new Vector2D(Target.X, Target.Y);

            #region Distance
            if (Room.RoomData.Description.Contains("NOSHOOT") && RoleplayManager.PurgeTime == false)
            {
                Session.SendWhisper("Sorry, but this is a non bombing zone!");
                return false;
            }
            if (Room.RoomData.Description.Contains("SAFEZONE"))
            {
                Session.SendWhisper("Sorry, but this is a safezone!");
                return false;
            }
            int MyJobId = Session.GetRoleplay().JobId;
            int MyJobRank = Session.GetRoleplay().JobRank;
            if (Session.GetRoleplay().Bombs <= 0 && (!JobManager.JobRankData[MyJobId, MyJobRank].hasRights("army") && !Session.GetRoleplay().Working))
            {
                Session.SendWhisper("You do not have any bombs!");
                return false;
            }
            #endregion

            #region Status Conditions
            if (Session.GetRoleplay().Energy <= 0)
            {
                Session.SendWhisper("You ran out of energy ");
                return false;
            }
            if (Session.GetRoleplay().inBrawl == true)
            {
                Session.SendWhisper("Cannot do this in a Brawl");
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
            if (Session.GetRoleplay().GunShots >= 7)
            {
                Session.GetHabbo().GetRoomUser().LastBubble = 3;
                Session.Shout("*Stops to stretch their arm, cramped from throwing so many bombs*");
                Session.GetHabbo().GetRoomUser().LastBubble = 0;
                ServerMessage message = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                message.AppendInteger(5);
                Session.GetHabbo().GetRoomUser().IsGunReloaded = true;
                Session.GetHabbo().GetRoomUser().ReloadExpiryTime = Plus.GetUnixTimeStamp() + 5;
                Session.GetHabbo().GetRoomUser().GetClient().SendMessage(message);
                Session.GetRoleplay().GunShots = 0;
                return false;
            }
            if (TargetSession.GetRoleplay().usingPlane && !WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Name.ToLower().Contains("rpg"))
            {
                RoleplayManager.Shout(Session, "*Attempts to shoot " + TargetSession.GetHabbo().UserName + ", but misses as they are in a plane*");
                Session.GetRoleplay().GunShots++;
                return false;
            }
            if (RoleplayManager.Distance(Pos1, Pos2) > 6)
            {
                Session.GetHabbo().GetRoomUser().LastBubble = 3;
                RoleplayManager.Shout(Session, "*Throws my Grenade at " + TargetSession.GetHabbo().UserName + ", but misses*", 3);
                Session.GetHabbo().GetRoomUser().LastBubble = 0;
                Session.GetRoleplay().GunShots++;
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

        public static void ExecuteAttack(GameClient Session, GameClient TargetSession)
        {
            //int Damage = Gun_weaponCombat.CalculateDamage(Session, TargetSession);
            int Damage = new Random().Next(1, 18);
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
            Session.GetRoleplay().Energy -= 4;
            Session.GetRoleplay().Bombs -= 1;
            Session.GetRoleplay().SaveQuickStat("bombs", "" + Session.GetRoleplay().Bombs);
            Session.GetRoleplay().GunShots++;

            if (TargetSession.GetRoleplay().CurHealth <= 0)
            {
                #region Gang Rewards

                if (Session.GetRoleplay().GangId > 0 && HabboHotel.Roleplay.Gangs.GangManager.validGang(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank))
                {

                    Random _s = new Random();
                    using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                    {
                        bool HasGang = false;
                        int gangpts = _s.Next(TargetSession.GetRoleplay().Strength * 1 - (int)Math.Round((double)TargetSession.GetRoleplay().Strength / 3, 0), TargetSession.GetRoleplay().Strength * 2);
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
                    Session.GetRoleplay().BombKills++;
                    Session.SendMessage(AchievementScoreUpdateComposer.Compose(Session.GetHabbo().AchievementPoints));
                    RoleplayManager.Shout(Session, "*Joga minha granada em " + TargetSession.GetHabbo().UserName + ", causando " + Damage + " dano e matando ! [-4 Energia, +" + score + " PTS]*");
                /*}
                if (Session.GetRoleplay().LastKilled == TargetSession.GetHabbo().UserName)
                {
                    Misc.Shout(Session, "*Bombs " + TargetSession.GetHabbo().UserName + ", causing " + Damage + " damage and killing them ! [-2 Energy]*");
                    Session.SendWhisper("The last person you killed is this same person!");
                }*/
               Session.GetHabbo().GetRoomUser().LastBubble = 0;
                if (!Session.GetHabbo().HasFuse("fuse_owner"))
                {
                    TargetSession.GetRoleplay().Deaths++;
                }

                /*string rekt = "";

                rekt += "Hello, \n\nYou have been bombed to death by " + Session.GetHabbo().UserName + ", and are being transported to the hospital.";*/

                TargetSession.SendNotif("Você foi bombardeado até a morte por " + Session.GetHabbo().UserName + ", e estão sendo transportados para o hospital.");

                if (TargetSession.GetRoleplay().Working)
                {
                    TargetSession.GetRoleplay().StopWork();
                }

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
                    if (TargetSession.GetHabbo().Credits > lol)
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
               Session.GetHabbo().GetRoomUser().LastBubble = 3;
               TargetSession.GetHabbo().GetRoomUser().LastBubble = 3;

                RoleplayManager.Shout(Session, "*Joga uma granada em " + TargetSession.GetHabbo().UserName + ", causando " + Damage + " dano [-4 Energia]*", 3);
                if (TargetSession.GetRoleplay().Armor >= 1)
                {
                    Misc.RoleplayManager.commandShout(TargetSession, "*[" + TargetSession.GetRoleplay().Armor + "AP Left!]*");
                }
                else
                {
                    Misc.RoleplayManager.commandShout(TargetSession, "*[" + TargetSession.GetRoleplay().CurHealth + "/" + TargetSession.GetRoleplay().MaxHealth + "]*");
                } 
               TargetSession.GetHabbo().GetRoomUser().ApplyEffect(25);

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
