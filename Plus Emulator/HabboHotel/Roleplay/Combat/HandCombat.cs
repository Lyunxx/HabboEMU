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
using Plus.HabboHotel.Roleplay.Gangs;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Roleplay.Timers;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.RoomBots;
using Plus.HabboHotel.Roleplay.Minigames.Mafia_Wars;
using Plus.HabboHotel.Pets;
using Plus.Messages;
using System.Drawing;
using Plus.Util;
using System.Threading;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay;
using System.Net;
using Plus.HabboHotel.Achievements.Composer;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.Roleplay.Minigames.Colour_Wars;

namespace Plus.HabboHotel.Roleplay.Combat
{
    public class HandCombat
    {
        public static void ExecuteAttack(GameClient Session, GameClient TargetSession)
        {
            if (CombatManager.CanAttack(Session, TargetSession))
            {

                bool Boxed = false;
                int Damage = CombatManager.DamageCalculator(Session);

                if (TargetSession.GetRoleplay().Armor >= 1 && TargetSession.GetRoleplay().inColourWars == false)
                {
                    TargetSession.GetRoleplay().Armor -= Damage;
                    #region Armor Broken?
                    if (TargetSession.GetRoleplay().Armor <= 0 && TargetSession.GetRoleplay().Armored == true)
                    {
                        TargetSession.GetRoleplay().Armored = false;
                        TargetSession.GetRoleplay().ArmoredFigSet = false;
                        Session.GetRoleplay().ApplySpecialStatus("armored");
                       TargetSession.GetHabbo().GetRoomUser().LastBubble = 3;
                        RoleplayManager.Shout(TargetSession, "*Body-armor shatters*");
                       TargetSession.GetHabbo().GetRoomUser().LastBubble = 0;
                    }
                    #endregion
                }
                else
                {
                     TargetSession.GetRoleplay().CurHealth -= Damage;
                }

                if (!Session.GetRoleplay().inColourWars && Session.GetRoleplay().InMafiaWars == false)
                {
                    Session.GetRoleplay().Energy -= 2;
                }

                Session.GetRoleplay().Punches++;

                if (TargetSession.GetRoleplay().CurHealth <= 0)
                {
                    if (TargetSession.GetRoleplay().IsBoxing)
                    {
                        if (TargetSession == Session)
                        {
                            Session.GetRoleplay().CurHealth = 1;
                            Session.SendWhisper("You cannot kill yourself!");
                            return;
                        }

                        if (Session.GetRoleplay().BoxingRoom != null && Session.GetRoleplay().BoxingRoom.SoloQueue != null)
                        {

                            RoleplayManager.Shout(Session, "*Swings my fist at " + TargetSession.GetHabbo().UserName + ", knocking them out winning the round!*");

                            Boxed = true;
                            Session.GetRoleplay().BoxingRoom.SoloQueue.EndMatch(Session.GetHabbo().UserName, true);
                            
                        }
        
                    }
                    if (TargetSession.GetRoleplay().inColourWars)
                    {

                        if (TargetSession == Session)
                        {
                            Session.GetRoleplay().CurHealth = 1;
                            Session.SendWhisper("You cannot kill yourself!");
                            return;
                        }
                        Session.GetHabbo().GetRoomUser().LastBubble = 3;
                        Session.GetRoleplay().GiveColorWarPoints(ColourManager.KillPoints);

                        Session.Shout("*Clecnhes my fist, swinging it hard and fast at " + TargetSession.GetHabbo().UserName + ", Landing a blow on their face causing " + Damage + " damage, sending them back to the graveyard! [+ " + ColourManager.KillPoints + " Color Wars Pts]*");

                        TargetSession.GetRoleplay().Transport(ColourManager.GraveYardID, 2);
                        TargetSession.GetRoleplay().CurHealth = 100;
                        TargetSession.GetRoleplay().Energy = 100;
                        TargetSession.GetRoleplay().Hunger = 0;

                        new DeadTimer(TargetSession);

                        // Add color points

                    }

                    if (TargetSession.GetRoleplay().InMafiaWars)
                    {

                        if (TargetSession == Session)
                        {
                            Session.GetRoleplay().CurHealth = 1;
                            Session.SendWhisper("You cannot kill yourself!");
                            return;
                        }
                        Session.GetHabbo().GetRoomUser().LastBubble = 3;

                        Session.GetRoleplay().GiveMafiaWarPoints(MafiaWarManager.KillPoints);
                        RoleplayManager.Shout(Session, "*Swings my fist at " + TargetSession.GetHabbo().UserName + ", Landing a blow on their face causing " + Damage + " damage, sending them back to the start! [+ " + MafiaWarManager.KillPoints + " Mafia Wars Pts]*");


                        TargetSession.GetRoleplay().Transport(Plus.GetGame().MafiaWars.GraveYardID, 2);
                        TargetSession.GetRoleplay().CurHealth = 100;
                        TargetSession.GetRoleplay().Energy = 100;
                        TargetSession.GetRoleplay().Hunger = 0;

                    }

                    if (TargetSession.GetRoleplay().Brawl == true)
                    {

                        Session.GetRoleplay().GiveColorWarPoints(1);
                       Session.GetHabbo().GetRoomUser().LastBubble = 3;

                        RoleplayManager.Shout(Session, "*Swings at " + TargetSession.GetHabbo().UserName + ", causing " + Damage + " damage, and kicking them out of brawl! [+5BP]*");

                       Session.GetHabbo().GetRoomUser().LastBubble = 0;

                        Session.GetRoleplay().Brawl_Pts += 5;
                        Session.GetRoleplay().SaveQuickStat("brawl_pts", Session.GetRoleplay().Brawl_Pts + "");
                        TargetSession.GetRoleplay().Brawl = false;
                        TargetSession.GetRoleplay().inBrawl = false;
                        if (TargetSession.GetRoleplay().Working)
                        {
                            TargetSession.GetRoleplay().StopWork();
                        }

                        TargetSession.GetRoleplay().DeadFigSet = false;
                        TargetSession.GetRoleplay().DeadSeconds = 60;
                        TargetSession.GetRoleplay().DeadTimer = 2;
                        TargetSession.GetRoleplay().Dead = true;
                        if (TargetSession.GetRoleplay().usingCar == true)
                        {
                            TargetSession.GetRoleplay().usingCar = false;
                           TargetSession.GetHabbo().GetRoomUser().FastWalking = false;
                        }


                        RoomUser User1 = null;
                        User1 = TargetSession.GetHabbo().GetRoomUser();

                        RoleplayManager.HandleDeath(TargetSession);

                    }
                    else if (TargetSession.GetRoleplay().Brawl == false && TargetSession.GetRoleplay().inColourWars == false && !TargetSession.GetRoleplay().InMafiaWars && Boxed == false)
                    {

                        #region Gang Rewards

                        if (Session.GetRoleplay().GangId > 0 && GangManager.validGang(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank))
                        {

                            Random _s = new Random();
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                bool HasGang = false;
                                int gangpts = _s.Next((TargetSession.GetRoleplay().Strength + TargetSession.GetRoleplay().savedSTR) * 1 - (int)Math.Round((double)(TargetSession.GetRoleplay().Strength + TargetSession.GetRoleplay().savedSTR )/ 3, 0), TargetSession.GetRoleplay().Strength * 2);
                                GangManager.GangData[Session.GetRoleplay().GangId].Kills++;
                                GangManager.GangData[Session.GetRoleplay().GangId].Points += gangpts;

                                if (TargetSession.GetRoleplay().GangId > 0 && GangManager.validGang(TargetSession.GetRoleplay().GangId, TargetSession.GetRoleplay().GangRank))
                                {
                                    HasGang = true;
                                }

                                if (HasGang)
                                {
                                    if (!Session.GetHabbo().HasFuse("fuse_owner"))
                                    {
                                        GangManager.GangData[TargetSession.GetRoleplay().GangId].Deaths++;
                                    }
                                    dbClient.RunFastQuery("UPDATE rp_gangs SET deaths = " + GangManager.GangData[TargetSession.GetRoleplay().GangId].Deaths + " WHERE id = " + TargetSession.GetRoleplay().GangId + "");
                                }

                                dbClient.RunFastQuery("UPDATE rp_gangs SET kills = " + GangManager.GangData[Session.GetRoleplay().GangId].Kills + ", points = " + GangManager.GangData[Session.GetRoleplay().GangId].Points + " WHERE id = " + Session.GetRoleplay().GangId + "");

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
                        Session.GetRoleplay().PunchKills++;
                        Session.SendMessage(AchievementScoreUpdateComposer.Compose(Session.GetHabbo().AchievementPoints));
                        Session.Shout("*Lands a final blow to their face " + TargetSession.GetHabbo().UserName + ", causing " + Damage + " damage sending them to hospital! [-2 Energy, +" + score + " PTS]*");
                        /*}
                        if (Session.GetRoleplay().LastKilled == TargetSession.GetHabbo().UserName)
                        {
                            Shout(Session, "*Swings at " + TargetSession.GetHabbo().UserName + ", causing " + Damage + " damage and knocking them out! [-2 Energy]*");
                            Session.SendWhisper("The last person you killed is this same person!");
                        }*/
                       Session.GetHabbo().GetRoomUser().LastBubble = 0;
                        if (!Session.GetHabbo().HasFuse("fuse_owner"))
                        {
                            TargetSession.GetRoleplay().Deaths++;
                        }
                        TargetSession.SendNotif("You were knocked out by " + Session.GetHabbo().UserName + ", and being transported to the hospital!");

                        if (TargetSession.GetRoleplay().Working)
                        {
                            TargetSession.GetRoleplay().StopWork();
                        }

                        TargetSession.GetRoleplay().DeadFigSet = false;
                        TargetSession.GetRoleplay().DeadSeconds = 60;
                        TargetSession.GetRoleplay().DeadTimer = 2;
                        TargetSession.GetRoleplay().Dead = true;
                        if (TargetSession.GetRoleplay().usingCar == true)
                        {
                            TargetSession.GetRoleplay().usingCar = false;
                           TargetSession.GetHabbo().GetRoomUser().FastWalking = false;
                        }


                        RoomUser User = null;
                        User = TargetSession.GetHabbo().GetRoomUser();

                        RoleplayManager.HandleDeath(TargetSession);

                        Bounties.CheckBounty(Session, TargetSession.GetHabbo().UserName);
                        Plus.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_Fighter", 1);
                        Plus.GetGame().GetAchievementManager().ProgressUserAchievement(TargetSession, "ACH_Death", 1);
                    }

                }
                else
                {
                   Session.GetHabbo().GetRoomUser().LastBubble = 3;
                   TargetSession.GetHabbo().GetRoomUser().LastBubble = 3;

                    Session.Shout("*Swings at " + TargetSession.GetHabbo().UserName + ", causing " + Damage + " damage [-2 Energy]*");
                    if (TargetSession.GetRoleplay().Armor >= 1 && TargetSession.GetRoleplay().inColourWars == false)
                    {
                        RoleplayManager.commandShout(TargetSession, "*[" + TargetSession.GetRoleplay().Armor + "AP Left!]*", 3);
                    }
                    else
                    {
                        RoleplayManager.commandShout(TargetSession, "*[" + TargetSession.GetRoleplay().CurHealth + "/" + TargetSession.GetRoleplay().MaxHealth + "]*", 3);
                    }

                   Session.GetHabbo().GetRoomUser().LastBubble = 0;
                   TargetSession.GetHabbo().GetRoomUser().LastBubble = 0;
                }

                Session.GetRoleplay().CoolDown = 3;
                Session.GetRoleplay().LastKilled = TargetSession.GetHabbo().UserName;
                TargetSession.GetRoleplay().UpdateStats++;
                Session.GetRoleplay().UpdateStats++;
            }
            else
                return;
        }

        internal static bool ExecuteAttackBot(GameClient User1, RoomUser RoomUser2, Pet Pet, RoomBot BotData)
        {


            Room Room = User1.GetHabbo().CurrentRoom;
            RoomUser RoomUser1 = User1.GetHabbo().GetRoomUser();

            Vector2D Pos1 = new Vector2D(RoomUser1.X, RoomUser1.Y);
            Vector2D Pos2 = new Vector2D(RoomUser2.X, RoomUser2.Y);
            bool canattack = false;

            #region Cooldown
            if (User1.GetRoleplay().CoolDown > 0)
            {
                User1.SendWhisper("Cooling down [" + User1.GetRoleplay().CoolDown + "/3]");
                return false;
            }
            #endregion

            #region Distance
            if (RoleplayManager.WithinAttackDistance(RoomUser1, RoomUser2))
            {
                canattack = true;
            }
            else if (RoleplayManager.Distance(Pos1, Pos2) > 1 && RoleplayManager.Distance(Pos1, Pos2) <= 4)
            {
                User1.Shout("*Swings at " + BotData.Name + ", but misses*");
                return false;
            }
            else if (RoleplayManager.Distance(Pos1, Pos2) >= 5)
            {
                User1.SendWhisper("You are too far away!");
                return false;
            }
            #endregion

            #region Status Conditions

            if (Room.RoomData.Description.Contains("NOHIT") && RoleplayManager.PurgeTime == false)
            {
                User1.SendWhisper("Sorry, but this is a no hitting zone!");
                User1.GetRoleplay().CoolDown = 3;
                return false;
            }
            if (Room.RoomData.Description.Contains("SAFEZONE"))
            {
                User1.SendWhisper("Sorry, but this is a safezone!");
                User1.GetRoleplay().CoolDown = 3;
                return false;
            }
            if (User1.GetRoleplay().Energy <= 0)
            {
                User1.SendWhisper("You do not have enough energy to do this!");
                return false;
            }
            if (User1.GetRoleplay().Dead)
            {
                User1.SendWhisper("Cannot complete this action while you are dead!");
                User1.GetRoleplay().CoolDown = 3;
                return false;
            }

            if (User1.GetRoleplay().Jailed)
            {
                User1.SendWhisper("Cannot complete this action while you are jailed!");
                User1.GetRoleplay().CoolDown = 3;
                return false;
            }

            if (RoomUser1.Stunned)
            {
                User1.SendWhisper("Cannot complete this action while you are stunned!");
                User1.GetRoleplay().CoolDown = 3;
                return false;
            }

            #endregion


            if (canattack)
            {
                GameClient Session = User1;

                int Damage = CombatManager.DamageCalculator(User1);

                BotData.cur_Health -= Damage;

                if (BotData.cur_Health <= 0)
                {

                    if (RoomUser2.BotData != null && RoomUser2.BotData._Boss)
                    {
                        Session.GetRoleplay().GiveMafiaWarPoints(MafiaWarManager.BossKillPoints);
                        User1.Shout("*Swings at " + BotData.Name + ", causing " + Damage + " damage and killing them and winning the Game! [+ " + MafiaWarManager.BossKillPoints + " Mafia Wars Pts]*");
                    }
                    else if(!RoomUser2.BotData._Boss)
                    {
                        Session.GetRoleplay().GiveMafiaWarPoints(MafiaWarManager.ThugKillPoints);
                        User1.Shout("*Swings at " + BotData.Name + ", causing " + Damage + " damage and killing them! [+ " + MafiaWarManager.ThugKillPoints + " Mafia Wars Pts]*");
                    }

                    RoomUser2.Chat(null, "*Passes out*", true, 1);
                    User1.GetHabbo().GetRoomUser().Attacker = null;
                    RoomUser2.BotAI._Victim = null;

                    if (RoomUser2.IsPet)
                    {
                        if (RoomUser2.FollowingOwner != null)
                        {
                            RoomUser2.FollowingOwner.GetClient().GetMessageHandler().PickUpPet(RoomUser2.FollowingOwner.GetClient(), RoomUser2.PetData.PetId, true);
                            return true;
                        }
                        else
                        {
                            Room.GetRoomUserManager().RemoveBot(RoomUser2.VirtualId, true);
                        }
                    }
                    else
                    {
                        Room.GetRoomUserManager().RemoveBot(RoomUser2.VirtualId, true);
                    }

                    if (RoomUser2.BotData != null && RoomUser2.BotData._Boss)
                    {

                        Session.GetRoleplay().GiveMafiaWarPoints(MafiaWarManager.BossKillPoints);
              
                        if (RoomUser2.BotData._Team.TeamName == "Green")
                        {
                            Plus.GetGame().MafiaWars.TeamWon("Blue");
                        }
                        else
                        {
                            Plus.GetGame().MafiaWars.TeamWon("Green");
                        }
                    }
                }
                else
                {
                    User1.Shout("*Swings at " + BotData.Name + ", causing " + Damage + " damage*");
                    RoomUser2.Chat(null, "[" + BotData.cur_Health + "/" + BotData.max_Health + "]", true, 1);

                    if (User1.GetHabbo().GetRoomUser().Attacker != null)
                    {
                        User1.GetHabbo().GetRoomUser().Attacker = null;
                        RoomUser2.BotAI._Victim = User1.GetHabbo().GetRoomUser();
                    }

                    User1.GetRoleplay().LastHitBot = RoomUser2;
                }

                Session.GetRoleplay().CoolDown = 3;
            }

            return true;
        }

    }
}
