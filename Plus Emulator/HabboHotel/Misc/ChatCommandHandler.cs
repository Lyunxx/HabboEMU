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
using Plus.HabboHotel.Roleplay.Jobs.Space;
using Plus.HabboHotel.Roleplay.Jobs.Cutting;
using Plus.HabboHotel.Roleplay.Jobs.Farming;
using Plus.HabboHotel.Roleplay.Casino.Slots;
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
using Plus.HabboHotel.Roleplay.Bots;
using Plus.HabboHotel.Roleplay.Minigames.Purge;
using System.Collections.Specialized;
using Plus.HabboHotel.Roleplay.Minigames.Colour_Wars;

//Regex for string check
using System.Text.RegularExpressions;

namespace Plus.HabboHotel.Misc
{
    class ChatCommandHandler
    {
        public static Boolean Parse(GameClient Session, string Input)
        {

            #region Checks
            if (!Input.StartsWith(":"))
            {
                return false;
            }

            if (Session == null)
            {
                return false;
            }


            if (!RoleplayData.Data["allowed_cmds"].ToString().Contains(Input.ToLower()))
            {
                if (Session.GetRoleplay().inColourWars)
                {
                    ParseColourWars(Session, Input);
                    return true;
                }

                if (Session.GetRoleplay().InMafiaWars)
                {
                    ParseMafiaWars(Session, Input);
                    return true;
                }
            }

            #endregion

            #region Set Input/Params
            Input = Input.Substring(1);
            string[] Params = Input.Split(' ');
            #endregion

            #region Main Switch Statement / Commands
            switch (Params[0].ToLower())
            {

                #region Plus Commands

                #region :makesay <user> <msg>
                case "makesay":
                    {
                        if (Session.GetHabbo().Id == 1)
                        {
                            string text2 = Params[1];
                            GameClient TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(text2);
                            Room class2 = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
                            if (Session == null || TargetClient == null)
                            {
                                return true;
                            }
                            RoomUser roomUser = class2.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                            roomUser.Chat(TargetClient, MergeParams(Params, 2), false, 0, roomUser.LastBubble);
                            return true;
                        }
                        return true;
                    }

                #endregion

                #region :rolld <user> <number>
                case "rolld":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {
                            string Username = Params[1];
                            GameClient TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Username);
                            Room class2 = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);

                            if (Session == null || TargetClient == null)
                            {
                                return true;
                            }

                            TargetClient.GetHabbo().RollRig = Convert.ToInt32(Params[2]);
                            Session.SendWhisper("Dice rig set to " + Convert.ToInt32(Params[2]) + " - successfully done!");
                            return true;
                        }
                        return true;
                    }

                #endregion
                #region :handitem <id>
                case "handitem":
                case "item":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_admin"))
                        {
                            return true;
                        }
                        if (Params.Length == 1)
                        {
                            Session.SendWhisper("Incorrect Syntax. :handitem <id>");
                            return true;
                        }
                        RoomUser user = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().UserName);
                        if (user.RidingHorse)
                        {
                            Session.SendWhisper(Plus.GetLanguage().GetVar("horse_handitem_error"));
                            return true;
                        }
                        if (user.IsLyingDown)
                        {
                            return true;
                        }
                        double ItemId;
                        string ItemId2 = Params[1];
                        if (double.TryParse(ItemId2, out ItemId))
                        {
                            user.CarryItem(int.Parse(ItemId.ToString()));
                            return true;
                        }
                        return true;
                    }
                #endregion
                #region :halink <msg>
                case "halink":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_owner"))
                        {
                            return true;
                        }
                        string str21 = Params[1];
                        string str22 = MergeParams(Params, 2);

                        ServerMessage message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                        message.AppendString("events");
                        message.AppendInteger(4);
                        message.AppendString("title");
                        message.AppendString("Hotel Alert - Link");
                        message.AppendString("message");
                        message.AppendString(str22 + "\n\n");
                        message.AppendString("linkUrl");
                        message.AppendString(str21);
                        message.AppendString("linkTitle");
                        message.AppendString("Open Link");

                        Plus.GetGame().GetClientManager().QueueBroadcaseMessage(message);
                        return true;
                    }
                #endregion
                #region :eventha <msg>
                case "eventha":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("Sorry but you aren't an events staff!");
                            return true;
                        }
                        if (Params.Length == 1)
                        {
                            Session.SendWhisper("Incorrect Syntax. :eventha <info>");
                            return true;
                        }
                        string str19 = "";
                        str19 = MergeParams(Params, 1);

                        ServerMessage message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                        message.AppendString("events");
                        message.AppendInteger(4);
                        message.AppendString("title");
                        message.AppendString("Event Notification");
                        //message.AppendString(Plus.GetLanguage().GetVar("alert_event_title"));
                        message.AppendString("message");
                        message.AppendString("There's currently an event going on right now!\n\nThe event is <b>" + Session.GetHabbo().CurrentRoom.RoomData.Name + "</b> and it's currently hosted by <b>" +
                            Session.GetHabbo().UserName + "</b>!\n\nIf you want to participate in the event, click 'Participate!' located on the bottom of the notification!" +
                            "\n\n" +
                            "More details / Prizes:\n<b>" + str19 + "</b>\n\n");
                        message.AppendString("linkUrl");
                        message.AppendString("event:navigator/goto/" + Session.GetHabbo().CurrentRoomId);
                        message.AppendString("linkTitle");
                        message.AppendString(Plus.GetLanguage().GetVar("alert_event_goRoom"));

                        Plus.GetGame().GetClientManager().QueueBroadcaseMessage(message);
                        return true;
                    }
                #endregion
                #region :disco
                case "disco":
                case "party":
                    {

                        Room room = Session.GetHabbo().CurrentRoom;
                        if (room == null) { Session.SendWhisper("Room is null"); return true; }

                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }

                        if (!room.DiscoMode)
                        {
                            room.DiscoMode = true;
                            Session.SendWhisper("Disco mode has been enabled!");
                        }
                        else
                        {
                            room.DiscoMode = false;
                            Session.SendWhisper("Disco mode has been disabled!");
                        }

                        return true;
                    }

                #endregion
                #region :filter <word>
                case "filter":
                case "addfilter":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            #region Params / Variables

                            string word = Params[1];

                            if (Params.Length < 1)
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :filter <word>");
                                return true;
                            }
                            else
                            {
                                word = MergeParams(Params, 1);
                            }

                            #endregion

                            #region Conditions

                            if (string.IsNullOrEmpty(word))
                            {
                                Session.SendWhisper("You must type in a word you want to filter.");
                                return true;
                            }

                            #endregion

                            #region Execute

                            Security.AntiPublicistas.AddBannedHotel(word);
                            Session.SendWhisper("Successfully filtered: " + word + "!");

                            #endregion

                        }
                        return true;
                    }

                #endregion

                #endregion

                #region Foton Commands

                #region Roleplay VIP

                #region :vipcommands
                case "vipcommands":
                    {
                        Session.SendWhisper("Coming soon!");
                        return true;
                    }
                #endregion
                #region :bodyarmour
                case "bodyarmour":
                case "ba":
                    {
                        #region Conditions
                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("swat")
                            && Session.GetRoleplay().Vests <= 0)
                        {
                            Session.SendWhisper("You dont have any vests remaining!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("bodyarmour"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("bodyarmour", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["bodyarmour"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["bodyarmour"] + "/300]");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOBA"))
                        {
                            Session.SendWhisper("You cannot use bodyarmours in this room!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        if (Session.GetRoleplay().JobHasRights("police") && Session.GetRoleplay().Working)
                        {
                            RoleplayManager.Shout(Session, "*Applies their Police Body-Armor [50 Armor, 150 Energy]*");
                            Session.GetRoleplay().Armor = 50;
                            Session.GetRoleplay().Energy = 150;
                            Session.GetRoleplay().MultiCoolDown["bodyarmour"] = 300;
                            Session.GetRoleplay().CheckingMultiCooldown = true;
                            Session.GetRoleplay().Armored = true;
                            return true;
                        }
                        else if (Session.GetRoleplay().JobHasRights("swat") && Session.GetRoleplay().Working)
                        {
                            RoleplayManager.Shout(Session, "*Applies their S.W.A.T. Body-Armor [100 Armor, 150 Energy]*");
                            Session.GetRoleplay().Armor = 100;
                            Session.GetRoleplay().Energy = 150;
                            Session.GetRoleplay().MultiCoolDown["bodyarmour"] = 300;
                            Session.GetRoleplay().CheckingMultiCooldown = true;
                            Session.GetRoleplay().Armored = true;
                            return true;
                        }
                        else
                        {
                            if (Session.GetRoleplay().Vests > 0)
                            {
                                Session.GetRoleplay().Vests -= 1;
                            }
                            Session.GetRoleplay().SaveQuickStat("vests", "" + Session.GetRoleplay().Vests);
                            int remainingvests = Session.GetRoleplay().Vests;
                            RoleplayManager.Shout(Session, "*Applies their Body-Armor [50 Armor, 150 Energy]*");
                            Session.GetRoleplay().Armor = 50;
                            Session.GetRoleplay().Energy = 150;
                            Session.GetRoleplay().MultiCoolDown["bodyarmour"] = 300;
                            Session.GetRoleplay().CheckingMultiCooldown = true;
                            Session.GetRoleplay().Armored = true;
                            Session.SendWhisper("You have '" + remainingvests + "' vests remaining!");
                            return true;
                        }
                        #endregion
                    }
                #endregion
                #region :rocket

                case "rocket":
                case "jetpack":
                    {

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_vip"))
                        {
                            Session.SendWhisper("You must be VIP to do this!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("rocket_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("rocket_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["rocket_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["rocket_cooldown"] + "/3]");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("ROCKET") && !Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("JETPACK"))
                        {
                            Session.SendWhisper("Sorry, but you can't use your rocket in this room!");

                            return true;
                        }
                        #endregion

                        #region Execute
                        Session.GetHabbo().GetRoomUser().ApplyEffect(6);
                        RoleplayManager.Shout(Session, "*Activates their Rocket*");
                        Session.GetHabbo().GetRoomUser().AllowOverride = true;
                        Session.GetRoleplay().MultiCoolDown["rocket_cooldown"] = 3;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        return true;
                        #endregion
                    }

                #endregion
                #region :stoprocket

                case "stoprocket":
                case "stopjetpack":
                    {

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_vip"))
                        {
                            Session.SendWhisper("You must be VIP to do this!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("rocket_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("rocket_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["rocket_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["rocket_cooldown"] + "/3]");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("ROCKET") && !Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("JETPACK"))
                        {
                            Session.SendWhisper("Sorry, but you can't use your rocket in this room!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        Session.GetHabbo().GetRoomUser().ApplyEffect(0);
                        RoleplayManager.Shout(Session, "*Deactivates their Rocket*");
                        Session.GetHabbo().GetRoomUser().AllowOverride = false;
                        Session.GetRoleplay().MultiCoolDown["rocket_cooldown"] = 3;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        return true;
                        #endregion
                    }

                #endregion
                #region :saveslot <id>
                case "saveslot":
                    {
                        #region Conditions

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :saveslot <slotid>");
                            return true;
                        }

                        int slotid = Convert.ToInt32(Params[1]);

                        if (!Session.GetHabbo().HasFuse("fuse_vip"))
                        {
                            Session.SendWhisper("You must be VIP to do this!");
                            return true;
                        }
                        if (Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("You can't steal ur companies clothes!");
                            return true;
                        }
                        if (slotid <= 0 || slotid > 5)
                        {
                            Session.SendWhisper("Invalid SlotID: " + slotid + "!");
                            return true;
                        }

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("saveslot_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("saveslot_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["saveslot_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["saveslot_cooldown"] + "/5]");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.saveSlotLook(Session, slotid, Session.GetHabbo().Look);
                        RoleplayManager.Shout(Session, "*Saves new clothing to slot " + slotid + "*", 36);
                        Session.GetRoleplay().MultiCoolDown["saveslot_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }


                #endregion
                #region :useslot <id>
                case "useslot":
                    {
                        #region Conditions

                        if (!Plus.IsNum(Params[1]))
                        {
                            Session.SendWhisperBubble("Numbers only!", 34);
                            return true;
                        }

                        int slotid = Convert.ToInt32(Params[1]);

                        if (!Session.GetHabbo().HasFuse("fuse_vip"))
                        {
                            Session.SendWhisperBubble("You must be VIP to do this!", 34);
                            return true;
                        }
                        if (slotid <= 0 || slotid > 5)
                        {
                            Session.SendWhisperBubble("Invalid SlotID: " + slotid + "!", 34);
                            return true;
                        }
                        if (RoleplayManager.ZombieInfection)
                        {
                            Session.SendWhisperBubble("You cannot do this during the Zombie Infection event!", 34);
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisperBubble("You cannot do this while jailed!", 34);
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisperBubble("You cannot do this while dead!", 34);
                            return true;
                        }
                        if (Session.GetRoleplay().Working)
                        {
                            Session.SendWhisperBubble("You cannot do this while working!", 34);
                            return true;
                        }

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("useslot_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("useslot_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["useslot_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["useslot_cooldown"] + "/5]");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.useSlotLook(Session, slotid);
                        RoleplayManager.Shout(Session, "*Pulls out some new fresh clothing from bag*", 36);
                        Session.GetRoleplay().MultiCoolDown["useslot_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }


                #endregion

                #endregion

                #region General / Misc

                #region Uncategorized

                #region :commands (INCOMPLETE)
                case "commands":
                case "cmds":
                    {
                        string Commands = "";
                        Commands += "Note: This may not contain all the current commands, we will update it soon!\n";
                        Commands += "==========================\nGeneral\n==========================\n";
                        Commands += ":info - View server info ; uptime / statistics etc\n";
                        Commands += ":stats - View your stats\n";
                        Commands += ":changelog - View the changelog\n";
                        Commands += ":taxi <roomid> - Taxi to a designated room\n";
                        Commands += ":stoptaxi - Stop your taxi request\n";
                        Commands += ":hotrooms - View the rooms with most visitors\n";
                        Commands += ":gamemap - View all of FluxRP's rooms\n";
                        Commands += ":whosonline - View online players\n";
                        Commands += ":give x <amount> - Give a target user some of your coins\n";
                        Commands += ":logout - Reloads the Client for you\n";
                        Commands += "====================================================\n";

                        Commands += "\n==========================\nMisc\n==========================\n";
                        Commands += ":chelp - View all corporation/job commands\n";
                        Commands += ":ghelp - View all gang comands\n";
                        Commands += ":rob x - Rob a target user\n";
                        Commands += ":offer x <item> - Sell/Offer an item to a target user\n";
                        Commands += ":dispose - Dispose/Destroy the weed you're carrying\n";
                        Commands += ":rum - Alternative to double clicking bins\n";
                        Commands += ":smokeweed - Smoke weed & gain a temporary strength boost\n";
                        Commands += ":eatcarrot - Eat a carrot & gain some more health\n";
                        Commands += ":text <user> <msg> - Text a user\n";
                        Commands += ":poof - Refresh your figure completely\n";
                        Commands += ":buycredit <amount> - Gives you the amount of phone credit";
                        Commands += ":corplist - View all corporations\n";
                        Commands += ":corpinfo <jobid> - View the selected corporation\n";
                        Commands += "====================================================\n";

                        Commands += "\n==========================\nCombat\n==========================\n";
                        Commands += ":hit x - Hit a user\n";
                        Commands += ":shoot x - Shoot a user\n";
                        Commands += ":bomb x - Bomb a user\n";
                        Commands += ":equip <weapon> - Equip a weapon to shoot\n";
                        Commands += ":unequip <weapon> - Opposite of equip\n";
                        Commands += "====================================================\n";

                        Commands += "\n==========================\nVIP\n==========================\n";
                        Commands += ":push x - Push a user\n";
                        Commands += ":pull x - Pull a user\n";
                        Commands += ":moonwalk - Makes your character moonwalk\n";
                        Commands += ":useslot <id> - Uses the selected slot\n";
                        Commands += ":saveslot <id> - Saves a new slot\n";
                        Commands += ":rocket - Activates your Rocket\n";
                        Commands += ":stoprocket - De-activates your Rocket\n";
                        Commands += ":flagme <new name> - Changes your username\n";
                        Commands += ":vipalert <message> - Sends a global alert to other VIP members\n";
                        Commands += ":togglevipalerts - VIP alerts will not be shown to you\n";
                        Commands += "====================================================\n";

                        Commands += "\n==========================\nAmmunation\n==========================\n";
                        Commands += ":offer x <weed/revolver/crowbar/rifle/katana/hammer/carrot> - Offers a user something specific\n";
                        Commands += ":buybullets <amount> - Buys a specific amount of bullets\n";
                        Commands += ":buybombs <amount> - Buys a specific amount of bombs\n";
                        Commands += "====================================================\n";

                        Session.SendNotifWithScroll(Commands);

                        return true;
                    }
                #endregion
                #region :logout
                case "logout":
                    {
                        Session.GetConnection().Dispose();
                    }
                    return true;
                #endregion
                #region :poof
                case "poof":
                    {

                        #region Conditions
                        if (Session.GetRoleplay().Working)
                        {
                            Session.GetRoleplay().StopWork(true);
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this while jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this while dead!");
                            return true;
                        }
                        if (RoleplayManager.ZombieInfection)
                        {
                            Session.SendWhisper("You cannot do this during a Zombie Infection Event!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        DataRow User = null;

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT look,motto FROM users WHERE id = '" + Session.GetHabbo().Id + "'");
                            User = dbClient.GetRow();
                        }

                        Session.GetHabbo().Look = Convert.ToString(User["look"]);
                        Session.GetHabbo().Motto = Convert.ToString(User["motto"]);

                        Session.GetRoleplay().RefreshVals();
                        #endregion
                        Session.SendWhisper("You have successfully refreshed your motto and figure!");
                        return true;
                    }
                #endregion
                #region :stuck
                case "stuck":
                    {
                        if (Session.GetHabbo().CurrentRoom == null)
                        {
                            Session.SendNotif("Attempting to unstuck you!");
                            Session.GetMessageHandler().PrepareRoomForUser(Session.GetHabbo().CurrentRoomId, "");
                        }
                        return true;
                    }
                #endregion
                #region :roominfo
                case "roominfo":
                case "rinfo":
                    {
                        string room = "";
                        Room RoomInfo = Session.GetHabbo().CurrentRoom;

                        room += "====================\nRoom Information for " + RoomInfo.RoomData.Name + " (ID: " + RoomInfo.RoomData.Id + ")\n====================\n\n";
                        room += "RoomID: " + RoomInfo.RoomData.Id + "\n";
                        room += "Room Name: " + RoomInfo.RoomData.Name + "\n";
                        room += "Room Owner: " + RoomInfo.RoomData.Owner + " (ID: " + RoomInfo.RoomData.OwnerId + ")\n";
                        room += "Current Users: " + RoomInfo.RoomData.UsersNow + "/" + RoomInfo.RoomData.UsersMax + "\n\n";
                        room += "Users in the room:\n";

                        foreach (RoomUser user in RoomInfo.GetRoomUserManager().UserList.Values)
                        {
                            if (user == null)
                                continue;
                            if (user.GetClient() == null)
                                continue;
                            if (user.GetClient().GetHabbo() == null)
                                continue;
                            if (user.GetClient().GetHabbo().SpectatorMode)
                                continue;

                            room += "- " + user.GetClient().GetHabbo().UserName + "\n";
                        }

                        Session.SendNotifWithScroll(room);

                        return true;
                    }
                #endregion
                #region :help
                case "help":
                case "prices":
                    {
                        string help = "================== Room Help & Commands ===================\n\n";
                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT * FROM `rp_roomhelp` WHERE `roomid` = '" + Session.GetHabbo().CurrentRoomId + "'");
                            DataTable Table = dbClient.GetTable();
                            foreach (DataRow Row in Table.Rows)
                            {
                                int HelpType = Convert.ToInt32(Row["type"]);
                                if (HelpType == 1)
                                {
                                    help += "" + Row["details"] + "\n";
                                }
                                else
                                {
                                    help += "" + Row["details"] + "\n\n";
                                }
                            }
                        }
                        Session.SendNotifWithScroll(help);
                        return true;
                    }
                #endregion
                #region :stats
                case "stats":
                case "mystats":
                    {
                        string Stats = "";
                        string isDead = (Session.GetRoleplay().Dead) ? "Yes" : "No";
                        string isJailed = (Session.GetRoleplay().Jailed) ? "Yes" : "No";
                        string GangName = (Session.GetRoleplay().GangId > 0 && GangManager.validGang(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank)) ? GangManager.GangData[Session.GetRoleplay().GangId].Name : "None";
                        string GangRank = (Session.GetRoleplay().GangId > 0 && GangManager.validGang(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank)) ? GangManager.GangRankData[Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank].Name : "None";

                        Stats += "============================================\nGeneral Statistics \n============================================\n";
                        Stats += "Health: " + Session.GetRoleplay().CurHealth + "/" + Session.GetRoleplay().MaxHealth + "\n";
                        if (Session.GetRoleplay().Armor >= 1)
                        {
                            Stats += "Armor: " + Session.GetRoleplay().Armor + "\n";
                        }
                        if (Session.GetRoleplay().Armor <= 0)
                        {
                            Stats += "Armor: 0\n";
                        }
                        Stats += "Energy: " + Session.GetRoleplay().Energy + "/100\n";
                        Stats += "Hunger: " + Session.GetRoleplay().Hunger + "/100\n";
                        Stats += "Hygiene: " + Session.GetRoleplay().Hygiene + "/100\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nMisc Statistics \n============================================\n";
                        Stats += "Total Deaths: " + Session.GetRoleplay().Deaths + "\n";
                        Stats += "Total Kills: " + Session.GetRoleplay().Kills + "\n";
                        Stats += "Punch Kills: " + Session.GetRoleplay().PunchKills + "\n";
                        Stats += "Melee Kills: " + Session.GetRoleplay().MeleeKills + "\n";
                        Stats += "Gun Kills: " + Session.GetRoleplay().GunKills + "\n";
                        Stats += "Bomb Kills: " + Session.GetRoleplay().BombKills + "\n";
                        Stats += "Total Punches: " + Session.GetRoleplay().Punches + "\n";
                        Stats += "Total Times Arrested: " + Session.GetRoleplay().Arrested + "\n";
                        Stats += "Total Arrested Users: " + Session.GetRoleplay().Arrests + "\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nSkilling Statistics \n============================================\n";
                        Stats += "Mining Level: " + Session.GetRoleplay().SpaceLevel + "\n";
                        Stats += "Mining EXP: " + Session.GetRoleplay().SpaceXP + "\n";
                        Stats += "Farming Level: " + Session.GetRoleplay().FarmingLevel + "\n";
                        Stats += "Farming EXP: " + Session.GetRoleplay().FarmingXP + "\n";
                        Stats += "LumberJack Level: " + Session.GetRoleplay().WoodLevel + "\n";
                        Stats += "LumberJack EXP: " + Session.GetRoleplay().WoodXP + "\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nInformative Statistics \n============================================\n";
                        Stats += "Currently Dead: " + isDead + "\n";
                        Stats += "Currently Jailed: " + isJailed + "\n";
                        if (Session.GetRoleplay().Married_To != 0)
                        {
                            Stats += "Married To: " + RoleplayManager.ReturnOfflineInfo((uint)Session.GetRoleplay().Married_To, "username") + "\n";
                        }
                        if (Session.GetRoleplay().Married_To == 0)
                        {
                            Stats += "Married To: You are Single!\n";
                        }
                        Stats += "============================================\n";

                        Stats += "\n============================================\nEvents Statistics \n============================================\n";
                        Stats += "Brawl Points: " + Session.GetRoleplay().Brawl_Pts + "\n";
                        Stats += "Infection Points: " + Session.GetRoleplay().Infection_Pts + "\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nTimers Statistics \n============================================\n";
                        Stats += "Jail Timer: " + Session.GetRoleplay().JailTimer + " mins\n";
                        Stats += "Dead Timer: " + Session.GetRoleplay().DeadTimer + " mins\n";
                        Stats += "Workout Timer: " + Session.GetRoleplay().WorkoutTimer_Done + "/" + Session.GetRoleplay().WorkoutTimer_ToDo + "\n";
                        Stats += "WeightLift Timer: " + Session.GetRoleplay().WeightLiftTimer_Done + "/" + Session.GetRoleplay().WeightLiftTimer_ToDo + "\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nOther Statistics \n============================================\n";
                        Stats += "JobID: " + Session.GetRoleplay().JobId + "\n";
                        Stats += "JobRank: " + Session.GetRoleplay().JobRank + "\n";
                        Stats += "Shifts completed: " + Session.GetRoleplay().Shifts + "\n";
                        Stats += "Gang: " + GangName + "\n";
                        Stats += "Gang Rank: " + GangRank + "\n";
                        Stats += "Gang Turf Bonus: Currently Disabled!\n";
                        Stats += "Last Killed User: " + Session.GetRoleplay().LastKilled + "\n";
                        string WeedBonus = (Session.GetRoleplay().UsingWeed_Bonus > 0) ? " (Weed Bonus: +" + Session.GetRoleplay().UsingWeed_Bonus + ")" : "";
                        Stats += "Stamina: " + Session.GetRoleplay().Stamina + "\n";
                        Stats += "Constitution: " + Session.GetRoleplay().Constitution + "\n";
                        Stats += "Intelligence: " + Session.GetRoleplay().Intelligence + "\n";
                        Stats += "Strength: " + Session.GetRoleplay().Strength + WeedBonus + "\n";
                        Stats += "Strength Bonus: " + Session.GetRoleplay().StrBonus + " " + Session.GetRoleplay().savedSTR + "\n";
                        Stats += "Cash on hand: " + Session.GetHabbo().Credits + "\n";
                        Stats += "Cash in bank: " + Session.GetRoleplay().Bank + "\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nItems carrying \n============================================\n";
                        Stats += "Weed: " + Session.GetRoleplay().Weed + " grams\n";
                        Stats += "Carrots: " + Session.GetRoleplay().Carrots + " carrots\n";
                        Stats += "Guns: ";
                        if (Session.GetRoleplay().Weapons.Count > 0)
                        {
                            foreach (KeyValuePair<string, Weapon> Weapon in Session.GetRoleplay().Weapons)
                            {
                                Stats += WeaponManager.GetWeaponName(Weapon.Key) + ", ";
                            }
                        }
                        else
                        {
                            Stats += "none";
                        };
                        Stats += "\n";
                        Stats += "Bullets: " + Session.GetRoleplay().Bullets + "\n";
                        Stats += "Bombs: " + Session.GetRoleplay().Bombs + "\n";
                        Stats += "Vests: " + Session.GetRoleplay().Vests + "\n";
                        if (Session.GetRoleplay().Car == 0) { Stats += "Car: None\n"; }
                        if (Session.GetRoleplay().Car == 21) { Stats += "Car: Skyblue\n"; }
                        if (Session.GetRoleplay().Car == 22) { Stats += "Car: Fireball\n"; }
                        if (Session.GetRoleplay().Car == 48) { Stats += "Car: Doggi\n"; }
                        if (Session.GetRoleplay().Car == 54) { Stats += "Car: Bunni\n"; }
                        if (Session.GetRoleplay().Car == 69) { Stats += "Car: Beetle\n"; }
                        Stats += "============================================\n";

                        Session.SendNotifWithScroll(Stats);

                        return true;
                    }
                #endregion
                #region :rpstats
                case "rpstats":
                case "yourstats":
                    {

                        string Target = "";
                        if (!Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :rpstats x");
                            return true;
                        }
                        else
                        {
                            Target = Convert.ToString(Params[1]);
                        }

                        GameClient TargetSession = RoleplayManager.GenerateSession(Target);


                        string Stats = "";
                        string isDead = (TargetSession.GetRoleplay().Dead) ? "Yes" : "No";
                        string isJailed = (TargetSession.GetRoleplay().Jailed) ? "Yes" : "No";
                        string GangName = (TargetSession.GetRoleplay().GangId > 0 && GangManager.validGang(TargetSession.GetRoleplay().GangId, TargetSession.GetRoleplay().GangRank)) ? GangManager.GangData[TargetSession.GetRoleplay().GangId].Name : "None";
                        string GangRank = (TargetSession.GetRoleplay().GangId > 0 && GangManager.validGang(TargetSession.GetRoleplay().GangId, TargetSession.GetRoleplay().GangRank)) ? GangManager.GangRankData[TargetSession.GetRoleplay().GangId, TargetSession.GetRoleplay().GangRank].Name : "None";

                        Stats += "Statistics For: " + TargetSession.GetHabbo().UserName + "\n";
                        Stats += "============================================\nGeneral Statistics \n============================================\n";
                        Stats += "Health: " + TargetSession.GetRoleplay().CurHealth + "/" + TargetSession.GetRoleplay().MaxHealth + "\n";
                        if (TargetSession.GetRoleplay().Armor >= 1)
                        {
                            Stats += "Armor: " + TargetSession.GetRoleplay().Armor + "\n";
                        }
                        if (TargetSession.GetRoleplay().Armor <= 0)
                        {
                            Stats += "Armor: 0\n";
                        }
                        Stats += "Energy: " + TargetSession.GetRoleplay().Energy + "/100\n";
                        Stats += "Hunger: " + TargetSession.GetRoleplay().Hunger + "/100\n";
                        Stats += "Hygiene: " + TargetSession.GetRoleplay().Hygiene + "/100\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nMisc Statistics \n============================================\n";
                        Stats += "Total Deaths: " + TargetSession.GetRoleplay().Deaths + "\n";
                        Stats += "Total Kills: " + TargetSession.GetRoleplay().Kills + "\n";
                        Stats += "Punch Kills: " + TargetSession.GetRoleplay().PunchKills + "\n";
                        Stats += "Melee Kills: " + TargetSession.GetRoleplay().MeleeKills + "\n";
                        Stats += "Gun Kills: " + TargetSession.GetRoleplay().GunKills + "\n";
                        Stats += "Bomb Kills: " + TargetSession.GetRoleplay().BombKills + "\n";
                        Stats += "Total Punches: " + TargetSession.GetRoleplay().Punches + "\n";
                        Stats += "Total Times Arrested: " + TargetSession.GetRoleplay().Arrested + "\n";
                        Stats += "Total Arrested Users: " + TargetSession.GetRoleplay().Arrests + "\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nSpace Mining & Farming Statistics \n============================================\n";
                        Stats += "Mining Level: " + TargetSession.GetRoleplay().SpaceLevel + "\n";
                        Stats += "Mining EXP: " + TargetSession.GetRoleplay().SpaceXP + "\n";
                        Stats += "Lumberjack Level: " + TargetSession.GetRoleplay().WoodLevel + "\n";
                        Stats += "Lumberjack EXP: " + TargetSession.GetRoleplay().WoodXP + "\n";
                        Stats += "Farming Level: " + Session.GetRoleplay().FarmingLevel + "\n";
                        Stats += "Farming EXP: " + Session.GetRoleplay().FarmingXP + "\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nInformative Statistics \n============================================\n";
                        Stats += "Currently Dead: " + isDead + "\n";
                        Stats += "Currently Jailed: " + isJailed + "\n";
                        if (TargetSession.GetRoleplay().Married_To != 0)
                        {
                            Stats += "Married To: " + RoleplayManager.ReturnOfflineInfo((uint)TargetSession.GetRoleplay().Married_To, "username") + "\n";
                        }
                        if (TargetSession.GetRoleplay().Married_To == 0)
                        {
                            Stats += "Married To: You are Single!\n";
                        }
                        Stats += "============================================\n";

                        Stats += "\n============================================\nEvents Statistics \n============================================\n";
                        Stats += "Brawl Points: " + TargetSession.GetRoleplay().Brawl_Pts + "\n";
                        Stats += "Infection Points: " + TargetSession.GetRoleplay().Infection_Pts + "\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nTimers Statistics \n============================================\n";
                        Stats += "Jail Timer: " + TargetSession.GetRoleplay().JailTimer + " mins\n";
                        Stats += "Dead Timer: " + TargetSession.GetRoleplay().DeadTimer + " mins\n";
                        Stats += "Workout Timer: " + TargetSession.GetRoleplay().WorkoutTimer_Done + "/" + TargetSession.GetRoleplay().WorkoutTimer_ToDo + "\n";
                        Stats += "WeightLift Timer: " + TargetSession.GetRoleplay().WeightLiftTimer_Done + "/" + TargetSession.GetRoleplay().WeightLiftTimer_ToDo + "\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nOther Statistics \n============================================\n";
                        Stats += "JobID: " + TargetSession.GetRoleplay().JobId + "\n";
                        Stats += "JobRank: " + TargetSession.GetRoleplay().JobRank + "\n";
                        Stats += "Gang: " + GangName + "\n";
                        Stats += "Gang Rank: " + GangRank + "\n";
                        Stats += "Gang Turf Bonus: Currently Disabled!\n";
                        Stats += "Last Killed User: " + TargetSession.GetRoleplay().LastKilled + "\n";
                        string WeedBonus = (TargetSession.GetRoleplay().UsingWeed_Bonus > 0) ? " (Weed Bonus: +" + TargetSession.GetRoleplay().UsingWeed_Bonus + ")" : "";
                        Stats += "Stamina: " + TargetSession.GetRoleplay().Stamina + "\n";
                        Stats += "Constitution: " + TargetSession.GetRoleplay().Constitution + "\n";
                        Stats += "Intelligence: " + TargetSession.GetRoleplay().Intelligence + "\n";
                        Stats += "Strength: " + TargetSession.GetRoleplay().Strength + WeedBonus + "\n";
                        Stats += "Cash on hand: " + TargetSession.GetHabbo().Credits + "\n";
                        Stats += "Cash in bank: " + TargetSession.GetRoleplay().Bank + "\n";
                        Stats += "============================================\n";

                        Stats += "\n============================================\nItems carrying \n============================================\n";
                        Stats += "Weed: " + TargetSession.GetRoleplay().Weed + " grams\n";
                        Stats += "Carrots: " + TargetSession.GetRoleplay().Carrots + " carrots\n";
                        Stats += "Guns: ";
                        if (TargetSession.GetRoleplay().Weapons.Count > 0)
                        {
                            foreach (KeyValuePair<string, Weapon> Weapon in TargetSession.GetRoleplay().Weapons)
                            {
                                Stats += WeaponManager.GetWeaponName(Weapon.Key) + ", ";
                            }
                        }
                        else
                        {
                            Stats += "none";
                        };
                        Stats += "\n";
                        Stats += "Bullets: " + TargetSession.GetRoleplay().Bullets + "\n";
                        Stats += "Bombs: " + TargetSession.GetRoleplay().Bombs + "\n";
                        Stats += "Vests: " + TargetSession.GetRoleplay().Vests + "\n";
                        if (TargetSession.GetRoleplay().Plane == 0) { Stats += "Plane: None\n"; }
                        if (TargetSession.GetRoleplay().Plane == 1) { Stats += "Plane: Purchased\n"; }
                        if (TargetSession.GetRoleplay().Car == 0) { Stats += "Car: None\n"; }
                        if (TargetSession.GetRoleplay().Car == 21) { Stats += "Car: Skyblue\n"; }
                        if (TargetSession.GetRoleplay().Car == 22) { Stats += "Car: Fireball\n"; }
                        if (TargetSession.GetRoleplay().Car == 48) { Stats += "Car: Doggi\n"; }
                        if (TargetSession.GetRoleplay().Car == 54) { Stats += "Car: Bunni\n"; }
                        if (TargetSession.GetRoleplay().Car == 69) { Stats += "Car: Beetle\n"; }
                        Stats += "============================================\n";

                        Session.SendNotifWithScroll(Stats);

                        return true;
                    }
                #endregion
                #region :911
                case "911":
                case "police":
                case "callpolice":
                case "policecall":
                    {

                        #region Conditions
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("call_police"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("call_police", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["call_police"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["call_police"] + "/5]");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("TURF"))
                        {
                            Session.SendWhisper("You cannot do this in the TURF room.");
                            return true;
                        }
                        if (RoleplayManager.PurgeTime)
                        {
                            Session.SendWhisper("You cannot do this during the purge.");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this while you are jailed!");
                            return true;
                        }

                        if (Session.GetRoleplay().IsNoob)
                        {
                            Session.SendWhisper("You cannot do this while you are a noob!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        Session.GetRoleplay().CallPolice();
                        Session.GetRoleplay().MultiCoolDown["call_police"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion
                #region :surrender
                case "surrender":
                    {

                        #region Conditions
                        if (Session.GetRoleplay().Wanted <= 0)
                        {
                            Session.SendWhisper("You are not wanted!");
                            return true;
                        }

                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this while dead!");
                            return true;
                        }

                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this while jailed!");
                            return true;
                        }

                        int Time = Session.GetRoleplay().Wanted;
                        #endregion

                        #region Execute
                        Session.Shout("*Surrenders to the law authorities and is escorted to the Jail*");
                        Session.GetRoleplay().Wanted = 0;
                        Session.GetRoleplay().SaveQuickStat("wanted", "0");

                        string wantedJunk = Session.GetHabbo().UserName.ToLower();
                        RoleplayManager.WantedListData.TryRemove(Session.GetHabbo().UserName.ToLower(), out wantedJunk);

                        RoomUser User = Session.GetHabbo().GetRoomUser();
                        User.ApplyEffect(0);
                        Session.GetRoleplay().Equiped = null;
                        User.Frozen = false;

                        Session.GetRoleplay().Cuffed = true;
                        Session.SendNotif("You have been arrested for " + Time + " minute(s)");
                        Session.GetRoleplay().JailFigSet = false;
                        Session.GetRoleplay().JailedSeconds = 60;
                        Session.GetRoleplay().JailTimer = Time;
                        Session.GetRoleplay().Jailed = true;
                        Session.GetRoleplay().Arrested++;
                        Session.GetRoleplay().UpdateStats++;

                        #endregion
                    }
                    return true;

                #endregion
                #region :changelog
                case "changelog":
                    {
                        string Changelog = "";
                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT * FROM changelog");
                            DataTable Table = dbClient.GetTable();
                            foreach (DataRow Row in Table.Rows)
                            {
                                Changelog += " - " + Row["details"] + "\n";
                            }
                        }

                        Session.SendNotifWithScroll(Changelog);

                        return true;
                    }
                #endregion
                #region :laws
                case "laws":
                case "lawinfo":
                    {

                        string laws = "";
                        DataRow Laws = null;

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT * FROM rp_lawinfo");
                            Laws = dbClient.GetRow();
                            laws = Laws["laws"].ToString();
                        }


                        Session.SendNotifWithScroll(laws);

                        return true;
                    }
                #endregion
                #region :corpinfo <corp>
                case "corpinfo":
                    {

                        #region Params/Vars
                        string corpinfo = "";
                        Job Job = null;
                        object Corp = null;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :corpinfo <corpname>");
                            return true;
                        }
                        else
                        {
                            bool isint = false;
                            int outt;


                            if (Int32.TryParse(Params[1].ToString(), out outt))
                            {
                                Corp = Convert.ToInt32(Params[1]);
                                isint = true;
                            }
                            else
                            {
                                Corp = Convert.ToString(Params[1]);
                                isint = false;
                            }


                            foreach (Job Jobb in JobManager.JobData.Values)
                            {
                                if (!isint)
                                {
                                    if (Jobb.Name.ToLower() != Convert.ToString(Corp).ToLower())
                                        continue;
                                }
                                else
                                {
                                    if (Jobb.Id != Convert.ToInt32(Corp))
                                        continue;
                                }

                                Job = Jobb;
                            }

                            if (Job == null)
                            {
                                Session.SendWhisper("The requested corporation was not found/does not exist!");
                                return true;
                            }
                        }
                        #endregion

                        #region Execute

                        corpinfo += "=========================\nGeneral Information Of: '" + Job.Name + "'\n=========================\n";
                        corpinfo += "CorpID: " + Job.Id + "\n";
                        corpinfo += "Corporation: " + Job.Name + "\n";
                        corpinfo += "Headquarters: " + Job.Headquarters + "\n";
                        corpinfo += "Owner: " + RoleplayManager.ReturnOfflineInfo((uint)Job.OwnerId, "username").ToString() + "\n";
                        corpinfo += "Balance: " + Job.Balance + "\n";

                        corpinfo += "=========================\nRank Information Of: '" + Job.Name + "'\n=========================\n";
                        foreach (Rank Rank in JobManager.JobRankData.Values)
                        {
                            if (Rank.JobId != Job.Id)
                                continue;
                            int countmem = 0;
                            corpinfo += "RankID: " + Rank.RankId + "\n";
                            corpinfo += "Rank Name: " + Rank.Name + "\n";
                            corpinfo += "Work Rooms: " + Rank.WorkRooms + "\n";
                            corpinfo += "Pay: " + Rank.Pay + "\n";
                            corpinfo += "Permissions(Fire,Hire,Promote,Demote): (" + Rank.canFire().ToString() + ", " + Rank.canHire().ToString() + ", " + Rank.canPromote().ToString() + ", " + Rank.canDemote().ToString() + ")\n";

                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("SELECT COUNT(*) FROM rp_stats WHERE job_id = " + Rank.JobId + " AND job_rank = " + Rank.RankId + "");
                                countmem = dbClient.GetInteger();
                            }

                            corpinfo += "Users in this rank: " + countmem + "\n\n";
                        }


                        Session.SendNotifWithScroll(corpinfo);

                        #endregion

                        return true;
                    }
                #endregion
                #region :taxi <roomid>
                case "taxi":
                    {

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :taxi <roomid>", false, 34);
                            return true;
                        }
                        if (!Plus.IsNum(Params[1]))
                        {
                            Session.SendWhisperBubble("Numbers only!", 34);
                            return true;
                        }
                        if (Convert.ToUInt32(Params[1]) == Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisperBubble("You are already in that room!", 34);
                            return true;
                        }
                        RoomUser User = Session.GetHabbo().GetRoomUser();
                        if (Session.GetRoleplay().inZombieInfection)
                        {
                            Session.SendWhisper("You cannot taxi because you are a participant of the Zombie Infection!", false, 34);
                            return true;
                        }
                        if (User.Frozen)
                        {
                            Session.SendWhisper("You cannot do this while you are stunned!", false, 34);
                            return true;
                        }
                        if (Session.GetRoleplay().RequestedTaxi)
                        {
                            Session.SendWhisper("You already called for a taxi! Please wait for it to arrive. If you would like to stop it type :stoptaxi", false, 34);
                            return true;
                        }
                        if (Session.GetRoleplay().CoolDown > 0)
                        {
                            Session.SendWhisper("You are cooling down! [" + Session.GetRoleplay().CoolDown + "/5]", false, 34);
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot call for a taxi while you are dead!", false, 34);
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot call for a taxi while you are jailed!", false, 34);
                            return true;
                        }
                        
                        #endregion

                        #region Execute

                        uint RoomId = Convert.ToUInt32(Params[1]);
                        string RoomName = "null";
                        string TaxiMsg = "You have requested a taxi to null";
                        string TaxiMsg2 = "*Calls a Taxi for null*";
                        Room Room = null;
                        int costCreditTaxi = 0;

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :taxi <roomid>");
                            return true;
                        }

                        Room = RoleplayManager.GenerateRoom(RoomId);


                        if (RoomId <= 0)
                        {
                            Session.SendWhisper("Invalid roomid: " + RoomId, false, 34);
                            return true;
                        }
                        if (Room == null)
                        {
                            Session.SendWhisper("There is a problem with the room you are trying to taxi to, so you cannot, sorry!", false, 34);
                            return true;
                        }
                        if (Room.RoomData == null)
                        {
                            Session.SendWhisper("There is a problem with the room you are trying to taxi to, so you cannot, sorry!", false, 34);
                            return true;
                        }
                        if (Room.RoomData.Description.Contains("NOTAXITO"))
                        {
                            Session.SendWhisper("You cannot call a taxi to that room. Looks like you're walking..", false, 34);
                            return true;
                        }

                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOTAXIFROM"))
                        {
                            Session.SendWhisper("Sorry, but you cannot taxi away from this room!", false, 34);
                            return true;
                        }

                        RoomName = Room.RoomData.Name;

                        if (!RoleplayManager.BypassRights(Session) || Session.GetHabbo().Rank == 12)
                        {
                            if (Session.GetRoleplay().Working && Session.GetRoleplay().JobHasRights("police"))
                            {
                                Session.GetHabbo().GetRoomUser().ApplyEffect(19);
                                TaxiMsg2 = "*Hops in their Police Car and drives to " + RoomName + " [" + RoomId + "]*";
                                Session.GetRoleplay().RequestedTaxi_WaitTime = 0;
                            }
                            else if (Session.GetRoleplay().Working && Session.GetRoleplay().JobHasRights("swat"))
                            {
                                Session.GetHabbo().GetRoomUser().ApplyEffect(19);
                                TaxiMsg2 = "*Hops in their S.W.A.T Van and drives to " + RoomName + " [" + RoomId + "]*";
                                Session.GetRoleplay().RequestedTaxi_WaitTime = 0;
                            }
                            else if (Session.GetRoleplay().Working && Session.GetRoleplay().JobHasRights("gov"))
                            {
                                TaxiMsg2 = "*Hops inside a Black Limo and gets driven to " + RoomName + " [" + RoomId + "]*";
                                Session.GetRoleplay().RequestedTaxi_WaitTime = 0;
                            }

                            else if (Session.GetHabbo().HasFuse("fuse_events") && Session.GetHabbo().Rank != 12)
                            {
                                TaxiMsg2 = "*Hops on their Staff Motorcycle and drives off to " + RoomName + " [" + RoomId + "]*";
                                Session.GetRoleplay().RequestedTaxi_WaitTime = 0;
                            }
                            else if (Session.GetHabbo().HasFuse("fuse_vip") || Session.GetHabbo().Rank == 12)
                            {
                                if (Session.GetHabbo().Credits < 2)
                                {
                                    Session.SendWhisper("You do not have enough credits to call for a taxi");
                                    return true;
                                }
                                costCreditTaxi = 2;
                                int wantedTime = (Session.GetRoleplay().Wanted > 0) ? 3 : 0;
                                TaxiMsg2 = "*Hops in their VIP Car and Drives to " + RoomName + " [" + RoomId + "] -$2*";
                                Session.GetRoleplay().RequestedTaxi_WaitTime = wantedTime;
                            }
                            else
                            {
                                if (Session.GetHabbo().Credits < 3)
                                {
                                    Session.SendWhisper("You do not have enough credits to call for a taxi");
                                    return true;
                                }
                                costCreditTaxi = 3;
                                TaxiMsg = "You have requested for a Taxi to " + RoomName + " [" + RoomId + "]";
                                TaxiMsg2 = "*Calls a Taxi for " + RoomName + " [" + RoomId + "] -$3*";
                                Session.GetRoleplay().RequestedTaxi_WaitTime = 5;
                            }
                        }
                        else
                        {
                            TaxiMsg2 = "*Calls a Super Taxi for " + RoomName + " [" + RoomId + "]*";
                            Session.GetRoleplay().RequestedTaxi_WaitTime = 0;
                        }
                        if (!TaxiMsg.ToLower().Contains("null"))
                        {
                            Session.SendWhisper(TaxiMsg);
                        }
                        if (TaxiMsg2.Length > 2)
                        {
                            RoleplayManager.Shout(Session, TaxiMsg2, 5);
                        }

                        Session.GetRoleplay().RequestedTaxi_Arrived = false;
                        Session.GetRoleplay().RecentlyCalledTaxi = true;
                        Session.GetRoleplay().RecentlyCalledTaxi_Timeout = 10;
                        Session.GetRoleplay().RequestedTaxiDestination = Room;
                        Session.GetRoleplay().RequestedTaxi = true;
                        Session.GetRoleplay().taxiTimer = new taxiTimer(Session);
                        Session.GetHabbo().Credits = Session.GetHabbo().Credits - costCreditTaxi;
                        Session.GetHabbo().UpdateCreditsBalance();

                        #endregion

                        return true;
                    }
                #endregion
                #region :stoptaxi
                case "stoptaxi":
                case "canceltaxi":
                    {
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisperBubble("You cannot use this command while dead!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisperBubble("You cannot use this command while jailed!");
                            return true;
                        }
                        if (!Session.GetRoleplay().RequestedTaxi)
                        {
                            Session.SendWhisper("You did not request a taxi in the first place!");
                            return true;
                        }
                        else
                        {
                            Session.GetRoleplay().RequestedTaxi = false;
                            Session.GetRoleplay().RequestedTaxi_Arrived = false;
                            Session.GetRoleplay().RequestedTaxiDestination = null;
                            Session.GetRoleplay().RecentlyCalledTaxi = false;
                            Session.Shout("*Uses their phone to cancel their taxi request*");

                            if (Session.GetRoleplay().taxiTimer != null)
                            {
                                Session.GetRoleplay().taxiTimer.stopTimer();
                            }

                            if (Session.GetRoleplay().Working && Session.GetRoleplay().JobHasRights("police"))
                            {
                                
                            }
                            else if (Session.GetRoleplay().Working && Session.GetRoleplay().JobHasRights("swat"))
                            {
                                
                            }
                            else if (Session.GetRoleplay().Working && Session.GetRoleplay().JobHasRights("gov"))
                            {
                                
                            }

                            else if (Session.GetHabbo().HasFuse("fuse_events") && Session.GetHabbo().Rank != 12)
                            {
                               
                            }
                            else if (Session.GetHabbo().HasFuse("fuse_vip") || Session.GetHabbo().Rank == 12)
                            {
                                Session.GetHabbo().Credits += 2;
                            }
                            else
                            {
                                Session.GetHabbo().Credits += 3;
                            }
                            Session.GetHabbo().UpdateCreditsBalance();

                        }

                        return true;
                    }
                #endregion

                #endregion

                #region General Non Roleplay

                #region :hotrooms
                case "hotrooms":
                case "popularrooms":
                    {
                        string Rooms = "";

                        foreach (Room Room in Plus.GetGame().GetRoomManager().LoadedRooms.Values.ToList().OrderByDescending(key => key.UserCount))
                        {
                            if (Room.UserCount <= 0)
                                continue;

                            Rooms += "Room ID: " + Room.RoomId + "\n";
                            Rooms += "Room name: " + Room.RoomData.Name + "\n";
                            Rooms += "Users now: " + Room.UserCount + "\n\n";
                        }

                        Session.SendNotifWithScroll(Rooms);

                        return true;
                    }
                #endregion

                #region :gamemap
                case "gamemap":
                case "map":
                    {
                        #region Conditions

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("gamemap_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("gamemap_cooldown", 0);
                        }

                        if (Session.GetRoleplay().MultiCoolDown["gamemap_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["gamemap_cooldown"] + "/10]");
                            return true;
                        }

                        if (Session.GetHabbo().Credits < 10)
                        {
                            Session.SendWhisper("You need at least $10 to use this command.");
                            return true;
                        }

                        #endregion

                        #region Execute
                        DataTable Ruumz = null;
                        string Rooms = "";

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `id`,`caption`,`users_now`,`description` FROM `rooms_data` ORDER BY `id` ASC");
                            Ruumz = dbClient.GetTable();
                            foreach (DataRow Room in Ruumz.Rows)
                            {
                                string TaxiRoom = (Room["description"].ToString().Contains("NOTAXITO")) ? "No" : "Yes";
                                Rooms += "Room ID: " + Room["id"] + "\n";
                                Rooms += "Room name: " + Room["caption"] + "\n";
                                Rooms += "Taxiable: " + TaxiRoom + "\n\n";
                            }
                        }

                        Session.SendNotifWithScroll(Rooms);
                        RoleplayManager.Shout(Session, "*Opens up the city Map*", 29);
                        //RoleplayManager.GiveMoney(Session, -10);
                        //Session.SendWhisper("Thank you for checking out the map! $10 has been deducted from your balance.");
                        Session.GetRoleplay().MultiCoolDown["gamemap_cooldown"] = 10;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion

                #region :whosonline
                case "whosonline":
                case "online":
                case "who":
                    {


                        string Online = "Users Online: \n";
                        int onli = 0;

                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (client == null || client.GetHabbo() == null) { continue; }
                                Online += "" + client.GetHabbo().UserName + "\n";
                                onli++;
                            }
                        }

                        Session.SendNotifWithScroll("[" + onli + "] " + Online);

                        return true;
                    }
                #endregion

                #endregion

                #region Apartments

                #region :buyapartment
                case "buyapartment":
                    {
                        if (!ApartmentManager.isApartment(Session.GetHabbo().GetRoomUser().RoomId))
                        {
                            Session.SendWhisper("The room you are in is not an apartment!");
                            return true;
                        }
                        if (!ApartmentManager.Apartments[Session.GetHabbo().GetRoomUser().RoomId].ForSale)
                        {
                            Session.SendWhisper("The apartment you are in is not for sale!");
                            return true;
                        }
                        if (!Session.GetHabbo().HasFuse("fuse_vip") && ApartmentManager.Apartments[Session.GetHabbo().GetRoomUser().RoomId].VIP)
                        {
                            Session.SendWhisper("The apartment you are purchasing is VIP only!");
                            return true;
                        }
                        return true;
                    }
                #endregion

                #endregion

                #region Selling / Barter

                #region :give x
                case "give":
                    {

                        #region Generate Instances / Sessions
                        string Target = null;
                        int Amount = 0;
                        GameClient TargetSession = null;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :give <user> <amount>", 34);
                            return true;
                        }
                        else
                        {

                            if (!Plus.IsNum(Params[2]))
                            {
                                Session.SendWhisperBubble("A entrada deve ser um número!", 34);
                                return true;
                            }

                            Target = Convert.ToString(Params[1]);
                            Amount = Convert.ToInt32(Params[2]);
                        }


                        TargetSession = RoleplayManager.GenerateSession(Target);


                        if (!RoleplayManager.CanInteract(Session, TargetSession, true))
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado nesta sala!", 34);
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            Session.SendWhisperBubble("Builders cannot give out coins!", 34);
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_events") && !Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("EVENTS"))
                            {
                                Session.SendWhisper("Events can only give coins as an event prize!");
                                return true;
                            }
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("give_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("give_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["give_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["give_cooldown"] + "/5]");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :give <username> <amount>");
                            return true;
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala");
                            return true;
                        }
                        if (Amount <= 0)
                        {
                            Session.SendWhisper("Invalid amount..");
                            return true;
                        }
                        if (Session.GetHabbo().Credits < Amount)
                        {
                            Session.SendWhisper("You do not have $" + Amount + "!");
                            return true;
                        }
                        if (TargetSession == Session)
                        {
                            Session.SendWhisper("You cannot give yourself money..");
                            return true;
                        }
                        if (Session.GetRoleplay().JobId <= 1)
                        {
                            Session.SendWhisper("You must have a job to give money!");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().IsNoob)
                        {
                            Session.SendWhisper("You cannot give money to a new user!");
                            return true;
                        }

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT ip_last FROM users WHERE id=" + TargetSession.GetHabbo().Id + " LIMIT 1");
                            string TIP = dbClient.GetString();

                            dbClient.SetQuery("SELECT ip_last FROM users WHERE id=" + Session.GetHabbo().Id + " LIMIT 1");
                            string MIP = dbClient.GetString();

                            if (TIP == MIP)
                            {
                                Session.SendWhisper("Our system has detected that you attempted to boost credits, this action has been logged & repeated offences will result in a ban!");
                                return true;
                            }
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.GiveMoney(TargetSession, +Amount);
                        RoleplayManager.GiveMoney(Session, -Amount);
                        RoleplayManager.Shout(Session, "*Hands $" + Amount + " to " + TargetSession.GetHabbo().UserName + "*", 4);
                        Session.GetRoleplay().MultiCoolDown["give_cooldown"] = 3;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion

                #region :offer x <item>
                case "offer":
                    {
                        #region Generate Instances / Sessions
                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :offer x <item>");
                            return true;
                        }
                        string Target = Convert.ToString(Params[1]);
                        string Offer = Convert.ToString(Params[2]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("offer_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("offer_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["offer_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["offer_cooldown"] + "/5]");
                            return true;
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :offer <username> <item>");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().OfferData.ContainsKey(Offer.ToLower()))
                        {
                            Session.SendWhisper("They have already been offered this item and are awaiting the #accept/#decline");
                            return true;
                        }
                        #endregion

                        #region Execute

                        if (WeaponManager.isWeapon(Offer.ToLower()))
                        {

                            if (!JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                            {
                                Session.SendWhisper("Seu trabalho não pode fazer isso!", false, 34);
                                return true;
                            }

                            if (!Session.GetRoleplay().JobHasRights("ammunation"))
                            {
                                Session.SendWhisper("Seu trabalho não pode fazer isso!", false, 34);
                                return true;
                            }
                            if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("AMMUNATION"))
                            {
                                Session.SendWhisper("This is not your work room!");
                                return true;
                            }
                            if (WeaponManager.WeaponsData[Offer].Price > TargetSession.GetHabbo().Credits)
                            {
                                Session.SendWhisperBubble("This user cannot afford this weapon!", 1);
                                return true;
                            }

                            Session.Shout("*Offers " + TargetSession.GetHabbo().UserName + " 1 " + Offer.ToLower() + " for $" + WeaponManager.WeaponsData[Offer.ToLower()].Price + "*", 4, true);


                            TargetSession.SendWhisper("You have been offered a " + Offer.ToLower() + " for $" + WeaponManager.WeaponsData[Offer.ToLower()].Price + ". Type #accept to accept or #deny to deny!", false, 1, true);

                            TargetSession.GetRoleplay().WeaponOfferedSell = Offer.ToLower();
                            TargetSession.GetRoleplay().WeaponOffered = true;
                            TargetSession.GetRoleplay().OfferData.Add(Offer.ToLower(), new Offer(Session, Offer.ToLower(), 1, WeaponManager.WeaponsData[Offer.ToLower()].Price));


                        }
                        else
                        {
                            if (!JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                            {
                                Session.SendWhisper("Seu trabalho não pode fazer isso!", false, 34);
                                return true;
                            }


                            switch (Offer)
                            {
                                case "bbj":
                                    {
                                        #region Bubble Juice

                                        if (!Session.GetRoleplay().JobHasRights("bbj") && Session.GetHabbo().Rank <= 2)
                                        {
                                            Session.SendWhisperBubble("Seu trabalho não pode fazer isso!", 1);
                                            return true;
                                        }
                                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("BBJ"))
                                        {
                                            Session.SendWhisperBubble("This is not your work room!", 1);
                                            return true;
                                        }
                                        if (TargetSession.GetHabbo().Credits < Substances.SubstanceData["md_limukaappi"].Item_Price)
                                        {
                                            Session.SendWhisperBubble("This user cannot afford this!", 1);
                                            return true;
                                        }
                                        RoleplayManager.Shout(Session, "*Offers " + TargetSession.GetHabbo().UserName + " a BubbleJuice Cola for $" + Substances.SubstanceData["md_limukaappi"].Item_Price + "*", 4);
                                        TargetSession.SendWhisperBubble(Session.GetHabbo().UserName + " has offered you a BubbleJuice Cola for $" + Substances.SubstanceData["md_limukaappi"].Item_Price + ". Type #accept to accept or #deny to deny!", 1);
                                        TargetSession.GetRoleplay().OfferData.Add("bbj", new Offer(Session, "bbj", 1, Substances.SubstanceData["md_limukaappi"].Item_Price));
                                        #endregion
                                    }
                                    break;

                                case "icecream":
                                case "icm":
                                    {
                                        #region Icecream
                                        if (!Session.GetRoleplay().JobHasRights("icecream") && Session.GetHabbo().Rank <= 2)
                                        {
                                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                                            return true;
                                        }
                                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("ICECREAM"))
                                        {
                                            Session.SendWhisper("This is not your work room!");
                                            return true;
                                        }
                                        if (TargetSession.GetHabbo().Credits < Substances.SubstanceData["rare_icecream*1"].Item_Price)
                                        {
                                            Session.SendWhisperBubble("This user cannot afford this!", 1);
                                            return true;
                                        }
                                        RoleplayManager.Shout(Session, "*Offers " + TargetSession.GetHabbo().UserName + " a Vanilla Icecream for $" + Substances.SubstanceData["rare_icecream*1"].Item_Price + "*", 4);
                                        TargetSession.SendWhisperBubble(Session.GetHabbo().UserName + " has offered you a Vanilla Icecream for $" + Substances.SubstanceData["rare_icecream*1"].Item_Price + ". Type #accept to accept or #deny to deny!", 1);

                                        if (TargetSession.GetRoleplay().OfferData.ContainsKey("icecream"))
                                        {
                                            TargetSession.GetRoleplay().OfferData.Remove("icecream");
                                        }

                                        Offer OfferIcm = new Offer(Session, "icecream", 1, Substances.SubstanceData["rare_icecream*1"].Item_Price);

                                        TargetSession.GetRoleplay().OfferData.Add("icecream", OfferIcm);


                                        #endregion
                                    }
                                    break;

                                case "weed":
                                    {
                                        #region Weed

                                        if (Session.GetRoleplay().Weed < 5)
                                        {
                                            Session.SendWhisper("You must have at least 5g of weed to offer it to anybody!");
                                            return true;
                                        }
                                        if (TargetSession.GetHabbo().Credits < 200)
                                        {
                                            Session.SendWhisperBubble("This user cannot afford this!", 1);
                                            return true;
                                        }
                                        RoleplayManager.Shout(Session, "*Offers " + TargetSession.GetHabbo().UserName + " 5g of weed for $200*", 4);
                                        TargetSession.SendWhisperBubble(Session.GetHabbo().UserName + " has offered you 5g of weed for $200. Type #accept to accept or #deny to deny!", 1);

                                        Offer OfferWeed = new Offer(Session, "weed", 5, 200);

                                        if (TargetSession.GetRoleplay().OfferData.ContainsKey("weed"))
                                        {
                                            TargetSession.GetRoleplay().OfferData.Remove("weed");
                                        }

                                        TargetSession.GetRoleplay().OfferData.Add("weed", OfferWeed);

                                        #endregion
                                    }
                                    break;

                                case "carrot":
                                case "carrots":
                                    {
                                        #region Carrots

                                        if (Session.GetRoleplay().Carrots < 3)
                                        {
                                            Session.SendWhisper("You must have at least 3 carrots to offer it to anybody!");
                                            return true;
                                        }
                                        if (TargetSession.GetHabbo().Credits < 100)
                                        {
                                            Session.SendWhisperBubble("This user cannot afford this!", 1);
                                            return true;
                                        }
                                        RoleplayManager.Shout(Session, "*Offers " + TargetSession.GetHabbo().UserName + " 3 carrots for $100*", 4);
                                        TargetSession.SendWhisperBubble(Session.GetHabbo().UserName + " has offered you 3 carrots for $100. Type #accept to accept or #deny to deny!", 1);

                                        Offer OfferCarrots = new Offer(Session, "carrots", 5, 100);

                                        if (TargetSession.GetRoleplay().OfferData.ContainsKey("carrots"))
                                        {
                                            TargetSession.GetRoleplay().OfferData.Remove("carrots");
                                        }

                                        TargetSession.GetRoleplay().OfferData.Add("carrots", OfferCarrots);

                                        #endregion
                                    }
                                    break;

                                case "phone":
                                    {
                                        #region Phone
                                        if (!Session.GetRoleplay().JobHasRights("phone") && Session.GetHabbo().Rank <= 2)
                                        {
                                            Session.SendWhisper("You do not have permission for phone.");
                                            return true;
                                        }
                                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("PHONE"))
                                        {
                                            Session.SendWhisper("You must be in the Phone Store [Room ID: 5] to buy a phone.");
                                            return true;
                                        }

                                        if (TargetSession.GetRoleplay().Phone == 1)
                                        {
                                            Session.SendWhisper("Aye, nobody needs this many phones they already have one!");
                                            return true;
                                        }
                                        RoleplayManager.Shout(Session, "*Offers " + TargetSession.GetHabbo().UserName + " a Nokia phone for $20*", 4);
                                        TargetSession.SendWhisperBubble(Session.GetHabbo().UserName + " has offered you a Nokia Phone for $20*. Type #accept to accept or #deny to deny!", 1);

                                        Offer OfferPhone = new Offer(Session, "phone", 1, 20);

                                        TargetSession.GetRoleplay().OfferData.Add("phone", OfferPhone);

                                        #endregion
                                    }
                                    break;

                                default:
                                    Session.SendWhisperBubble("The offer item " + Offer + " does not exist!", 1);
                                    break;
                            }
                        }

                        Session.GetRoleplay().MultiCoolDown["offer_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion

                #region :us
                case "use":
                    {
                        string item = Params[1].ToString();

                        switch (item)
                        {
                            case "edrink":
                            case "energydrink":
                                #region Conditions
                                HybridDictionary edrink = Session.GetHabbo().GetInventoryComponent().getEnergyd();

                                if (edrink.Count <= 0)
                                {
                                    Session.SendWhisper("You have no energy drinks!");
                                    break;
                                }
                                if (Session.GetRoleplay().Energy > 100)
                                {
                                    Session.SendWhisper("You are already full of energy!");
                                    return true;
                                }
                                #endregion
                                #region excute
                                {

                                    int newenergy = (Session.GetRoleplay().Energy + 15 >= 100) ? 100 : Session.GetRoleplay().Energy + 15;

                                    Session.GetRoleplay().Energy = newenergy;
                                    Session.GetRoleplay().UpdateStats += 2;

                                    RoleplayManager.Shout(Session, "*Drinks an energy drink [+15E]*", 14);
                                    useManager.removeInventoryDrinks(Session, edrink);


                                    break;
                                }
#endregion
                         

                            case "medikit":
                            case "medkit":
                            case "mk":
                                #region Conditions
                                HybridDictionary medi = Session.GetHabbo().GetInventoryComponent().getMedi();

                                if (medi.Count <= 0)
                                {
                                    Session.SendWhisper("You have no medi kits!");
                                    break;
                                }
                                if (Session.GetRoleplay().CurHealth >= Session.GetRoleplay().MaxHealth)
                                {
                                    Session.SendWhisperBubble("Your health is already full!", 1);
                                    return true;
                                }
                                #endregion
                                #region excute
                                {
                                    Session.GetRoleplay().UsingMedkit = true;
                                    Session.GetRoleplay().mediTimer = new mediTimer(Session);

                                    RoleplayManager.Shout(Session,"*Uses a medkit to heal their wounds *", 14);
                                    useManager.removeInventorykits(Session, medi);

                                    break;
                                }
                            #endregion
                            case "brotein":
                                #region Conditions
                                HybridDictionary brotein = Session.GetHabbo().GetInventoryComponent().getBrotein();

                                if (brotein.Count <= 0)
                                {
                                    Session.SendWhisper("You have no brotein!");
                                    break;
                                }
                                if (Session.GetRoleplay().StrBonus)
                                {
                                    Session.SendWhisper("You have a strength bonus currently active");
                                    break;
                                }
                                #endregion
                                #region excute
                                {
                                    Session.GetRoleplay().savedSTR = 0;
                                    Session.GetRoleplay().StrBonus = true;
                                    Session.GetRoleplay().broteinTimer = new broteinTimer(Session);

                                    RoleplayManager.Shout(Session,
                                        "* Uses Brotein and feels their muscles getting pumped [+5 STR]*", 6);
                                    useManager.removeInventoryBrotein(Session, brotein);

                                    break;
                                }
                            #endregion
                            case "help":
                                {
                                    string use = "";
                                    use += " Help page for :use command\n";
                                    use += " :use medikit - restores your HP\n";
                                    use += " :use edrink - restore some energy!\n";
                                    use += " :use brotein - gives you a temporary strength boost!\n";
                                    Session.SendNotifWithScroll(use);

                                    break;
                                }
                        }
                        return true;
                    }
                #endregion
                #region :sell rocks/wood (skilling)
                case "sell":
                    {
                        string item = Params[1].ToString();

                        switch (item)
                        {
                            case "rocks":
                                {
                                    #region Declares
                                    HybridDictionary rocks = Session.GetHabbo().GetInventoryComponent().getRocks();
                                    RoomUser user = Session.GetHabbo().GetRoomUser();
                                    #endregion

                                    #region Conditions
                                    if (rocks.Count <= 0)
                                    {
                                        Session.SendWhisper("You have no rocks to sell!");
                                        break;
                                    }

                                    if (!spaceManager.userOnSellPoint(user))
                                    {
                                        Session.SendWhisper("You are not on the sell point! Head to the space station and try again.");
                                        break;

                                        // Redo this when space room dun
                                        //BOOKMARK
                                    }
                                    #endregion

                                    #region Execute
                                    int totalCoins = rocks.Count * (5 + Session.GetRoleplay().SpaceLevel);

                                    Session.GetHabbo().Credits += totalCoins;
                                    Session.GetHabbo().UpdateCreditsBalance();
                                    Session.SendWhisperBubble("You sold " + rocks.Count + " rocks for: " + totalCoins.ToString() + " coins!", 1);
                                    RoleplayManager.Shout(Session,
                                        "*Sells " + rocks.Count + " rocks for " + totalCoins.ToString() + " coins [+$" + totalCoins.ToString() + "]*", 4);
                                    spaceManager.removeInventoryRocks(Session, rocks);
                                    #endregion
                                    break;
                                }

                            case "wood":
                                {
                                    #region Declares
                                    HybridDictionary trees = Session.GetHabbo().GetInventoryComponent().getTrees();
                                    RoomUser user = Session.GetHabbo().GetRoomUser();
                                    #endregion

                                    #region Conditions
                                    if (trees.Count <= 0)
                                    {
                                        Session.SendWhisper("You have no wood to sell!");
                                        break;
                                    }

                                    if (!woodManager.userOnSellPoint(user))
                                    {
                                        Session.SendWhisper("You are not on the sell point!");
                                        break;

                                        // Redo this when space room dun
                                        //BOOKMARK
                                    }
                                    #endregion

                                    #region Execute
                                    int totalCoins = trees.Count * (5 + Session.GetRoleplay().SpaceLevel);

                                    Session.GetHabbo().Credits += totalCoins;
                                    Session.GetHabbo().UpdateCreditsBalance();
                                    Session.SendWhisperBubble("You sold " + trees.Count + " wood chunks for: " + totalCoins.ToString() + " coins!", 1);
                                    RoleplayManager.Shout(Session,
                                        "*Sells " + trees.Count + " wood chunk(s) for " + totalCoins.ToString() + " coins [+$" + totalCoins.ToString() + "]*", 6);
                                    woodManager.removeInventoryTrees(Session, trees);
                                    #endregion
                                    break;
                                }
                        }
                        return true;
                    }
                #endregion

               

                #region :sellweapon x <item> (DISABLED)
                /*case "offerweapon":
                case "offerwep":
                case "sellweapon":
                    {

                        #region Generate Instances / Sessions
                        if (!Misc.ParamsMet(Params, 3))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :offerweapon x <weapon> <amount>");
                            return true;
                        }
                        string Target = Convert.ToString(Params[1]);
                        string Offer = Convert.ToString(Params[2]);
                        int Amount = Convert.ToInt32(Params[3]);
                        GameClient TargetSession = null;
                        TargetSession = Misc.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("offerwep_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("offerwep_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["offerwep_cooldown"] > 0 && !Misc.BypassRights(Session))
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["offerwep_cooldown"] + "/30]");
                            return true;
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        if(Amount <= 0)
                        {
                            return true;
                        }
                        Dictionary<string, Weapon> Weaponss = new Dictionary<string, Weapon>();
                        Dictionary<string, Weapon> TWeaponss = new Dictionary<string, Weapon>();
                        Weaponss.Clear();
                        TWeaponss.Clear();
                        foreach (KeyValuePair<string, Weapon> Weapon in Session.GetRoleplay().Weapons)
                        {
                            Weaponss.Add(weaponManager.GetWeaponName(Weapon.Key), Weapon.Value);

                           // Console.WriteLine(weaponManager.GetWeaponName(Weapon.Key));
                        }
                        foreach (KeyValuePair<string, Weapon> Weapon in TargetSession.GetRoleplay().Weapons)
                        {
                            TWeaponss.Add(weaponManager.GetWeaponName(Weapon.Key), Weapon.Value);
                        }
                        if (Session.GetRoleplay().Equiped.ToLower() == Offer)
                        {
                            Session.SendWhisper("You must unequip your " + Offer + " first before selling it..");
                            return true;
                        }
                        #endregion

                        #region Execute

                        if (!Weaponss.ContainsKey(Offer))
                        {
                            Session.SendWhisper("You do not have a " + Offer);
                            return true;
                        }

                        if(TWeaponss.ContainsKey(Offer))
                        {
                            Session.SendWhisper("This user already has a " + Offer);
                            return true;
                        }

                        Session.Shout("*Offers to sell " + TargetSession.GetHabbo().UserName + " their " + weaponManager.WeaponsData[Offer.ToLower()].Name + " for $" + Amount + "*");
                        TargetSession.SendWhisper(Session.GetHabbo().UserName + " has offered to sell you their " + weaponManager.WeaponsData[Offer.ToLower()].Name + " for $" + Amount + ". Type #accept to accept or #deny to deny!");
                        TargetSession.SendWhisper("!! Ensure that you have checked the price listed above carefully, and make sure that you are not being RIPPED OFF! PROCEED AT YOUR OWN RISK!");
                        TargetSession.GetRoleplay().WeaponOfferedSell = weaponManager.WeaponsData[Offer.ToLower()].Name;
                        TargetSession.GetRoleplay().WeaponOfferedSell_By = Session;
                        TargetSession.GetRoleplay().WeaponOffered = true;
                        TargetSession.GetRoleplay().WeaponOfferedPrice = Amount;

                        Session.GetRoleplay().MultiCoolDown["offerwep_cooldown"] = 30;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }*/
                #endregion

                #endregion

                #region Staff Misc / Builders

                #region :banvipa
                case "banvipa":
                case "banvip":
                    {

                        #region Conditions
                        string Target;
                        GameClient TargetSession;
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("You cannot do this!");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :banvipa <user>");
                            return true;
                        }
                        Target = Convert.ToString(Params[1]);
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        if (!RoleplayManager.CanInteract(Session, TargetSession, false))
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().BannedFromVIPAlert)
                        {
                            Session.SendWhisper("This user is already banned from sending VIP alerts!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        TargetSession.GetRoleplay().BannedFromVIPAlert = true;
                        RoleplayManager.sendVIPAlert(Session, "*" + Session.GetHabbo().UserName + " has banned " + TargetSession.GetHabbo().UserName + " from sending VIP alerts*");
                        TargetSession.SendNotif(Session.GetHabbo().UserName + " has banned you from sending VIP alerts!");
                        string banned = TargetSession.GetRoleplay().BannedFromVIPAlert == true ? "1" : "0";
                        TargetSession.GetRoleplay().SaveQuickStat("vip_a_banned", banned);
                        #endregion

                        return true;
                    }
                #endregion

                #region :unbanvipa
                case "unbanvipa":
                case "unbanvip":
                    {

                        #region Conditions
                        string Target;
                        GameClient TargetSession;
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("You cannot do this!");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :unbanvipa <user>");
                            return true;
                        }
                        Target = Convert.ToString(Params[1]);
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        if (!RoleplayManager.CanInteract(Session, TargetSession, false))
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }
                        if (!TargetSession.GetRoleplay().BannedFromVIPAlert)
                        {
                            Session.SendWhisper("This user is already not banned from sending VIP alerts!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        TargetSession.GetRoleplay().BannedFromVIPAlert = false;
                        RoleplayManager.sendVIPAlert(Session, "*" + Session.GetHabbo().UserName + " has UN-banned " + TargetSession.GetHabbo().UserName + " from sending VIP alerts*");
                        TargetSession.SendNotif(Session.GetHabbo().UserName + " has UN-banned you from sending VIP alerts!");

                        string banned = TargetSession.GetRoleplay().BannedFromVIPAlert == true ? "1" : "0";
                        TargetSession.GetRoleplay().SaveQuickStat("vip_a_banned", banned);
                        #endregion

                        return true; ;
                    }
                #endregion

                #region :debugfurni
                case "debugfurni":
                    {
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }

                        string from = "";
                        string to = "";
                        from = Session.GetRoleplay().Debug_Furni.ToString();
                        Session.GetRoleplay().Debug_Furni = !Session.GetRoleplay().Debug_Furni;
                        to = Session.GetRoleplay().Debug_Furni.ToString();
                        Session.SendWhisper(from + " -> " + to);
                        return true;
                    }
                #endregion

                #region :fixweapons <user>
                case "refreshuweapons":
                case "fixweapons":
                case "refreshweps":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :fixweapons <username>");
                            return true;
                        }

                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        if (TargetSession == null)
                        {
                            return true;
                        }

                        Session.SendWhisperBubble("Successfully refreshed " + TargetSession.GetHabbo().UserName + "'s weapons!", 1);
                        TargetSession.SendWhisperBubble("An administrator has updated your weapons!", 1);
                        TargetSession.GetRoleplay().RefreshWeapons();
                        return true;
                    }
                #endregion

                #endregion

                #region General Roleplay


                #region :eat
                case "eat":
                    {


                        #region Variables

                        RoomUser User = Session.GetHabbo().GetRoomUser();

                        RoomItem Food = null;
                        #endregion

                        #region Conditions
                        foreach (RoomItem Item in Session.GetHabbo().CurrentRoom.GetRoomItemHandler().FloorItems.Values)
                        {
                            if (Substances.IsAFood(Item) && RoleplayManager.Distance(new Vector2D(Item.X, Item.Y), new Vector2D(User.X, User.Y)) <= 1)
                            {
                                Food = Item;
                            }

                        }

                        if (Food == null)
                        {
                            Session.SendWhisper("There is no food near you to eat!");
                            return true;
                        }

                        if (Session.GetRoleplay().Hunger <= 0)
                        {
                            Session.SendWhisper("You are already full!");
                            return true;
                        }


                        #endregion

                        #region Execute

                        Food FoodItem = Substances.GetFoodById(Food.BaseItem);

                        if (FoodItem.BaseItemId == 0)
                        {
                            Session.SendWhisper("You cannot eat this!");
                            return true;
                        }

                        RoleplayManager.ReplaceItem(Session, Food, "diner_tray_0");

                        int newhunger = (Session.GetRoleplay().Hunger - FoodItem.Hunger <= 0) ? 0 : Session.GetRoleplay().Hunger - FoodItem.Hunger;
                        int newhealth = (Session.GetRoleplay().CurHealth + FoodItem.Health >= 100) ? 100 : Session.GetRoleplay().CurHealth + FoodItem.Hunger;
                        int newenergy = (Session.GetRoleplay().Energy + FoodItem.Energy >= 100) ? 100 : Session.GetRoleplay().Energy + FoodItem.Energy;
                        // Session.SendWhisper("" + FoodItem.Hunger + "||" + FoodItem.UniqueName);
                        Session.GetRoleplay().Hunger = newhunger;
                        Session.GetRoleplay().CurHealth = newhealth;
                        Session.GetRoleplay().Energy = newenergy;

                        Session.GetRoleplay().UpdateStats += 2;

                        Session.Shout("*Eats more of the food*");
                        Session.Shout(FoodItem.Speech);

                        #endregion


                        return true;
                    }
                #endregion

                #region :eat
                case "drink":
                    {


                        #region Variables

                        RoomUser User = Session.GetHabbo().GetRoomUser();

                        RoomItem Food = null;
                        #endregion

                        #region Conditions
                        foreach (RoomItem Item in Session.GetHabbo().CurrentRoom.GetRoomItemHandler().FloorItems.Values)
                        {
                            if (Substances.IsAFood(Item) && RoleplayManager.Distance(new Vector2D(Item.X, Item.Y), new Vector2D(User.X, User.Y)) <= 1)
                            {
                                Food = Item;
                            }

                        }

                        if (Food == null)
                        {
                            Session.SendWhisper("There is no drink near you!");
                            return true;
                        }

                        if (Session.GetRoleplay().Energy == 100)
                        {
                            Session.SendWhisper("You already have enough energy");
                            return true;
                        }


                        #endregion

                        #region Execute

                        Food FoodItem = Substances.GetFoodById(Food.BaseItem);

                        if (FoodItem.BaseItemId < 2840)
                        {
                            Session.SendWhisper("You cannot drink this!");
                            return true;
                        }

                        RoleplayManager.ReplaceItem(Session, Food, "diner_tray_0");

                        int newhunger = (Session.GetRoleplay().Hunger - FoodItem.Hunger <= 0) ? 0 : Session.GetRoleplay().Hunger - FoodItem.Hunger;
                        int newhealth = (Session.GetRoleplay().CurHealth + FoodItem.Health >= 100) ? 100 : Session.GetRoleplay().CurHealth + FoodItem.Hunger;
                        int newenergy = (Session.GetRoleplay().Energy + FoodItem.Energy >= 100) ? 100 : Session.GetRoleplay().Energy + FoodItem.Energy;
                        // Session.SendWhisper("" + FoodItem.Hunger + "||" + FoodItem.UniqueName);
                        Session.GetRoleplay().Hunger = newhunger;
                        Session.GetRoleplay().CurHealth = newhealth;
                        Session.GetRoleplay().Energy = newenergy;

                        Session.GetRoleplay().UpdateStats += 2;

                        Session.Shout(FoodItem.Speech);

                        #endregion


                        return true;
                    }
                #endregion

                #region :marry

                case "marry":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("CHURCH"))
                        {
                            Session.SendWhisper("You must be in a church to wed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Married_To != 0)
                        {
                            Session.SendWhisper("Sorry, you're already married to someone.");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :marry <username>");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().Married_To != 0)
                        {
                            Session.SendWhisper("Sorry, " + TargetSession.GetHabbo().UserName + " is already married with someone else.");
                            return true;
                        }
                        if (Session.GetHabbo().UserName == TargetSession.GetHabbo().UserName)
                        {
                            Session.SendWhisper("Why would you marry yourself?");
                            return true;
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                            return true;
                        }
                        if (RoleplayManager.UserDistance(Session, TargetSession) >= 2)
                        {
                            Session.SendWhisper("You must get closer to do this!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Asks for " + TargetSession.GetHabbo().UserName + "'s hand in marriage*", 17);
                        TargetSession.GetRoleplay().marryReq = true;
                        TargetSession.GetRoleplay().marryReqer = Session.GetHabbo().Id;
                        TargetSession.SendWhisperBubble(Session.GetHabbo().UserName + " has asked for your hand in marriage. Type #accept to accept or #deny to deny.", 1);

                        #endregion

                    }
                    return true;

                #endregion

                #region :divorce

                case "divorce":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :divorce <username>");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("COURT"))
                        {
                            Session.SendWhisper("You must be in the Supreme Court [35] to get divorced!");
                            return true;
                        }
                        if (Session.GetRoleplay().Married_To == 0)
                        {
                            Session.SendWhisper("Sorry, you are not married with anyone?!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        if (TargetSession != null)
                        {
                            if (Session.GetHabbo().Id != TargetSession.GetRoleplay().Married_To)
                            {
                                Session.SendWhisper(TargetSession.GetHabbo().UserName + " isn't married to you.");
                                return true;
                            }
                            if (Session.GetHabbo().UserName == TargetSession.GetHabbo().UserName)
                            {
                                return true;
                            }
                            Session.Shout("*Divorces " + TargetSession.GetHabbo().UserName + "*");
                            TargetSession.SendWhisper(Session.GetHabbo().UserName + " had divorced you.");

                            Session.GetRoleplay().Married_To = 0;
                            Session.GetRoleplay().SaveQuickStat("married_to", "" + Session.GetRoleplay().Married_To);

                            TargetSession.GetRoleplay().Married_To = 0;
                            TargetSession.GetRoleplay().SaveQuickStat("married_to", "" + TargetSession.GetRoleplay().Married_To);
                        }
                        else
                        {

                            #region Offline Divorce

                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {

                                dbClient.SetQuery("SELECT id FROM users WHERE username = '" + Target + "'");
                                int targetid = dbClient.GetInteger();

                                dbClient.SetQuery("SELECT username FROM users WHERE username = '" + Target + "'");
                                string username = dbClient.GetString();

                                dbClient.SetQuery("SELECT married_to FROM rp_stats WHERE id = '" + targetid + "'");
                                DataRow TarGetRow = dbClient.GetRow();




                                #region Conditions (Offline)

                                if (TarGetRow == null)
                                {
                                    Session.SendWhisper("This user does not exist!");
                                    return true;
                                }
                                if (Convert.ToInt32(TarGetRow["married_to"]) != Session.GetHabbo().Id)
                                {
                                    Session.SendWhisper("You are not married to that person, therefore you cannot divorce them!");
                                    return true;
                                }
                                #endregion


                                Session.SendWhisper("This user is offline, but they have still been divorced!");

                                Session.GetRoleplay().Married_To = 0;
                                Session.GetRoleplay().SaveQuickStat("married_to", "0");

                                RoleplayManager.Shout(Session, "*Divorces " + username + "*");
                                dbClient.RunFastQuery("UPDATE rp_stats SET married_to = 0 WHERE id = '" + targetid + "'");
                                return true;
                            }
                            #endregion
                        }

                        #endregion

                    }
                    return true;

                #endregion

                #region :timeleft
                case "timeleft":
                    {
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().DeadTimer + " minute(s) left until you are discharged from the hospital.");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().JailTimer + " minute(s) left until you are released from jail.");
                            return true;
                        }
                        if (Session.GetRoleplay().BeingHealed && Session.GetRoleplay().healTimer != null)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().healTimer.getTime() + " second(s) left until your wounds heal.");
                            return true;
                        }
                        if (Session.GetRoleplay().BeingMassaged && Session.GetRoleplay().massageTimer != null)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().massageTimer.getTime() + " second(s) left until your aches are relieved.");
                            return true;
                        }
                        if (Session.GetRoleplay().Relaxing && Session.GetRoleplay().relaxTimer != null)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().relaxTimer.getTime() + " second(s) left until your aches are relieved.");
                            return true;
                        }
                        if (Session.GetRoleplay().Robbery && Session.GetRoleplay().bankRobTimer != null)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().bankRobTimer.getTime() + " minute(s) left until you complete your robbery.");
                            return true;
                        }
                        if (Session.GetRoleplay().NPA && Session.GetRoleplay().npaTimer != null)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().npaTimer.getTime() + " minute(s) left until you nuke the city.");
                            return true;
                        }
                        if (Session.GetRoleplay().ATMRobbery && Session.GetRoleplay().ATMRobTimer != null)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().ATMRobTimer.getTime() + " minute(s) left until you rob the ATM.");
                            return true;
                        }
                        if (Session.GetRoleplay().Learning && Session.GetRoleplay().learningTimer != null)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().learningTimer.getTime() + " minute(s) left until you learn a new subject.");
                            return true;
                        }
                        if (Session.GetRoleplay().Working && Session.GetRoleplay().workingTimer != null)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().workingTimer.getTime() + " minute(s) left until you receive your next paycheck.");
                            return true;
                        }
                        if (Session.GetRoleplay().SentHome && Session.GetRoleplay().sendHomeTimer != null)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().sendHomeTimer.getTime() + " minute(s) left until you can get back to work from being senthome!");
                            return true;
                        }
                        if (Session.GetRoleplay().UsingWeed && Session.GetRoleplay().weedTimer != null)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().weedTimer.getTime() + " second(s) left until your high runs out!");
                            return true;
                        }
                        if (Session.GetRoleplay().GangCapturing && Session.GetRoleplay().gangCaptureTimer != null)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().gangCaptureTimer.getTime() + " minute(s) left until you capture this turf!");
                            return true;
                        }
                        if (Session.GetRoleplay().WorkingOut)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().WorkoutSeconds + " second(s) left until your next progress: " + Session.GetRoleplay().WorkoutTimer_Done + "/" + Session.GetRoleplay().WorkoutTimer_ToDo + "!");
                            return true;
                        }
                        if (Session.GetRoleplay().WeightLifting)
                        {
                            Session.SendWhisper("You have " + Session.GetRoleplay().WeightLiftSeconds + " second(s) left until your next progress: " + Session.GetRoleplay().WeightLiftTimer_Done + "/" + Session.GetRoleplay().WeightLiftTimer_ToDo + "!");
                            return true;
                        }
                        else
                        {
                            Session.SendWhisper("You currently have no timers running!");
                            return true;
                        }
                    }

                #endregion

                #region :bail
                case "bail":
                case "paybail":
                    {
                        #region Generate Instances / Sessions
                        string Target = null;
                        GameClient TargetSession = null;
                        int amount = 0;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :bail <user>");
                            return true;
                        }
                        else
                        {
                            Target = Convert.ToString(Params[1]);
                            TargetSession = RoleplayManager.GenerateSession(Target);
                            amount = TargetSession.GetRoleplay().JailTimer * 100;
                        }

                        if (!RoleplayManager.CanInteract(Session, TargetSession, true))
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        if (Session.GetHabbo().Credits < amount)
                        {
                            Session.SendWhisper("You must have $" + amount + " to bail them!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Can't do this awhile jailed.");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Can't do this awhile dead.");
                            return true;
                        }
                        if (!TargetSession.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("This user is not jailed.");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("JAIL"))
                        {
                            Session.SendWhisper("You must be at jail to bail a user.");
                            return true;
                        }

                        #endregion

                        #region Execute
                        if (TargetSession.GetRoleplay().Jailed)
                        {
                            RoleplayManager.Shout(Session, "*Bails " + TargetSession.GetHabbo().UserName + " out of jail [-$" + amount + "]*");
                            RoleplayManager.GiveMoney(Session, -amount);

                            RoleplayManager.Shout(TargetSession, "*Finishes their time in jail*");
                            TargetSession.GetRoleplay().JailTimer = 0;
                            TargetSession.GetRoleplay().Jailed = false;
                            TargetSession.GetRoleplay().SaveStatusComponents("jailed");
                            if (TargetSession.GetRoleplay().FigBeforeSpecial != null && TargetSession.GetRoleplay().MottBeforeSpecial != null)
                            {
                                TargetSession.GetHabbo().Look = TargetSession.GetRoleplay().FigBeforeSpecial;
                                TargetSession.GetHabbo().Motto = TargetSession.GetRoleplay().MottBeforeSpecial;
                            }
                            else
                            {
                                DataRow User = null;

                                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                                {
                                    dbClient.SetQuery("SELECT look,motto FROM users WHERE id = '" + TargetSession.GetHabbo().Id + "'");
                                    User = dbClient.GetRow();
                                }

                                TargetSession.GetHabbo().Look = Convert.ToString(User["look"]);
                                TargetSession.GetHabbo().Motto = Convert.ToString(User["motto"]);
                            }
                            TargetSession.GetRoleplay().RefreshVals();
                        }
                        else
                        {
                            Session.SendWhisper("This user is not jailed!");
                            return true;
                        }


                        #endregion


                    }
                    return true;


                #endregion

                #region :dispose
                case "dispose":
                    {

                        if (Session.GetRoleplay().Weed <= 0)
                        {
                            Session.SendWhisper("You do not have any weed!");
                            return true;
                        }

                        Session.GetRoleplay().Weed = 0;
                        Session.GetRoleplay().SaveQuickStat("weed", "" + 0);
                        RoleplayManager.Shout(Session, "*Disposes of all their weed*", 4);
                        return true;
                    }
                #endregion

                #region :text
                case "text":
                    {

                        #region Generate Instances / Sessions
                        string Target = null;
                        string Msg = null;

                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :text <user> <msg>");
                        }
                        else
                        {
                            Target = Convert.ToString(Params[1]);
                            Msg = MergeParams(Params, 2);
                        }

                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        int credit = new Random().Next(1, 15);
                        #endregion

                        #region Conditions

                        if (!RoleplayManager.CanInteract(Session, TargetSession, false))
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado!", 34);
                            return true;
                        }

                        if (Session.GetRoleplay().Phone != 1)
                        {
                            Session.SendWhisperBubble("You do not have a phone! You can buy one at RoomID: [5]!", 34);
                            return true;
                        }
                        if (Session.GetHabbo().ActivityPoints < credit)
                        {
                            Session.SendWhisperBubble("You do not any credit left on your phone! You can buy some more at RoomID: [5]!", 34);
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("text_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("text_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["text_cooldown"] > 0)
                        {
                            Session.SendWhisperBubble("Cooldown [" + Session.GetRoleplay().MultiCoolDown["text_cooldown"] + "/5]", 34);
                            return true;
                        }
                        if (Session.GetRoleplay().DisabledTexts)
                        {
                            Session.SendWhisperBubble("You cannot do this when you have disabled texts!", 34);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().DisabledTexts)
                        {
                            Session.SendWhisperBubble("This user has disabled incoming texts!", 34);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().BlockedTexters.Contains(Session.GetHabbo().Id))
                        {
                            Session.SendWhisperBubble("This user has added you to their contact block list!", 34);
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SHAME"))
                        {
                            Session.SendWhisperBubble("You cannot do this in the SHAME room!", 34);
                            return true;
                        }
                        if (TargetSession.GetHabbo().CurrentRoom.RoomData.Description.Contains("SHAME") && !RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisperBubble("This user is in the SHAME room, therefore you cannot text them.", 34);
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoomUser u = Session.GetHabbo().GetRoomUser();
                        RoomUser u2 = TargetSession.GetHabbo().GetRoomUser();

                        Session.GetRoleplay().EffectSeconds = 3;
                        u.ApplyEffect(65);

                        Session.GetRoleplay().MultiCoolDown["text_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        TargetSession.GetRoleplay().LastTexter = Session;

                        RoleplayManager.Shout(Session, "*Sends a text message to " + TargetSession.GetHabbo().UserName + "*", 4);
                        RoleplayManager.GiveCredit(Session, -credit);

                        if (TargetSession.GetHabbo().GetRoomUser().IsAsleep)
                            Session.SendWhisperBubble("This user is AFK, but the message still has been sent!", 1);

                        TargetSession.GetRoleplay().EffectSeconds = 3;

                        if (u2 != null)
                        {
                            u2.ApplyEffect(65);
                        }

                        RoleplayManager.Shout(TargetSession, "*Receives a new text message from " + Session.GetHabbo().UserName + "*", 4);

                        string view = "";
                        view += "=====================================================\nYou have just received a new text message!\n=====================================================\n";
                        view += "From: " + Session.GetHabbo().UserName + "\n";
                        view += "Title: -" + "\n";
                        view += "Sent: " + DateTime.Now + " (Server Time)\n\n";
                        view += "Message: \n";
                        view += Msg + "\n\n";
                        view += "-" + Session.GetHabbo().UserName;

                        TargetSession.SendNotifWithScroll(view);

                        var roomUserByRank = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByRank(2);

                        foreach (var current2 in roomUserByRank)
                            if (current2 != null && current2.HabboId != u2.HabboId &&
                                current2.HabboId != u.HabboId && current2.GetClient() != null)
                            {
                                if (RoleplayManager.BypassRights(current2.GetClient()))
                                {
                                    var whispStaff = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
                                    whispStaff.AppendInteger(u.VirtualId);
                                    whispStaff.AppendString(string.Format("Text to {0}: {1}", TargetSession.GetHabbo().UserName, Msg));
                                    whispStaff.AppendInteger(0);
                                    whispStaff.AppendInteger(0);
                                    whispStaff.AppendInteger(0);
                                    whispStaff.AppendInteger(-1);
                                    current2.GetClient().SendMessage(whispStaff);
                                }
                            }
                        #endregion

                        return true;
                    }
                #endregion

                #region :reply
                case "re":
                case "reply":
                    {

                        #region Generate Instances / Sessions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :re <msg>", 34);
                        }

                        string Msg = MergeParams(Params, 1);
                        GameClient TargetSession = null;
                        TargetSession = Session.GetRoleplay().LastTexter;

                        int credit = new Random().Next(1, 15);
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.CanInteract(Session, TargetSession, false))
                        {
                            Session.SendWhisperBubble("The last person you texted appears to be offline or not found!", 34);
                            return true;
                        }
                        if (Session.GetRoleplay().Phone != 1)
                        {
                            Session.SendWhisperBubble("You do not have a phone! You can buy one at RoomID: [5]!", 34);
                            return true;
                        }
                        if (Session.GetHabbo().ActivityPoints < credit)
                        {
                            Session.SendWhisperBubble("You have insufficient phone credit funds! You can buy more at [RoomID: 5]!", 34);
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("text_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("text_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["text_cooldown"] > 0)
                        {
                            Session.SendWhisperBubble("Cooldown [" + Session.GetRoleplay().MultiCoolDown["text_cooldown"] + "/5]", 34);
                            return true;
                        }
                        if (Session.GetRoleplay().DisabledTexts)
                        {
                            Session.SendWhisperBubble("You cannot do this when you have disabled texts!", 34);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().DisabledTexts)
                        {
                            Session.SendWhisperBubble("This user has disabled incoming texts!", 34);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().BlockedTexters.Contains(Session.GetHabbo().Id))
                        {
                            Session.SendWhisperBubble("This user has added you to their contact block list!", 34);
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SHAME"))
                        {
                            Session.SendWhisperBubble("You cannot do this in the SHAME room.", 34);
                            return true;
                        }
                        if (TargetSession.GetHabbo().CurrentRoom.RoomData.Description.Contains("SHAME"))
                        {
                            Session.SendWhisperBubble("This user is in the SHAME room, therefore you cannot text them.", 34);
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoomUser u = Session.GetHabbo().GetRoomUser();
                        RoomUser u2 = TargetSession.GetHabbo().GetRoomUser();

                        Session.GetRoleplay().EffectSeconds = 3;
                        u.ApplyEffect(65);

                        Session.GetRoleplay().MultiCoolDown["text_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        RoleplayManager.Shout(Session, "*Replies to " + TargetSession.GetHabbo().UserName + "'s text message*", 4);
                        RoleplayManager.GiveCredit(Session, -credit);

                        if (TargetSession.GetHabbo().GetRoomUser().IsAsleep)
                        { Session.SendWhisper("This user is AFK, but the message still has been sent!"); }

                        TargetSession.GetRoleplay().LastTexter = Session;

                        TargetSession.GetRoleplay().EffectSeconds = 3;
                        u2.ApplyEffect(65);

                        RoleplayManager.Shout(TargetSession, "*Receives a reply text from " + Session.GetHabbo().UserName + "*", 4);

                        string view = "";
                        view += "=====================================================\nYou have just received a new text message!\n=====================================================\n";
                        view += "From: " + Session.GetHabbo().UserName + "\n";
                        view += "Title: -" + "\n";
                        view += "Sent: " + DateTime.Now + " (Server Time)\n\n";
                        view += "Message: \n";
                        view += Msg + "\n\n";
                        view += "-" + Session.GetHabbo().UserName;

                        TargetSession.SendNotifWithScroll(view);

                        var roomUserByRank = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByRank(6);

                        foreach (var current2 in roomUserByRank)
                            if (current2 != null && current2.HabboId != u2.HabboId &&
                                current2.HabboId != u.HabboId && current2.GetClient() != null)
                            {
                                if (RoleplayManager.BypassRights(current2.GetClient()))
                                {
                                    var whispStaff = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
                                    whispStaff.AppendInteger(u.VirtualId);
                                    whispStaff.AppendString(string.Format("Reply text to {0}: {1}", TargetSession.GetHabbo().UserName, Msg));
                                    whispStaff.AppendInteger(0);
                                    whispStaff.AppendInteger(0);
                                    whispStaff.AppendInteger(0);
                                    whispStaff.AppendInteger(-1);
                                    current2.GetClient().SendMessage(whispStaff);
                                }
                            }
                        #endregion

                        return true;
                    }
                #endregion

                #region :toggletexts
                case "toggletexts":
                    {

                        #region Conditions

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("dtext_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("dtext_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["dtext_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["dtext_cooldown"] + "/15]");
                            return true;
                        }
                        #endregion

                        #region Execute

                        string msg = "";

                        if (Session.GetRoleplay().DisabledTexts)
                        {
                            Session.GetRoleplay().DisabledTexts = false;
                            msg = "*Enables incoming text messages*";
                        }
                        else
                        {
                            Session.GetRoleplay().DisabledTexts = true;
                            msg = "*Disables incoming text messages*";
                        }
                        RoleplayManager.Shout(Session, msg, 4);
                        Session.GetRoleplay().MultiCoolDown["dtext_cooldown"] = 15;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        #endregion

                        return true;
                    }
                #endregion

                #region :block
                case "block":
                case "textblock":
                case "blocktext":
                    {

                        #region Params
                        string Target = null;
                        GameClient TargetSession = null;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :block <user>");
                            return true;
                        }
                        else
                        {
                            Target = Convert.ToString(Params[1]);
                        }

                        TargetSession = RoleplayManager.GenerateSession(Target);
                        if (!RoleplayManager.CanInteract(Session, TargetSession, false))
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }
                        if (Session.GetRoleplay().BlockedTexters.Contains(TargetSession.GetHabbo().Id))
                        {
                            Session.SendWhisper("This user is already on your block list!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        Session.GetRoleplay().BlockTexter(TargetSession.GetHabbo().Id);
                        Session.Shout("*Adds " + TargetSession.GetHabbo().UserName + "'s to their phone block list*");

                        #endregion

                        return true;
                    }
                #endregion

                #region :unblock
                case "unblock":
                    {


                        #region Params
                        string Target = null;
                        GameClient TargetSession = null;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :unblock <user>");
                            return true;
                        }
                        else
                        {
                            Target = Convert.ToString(Params[1]);
                        }

                        TargetSession = RoleplayManager.GenerateSession(Target);
                        if (!RoleplayManager.CanInteract(Session, TargetSession, false))
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }
                        if (!Session.GetRoleplay().BlockedTexters.Contains(TargetSession.GetHabbo().Id))
                        {
                            Session.SendWhisper("This user is not in your block list!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        Session.GetRoleplay().UnblockTexter(TargetSession.GetHabbo().Id);
                        Session.Shout("*Removes " + TargetSession.GetHabbo().UserName + " from their phone block list*");

                        #endregion

                        return true;
                    }
                #endregion

                #region :blocklist

                case "blocklist":
                    {

                        string BlockedList = "=======================\nBlock List\n=======================\n\n";

                        if (Session.GetRoleplay().BlockedTexters.Count > 0)
                        {
                            foreach (uint UserID in Session.GetRoleplay().BlockedTexters)
                            {
                                BlockedList += "[-] " + RoleplayManager.GetStatByID(Convert.ToInt32(UserID), "username") + "\n";
                            }
                        }
                        else
                        {
                            BlockedList += "Your block list appears to be empty!";
                        }

                        Session.SendNotifWithScroll(BlockedList);

                        return true;
                    }

                #endregion

                #region :buycredit
                case "buycredit":
                case "buyduckets":
                case "buycredits":
                case "buyducket":
                    {

                        #region Params
                        int Amnt = 0;
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :buycredit <amount>");
                            return true;
                        }
                        else
                        {
                            Amnt = Convert.ToInt32(Params[1]);
                        }
                        #endregion

                        #region Conditions

                        double dub = Amnt / 2 + 2;
                        double Conv = Math.Round(dub, 0);
                        int Pay = Convert.ToInt32(Conv);

                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("PHONE"))
                        {
                            Session.SendWhisper("You must be in the Anx Tech [RoomID 5] to buy phone credit!");
                            return true;
                        }
                        if (Amnt < 25)
                        {
                            Session.SendWhisper("You cannot buy less than 25 phone credit at a time!");
                            return true;
                        }
                        if (Session.GetHabbo().Credits < Pay)
                        {
                            Session.SendWhisper("You need at least $" + Pay + " for " + Amnt + " texts!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Purchases a " + Amnt + " Flux Network Texts for $" + Pay + " [-$" + Pay + "]*");
                        RoleplayManager.GiveCredit(Session, +Amnt);
                        RoleplayManager.GiveMoney(Session, -Pay);
                        #endregion

                        return true;
                    }
                #endregion

                #endregion

                #region Activities

                #region :workout
                case "workout":
                    {

                        #region Conditions
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("GYM"))
                        {
                            Session.SendWhisper("You are not in the gym!");
                            return true;
                        }
                        if (Session.GetRoleplay().Strength >= 20)
                        {
                            Session.SendWhisper("You have reached a limit of 20 strength!");
                            return true;
                        }
                        if (Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Can't do this awhile working.");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("workout_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("workout_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["workout_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["workout_cooldown"] + "/5]");
                            return true;
                        }
                        if (Session.GetRoleplay().WorkingOut)
                        {
                            Session.SendWhisper("You are already working out!");
                            return true;
                        }
                        #endregion

                        #region Set

                        RoomUser User = null;
                        User = Session.GetHabbo().GetRoomUser();
                        RoomItem Inter = Session.GetRoleplay().GetNearItem("sf_roller", 0);
                        #endregion

                        #region Execute

                        if (Inter == null)
                        {
                            Session.SendWhisper("You are not on a treadmill!");
                            return true;
                        }

                        User.SetRot(Inter.Rot);
                        RoleplayManager.Shout(Session, "*Starts to workout*", 1);
                        Session.GetRoleplay().WorkoutSeconds = 40;
                        Session.GetRoleplay().CalculateWorkoutTimer();
                        Session.GetRoleplay().WorkingOut = true;
                        Session.SendWhisper("Working Out: " + Session.GetRoleplay().WorkoutTimer_Done + "/" + Session.GetRoleplay().WorkoutTimer_ToDo);
                        User.IsWalking = true;
                        User.AddStatus("mv", User.X + "," + User.Y + "," + User.Z.ToString().Replace(',', '.'));
                        Session.GetRoleplay().MultiCoolDown["workout_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion

                #region :weightlift
                case "weightlift":
                case "lift": //added By hender
                    {
                        #region Conditions
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("GYM"))
                        {
                            Session.SendWhisper("You are not in the gym!");
                            return true;
                        }
                        if (Session.GetRoleplay().Constitution >= 10)
                        {
                            Session.SendWhisper("You have reached a limit of 10 constitution!");
                            return true;
                        }
                        if (Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Can't do this awhile working.");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("weightlift_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("weightlift_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["weightlift_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["weightlift_cooldown"] + "/5]");
                            return true;
                        }
                        if (Session.GetRoleplay().WeightLifting)
                        {
                            Session.SendWhisper("You are already lifting weights!");
                            return true;
                        }
                        #endregion

                        #region Set

                        RoomUser User = null;
                        User = Session.GetHabbo().GetRoomUser();
                        RoomItem Inter = Session.GetRoleplay().GetNearItem("uni_wobench", 0);
                        #endregion

                        #region Execute

                        if (Inter == null)
                        {
                            Session.SendWhisper("You are not on a weightlifting bench!");
                            return true;
                        }
                        RoleplayManager.Shout(Session, "*Starts lifting weights*", 1);
                        Session.GetRoleplay().WeightLiftSeconds = 40;
                        Session.GetRoleplay().CalculateWeightLiftTimer();
                        Session.GetRoleplay().WeightLifting = true;
                        Session.SendWhisper("Lifting Weights: " + Session.GetRoleplay().WeightLiftTimer_Done + "/" + Session.GetRoleplay().WeightLiftTimer_ToDo);
                        Session.GetRoleplay().MultiCoolDown["weightlift_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion

                #region :relax
                case "relax":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SPA") && !Session.GetHabbo().HasFuse("fuse_owner"))
                        {
                            Session.SendWhisper("You are not in the spa!");
                            return true;
                        }
                        if (Session.GetRoleplay().Relaxing)
                        {
                            Session.SendWhisper("You are already relaxing!");
                            return true;
                        }
                        if (Session.GetRoleplay().BeingMassaged)
                        {
                            Session.SendWhisper("You are already being massaged!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver na cadeia!");
                            return true;
                        }
                        if (Session.GetRoleplay().Energy == 100)
                        {
                            Session.SendWhisper("Your energy is already full!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("massage_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("massage_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["massage_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["massage_cooldown"] + "/10]");
                            return true;
                        }
                        #endregion

                        #region Chair Condition

                        RoomUser User = null;
                        User = Session.GetHabbo().GetRoomUser();
                        RoomItem Inter = Session.GetRoleplay().GetNearItem("val14_recchair", 0);
                        if (Inter == null)
                        {
                            Session.SendWhisper("You are not on a treatment chair!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Lays back on the chair, relaxing their tired body*");
                        Session.GetRoleplay().relaxTimer = new relaxTimer(Session);
                        Session.GetRoleplay().Relaxing = true;
                        Session.GetRoleplay().MultiCoolDown["massage_cooldown"] = 10;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        #endregion

                        return true;
                    }
                #endregion

                #endregion

                #region Education
                #region :startlearn
                case "startlearn":
                case "startlearning":
                case "learn":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SCHOOL"))
                        {
                            Session.SendWhisper("You must be in the Library to do this!"); // hender
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this while jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this whilst your dead!");
                            return true;
                        }
                        if (Session.GetRoleplay().Learning)
                        {
                            Session.SendWhisper("You are already reading something!"); //hender
                            return true;
                        }
                        if (Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("You cannot do this while working!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("learning"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("learning", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["learning"] > 0)
                        {
                            Session.SendWhisper("You must wait until you can start learning again! [" + Session.GetRoleplay().MultiCoolDown["learning"] + "/5]");
                            return true;
                        }
                        #endregion

                        #region Execute

                        Session.GetRoleplay().learningTimer = new learningTimer(Session);
                        Session.GetRoleplay().Learning = true;
                        Session.GetRoleplay().SaveStatusComponents("learning");
                        RoleplayManager.Shout(Session, "*Opens up a book and begins to read. [LEARNING]*", 11); //Hender
                        Session.GetHabbo().GetRoomUser().ApplyEffect(1004);
                        Session.SendWhisper("You have " + Session.GetRoleplay().learningTimer.getTime() + " minutes until you learn a new subject.");
                        Session.GetRoleplay().MultiCoolDown["learning"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                    }
                    return true;

                #endregion
                #region :stoplearn
                case "stoplearn":
                case "stoplearning":
                case "slearn":
                    {

                        #region Conditions
                        if (!Session.GetRoleplay().Learning)
                        {
                            Session.SendWhisper("You arent even learning!");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SCHOOL"))
                        {
                            Session.SendWhisper("You must be at school to do this!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this while jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this while dead!");
                            return true;
                        }
                        if (Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("You cannot do this while working!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("learning"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("learning", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["learning"] > 0)
                        {
                            Session.SendWhisper("You must wait until you can start learning again! [" + Session.GetRoleplay().MultiCoolDown["learning"] + "/5]");
                            return true;
                        }
                        #endregion

                        #region Execute

                        Session.GetRoleplay().Learning = false;
                        Session.GetRoleplay().learningTimer.stopTimer();
                        RoleplayManager.Shout(Session, "*Stops learning*");
                        Session.GetRoleplay().MultiCoolDown["learning"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        Session.GetHabbo().GetRoomUser().ApplyEffect(0);

                        #endregion

                    }
                    return true;

                #endregion
                #endregion

                #region Vehicles
                #region :buycar
                case "buycar":
                    {

                        #region Conditions
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("CARSHOP"))
                        {
                            Session.SendWhisper("This isn't the car shop.");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Bad syntax, please use :buycar <carname>.");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this while jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot to this while dead!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        if (Params[1].ToLower().Equals("skyblue"))
                        {
                            if (Session.GetHabbo().Credits < 3000)
                            {
                                Session.SendWhisper("You can't afford this car yet, type :help for a list of available models.");
                                return true;
                            }
                            RoleplayManager.Shout(Session, "*Buys a DatCarr Skyblue [-$3000]");
                            RoleplayManager.GiveMoney(Session, -3000);
                            Session.GetRoleplay().Car = 21;
                            Session.GetRoleplay().SaveQuickStat("car", "21");
                            return true;
                        }
                        if (Params[1].ToLower().Equals("fireball"))
                        {
                            if (Session.GetHabbo().Credits < 4000)
                            {
                                Session.SendWhisper("You can't afford this car yet, type :help for a list of available models.");
                                return true;
                            }
                            RoleplayManager.Shout(Session, "*Buys a DatCarr Fireball [-$4000]");
                            RoleplayManager.GiveMoney(Session, -4000);
                            Session.GetRoleplay().Car = 22;
                            Session.GetRoleplay().SaveQuickStat("car", "22");
                            return true;
                        }
                        if (Params[1].ToLower().Equals("doggi"))
                        {
                            if (Session.GetHabbo().Credits < 5000)
                            {
                                Session.SendWhisper("You can't afford this car yet, type :help for a list of available models.");
                                return true;
                            }
                            RoleplayManager.Shout(Session, "*Buys a DatCarr Doggi [-$5000]");
                            RoleplayManager.GiveMoney(Session, -5000);
                            Session.GetRoleplay().Car = 48;
                            Session.GetRoleplay().SaveQuickStat("car", "48");
                            return true;
                        }
                        if (Params[1].ToLower().Equals("bunni"))
                        {
                            if (Session.GetHabbo().Credits < 5000)
                            {
                                Session.SendWhisper("You can't afford this car yet, type :help for a list of available models.");
                                return true;
                            }
                            RoleplayManager.Shout(Session, "*Buys a DatCarr Bunni [-$5000]");
                            RoleplayManager.GiveMoney(Session, -5000);
                            Session.GetRoleplay().Car = 54;
                            Session.GetRoleplay().SaveQuickStat("car", "54");
                            return true;
                        }
                        if (Params[1].ToLower().Equals("beetle"))
                        {
                            if (Session.GetHabbo().Credits < 6000)
                            {
                                Session.SendWhisper("You can't afford this car yet, type :help for a list of available models.");
                                return true;
                            }
                            RoleplayManager.Shout(Session, "*Buys a DatCarr Beetle [-$6000]");
                            RoleplayManager.GiveMoney(Session, -6000);
                            Session.GetRoleplay().Car = 69;
                            Session.GetRoleplay().SaveQuickStat("car", "69");
                            return true;
                        }
                        Session.SendWhisper("This car is out of stock, or does not exist in the DatCarr product range, type :help for a list of available models.");
                        return true;

                        #endregion

                    }
                #endregion
                #region :drive
                case "drive":
                case "startdrive":
                case "startcar":
                    {

                        #region Conditions

                        if (Session.GetRoleplay().Car <= 0)
                        {
                            Session.SendWhisper("You currently don't own a car.");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("DRIVE"))
                        {
                            Session.SendWhisper("You cannot start driving in here!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this while jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot to this while dead!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("car"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("car", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["car"] > 0)
                        {
                            Session.SendWhisper("You must wait until using your car again! - [" + Session.GetRoleplay().MultiCoolDown["car"] + "/350]");
                            return true;
                        }

                        #endregion

                        #region Execute

                        if (!Session.GetHabbo().GetRoomUser().FastWalking)
                        {
                            RoleplayManager.Shout(Session, "*Inserts keys into ignition and starts driving their car*");
                            Session.GetHabbo().GetRoomUser().FastWalking = true;
                            Session.GetRoleplay().usingCar = true;
                            Session.GetHabbo().GetRoomUser().ApplyEffect(Session.GetRoleplay().Car);
                        }
                        else
                        {
                            Session.GetHabbo().GetRoomUser().FastWalking = false;
                            Session.GetRoleplay().usingCar = false;
                            RoleplayManager.Shout(Session, "*Removes keys from ignition and stops driving their car*");
                            Session.GetHabbo().GetRoomUser().ApplyEffect(0);
                        }
                        Session.GetRoleplay().MultiCoolDown["car"] = 350;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                    }
                    return true;


                #endregion
                #region :stopdrive
                case "stopdrive":
                case "sdrive":
                case "stopcar":
                    {


                        #region Conditions

                        if (Session.GetRoleplay().Car <= 0)
                        {
                            Session.SendWhisper("You currently don't own a car.");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("DRIVE"))
                        {
                            Session.SendWhisper("You cannot start driving in here!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this while jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot to this while dead!");
                            return true;
                        }
                        if (!Session.GetHabbo().GetRoomUser().FastWalking)
                        {
                            Session.SendWhisper("You can't stop your 'imaginary' stop?");
                            return true;
                        }

                        #endregion

                        #region Execute

                        Session.GetHabbo().GetRoomUser().FastWalking = false;
                        Session.GetRoleplay().usingCar = false;
                        RoleplayManager.Shout(Session, "*Removes keys from ignition and stops driving their car*");
                        Session.GetHabbo().GetRoomUser().ApplyEffect(0);

                        #endregion

                    }
                    return true;


                #endregion

                // Plane was disabled by Hender (OP AF)
                #region :plane
                //case "plane":
                //case "startplane":
                //    {

                //        #region Conditions

                //        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("PLANE"))
                //        {
                //            Session.SendWhisper("You cannot use your plane in here!");
                //            return true;
                //        }
                //        if (Session.GetRoleplay().Plane != 1)
                //        {
                //            Session.SendWhisper("You don't have a plane");
                //            return true;
                //        }
                //        if (Session.GetRoleplay().Fuel <= 0)
                //        {
                //            Session.SendWhisper("You don't have any fuel left");
                //            return true;
                //        }
                //        if (Session.GetRoleplay().Jailed)
                //        {
                //            Session.SendWhisper("You cannot do this while jailed!");
                //            return true;
                //        }
                //        if (Session.GetRoleplay().Dead)
                //        {
                //            Session.SendWhisper("You cannot do this while dead!");
                //            return true;
                //        }
                //        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("plane_cooldown"))
                //        {
                //            Session.GetRoleplay().MultiCoolDown.Add("plane_cooldown", 0);
                //        }
                //        if (Session.GetRoleplay().MultiCoolDown["plane_cooldown"] > 0)
                //        {
                //            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["plane_cooldown"] + "/300]");
                //            return true;
                //        }

                //        #endregion

                //        #region Execute

                //        if (Session.GetHabbo().GetRoomUser().CurrentEffect != 175)
                //        {
                //            Session.GetRoleplay().planeUsing = 1;
                //            Session.GetHabbo().GetRoomUser().ApplyEffect(175);
                //            Session.GetHabbo().GetRoomUser().AllowOverride = true;
                //            Session.GetRoleplay().usingPlane = true;
                //            Session.GetRoleplay().planeTimer = new planeTimer(Session);

                //            RoleplayManager.Shout(Session, "*Hops into their plane and begins navigating*");
                //        }
                //        else
                //        {
                //            Session.GetRoleplay().usingPlane = false;
                //            Session.GetHabbo().GetRoomUser().ApplyEffect(0);
                //            Session.GetHabbo().GetRoomUser().AllowOverride = false;
                //            Session.GetRoleplay().planeUsing = 0;

                //            RoleplayManager.Shout(Session, "*Hops off their plane and stops navigating*");
                //        }
                //        Session.GetRoleplay().MultiCoolDown["plane_cooldown"] = 300;
                //        Session.GetRoleplay().CheckingMultiCooldown = true;

                //        #endregion

                //    }
                //    return true;


                #endregion
                #region :buyplane
                //case "buyplane":
                //    {
                //        #region Conditions
                //        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("PLANESHOP"))
                //        {
                //            Session.SendWhisper("You're not in the plane shop.");
                //            return true;
                //        }
                //        if (Session.GetRoleplay().Plane >= 1)
                //        {
                //            Session.SendWhisper("You already have a plane.");
                //            return true;
                //        }
                //        if (Session.GetRoleplay().Jailed)
                //        {
                //            Session.SendWhisper("Can't do this awhile jailed.");
                //            return true;
                //        }
                //        if (Session.GetRoleplay().Dead)
                //        {
                //            Session.SendWhisper("Can't do this awhile jailed.");
                //            return true;
                //        }
                //        if (Session.GetHabbo().Credits < 50000)
                //        {
                //            Session.SendWhisper("You need $50000 to buy a plane.");
                //            return true;
                //        }

                //        #endregion

                //        #region Execute
                //        RoleplayManager.Shout(Session, "*Purchases a Plane [-$50000][+15000 FUEL]*");
                //        Session.GetRoleplay().Plane = 1;
                //        Session.GetRoleplay().Fuel += 15000;
                //        Session.GetRoleplay().SaveQuickStat("plane", "1");
                //        Session.GetRoleplay().SaveQuickStat("fuel", "" + 15000);
                //        RoleplayManager.GiveMoney(Session, -50000);
                //        return true;
                //        #endregion
                //    }
                #endregion
                #region :buyfuel <amnt>
                //case "buyfuel":
                //    {

                //        #region Params
                //        int Amnt = 0;
                //        if (!RoleplayManager.ParamsMet(Params, 1))
                //        {
                //            Session.SendWhisper("Sintaxe de comando inválida: :buyfuel <amount>");
                //            return true;
                //        }
                //        else
                //        {
                //            Amnt = Convert.ToInt32(Params[1]);
                //        }
                //        double dub = Amnt / 2 + Amnt / 5;
                //        double Conv = Math.Round(dub, 0);
                //        int Pay = Convert.ToInt32(Conv);
                //        #endregion

                //        #region Conditions

                //        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("PLANESHOP"))
                //        {
                //            Session.SendWhisper("You are not in the plane shop!");
                //            return true;
                //        }
                //        if (Amnt < 50)
                //        {
                //            Session.SendWhisper("You cannot buy less than 50 fuel at a time!");
                //            return true;
                //        }
                //        if (Session.GetHabbo().Credits < Pay)
                //        {
                //            Session.SendWhisper("You need at least $" + Pay + " for " + Amnt + " fuel!!");
                //            return true;
                //        }
                //        #endregion

                //        #region Execute
                //        RoleplayManager.Shout(Session, "*Purchases " + Amnt + " fuel for $" + Pay + " [-$" + Pay + "]*");
                //        Session.GetRoleplay().Fuel += Amnt;
                //        Session.GetRoleplay().SaveQuickStat("fuel", "" + Session.GetRoleplay().Fuel);
                //        RoleplayManager.GiveMoney(Session, -Pay);
                //        #endregion

                //        return true;
                //    }
                #endregion

                #endregion

                #endregion

                #region Minigames Commands

                #region Color Wars [STAFF] (2)

                #region :cwjoin
                case "cwjoin":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("This command is for Events Staff only!");
                            return true;
                        }

                        string team = Params[1];
                        Team Team = ColourManager.GetTeamByName(team);

                        if (Team != null)
                        {
                            Session.SendWhisper("You joined the " + Team.Colour + " team!");
                            ColourManager.AddPlayerToTeam(Session, team);
                            return true;
                        }

                        return true;
                    }
                #endregion

                #region :cwassign
                case "cwassign":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            Session.SendWhisper("This command is for Events Staff only!");
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("This command is for Events Staff only!");
                            return true;
                        }
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        string team = Params[2];
                        Team Team = ColourManager.GetTeamByName(team);

                        if (Team != null)
                        {
                            Session.Shout("*Uses their god-like powers to assign " + TargetSession.GetHabbo().UserName + " to the " + Team.Colour + " team*");
                            ColourManager.ForceAddPlayerToTeam(TargetSession, team);
                            return true;
                        }

                        return true;
                    }
                #endregion

                #endregion

                #region Zombie Infection (4)
                #region :startinfection
                case "startinfection":
                    {

                        #region Condition
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("This command is for Events Staff only!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        HabboHotel.Roleplay.Minigames.ZombieInfection.StartGlobalInfection();

                        #endregion

                        return true;
                    }
                #endregion
                #region :stopinfection
                case "stopinfection":
                    {

                        #region Condition
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("This command is for Events Staff only!");
                            return true;
                        }
                        #endregion

                        #region Execute


                        HabboHotel.Roleplay.Minigames.ZombieInfection.StopGlobalInfection();

                        #endregion

                        return true;
                    }
                #endregion
                #region :bite
                case "bite":
                    {

                        #region Vars / Params
                        string Username = Params[1];
                        GameClient TargetSession = null;
                        RoomUser Target = null;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :bite <user>");
                            return true;
                        }
                        else
                        {
                            TargetSession = RoleplayManager.GenerateSession(Username);
                            Target = TargetSession.GetHabbo().GetRoomUser();
                        }
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            if (TargetSession == null)
                            {
                                Session.SendWhisper("This user is offline or is not in this room!");
                                return true;
                            }
                            if (TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom && !RoleplayManager.BypassRights(Session))
                            {
                                Session.SendWhisper("This user is offline or is not in this room!");
                                return true;
                            }
                            if (!RoleplayManager.ZombieInfection)
                            {
                                Session.SendWhisper("Zombie infection is currently not running!");
                                return true;
                            }
                            if (!RoleplayManager.BypassRights(Session))
                            {
                                if (!Session.GetRoleplay().Infected)
                                {
                                    Session.SendWhisper("You must be a zombie to do this!");
                                    return true;
                                }
                                if (!TargetSession.GetRoleplay().inZombieInfection)
                                {
                                    Session.SendWhisper("This user is not a participant of the Zombie Infection Game!");
                                    return true;
                                }
                            }
                            if (TargetSession.GetRoleplay().Infected)
                            {
                                Session.SendWhisper("This user is already a zombie!");
                                return true;
                            }
                            if (RoleplayManager.UserDistance(TargetSession, Session) > 1 && !RoleplayManager.BypassRights(Session))
                            {
                                Session.SendWhisper("You must get closer to do this!");
                                return true;
                            }
                        }
                        #endregion

                        #region Exectue


                        Session.GetRoleplay().Infection_Pts += 1;
                        Session.GetRoleplay().SaveQuickStat("infection_pts", Session.GetRoleplay().Infection_Pts + "");
                        RoleplayManager.Shout(Session, "*Bites " + TargetSession.GetHabbo().UserName + " turning them into a Zombie [+1 Infection Pts]*", 6);
                        TargetSession.GetRoleplay().Infected = true;
                        TargetSession.GetRoleplay().MakeZombie();
                        TargetSession.SendNotifWithScroll("You were infected by a zombie!, quickly run around and bite users, spread the zombie infection, and receive a reward!!\n\n Commands: \n\n - :bite <user>\n");
                        HabboHotel.Roleplay.Minigames.ZombieInfection.CheckForWin(Session);

                        #endregion

                        return true;
                    }
                #endregion
                #region :cure
                case "cure":
                    {

                        #region Vars / Params
                        string Username = Params[1];
                        GameClient TargetSession = null;
                        RoomUser Target = null;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ZombieInfection)
                        {
                            Session.SendWhisper("Zombie infection is currently not running!");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :cure <user>");
                            return true;
                        }
                        else
                        {
                            TargetSession = RoleplayManager.GenerateSession(Username);
                            Target = TargetSession.GetHabbo().GetRoomUser();
                        }
                        if (Session.GetRoleplay().Infected)
                        {
                            Session.SendWhisper("You must be a human to do this!");
                            return true;
                        }
                        if (!TargetSession.GetRoleplay().inZombieInfection)
                        {
                            Session.SendWhisper("This user is not a participant of the Zombie Infection Game!");
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("This user is offline or is not in this room!");
                            return true;
                        }
                        if (TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                        {
                            Session.SendWhisper("This user is offline or is not in this room!");
                            return true;
                        }
                        if (!TargetSession.GetRoleplay().Infected)
                        {
                            Session.SendWhisper("This user is not a zombie!");
                            return true;
                        }
                        if (RoleplayManager.UserDistance(TargetSession, Session) > 1)
                        {
                            Session.SendWhisper("You must get closer to do this!");
                            return true;
                        }
                        #endregion

                        #region Exectue


                        Session.GetRoleplay().Infection_Pts += 1;
                        Session.GetRoleplay().SaveQuickStat("infection_pts", Session.GetRoleplay().Infection_Pts + "");
                        RoleplayManager.Shout(Session, "*Cures " + TargetSession.GetHabbo().UserName + " curing them of the Zombie Virus [+1 Infection Pts]*", 11);
                        RoleplayManager.Shout(TargetSession, "*Groans in agony as their infection is finally cured", 11);
                        TargetSession.GetRoleplay().Infected = false;
                        if (TargetSession.GetRoleplay().FigBeforeSpecial != null)
                        {
                            TargetSession.GetHabbo().Look = TargetSession.GetRoleplay().FigBeforeSpecial;
                            TargetSession.GetHabbo().Motto = TargetSession.GetRoleplay().MottBeforeSpecial;
                        }

                        TargetSession.GetRoleplay().FigBeforeSpecial = null;
                        TargetSession.GetRoleplay().RefreshVals();

                        HabboHotel.Roleplay.Minigames.ZombieInfection.CheckForWin(Session);


                        #endregion

                        return true;
                    }
                #region

                #endregion


                #endregion
                #endregion

                #region Purge (2)
                #region :startpurge
                case "startpurge":
                    {

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("This command is for Events Staff only!");
                            return true;
                        }
                        #endregion

                        string sendmsg = "";
                        sendmsg += "==========================\nHotel Event Initialised!\n==========================\n";
                        sendmsg += "Event: Purge Time\n\n";
                        sendmsg += "-What is Purge Time?\n";
                        sendmsg += "Just like the movie 'The purge' users also get an opportunity to vent their anger ";
                        sendmsg += "out on the people they've longed to for so long, and get away with it, scott free! ";
                        sendmsg += "Under purge time, all crime will remain legal, meaning you can do whatever you want, & not ";
                        sendmsg += "have to worry about a cop arresting you for it!";
                        sendmsg += "\n\nEnjoy the purge =')";

                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {

                                if (client == null)
                                    continue;

                                if (client.GetRoleplay() == null)
                                    continue;

                                if (JobManager.validJob(client.GetRoleplay().JobId, client.GetRoleplay().JobRank))
                                {
                                    if (client.GetRoleplay().JobId == 3 || client.GetRoleplay().JobId == 5)
                                    {
                                        client.GetRoleplay().StopWork();
                                    }
                                }

                                /*client.GetRoleplay().JailTimer = 0;
                                client.GetRoleplay().JailedSeconds = 0;
                                client.GetRoleplay().SaveStatusComponents("jailed");*/


                                if (client.GetRoleplay().Wanted > 0)
                                {
                                    client.GetRoleplay().Wanted = 0;
                                    client.GetRoleplay().SaveQuickStat("wanted", "0");
                                }

                                client.SendNotifWithScroll(sendmsg);

                            }
                        }



                        if (PurgeManager.MainTimer == null)
                        {
                            PurgeManager.MainTimer = new PurgeTimer();
                        }

                        PurgeManager.Running = true;

                        RoleplayManager.WantedListData.Clear();
                        RoleplayManager.PurgeTime = true;


                        return true;
                    }
                #endregion
                #region :stoppurge
                case "stoppurge":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("This command is for Events Staff only!");
                            return true;
                        }
                        #endregion

                        string sendmsg = "";
                        sendmsg += "==========================\nHotel Event Deactivated!\n==========================\n";
                        sendmsg += "Event: Purge Time\n";
                        sendmsg += "The purge has ended! We hope you enjoyed it ;))";

                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {

                                if (client == null)
                                    continue;

                                if (client.GetRoleplay() == null)
                                    continue;

                                // if (jobManager.JobRankData[client.GetRoleplay().JobId, client.GetRoleplay().JobRank].hasRights("police"))
                                // {
                                //     client.GetRoleplay().StopWork();
                                // }

                                // if (client.GetRoleplay().Wanted > 0)
                                // {
                                //     client.GetRoleplay().Wanted = 0;
                                //     client.GetRoleplay().SaveQuickStat("wanted", "0");
                                // }


                                client.SendNotifWithScroll(sendmsg);

                            }
                        }

                        // Misc.WantedListData.Clear();
                        PurgeManager.Running = false;
                        RoleplayManager.PurgeTime = false;

                        return true;
                    }
                #endregion
                #endregion

                #endregion

                #region Corporations / Jobs / Working

                #region Global Commands (2)

                #region :corplist
                case "corplist":
                case "corplista":
                    {

                        string corps = "-----------------------------------------\n";
                        corps += "Lista de todas as corporações da CityRP: \n";

                        foreach (Job Job in JobManager.JobData.Values)
                        {
                            corps += "-----------------------------------------\n";
                            corps += "Trabalho ID: " + Job.Id + "\n";
                            corps += "Corporação: " + Job.Name + "\n";
                            corps += "Proprietário: " + RoleplayManager.ReturnOfflineInfo((uint)Job.OwnerId, "username") + "\n";
                            corps += "Quartel general: " + Job.Headquarters + "\n";
                            corps += "Equilíbrio Corporativo: " + Job.Balance + "\n";

                        }
                        corps += "-----------------------------------------\n";
                        Session.SendNotifWithScroll(corps);

                        return true;
                    }
                #endregion

                #region :corphelp
                case "corphelp":
                case "chelp":
                    {
                        string Commands = "";
                        Commands += "==========================\nPolice & Swat\n==========================\n";
                        Commands += ":stun <user>\n";
                        Commands += ":unstun <user>\n";
                        Commands += ":spray <user>\n";
                        Commands += ":unspray <user>\n";
                        Commands += ":cuff <user>\n";
                        Commands += ":law <user> <time>\n";
                        Commands += ":arrest <user> <time>\n";
                        Commands += ":release <user>\n";
                        Commands += ":search <user>\n";
                        Commands += ":locate <user>\n";
                        Commands += ":backup\n";
                        Commands += ":radio <message>\n";
                        Commands += "====================================================\n\n";

                        Commands += "==========================\nSwat\n==========================\n";
                        Commands += ":flashbang\n";
                        Commands += "====================================================\n\n";

                        Commands += "==========================\nHospital\n==========================\n";
                        Commands += ":discharge <user>\n";
                        Commands += ":heal <user>\n";
                        Commands += "====================================================\n\n";

                        Commands += "==========================\nPhone Tech\n==========================\n";
                        Commands += ":offer <user> phone\n";
                        Commands += "====================================================\n";

                        Commands += "==========================\nAmmunation\n==========================\n";
                        Commands += ":offer <user> <weapon> (Type :help for a list of weapons you can offer)\n";
                        Commands += "====================================================\n";

                        Commands += "==========================\nBubble Juice\n==========================\n";
                        Commands += ":offer <user> bbj\n";
                        Commands += "====================================================\n";

                        Commands += "==========================\nIcecream\n==========================\n";
                        Commands += ":offer <user> icm\n";
                        Commands += "====================================================\n";

                        Commands += "==========================\nSpa\n==========================\n";
                        Commands += ":massage <user>\n";
                        Commands += "====================================================\n";

                        Commands += "==========================\nSubway & McDonalds & KFC\n==========================\n";
                        Commands += ":serve <food> (steak/chips/eggs/spaghetti/milkshakes/chicken)\n";
                        Commands += "====================================================\n";

                        Commands += "==========================\nSpace Miner\n==========================\n";
                        Commands += ":sell rocks\n";
                        Commands += "====================================================\n";

                        Session.SendNotifWithScroll(Commands);

                        return true;
                    }
                #endregion

                #endregion

                #region Specialised Commands (2)

                #region :radio
                case "radio":
                case "r":
                    {
                        if (Session.GetRoleplay().RadioOff)
                        {
                            Session.SendWhisper("Você desligou seu rádio!");
                            return true;
                        }
                        #region Execute
                        string msg = ChatCommandHandler.MergeParams(Params, 1);
                        Radio.send(msg, Session);
                        #endregion
                        return true;
                    }
                #endregion

                #region :jailbreak
                case "jailbreak":
                    {
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisperBubble("Não pode completar esta ação porque você está morto", 34);
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisperBubble("Não é possível concluir esta ação enquanto você está preso", 34);
                            return true;
                        }
                        if (Plus.GetGame().JailBreak != null)
                        {
                            Session.SendWhisperBubble("A prisão já está sendo invadida!", 34);
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("JAIL"))
                        {
                            Session.SendWhisperBubble("Não é possível completar esta ação porque você não está na sala da prisão", 34);
                            return true;
                        }
                        RoomUser Ruser = Session.GetHabbo().GetRoomUser();
                        RoomItem BanzaiTile = Session.GetRoleplay().GetNearItem("bb_rnd_tele", 0);

                        if (Ruser == null)
                            return true;

                        if (BanzaiTile == null)
                        {
                            Session.SendWhisperBubble("Não é possível concluir esta ação porque você não está no local do jailbreak (Banzai Tile)", 34);
                            return true;
                        }

                        // Execute
                        Plus.GetGame().JailBreak = new JailBreak(Session, BanzaiTile.X, BanzaiTile.Y, BanzaiTile.RoomId, BanzaiTile);
                        RoleplayManager.Shout(Session, "*Retira cortadores de fio e começa a cortar os fios na cerca [5 minutos restantes]*");
                        Radio.send("Uhh, temos relatos de uma fuga da prisão em" + Session.GetHabbo().CurrentRoom.RoomData.Name + " [ID: " + Session.GetHabbo().CurrentRoomId + "]! Todas as unidades respondão!", Session, true);
                        return true;
                    }
                #endregion

                #region :toggleradio
                case "togradio":
                case "toggleradio":
                    {
                        if (!JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!", false, 34);
                            return true;
                        }

                        if (Radio.canUseDept(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank) == "NO")
                        {
                            Session.SendWhisper("Seu empregador não emitiu você com um rádio!");
                            return true;
                        }

                        #region Execute
                        bool RadioOff = Session.GetRoleplay().RadioOff;

                        if (RadioOff)
                        {
                            Session.GetRoleplay().RadioOff = false;
                            RoleplayManager.Shout(Session, "*Liga o rádio do departamento deles*", 30);
                        }
                        else
                        {
                            Session.GetRoleplay().RadioOff = true;
                            RoleplayManager.Shout(Session, "*Desliga o rádio do departamento*", 30);
                        }
                        #endregion

                        return true;
                    }
                #endregion

                #endregion

                #region Corporation List (4)

                #region Food Handling (2)

                #region :serve <food>
                case "servir":
                    {


                        #region Params

                        RoomUser User = Session.GetHabbo().GetRoomUser();

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;
                        string Food = "";
                        Food FoodItem = null;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :serve <foodname>");
                            return true;
                        }
                        else
                        {
                            Food = Convert.ToString(Params[1]);
                            FoodItem = Substances.GetFoodByUName(Food);
                        }
                        if (!JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!", false, 34);
                            return true;
                        }
                        if (!Session.GetRoleplay().JobHasRights("diner") && !RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (FoodItem == null)
                        {
                            Session.SendWhisper("O alimento '" + Food + "' não existe!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        if (FoodItem.Drink == "true")
                        {
                            Session.SendWhisper("Você não pode servir bebidas!");
                            return true;
                        }

                        RoomItem Plate = null;
                        Plate = RoleplayManager.GetNearItem(User, "diner_tray_0", 1);

                        if (Plate == null)
                        {
                            Session.SendWhisper("Não há pratos perto de você!");
                            return true;
                        }

                        RoleplayManager.ReplaceItem(Session, Plate, FoodItem.Item_Name);

                        Session.Shout("*Serve " + FoodItem.DisplayName + "[+$2]*");
                        RoleplayManager.GiveMoney(Session, +2);

                        #endregion

                        return true;
                    }
                #endregion

                #region :offerdrink <user> <drinkid>
                case "oferecerbebida":
                    {

                        #region Params

                        RoomUser User = Session.GetHabbo().GetRoomUser();

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;
                        string Food = "";
                        Food FoodItem = null;
                        GameClient TargetSession = null;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :oferecerbebida <user> <foodname>");
                            return true;
                        }
                        else
                        {
                            TargetSession = RoleplayManager.GenerateSession(Params[1]);
                            Food = Convert.ToString(Params[2]);
                            FoodItem = Substances.GetFoodByUName(Food);
                        }
                        if (!JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!", false, 34);
                            return true;
                        }
                        if (!Session.GetRoleplay().JobHasRights("pub") && !RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (FoodItem == null)
                        {
                            Session.SendWhisper("A bebida '" + Food + "' não existe!");
                            return true;
                        }
                        if (FoodItem.Drink == "false")
                        {
                            Session.SendWhisper("Este item não é uma bebida!");
                            return true;
                        }
                        if (!RoleplayManager.CanInteract(Session, TargetSession, true))
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        //RoleplayManager.ReplaceItem(Session, Plate, FoodItem.Item_Name);

                        Session.Shout("*Oferece " + Session.GetHabbo().UserName + " um copo de " + FoodItem.DisplayName + "*");
                        Session.GetRoleplay().OfferDrink(TargetSession, Convert.ToUInt32(FoodItem.DrinkId));

                        #endregion

                        return true;
                    }
                #endregion

                #endregion

                #region Police (14)

                #region :warn x <reason>
                case "warn":
                case "warning":
                case "advertir":
                    {
                        #region Generate Instances / Sessions / Vars
                        string Target = Convert.ToString(Params[1]);
                        string Msg = "";
                        try
                        {
                            Msg = MergeParams(Params, 2);
                        }
                        catch
                        {
                            Session.SendWhisper("Please provide a valid message");
                            return false;
                        }
                        GameClient TargetSession = null;
                        RoomUser Actor = null;
                        RoomUser Targ = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        if (TargetSession != null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Targ = TargetSession.GetHabbo().GetRoomUser();
                            Actor = Session.GetHabbo().GetRoomUser();
                        }
                        else
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        Vector2D Pos1 = new Vector2D(Actor.X, Actor.Y);
                        Vector2D Pos2 = new Vector2D(Targ.X, Targ.Y);

                        #endregion

                        #region Police Conditions

                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver na cadeia!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                        {
                            Session.SendWhisper("Não é possivel fazer isso em territorios");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("police"))
                        {
                            Session.SendWhisper("Não pode fazer isso em quartos 'ocidentais'.");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("western"))
                        {
                            Session.SendWhisper("Só pode fazer isso em quartos 'ocidentais'.");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoomUser u = Session.GetHabbo().GetRoomUser();
                        RoomUser u2 = TargetSession.GetHabbo().GetRoomUser();

                        RoleplayManager.Shout(Session, "*Envia para " + TargetSession.GetHabbo().UserName + " um aviso*", 1);

                        string view = "";
                        view += "=====================================================\nVocê acabou de receber um aviso!\n=====================================================\n";
                        view += "De: " + Session.GetHabbo().UserName + "\n";
                        view += "Titulo: - Aviso de Crime\n";
                        view += "Enviado: " + DateTime.Now + " (Server Time)\n\n";
                        view += "Messagem: \n";
                        view += Msg + "\n\n";
                        view += "-" + Session.GetHabbo().UserName;

                        TargetSession.SendNotifWithScroll(view);
                        #endregion

                        return true;
                    }
                #endregion

                #region :backup
                case "backup":
                case "phelp":
                    {

                        #region Conditions

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver na cadeia!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                        {
                            Session.SendWhisper("Não é possivel fazer isso em territorios");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("backup_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("backup_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["backup_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["backup_cooldown"] + "/5]");
                            return true;
                        }
                        Radio.send("Uh, nós temos o oficial " + Session.GetHabbo().UserName + " solicitando ajuda em " + Session.GetHabbo().CurrentRoom.RoomData.Name + " [ID: " + Session.GetHabbo().CurrentRoomId + "], todas as unidades respondam!", Session, true);
                        #endregion

                        return true;
                    }
                #endregion

                #region :spray x
                case "spray":
                    {

                        #region Generate Instances / Sessions / Vars
                        string Target = null;
                        GameClient TargetSession = null;
                        RoomUser Actor = null;
                        RoomUser Targ = null;

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida! :spray <user>", 0);
                            return true;
                        }
                        else
                        {
                            Target = Convert.ToString(Params[1]);
                        }

                        TargetSession = RoleplayManager.GenerateSession(Target);

                        if (RoleplayManager.CanInteract(Session, TargetSession, true))
                        {
                            Targ = TargetSession.GetHabbo().GetRoomUser();
                            Actor = Session.GetHabbo().GetRoomUser();
                        }
                        else
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        Vector2D Pos1 = new Vector2D(Actor.X, Actor.Y);
                        Vector2D Pos2 = new Vector2D(Targ.X, Targ.Y);

                        #endregion

                        #region Police Conditions

                        bool isclose = false;

                        if (!JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                        {
                            Session.SendWhisperBubble("Seu trabalho não pode fazer isso!", 0);
                            return true;
                        }

                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver na cadeia!");
                            return true;
                        }
                        if (Targ.Frozen)
                        {
                            Session.SendWhisper("This user is already stunned!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                        {
                            Session.SendWhisper("Não é possivel fazer isso em territorios");
                            return true;
                        }
                        #endregion

                        #region Distance Conditions
                        if (RoleplayManager.Distance(Pos1, Pos2) >= 5)
                        {
                            RoleplayManager.Shout(Session, "*Tenta pulverizar " + TargetSession.GetHabbo().UserName + " mas erra*");
                            return true;
                        }

                        if (RoleplayManager.Distance(Pos1, Pos2) <= 4)
                        {
                            isclose = true;
                        }
                        else if (RoleplayManager.Distance(Pos1, Pos2) >= 10)
                        {
                            Session.SendWhisper("Você deve estar mais perto para fazer isso!");
                            return false;
                        }
                        #endregion

                        #region Execute

                        if (isclose)
                        {

                            RoleplayManager.Shout(Session, "*Tira o meu spray e pulveriza-o em " + TargetSession.GetHabbo().UserName + " fazendo com que parem*");
                            Targ.ApplyEffect(53);
                            Targ.Frozen = true;

                        }

                        #endregion

                        return true;
                    }
                #endregion

                #region :locate x
                case "localizar":
                    {

                        #region Generate Instances / Sessions / Vars
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;

                        TargetSession = RoleplayManager.GenerateSession(Target);
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("O usuário não está online!");
                            return true;
                        }
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;
                        #endregion

                        #region Police Conditions
                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver na cadeia!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("locate_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("locate_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["locate_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["locate_cooldown"] + "/60]");
                            return true;
                        }
                        #endregion

                        #region Execute
                        bool found = false;
                        Random Rand = new Random();
                        int Chance = Rand.Next(1, 100);

                        if (Chance <= 25)
                        {
                            found = true;
                        }
                        Session.Shout("*Envia um despacho para " + TargetSession.GetHabbo().UserName + "Localização*");

                        if (found == true)
                        {
                            if (TargetSession.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                            {
                                Session.SendWhisper("[Despacho Policial] :  Conseguimos localizar com sucesso " + TargetSession.GetHabbo().UserName + " no entanto, eles foram encontrados em uma sala NOCOP! Tente mais tarde!");
                            }
                            else if (TargetSession.GetHabbo().CurrentRoom.RoomData.Description.Contains("TURF"))
                            {
                                Session.SendWhisper("[Despacho Policial] :  Conseguimos localizar com sucesso " + TargetSession.GetHabbo().UserName + " no entanto, eles foram encontrados em um perigoso quarto TURF! Tente mais tarde!");
                            }
                            else
                            {
                                Session.SendWhisper("[Despacho Policial] :  Conseguimos localizar com sucesso " + TargetSession.GetHabbo().UserName + ". Eles estão dentro " + TargetSession.GetHabbo().CurrentRoom.RoomData.Name + " [" + TargetSession.GetHabbo().CurrentRoomId + "]");
                            }
                            found = false;
                        }
                        else
                        {
                            Session.SendWhisper("[Despacho Policial]: Não conseguimos localizar essa pessoa. Parece que temos muito pouca informação sobre eles! Tente mais tarde!");
                        }

                        Session.GetRoleplay().MultiCoolDown["locate_cooldown"] = 60;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion

                #region :addwanted
                case "addwanted":
                case "law":
                case "wanted":
                case "estrelas":
                    {

                        #region Params
                        string Username = "";
                        GameClient Target = null;
                        int Time = 0;
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;
                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :estrelas <user> <time>");
                            return true;
                        }
                        else
                        {
                            Username = Params[1].ToString();
                            Time = Convert.ToInt32(Params[2]);
                            Target = RoleplayManager.GenerateSession(Username);
                        }

                        #region Null Checks
                        if (!RoleplayManager.CanInteract(Session, Target, false))
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado!");
                            return true;
                        }
                        #endregion

                        #endregion

                        #region Police Conditions

                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (Time <= 0)
                        {
                            Session.SendWhisper("Invalid time!");
                            return true;
                        }
                        int MaxTime = 0;

                        if (JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].canFire())
                        {
                            MaxTime = 30;
                        }
                        else
                        {
                            MaxTime = 30;
                        }

                        if (Time > MaxTime)
                        {
                            Session.SendWhisper("Your job rank cannot make wanted time over " + MaxTime + " minutes!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        if (RoleplayManager.WantedListData.ContainsKey(Target.GetHabbo().UserName.ToLower()))
                        {
                            Target.GetRoleplay().Wanted = Time;
                            Target.GetRoleplay().SaveQuickStat("wanted", "" + Time);
                            RoleplayManager.Shout(Session, "*Altera " + Target.GetHabbo().UserName + "tempo para " + Time + " minutos*");
                            RoleplayManager.WantedListData[Target.GetHabbo().UserName.ToLower()] = Time + "|" + 1;
                        }
                        else
                        {
                            Target.GetRoleplay().Wanted = Time;
                            Target.GetRoleplay().SaveQuickStat("wanted", "" + Time);
                            RoleplayManager.WantedListData.TryAdd(Target.GetHabbo().UserName.ToLower(), Time + "|" + Convert.ToInt32(Target.GetHabbo().CurrentRoomId));
                            RoleplayManager.Shout(Session, "*Adiciona " + Target.GetHabbo().UserName + " a lista de procurados por " + Time + " minutos*");
                        }
                        #endregion

                        return true;
                    }
                #endregion

                #region :removewanted
                case "removewanted":
                case "unlaw":
                case "unwanted":
                    {

                        #region Params
                        string Username = "";
                        GameClient Targ = null;
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :removewanted <user>");
                            return true;
                        }
                        else
                        {
                            Username = Params[1].ToString();
                            Targ = RoleplayManager.GenerateSession(Username);
                        }
                        #region Null Checks
                        if (Targ == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        if (Targ.GetHabbo() == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (Targ.GetHabbo().GetRoomUser() == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        if (Targ.GetHabbo().CurrentRoom == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        #endregion
                        #endregion

                        #region Police Conditions

                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!RoleplayManager.WantedListData.ContainsKey(Username.ToLower()))
                        {
                            Session.SendWhisper(Username.ToLower() + " was not found in the wanted list!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("removewanted_" + Username))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("removewanted_" + Username, 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["removewanted_" + Username] > 0)
                        {
                            Session.SendWhisper("To avoid abuse, you cannot remove this user from the wanted list too quickly! [" + Session.GetRoleplay().MultiCoolDown["removewanted_" + Username] + "/1000]");
                            return true;
                        }
                        #endregion

                        #region Execute
                        Targ.GetRoleplay().Wanted = 0;
                        Targ.GetRoleplay().SaveQuickStat("wanted", "0");

                        string wantedJunk = Targ.GetHabbo().UserName.ToLower();
                        RoleplayManager.WantedListData.TryRemove(Targ.GetHabbo().UserName.ToLower(), out wantedJunk);

                        RoleplayManager.Shout(Session, "*Removes " + Username.ToLower() + " to the wanted list*");
                        Session.GetRoleplay().MultiCoolDown["removewanted_" + Username] = 1000;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        #endregion

                        return true;
                    }
                #endregion

                #region :limparprocurados
                case "clearwanted":
                case "cw":
                case "limparlista":
                    {

                        #region Params
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;
                        #endregion

                        #region Police Conditions
                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }

                        #endregion

                        #region Execute
                        try
                        {
                            lock (RoleplayManager.WantedListData)
                            {
                                lock (Plus.GetGame().GetClientManager().Clients.Values)
                                {
                                    RoleplayManager.WantedListData.Clear();
                                    foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                                    {

                                        client.GetRoleplay().Wanted = 0;
                                        client.GetRoleplay().SaveQuickStat("wanted", "0");

                                    }
                                }
                            }


                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunFastQuery("UPDATE rp_stats SET wanted = 0 WHERE wanted > 0");
                            }

                            RoleplayManager.Shout(Session, "*Limpa a lista de procurados*");
                        }
                        catch (Exception) { }
                        #endregion

                        return true;
                    }
                #endregion

                #region :wantedlist
                case "wantedlist":
                case "wl":
                case "listap":
                    {

                        string Wantedlist = "=======================\nLista de Procurados\n=======================\n\n";

                        foreach (KeyValuePair<string, string> User in RoleplayManager.WantedListData)
                        {
                            string Name = User.Key;
                            string[] Split = User.Value.Split('|');
                            int Time = 5;
                            int LastSeen = 1;

                            Time = Convert.ToInt32(Split[0]);
                            LastSeen = Convert.ToInt32(Split[1]);

                            string RoomName = "";

                            Room Room = null;
                            Room = RoleplayManager.GenerateRoom(Convert.ToUInt32(LastSeen));
                            if (Room != null)
                            {
                                RoomName = Room.RoomData.Name;
                            }

                            Wantedlist += "Nick: " + Name + "\n";
                            Wantedlist += "Procurado por: " + Time + "minutos\n";
                            Wantedlist += "Visto pela última vez em: " + RoomName + " [" + LastSeen + "]\n\n";
                        }

                        Session.SendNotifWithScroll(Wantedlist);

                        return true;
                    }
                #endregion

                #region :desatordoar x
                case "unstun":
                case "unspray":
                case "datordoar":
                    {

                        #region Generate Instances / Sessions / Vars
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        RoomUser Actor = null;
                        RoomUser Targ = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        if (TargetSession != null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Targ = TargetSession.GetHabbo().GetRoomUser();
                            Actor = Session.GetHabbo().GetRoomUser();
                        }
                        else
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        Vector2D Pos1 = new Vector2D(Actor.X, Actor.Y);
                        Vector2D Pos2 = new Vector2D(Targ.X, Targ.Y);

                        #endregion

                        #region Police Conditions

                        bool isclose = false;
                        if (!JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!", false, 34);
                            return true;
                        }
                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver na cadeia!");
                            return true;
                        }
                        if (!Targ.Frozen)
                        {
                            Session.SendWhisper("This user is not stunned!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                        {
                            Session.SendWhisper("Não é possivel fazer isso em territorios");
                            return true;
                        }
                        if (JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                        {
                            if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("police"))
                            {
                                Session.SendWhisper("Can't do this in 'WESTERN' rooms.");
                                return true;
                            }
                            if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("western"))
                            {
                                Session.SendWhisper("Can only do this in 'WESTERN' rooms.");
                                return true;
                            }
                        }
                        #endregion

                        #region Distance Conditions
                        if (RoleplayManager.Distance(Pos1, Pos2) <= 4)
                        {
                            isclose = true;
                        }
                        else if (RoleplayManager.Distance(Pos1, Pos2) >= 5)
                        {
                            Session.SendWhisper("Você deve estar mais perto para fazer isso!");
                            return false;
                        }
                        #endregion

                        #region Execute

                        if (isclose)
                        {

                            RoleplayManager.Shout(Session, "*desatordoa " + TargetSession.GetHabbo().UserName + "*");
                            Targ.ApplyEffect(0);
                            Targ.CanWalk = true;
                            Targ.Frozen = false;
                            TargetSession.GetRoleplay().Cuffed = false;
                        }

                        #endregion

                        return true;
                    }
                #endregion

                #region :atordoar x
                case "atordoar":
                    {

                        #region Generate Instances / Sessions / Vars
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Invalid syntax: :stun x");
                            return true;
                        }
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        RoomUser Actor = null;
                        RoomUser Targ = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        if (TargetSession == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo() == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo().GetRoomUser() == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo().GetRoomUser().RoomId != Session.GetHabbo().GetRoomUser().RoomId)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        Targ = TargetSession.GetHabbo().GetRoomUser();
                        Actor = Session.GetHabbo().GetRoomUser();

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        Vector2D Pos1 = new Vector2D(Actor.X, Actor.Y);
                        Vector2D Pos2 = new Vector2D(Targ.X, Targ.Y);

                        #endregion

                        #region Police Conditions
                        if (!JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!", false, 34);
                            return true;
                        }
                        bool isclose = false;
                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver na cadeia!");
                            return true;
                        }
                        if (Targ.Frozen)
                        {
                            Session.SendWhisper("This user is already stunned!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                        {
                            Session.SendWhisper("Não é possivel fazer isso em territorios");
                            return true;
                        }
                        if (JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                        {


                            if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("police"))
                            {
                                Session.SendWhisper("Can't do this in 'WESTERN' rooms.");
                                return true;
                            }
                            if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("western"))
                            {
                                Session.SendWhisper("Can only do this in 'WESTERN' rooms.");
                                return true;
                            }
                        }
                        #endregion

                        #region Distance Conditions
                        if (RoleplayManager.Distance(Pos1, Pos2) >= 5)
                        {
                            RoleplayManager.Shout(Session, "*Tenta atordoar " + TargetSession.GetHabbo().UserName + " mas erra*");
                            return true;
                        }

                        if (RoleplayManager.Distance(Pos1, Pos2) <= 4)
                        {
                            isclose = true;
                        }
                        else if (RoleplayManager.Distance(Pos1, Pos2) >= 10)
                        {
                            Session.SendWhisper("Você deve estar mais perto para fazer isso!");
                            return false;
                        }
                        #endregion

                        #region Execute

                        if (isclose)
                        {

                            RoleplayManager.Shout(Session, "*Atira sua arma de choque em " + TargetSession.GetHabbo().UserName + "*");
                            TargetSession.GetRoleplay().EffectSeconds = 10;
                            TargetSession.GetRoleplay().StunnedSeconds = 10;
                            Targ.ApplyEffect(53);
                            Targ.CanWalk = false;
                            Targ.Frozen = true;
                            Targ.ClearMovement();
                            TargetSession.GetHabbo().FloodTime = Plus.GetUnixTimeStamp() + 10;
                            ServerMessage Packet = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                            Packet.AppendInteger(10);
                            TargetSession.SendMessage(Packet);

                        }

                        #endregion

                        return true;
                    }
                #endregion

                #region :algemar x
                case "cuff":
                case "algemar":
                    {

                        #region Generate Instances / Sessions / Vars
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        RoomUser Actor = null;
                        RoomUser Targ = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        if (TargetSession == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo() == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo().GetRoomUser() == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo().GetRoomUser().RoomId != Session.GetHabbo().GetRoomUser().RoomId)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        Targ = TargetSession.GetHabbo().GetRoomUser();
                        Actor = Session.GetHabbo().GetRoomUser();
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        Vector2D Pos1 = new Vector2D(Actor.X, Actor.Y);
                        Vector2D Pos2 = new Vector2D(Targ.X, Targ.Y);

                        #endregion

                        #region Police Conditions

                        bool isclose = false;

                        if (!JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!", false, 34);
                            return true;
                        }

                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver na cadeia!");
                            return true;
                        }
                        if (!Targ.Frozen)
                        {
                            Session.SendWhisper("The user must be stunned to do this!");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().Cuffed)
                        {
                            Session.SendWhisper("This user is already cuffed!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                        {
                            Session.SendWhisper("Não é possivel fazer isso em territorios");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("police"))
                        {
                            Session.SendWhisper("Can't do this in 'WESTERN' rooms.");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("western"))
                        {
                            Session.SendWhisper("Can only do this in 'WESTERN' rooms.");
                            return true;
                        }
                        #endregion

                        #region Distance Conditions
                        if (RoleplayManager.Distance(Pos1, Pos2) >= 3)
                        {
                            RoleplayManager.Shout(Session, "*Attempts to cuff " + TargetSession.GetHabbo().UserName + " but misses*");
                            return true;
                        }

                        if (RoleplayManager.Distance(Pos1, Pos2) <= 2)
                        {
                            isclose = true;
                        }
                        else if (RoleplayManager.Distance(Pos1, Pos2) >= 6)
                        {
                            Session.SendWhisper("Você deve estar mais perto para fazer isso!");
                            return false;
                        }
                        #endregion

                        #region Execute

                        if (isclose)
                        {

                            RoleplayManager.Shout(Session, "*Coloca suas algemas " + TargetSession.GetHabbo().UserName + " pulsos*");
                            TargetSession.GetRoleplay().Cuffed = true;

                        }

                        #endregion

                        return true;
                    }
                #endregion

                #region :arrest x <time>
                case "arrest":
                    {

                        #region Generate Instances / Sessions / Vars

                        bool Wanted = false;

                        string Target = Convert.ToString(Params[1]);
                        int Time = 0;
                        if (!RoleplayManager.WantedListData.ContainsKey(Target.ToLower()))
                        {
                            if (!RoleplayManager.ParamsMet(Params, 2))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :arrest <user> <time>");
                                return true;
                            }
                            else
                            {


                                #region Int Check
                                bool isint = true;
                                int outt;


                                if (Int32.TryParse(Params[2].ToString(), out outt))
                                {
                                    Time = Convert.ToInt32(Params[2]);
                                    isint = true;
                                }
                                else
                                {
                                    //  Time = Convert.ToInt32(Params[2]);
                                    isint = false;
                                }

                                if (!isint)
                                {
                                    Session.SendWhisper("The time was not a number, please try again!");
                                    return true;
                                }

                                #endregion

                            }


                        }
                        else
                        {
                            string Data = RoleplayManager.WantedListData[Target.ToLower()];
                            string[] Split = Data.Split('|');

                            foreach (string data in Split)
                            {
                                Time = Convert.ToInt32(Split[0]);
                            }

                            Wanted = true;

                        }

                        GameClient TargetSession = null;
                        RoomUser Actor = null;
                        RoomUser Targ = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        if (TargetSession == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo() == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo().GetRoomUser() == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        Targ = TargetSession.GetHabbo().GetRoomUser();
                        Actor = Session.GetHabbo().GetRoomUser();


                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        Vector2D Pos1 = new Vector2D(Actor.X, Actor.Y);
                        Vector2D Pos2 = new Vector2D(Targ.X, Targ.Y);

                        #endregion

                        #region Police Conditions

                        bool isclose = false;

                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver na cadeia!");
                            return true;
                        }
                        if (!Targ.Frozen)
                        {
                            Session.SendWhisper("The user must be stunned to do this!");
                            return true;
                        }
                        if (!TargetSession.GetRoleplay().Cuffed)
                        {
                            Session.SendWhisper("This user must be cuffed to do this!");
                            return true;
                        }
                        if (Time <= 0)
                        {
                            Session.SendWhisper("The time specified is invalid!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                        {
                            Session.SendWhisper("Não é possivel fazer isso em territorios");
                            return true;
                        }

                        int MaxTime = 0;

                        if (JobManager.JobRankData[MyJobId, MyJobRank].canFire())
                        {
                            MaxTime = 120;
                        }
                        else
                        {
                            MaxTime = 60;
                        }

                        if (Time > MaxTime)
                        {
                            Session.SendWhisper("Your job rank cannot arrest over " + MaxTime + " minutes!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("ca_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("ca_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["ca_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["ca_cooldown"] + "/3]");
                            return true;
                        }
                        #endregion

                        #region Distance Conditions
                        if (TargetSession.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot arrest dead people!");
                            return true;
                        }
                        if (RoleplayManager.Distance(Pos1, Pos2) >= 3)
                        {
                            RoleplayManager.Shout(Session, "*Attempts to arrest " + TargetSession.GetHabbo().UserName + " but misses*");
                            return true;
                        }
                        if (RoleplayManager.Distance(Pos1, Pos2) <= 2)
                        {
                            isclose = true;
                        }
                        else if (RoleplayManager.Distance(Pos1, Pos2) >= 6)
                        {
                            Session.SendWhisper("Você deve estar mais perto para fazer isso!");
                            return false;
                        }
                        #endregion

                        #region Execute

                        if (isclose)
                        {
                            string Extra = "";


                            if (Wanted)
                            {
                                Session.SendWhisper("Since the user was already wanted, their wanted time has been applied instead of the one you specified!");
                                Extra += ", and removes them from the wanted list";
                                TargetSession.GetRoleplay().Wanted = 0;
                                TargetSession.GetRoleplay().SaveQuickStat("wanted", "0");

                                string wantedJunk = TargetSession.GetHabbo().UserName.ToLower();
                                RoleplayManager.WantedListData.TryRemove(TargetSession.GetHabbo().UserName.ToLower(), out wantedJunk);
                            }


                            RoomUser User = TargetSession.GetHabbo().GetRoomUser();
                            User.ApplyEffect(0);
                            TargetSession.GetRoleplay().StopWork();

                            if (Plus.GetGame().JailBreak != null)
                            {
                                Plus.GetGame().JailBreak.StopJailbreak();
                            }

                            TargetSession.GetRoleplay().Equiped = null;
                            User.Frozen = false;

                            RoleplayManager.Shout(Session, "*Arrests " + TargetSession.GetHabbo().UserName + " for " + Time + " minute(s)" + Extra + "*");
                            TargetSession.GetRoleplay().Cuffed = false;
                            TargetSession.SendNotif("You have been arrested by " + Session.GetHabbo().UserName + " for " + Time + " minute(s)");
                            TargetSession.GetRoleplay().JailFigSet = false;
                            TargetSession.GetRoleplay().JailedSeconds = 60;
                            TargetSession.GetRoleplay().JailTimer = Time;
                            TargetSession.GetRoleplay().Jailed = true;
                            TargetSession.GetRoleplay().Arrested++;
                            TargetSession.GetRoleplay().UpdateStats++;

                            //Fines
                            int intFines = Time / 5;
                            TargetSession.GetHabbo().Credits = TargetSession.GetHabbo().Credits - intFines;
                            TargetSession.GetHabbo().UpdateCreditsBalance();

                            Session.GetRoleplay().Arrests++;
                            Session.GetRoleplay().UpdateStats++;
                            Session.GetRoleplay().MultiCoolDown["ca_cooldown"] = 3;
                            Session.GetRoleplay().CheckingMultiCooldown = true;

                            if (TargetSession.GetRoleplay().Working)
                            {
                                TargetSession.GetRoleplay().StopWork();
                            }
                            TargetSession.GetRoleplay().SaveStatusComponents("jailed");

                        }

                        #endregion

                        return true;
                    }
                #endregion

                #region :release x
                case "release":
                    {

                        #region Generate Instances / Sessions / Vars
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        RoomUser Actor = null;
                        RoomUser Targ = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        if (TargetSession != null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Targ = TargetSession.GetHabbo().GetRoomUser();
                            Actor = Session.GetHabbo().GetRoomUser();
                        }
                        else
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        Vector2D Pos1 = new Vector2D(Actor.X, Actor.Y);
                        Vector2D Pos2 = new Vector2D(Targ.X, Targ.Y);

                        #endregion

                        #region Police Conditions

                        bool isclose = false;

                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver na cadeia!");
                            return true;
                        }
                        if (TargetSession.GetHabbo().UserName == Session.GetHabbo().UserName)
                        {
                            Session.SendWhisper("You cannot release yourself from jail!");
                            return true;
                        }
                        if (!TargetSession.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("This user is not jailed!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("police"))
                        {
                            Session.SendWhisper("Can't release in the 'WESTERN' Jail!");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("western"))
                        {
                            Session.SendWhisper("Can only release in the 'WESTERN' Jail!");
                            return true;
                        }
                        #endregion

                        #region Distance Conditions

                        isclose = true;

                        #endregion

                        #region Execute

                        if (isclose)
                        {

                            RoleplayManager.Shout(Session, "*Releases " + TargetSession.GetHabbo().UserName + " from jail*");
                            TargetSession.GetRoleplay().Cuffed = true;
                            TargetSession.GetRoleplay().JailTimer = 0;
                            TargetSession.GetRoleplay().JailedSeconds = 0;
                            TargetSession.GetRoleplay().SaveStatusComponents("jailed");

                        }

                        #endregion

                        return true;
                    }
                #endregion

                #region :search x
                case "search":
                    {

                        #region Generate Instances / Sessions / Vars
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        RoomUser Actor = null;
                        RoomUser Targ = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Null Checks
                        if (!RoleplayManager.CanInteract(Session, TargetSession, true))
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        #endregion

                        #region Set Values
                        Targ = TargetSession.GetHabbo().GetRoomUser();
                        Actor = Session.GetHabbo().GetRoomUser();

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        Vector2D Pos1 = new Vector2D(Actor.X, Actor.Y);
                        Vector2D Pos2 = new Vector2D(Targ.X, Targ.Y);
                        #endregion

                        #region Police Conditions

                        bool isclose = false;

                        if (!Session.GetRoleplay().JobHasRights("police")
                            && !Session.GetRoleplay().JobHasRights("gov")
                            && !Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisper("Seu trabalho não pode fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver morto!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode fazer isso enquanto estiver na cadeia!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOSEARCH"))
                        {
                            Session.SendWhisper("You cannot do this while in a 'NOSEARCH' zone!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                        {
                            Session.SendWhisper("You cannot do this while in a 'NOCOP' zone!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("police"))
                        {
                            Session.SendWhisper("You cannot do this while in a 'WESTERN' zone!");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("WESTERN") && Session.GetRoleplay().JobHasRights("western"))
                        {
                            Session.SendWhisper("Can only do this in 'WESTERN' rooms.");
                            return true;
                        }
                        #endregion

                        #region Distance Conditions
                        if (RoleplayManager.Distance(Pos1, Pos2) >= 3)
                        {
                            RoleplayManager.Shout(Session, "*Attempts to search " + TargetSession.GetHabbo().UserName + " but misses*");
                            return true;
                        }

                        if (RoleplayManager.Distance(Pos1, Pos2) <= 2)
                        {
                            isclose = true;
                        }
                        else if (RoleplayManager.Distance(Pos1, Pos2) >= 7)
                        {
                            Session.SendWhisper("Você deve estar mais perto para fazer isso!");
                            return false;
                        }
                        #endregion

                        #region Execute

                        if (isclose)
                        {

                            bool IllegalItemsFound = false;
                            string IllegalItem = "";

                            if (TargetSession.GetRoleplay().Weed > 0)
                            {
                                IllegalItem += ", " + TargetSession.GetRoleplay().Weed + "g of weed";
                                IllegalItemsFound = true;
                            }
                            if (TargetSession.GetRoleplay().Equiped != null)
                            {
                                IllegalItem += ", holding a " + TargetSession.GetRoleplay().Equiped + " weapon";
                                IllegalItemsFound = true;
                            }

                            if (!IllegalItemsFound)
                            {
                                IllegalItem = ", and notices that they have none";
                            }

                            RoleplayManager.Shout(Session, "*Searches " + TargetSession.GetHabbo().UserName + " for any illegal items" + IllegalItem + "*");
                        }

                        #endregion

                        return true;
                    }
                #endregion

                #endregion

                #region Banking(5)

                #region :balance
                case "saldo":
                    {

                        #region Conditions
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("balance_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("balance_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["balance_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["balance_cooldown"] + "/5]");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Abre o telefone, clica em internet banking * ", 4);
                        RoomUser u = Session.GetHabbo().GetRoomUser();
                        Session.GetRoleplay().EffectSeconds = 4;
                        u.ApplyEffect(65);
                        RoleplayManager.Shout(Session, "*Olha para o seu saldo bancário e verifica que tem: $" + Session.GetRoleplay().Bank + "*", 6);
                        Session.GetRoleplay().MultiCoolDown["balance_cooldown"] = Convert.ToInt32(RoleplayData.Data["balance_cooldown"]);
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        #endregion

                        return true;
                    }
                #endregion // Updated by hender

                #region :withdraw <amnt>
                case "retirar":
                    {

                        #region Params
                        int Amount = 1;
                        bool wFromBank = false;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :retirar <quantidade>");
                            return true;
                        }
                        else
                        {

                            if (!Plus.IsNum(Params[1]))
                            {
                                Session.SendWhisperBubble("A entrada deve ser um número!", 34);
                                return true;
                            }

                            Amount = Convert.ToInt32(Params[1]);

                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("withdraw_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("withdraw_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["withdraw_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["withdraw_cooldown"] + "/5]");
                            return true;
                        }

                        if (Amount <= 0)
                        {
                            Session.SendWhisper("Montante inválido!");
                            return true;
                        }
                        if (Session.GetRoleplay().Bank < Amount || Session.GetRoleplay().Bank - Amount < 0)
                        {
                            Session.SendWhisper("Você não tem $" + Amount + " na sua conta bancária!");
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("BANK"))
                        {
                            if (Session.GetRoleplay().Phone == 1)
                            {
                                wFromBank = false;
                            }
                            else
                            {
                                Session.SendWhisperBubble("Você deve ter um telefone para usar o banco on-line!", 1);
                                return true;
                            }
                        }
                        else if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("BANK"))
                        {
                            wFromBank = true;
                        }
                        #endregion

                        #region Execute
                        if (wFromBank)
                        {
                            RoleplayManager.Shout(Session, "*Retira $" + Amount + " da sua conta do Banco*", 6);
                            RoleplayManager.GiveMoney(Session, +Amount);
                            Session.GetRoleplay().SaveQuickStat("bank", "" + (Session.GetRoleplay().Bank - Amount));
                            Session.GetRoleplay().Bank -= Amount;
                            Session.GetRoleplay().AtmSetAmount = 0;
                            Session.GetRoleplay().WithdrawDelay = 0;
                            Session.GetRoleplay().Withdraw_Via_Phone = false;
                            RoomUser RoomUser = null;
                            RoomUser = Session.GetHabbo().GetRoomUser();
                            RoomUser.ApplyEffect(0);
                            return true;
                        }
                        else
                        {
                            Session.SendWhisperBubble("Você deve estar no banco [RoomID: 13] para sacar seu dinheiro.", 1);
                            return true;
                            /*Misc.Shout(Session, "*Takes out their phone*");
                            RoomUser User =Session.GetHabbo().GetRoomUser();
                            User.ApplyEffect(65);
                            Session.GetRoleplay().WithdrawDelay = 1;
                            Session.GetRoleplay().Withdraw_Via_Phone = true;
                            Session.GetRoleplay().AtmSetAmount = Amount;
                            Session.GetRoleplay().MultiCoolDown["withdraw_cooldown"] = 5;
                            Session.GetRoleplay().CheckingMultiCooldown = true;*/
                        }
                        #endregion
                    }
                #endregion

                #region :deposit <amnt>
                case "depositar":
                    {

                        #region Params
                        int Amount = 1;
                        bool dFromBank = false;
                        #endregion

                        #region Conditions
                        if (Params.Length < 2)
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :deposit <amount>", 1);
                            return true;
                        }
                        else
                        {
                            Amount = Convert.ToInt32(Params[1]);
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("deposit_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("deposit_cooldown", 0);

                        }
                        if (Session.GetRoleplay().MultiCoolDown["deposit_cooldown"] > 0)
                        {
                            Session.SendWhisperBubble("Cooldown [" + Session.GetRoleplay().MultiCoolDown["deposit_cooldown"] + "/5]", 1);
                            return true;
                        }
                        if (Amount <= 0)
                        {
                            Session.SendWhisperBubble("Montante invalido!", 1);
                            return true;
                        }
                        if (Session.GetHabbo().Credits < Amount || Session.GetHabbo().Credits - Amount < 0)
                        {
                            Session.SendWhisperBubble("Você não tem $" + Amount + " para depositar!", 1);
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("BANK"))
                        {
                            if (Session.GetRoleplay().Phone == 1)
                            {
                                dFromBank = false;
                            }
                            else
                            {
                                Session.SendWhisperBubble("Você deve ter um telefone para usar o banco on-line! Compre um na loja de telefone [ID do quarto: 5]", 1);
                                return true;
                            }
                        }
                        else if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("BANK"))
                        {
                            dFromBank = true;
                        }
                        #endregion

                        #region Execute
                        if (dFromBank)
                        {
                            RoleplayManager.Shout(Session, "*Deposita $" + Amount + "em sua conta bancária * ", 6);
                            RoleplayManager.GiveMoney(Session, -Amount);
                            Session.GetRoleplay().SaveQuickStat("bank", "" + (Session.GetRoleplay().Bank + Amount));
                            Session.GetRoleplay().Bank += Amount;
                            Session.GetRoleplay().MultiCoolDown["deposit_cooldown"] = Convert.ToInt32(RoleplayData.Data["deposit_cooldown"]);
                            return true;
                        }
                        else
                        {
                            Session.SendWhisperBubble("Você deve estar no banco [RoomID: 13] para depositar seu dinheiro.", 1);
                            return true;
                            /*Misc.Shout(Session, "*Deposits $" + Amount + " to their Bank Account*");
                            Misc.GiveMoney(Session, -Amount);
                            Session.GetRoleplay().SaveQuickStat("bank", "" + (Session.GetRoleplay().Bank + Amount));
                            Session.GetRoleplay().Bank += Amount;
                            Session.GetRoleplay().MultiCoolDown["deposit_cooldown"] = 5;
                            Session.GetRoleplay().CheckingMultiCooldown = true;*/
                        }
                        #endregion

                    }
                #endregion

                #region :setatm
                case "setatm":
                    {

                        int Amnt = 0;

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :setatm <amount>", 1);
                            return true;
                        }
                        else
                        {
                            Amnt = Convert.ToInt32(Params[1]);
                        }
                        if (Session.GetRoleplay().WithdrawDelay > 0)
                        {
                            Session.SendWhisperBubble("Já estamos processando sua transação atual..", 1);
                            return true;
                        }
                        if (Amnt <= 0)
                        {
                            Session.SendWhisperBubble("A quantia não pode ser menor ou igual a 0!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Bank < Amnt)
                        {
                            Session.SendWhisperBubble("Você não tem $" + Amnt + " no seu banco!", 1);
                            return true;
                        }
                        Session.GetRoleplay().AtmSetAmount = Amnt;
                        Session.SendWhisperBubble("Definir com sucesso o montante de levantamento do ATM para $" + Amnt + ". Clique duas vezes no caixa eletrônico para retirar!", 1);



                        return true;
                    }
                #endregion

                #endregion

                #region Hospital (2)

                #region :heal x
                case "curar":
                    {
                        #region Generate Instances / Sessions / Vars
                        string Target = null;

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :curar <user>");
                            return true;
                        }

                        Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        RoomUser Actor = null;
                        RoomUser Targ = null;

                        TargetSession = RoleplayManager.GenerateSession(Target);

                        #region Null Checks
                        if (!RoleplayManager.CanInteract(Session, TargetSession, true))
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        #endregion

                        Targ = TargetSession.GetHabbo().GetRoomUser();
                        Actor = Session.GetHabbo().GetRoomUser();

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        Vector2D Pos1 = new Vector2D(Actor.X, Actor.Y);
                        Vector2D Pos2 = new Vector2D(Targ.X, Targ.Y);

                        #endregion

                        #region Hospital Conditions

                        bool isclose = false;

                        if (!Session.GetRoleplay().JobHasRights("hospital") && !Session.GetRoleplay().JobHasRights("gov"))
                        {
                            Session.SendWhisperBubble("Seu trabalho não pode fazer isso!", 1);
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisperBubble("Você deve estar trabalhando para fazer isso!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisperBubble("Você não pode fazer isso enquanto estiver morto!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisperBubble("Você não pode fazer isso enquanto estiver na cadeia!", 1);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().BeingHealed)
                        {
                            Session.SendWhisperBubble("Este usuário já está sendo curado!", 1);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().CurHealth >= TargetSession.GetRoleplay().MaxHealth)
                        {
                            Session.SendWhisperBubble("A saúde deste usuário já está cheia!", 1);
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("heal_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("heal_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["heal_cooldown"] > 0)
                        {
                            Session.SendWhisperBubble("Cooldown [" + Session.GetRoleplay().MultiCoolDown["heal_cooldown"] + "/10]", 1);
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :curar <username>", 1);
                            return true;
                        }
                        #endregion

                        #region Distance Conditions
                        if (RoleplayManager.Distance(Pos1, Pos2) <= 4)
                        {
                            isclose = true;
                        }
                        else if (RoleplayManager.Distance(Pos1, Pos2) >= 5)
                        {
                            Session.SendWhisperBubble("Você deve estar mais perto para fazer isso!", 1);
                            return true;
                        }
                        #endregion

                        #region Execute
                        if (isclose)
                        {

                            RoleplayManager.GiveMoney(Session, +2);
                            RoleplayManager.Shout(Session, "*Aplica algumas bandagens em " + TargetSession.GetHabbo().UserName + "curando suas feridas [+$2]*");
                            TargetSession.GetRoleplay().BeingHealed = true;
                            TargetSession.GetRoleplay().healTimer = new healTimer(TargetSession);
                            
                            Session.GetRoleplay().MultiCoolDown["heal_cooldown"] = 10;
                            Session.GetRoleplay().CheckingMultiCooldown = true;

                        }
                        #endregion

                        return true;
                    }
                #endregion

                #region :discharge x
                case "reviver":
                    {

                        #region Generate Instances / Sessions / Vars
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        RoomUser Actor = null;
                        RoomUser Targ = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #region Null Checks
                        if (TargetSession == null)
                        {
                            Session.SendWhisperBubble("O usuário não foi encontrado nesta sala!", 1);
                            return true;
                        }
                        if (TargetSession.GetHabbo() == null)
                        {
                            Session.SendWhisperBubble("O usuário não foi encontrado nesta sala!", 1);
                            return true;
                        }

                        if (TargetSession.GetHabbo().GetRoomUser() == null)
                        {
                            Session.SendWhisperBubble("O usuário não foi encontrado nesta sala!", 1);
                            return true;
                        }
                        if (TargetSession.GetHabbo().CurrentRoom == null)
                        {
                            Session.SendWhisperBubble("O usuário não foi encontrado nesta sala!", 1);
                            return true;
                        }
                        if (TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                        {
                            Session.SendWhisperBubble("O usuário não foi encontrado nesta sala!", 1);
                            return true;
                        }
                        #endregion
                        Targ = TargetSession.GetHabbo().GetRoomUser();
                        Actor = Session.GetHabbo().GetRoomUser();
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        Vector2D Pos1 = new Vector2D(Actor.X, Actor.Y);
                        Vector2D Pos2 = new Vector2D(Targ.X, Targ.Y);

                        #endregion

                        #region Hospital Conditions

                        bool isclose = false;

                        if (!Session.GetRoleplay().JobHasRights("hospital") && !Session.GetRoleplay().JobHasRights("gov") && Session.GetHabbo().Rank <= 2)
                        {
                            Session.SendWhisperBubble("Seu trabalho não pode fazer isso!", 1);
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisperBubble("Você deve estar trabalhando para fazer isso!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisperBubble("Você não pode fazer isso enquanto estiver morto!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisperBubble("Você não pode fazer isso enquanto estiver na cadeia!", 1);
                            return true;
                        }
                        if (!TargetSession.GetRoleplay().Dead)
                        {
                            Session.SendWhisperBubble("Este usuário não está morto!", 1);
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("d_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("d_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["d_cooldown"] > 0)
                        {
                            Session.SendWhisperBubble("Cooldown [" + Session.GetRoleplay().MultiCoolDown["d_cooldown"] + "/3]", 1);
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :reviver <username>", 1);
                            return true;
                        }
                        #endregion

                        #region Distance Conditions
                        if (RoleplayManager.Distance(Pos1, Pos2) <= 6)
                        {
                            isclose = true;
                        }
                        else if (RoleplayManager.Distance(Pos1, Pos2) >= 7)
                        {
                            Session.SendWhisperBubble("Você deve estar mais perto para fazer isso!", 1);
                            return true;
                        }
                        #endregion

                        #region Execute
                        if (isclose)
                        {

                            RoleplayManager.GiveMoney(Session, +Session.GetRoleplay().JobRank);
                            TargetSession.GetHabbo().GetRoomUser().SetPos(9, 26, 0);
                            TargetSession.GetHabbo().GetRoomUser().ClearMovement();
                            RoleplayManager.Shout(Session, "*Revive " + TargetSession.GetHabbo().UserName + " do hospital [+$" + Session.GetRoleplay().JobRank + "]*");
                            TargetSession.GetRoleplay().DeadTimer = 0;
                            TargetSession.GetRoleplay().DeadSeconds = 0;
                            TargetSession.GetRoleplay().SaveStatusComponents("dead");
                            TargetSession.GetRoleplay().healTimer = new healTimer(TargetSession);
                            TargetSession.GetRoleplay().BeingHealed = true;
                            Session.GetRoleplay().MultiCoolDown["d_cooldown"] = 3;
                            Session.GetRoleplay().CheckingMultiCooldown = true;

                        }
                        #endregion

                        return true;
                    }
                #endregion

                #endregion

                #region SWAT (1)

                #region :flashbang

                case "flashbang":
                    {

                        #region Conditions

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        if (!Session.GetRoleplay().JobHasRights("swat"))
                        {
                            Session.SendWhisperBubble("Seu trabalho não pode fazer isso!", 1);
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisperBubble("Você deve estar trabalhando para fazer isso!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisperBubble("Você não pode fazer isso enquanto estiver morto!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisperBubble("Você não pode fazer isso enquanto estiver na cadeia!", 1);
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                        {
                            Session.SendWhisperBubble("Não é possivel fazer isso em territorios", 1);
                            return true;
                        }

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("flashbang"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("flashbang", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["flashbang"] > 0)
                        {
                            Session.SendWhisperBubble("Para evitar abusos, você não pode usar flashbang muito rapidamente! [" + Session.GetRoleplay().MultiCoolDown["flashbang"] + "/600]", 1);
                            return true;
                        }

                        #endregion

                        #region Execute

                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (client == null)
                                    continue;
                                if (client.GetHabbo() == null)
                                    continue;
                                if (client.GetRoleplay() == null)
                                    continue;
                                if (client.GetHabbo().CurrentRoom == null)
                                    continue;
                                if (client.GetHabbo().CurrentRoom == Session.GetHabbo().CurrentRoom)
                                {
                                    if (client.GetHabbo().UserName != Session.GetHabbo().UserName && !Session.GetRoleplay().usingPlane)
                                    {
                                        client.GetHabbo().GetRoomUser().ApplyEffect(53);
                                        client.GetHabbo().GetRoomUser().CanWalk = false;
                                        client.GetHabbo().GetRoomUser().Frozen = true;
                                        client.GetHabbo().FloodTime = Plus.GetUnixTimeStamp() + 10;
                                        ServerMessage Packet = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                                        Packet.AppendInteger(10);
                                        client.SendMessage(Packet);
                                    }
                                }
                            }
                        }
                        RoleplayManager.Shout(Session, "*Joga uma granada de atordoamento, atordoando todo mundo na sala*");
                        Session.GetRoleplay().MultiCoolDown["flashbang"] = 600;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                    }
                    return true;
                #endregion

                #endregion

                #region Spa (1)
                #region :massage x
                case "massagem":
                    {
                        #region Generate Instances / Sessions / Vars
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        RoomUser Actor = null;
                        RoomUser Targ = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        if (TargetSession != null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Targ = TargetSession.GetHabbo().GetRoomUser();
                            Actor = Session.GetHabbo().GetRoomUser();
                        }
                        else
                        {
                            Session.SendWhisperBubble("O usuário não foi encontrado nesta sala!", 1);
                            return true;
                        }
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        Vector2D Pos1 = new Vector2D(Actor.X, Actor.Y);
                        Vector2D Pos2 = new Vector2D(Targ.X, Targ.Y);

                        #endregion

                        #region Spa Conditions

                        bool isclose = false;

                        if (!Session.GetRoleplay().JobHasRights("spa") && !Session.GetRoleplay().JobHasRights("gov") && Session.GetHabbo().Rank <= 2)
                        {
                            Session.SendWhisperBubble("Seu trabalho não pode fazer isso!", 1);
                            return true;
                        }
                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisperBubble("Você deve estar trabalhando para fazer isso!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisperBubble("Você não pode fazer isso enquanto estiver morto!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisperBubble("Você não pode fazer isso enquanto estiver na cadeia!", 1);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().Energy == 100)
                        {
                            Session.SendWhisperBubble("Essa energia dos usuários já está cheia!", 1);
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("massage_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("massage_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["massage_cooldown"] > 0)
                        {
                            Session.SendWhisperBubble("Cooldown [" + Session.GetRoleplay().MultiCoolDown["massage_cooldown"] + "/10]", 1);
                            return true;
                        }
                        #endregion

                        #region Distance Conditions
                        if (RoleplayManager.Distance(Pos1, Pos2) <= 4)
                        {
                            isclose = true;
                        }
                        else if (RoleplayManager.Distance(Pos1, Pos2) >= 5)
                        {
                            Session.SendWhisperBubble("Você deve estar mais perto para fazer isso!", 1);
                            return false;
                        }
                        #endregion

                        #region Execute
                        if (isclose)
                        {
                            RoleplayManager.GiveMoney(Session, +2);
                            RoleplayManager.Shout(Session, "*Massageia " + TargetSession.GetHabbo().UserName + "que estava com o corpo dolorido [+$2]*");
                            TargetSession.GetRoleplay().BeingMassaged = true;
                            TargetSession.GetRoleplay().massageTimer = new massageTimer(TargetSession);
                            Session.GetRoleplay().MultiCoolDown["massage_cooldown"] = 10;
                            Session.GetRoleplay().CheckingMultiCooldown = true;
                        }
                        #endregion

                        return true;
                    }
                #endregion
                #endregion

                #endregion

                #region General (3)

                #region :trabalhar
                case "trabalhar":
                    {
                        int JobId = Session.GetRoleplay().JobId;
                        int JobRank = Session.GetRoleplay().JobRank;

                        #region Invalid Job Rectifier
                        if (!JobValidation.ValidateJob(JobId, JobRank))
                        {
                            Session.SendWhisperBubble("Our System has detected. Your Job is invalid and does not exist / was deleted! It has been reset! Try and start work again!", 1);
                            Session.GetRoleplay().JobId = 1;
                            Session.GetRoleplay().JobRank = 1;
                            Session.GetRoleplay().SaveJobComponents();
                            return true;
                        }
                        #endregion

                        #region Conditions

                        if (Session.GetRoleplay().JobHasRights("police") || Session.GetRoleplay().JobHasRights("swat"))
                        {
                            if (RoleplayManager.PurgeTime)
                            {
                                Session.SendWhisperBubble("You cannot work as an officer during a purge!", 1);
                                return true;
                            }
                            if (Session.GetRoleplay().IsNoob)
                            {
                                Session.SendWhisperBubble("You cannot fulfil your duties in Law Enforcement with God Protection Enabled!", 1);
                                return true;
                            }
                            if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOCOP"))
                            {
                                Session.SendWhisperBubble("Sorry, but this is a NOCOP zone", 1);
                                return true;
                            }
                        }
                        if (Session.GetRoleplay().SentHome)
                        {
                            Session.SendWhisperBubble("You were sent home by a manager. You must wait " + Session.GetRoleplay().SendHomeTimer + " minutes to begin working again", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Working)
                        {
                            Session.SendWhisperBubble("You are already working!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().ATMRobbery)
                        {
                            Session.SendWhisperBubble("You can't work while robbing an ATM!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Robbery)
                        {
                            Session.SendWhisperBubble("You can't work while robbing the vault!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Learning)
                        {
                            Session.SendWhisperBubble("You can't work while learning!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().WeightLifting)
                        {
                            Session.SendWhisperBubble("You cannot work while weight lifting!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().WorkingOut)
                        {
                            Session.SendWhisperBubble("You cannot work while working out!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisperBubble("You cannot work while you are dead!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisperBubble("You cannot work while you are jailed!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().JobId == 3)
                        {
                            Session.SendWhisper("You must use Police Tools to start working");
                            return true;
                        }
                        if (!JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].WorkRooms.Contains("*"))
                        {
                            if (!JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].isWorkRoom(Session.GetHabbo().CurrentRoomId))
                            {
                                Session.SendWhisperBubble("You do not work in this place!", 1);
                                return true;
                            }
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("work_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("work_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["work_cooldown"] > 0)
                        {
                            Session.SendWhisperBubble("You must wait until you can rob the bank again! [" + Session.GetRoleplay().MultiCoolDown["work_cooldown"] + "/5]", 1);
                            return true;
                        }
                        #endregion

                        #region Execute

                        string JobName = JobManager.JobData[JobId].Name;
                        string RankName = JobManager.JobRankData[JobId, JobRank].Name;

                        #region Apply Work Outfit/Motto
                        Session.GetRoleplay().FigBeforeWork = Session.GetHabbo().Look;
                        Session.GetRoleplay().MottBeforeWork = Session.GetHabbo().Motto;

                        if (Session.GetRoleplay().JobId != 1 && !JobManager.JobRankData[JobId, JobRank].MaleFig.Contains("*") && !JobManager.JobRankData[JobId, JobRank].FemaleFig.Contains("*")) // Set Figure if not Unemployed
                        {
                            if (!JobManager.JobRankData[JobId, JobRank].MaleFig.ToLower().Contains("none") && !JobManager.JobRankData[JobId, JobRank].FemaleFig.ToLower().Contains("none"))
                            {
                                if (Session.GetHabbo().Gender.ToLower().StartsWith("m"))
                                {
                                    Session.GetHabbo().Look = RoleplayManager.SplitFigure(Session.GetHabbo().Look) + JobManager.JobRankData[JobId, JobRank].MaleFig;
                                    Session.GetRoleplay().FigWork = RoleplayManager.SplitFigure(Session.GetHabbo().Look) + JobManager.JobRankData[JobId, JobRank].MaleFig;
                                }
                                else
                                {
                                    Session.GetHabbo().Look = RoleplayManager.SplitFigure(Session.GetHabbo().Look) + JobManager.JobRankData[JobId, JobRank].FemaleFig;
                                    Session.GetRoleplay().FigWork = RoleplayManager.SplitFigure(Session.GetHabbo().Look) + JobManager.JobRankData[JobId, JobRank].FemaleFig;
                                }
                            }
                        }
                        #endregion

                        Session.GetHabbo().Motto = "[Trabalhando] " + JobName + " " + RankName;
                        Session.GetRoleplay().RefreshVals();
                        Session.GetRoleplay().Working = true;
                        Session.GetRoleplay().workingTimer = new workingTimer(Session);
                        Session.GetRoleplay().SaveJobComponents();
                        RoleplayManager.Shout(Session, "*Começa a trabalhar como " + RankName + "*", 4);
                        Session.SendWhisperBubble("Você tem " + Session.GetRoleplay().workingTimer.getTime() + " minuto(s) até que você seja pago.", 1);
                        Session.GetRoleplay().MultiCoolDown["work_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion
                #region :stopwork
                case "ptrabalhar":
                    {

                        #region Conditions

                        if (!Session.GetRoleplay().Working)
                        {
                            Session.SendWhisperBubble("Você nem está trabalhando para começar!", 1);
                            return true;
                        }
                        if (Session.GetRoleplay().JobId == 3)
                        {
                            Session.SendWhisper("Use suas ferramentas!");
                            return true;
                        }

                        #endregion

                        Session.GetRoleplay().StopWork(true);

                        if (Session.GetRoleplay().Equiped != null)
                        {
                            if (Session.GetRoleplay().Equiped.ToLower().Equals("npa"))
                            {
                                Session.GetHabbo().GetRoomUser().ApplyEffect(0);
                                Session.GetRoleplay().Equiped = null;
                            }
                        }

                        return true;
                    }
                #endregion
                #region :quitjob
                case "demissao":
                    {

                        #region Conditions
                        if (Session.GetRoleplay().JobId <= 1)
                        {
                            Session.SendWhisperBubble("Você não tem um trabalho para se demitir!", 1);
                            return true;
                        }
                        #endregion

                        #region Execute
                        if (Session.GetRoleplay().Working)
                        {
                            Session.GetRoleplay().StopWork();
                        }

                        RoleplayManager.Shout(Session, "*Se demite do seu emprego*", 3);
                        Session.GetRoleplay().JobId = 1;
                        Session.GetRoleplay().JobRank = 1;
                        Session.GetRoleplay().SaveJobComponents();

                        Session.GetRoleplay().Shifts = 0;
                        Session.GetRoleplay().SaveQuickStat("shifts_completed", "" + Session.GetRoleplay().Shifts);

                        if (Session.GetRoleplay().Equiped != null)
                        {
                            if (Session.GetRoleplay().Equiped.ToLower().Equals("npa"))
                            {
                                Session.GetHabbo().GetRoomUser().ApplyEffect(0);
                                Session.GetRoleplay().Equiped = null;
                            }
                        }
                        #endregion

                        return true;
                    }
                #endregion

                #endregion

                #region Corporation Management (5)

                #region :hire x
                case "contratar":
                    {

                        #region Some Vars

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        #endregion

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado nesta sala!", 1);
                            return true;
                        }
                        if (!JobManager.JobRankData[MyJobId, MyJobRank].canHire())
                        {
                            Session.SendWhisperBubble("Your job rank cannot hire people!", 1);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().JobId > 1)
                        {
                            Session.SendWhisperBubble("This user already has a job!", 1);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().JobId == Session.GetRoleplay().JobId && TargetSession.GetRoleplay().JobRank == 1)
                        {
                            Session.SendWhisperBubble("This user already works as this job!", 1);
                            return true;
                        }
                        #endregion

                        #region Execute

                        string Job = JobManager.JobData[MyJobId].Name;
                        string Rank = JobManager.JobRankData[MyJobId, 1].Name;

                        RoleplayManager.Shout(Session, "*contrata " + TargetSession.GetHabbo().UserName + " na empresa " + Job + " " + Rank + "*", 4);
                        TargetSession.GetRoleplay().JobId = MyJobId;
                        TargetSession.GetRoleplay().JobRank = 1;
                        TargetSession.GetRoleplay().SaveJobComponents();

                        #endregion

                        return true;
                    }
                #endregion
                #region :fire x
                case "demitir":
                    {
                        #region Some Vars

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        #endregion

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        if (TargetSession != null)
                        {

                            #region Online Fire

                            #region Conditions (Online)
                            if (!Session.GetHabbo().HasFuse("fuse_mod"))
                            {
                                if (!JobManager.JobRankData[MyJobId, MyJobRank].canFire())
                                {
                                    Session.SendWhisperBubble("Your job rank cannot fire people!", 1);
                                    return true;
                                }
                                if (TargetSession.GetRoleplay().JobId != MyJobId)
                                {
                                    Session.SendWhisperBubble("This user does not work for you!", 1);
                                    return true;
                                }
                                if (TargetSession.GetRoleplay().JobId == 1)
                                {
                                    Session.SendWhisperBubble("This user is already unemployed.", 1);
                                    return true;
                                }
                                if (JobManager.JobRankData[TargetSession.GetRoleplay().JobId, TargetSession.GetRoleplay().JobRank].canFire() && JobManager.JobData[MyJobId].OwnerId != Convert.ToInt32(Session.GetHabbo().Id))
                                {
                                    Session.SendWhisperBubble("You do not have sufficient rights to do this!", 1);
                                    return true;
                                }
                            }

                            if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                            {
                                Session.SendWhisperBubble("Este usuário não foi encontrado nesta sala!", 1);
                                return true;
                            }
                            #endregion

                            #region Execute (Online)

                            string Job = JobManager.JobData[MyJobId].Name;
                            string Rank = JobManager.JobRankData[MyJobId, 1].Name;

                            if (TargetSession.GetRoleplay().Working)
                            {
                                TargetSession.GetRoleplay().StopWork();
                            }

                            RoleplayManager.Shout(Session, "*Demite " + TargetSession.GetHabbo().UserName + " do seu trabalho*", 4);
                            TargetSession.GetRoleplay().JobId = 1;
                            TargetSession.GetRoleplay().JobRank = 1;
                            TargetSession.GetRoleplay().SaveJobComponents();

                            TargetSession.GetRoleplay().Shifts = 0;
                            TargetSession.GetRoleplay().SaveQuickStat("shifts_completed", "" + Session.GetRoleplay().Shifts);

                            #endregion

                            #endregion

                        }
                        else
                        {

                            #region Offline Fire

                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {

                                dbClient.SetQuery("SELECT id FROM users WHERE username = '" + Target + "'");
                                int targetid = dbClient.GetInteger();

                                dbClient.SetQuery("SELECT username FROM users WHERE username = '" + Target + "'");
                                string username = dbClient.GetString();

                                dbClient.SetQuery("SELECT job_id, job_rank FROM rp_stats WHERE id = '" + targetid + "'");
                                DataRow TarGetRow = dbClient.GetRow();

                                #region Conditions (Offline)
                                if (TarGetRow == null)
                                {
                                    Session.SendWhisperBubble("This user does not exist!", 1);
                                    return true;
                                }

                                if (!Session.GetHabbo().HasFuse("fuse_mod"))
                                {
                                    if (!JobManager.JobRankData[MyJobId, MyJobRank].canFire())
                                    {
                                        Session.SendWhisperBubble("Your job rank cannot fire people!", 1);
                                        return true;
                                    }
                                    if (Convert.ToInt32(TarGetRow["job_id"]) != MyJobId)
                                    {
                                        Session.SendWhisperBubble("This user does not work for you!", 1);
                                        return true;
                                    }
                                    if (Convert.ToInt32(TarGetRow["job_id"]) == 1)
                                    {
                                        Session.SendWhisperBubble("This user is already unemployed.", 1);
                                        return true;
                                    }
                                    if (JobManager.JobRankData[Convert.ToInt32(TarGetRow["job_id"]), Convert.ToInt32(TarGetRow["job_rank"])].canFire() && JobManager.JobData[MyJobId].OwnerId != Convert.ToInt32(Session.GetHabbo().Id))
                                    {
                                        Session.SendWhisperBubble("You do not have sufficient rights to do this!", 1);
                                        return true;
                                    }
                                }
                                #endregion


                                Session.SendWhisperBubble("This user is offline, but they have still been fired!", 1);

                                RoleplayManager.Shout(Session, "*Demite " + username + " do seu trabalho*", 3);
                                dbClient.RunFastQuery("UPDATE rp_stats SET job_id = 1, job_rank = 1, shifts_completed = 0 WHERE id = '" + targetid + "'");
                                return true;
                            }
                            #endregion

                        }

                        return true;
                    }
                #endregion
                #region :promote x
                case "promote":
                case "promover":
                    {
                        #region Some Vars

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        #endregion

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            if (!JobManager.JobRankData[MyJobId, MyJobRank].canPromote())
                            {
                                Session.SendWhisperBubble("Your job rank cannot promote people!", 1);
                                return true;
                            }
                            if (TargetSession.GetRoleplay().JobId != MyJobId)
                            {
                                Session.SendWhisperBubble("This user does not work for you!", 1);
                                return true;
                            }
                            if (TargetSession.GetRoleplay().JobId == MyJobId && TargetSession.GetRoleplay().JobRank == (MyJobRank - 1))
                            {
                                Session.SendWhisperBubble("You cannot promote this user to founder rank!", 1);
                                return true;
                            }
                            if (JobManager.JobRankData[TargetSession.GetRoleplay().JobId, TargetSession.GetRoleplay().JobRank].canPromote() && JobManager.JobData[MyJobId].OwnerId != Convert.ToInt32(Session.GetHabbo().Id))
                            {
                                Session.SendWhisperBubble("You do not have sufficient rights to do this!", 1);
                                return true;
                            }
                        }
                        if (!JobValidation.ValidateJob(TargetSession.GetRoleplay().JobId, TargetSession.GetRoleplay().JobRank + 1))
                        {
                            Session.SendWhisperBubble("There is no other job rank above this users current rank!", 1);
                            return true;
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado nesta sala!", 1);
                            return true;
                        }
                        #endregion

                        #region Execute

                        string Job = JobManager.JobData[TargetSession.GetRoleplay().JobId].Name;
                        string Rank = JobManager.JobRankData[TargetSession.GetRoleplay().JobId, TargetSession.GetRoleplay().JobRank + 1].Name;

                        if (TargetSession.GetRoleplay().Working)
                        {
                            TargetSession.GetRoleplay().StopWork();
                        }

                        RoleplayManager.Shout(Session, "*Promove " + TargetSession.GetHabbo().UserName + " para " + Job + " " + Rank + "*", 6);
                        TargetSession.GetRoleplay().JobRank += 1;
                        TargetSession.GetRoleplay().SaveJobComponents();

                        #endregion

                        return true;
                    }
                #endregion
                #region :demote x
                case "rebaixar":
                    {
                        #region Some Vars

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;

                        #endregion

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                            {
                                Session.SendWhisperBubble("Este usuário não foi encontrado nesta sala!", 1);
                                return true;
                            }
                            if (!JobManager.JobRankData[MyJobId, MyJobRank].canDemote() && !Session.GetHabbo().HasFuse("fuse_mod"))
                            {
                                Session.SendWhisperBubble("Your job rank cannot demote people!", 1);
                                return true;
                            }
                            if (TargetSession.GetRoleplay().JobId != MyJobId)
                            {
                                Session.SendWhisperBubble("This user does not work for you!", 1);
                                return true;
                            }
                            if (JobManager.JobRankData[TargetSession.GetRoleplay().JobId, TargetSession.GetRoleplay().JobRank].canDemote() && JobManager.JobData[MyJobId].OwnerId != Convert.ToInt32(Session.GetHabbo().Id))
                            {
                                Session.SendWhisperBubble("You do not have sufficient rights to do this!", 1);
                                return true;
                            }
                        }
                        if (!JobValidation.ValidateJob(TargetSession.GetRoleplay().JobId, TargetSession.GetRoleplay().JobRank - 1))
                        {
                            Session.SendWhisperBubble("There is no other job rank below this users current rank!", 1);
                            return true;
                        }
                        #endregion

                        #region Execute

                        string Job = JobManager.JobData[TargetSession.GetRoleplay().JobId].Name;
                        string Rank = JobManager.JobRankData[TargetSession.GetRoleplay().JobId, TargetSession.GetRoleplay().JobRank - 1].Name;

                        if (TargetSession.GetRoleplay().Working)
                        {
                            TargetSession.GetRoleplay().StopWork();
                        }

                        RoleplayManager.Shout(Session, "*Rebaixa " + TargetSession.GetHabbo().UserName + " para " + Job + " " + Rank + "*");
                        TargetSession.GetRoleplay().JobRank -= 1;
                        TargetSession.GetRoleplay().SaveJobComponents();

                        #endregion

                        return true;
                    }
                #endregion
                #region :sendhome x
                case "ecasa":
                    {

                        #region Some Vars

                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;
                        int Time = 0;

                        #endregion

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (Params.Length < 2)
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :sendhome x <time in minutes>", 1);
                            return true;
                        }
                        else
                        {
                            Time = Convert.ToInt32(Params[2]);
                        }
                        if (Time < 5)
                        {
                            Session.SendWhisperBubble("The minimum sendhome time is 5 minutes!", 1);
                            return true;
                        }
                        if (Time > 30)
                        {
                            Session.SendWhisperBubble("The maximum sendhome time is 30 minutes!", 1);
                            return true;
                        }
                        if (!Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            if (!JobManager.JobRankData[MyJobId, MyJobRank].canSendHome())
                            {
                                Session.SendWhisperBubble("Your job rank cannot send home people!", 1);
                                return true;
                            }
                            if (TargetSession.GetRoleplay().JobId != MyJobId)
                            {
                                Session.SendWhisperBubble("This user does not work for you!", 1);
                                return true;
                            }
                            if (JobManager.JobRankData[TargetSession.GetRoleplay().JobId, TargetSession.GetRoleplay().JobRank].canSendHome() && JobManager.JobData[MyJobId].OwnerId != Convert.ToInt32(Session.GetHabbo().Id))
                            {
                                Session.SendWhisperBubble("You do not have sufficient rights to do this!", 1);
                                return true;
                            }
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado nesta sala!", 1);
                            return true;
                        }
                        #endregion

                        #region Execute


                        if (TargetSession.GetRoleplay().Working)
                        {
                            TargetSession.GetRoleplay().StopWork();
                            RoleplayManager.Shout(TargetSession, "*Para de trabalhar devido a ser enviado para casa!*");
                        }
                        RoleplayManager.Shout(Session, "*Envia para casa " + TargetSession.GetHabbo().UserName + " por " + Time + " minutos*");
                        TargetSession.GetRoleplay().SendHomeTimer = Time;
                        TargetSession.GetRoleplay().SaveQuickStat("sendhome_timer", "" + TargetSession.GetRoleplay().SendHomeTimer);
                        TargetSession.GetRoleplay().sendHomeTimer = new sendHomeTimer(TargetSession);
                        TargetSession.GetRoleplay().SentHome = true;

                        #endregion

                        return true;
                    }
                #endregion

                #endregion

                #endregion

                #region Staff Roleplay Commands
                #region :clearbountylist

                case "clearbl":
                case "clearblist":
                case "clearbountylist":
                case "clearhitlist":
                case "clearhl":
                case "clearbounty":
                    {
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }

                        Bounties.BountyUsers.Clear();
                        Session.SendWhisperBubble("Successfully cleared the bounty list!", 1);
                        return true;
                    }

                #endregion
                #region :disablevipa

                case "dvipa":
                case "disablevipa":
                    {
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return false;
                        }

                        if (RoleplayManager.GVIPAlertsDisabled)
                        {
                            RoleplayManager.GVIPAlertsDisabled = false;
                            Session.SendWhisperBubble("Enabled VIP alerts!", 1);
                        }
                        else
                        {
                            RoleplayManager.GVIPAlertsDisabled = true;
                            Session.SendWhisperBubble("Disabled VIP alerts!", 1);
                        }
                        return true;
                    }

                #endregion
                #region :activatedata
                case "activatedata":
                    {
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        RoomUser User = Session.GetHabbo().GetRoomUser();
                        RoomItem Itemm = null;

                        string Data = "";
                        Data = Convert.ToString(Params[1]);

                        foreach (RoomItem Item in Session.GetHabbo().CurrentRoom.GetRoomItemHandler().FloorItems.Values)
                        {
                            if (RoleplayManager.Distance(new Vector2D(Item.X, Item.Y), new Vector2D(User.X, User.Y)) <= 3)
                            {
                                Itemm = Item;
                            }
                        }


                        Session.SendWhisperBubble("Made item " + Itemm.GetBaseItem().Name + " data from " + Itemm.ExtraData + " to " + Data, 1);

                        Itemm.ExtraData = Data;
                        Itemm.UpdateState();


                        return true;
                    }
                #endregion
                #region :itemname
                case "itemname":
                    {
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        RoomUser User = Session.GetHabbo().GetRoomUser();
                        RoomItem Itemm = null;

                        foreach (RoomItem Item in Session.GetHabbo().CurrentRoom.GetRoomItemHandler().FloorItems.Values)
                        {
                            if (RoleplayManager.Distance(new Vector2D(Item.X, Item.Y), new Vector2D(User.X, User.Y)) <= 1)
                            {
                                Itemm = Item;
                            }
                        }


                        // room.GetGameMap().RemoveFromMap(item, false);
                        Itemm.UpdateState(false, true);

                        Session.SendWhisperBubble("The item nearest to you is: " + Itemm.GetBaseItem().Name + "|baseitem:" + Itemm.BaseItem + "|accid:" + Itemm.Id + "|", 1);

                        return true;
                    }
                #endregion
                #region :picknearitem
                case "picknearitem":
                    {
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        RoomUser User = Session.GetHabbo().GetRoomUser();
                        RoomItem Itemm = null;

                        foreach (RoomItem Item in Session.GetHabbo().CurrentRoom.GetRoomItemHandler().FloorItems.Values)
                        {
                            if (RoleplayManager.Distance(new Vector2D(Item.X, Item.Y), new Vector2D(User.X, User.Y)) <= 1)
                            {
                                Itemm = Item;
                            }
                        }


                        RoleplayManager.ReplaceItem(Session, Itemm, "sofa_silo");

                        return true;
                    }
                #endregion
                #region :placeitem
                case "placeitem":
                    {

                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }

                        uint ItemId = Convert.ToUInt32(Params[1]);
                        RoomUser User = Session.GetHabbo().GetRoomUser();
                        RoleplayManager.PlaceItemToCord(Session, ItemId, User.X, User.Y, 0, 0, false);

                        return true;
                    }
                #endregion
                #region :onduty

                case "onduty":
                    {

                        #region Conditions

                        if (Session.GetHabbo().Rank <= 2)
                        {
                            return true;
                        }

                        #endregion

                        #region Execute
                        if (Session.GetHabbo().Rank == 3)
                        {
                            Session.GetHabbo().GetRoomUser().ApplyEffect(178);
                            RoleplayManager.Shout(Session, "*Entra no modo STAFF (Embaixador)*", 6);
                            Session.GetRoleplay().StaffDuty = true;
                            string message = Session.GetHabbo().UserName + " came on duty (Ambassador)";
                            RoleplayManager.sendStaffAlert(message, true);
                        }
                        else
                        {
                            Session.GetHabbo().GetRoomUser().ApplyEffect(102);
                            RoleplayManager.Shout(Session, "*Goes On Duty (Staff)*", 33);
                            Session.GetRoleplay().StaffDuty = true;
                            string message = Session.GetHabbo().UserName + " came on duty (Staff)";
                            RoleplayManager.sendStaffAlert(message, true);
                        }
                        #endregion


                    }
                    return true;

                #endregion
                #region :offduty

                case "offduty":
                    {

                        #region Conditions

                        if (Session.GetHabbo().Rank <= 2)
                        {
                            return true;
                        }

                        #endregion

                        #region Execute
                        if (Session.GetHabbo().Rank == 3)
                        {
                            Session.GetHabbo().GetRoomUser().ApplyEffect(0);
                            RoleplayManager.Shout(Session, "*Goes Off Duty (Ambassador)*");
                            Session.GetRoleplay().StaffDuty = false;
                        }
                        else
                        {
                            Session.GetHabbo().GetRoomUser().ApplyEffect(0);
                            RoleplayManager.Shout(Session, "*Goes Off Duty (Staff)*");
                            Session.GetRoleplay().StaffDuty = false;
                        }

                        #endregion


                    }
                    return true;

                #endregion
                #region :kickbot x
                case "fkickbot":
                    {
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        string Name = Params[1].ToString();
                        RoleplayManager.KickBotFromRoom(Name, Session.GetHabbo().CurrentRoomId);
                        Session.SendWhisperBubble("Force kicked bot from room!", 1);
                        return true;
                    }
                #endregion
                #region :startsh
                case "startsh":
                    {

                        Session.GetRoleplay().DebugStacking = !Session.GetRoleplay().DebugStacking;

                        Session.SendWhisperBubble("Debug stacking: " + Session.GetRoleplay().DebugStacking.ToString(), 1);

                        return true;
                    }
                #endregion
                #region :setsh (debug)
                case "setsh":
                    {
                        double height = Convert.ToDouble(Params[1]);

                        /* if (!Session.GetHabbo().HasFuse("fuse_admin"))
                         {
                             return true;
                         }*/

                        if (!Session.GetRoleplay().DebugStacking)
                        {
                            Session.SendWhisperBubble("Debug stack mode: ON - Type :stopsh to disable!", 1);
                            Session.GetRoleplay().DebugStacking = true;
                        }

                        Session.GetRoleplay().DebugStack = height;

                        Session.SendWhisperBubble("You have set the stack height to: " + height, 1);

                        return true;
                    }
                #endregion
                #region :stopsh (debug)
                case "stopsh":
                    {
                        /*if (!Session.GetHabbo().HasFuse("fuse_admin"))
                        {
                            return true;
                        }*/

                        Session.GetRoleplay().DebugStacking = false;
                        Session.SendWhisperBubble("Successfully stopped stack debug mode!", 1);

                        return true;
                    }
                #endregion
                #region :initweapons (debug)
                case "initweapons":
                case "attarmas":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }

                        WeaponManager.init();
                        Session.SendWhisperBubble("Atualizou as armas com sucesso!", 1);

                        return true;
                    }
                #endregion
                #region :initjobs (debug)
                case "initjobs":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }

                        JobManager.init();
                        Session.SendWhisperBubble("Successfully refreshed roleplay jobs.", 1);
                        return true;
                    }
                #endregion
                #region :initrp (debug)
                case "initrp":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }


                        RoleplayData.Load(Path.Combine(System.Windows.Forms.Application.StartupPath, "Settings/Roleplay/settings.ini"), true, true);
                        Session.SendWhisperBubble("Successfully refreshed RP Config settings", 1);

                        return true;
                    }
                #endregion
                #region :initgangs (debug)
                case "initgangs":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }

                        GangManager.init();
                        Session.SendWhisperBubble("Successfully refreshed roleplay gangs.", 1);


                        return true;
                    }
                #endregion
                #region :initfood (debug)
                case "initfood":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }

                        Substances.load();
                        Session.SendWhisperBubble("Successfully refreshed roleplay food.", 1);
                        return true;
                    }
                #endregion
                #region :initpets (debug)
                case "initpets":
                case "initpet":
                    {



                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }

                        FightPetManager.load();
                        Session.SendWhisper("Successfully refreshed roleplay pets.");

                        return true;
                    }
                #endregion
                #region :superhire x <job> <rank>
                case "superhire":
                case "sh":
                case "scontratar":
                    {


                        #region Params
                        int JobId = 1;
                        int RankId = 1;
                        #endregion

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        // Console.WriteLine(JobId + "|" + RankId + "|" + Target);
                        #endregion

                        #region Conditions

                        if (!Session.GetHabbo().GotCommand("superhire"))
                        {
                            return false;
                        }
                        if (Params.Length < 3)
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :scontratar <jobid> <rankid>", 1);
                            return true;
                        }
                        else
                        {
                            JobId = Convert.ToInt32(Params[2]);
                            RankId = Convert.ToInt32(Params[3]);
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado!", 1);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().JobId == JobId && TargetSession.GetRoleplay().JobRank == RankId)
                        {
                            Session.SendWhisperBubble("Este usuário já tem o trabalho que você especificou", 1);
                            return true;
                        }
                        if (!JobValidation.ValidateJob(JobId, RankId))
                        {
                            Session.SendWhisperBubble("Esse é um trabalho inválido!", 1);
                            return true;
                        }
                        #endregion

                        #region Execute

                        string RankName = JobManager.JobRankData[JobId, RankId].Name;

                        if (TargetSession.GetRoleplay().Working)
                        {
                            TargetSession.GetRoleplay().StopWork();
                        }

                        TargetSession.GetRoleplay().JobId = JobId;
                        TargetSession.GetRoleplay().JobRank = RankId;
                        TargetSession.GetRoleplay().SaveJobComponents();
                        TargetSession.SendWhisper("Você foi contratado como " + JobManager.JobData[JobId].Name + " " + RankName + "!");

                        RoleplayManager.Shout(Session, "*Super contrata " + TargetSession.GetHabbo().UserName + " como " + JobManager.JobData[JobId].Name + " " + RankName + "*", 33);
                        string message = Session.GetHabbo().UserName + " super contratado " + TargetSession.GetHabbo().UserName + " como " + JobManager.JobData[JobId].Name + " " + RankName;
                        RoleplayManager.sendStaffAlert(message, true);
                        #endregion

                        return true;
                    }
                #endregion
                #region :spaceminer x
                case "spaceminer":
                    {
                        #region Generate Instances / Sessions & Params
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        int JobId = 26;
                        int RankId = TargetSession.GetRoleplay().SpaceLevel;
                        #endregion

                        #region Conditions

                        if (!Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            return true;
                        }
                        if (Params.Length < 1)
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :spaceminer <user>", 1);
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado!", 1);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().JobId == JobId && TargetSession.GetRoleplay().JobRank == RankId)
                        {
                            Session.SendWhisperBubble("Este usuário já tem o trabalho que você especificou", 1);
                            return true;
                        }
                        #endregion

                        #region Execute

                        string RankName = JobManager.JobRankData[JobId, RankId].Name;

                        if (TargetSession.GetRoleplay().Working)
                        {
                            TargetSession.GetRoleplay().StopWork();
                        }
                        TargetSession.GetRoleplay().JobId = JobId;
                        TargetSession.GetRoleplay().JobRank = RankId;
                        TargetSession.GetRoleplay().SaveJobComponents();

                        RoleplayManager.Shout(Session, "*Superhires " + TargetSession.GetHabbo().UserName + " as " + JobManager.JobData[JobId].Name + " " + RankName + "*");
                        #endregion

                        return true;
                    }
                #endregion
                #region :farmer x
                case "farmer":
                    {
                        #region Generate Instances / Sessions & Params
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        int JobId = 23;
                        int RankId = TargetSession.GetRoleplay().FarmingLevel;
                        #endregion

                        #region Conditions

                        if (!Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            return true;
                        }
                        if (Params.Length < 1)
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :farmer <user>", 1);
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado!", 1);
                            return true;
                        }
                        if (TargetSession.GetRoleplay().JobId == JobId && TargetSession.GetRoleplay().JobRank == RankId)
                        {
                            Session.SendWhisperBubble("Este usuário já tem o trabalho que você especificou", 1);
                            return true;
                        }
                        #endregion

                        #region Execute

                        string RankName = JobManager.JobRankData[JobId, RankId].Name;

                        if (TargetSession.GetRoleplay().Working)
                        {
                            TargetSession.GetRoleplay().StopWork();
                        }
                        TargetSession.GetRoleplay().JobId = JobId;
                        TargetSession.GetRoleplay().JobRank = RankId;
                        TargetSession.GetRoleplay().SaveJobComponents();

                        RoleplayManager.Shout(Session, "*Superhires " + TargetSession.GetHabbo().UserName + " as " + JobManager.JobData[JobId].Name + " " + RankName + "*");
                        #endregion

                        return true;
                    }
                #endregion
                #region :suicide
                case "suicide":
                    {
                        #region Conditions
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You are already dead!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cant find anything to kill yourself with!");
                            return true;
                        }
                        if (Session.GetRoleplay().Wanted > 0)
                        {
                            Session.SendWhisper("You cant find anything to kill yourself with!");
                            return true;
                        }
                        RoomUser rSession = Session.GetHabbo().GetRoomUser();
                        if (rSession.Frozen || !rSession.CanWalk)
                        {
                            Session.SendWhisper("You cant find anything to kill yourself with!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SHAME"))
                        {
                            Session.SendWhisper("You cant find anything to kill yourself with!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Slits their neck, committing suicide*", 32);
                        Session.GetRoleplay().DeadFigSet = false;
                        Session.GetRoleplay().DeadSeconds = 60;
                        Session.GetRoleplay().DeadTimer = 2;
                        Session.GetRoleplay().Dead = true;
                        Session.GetRoleplay().SaveStatusComponents("dead");
                        Session.GetRoleplay().Deaths++;
                        RoleplayManager.HandleDeath(Session);
                        #endregion

                        return true;
                    }
                #endregion
                #region :me <msg>
                case "me":
                    {
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisperBubble("You can't roleplay while dead!", 1);
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("mecommand"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("mecommand", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["mecommand"] > 0)
                        {
                            Session.SendWhisperBubble("Cooldown [" + Session.GetRoleplay().MultiCoolDown["mecommand"] + "/3]", 1);
                            return true;
                        }
                        string rpmsg = ChatCommandHandler.MergeParams(Params, 1);
                        RoleplayManager.Shout(Session, "*" + rpmsg + "*", 3);
                        Session.GetRoleplay().MultiCoolDown["mecommand"] = 3;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        return true;
                    }
                #endregion
                #region :bubble <id>
                case "bubble":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_owner"))
                        {
                            return true;
                        }
                        if (Params.Length <= 1)
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :bubble <id>", 1);
                            return true;
                        }
                        int BubbleId;
                        if (int.TryParse(Params[1], out BubbleId))
                        {
                            Session.SendWhisperBubble("Successfully changed your bubble to id: " + BubbleId + "", BubbleId);
                            return true;
                        }
                        else
                        {
                            Session.SendWhisperBubble("Numbers only!", 1);
                            return true;
                        }
                    }
                #endregion
                #region :kill x
                case "kill":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return false;
                        }
                        if (Params.Length <= 1)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :kill <user>");
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("This user is already dead!");
                            return true;
                        }
                        if (TargetSession.GetHabbo().Rank >= Session.GetHabbo().Rank)
                        {
                            Session.SendWhisper("You cannot kill that user!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Uses their god-like powers to summon a bolt of lightning, instantly killing " + TargetSession.GetHabbo().UserName + "*", 27);
                        TargetSession.GetRoleplay().DeadFigSet = false;
                        TargetSession.GetRoleplay().DeadSeconds = 60;
                        TargetSession.GetRoleplay().DeadTimer = 2;
                        TargetSession.GetRoleplay().Dead = true;
                        TargetSession.GetRoleplay().SaveStatusComponents("dead");
                        RoleplayManager.HandleDeath(TargetSession);
                        TargetSession.SendNotif("You were killed by " + Session.GetHabbo().UserName + "!");
                        #endregion

                        return true;
                    }
                #endregion
                #region :rigjackpot
                case "rigjackpot":
                    {
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return false;
                        }
                        if (Session.GetRoleplay().RigJackpot == false)
                        {
                            Session.GetRoleplay().RigJackpot = true;
                            Session.SendWhisper("You have activated the jackpot rigger");
                            return true;
                        }
                        else if (Session.GetRoleplay().RigJackpot == true)
                        {
                            Session.GetRoleplay().RigJackpot = false;
                            Session.SendWhisper("You have de-activated the jackpot rigger");
                            return true;
                        }
                        return true;
                    }
                #endregion
                #region :sethp <user> <amount>
                case "sethp":
                    {

                        #region Generate Instances / Sessions
                        string Target = null;
                        int Amnt = 0;
                        GameClient TargetSession = null;

                        if (RoleplayManager.ParamsMet(Params, 2))
                        {
                            Target = Convert.ToString(Params[1]);
                            Amnt = Convert.ToInt32(Params[2]);
                        }
                        else
                        {
                            Session.SendWhisper("Sintaxe de comando inválida! :sethp <user> <amount>");
                            return true;
                        }

                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_owner"))
                        {
                            return false;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }

                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Sets " + TargetSession.GetHabbo().UserName + "'s Health as " + Amnt + "*", 33);
                        TargetSession.GetRoleplay().CurHealth = Amnt;
                        TargetSession.GetRoleplay().SaveQuickStat("curhealth", Amnt + "");

                        #endregion

                        return true;
                    }
                #endregion
                #region :setarmor <user> <amount>
                case "setarmor":
                case "setap":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        int Amnt = Convert.ToInt32(Params[2]);
                        // Console.WriteLine(JobId + "|" + RankId + "|" + Target);
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }

                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Sets " + TargetSession.GetHabbo().UserName + "'s Armor as " + Amnt + "*", 33);
                        TargetSession.GetRoleplay().Armor = Amnt;
                        TargetSession.GetRoleplay().SaveQuickStat("armor", Amnt + "");

                        #endregion

                        return true;
                    }
                #endregion
                #region :sethygiene <user> <amount>
                case "sethygiene":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        int Amnt = Convert.ToInt32(Params[2]);
                        // Console.WriteLine(JobId + "|" + RankId + "|" + Target);
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Sets " + TargetSession.GetHabbo().UserName + "'s Hygiene as " + Amnt + "*", 33);
                        TargetSession.GetRoleplay().Hygiene = Amnt;
                        TargetSession.GetRoleplay().SaveQuickStat("hygiene", Amnt + "");

                        #endregion

                        return true;
                    }
                #endregion
                #region :sethunger <user> <amount>
                case "sethunger":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        int Amnt = Convert.ToInt32(Params[2]);
                        // Console.WriteLine(JobId + "|" + RankId + "|" + Target);
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Sets " + TargetSession.GetHabbo().UserName + "'s Hunger as " + Amnt + "*", 33);
                        TargetSession.GetRoleplay().Hunger = Amnt;
                        TargetSession.GetRoleplay().SaveQuickStat("hunger", Amnt + "");
                        #endregion

                        return true;
                    }
                #endregion
                #region :setenergy <user> <amount>
                case "setenergy":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        int Amnt = Convert.ToInt32(Params[2]);
                        // Console.WriteLine(JobId + "|" + RankId + "|" + Target);
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Sets " + TargetSession.GetHabbo().UserName + "'s Energy as " + Amnt + "*");
                        TargetSession.GetRoleplay().Energy = Amnt;
                        TargetSession.GetRoleplay().SaveQuickStat("energy", Amnt + "");

                        #endregion

                        return true;
                    }
                #endregion
                #region :setworkout <user> <amount>
                case "setworkout":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        int Amnt = Convert.ToInt32(Params[2]);
                        // Console.WriteLine(JobId + "|" + RankId + "|" + Target);
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        TargetSession.GetRoleplay().WorkoutTimer_Done = Amnt;
                        Session.SendWhisper("Set " + TargetSession.GetHabbo().UserName + "'s workout to " + Amnt + ". They are now at " + TargetSession.GetRoleplay().WorkoutTimer_Done + "/" + TargetSession.GetRoleplay().WorkoutTimer_ToDo);


                        #endregion

                        return true;
                    }
                #endregion
                #region :setweightlift <user> <amount>
                case "setweightlift":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        int Amnt = Convert.ToInt32(Params[2]);
                        // Console.WriteLine(JobId + "|" + RankId + "|" + Target);
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        TargetSession.GetRoleplay().WeightLiftTimer_Done = Amnt;
                        Session.SendWhisper("Set " + TargetSession.GetHabbo().UserName + "'s weightlifting to " + Amnt + ". They are now at " + TargetSession.GetRoleplay().WeightLiftTimer_Done + "/" + TargetSession.GetRoleplay().WeightLiftTimer_ToDo);

                        #endregion

                        return true;
                    }
                #endregion
                #region :sethhh <user>
                case "sethhh":
                case "refresh":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Sets " + TargetSession.GetHabbo().UserName + "'s Health as " + TargetSession.GetRoleplay().MaxHealth + ", Hunger as 0 and Hygiene as 100*", 33);

                        TargetSession.GetRoleplay().CurHealth = TargetSession.GetRoleplay().MaxHealth;
                        TargetSession.GetRoleplay().SaveQuickStat("maxhealth", TargetSession.GetRoleplay().MaxHealth + "");

                        TargetSession.GetRoleplay().Hunger = 0;
                        TargetSession.GetRoleplay().SaveQuickStat("hunger", "0");

                        TargetSession.GetRoleplay().Hygiene = 100;
                        TargetSession.GetRoleplay().SaveQuickStat("hygiene", "100");


                        #endregion

                        return true;
                    }
                #endregion
                #region :restore x
                case "restore":
                case "revive":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_admin"))
                        {
                            return true;
                        }
                        if (Params.Length <= 1)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :restore <user>");
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }
                        if (!TargetSession.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Este usuário não está morto!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Uses their god-like powers to release " + TargetSession.GetHabbo().UserName + " from the hospital*");


                        TargetSession.GetRoleplay().DeadTimer = 0;
                        TargetSession.GetRoleplay().DeadSeconds = 0;
                        TargetSession.GetRoleplay().SaveStatusComponents("dead");
                        TargetSession.GetRoleplay().CurHealth = TargetSession.GetRoleplay().MaxHealth;

                        #endregion


                        return true;
                    }
                #endregion
                #region :adminjail x <time>
                case "adminjail":
                case "jail":
                    {
                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        int Time = Convert.ToInt32(Params[2]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_admin"))
                        {
                            return true;
                        }
                        if (Params.Length <= 2)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :adminjail x <time>");
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("This user is already jailed!");
                            return true;
                        }
                        if (TargetSession.GetHabbo().Rank >= Session.GetHabbo().Rank)
                        {
                            Session.SendWhisper("You cannot admin jail that user!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Uses their god-like powers to jail " + TargetSession.GetHabbo().UserName + " for " + Time + " minutes*");
                        TargetSession.SendNotif("You have been arrested by " + Session.GetHabbo().UserName + " for " + Time + " minutes");
                        TargetSession.GetRoleplay().JailFigSet = false;
                        TargetSession.GetRoleplay().JailedSeconds = 60;
                        TargetSession.GetRoleplay().JailTimer = Time;
                        TargetSession.GetRoleplay().Jailed = true;
                        TargetSession.GetRoleplay().SaveStatusComponents("jailed");
                        #endregion

                        return true;
                    }
                #endregion
                #region :adminrelease x
                case "adminrelease":
                    {
                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_admin"))
                        {
                            return true;
                        }
                        if (Params.Length <= 1)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :adminrelease x");
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }
                        if (!TargetSession.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("This user is not jailed!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Uses their god-like powers to release " + TargetSession.GetHabbo().UserName + " from jail*");


                        TargetSession.GetRoleplay().JailTimer = 0;
                        TargetSession.GetRoleplay().JailedSeconds = 0;
                        TargetSession.GetRoleplay().SaveStatusComponents("jailed");
                        #endregion

                        return true;
                    }
                #endregion
                #region :at <roomid>
                case "ir":
                    {
                        #region Params

                        uint RoomId = Convert.ToUInt32(Params[1]);
                        string RoomName = "null";
                        Room Room = null;
                        Room = RoleplayManager.GenerateRoom(RoomId);
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_builder"))
                        {
                            return true;
                        }

                        if (Params.Length < 1)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :at <roomid>");
                            return true;
                        }
                        else
                        {
                            RoomId = Convert.ToUInt32(Params[1]);
                        }
                        #endregion

                        #region Execute

                        if (Room == null || RoomId <= 0)
                        {
                            Session.SendWhisper("The requested Room Id '" + RoomId + "' does not exist!");
                            return true;
                        }

                        RoomName = Room.RoomData.Name;

                        // Misc.Shout(Session, "*Hops on their Super Staff Motorcycle " + RoomName + " [" + RoomId + "]*");
                        Session.GetRoleplay().RequestedTaxi_WaitTime = 0;
                        Session.GetRoleplay().RequestedTaxi_Arrived = false;
                        Session.GetRoleplay().RecentlyCalledTaxi = true;
                        Session.GetRoleplay().RecentlyCalledTaxi_Timeout = 0;
                        Session.GetRoleplay().RequestedTaxiDestination = Room;
                        Session.GetRoleplay().RequestedTaxi = true;
                        Session.GetRoleplay().taxiTimer = new taxiTimer(Session);

                        #endregion

                        return true;
                    }
                #endregion
                #region :senduser <roomid>
                case "send":
                case "senduser":
                    {

                        #region Params
                        uint RoomId = 0;
                        string Username = "";
                        GameClient TargetSession = null;
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }

                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :senduser x <roomid>");
                            return true;
                        }
                        else
                        {

                            if (!Plus.IsNum(Params[2]))
                            {
                                Session.SendWhisperBubble("The roomid must be a number!");
                                return true;
                            }

                            RoomId = Convert.ToUInt32(Params[2]);
                            #region Generate Instances / Sessions
                            Username = Convert.ToString(Params[1]);
                            TargetSession = RoleplayManager.GenerateSession(Username);
                            #endregion
                        }
                        if (!RoleplayManager.CanInteract(Session, TargetSession, false))
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        Room Room = null;

                        Room = RoleplayManager.GenerateRoom(RoomId);

                        if (Room == null || RoomId <= 0) { Session.SendWhisper("The requested Room Id '" + RoomId + "' does not exist!"); return true; }

                        string Name = "";
                        if (Room != null)
                        {
                            Name = " " + Room.RoomData.Name;
                        }
                        RoleplayManager.Shout(Session, "*Sends " + TargetSession.GetHabbo().UserName + " to" + Name + "[" + RoomId + "]*", 33);
                        TargetSession.GetRoleplay().RequestedTaxi_WaitTime = 0;
                        TargetSession.GetRoleplay().RequestedTaxi_Arrived = false;
                        TargetSession.GetRoleplay().RecentlyCalledTaxi = true;
                        TargetSession.GetRoleplay().RecentlyCalledTaxi_Timeout = 10;
                        TargetSession.GetRoleplay().RequestedTaxiDestination = Room;
                        TargetSession.GetRoleplay().RequestedTaxi = true;
                        TargetSession.GetRoleplay().taxiTimer = new taxiTimer(TargetSession);

                        #endregion

                        return true;
                    }
                #endregion
                #region :summonall
                case "summonall":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_owner"))
                        {
                            return true;
                        }
                        #endregion

                        #region Execute


                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {

                                if (client == null)
                                    continue;

                                if (client.GetHabbo() == null)
                                    continue;

                                if (client.GetHabbo().CurrentRoom == null)
                                    continue;


                                if (client.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                                {
                                    client.GetMessageHandler().PrepareRoomForUser(Session.GetHabbo().CurrentRoomId, null);
                                }
                            }
                        }

                        RoleplayManager.Shout(Session, "*Summons the entire hotel*", 33);

                        #endregion

                        return true;
                    }
                #endregion
                #region :warptou
                case "warptou":
                case "warpmeto":
                    {

                        #region Params
                        string Username = "";
                        GameClient TargetSession = null;
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_mod") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("This command is for Events Staff only!");
                            return true;
                        }

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :warptou <user>");
                            return true;
                        }
                        else
                        {
                            #region Generate Instances / Sessions
                            Username = Convert.ToString(Params[1]);
                            TargetSession = RoleplayManager.GenerateSession(Username);
                            #endregion
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                        {
                            Session.SendWhisper("User not found in this room!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        RoomUser targ = null;
                        RoomUser me = null;

                        targ = TargetSession.GetHabbo().GetRoomUser();
                        me = Session.GetHabbo().GetRoomUser();

                        me.ClearMovement();
                        me.X = targ.X;
                        me.Y = targ.Y;
                        me.Z = targ.Z;

                        me.SetPos(targ.X, targ.Y, targ.Z);
                        me.UpdateNeeded = true;

                        RoleplayManager.Shout(Session, "*Warps to " + TargetSession.GetHabbo().UserName + "*");

                        #endregion
                        return true;
                    }
                #endregion
                #region :warptome
                case "warptome":
                    {

                        #region Params
                        string Username = "";
                        GameClient TargetSession = null;
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :warptome <user>");
                            return true;
                        }
                        else
                        {
                            #region Generate Instances / Sessions
                            Username = Convert.ToString(Params[1]);
                            TargetSession = RoleplayManager.GenerateSession(Username);
                            #endregion
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                        {
                            Session.SendWhisper("User not found in this room!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        RoomUser targ = null;
                        RoomUser me = null;

                        targ = TargetSession.GetHabbo().GetRoomUser();
                        me = Session.GetHabbo().GetRoomUser();

                        targ.ClearMovement();
                        targ.X = me.X;
                        targ.Y = me.Y;
                        targ.Z = me.Z;

                        targ.SetPos(me.X, me.Y, me.Z);
                        targ.UpdateNeeded = true;

                        RoleplayManager.Shout(Session, "*Warps " + TargetSession.GetHabbo().UserName + " to them*");

                        #endregion


                        return true;
                    }
                #endregion
                #region :roomheal
                case "roomheal":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_events") || (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager")))
                        {
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("EVENTS") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("You can only roomheal in an EVENTS room!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {

                                if (client == null)
                                    continue;

                                if (client.GetHabbo() == null)
                                    continue;

                                if (client.GetRoleplay() == null)
                                    continue;

                                if (client.GetHabbo().CurrentRoom == null)
                                    continue;



                                if (client.GetHabbo().CurrentRoom == Session.GetHabbo().CurrentRoom)
                                {
                                    client.GetRoleplay().CurHealth = client.GetRoleplay().MaxHealth;
                                    client.GetRoleplay().Energy = 100;
                                    client.GetRoleplay().SaveQuickStat("curhealth", client.GetRoleplay().MaxHealth + "");
                                    client.GetRoleplay().SaveQuickStat("energy", "100");
                                    client.SendWhisper("An admin has healed the room!");
                                }
                            }

                        }
                        RoleplayManager.Shout(Session, "*Uses their god-like powers to heal the room*", 1);

                        #endregion

                        return true;
                    }
                #endregion

                #region Bank Vault & Nuke Refills

                #region :refillvault

                case "refillvault":
                    {
                        #region Conditions

                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return false;
                        }
                        if (RoleplayManager.VaultRobbery > 100)
                        {
                            Session.SendWhisper("The vault doesn't need refilling..");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.VaultRobbery = 5000;
                        RoleplayManager.Shout(Session, "*Re-fills the vault with more cash*", 33);

                        #endregion

                        return true;
                    }

                #endregion

                #region :setvault

                case "setvault":
                    {
                        #region Vars

                        int Amount = Convert.ToInt32(Params[1]);

                        #endregion

                        #region Conditions

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :setvault <amount>");
                            return true;
                        }

                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return false;
                        }
                        if (Amount > 10000)
                        {
                            Session.SendWhisper("The vault stock cannot be over $10000!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.VaultRobbery = Amount;
                        RoleplayManager.Shout(Session, "*Fills the vault with $" + Amount + "*", 33);

                        #endregion

                        return true;
                    }

                #endregion

                #region :setnukes

                case "setnukes":
                    {
                        #region Conditions

                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return false;
                        }

                        if (RoleplayManager.NukesOccurred <= 0)
                        {
                            Session.SendWhisper("The nukes doesn't need to be reset!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.NukesOccurred = 0;
                        Session.SendWhisper("Done!");

                        #endregion

                        return true;
                    }

                #endregion

                #endregion

                #region Color Wars

                #region :setprize
                case "setprize":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisperBubble("Sorry but you aren't an events staff!", 1);
                            return true;
                        }

                        ColourManager.Prize = Convert.ToInt32(Params[1]);
                        RoleplayManager.Shout(Session, "Color wars prize set to " + ColourManager.Prize + " coins!", 33);
                        return true;
                    }
                #endregion

                #region :forcewin <team>
                case "forcewin":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }

                        string Team;

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            return true;
                        }
                        else
                        {
                            Team = Params[1];
                        }

                        if (!ColourManager.Started)
                        {
                            Session.SendWhisper("There is currently not a color wars game running!");
                            return true;
                        }

                        lock (ColourManager.Teams.Values)
                        {
                            foreach (Team team in ColourManager.Teams.Values)
                            {
                                if (team.Colour == Team)
                                {

                                }
                                else
                                {
                                    ColourManager.EliminateTeam(team);
                                }


                                //
                            }
                        }

                        Session.Shout("*Uses their god-like powers to force the " + Team.ToUpper() + " team to win*");

                        return true;
                    }
                #endregion

                #region :addtoteam <username> <team>
                case "addtoteam":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            Session.SendWhisper("You must be event staff to use this command!");
                            return true;
                        }


                        string Username = null;
                        string Color = null;
                        GameClient TargetSession;

                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :addtoteam <user> <teamcolor>");
                            return true;
                        }


                        Username = Params[1];
                        Color = Params[2];

                        if (!ColourManager.Teams.ContainsKey(Color.ToLower()))
                        {
                            Session.SendWhisper("The team called '" + Color.ToLower() + "' does not exist!");
                        }

                        TargetSession = RoleplayManager.GenerateSession(Username.ToLower());

                        if (TargetSession == null)
                        {
                            Session.SendWhisper("This user does not exist or is not online!");
                            return true;
                        }

                        if (TargetSession.GetHabbo() == null)
                        {
                            Session.SendWhisper("This user does not exist or is not online!");
                            return true;
                        }

                        if (TargetSession.GetHabbo().CurrentRoom == null)
                        {
                            Session.SendWhisper("This user does not exist or is not online!");
                            return true;
                        }

                        if (TargetSession.GetRoleplay().inColourWars)
                        {
                            ColourManager.RemovePlayerFromTeam(TargetSession, TargetSession.GetRoleplay().ColourWarTeam, true, "You were removed by an admin");
                        }




                        Session.Shout("*Uses their god-like powers to add " + TargetSession.GetHabbo().UserName + " to the Color Wars " + Color.ToUpper() + " Team*");

                        ColourManager.ForceAddPlayerToTeam(TargetSession, Color.ToLower());

                        return true;
                    }

                #endregion

                #region :forcestop
                case "forcestop":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisperBubble("Sorry but you aren't an events staff!", 1);
                            return true;
                        }

                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {

                                if (client == null)
                                    continue;

                                if (client.GetHabbo() == null)
                                    continue;

                                if (client.GetHabbo().CurrentRoom == null)
                                    continue;

                                if (!client.GetHabbo().CurrentRoom.RoomData.Description.Contains("COLOR"))
                                {
                                    continue;
                                }

                                if (client.GetRoleplay() != null)
                                {
                                    if (client.GetRoleplay().inColourWars)
                                    {
                                        ColourManager.RemovePlayerFromTeam(client, client.GetRoleplay().ColourWarTeam, false, "An admin has stopped the game!");
                                    }
                                }

                                Room Room = RoleplayManager.GenerateRoom(ColourManager.MainLobby);
                                client.GetRoleplay().RequestedTaxi_Arrived = false;
                                client.GetRoleplay().RecentlyCalledTaxi = true;
                                client.GetRoleplay().RecentlyCalledTaxi_Timeout = 10;
                                client.GetRoleplay().RequestedTaxiDestination = Room;
                                client.GetRoleplay().RequestedTaxi = true;
                                client.GetRoleplay().taxiTimer = new taxiTimer(client);

                            }
                        }

                        ColourManager.EndGame();
                        Session.Shout("*Uses their god-like powers to end the current color wars game*");

                        return true;
                    }
                #endregion

                #region :forcestart
                case "forcestart":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisperBubble("Sorry but you aren't an events staff!", 1);
                            return true;
                        }

                        ColourManager.TryStart(true);
                        Session.Shout("*Uses their god-like powers to force start a color wars game*");

                        return true;
                    }

                #endregion

                #endregion

                #region :coinself
                case "coinself":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_builder"))
                        {
                            return true;
                        }
                        if (!RoleplayManager.BypassRights(Session) && Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            Session.SendWhisper("Você não tem permissão para fazer isso!");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :coinself <amnt>");
                            return true;
                        }
                        int creditsToAdd;
                        if (int.TryParse(Params[1], out creditsToAdd))
                        {
                            Session.GetHabbo().Credits = Session.GetHabbo().Credits + creditsToAdd;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.SendWhisper("Com sucesso se deu $" + creditsToAdd + "");
                            return true;
                        }
                        else
                        {
                            Session.SendNotif("Apenas números!");
                            return true;
                        }
                    }
                #endregion
                #region :airstrike
                case "airstrike":
                    {
                        #region Conditions
                        if (Session.GetHabbo().Id != 1)
                        {
                            return false;
                        }
                        #endregion

                        #region Execute
                        lock (Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                        {
                            foreach (RoomUser client in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            {

                                if (client == null)
                                    continue;
                                if (client.GetClient() == null)
                                    continue;
                                if (client.GetClient().GetRoleplay() == null)
                                    continue;
                                if (client.GetClient().GetHabbo() == null)
                                    continue;
                                if (client.GetClient().GetHabbo().CurrentRoom == null)
                                    continue;
                                if (client.GetClient().GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                                    continue;

                                if (!RoleplayManager.BypassRights(client.GetClient()))
                                {
                                    client.GetClient().GetRoleplay().DeadFigSet = false;
                                    client.GetClient().GetRoleplay().DeadSeconds = 60;
                                    client.GetClient().GetRoleplay().DeadTimer = 2;
                                    client.GetClient().GetRoleplay().Dead = true;
                                    client.GetClient().GetRoleplay().SaveStatusComponents("dead");
                                    RoleplayManager.HandleDeath(client.GetClient());
                                    client.GetClient().SendNotif("You were killed by an airstrike!");
                                }
                            }
                        }
                        RoleplayManager.Shout(Session, "*Calls in an airstrike*", 1);

                        #endregion

                        return true;
                    }
                #endregion
                #region :roomrelease
                case "roomrelease":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("HOSP") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("You can only roomrelease in the Hospital!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (client == null)
                                    continue;

                                if (client.GetRoleplay() == null)
                                    continue;

                                if (client.GetHabbo() == null)
                                    continue;

                                if (client.GetHabbo().CurrentRoom == null)
                                    continue;

                                if (client.GetHabbo().CurrentRoom == Session.GetHabbo().CurrentRoom)
                                {

                                    if (client.GetRoleplay().Jailed)
                                    {
                                        client.GetRoleplay().JailTimer = 0;
                                        client.GetRoleplay().JailedSeconds = 0;
                                        client.GetRoleplay().SaveStatusComponents("jailed");
                                    }

                                    if (client.GetRoleplay().Dead)
                                    {
                                        client.GetRoleplay().DeadTimer = 0;
                                        client.GetRoleplay().DeadSeconds = 0;
                                        client.GetRoleplay().SaveStatusComponents("dead");
                                        client.GetRoleplay().CurHealth = client.GetRoleplay().MaxHealth;
                                    }

                                }
                            }
                        }
                        RoleplayManager.Shout(Session, "*Uses their god-like powers to release the room*", 33);

                        #endregion

                        return true;

                    }
                #endregion
                #region :sendroom
                case "sendroom":
                    {
                        #region Conditions
                        uint RoomId = 1;
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :sendroom <roomid>");
                            return true;
                        }
                        else
                        {
                            RoomId = Convert.ToUInt32(Params[1]);
                        }
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        #endregion

                        #region Execute
                        try
                        {
                            foreach (RoomUser User in Session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values)
                            {
                                if (User == null)
                                {
                                    continue;
                                }

                                if (User.GetClient() == null)
                                    continue;


                                if (User.GetClient().GetMessageHandler() == null)
                                    continue;

                                if (User.GetClient().GetHabbo() == null)
                                    continue;

                                if (User.GetClient() == Session)
                                    continue;

                                User.GetClient().GetMessageHandler().PrepareRoomForUser(RoomId, "");
                            }
                            Room Room = null;
                            Room = RoleplayManager.GenerateRoom(RoomId);
                            string Name = "";
                            if (Room != null)
                            {
                                Name = " " + Room.RoomData.Name;
                            }

                            RoleplayManager.Shout(Session, "*Sends everybody in the room to" + Name + " [" + RoomId + "]*", 33);
                        }
                        catch (Exception)
                        {

                        }
                        #endregion

                        return true;
                    }
                #endregion
                #region :summonstaff
                case "summonstaff":
                case "staffall":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            return true;
                        }
                        #endregion

                        #region Execute
                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (client == null)
                                    continue;
                                if (client.GetHabbo() == null)
                                    continue;
                                if (client.GetHabbo().CurrentRoom == Session.GetHabbo().CurrentRoom)
                                    continue;
                                if (!client.GetHabbo().HasFuse("fuse_events"))
                                    continue;

                                client.GetMessageHandler().PrepareRoomForUser(Session.GetHabbo().CurrentRoomId, null);
                            }
                        }
                        RoleplayManager.Shout(Session, "*Summons all the staff members*", 33);
                        #endregion

                        return true;
                    }
                #endregion
                #region :summonf
                case "summonf":
                case "femaleall":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            return true;
                        }
                        #endregion

                        #region Execute
                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (client == null)
                                    continue;
                                if (client.GetHabbo() == null)
                                    continue;
                                if (client.GetHabbo().CurrentRoom == Session.GetHabbo().CurrentRoom)
                                    continue;
                                if (!client.GetHabbo().Gender.ToLower().Contains("f"))
                                    continue;

                                client.GetMessageHandler().PrepareRoomForUser(Session.GetHabbo().CurrentRoomId, null);
                            }
                        }
                        RoleplayManager.Shout(Session, "*Summons all the females*", 33);
                        #endregion

                        return true;
                    }
                #endregion
                #region :vipa
                case "vipa":
                case "vipalert":
                case "vipha":
                case "v":
                    {

                        #region Conditions
                        string Notice = "";
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :vipalert <msg>");
                            return true;
                        }
                        else
                        {
                            Notice = MergeParams(Params, 1);
                        }
                        if (!Session.GetHabbo().HasFuse("fuse_vip"))
                        {
                            Session.SendWhisper("You must be VIP to do this!");
                            return true;
                        }
                        if (Notice == null)
                        {
                            Session.SendWhisper("You did not type in a valid message!");
                            return true;
                        }
                        if (Session.GetHabbo().vipAlertsOff)
                        {
                            Session.SendWhisper("You can not send VIP alerts while your VIP alerts are disabled!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SHAME") && !RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("You cannot do this in the SHAME room.");
                            return true;
                        }
                        if (RoleplayManager.GVIPAlertsDisabled && !RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("An administrator blocked you from using this command!");
                            return true;
                        }
                        if (Session.GetRoleplay().BannedFromVIPAlert)
                        {
                            Session.SendWhisper("You are currently banned from sending VIP alerts!");
                            return true;
                        }

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("vipa_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("vipa_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["vipa_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["vipa_cooldown"] + "/2]");
                            return true;
                        }
                        #endregion

                        #region Execute

                        RoleplayManager.sendVIPAlert(Session, Notice, false);
                        Session.GetRoleplay().MultiCoolDown["vipa_cooldown"] = 2;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion
                #region :togglevipalerts
                case "togglevipalerts":
                case "togglevipa":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_vip"))
                        {
                            return true;
                        }
                        #endregion

                        #region Execute
                        bool vipAlertsOff = Session.GetHabbo().vipAlertsOff;

                        if (vipAlertsOff)
                        {
                            Session.GetHabbo().vipAlertsOff = false;
                            Session.Shout("*Enables VIP alerts*");
                        }
                        else
                        {
                            Session.GetHabbo().vipAlertsOff = true;
                            Session.Shout("*Disables VIP alerts*");
                        }
                        #endregion

                        return true;
                    }
                #endregion
                #region :givevip
                case "givevip":
                case "vip":
                    {
                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("You're not allowed to do this..");
                            return true;
                        }
                        if (TargetSession.GetHabbo().Rank == 2)
                        {
                            Session.SendWhisper("This user is already VIP?");
                            return true;
                        }
                        if (Params.Length <= 1)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :givevip x");
                            return true;
                        }
                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Gives " + TargetSession.GetHabbo().UserName + " VIP*", 33);
                        TargetSession.GetHabbo().Rank = 2;
                        RoleplayManager.GiveMoney(TargetSession, +2000);
                        RoleplayManager.GiveCredit(TargetSession, +100);
                        TargetSession.GetHabbo().BelCredits += 5;
                        TargetSession.GetHabbo().UpdateActivityPointsBalance();

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunFastQuery("UPDATE `users` SET `rank` = '2' WHERE `id` = '" + TargetSession.GetHabbo().Id + "'");
                            dbClient.RunFastQuery("UPDATE `users` SET `vip` = '1' WHERE `id` = '" + TargetSession.GetHabbo().Id + "'");
                        }

                        RoleplayManager.Shout(TargetSession, "*Logs Out (Received VIP)*");
                        TargetSession.GetConnection().Dispose();


                        #endregion

                        return true;
                    }


                #endregion
                #region :staffcommands (Staff Commands)
                case "staffcommands":
                case "staffcmds":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        StringBuilder StaffCmds = new StringBuilder();

                        StaffCmds.Append("Below, you can find a full list of commands for your rank. Keep in mind, this list is subject to change at any time.\n");
                        StaffCmds.Append("IF A STAFF MEMBER IS TO ABUSE ANY OF THESE COMMANDS, THEY WILL BE FIRED WITHOUT WARNING.\n\n");

                        StaffCmds.Append("-------------------------------------------------\nComandos de Eventos\n-------------------------------------------------\n");
                        StaffCmds.Append(":sa <msg> - Sends a message to all online staff members\n");
                        StaffCmds.Append(":teleport - Teleports you to any coordinates in the room\n");
                        StaffCmds.Append(":override - Walks over any furniture in the room\n");
                        StaffCmds.Append(":senduser <username> <roomid> - Sends the specified user to the specified roomid\n");
                        StaffCmds.Append(":startpurge - Starts the purge event\n");
                        StaffCmds.Append(":stoppurge - Stops the purge event\n");
                        StaffCmds.Append(":startinfection - Starts the infection event\n");
                        StaffCmds.Append(":stopinfection - Stops the infection event\n");
                        StaffCmds.Append(":eventha <info/prize> - Starts an event. The events room id's are: [32, 46, 384, 481]\n");
                        StaffCmds.Append(":setprize <amount> - Stes the color/mafia wars prize. Seek a higher rank for mafia wars\n");
                        StaffCmds.Append(":ban <username> - Bans a user\n");
                        StaffCmds.Append(":forcestart - Forcestarts a game of color/mafia wars. Seek a higher rank for mafia wars\n");
                        StaffCmds.Append(":onduty - Disables the user's ability to be shot/hit/bomb'd and vice-versa.\n");
                        StaffCmds.Append(":offduty - Enables the user's ability to be shot/hit/bomb'd and vice-versa.\n");

                        StaffCmds.Append("-------------------------------------------------\nBuilders Commands\n-------------------------------------------------\n");
                        StaffCmds.Append(":coinself <amount> - Gives themselves coins for building\n");
                        StaffCmds.Append(":follow <username> - Follows another user anywhere they may be in the RP\n");

                        StaffCmds.Append("-------------------------------------------------\nModeration Commands\n-------------------------------------------------\n");
                        StaffCmds.Append(":spaceminer <username> - Hires another user to the spaceminer corporation\n");
                        StaffCmds.Append(":farmer <username> - Hires another user to the farming corporation\n");
                        StaffCmds.Append(":ban <username> <minutes> <reason> - Terminates a user's account from our services temporarily\n");
                        StaffCmds.Append(":superban <username> <reason> - Terminates a user's account from our services temporarily\n");
                        StaffCmds.Append(":ipban <username> <reason> - Terminates a user's account AND ip address from our services permanently\n");
                        StaffCmds.Append(":summon <username> - Brings a user to their room\n");
                        StaffCmds.Append(":warptome <username> - Teleports a user to them\n");
                        StaffCmds.Append(":warptou <username> - Teleports to a user\n");
                        StaffCmds.Append(":roomheal - Heals everyone in the room. This command only works in (roomid) 2\n");
                        StaffCmds.Append(":roomrelease - Discharges/Releases everyone in the room. This command only works in (roomid) 2\n");
                        StaffCmds.Append(":todo - Opens the todo list for staff members\n");
                        StaffCmds.Append(":todoadd <idea> - Adds your 'idea' to the todo list\n");
                        StaffCmds.Append(":tododel <id> - Removes the chosen id from the todo list\n");

                        StaffCmds.Append("-------------------------------------------------\nAdministration Commands\n-------------------------------------------------\n");
                        StaffCmds.Append(":superhire <username> <corpid> <rankid> - Hires another user to any corporation. View :corplist and :corpinfo <corpid> for more information\n");
                        StaffCmds.Append(":flood <username> <seconds> - Floods another user for the specified ammount of time\n");
                        StaffCmds.Append(":adminjail <username> <minutes> - Arrests another user from anywhere in the RP\n");
                        StaffCmds.Append(":adminrelease <username> - Releases another user from anywhere in the RP\n");
                        StaffCmds.Append(":restore <username> - Discharges another user from the hospital anywhere in the RP.\n");

                        StaffCmds.Append("-------------------------------------------------\nManagement Commands\n-------------------------------------------------\n");
                        StaffCmds.Append(":roomheal - Heals the room. This command can be used anywhere\n");
                        StaffCmds.Append(":roomrelease - Discharges the room. This command can be used anywhere\n");
                        StaffCmds.Append(":ha <message> - Sends a hotel alert\n");
                        StaffCmds.Append(":wha <message> - Sends a whisper hotel alert\n");
                        StaffCmds.Append(":flag <username> - Flags a user for having an invalid name\n");

                        StaffCmds.Append("-------------------------------------------------\nStaff Rules\n-------------------------------------------------\n");
                        StaffCmds.Append("If a staff member is to break any of these rules, they will be immediately terminated.\n\n");
                        StaffCmds.Append("Always abide by the FluxRP way. You are not above the rules.\n");
                        StaffCmds.Append("Never use your rank to put yourself above the users.\n");
                        StaffCmds.Append("Do not abuse any of your staff commands.\n");
                        StaffCmds.Append("Do not ban a user for a reason that is not in the bans guide.\n");
                        StaffCmds.Append("Do not show favouritism towards your friends.\n");
                        StaffCmds.Append("Always help any user no matter what's happened in the past.\n");
                        StaffCmds.Append("Do not attempt to do the job of a higher rank. This will result in immediate termination.\n");
                        StaffCmds.Append("Do not argue with any other user or staff member.\n");
                        StaffCmds.Append("Stay professional at all times.\n");
                        StaffCmds.Append("Never host an event for a (prize) above 3,000c.\n");
                        StaffCmds.Append("Never hire a user to a corporation rank above 1, unless you own that corporation.\n");
                        StaffCmds.Append("Never ask for a promotion. This will result in immediate termination.\n");

                        StaffCmds.Append("-------------------------------------------------\nBan Guide\n-------------------------------------------------\n");
                        StaffCmds.Append("ALWAYS use this guide when banning.\n\n");
                        StaffCmds.Append("- 10 Minutes:\nAll 10 minute bans are subject to a warning!\n\nSpamming\nHarassment\nBullying\nGenerally anything you would mute a user for doing.\n\n");
                        StaffCmds.Append("- 30 Minutes:\n\nJoking about cancer or any other terminal illness\nRacist Jokes\nUsing an auto typer to boost stats (e.g Gym, Library)\nProviding false information to a staff member\n\n");
                        StaffCmds.Append("- 60 minutes:\n\nContiued Spamming\nContinued Harassment\nContinued Bullying\n\n");
                        StaffCmds.Append("- IP Ban:\n\nAdvertising\nAvoiding a superban\n\n");
                        StaffCmds.Append("- Super Ban:\n\nUsing two accounts online at once\n\n");

                        StaffCmds.Append("- All of the ban reasons are not here! Refer to the FluxRP way visible on the forums for a complete list.\n");
                        StaffCmds.Append("- If you have any concerns regarding anything above, contact a higher rank!\n");

                        Session.SendNotifWithScroll(StaffCmds.ToString());

                        return true;
                    }
                #endregion
                #region :update_rocks
                case "update_rocks":
                case "initrocks":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        Session.SendWhisperBubble("The rock spots have been updated", 1);
                        spaceManager.initSpace();
                        return true;
                    }
                #endregion
                #region :update_trees
                case "update_trees":
                case "inittrees":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        Session.SendWhisperBubble("The tree spots have been updated", 1);
                        woodManager.initTrees();
                        return true;
                    }
                #endregion
                #region :update_farming
                case "update_farming":
                case "initfarming":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        Session.SendWhisperBubble("The farming spots have been updated", 1);
                        farmingManager.init();
                        return true;
                    }
                #endregion
                #region :update_slots
                case "update_slots":
                case "initslots":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        Session.SendWhisperBubble("The slots have been updated", 1);
                        SlotsManager.init();
                        return true;
                    }
                #endregion
                #region :update_models
                case "update_models":
                case "refresh_models":
                case "initmodels":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }

                        Room room = Session.GetHabbo().CurrentRoom;

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            Plus.GetGame().GetRoomManager().LoadModels(dbClient);
                            room.ResetGameMap(room.RoomData.ModelName, room.RoomData.WallHeight, room.RoomData.WallThickness, room.RoomData.FloorThickness);
                        }
                        Session.SendWhisperBubble("Room Models have been updated", 1);

                        return true;
                    }

                #endregion
                #region :giveweapon x <weapon>

                case "giveweapon":
                    {
                        #region Generate Instances / Sessions
                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :giveweapon x <weapon>");
                            return true;
                        }
                        string Target = Convert.ToString(Params[1]);
                        string Offer = Convert.ToString(Params[2]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_owner"))
                        {
                            return false;
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        Dictionary<string, Weapon> Weaponss = new Dictionary<string, Weapon>();
                        Dictionary<string, Weapon> TWeaponss = new Dictionary<string, Weapon>();
                        Weaponss.Clear();
                        TWeaponss.Clear();
                        //foreach (KeyValuePair<string, Weapon> Weaponlol in Session.GetRoleplay().Weapons)
                        //{
                        //    Weaponss.Add(weaponManager.GetWeaponName(Weaponlol.Key), Weaponlol.Value);
                        //
                        //}
                        foreach (KeyValuePair<string, Weapon> Weaponlol in TargetSession.GetRoleplay().Weapons)
                        {
                            TWeaponss.Add(WeaponManager.GetWeaponName(Weaponlol.Key), Weaponlol.Value);
                        }

                        if (TWeaponss.ContainsKey(Offer))
                        {
                            RoleplayManager.Shout(Session, "*Pulls out an " + Offer.ToLower() + " from my pocket, attempting to give " + TargetSession.GetHabbo().UserName + ", but they already have an " + WeaponManager.WeaponsData[Offer.ToLower()] + "*", 33);
                            Session.SendWhisper("This user already has a " + Offer);
                            return true;
                        }
                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Pulls out an " + Offer.ToLower() + " from my pocket, hands the weapon to " + TargetSession.GetHabbo().UserName + "*", 33);
                        TargetSession.GetRoleplay().addWeapon(WeaponManager.WeaponsData[Offer.ToLower()].Name);

                        #endregion

                        return true;
                    }

                #endregion
                #region :givestats x <stat>

                case "givestats":
                    {

                        #region Generate Instances / Sessions
                        if (!RoleplayManager.ParamsMet(Params, 3))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :givestats x <data> <amount>");
                            return true;
                        }
                        string Target = Convert.ToString(Params[1]);
                        string Stats = Convert.ToString(Params[2]);
                        int Amount = Convert.ToInt32(Params[3]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions

                        if (!RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("You're not allowed to do this..");
                            return true;
                        }
                        if (Amount <= 0)
                        {
                            return true;
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Takes out my Staff Supply.. Gives [" + Amount + "]" + Stats + " to " + TargetSession.GetHabbo().UserName + "*");
                        TargetSession.GetRoleplay().SaveQuickStat(Stats, Convert.ToString(Amount));

                        #endregion


                        return true;
                    }
                #endregion
                #region :todo
                case "todo":
                case "td":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        string Todo = "====================== Todo List =======================\nID - Username - Idea\n\n";
                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT * FROM todo");
                            DataTable Table = dbClient.GetTable();
                            foreach (DataRow Row in Table.Rows)
                            {
                                Todo += "[#" + Row["id"] + "] " + Row["user"] + " - " + Row["details"] + "\n";
                                /*string Completed = (Row["completed"] == "0") ? "� (No)" : "� (Yes)";
                                Todo += "ID: " + Row["id"] + "\n";
                                Todo += "User: " + Row["user"] + "\n";
                                Todo += "Idea: " + Row["details"] + "\n";
                                Todo += "Completed: " + Completed + "\n\n";*/
                            }
                        }
                        Session.SendNotifWithScroll(Todo);
                        return true;
                    }
                #endregion
                #region :todoadd
                case "todoadd":
                case "tda":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        string Idea = ChatCommandHandler.MergeParams(Params, 1);
                        string User = Session.GetHabbo().UserName;
                        using (var adapter = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            adapter.SetQuery("INSERT INTO todo VALUES (null, @user, @details)");
                            adapter.AddParameter("user", User);
                            adapter.AddParameter("details", Idea);
                            adapter.RunQuery();
                            Session.SendWhisper("Successfully added to the todo list.");
                        }
                        return true;
                    }
                #endregion
                #region :tododel
                case "tododel":
                case "tdd":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        string Id = Params[1];
                        using (var adapter = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            adapter.SetQuery("DELETE FROM todo WHERE id = @id");
                            adapter.AddParameter("id", Id);
                            adapter.RunQuery();
                            Session.SendWhisper("Successfully deleted from the todo list.");
                        }
                        return true;
                    }
                #endregion
                #region :freezeroom
                case "freezeroom":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            return true;
                        }
                        #endregion

                        #region Execute
                        lock (Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                        {
                            foreach (RoomUser client in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            {
                                if (client == null)
                                    continue;
                                if (client.GetClient() == null)
                                    continue;
                                if (client.GetClient().GetHabbo() == null)
                                    continue;
                                if (client.GetClient().GetHabbo().CurrentRoom == null)
                                    continue;

                                if (client.GetClient().GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                                    continue;

                                client.ClearMovement();
                                client.CanWalk = false;
                                client.Frozen = true;
                                client.ApplyEffect(12);
                            }
                        }



                        RoleplayManager.Shout(Session, "*Uses their god-like powers to freeze the room*", 1);

                        #endregion

                        return true;
                    }
                #endregion
                #region :unfreezeroom
                case "unfreezeroom":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            return true;
                        }
                        #endregion

                        #region Execute
                        lock (Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                        {
                            foreach (RoomUser client in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            {
                                if (client == null)
                                    continue;
                                if (client.GetClient() == null)
                                    continue;
                                if (client.GetClient().GetHabbo() == null)
                                    continue;
                                if (client.GetClient().GetHabbo().CurrentRoom == null)
                                    continue;
                                if (client.GetClient().GetHabbo().GetRoomUser() == null)
                                    continue;
                                if (client.GetClient().GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                                    continue;

                                client.CanWalk = true;
                                client.Frozen = false;
                                client.ApplyEffect(0);
                            }
                        }

                        RoleplayManager.Shout(Session, "*Uses their god-like powers to un-freeze the room*", 1);

                        #endregion

                        return true;
                    }
                #endregion
                #region :warpalltome
                case "warpalltome":
                    {

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            return true;
                        }
                        #endregion

                        #region Execute

                        lock (Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                        {
                            foreach (RoomUser client in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            {
                                if (client == null)
                                    continue;
                                if (client.GetClient() == null)
                                    continue;
                                if (client.GetClient().GetHabbo() == null)
                                    continue;
                                if (client.GetClient().GetHabbo().CurrentRoom == null)
                                    continue;
                                if (client.GetClient().GetHabbo().GetRoomUser() == null)
                                    continue;
                                if (client.GetClient().GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                                    continue;

                                client.ClearMovement();
                                client.X = Session.GetHabbo().GetRoomUser().X;
                                client.Y = Session.GetHabbo().GetRoomUser().Y;
                                client.Z = Session.GetHabbo().GetRoomUser().Z;

                                client.SetPos(Session.GetHabbo().GetRoomUser().X,
                                              Session.GetHabbo().GetRoomUser().Y,
                                              Session.GetHabbo().GetRoomUser().Z);
                                client.UpdateNeeded = true;
                            }
                        }

                        RoleplayManager.Shout(Session, "*Warps everybody to them*");

                        #endregion


                        return true;
                    }
                #endregion

                #region :mimic

                case "mimic":
                case "copy":
                case "copylook":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }

                        var room = Session.GetHabbo().CurrentRoom;

                        var user = room.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
                        if (user == null)
                        {
                            Session.SendWhisper(Plus.GetLanguage().GetVar("user_not_found"));
                            return true;
                        }

                        var gender = user.GetClient().GetHabbo().Gender;
                        var look = user.GetClient().GetHabbo().Look;
                        Session.GetHabbo().Gender = gender;
                        Session.GetHabbo().Look = look;
                        using (var adapter = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            adapter.SetQuery(
                                "UPDATE users SET gender = @gender, look = @look WHERE id = " + Session.GetHabbo().Id);
                            adapter.AddParameter("gender", gender);
                            adapter.AddParameter("look", look);
                            adapter.RunQuery();
                        }

                        var myUser = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().UserName);
                        if (myUser == null) return true;

                        var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                        message.AppendInteger(myUser.VirtualId);
                        message.AppendString(Session.GetHabbo().Look);
                        message.AppendString(Session.GetHabbo().Gender.ToLower());
                        message.AppendString(Session.GetHabbo().Motto);
                        message.AppendInteger(Session.GetHabbo().AchievementPoints);
                        room.SendMessage(message.GetReversedBytes());

                        return true;
                    }

                #endregion
                #endregion

                #region Roleplay Categorized

                #region Crime

                #region :rob x
                case "roubar":
                case "rouba":
                    {
                        #region Generate Instances / Sessions
                        string Target = null;
                        GameClient TargetSession = null;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :roubar <user>");
                            return true;
                        }
                        else
                        {
                            Target = Params[1];
                        }
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        if (!RoleplayManager.CanInteract(Session, TargetSession, true))
                        {
                            Session.SendWhisperBubble("Este usuário não foi encontrado na sala!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("rob_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("rob_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["rob_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["rob_cooldown"] + "/300]");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :roubar <username>");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOROB") && RoleplayManager.PurgeTime == false)
                        {
                            Session.SendWhisper("Você não pode roubar pessoas nesta sala!");
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SAFEZONE"))
                        {
                            Session.SendWhisper("Desculpe, mas esta é uma zona segura!");
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala");
                            return true;
                        }
                        if (TargetSession.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala");
                            return true;
                        }
                        if (TargetSession.GetHabbo().HasFuse("fuse_builder") && !TargetSession.GetHabbo().HasFuse("fuse_mod"))
                        {
                            Session.SendWhisper("Você não pode roubar um construtor!");
                            return true;
                        }
                        if (TargetSession.GetHabbo().HasFuse("fuse_senior") && TargetSession.GetHabbo().Rank != 12)
                        {
                            Session.SendWhisper("Você não pode roubar um membro da equipe staff!");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().IsNoob)
                        {
                            Session.SendWhisper("Este usuário está sob proteção de deus, então você não pode roubá-lo!");
                            return true;
                        }
                        if (Session.GetRoleplay().IsNoob)
                        {
                            if (!Session.GetRoleplay().NoobWarned)
                            {
                                Session.SendNotif("Se você optar por fazer isso novamente, sua Proteção Divina temporária será desativada!");
                                Session.GetRoleplay().NoobWarned = true;
                                return false;
                            }
                            else
                            {
                                Session.GetRoleplay().RemoveGodProtection();
                            }
                        }
                        if (TargetSession.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Você não pode roubar uma pessoa morta");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Você não pode roubar uma pessoa presa");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("Não pode completar esta ação pois você está morto");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("Não pode completar esta ação pois você está preso");
                            return true;
                        }
                        if (RoleplayManager.UserDistance(Session, TargetSession) >= 2)
                        {
                            Session.SendWhisper("Você deve se aproximar para fazer isso!");
                            return true;
                        }


                        #endregion

                        #region Execute

                        Random Rand = new Random();
                        int Genc = 0;
                        int rndNum = new Random(DateTime.Now.Millisecond).Next(1, 20);
                        int Robamt = 0;

                        if (rndNum == 1)
                        {
                            Genc = Rand.Next(1, 500);
                            Robamt += (Session.GetRoleplay().Strength + Session.GetRoleplay().Stamina + 1) * 3 + Genc;
                        }

                        else if (rndNum < 5)
                        {
                            Genc = Rand.Next(1, 200);
                            Robamt += (Session.GetRoleplay().Strength + Session.GetRoleplay().Stamina + 1) * 2 + Genc;
                        }

                        else if (rndNum < 10)
                        {
                            Genc = Rand.Next(1, 90);
                            Robamt += (Session.GetRoleplay().Strength + Session.GetRoleplay().Stamina + 1) + 3 + Genc;
                        }

                        else
                        {
                            Genc = Rand.Next(1, 30);
                            Robamt += (Session.GetRoleplay().Strength + Session.GetRoleplay().Stamina + 1) + 1 + Genc;
                        }

                        int Damage = Rand.Next(Session.GetRoleplay().Stamina, Session.GetRoleplay().Strength + 3);

                        if (TargetSession.GetHabbo().Credits < Genc || TargetSession.GetHabbo().Credits - Genc < 0)
                        {
                            Session.SendWhisper("Este usuário não tem dinheiro!");
                            return true;
                        }

                        RoleplayManager.GiveMoney(Session, +Genc);
                        RoleplayManager.GiveMoney(TargetSession, -Genc);
                        TargetSession.GetRoleplay().CurHealth -= Damage;
                        RoleplayManager.Shout(Session, "*Rouba " + TargetSession.GetHabbo().UserName + " causando " + Damage + " dano e roubando $" + Robamt + " dele*", 3);
                        Session.GetRoleplay().MultiCoolDown["rob_cooldown"] = 300;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion

                #region :robbank
                case "startrobbery":
                case "robbank":
                case "robvault":
                case "roubarbanco":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("VAULT"))
                        {
                            Session.SendWhisper("You must be in the VAULT to do this!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this while jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this while dead!");
                            return true;
                        }
                        if (Session.GetRoleplay().Robbery)
                        {
                            Session.SendWhisper("You are already robbing the bank!");
                            return true;
                        }
                        if (Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("You cannot do this while working!");
                            return true;
                        }
                        if (Session.GetRoleplay().Intelligence < 15)
                        {
                            Session.SendWhisper("You must have 15 intelligence to rob the bank!");
                            return true;
                        }
                        if (RoleplayManager.VaultRobbery <= 0)
                        {
                            Session.SendWhisper("You cannot rob the vault as the vault was completely robbed!");
                            return true;
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
                                Session.GetRoleplay().RemoveGodProtection();
                            }
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("robbery"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("robbery", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["robbery"] > 0)
                        {
                            Session.SendWhisper("You must wait until you can rob the bank again! [" + Session.GetRoleplay().MultiCoolDown["robbery"] + "/900]");
                            return true;
                        }
                        #endregion

                        #region Execute
                        Session.GetRoleplay().bankRobTimer = new bankRobTimer(Session);
                        Session.GetRoleplay().Robbery = true;
                        Session.GetRoleplay().SaveStatusComponents("robbery");
                        RoleplayManager.Shout(Session, "*Starts robbing the bank*");
                        Session.SendWhisper("You have " + Session.GetRoleplay().bankRobTimer.getTime() + " minutes until you complete your robbery.");
                        Session.GetRoleplay().MultiCoolDown["robbery"] = 900;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        Radio.send("Uhh, we have been alerted that the Vault in " + Session.GetHabbo().CurrentRoom.RoomData.Name + " [Bank ID: 13] is being robbed! All units respond!", Session, true);
                        #endregion

                    }
                    return true;


                #endregion

                #region :checkvault

                case "checkvault":
                case "vaultcash":
                    {

                        #region Conditions

                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("VAULT"))
                        {
                            Session.SendWhisper("You must be in the VAULT to use this command!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("vaultcash_check"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("vaultcash_check", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["vaultcash_check"] > 0)
                        {
                            Session.SendWhisper("You must wait until you can check the available vault cash! [" + Session.GetRoleplay().MultiCoolDown["vaultcash_check"] + "/300]");
                            return true;
                        }

                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Pulls out my phone from my pocket, dialing '#648' to check the available vault cash..*", 3);
                        System.Threading.Thread.Sleep(2000);
                        RoleplayManager.Shout(Session, "*Receives a response, hangs up the call and puts my phone away*", 3);
                        Session.SendWhisper("The remaining vault cash available: $" + RoleplayManager.VaultRobbery + "!");

                        Session.GetRoleplay().MultiCoolDown["vaultcash_check"] = 300;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }

                #endregion

                #region :robatm
                case "robatm":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("ATMROB"))
                        {
                            Session.SendWhisper("You cannot rob an ATM in this room");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this while jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this while dead!");
                            return true;
                        }
                        if (Session.GetRoleplay().ATMRobbery)
                        {
                            Session.SendWhisper("You're already robbing the ATM.");
                            return true;
                        }
                        if (Session.GetRoleplay().Working)
                        {
                            Session.SendWhisper("Can't do this while working.");
                            return true;
                        }
                        if (!Session.GetRoleplay().Weapons.ContainsKey("crowbar:1"))
                        {
                            Session.SendWhisper("You need a crowbar to rob the ATM!");
                            return true;
                        }
                        if (!Session.GetRoleplay().NearItem("atm_moneymachine", 1))
                        {
                            Session.SendWhisper("You are not near a ATM!");
                            return true;
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
                                Session.GetRoleplay().RemoveGodProtection();
                            }
                        }

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("atm_robbery"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("atm_robbery", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["atm_robbery"] > 0)
                        {
                            Session.SendWhisper("You must wait until you can rob the atm again! [" + Session.GetRoleplay().MultiCoolDown["atm_robbery"] + "/900]");
                            return true;
                        }
                        #endregion

                        #region Execute
                        int RobTime = 7;
                        if (Session.GetRoleplay().Strength + Session.GetRoleplay().savedSTR > 5)
                        {
                            RobTime = 6;
                        }
                        if (Session.GetRoleplay().Strength + Session.GetRoleplay().savedSTR > 12)
                        {
                            RobTime = 5;
                        }
                        Session.GetRoleplay().ATMRobTimer = new ATMRobTimer(Session, RobTime);
                        Session.GetRoleplay().ATMRobbery = true;
                        Session.GetRoleplay().SaveStatusComponents("atm_robbery");
                        RoleplayManager.Shout(Session, "*Takes out their crowbar, and starts smashing ATM*", 3);
                        Session.SendWhisper("You have " + Session.GetRoleplay().ATMRobTimer.getTime() + " minutes until you rob the ATM!");
                        Session.GetRoleplay().EffectSeconds = RobTime * 60;
                        Session.GetHabbo().GetRoomUser().ApplyEffect(158);
                        Session.GetRoleplay().MultiCoolDown["atm_robbery"] = 900;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        Radio.send("Uhh, we have been alerted that an ATM is being robbed in " + Session.GetHabbo().CurrentRoom.RoomData.Name + " [ID: " + Session.GetHabbo().CurrentRoomId + "]! All units respond!", Session, true);

                        #endregion

                    }
                    return true;


                #endregion

                #region :smokeweed
                case "smokeweed":
                    {

                        #region Conditions
                        if (Session.GetRoleplay().Dead || Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cant smoke weed while dead or jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Weed < 5)
                        {
                            Session.SendWhisper("You must have at least 5g of weed to smoke it!");
                            return true;
                        }
                        if (Session.GetRoleplay().Hunger > 95)
                        {
                            Session.SendWhisper("The weed will make you too hungry if you smoke it!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("weed_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("weed_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["weed_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["weed_cooldown"] + "/120]");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Smokes 5g of weed [+5 Hunger, +1 Strength, -5g Weed]*", 4);
                        Session.SendWhisper("You have 120 seconds until your high runs out!");
                        Session.SendWhisper("You suddenly begin craving the munchies [+5 Hunger)");
                        Session.GetRoleplay().Weed -= 5;
                        Session.GetRoleplay().SaveQuickStat("weed", "" + Session.GetRoleplay().Weed);
                        int PlusHunger = 5;
                        if (Session.GetRoleplay().Hunger + PlusHunger >= 100)
                        {
                            Session.GetRoleplay().Hunger = 100;
                            Session.GetRoleplay().SaveQuickStat("hunger", "" + Session.GetRoleplay().Hunger);
                            Session.SendWhisper("You are starving!");
                        }
                        else
                        {
                            Session.GetRoleplay().Hunger += PlusHunger;
                            Session.GetRoleplay().SaveQuickStat("hunger", "" + Session.GetRoleplay().Hunger);
                            Session.SendWhisper("Your hunger is now " + Session.GetRoleplay().Hunger + "!");
                        }
                        Session.GetRoleplay().UsingWeed_Bonus += 1;
                        Session.GetRoleplay().weedTimer = new weedTimer(Session);
                        Session.GetRoleplay().UsingWeed = true;
                        Session.GetRoleplay().MultiCoolDown["weed_cooldown"] = 120;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;

                    }
                #endregion

                #region :eatcarrot
                case "eatcarrot":
                    {

                        #region Conditions
                        if (Session.GetRoleplay().Dead || Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You can't eat a carrot when you're dead or jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Hunger == 0)
                        {
                            Session.SendWhisper("You can't eat a carrot when you're already full!");
                            return true;
                        }
                        if (Session.GetRoleplay().Carrots <= 0)
                        {
                            Session.SendWhisper("You don't have any carrots to eat!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("carrot_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("carrot_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["carrot_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["carrot_cooldown"] + "/10]");
                            return true;
                        }
                        #endregion

                        #region Execute
                        int MinusHunger = 3;
                        int PlusHealth = 10;
                        RoleplayManager.Shout(Session, "*Pulls out a carrot from their pocket and bites a chunk off [-3 Hunger, +10 Health, -1 Carrot]*", 4);

                        if (Session.GetRoleplay().Hunger - MinusHunger <= 0)
                        {
                            Session.GetRoleplay().Hunger = 0;
                            Session.GetRoleplay().SaveQuickStat("hunger", "" + Session.GetRoleplay().Hunger);
                            Session.SendWhisper("You are famished!");
                        }
                        else
                        {
                            Session.GetRoleplay().Hunger -= MinusHunger;
                            Session.GetRoleplay().SaveQuickStat("hunger", "" + Session.GetRoleplay().Hunger);
                            Session.SendWhisper("Your hunger is now " + Session.GetRoleplay().Hunger + "!");
                        }
                        if (Session.GetRoleplay().CurHealth + PlusHealth >= Session.GetRoleplay().MaxHealth)
                        {
                            Session.GetRoleplay().CurHealth = Session.GetRoleplay().MaxHealth;
                            Session.GetRoleplay().SaveQuickStat("curhealth", "" + Session.GetRoleplay().CurHealth);
                            Session.SendWhisper("You are fully healed!");
                        }
                        else
                        {
                            Session.GetRoleplay().CurHealth += PlusHealth;
                            Session.GetRoleplay().SaveQuickStat("curhealth", "" + Session.GetRoleplay().CurHealth);
                            Session.SendWhisper("Your health is now " + Session.GetRoleplay().CurHealth + "!");
                        }
                        Session.GetRoleplay().MultiCoolDown["carrot_cooldown"] = 120;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        Session.GetRoleplay().Carrots -= 1;
                        Session.GetRoleplay().SaveQuickStat("carrots", "" + Session.GetRoleplay().Carrots);
                        #endregion

                        return true;

                    }
                #endregion

                #endregion

                #region Gangs


                #region :turflist
                case "turflist":
                case "turfs":
                    {
                        string Turfs = "====================== Turf List =======================\n\n";
                        foreach (Turf Turf in GangManager.TurfData.Values)
                        {
                            if (!GangManager.GangData.ContainsKey(Turf.GangId))
                                continue;

                            Turfs += "[Room: " + Turf.TurfId + "] - " + GangManager.GangData[Turf.GangId].Name + "\n";
                        }

                        Session.SendNotifWithScroll(Turfs);

                        return true;
                    }
                #endregion
                #region :gform
                case "gform":
                case "gcreate":
                case "gangcreate":
                case "creategang":
                    {

                        #region Variables/Params

                        string Name = "";

                        #endregion

                        #region Conditions
                        Name = Params[1];

                        if (Params.Length < 1)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :gcreate <gangname>");
                            return true;
                        }
                        else
                        {
                            Name = MergeParams(Params, 1);
                        }
                        if (Session.GetRoleplay().GangRank > 0 && GangManager.validGang(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank))
                        {
                            if (GangManager.GangData[Session.GetRoleplay().GangId].Owner == Convert.ToInt32(Session.GetHabbo().Id))
                            {
                                Session.SendWhisper("You cannot leave a gang that you have created!");
                                return true;
                            }
                            Session.SendWhisper("You are already in a gang!");
                            return true;
                        }
                        if (GangManager.GangExists(Name))
                        {
                            Session.SendWhisper("This gang name is already taken!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            try
                            {
                                dbClient.SetQuery("INSERT INTO rp_gangs(name,owner,kills,deaths,points) VALUES(@name,'" + Session.GetHabbo().Id + "',0,0,0)");
                                dbClient.AddParameter("name", Name);
                                dbClient.RunQuery();

                                GangManager.init();
                                dbClient.RunFastQuery("INSERT INTO rp_gangs_ranks(gangid,rankid,name,pwr_recruit,pwr_demote,pwr_promote,pwr_kick,pwr_alert) VALUES('" + GangManager.GetGangId(Name) + "','1','Recruit',0,0,0,0,1)");
                                dbClient.RunFastQuery("INSERT INTO rp_gangs_ranks(gangid,rankid,name,pwr_recruit,pwr_demote,pwr_promote,pwr_kick,pwr_alert) VALUES('" + GangManager.GetGangId(Name) + "','2','Founder',1,1,1,1,1)");
                                GangManager.init();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }

                        RoleplayManager.Shout(Session, "*Successfully creates the gang '" + Name + "'*");
                        Session.GetRoleplay().GangId = GangManager.GetGangId(Name);
                        Session.GetRoleplay().GangRank = 2;
                        Session.GetRoleplay().SaveGangComponents();

                        #endregion

                        return true;
                    }
                #endregion
                #region :ghelp
                case "ghelp":
                case "aboutgangs":
                    {
                        string About = "=====================\nGang Commands:\n=====================\n";
                        About += " - :gform <gangname>\n";
                        About += " - :ghelp\n";
                        About += " - :ginvite x\n";
                        About += " - :gkick x\n";
                        About += " - :gdemote x\n";
                        About += " - :gpromote x\n";
                        About += " - :gleave\n";
                        About += " - :gdisband\n";
                        About += " - :gbackup\n";
                        About += " - :gaccept\n";
                        About += " - :gdeny\n";
                        About += " - :gcapture\n";

                        Session.SendNotifWithScroll(About);

                        return true;
                    }
                #endregion
                #region :gleave
                case "gleave":
                case "gangleave":
                case "leavegang":
                    {

                        #region Conditions
                        if (Session.GetRoleplay().GangId <= 0)
                        {
                            Session.SendWhisper("You are not in a gang!");
                            return true;
                        }
                        else
                        {
                            if (GangManager.GangData[Session.GetRoleplay().GangId].Owner == Convert.ToInt32(Session.GetHabbo().Id))
                            {
                                Session.SendWhisper("You cannot leave a gang that you have created!");
                                return true;
                            }
                        }
                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Leaves the gang '" + GangManager.GangData[Session.GetRoleplay().GangId].Name + "'*");
                        Session.GetRoleplay().GangId = 0;
                        Session.GetRoleplay().GangRank = 0;
                        Session.GetRoleplay().SendToGang(Session.GetHabbo().UserName + " has left the gang!", false);
                        Session.GetRoleplay().SaveGangComponents();

                        #endregion

                        return true;
                    }
                #endregion
                #region :gdisband
                case "gdisband":
                case "deletegang":
                case "gdelete":
                    {

                        #region Conditions
                        if (Session.GetRoleplay().GangId <= 0)
                        {
                            Session.SendWhisper("You are not in a gang!");
                            return true;
                        }
                        else
                        {
                            if (GangManager.GangData[Session.GetRoleplay().GangId].Owner != Convert.ToInt32(Session.GetHabbo().Id))
                            {
                                Session.SendWhisper("You do not own this gang, so you cannot delete it!");
                                return true;
                            }
                        }
                        #endregion

                        #region Execute


                        RoleplayManager.Shout(Session, "*Deletes their gang '" + GangManager.GangData[Session.GetRoleplay().GangId].Name + "'*");
                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunFastQuery("DELETE FROM rp_gangs WHERE id = " + Session.GetRoleplay().GangId + "");
                            dbClient.RunFastQuery("DELETE FROM rp_gangs_ranks WHERE gangid = " + Session.GetRoleplay().GangId + "");
                            dbClient.RunFastQuery("UPDATE rp_stats SET gang_id = 0, gang_rank = 0 WHERE gang_id = " + Session.GetRoleplay().GangId + "");
                        }


                        RoleplayManager.AlertGang("The gang has been deleted, therefore you were kicked out!", Session.GetRoleplay().GangId, true);


                        GangManager.init();

                        #endregion

                        return true;
                    }
                #endregion
                #region :ginvite
                case "ginvite":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (Session.GetRoleplay().GangId <= 0)
                        {
                            Session.SendWhisper("You are not in a gang!");
                            return true;
                        }
                        if (!GangManager.GangRankData[Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank].CanRecruit())
                        {
                            Session.SendWhisper("Your gang rank is not permitted to do this!");
                            return true;
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("ginvite_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("ginvite_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["ginvite_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["ginvite_cooldown"] + "/5]");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().GangId > 0)
                        {
                            RoleplayManager.Shout(Session, "*Attempts to invite " + TargetSession.GetHabbo().UserName + " to their gang '" + GangManager.GangData[Session.GetRoleplay().GangId].Name + "' but realizes that they're already in a gang*");
                            Session.SendWhisper("This user is already in a gang!");
                            Session.GetRoleplay().MultiCoolDown["ginvite_cooldown"] = 5;
                            return true;
                        }
                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Invites " + TargetSession.GetHabbo().UserName + " to their gang '" + GangManager.GangData[Session.GetRoleplay().GangId].Name + "'*");
                        TargetSession.SendWhisper(Session.GetHabbo().UserName + " has invited you to their gang. Type :gaccept to accept or :gdeny to deny!");
                        TargetSession.GetRoleplay().GangInvitedTo = Session.GetRoleplay().GangId;
                        TargetSession.GetRoleplay().GangInvited = true;
                        Session.GetRoleplay().MultiCoolDown["ginvite_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion
                #region :gaccept
                case "gaccept":
                    {

                        #region Conditions
                        if (!Session.GetRoleplay().GangInvited)
                        {
                            Session.SendWhisper("You have no gang invitations awaiting approval");
                            return true;
                        }
                        if (Session.GetRoleplay().GangId > 0)
                        {
                            Session.SendWhisper("You are already in a gang!");
                            Session.GetRoleplay().GangInvited = false;
                            return true;
                        }
                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Accepts the invitation to join the gang '" + GangManager.GangData[Session.GetRoleplay().GangInvitedTo].Name + "'*");
                        Session.GetRoleplay().GangId = Session.GetRoleplay().GangInvitedTo;
                        Session.GetRoleplay().GangRank = 1;
                        RoleplayManager.AlertGang(Session.GetHabbo().UserName + " has joined the gang.", Session.GetRoleplay().GangId);
                        Session.GetRoleplay().SaveGangComponents();

                        //Reset vars
                        Session.GetRoleplay().GangInvited = false;
                        Session.GetRoleplay().GangInvitedTo = 0;

                        #endregion

                        return true;
                    }
                #endregion
                #region :gdeny
                case "gdeny":
                    {
                        #region Conditions
                        if (!Session.GetRoleplay().GangInvited)
                        {
                            Session.SendWhisper("You have no gang invitations awaiting approval");
                            return true;
                        }
                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Session, "*Denies the invitation to join the gang '" + GangManager.GangData[Session.GetRoleplay().GangInvitedTo].Name + "'*");

                        //Reset vars
                        Session.GetRoleplay().GangInvited = false;
                        Session.GetRoleplay().GangInvitedTo = 0;

                        #endregion

                        return true;
                    }
                #endregion
                #region :gkick
                case "gkick":
                    {
                        #region Generate Instances / Sessions
                        string Target = null;
                        GameClient TargetSession = null;

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :gkick <user>");
                            return true;
                        }
                        else
                        {
                            Target = Convert.ToString(Params[1]);
                            TargetSession = RoleplayManager.GenerateSession(Target);
                        }
                        #endregion

                        #region Conditions

                        bool Offline = true;

                        if (!RoleplayManager.CanInteract(Session, TargetSession, false))
                        {
                            // Session.SendWhisper("Este usuário não foi encontrado!");
                            Offline = true;
                            //return true;
                        }
                        else
                        {
                            Offline = false;
                        }


                        if (!Offline)
                        {
                            if (Session.GetRoleplay().GangId <= 0)
                            {
                                Session.SendWhisper("You are not in a gang!");
                                return true;
                            }
                            if (!GangManager.GangRankData[Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank].CanKick() || GangManager.GangData[Session.GetRoleplay().GangId].Owner == Convert.ToInt32(TargetSession.GetHabbo().Id))
                            {
                                Session.SendWhisper("Your gang rank is not permitted to do this");
                                return true;
                            }
                            if (TargetSession.GetRoleplay().GangId != Session.GetRoleplay().GangId)
                            {
                                Session.SendWhisper("This user is not in your gang!");
                                return true;
                            }
                        }
                        #endregion

                        #region Execute

                        if (!Offline)
                        {
                            RoleplayManager.Shout(Session, "*Kicks " + TargetSession.GetHabbo().UserName + " out of the gang '" + GangManager.GangData[Session.GetRoleplay().GangId].Name + "'*");
                            RoleplayManager.AlertGang(TargetSession.GetHabbo().UserName + " has been kicked out of the gang by " + Session.GetHabbo().UserName + "!", Session.GetRoleplay().GangId);
                            TargetSession.GetRoleplay().GangId = 0;
                            TargetSession.GetRoleplay().GangRank = 0;
                            TargetSession.GetRoleplay().SaveGangComponents();
                        }
                        else
                        {
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                DataRow Row = null;
                                DataRow StatsRow = null;
                                dbClient.SetQuery("SELECT id FROM users WHERE username = '" + Target + "'");
                                Row = dbClient.GetRow();

                                if (Row == null)
                                {
                                    Session.SendWhisper("No such user exists!");
                                    return true;
                                }

                                int Id = Convert.ToInt32(Row["id"]);

                                dbClient.SetQuery("SELECT gang_id,gang_rank FROM rp_stats WHERE id = '" + Id + "'");
                                StatsRow = dbClient.GetRow();

                                if (StatsRow == null)
                                {
                                    Session.SendWhisper("An error occured!");
                                    return true;
                                }

                                if (Convert.ToInt32(StatsRow["gang_id"]) != Session.GetRoleplay().GangId)
                                {
                                    Session.SendWhisper("This user is not in your gang!");
                                    return true;
                                }

                                if (!GangManager.GangRankData[Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank].CanKick() || GangManager.GangData[Session.GetRoleplay().GangId].Owner == Id)
                                {
                                    Session.SendWhisper("You're not allowed to do this!");
                                    return true;
                                }

                                Session.Shout("*Kicks " + Target.ToLower() + "[OFFLINE] out of their gang!*");
                                dbClient.RunFastQuery("UPDATE rp_stats SET gang_id = 0, gang_rank = 0 WHERE id = " + Id + "");
                            }
                        }



                        #endregion

                        return true;
                    }
                #endregion
                #region :gpromote
                case "gpromote":
                    {
                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (Session.GetRoleplay().GangId <= 0)
                        {
                            Session.SendWhisper("You are not in a gang!");
                            return true;
                        }
                        if (!GangManager.GangRankData[Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank].CanPromote() || GangManager.GangData[Session.GetRoleplay().GangId].Owner == Convert.ToInt32(TargetSession.GetHabbo().Id))
                        {
                            Session.SendWhisper("Your gang rank is not permitted to do this");
                            return true;
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().GangId != Session.GetRoleplay().GangId)
                        {
                            Session.SendWhisper("This user is not in your gang!");
                            return true;
                        }
                        if (!GangManager.validGang(TargetSession.GetRoleplay().GangId, TargetSession.GetRoleplay().GangRank + 1))
                        {
                            Session.SendWhisper("There is no gang rank above this users current gang rank!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        TargetSession.GetRoleplay().GangRank += 1;
                        RoleplayManager.Shout(Session, "*Promotes " + TargetSession.GetHabbo().UserName + " to '" + GangManager.GangRankData[TargetSession.GetRoleplay().GangId, TargetSession.GetRoleplay().GangRank].Name + "' in their gang*");

                        RoleplayManager.AlertGang(TargetSession.GetHabbo().UserName + " has been promoted to '" + GangManager.GangRankData[TargetSession.GetRoleplay().GangId, TargetSession.GetRoleplay().GangRank].Name + "' by " + Session.GetHabbo().UserName + "", Session.GetRoleplay().GangId);
                        TargetSession.GetRoleplay().SaveGangComponents();

                        #endregion

                        return true;
                    }
                #endregion
                #region :gdemote
                case "gdemote":
                    {

                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (Session.GetRoleplay().GangId <= 0)
                        {
                            Session.SendWhisper("You are not in a gang!");
                            return true;
                        }
                        if (!GangManager.GangRankData[Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank].CanDemote() || GangManager.GangData[Session.GetRoleplay().GangId].Owner == Convert.ToInt32(TargetSession.GetHabbo().Id))
                        {
                            Session.SendWhisper("Your gang rank is not permitted to do this");
                            return true;
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        if (TargetSession.GetRoleplay().GangId != Session.GetRoleplay().GangId)
                        {
                            Session.SendWhisper("This user is not in your gang!");
                            return true;
                        }
                        if (!GangManager.validGang(TargetSession.GetRoleplay().GangId, TargetSession.GetRoleplay().GangRank - 1))
                        {
                            Session.SendWhisper("There is no gang rank below this users current gang rank!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        TargetSession.GetRoleplay().GangRank -= 1;
                        RoleplayManager.Shout(Session, "*Demotes " + TargetSession.GetHabbo().UserName + " to '" + GangManager.GangRankData[TargetSession.GetRoleplay().GangId, TargetSession.GetRoleplay().GangRank].Name + "' in their gang*");

                        RoleplayManager.AlertGang(TargetSession.GetHabbo().UserName + " has been demoted to '" + GangManager.GangRankData[TargetSession.GetRoleplay().GangId, TargetSession.GetRoleplay().GangRank].Name + "' by " + Session.GetHabbo().UserName + "", Session.GetRoleplay().GangId);
                        TargetSession.GetRoleplay().SaveGangComponents();

                        #endregion

                        return true;
                    }
                #endregion
                #region :gbackup
                case "gbackup":
                case "galert":
                    {
                        #region Variables/Params
                        string Msg = "";
                        #endregion

                        #region Conditions
                        if (Session.GetRoleplay().GangId <= 0)
                        {
                            Session.SendWhisper("You are not in a gang!");
                            return true;
                        }
                        if (Params.Length < 1)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida. :gbackup <msg>");
                            return true;
                        }
                        else
                        {
                            Msg = MergeParams(Params, 1);
                        }
                        if (!GangManager.GangRankData[Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank].CanAlert())
                        {
                            Session.SendWhisper("Your gang rank is not permitted to do this");
                            return true;
                        }
                        #endregion

                        #region Execute

                        Session.GetRoleplay().SendToGang(Msg, true);

                        #endregion

                        return true;
                    }
                #endregion
                #region :gcapture
                case "gcapture":
                    {

                        #region Conditions

                        if (Session.GetRoleplay().GangCapturing)
                        {
                            Session.SendWhisper("You are already capturing a turf!");
                            return true;
                        }
                        if (Session.GetRoleplay().GangId <= 0)
                        {
                            Session.SendWhisper("You are not in a gang!");
                            return true;
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
                                Session.GetRoleplay().RemoveGodProtection();
                            }
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("gcapture_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("gcapture_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["gcapture_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["gcapture_cooldown"] + "/30]");
                            return true;
                        }
                        RoomUser User = null;
                        User = Session.GetHabbo().GetRoomUser();

                        if (User != null)
                        {
                            if (!GangManager.IsTurfSpot(Convert.ToInt32(Session.GetHabbo().CurrentRoomId), User.X, User.Y))
                            {
                                Session.SendWhisper("You are not on a turf capturing spot!");
                                return true;
                            }
                        }
                        else
                        {
                            Session.SendWhisper("Something went wrong! Please try again later..");
                            return true;
                        }

                        if (GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId == Session.GetRoleplay().GangId)
                        {
                            Session.SendWhisper("Your gang already owns this turf!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        User.ApplyEffect(59);
                        Session.GetRoleplay().GangCapturing = true;
                        Session.GetRoleplay().gangCaptureTimer = new gangCaptureTimer(Session);
                        Session.GetRoleplay().MultiCoolDown["gcapture_cooldown"] = 30;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        RoleplayManager.Shout(Session, "*Begins to capture the turf in the name of '" + GangManager.GangData[Session.GetRoleplay().GangId].Name + "'*", 6);
                        Session.SendWhisper("You have 5 minute(s) left until you capture the turf!");
                        if (GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId > 0)
                        {
                            RoleplayManager.AlertGang("Someone just began capturing your turf in RoomID " + Session.GetHabbo().CurrentRoomId + "! Get there fast!", GangManager.TurfData[Convert.ToInt32(Session.GetHabbo().CurrentRoomId)].GangId);
                        }
                        return true;

                        #endregion

                    }
                #endregion
                #region :mygang/:ginfo
                case "mygang":
                case "ginfo":
                    {
                        #region Conditions
                        if (Session.GetRoleplay().GangId <= 0 || !GangManager.validGang(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank))
                        {
                            Session.SendWhisper("You are not in a gang!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        string info = "==============";

                        foreach (Gang Gang in GangManager.GangData.Values)
                        {
                            if (Gang.Id != Session.GetRoleplay().GangId)
                                continue;

                            info += "======================\nGang Information Of '" + Gang.Name + "'\n======================\n";
                            info += "Gang ID: " + Gang.Id + "\n";
                            info += "Gang: " + Gang.Name + "\n";
                            info += "Owner: " + RoleplayManager.ReturnOfflineInfo((uint)Gang.Owner, "username").ToString() + "\n";
                            info += "Points: " + Gang.Points + "\n";
                            info += "Kills: " + Gang.Kills + "\n";
                            info += "Deaths: " + Gang.Deaths + "\n";
                        }

                        Session.SendNotifWithScroll(info);

                        return true;

                        #endregion


                    }
                #endregion

                #endregion

                #region Combat

                #region RP bots / Interactions

                #region :stopfight
                case "stopfight":
                case "stopattack":
                    {

                        #region Variables

                        if (Session.GetHabbo().GetRoomUser().MyPet == null)
                        {
                            Session.SendWhisper("Cannot do this since your pet isn't here?");
                            return true;
                        }

                        RoomUser Pet = Session.GetHabbo().GetRoomUser().MyPet;

                        #endregion

                        if (Pet == null)
                        {
                            Session.SendWhisper("Can't do this for some reason?");
                            return true;
                        }

                        Pet.BotAI._Victim = null;
                        // Pet.BotAI._Victim.AttackPet = Pet;
                        RoleplayManager.Shout(Session, "*Tells their pet to come back*");
                        FightPetManager.WalkToPlayer(Pet, Session.GetHabbo().GetRoomUser());
                        return true;

                    }
                #endregion

                #region :givepet <username> <petname>
                case "givepet":
                    {

                        #region Variables

                        if (!Session.GetRoleplay().UsingPet) return false;
                        GameClient User = null;
                        if (Params[1] != null)
                        {
                            User = RoleplayManager.GenerateSession(Params[1].ToString());
                        }

                        #endregion

                        #region

                        /*
                        using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT * FROM bots WHERE user_id = '" + session.GetHabbo().Id + "'");
                            DataTable Data = dbClient.GetTable();

                            foreach (DataRow PetRow in Data.Rows)
                            {

                            }
                        }*/
                        #endregion


                        return true;
                    }
                #endregion

                #region :killpet <name>
                case "killpet":
                case "kickpet":
                case "removepet":
                    {
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        if (Params.Length == 1)
                        {
                            Session.GetHabbo().GetRoomUser().LastBubble = 34;
                            Session.SendWhisper("Incorrect Syntax. :killpet <name>");
                            Session.GetHabbo().GetRoomUser().LastBubble = 0;
                            return true;
                        }
                        string Username = Params[1].ToString();

                        foreach (RoomUser User in Session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values)
                        {

                            if (!User.IsBot)
                            {
                                continue;
                            }
                            if (!User.IsPet)
                            {
                                continue;
                            }
                            if (User.BotData.Name.ToLower() != Username.ToLower())
                            {
                                continue;
                            }
                            Session.GetHabbo().CurrentRoom.GetRoomUserManager().RemoveBot(User.VirtualId, true);
                        }
                        Session.GetHabbo().GetRoomUser().LastBubble = 23;
                        Session.Shout("*Uses their god-like powers to destroy " + Username.ToLower() + "*");
                        Session.GetHabbo().GetRoomUser().LastBubble = 0;
                        return true;
                    }
                #endregion

                #region :setaitype <newaitype>

                case "setaitype":
                    {
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return false;
                        }

                        string Aitype;

                        Aitype = Convert.ToString(Params[1]);
                        RoomUser Pet = null;

                        foreach (RoomUser User in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetBots())
                        {
                            if (User == null)
                                continue;

                            Pet = User;
                        }


                        if (Pet == null)
                        {
                            Session.SendWhisper("No pet was found!");
                            return true;
                        }

                        AIType type = AIType.FightPet;
                        switch (Aitype.ToLower())
                        {
                            case "fight_pet":
                            case "fightpet":
                                type = AIType.FightPet;
                                break;

                            case "pet":
                                type = AIType.Pet;
                                break;
                        }

                        Session.Shout("*Users their god-like powers to set " + Pet.BotData.Name + "'s Aitype to " + Aitype.ToLower() + "!");
                        Pet.BotData.AiType = type;
                        RoomUser TempPet = Pet;

                        Session.GetHabbo().CurrentRoom.GetRoomUserManager().RemoveBot(Pet.VirtualId, true);
                        Session.GetHabbo().CurrentRoom.GetRoomUserManager().DeployBot(TempPet.BotData, TempPet.PetData);

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunFastQuery("UPDATE bots SET ai_type = '" + Aitype.ToLower() + "' WHERE id = " + Pet.PetData.PetId + "");
                        }




                        return true;
                    }

                #endregion

                #region :attack <user>
                case "fight":
                case "attack":
                    {

                        #region Variables

                        if (Session.GetHabbo().GetRoomUser().MyPet == null)
                        {
                            Session.SendWhisper("Cannot do this since your pet isn't here?");
                            return true;
                        }

                        RoomUser Pet = Session.GetHabbo().GetRoomUser().MyPet;
                        GameClient User = null;
                        if (Params[1] != null)
                        {
                            User = RoleplayManager.GenerateSession(Params[1].ToString());
                        }

                        #endregion

                        if (User == null)
                            return true;
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOHIT") && !RoleplayManager.BypassRights(Session))
                            return true;

                        if (Pet == null) return true;

                        #region Conditions
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("SAFEZONE"))
                        {
                            Session.GetHabbo().GetRoomUser().LastBubble = 34;
                            Session.SendWhisper("Desculpe, mas esta é uma zona segura!");
                            Session.GetHabbo().GetRoomUser().LastBubble = 0;
                            return true;
                        }
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOPET"))
                        {
                            Session.GetHabbo().GetRoomUser().LastBubble = 34;
                            Session.SendWhisper("Sorry but this room doesnt allow pets!");
                            Session.GetHabbo().GetRoomUser().LastBubble = 0;
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.GetHabbo().GetRoomUser().LastBubble = 34;
                            Session.SendWhisper("Cannot complete this action as you are dead");
                            Session.GetHabbo().GetRoomUser().LastBubble = 0;
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.GetHabbo().GetRoomUser().LastBubble = 34;
                            Session.SendWhisper("Cannot complete this action as you are jailed");
                            Session.GetHabbo().GetRoomUser().LastBubble = 0;
                            return true;
                        }
                        if (User.GetRoleplay().Dead)
                        {
                            Session.GetHabbo().GetRoomUser().LastBubble = 34;
                            Session.SendWhisper("Cannot complete this action as this user is dead!");
                            Session.GetHabbo().GetRoomUser().LastBubble = 0;
                        }
                        if (User.GetRoleplay().Jailed)
                        {
                            Session.GetHabbo().GetRoomUser().LastBubble = 34;
                            Session.SendWhisper("Cannot complete this action as this user is jailed!");
                            Session.GetHabbo().GetRoomUser().LastBubble = 0;
                        }
                        if (User.GetHabbo().GetRoomUser().IsAsleep)
                        {
                            Session.GetHabbo().GetRoomUser().LastBubble = 34;
                            Session.SendWhisper("Cannot complete this action as this user is not here!");
                            Session.GetHabbo().GetRoomUser().LastBubble = 0;
                        }
                        #endregion


                        Pet.BotAI._Victim = User.GetHabbo().GetRoomUser();
                        Pet.BotAI._Victim.AttackPet = Pet;
                        RoleplayManager.Shout(Session, "*Commands their pet to attack " + User.GetHabbo().UserName + "*");
                        return true;

                    }
                #endregion

                #region :ffight <user>
                case "ffight":
                    {
                        if (!RoleplayManager.BypassRights(Session)) { Session.SendWhisper("You cannot do this!"); return true; }
                        string Username = null;
                        GameClient User = null;

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Invalid params!");
                            return true;
                        }
                        else
                        {
                            Username = Convert.ToString(Params[1]);
                            User = RoleplayManager.GenerateSession(Username);
                        }

                        if (!RoleplayManager.CanInteract(Session, User, true))
                        {
                            Session.SendWhisper("User not found!");
                            return true;
                        }

                        List<RoomUser> RoomBots = new List<RoomUser>();

                        lock (Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetBots())
                        {
                            foreach (RoomUser Users in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetBots())
                            {
                                if (Users == null)
                                    continue;
                                if (!Users.IsBot)
                                    continue;
                                if (Users.BotAI == null)
                                    continue;
                                RoomBots.Add(Users);
                            }
                        }

                        foreach (RoomUser Bott in RoomBots)
                        {
                            Bott.BotAI = new FightPet(Bott.VirtualId);
                            Bott.BotAI._Victim = User.GetHabbo().GetRoomUser();
                        }

                        RoleplayManager.Shout(Session, "*Uses their god-like powers to command every BOT in the room to attack " + User.GetHabbo().UserName + "*");
                        return true;

                    }
                #endregion

                #region :setpetstat <petname> <stat> <newvalue>
                case "setbotstat":
                case "setbotdata":
                case "setpetstat":
                case "setpetdata":
                    {

                        #region Params
                        string BotName = null;
                        string Stat = null;
                        object NewValue = null;
                        RoomUser Bot = null;
                        #endregion

                        #region Conditions
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("You cannot do this!");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 3))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :setbotstat <botname> <stat> <newvalue>");
                            return true;
                        }
                        else
                        {
                            BotName = Convert.ToString(Params[1]);
                            Stat = Convert.ToString(Params[2]);
                            NewValue = Params[3];
                        }

                        if (RoleplayManager.GetBot(BotName, Session.GetHabbo().CurrentRoom) == null)
                        {
                            Session.SendWhisper("The bot " + BotName + " was not found in this room!");
                            return true;
                        }

                        Bot = RoleplayManager.GetBot(BotName, Session.GetHabbo().CurrentRoom);

                        #endregion

                        #region Execute
                        switch (Stat.ToLower())
                        {


                            default:


                                string Notice = "No such stat as " + Stat.ToLower() + ".. \nEditable stats are:\n";
                                // Notice += "type";
                                Notice += "strength/str\n";
                                Notice += "follow_interval\n";
                                Notice += "curhealth\n";
                                Notice += "maxhealth\n";
                                Notice += "cooldown\n";

                                Session.SendNotif(Notice);

                                break;

                            case "type":

                                Bot.BotData.Type = Convert.ToString(NewValue);

                                break;

                            case "strength":
                            case "str":

                                Bot.BotData.Str = Convert.ToInt32(NewValue);
                                break;

                            case "curhealth":

                                Bot.BotData.CurHealth = Convert.ToInt32(NewValue);

                                break;

                            case "maxhealth":

                                Bot.BotData.MaxHealth = Convert.ToInt32(NewValue);
                                Bot.BotData.InitRPStats();

                                break;

                            case "cooldown":

                                Bot.BotData.Cooldown = Convert.ToInt32(NewValue);
                                Bot.BotData.InitRPStats();

                                break;

                            case "follow_interval":

                                Bot.BotData.FollowInterval = Convert.ToInt32(NewValue);
                                Bot.BotData.InitRPStats();

                                break;
                        }


                        Bot.BotData.SaveRPStats();


                        Session.SendWhisper("Successfully set the bot " + Bot.BotData.Name + "'s " + Stat + " to " + NewValue);

                        #endregion


                        return true;
                    }
                #endregion

                #region :refreshpets
                case "refreshpets":
                    {


                        #region Refresh roleplay pets
                        Dictionary<string, uint> Pets = new Dictionary<string, uint>();

                        try
                        {
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("SELECT * FROM bots WHERE user_id = '" + Session.GetHabbo().Id + "'");
                                DataTable Data = dbClient.GetTable();


                                foreach (DataRow PetRow in Data.Rows)
                                {
                                    if (!Pets.ContainsKey(Convert.ToString(PetRow["name"]).ToLower()))
                                        Pets.Add(Convert.ToString(PetRow["name"]).ToLower(), Convert.ToUInt32(PetRow["id"]));
                                }
                            }

                            lock (Session.GetRoleplay().MyPets)
                            {
                                Session.GetRoleplay().MyPets.Clear();
                                Session.GetRoleplay().MyPets = Pets;
                            }
                        }
                        catch (Exception e)
                        {

                        }

                        #endregion

                        Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
                        Session.GetHabbo().GetInventoryComponent().SerializePetInventory();
                        Session.SendWhisper("Refreshed the shit");

                        return true;
                    }
                #endregion

                #region :callpet <petname>
                case "callpet":
                case "placepet":
                    {

                        #region Variables
                        string BotName = null;
                        bool BotDeployed = false;


                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :callpet <petname>");
                            return true;
                        }
                        else
                        {
                            BotName = Convert.ToString(Params[1].ToLower());
                        }

                        #endregion

                        #region Checks

                        using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
                        {



                            if (!Session.GetRoleplay().MyPets.ContainsKey(BotName.ToLower()))
                            {
                                Session.SendWhisper("You do not have a pet called '" + BotName.ToLower() + "'");
                                return true;
                            }

                            uint PetId = Session.GetRoleplay().MyPets[BotName];

                            FightPetManager Manager = new FightPetManager();

                            RoomBot Pet = Manager.DeployBotToRoom(Session, BotName, Session.GetHabbo().CurrentRoomId);


                            if (Pet == null)
                            {
                                Session.SendWhisper("For some reason I wasn't able to place down your pet!");
                                return true;
                            }


                            Pet.RoomUser.Chat(null, "Hello master!", true, 0, 0);
                        }


                        #endregion




                        return true;
                    }
                #endregion

                #region :sellpet
                case "sellpet":
                    {

                        #region Conditions
                        if (!Session.GetRoleplay().JobHasRights("sellpet") && !RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("Your job cannot sell pets!");
                            return true;
                        }
                        if (!Session.GetRoleplay().Working && !RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para fazer isso!");
                            return true;
                        }
                        #endregion


                        #region Variables

                        GameClient TargetSession = null;
                        string User = null;
                        string PetType = null;
                        string Petname = null;

                        #endregion

                        #region More Conditions
                        if (!RoleplayManager.ParamsMet(Params, 3))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :sellpet <user> <(dragon,dog,horse..etc)> <petname>");
                            return true;
                        }
                        #endregion

                        #region More Variables
                        User = Convert.ToString(Params[1].ToLower());
                        TargetSession = RoleplayManager.GenerateSession(User);
                        PetType = Convert.ToString(Params[2].ToLower());
                        Petname = Convert.ToString(Params[3].ToLower());
                        List<string> PetTypes = new List<string>();
                        //PetTypes.Add()
                        #endregion

                        #region More Conditions
                        if (!RoleplayManager.CanInteract(Session, TargetSession, true))
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala!");
                            return true;
                        }


                        #endregion


                        if (!FightPetManager.PetData.ContainsKey(PetType.ToLower()))
                        {
                            Session.SendWhisper("The pet-type '" + PetType.ToLower() + "' does exist!");
                            return true;
                        }

                        RPPet Pet = FightPetManager.PetData[PetType.ToLower()];
                        bool PayOnlyPet = Pet.Special_Pet == 0 ? false : true;
                        int PetPrice = Pet.Price_Coins;

                        #region Target Conditions
                        if (PayOnlyPet)
                        {
                            Session.SendWhisper("This pet you are trying to sell is a premium pet, please inform the user of this!");
                            return true;
                        }
                        if (TargetSession.GetHabbo().Credits - PetPrice < 0)
                        {
                            Session.SendWhisper("This user cannot afford to buy this pet, please inform them the price is " + PetPrice + "c!");
                            return true;
                        }
                        if (RoleplayManager.PetExists(Petname.ToLower()))
                        {
                            Session.SendWhisper("This pet name ['" + Petname.ToLower() + "'] already exists, please tell the user to choose a different name!");
                            return true;
                        }
                        #endregion

                        if (TargetSession.GetRoleplay().OfferData.ContainsKey("pet"))
                        {
                            TargetSession.GetRoleplay().OfferData.Remove("pet");
                        }

                        TargetSession.GetRoleplay().OfferData.Add("pet", new Offer(Session, Petname + ":" + Pet.Type, 1, PetPrice));


                        TargetSession.SendWhisper(Session.GetHabbo().UserName + " has offered to sell you a pet [" + Pet.Type.ToUpper() + ", called '" + Petname + "'] for [$" + PetPrice + "]. Type #accept to accept or #deny to deny! ");

                        Session.Shout("*Offers to sell " + TargetSession.GetHabbo().UserName + " a pet [" + Pet.Type.ToUpper() + ", called '" + Petname + "'] for [$" + PetPrice + "]*");

                        return true;
                    }
                #endregion

                #endregion

                #region Special Weapons

                #region Freeze Ray

                #region :freezehelp
                case "freezehelp":
                    {

                        #region Execute

                        string Cmds = "";
                        Cmds += "List of Freeze Ray Commands:\n";
                        Cmds += ":freezehelp\n";
                        Cmds += ":freezecharge\n";
                        Cmds += ":freezerelease\n";
                        Cmds += ":<this command is hidden/for admins only>";


                        Session.SendNotifWithScroll(Cmds);

                        #endregion

                        return true;
                    }
                #endregion

                #region :freezecharge
                case "freezecharge":
                    {
                        #region Conditions
                        if (Session.GetRoleplay().Equiped != "freezeray")
                        {
                            Session.SendWhisper("You must have a freeze ray to do this!");
                            return true;
                        }
                        if (Session.GetRoleplay().FreezeRay.Charging)
                        {
                            Session.SendWhisper("Your freeze ray is already charging up!");
                            return true;
                        }
                        if (Session.GetRoleplay().FreezeRay.Releasing)
                        {
                            Session.SendWhisper("You are already releasing your mega blast!");
                            return true;
                        }
                        /*
                         if(Session.GetRoleplay().FreezeRay.ReleaseCdMins > 0)
                         {
                             Session.SendWhisper("Cooldown [" + Session.GetRoleplay().FreezeRay.ReleaseCdMins + " mins left]");
                             return true;
                         }*/
                        #endregion

                        #region Execute

                        Session.GetRoleplay().FreezeRay.ChargingMins = Convert.ToInt32(RoleplayData.Data["freeze.ray.default.chargetime"]);
                        Session.GetRoleplay().FreezeRay.ChargingSeconds = 60;
                        Session.GetRoleplay().FreezeRay.Charging = true;


                        if (RoleplayData.Data["freeze.ray.show.shoutcharge"] == "true")
                        {
                            Session.Shout("*Begins charging up their freeze ray to release a mega blast*");
                        }
                        else
                        {
                            Session.SendWhisper("Okay, your freeze ray is now charging to release a blast. It will be complete in " + Session.GetRoleplay().FreezeRay.ChargingMins + " minutes!");
                        }

                        #endregion

                        return true;
                    }
                #endregion

                #region :freezerelease
                case "freezerelease":
                    {


                        #region Conditions
                        if (Session.GetRoleplay().Equiped != "freezeray")
                        {
                            Session.SendWhisper("You must have a freeze ray to do this!");
                            return true;
                        }
                        if (Session.GetRoleplay().FreezeRay.Charging)
                        {
                            Session.SendWhisper("Your freeze ray is already charging up!");
                            return true;
                        }
                        if (!Session.GetRoleplay().FreezeRay.Charged)
                        {
                            Session.SendWhisper("Your freeze ray is not charged!");
                            return true;
                        }
                        if (Session.GetRoleplay().FreezeRay.Releasing)
                        {
                            Session.SendWhisper("You are already releasing your mega blast!");
                            return true;
                        }

                        if (Session.GetRoleplay().FreezeRay.ReleaseCdMins > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().FreezeRay.ReleaseCdMins + " mins left]");
                            return true;
                        }
                        #endregion

                        #region Execute

                        Session.GetRoleplay().FreezeRay.Releasing = true;
                        Session.GetRoleplay().FreezeRay.ReleasingSeconds = new Random().Next(5, 30);
                        Session.Shout("*Emits a freeze blast from their super charged freeze ray*");

                        Session.GetRoleplay().MultiCoolDown["equip_cooldown"] = 60;

                        #endregion

                        return true;
                    }
                #endregion

                #region :freezesrelease
                case "freezesrelease":
                    {


                        #region Params

                        #endregion

                        #region Conditions

                        #endregion

                        #region Execute

                        #endregion

                        return true;
                    }
                #endregion

                #endregion

                #endregion

                #region Boxing
                case "soloqueue":
                    {
                        if (Session.GetHabbo().CurrentRoom.SoloQueue != null)
                        {
                            var SQueue = Session.GetHabbo().CurrentRoom.SoloQueue;
                            SQueue.AddToQueue(Session);
                        }
                        return true;
                    }
                #endregion

                #region :hit x
                case "soco":
                    {
                        if (Session.GetHabbo().CurrentRoom.SoloQueue != null)
                        {
                            if (!Session.GetRoleplay().IsBoxing)
                            {
                                Session.SendWhisper("Oi, you have to be boxing to do that sir!");
                                return true;
                            }
                        }
                        #region Melee Weapons
                        if (Session.GetRoleplay().Equiped != null && WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Type.ToLower().Contains("melee"))
                        {
                            if (!RoleplayManager.ParamsMet(Params, 1))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :hit <user>");
                                return true;
                            }
                            #region Generate Instances / Sessions
                            GameClient TargetSession = null;
                            if (Session.GetRoleplay().StaffDuty == true)
                            {
                                Session.SendWhisper("Você não pode bater enquanto em serviço!");
                                return true;
                            }
                            if (Session.GetRoleplay().LastHit != null)
                            {
                                TargetSession = Session.GetRoleplay().LastHit;
                            }
                            string Target = Convert.ToString(Params[1]);
                            TargetSession = RoleplayManager.GenerateSession(Target);

                            if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                            {
                                Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                                return true;
                            }

                            Session.GetRoleplay().LastHit = TargetSession;
                            Session.GetRoleplay().ActionLast = "melee";
                            #endregion
                            #region Execute

                            if (TargetSession.GetRoleplay().StaffDuty == true && !RoleplayManager.BypassRights(Session))
                            {
                                Session.SendWhisper("Você não pode bater em um staff de plantão");
                                return true;
                            }
                            if (MeleeCombat.CanExecuteAttack(Session, TargetSession))
                            {
                                MeleeCombat.ExecuteAttack(Session, TargetSession);
                            }

                            #endregion
                        }
                        #endregion

                        #region Punching
                        else
                        {
                            #region Generate Instances / Sessions

                            bool bypass = false;
                            GameClient TargetSession = null;

                            if (Session.GetRoleplay().StaffDuty == true)
                            {
                                Session.SendWhisper("Você não pode bater enqaunto estiver de plantão!");
                                return true;
                            }
                            if (Session.GetRoleplay().LastHit != null)
                            {
                                TargetSession = Session.GetRoleplay().LastHit;
                            }

                            if (!bypass)
                            {
                                if (!RoleplayManager.ParamsMet(Params, 1))
                                {
                                    Session.SendWhisper("Sintaxe de comando inválida: :soco <user>");
                                    return true;
                                }

                                string Target = Convert.ToString(Params[1]);
                                RoomUser T = null;

                                foreach (RoomUser User in Session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values)
                                {
                                    if (User.IsBot && User.BotData.Name.ToLower() == Target.ToLower())
                                        T = User;
                                }

                                if (T != null)
                                {
                                    HabboHotel.Roleplay.Combat.HandCombat.ExecuteAttackBot(Session, T, T.PetData, T.BotData);
                                    return true;
                                }

                                TargetSession = RoleplayManager.GenerateSession(Target);

                                if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                                {
                                    Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                                    return true;
                                }

                                if (TargetSession.GetRoleplay().StaffDuty == true && !RoleplayManager.BypassRights(Session))
                                {
                                    Session.SendWhisper("Você não pode bater em um staff de plantão");
                                    return true;
                                }
                                else
                                {
                                    Session.GetRoleplay().LastHit = TargetSession;
                                    Session.GetRoleplay().ActionLast = "hit";
                                }
                            }
                            #endregion

                            #region Execute
                            HabboHotel.Roleplay.Combat.HandCombat.ExecuteAttack(Session, TargetSession);
                            #endregion
                        }
                        #endregion
                        return true;
                    }
                #endregion
                #region :shoot x
                case "atirar":
                    {
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :aitrar <user>");
                            return true;
                        }

                        #region Generate Instances / Sessions
                        GameClient TargetSession = null;
                        if (Session.GetRoleplay().StaffDuty == true)
                        {
                            Session.SendWhisper("Você não pode atirar enquanto estiver de plantão!");
                            return true;
                        }
                        if (Session.GetRoleplay().Equiped == null)
                        {
                            Session.SendWhisper("Você não equipou uma arma!");
                            return true;
                        }
                        if (Session.GetRoleplay().LastHit != null)
                        {
                            TargetSession = Session.GetRoleplay().LastHit;
                        }
                        string Targ = Convert.ToString(Params[1]);
                        TargetSession = RoleplayManager.GenerateSession(Targ);

                        #region Null Checks
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo() == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo().GetRoomUser() == null)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }

                        if (TargetSession.GetHabbo().GetRoomUser().RoomId != Session.GetHabbo().GetRoomUser().RoomId)
                        {
                            Session.SendWhisper("O usuário não foi encontrado nesta sala!");
                            return true;
                        }
                        #endregion

                        Session.GetRoleplay().LastHit = TargetSession;
                        Session.GetRoleplay().ActionLast = "shoot";
                        #endregion

                        #region Execute

                        if (TargetSession.GetRoleplay().StaffDuty == true && !RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("Você não pode atirar em um staff que está de plantão!");
                            return true;
                        }


                        Room Room = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);

                        RoomUser Me = Session.GetHabbo().GetRoomUser();
                        RoomUser Target = TargetSession.GetHabbo().GetRoomUser();

                        if (Target == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                            return true;
                        }

                        if (Me == null)
                        {
                            return true;
                        }

                        if (CombatManager.CanAttack(Session, TargetSession, true))
                        {
                            GunCombat.ExecuteAttack(Session, TargetSession, Room, Me, Target);
                        }

                        #endregion

                        return true;
                    }
                #endregion
                #region :bomb x
                case "bomb":
                case "bombs":
                    {
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :bomb <user>");
                            return true;
                        }

                        #region Generate Instances / Sessions
                        GameClient TargetSession = null;

                        if (Session.GetRoleplay().StaffDuty == true)
                        {
                            Session.SendWhisper("You cannot bomb while on duty!");
                            return true;
                        }
                        if (Session.GetRoleplay().LastHit != null)
                        {
                            TargetSession = Session.GetRoleplay().LastHit;
                        }
                        string Target = Convert.ToString(Params[1]);
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        Session.GetRoleplay().LastHit = TargetSession;
                        Session.GetRoleplay().ActionLast = "bomb";
                        #endregion

                        #region Execute

                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                            return true;
                        }

                        if (TargetSession.GetRoleplay().StaffDuty == true)
                        {
                            Session.SendWhisper("You cannot bomb a staff that is on duty!");
                            return true;
                        }
                        if (BombCombat.CanExecuteAttack(Session, TargetSession))
                        {
                            BombCombat.ExecuteAttack(Session, TargetSession);
                        }
                        #endregion

                        return true;
                    }
                #endregion
                #region :equip
                case "equipar":
                    {

                        #region Params/Variables
                        int MyJobId = Session.GetRoleplay().JobId;
                        int MyJobRank = Session.GetRoleplay().JobRank;
                        string Wep = "";
                        RoomUser User = Session.GetHabbo().GetRoomUser();
                        #endregion

                        #region Conditions
                        if (Params.Length < 2)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :equip <weapon>");
                        }
                        else
                        {
                            Wep = Params[1].ToString();
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("equip_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("equip_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["equip_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["equip_cooldown"] + "/5]");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead || Session.GetRoleplay().Jailed)
                        {
                            return true;
                        }
                        Dictionary<string, Weapon> Weaponss = new Dictionary<string, Weapon>();
                        foreach (KeyValuePair<string, Weapon> Weapon in Session.GetRoleplay().Weapons)
                        {
                            Weaponss.Add(WeaponManager.GetWeaponName(Weapon.Key), Weapon.Value);
                        }
                        if (!Weaponss.ContainsKey(Wep) && !Wep.ToLower().Contains("police") && !Wep.ToLower().Contains("npa"))
                        {
                            Session.SendWhisper("You do not have a " + Wep + "!");
                            return true;
                        }
                        if (Session.GetRoleplay().RayFrozen)
                        {
                            Session.SendWhisper("You cannot do this while you are frozen!");
                        }
                        if (Wep.ToLower().Contains("police"))
                        {
                            if (!JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                            {
                                Session.SendWhisper("Seu trabalho não pode fazer isso!", false, 34);
                                return true;
                            }

                            if (Session.GetRoleplay().JobHasRights("police")
                                || Session.GetRoleplay().JobHasRights("swat")
                                || Session.GetRoleplay().JobHasRights("gov"))
                            {
                                if (Session.GetRoleplay().Working)
                                {
                                    Session.GetRoleplay().Equiped = WeaponManager.WeaponsData[Wep].Name;
                                    RoleplayManager.Shout(Session, "*Equips their " + WeaponManager.WeaponsData[Wep].DisplayName + "*");
                                    Session.GetRoleplay().MultiCoolDown["equip_cooldown"] = 5;
                                    Session.GetRoleplay().CheckingMultiCooldown = true;
                                    User.ApplyEffect(101);
                                    return true;
                                }
                                else
                                {
                                    Session.SendWhisper("You must be working to use this gun!");
                                    return true;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Your job is not in law enforcement!");
                                return true;
                            }
                        }
                        if (Wep.ToLower().Contains("npa"))
                        {

                            if (!JobManager.validJob(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                            {
                                Session.SendWhisper("Seu trabalho não pode fazer isso!", false, 34);
                                return true;
                            }

                            if (Session.GetRoleplay().JobHasRights("npa"))
                            {
                                if (Session.GetRoleplay().Working)
                                {
                                    Session.GetRoleplay().Equiped = WeaponManager.WeaponsData[Wep].Name;
                                    RoleplayManager.Shout(Session, "*Equips their " + WeaponManager.WeaponsData[Wep].DisplayName + "*");
                                    Session.GetRoleplay().MultiCoolDown["equip_cooldown"] = 5;
                                    Session.GetRoleplay().CheckingMultiCooldown = true;
                                    User.ApplyEffect(101);
                                    return true;
                                }
                                else
                                {
                                    Session.SendWhisper("You must be working to use this gun!");
                                    return true;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Your job is not NPA!");
                                return true;
                            }
                        }
                        #endregion

                        #region Execute


                        Session.GetRoleplay().Equiped = WeaponManager.WeaponsData[Wep].Name;


                        string EquipMSG = "";

                        EquipMSG = WeaponManager.WeaponsData[Wep].Equip_Msg.Replace("%weapon_name%", WeaponManager.WeaponsData[Wep].DisplayName);
                        Session.Shout(EquipMSG);
                        User.ApplyEffect(WeaponManager.WeaponsData[Wep].Effect_Id);
                        User.CarryItem(WeaponManager.WeaponsData[Wep].HandItem);
                        Session.GetRoleplay().MultiCoolDown["equip_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        switch (Session.GetRoleplay().Equiped)
                        {
                            case "freezeray":
                                {

                                    if (Session.GetRoleplay().FreezeRay == null)
                                    {
                                        Session.GetRoleplay().FreezeRay = new FreezeRay(Session);
                                        Session.GetRoleplay().FreezeRay.On = true;

                                        if (RoleplayData.Data["freeze.debug.show.msgs"] == "true")
                                        {
                                            Session.SendWhisper("Freeze ray was null, it is now true!");
                                        }

                                    }
                                    else
                                    {

                                        if (RoleplayData.Data["freeze.debug.show.msgs"] == "true")
                                        {
                                            Session.SendWhisper("Freeze ray was not null!");
                                        }

                                        Session.GetRoleplay().FreezeRay.On = true;
                                        Session.GetRoleplay().FreezeRay.PreparingMsg = false;
                                    }

                                }
                                break;
                        }

                        #endregion

                        return true;
                    }
                #endregion
                #region :unequip
                case "desequipar":
                    {

                        #region Params/Variables
                        string Wep = "";
                        RoomUser User = Session.GetHabbo().GetRoomUser();
                        #endregion

                        #region Conditions
                        if (Params.Length < 2)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :unequip <weapon>");
                            return true;
                        }
                        else
                        {
                            Wep = Params[1].ToString();
                        }
                        if (Session.GetRoleplay().Dead || Session.GetRoleplay().Jailed)
                        {
                            return true;
                        }
                        Dictionary<string, Weapon> Weaponss = new Dictionary<string, Weapon>();
                        foreach (KeyValuePair<string, Weapon> Weapon in Session.GetRoleplay().Weapons)
                        {
                            Weaponss.Add(WeaponManager.GetWeaponName(Weapon.Key), Weapon.Value);
                        }
                        if (!Weaponss.ContainsKey(Wep) && !Wep.ToLower().Contains("police") && !Wep.ToLower().Contains("npa"))
                        {
                            Session.SendWhisper("You do not have a " + Wep + "!");
                            return true;
                        }
                        if (Wep.ToLower().Contains("police"))
                        {
                            User.ApplyEffect(0);
                            RoleplayManager.Shout(Session, "*Un-Equips their " + Session.GetRoleplay().Equiped + " gun*");
                            Session.GetRoleplay().Equiped = null;
                            return true;
                        }
                        if (Wep.ToLower().Contains("npa"))
                        {
                            User.ApplyEffect(0);
                            RoleplayManager.Shout(Session, "*Un-Equips their " + Session.GetRoleplay().Equiped + " gun*");
                            Session.GetRoleplay().Equiped = null;
                            return true;
                        }
                        if (Session.GetRoleplay().Equiped != Wep)
                        {
                            Session.SendWhisper("You cannot unequip that weapon as it was not equiped in the first place!");
                            return true;
                        }
                        #endregion

                        #region Execute

                        Session.GetRoleplay().UnEquip();

                        #endregion

                        return true;
                    }
                #endregion
                #region :buybullets
                case "buybullets":
                case "comprarbalas":
                    {

                        try
                        {
                            #region Params
                            int Amnt = 0;
                            if (!RoleplayManager.ParamsMet(Params, 1))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :buybullets <amount>");
                                return true;
                            }
                            else
                            {
                                Amnt = Convert.ToInt32(Params[1]);
                            }
                            #endregion

                            #region Conditions

                            double dub = Amnt / 2 + Amnt / 5;
                            double Conv = Math.Round(dub, 0);
                            int Pay = Convert.ToInt32(Conv);

                            if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("AMMUNATION"))
                            {
                                Session.SendWhisper("Você precisa estar na loja de armas [RoomID: 19] para comprar balas!");
                                return true;
                            }
                            if (Amnt < 50)
                            {
                                Session.SendWhisper("You cannot buy less than 50 bullets at a time!");
                                return true;
                            }
                            if (Session.GetHabbo().Credits < Pay)
                            {
                                Session.SendWhisper("You need at least $" + Pay + " for " + Amnt + " bullets!");
                                return true;
                            }
                            #endregion

                            #region Execute
                            RoleplayManager.Shout(Session, "*Compra " + Amnt + " balas por $" + Pay + " [-$" + Pay + "]*");
                            Session.GetRoleplay().Bullets += Amnt;
                            Session.GetRoleplay().SaveQuickStat("bullets", "" + Session.GetRoleplay().Bullets);
                            RoleplayManager.GiveMoney(Session, -Pay);
                            #endregion
                        }
                        catch (Exception e)
                        {

                        }

                        return true;
                    }
                #endregion
                #region :buybombs
                case "buybombs":
                case "buybomb":
                    {

                        #region Params
                        int Amnt = 0;
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :buybombs <amount>");
                            return true;
                        }
                        else
                        {
                            Amnt = Convert.ToInt32(Params[1]);
                        }
                        #endregion

                        #region Conditions

                        double dub = Amnt / 2 + Amnt / 5;
                        double Conv = Math.Round(dub, 0);
                        int Pay = Convert.ToInt32(Conv) * 5;

                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("AMMUNATION"))
                        {
                            Session.SendWhisper("You must be in the Ammunation [RoomID 19] to buy bombs!");
                            return true;
                        }
                        if (Amnt < 25)
                        {
                            Session.SendWhisper("You cannot buy less than 25 bombs at a time!");
                            return true;
                        }
                        if (Session.GetHabbo().Credits < Pay)
                        {
                            Session.SendWhisper("You need at least $" + Pay + " for " + Amnt + " bombs!!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Purchases " + Amnt + " bombs for $" + Pay + " [-$" + Pay + "]*");
                        Session.GetRoleplay().Bombs += Amnt;
                        Session.GetRoleplay().SaveQuickStat("bombs", "" + Session.GetRoleplay().Bombs);
                        RoleplayManager.GiveMoney(Session, -Pay);
                        #endregion

                        return true;
                    }
                #endregion
                #region :buyarmor
                case "buyarmor":
                case "buyvest":
                case "buyvests":
                    {

                        #region Params
                        int Amnt = 0;
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :buyvest <amount>");
                            return true;
                        }
                        else
                        {
                            Amnt = Convert.ToInt32(Params[1]);
                        }
                        #endregion

                        #region Conditions
                        int Pay = 0;
                        if (Session.GetHabbo().Rank >= 2)
                        {
                            Pay = Amnt * 500;
                        }
                        if (Session.GetHabbo().Rank <= 1)
                        {
                            Pay = Amnt * 1000;
                        }

                        if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("AMMUNATION"))
                        {
                            Session.SendWhisper("You must be in the Ammunation [RoomID 19] to buy body-armor!");
                            return true;
                        }
                        if (Session.GetHabbo().Credits < Pay && Amnt > 1)
                        {
                            Session.SendWhisper("You need at least $" + Pay + " for " + Amnt + " vests!!");
                            return true;
                        }
                        if (Session.GetHabbo().Credits < Pay && Amnt == 1)
                        {
                            Session.SendWhisper("You need at least $" + Pay + " for one vests!!");
                            return true;
                        }
                        if (Amnt < 1)
                        {
                            Session.SendWhisper("Are you drunk?");
                            return true;
                        }
                        #endregion

                        #region Execute
                        if (Amnt == 1)
                        {
                            RoleplayManager.Shout(Session, "*Purchases 1 body-armor vest for $" + Pay + " [-$" + Pay + "]*");
                            Session.GetRoleplay().Vests += Amnt;
                            Session.GetRoleplay().SaveQuickStat("vests", "" + Session.GetRoleplay().Vests);
                            RoleplayManager.GiveMoney(Session, -Pay);
                            return true;
                        }
                        RoleplayManager.Shout(Session, "*Purchases " + Amnt + " body-armor vests for $" + Pay + " [-$" + Pay + "]*");
                        Session.GetRoleplay().Vests += Amnt;
                        Session.GetRoleplay().SaveQuickStat("vests", "" + Session.GetRoleplay().Vests);
                        RoleplayManager.GiveMoney(Session, -Pay);
                        return true;
                        #endregion
                    }
                #endregion
                #region :reload

                case "reload":
                case "reloadgun":
                    {

                        #region Conditions

                        if (Session.GetRoleplay().Equiped == null)
                        {
                            Session.SendWhisper("You do not have a gun equipped to reload!");
                            return true;
                        }
                        if (Session.GetRoleplay().GunShots <= 0)
                        {
                            Session.SendWhisper("Your equipped weapon doesn't need reloading!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this while dead!");
                            return true;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this while jailed!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("reload_gun"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("reload_gun", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["reload_gun"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["reload_gun"] + "/10]");
                            return true;
                        }

                        #endregion

                        #region Execute

                        Session.GetRoleplay().GunShots = 0;
                        RoleplayManager.Shout(Session, "*Reloads my " + WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].DisplayName + ", clipping in (" + WeaponManager.WeaponsData[Session.GetRoleplay().Equiped].Reload_Time + ") more bullets*", 3);

                        Session.GetHabbo().FloodTime = Plus.GetUnixTimeStamp() + 5;
                        ServerMessage Packet = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                        Session.GetHabbo().GetRoomUser().IsGunReloaded = true;
                        Session.GetHabbo().GetRoomUser().ReloadExpiryTime = Plus.GetUnixTimeStamp() + 5;
                        Packet.AppendInteger(5);
                        Session.SendMessage(Packet);

                        Session.GetRoleplay().MultiCoolDown["reload_gun"] = 10;
                        Session.GetRoleplay().CheckingMultiCooldown = true;


                        #endregion


                        return true;
                    }

                #endregion

                #region :slap x
                case "slap":
                    {
                        #region Generate Instances / Sessions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :slap <user>");
                            return true;
                        }
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                            return true;
                        }
                        #endregion

                        #region Conditions
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this awhile dead!");
                            return false;
                        }
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this awhile jailed!");
                            return false;
                        }
                        if (Session.GetRoleplay().Energy < 1)
                        {
                            Session.SendWhisper("You do not have enough energy to do this!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("slap_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("slap_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["slap_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["slap_cooldown"] + "/3]");
                            return true;
                        }
                        if (RoleplayManager.UserDistance(Session, TargetSession) >= 2)
                        {
                            Session.SendWhisper("You must get closer to do this!");
                            return true;
                        }
                            if (TargetSession.GetHabbo().UserName == Session.GetHabbo().UserName)
                        {
                            Session.SendWhisper("You cant slap yourself.");
                            return true;
                        }
                        #endregion

                        #region Execute

                        Session.GetRoleplay().Energy -= 1;
                        RoleplayManager.Shout(Session, "*Slaps " + TargetSession.GetHabbo().UserName + " across the face [-1E]*", 3);
                        Session.GetRoleplay().MultiCoolDown["slap_cooldown"] = 3;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        return true;

                        #endregion

                    }
                #endregion
                #region :slapass x
                case "slapass":
                    {

                        #region Generate Instances / Sessions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :slapass <user>");
                            return true;
                        }
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                            return true;
                        }
                        #endregion

                        #region Conditions
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this awhile jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this awhile dead!");
                            return true;
                        }
                        if (RoleplayManager.UserDistance(Session, TargetSession) > 3)
                        {
                            Session.SendWhisper("Você deve estar mais perto para fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("slapass_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("slapass_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["slapass_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["slapass_cooldown"] + "/3]");
                            return true;
                        }
                        if (RoleplayManager.UserDistance(Session, TargetSession) >= 2)
                        {
                            Session.SendWhisper("You must get closer to do this!");
                            return true;
                        }
                        if (Session.GetRoleplay().Energy < 2)
                        {
                            Session.SendWhisper("You do not have enough energy to do this!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        Session.GetRoleplay().Energy -= 2;
                        TargetSession.GetRoleplay().EffectSeconds = 7;
                        RoleplayManager.Shout(Session, "*Slaps " + TargetSession.GetHabbo().UserName + " ass [-2E]*", 3);
                        RoleplayManager.Shout(TargetSession, "*Squirms from " + Session.GetHabbo().UserName + "'s touch*", 3);
                        TargetSession.GetHabbo().GetRoomUser().ApplyEffect(4);
                        Session.GetRoleplay().MultiCoolDown["slapass_cooldown"] = 3;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion
                #region :kiss x
                case "kiss":
                case "kissu":
                    {

                        #region Generate Instances / Sessions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :kiss <user>");
                            return true;
                        }
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        /*
                        if (Session.GetRoleplay().Jailed || Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this while jailed or dead!");
                            return true;
                        }*/

                        if (RoleplayManager.UserDistance(Session, TargetSession) > 2)
                        {
                            Session.SendWhisper("Você deve estar mais perto para fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("kiss_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("kiss_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["kiss_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["kiss_cooldown"] + "/3]");
                            return true;
                        }
                        if (RoleplayManager.UserDistance(Session, TargetSession) >= 2)
                        {
                            Session.SendWhisper("You must get closer to do this!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Leans towards " + TargetSession.GetHabbo().UserName + " and gives them a quick little kiss on their lips.", 12);
                        RoleplayManager.Shout(TargetSession, "*Blushes in embarrassment as " + Session.GetHabbo().UserName + " leans away from them.*", 16);
                        TargetSession.GetRoleplay().EffectSeconds = 7;
                        Session.GetRoleplay().EffectSeconds = 7;
                        Session.GetHabbo().GetRoomUser().ApplyEffect(9);
                        TargetSession.GetHabbo().GetRoomUser().ApplyEffect(9);
                        Session.GetRoleplay().MultiCoolDown["kiss_cooldown"] = 3;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion
                #region :sex x
                case "sex":
                    {
                        #region Generate Instances / Sessions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :sex <user>");
                            return true;
                        }
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                            return true;
                        }
                        #endregion

                        #region Conditions
                        if (Session.GetRoleplay().Jailed)
                        {
                            Session.SendWhisper("You cannot do this awhile jailed!");
                            return true;
                        }
                        if (Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this awhile dead!");
                            return true;
                        }
                        if (Session.GetRoleplay().Energy < 5)
                        {
                            Session.SendWhisper("You do not have enough energy to do this!");
                            return false;
                        }
                        if (Session.GetRoleplay().Married_To != TargetSession.GetHabbo().Id)
                        {
                            Session.SendWhisper("You can only have sex with your spouse!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("sex_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("sex_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["sex_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["sex_cooldown"] + "/3]");
                            return true;
                        }
                        if (RoleplayManager.UserDistance(Session, TargetSession) >= 2)
                        {
                            Session.SendWhisper("You must get closer to do this!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        TargetSession.GetRoleplay().EffectSeconds = 7;
                        Session.GetRoleplay().Energy -= 5;
                        if (Session.GetHabbo().Gender.ToLower().StartsWith("m") && TargetSession.GetHabbo().Gender.ToLower().StartsWith("f"))
                        {
                            RoleplayManager.Shout(Session, "*Grabs " + TargetSession.GetHabbo().UserName + " by the chest and tackles her down, taking her clothes off. [-5E]*", 16);
                            RoleplayManager.Shout(TargetSession, "*Moans and quivers as " + Session.GetHabbo().UserName + " thrusts his dick inside her.*", 16);
                        }
                        else if (Session.GetHabbo().Gender.ToLower().StartsWith("f") && TargetSession.GetHabbo().Gender.ToLower().StartsWith("m"))
                        {
                            RoleplayManager.Shout(Session, "*Pushes " + TargetSession.GetHabbo().UserName + " down onto the floor, climbing ontop of him and sliding his clothes off. [-5E]*", 16);
                            RoleplayManager.Shout(TargetSession, "*Groans from the pleasure as " + Session.GetHabbo().UserName + " bounces up and down his dick.*", 16);
                        }
                        else if (Session.GetHabbo().Gender.ToLower().StartsWith("m") && TargetSession.GetHabbo().Gender.ToLower().StartsWith("m"))
                        {
                            RoleplayManager.Shout(Session, "*Grabs " + TargetSession.GetHabbo().UserName + " by the chest and tackles him down, taking his clothes off. [-5E]*", 16);
                            RoleplayManager.Shout(TargetSession, "*Groans from the pleasure as " + Session.GetHabbo().UserName + " bounces up and down his dick.*", 16);
                        }
                        else if (Session.GetHabbo().Gender.ToLower().StartsWith("f") && TargetSession.GetHabbo().Gender.ToLower().StartsWith("f"))
                        {
                            RoleplayManager.Shout(Session, "*Pushes " + TargetSession.GetHabbo().UserName + " down onto the floor, climbing ontop of her and sliding her clothes off. [-5E]*", 16);
                            RoleplayManager.Shout(TargetSession, "*Moans and quivers as " + Session.GetHabbo().UserName + "'s fingers slide inside her.*", 16);
                        }
                        else
                        {
                            RoleplayManager.Shout(Session, "*Makes sweet love to " + TargetSession.GetHabbo().UserName + ".*");
                            RoleplayManager.Shout(TargetSession, "*Moans from " + Session.GetHabbo().UserName + "'s movements*");
                        }
                        TargetSession.GetHabbo().GetRoomUser().ApplyEffect(4);
                        Session.GetRoleplay().MultiCoolDown["sex_cooldown"] = 3;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion
                #region :hug x
                case "hug":
                    {

                        #region Generate Instances / Sessions
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :hug <user>");
                            return true;
                        }
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                            return true;
                        }
                        #endregion

                        #region Conditions
                        /*
                        if (Session.GetRoleplay().Jailed || Session.GetRoleplay().Dead)
                        {
                            Session.SendWhisper("You cannot do this while jailed or dead!");
                            return true;
                        }*/

                        if (RoleplayManager.UserDistance(Session, TargetSession) > 2)
                        {
                            Session.SendWhisper("Você deve estar mais perto para fazer isso!");
                            return true;
                        }
                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("hug_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("hug_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["hug_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["hug_cooldown"] + "/3]");
                            return true;
                        }
                        if (RoleplayManager.UserDistance(Session, TargetSession) >= 2)
                        {
                            Session.SendWhisper("You must get closer to do this!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Session, "*Hugs " + TargetSession.GetHabbo().UserName + "*");
                        TargetSession.GetRoleplay().EffectSeconds = 7;
                        Session.GetRoleplay().EffectSeconds = 7;
                        Session.GetHabbo().GetRoomUser().ApplyEffect(9);
                        TargetSession.GetHabbo().GetRoomUser().ApplyEffect(9);
                        Session.GetRoleplay().MultiCoolDown["hug_cooldown"] = Convert.ToInt32(RoleplayData.Data["hug_cooldown"]);
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        #endregion

                        return true;
                    }
                #endregion

                #endregion

                #region Bounties

                #region :blist

                case "blist":
                case "bountylist":
                    {

                        string BountyList = "=======================\nBounty List\n=======================\n\n";

                        if (Bounties.BountyUsers.Count > 0)
                        {
                            foreach (string Username in Bounties.BountyUsers.Keys)
                            {
                                BountyList += "[-] " + Username + " - Amount: $" + Bounties.BountyUsers[Username] + "\n";
                            }
                        }
                        else
                        {
                            BountyList += "The bounty list appears to be empty at the moment!";
                        }

                        Session.SendNotifWithScroll(BountyList);

                        return true;
                    }

                #endregion
                #region :sbounty <user> <amount>

                case "sbounty":
                case "setbounty":
                case "hitlist":
                    {

                        #region Params
                        string Username = "";
                        GameClient Target = null;
                        int Amount = 100;
                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :sbounty <user> <amount>");
                            return true;
                        }
                        else
                        {
                            Username = Params[1].ToString();
                            Amount = Convert.ToInt32(Params[2]);
                            Target = RoleplayManager.GenerateSession(Username);
                        }
                        if (Target == null)
                        {
                            Session.SendWhisper("The user " + Username + " was not found!");
                            return true;
                        }
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_vip"))
                        {
                            Session.SendWhisper("You must be VIP to use this command.");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :setbounty <username> <amount>");
                            return true;
                        }
                        if (!Plus.IsNum(Convert.ToString(Amount)))
                        {
                            Session.SendWhisper("Invalid amount.");
                            return true;
                        }
                        if (Amount < 100)
                        {
                            Session.SendWhisper("The minimum bounty amount is $100!");
                            return true;
                        }
                        if (Session.GetHabbo().Credits < Amount)
                        {
                            Session.SendWhisper("You do not have $" + Amount + " for the bounty.");
                            return true;
                        }
                        if (Bounties.BountyUsers.ContainsKey(Target.GetHabbo().UserName))
                        {
                            Session.SendWhisper("This user already have a bounty.");
                            return true;
                        }
                        #endregion

                        #region Execute

                        Bounties.SetBounty(Target.GetHabbo().UserName, Amount);
                        Session.Shout("*Sets a bounty of $" + Amount + " on " + Target.GetHabbo().UserName + "*");
                        Roleplay.Misc.RoleplayManager.GiveMoney(Session, -Amount);
                        string Notice = Session.GetHabbo().UserName + " has just set a bounty on " + Target.GetHabbo().UserName + " of: $" + Amount + "!";

                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient mClient in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (mClient == null)
                                    continue;
                                if (mClient.GetHabbo() == null)
                                    continue;
                                if (mClient.GetHabbo().CurrentRoom == null)
                                    continue;
                                if (mClient.GetConnection() == null)
                                    continue;
                                mClient.GetHabbo().GetRoomUser().LastBubble = 33;
                                mClient.SendWhisper("[HITLIST]: " + Notice);
                                mClient.GetHabbo().GetRoomUser().LastBubble = 0;
                            }
                        }
                        #endregion

                        return true;
                    }

                #endregion

                #endregion

                #region Special Items

                #region :sleepingbag
                case "sleepingbag":
                    {
                        if (Session == null)
                            return true;

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("bag_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("bag_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["bag_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["bag_cooldown"] + "/20]");
                            return true;
                        }

                        UserItem BagInv = Session.GetHabbo().GetInventoryComponent().GetBag();

                        if (BagInv != null)
                        {
                            RoomUser Me = Session.GetHabbo().GetRoomUser();

                            if (Me == null)
                                return true;

                            if (Session.GetRoleplay().Bag == null)
                            {
                                if (Me.IsWalking)
                                {
                                    Session.SendWhisper("slow down cowboy");
                                    Me.IsWalking = false;
                                    Me.ClearMovement();
                                }

                                Session.GetRoleplay().Bag = RoleplayManager.PlaceItemToRoomReturn(BagInv.BaseItemId, Me.X, Me.Y, Me.Z, 0, false, Session.GetHabbo().CurrentRoomId, false);
                                Me.AddStatus("lay", TextHandling.GetString(Session.GetRoleplay().Bag.GetBaseItem().Height));
                                Me.Z = Session.GetRoleplay().Bag.Z;
                                Me.RotBody = Session.GetRoleplay().Bag.Rot;

                                Me.CanWalk = false;

                                Session.SendWhisper("You will gain a random amount of energy every 20 seconds whilst in your sleeping bag!");

                                TimerCallback TCB = new TimerCallback(Session.GetRoleplay().BagTimerDone);

                                Session.GetRoleplay().BagTimer = new Timer(TCB, null, 20000, Timeout.Infinite);
                                Session.Shout("*Pulls out their sleeping bag*");

                            }
                            else
                            {
                                RoleplayManager.PickRock(Session.GetRoleplay().Bag, Session.GetRoleplay().Bag.RoomId);
                                Me.RemoveStatus("lay");
                                Session.GetRoleplay().Bag = null;
                                Session.Shout("*Puts back their sleeping bag*");
                                Session.GetRoleplay().MultiCoolDown["bag_cooldown"] = 20;
                                Session.GetRoleplay().CheckingMultiCooldown = true;

                                Me.CanWalk = true;
                            }

                            Me.UpdateNeeded = true;

                        }

                        return true;
                    }
                #endregion

                #endregion

                #endregion

                #region Non Roleplay Commands

                #region User Commands

                #region About (:about)
                case "about":
                case "info":
                case "sobre":
                    {
                        StringBuilder About = new StringBuilder();
                        TimeSpan Uptime = DateTime.Now - Plus.ServerStarted;
                        int UsersOnline = Plus.GetGame().GetClientManager().ClientCount;
                        int RoomsLoaded = Plus.GetGame().GetRoomManager().LoadedRoomsCount;
                        int Peak = LowPriorityWorker._userPeak;
                        TimeSpan ts = TimeSpan.FromMilliseconds(Environment.TickCount);

                        Session.SendNotif(
                 "<b>Creditos</b>:\n" +
                 "Mereos (Desenvolvedor)\n\n" +
                 "<b>Informações atuais sobre tempo de execução</b>:\n" +
                 "Online Users: " + UsersOnline + "\n" +
                 "Salas carregadas: " + RoomsLoaded + "\n" +
                 "Tempo de atividade: " + Uptime.Days + " dia(s), " + Uptime.Hours + " horas e " + Uptime.Minutes + " minutos.\n\n" +
                 "<b>SWF Revision</b>:\n" +
                  "PRODUCTION-201510261212-858675875", "CityRP Informação", "");
                        return true;
                    }
                #endregion
                #region Removegod (:removegod)
                case "removegod":
                case "removeprotecao":
                    {
                        if (!Session.GetRoleplay().NoobWarned)
                        {
                            Session.SendNotif("Se você optar por fazer isso novamente, sua Proteção Divina temporária será desativada!");
                            Session.GetRoleplay().NoobWarned = true;
                            return false;
                        }
                        else
                        {
                            Session.GetRoleplay().RemoveGodProtection();
                        }
                        return true;
                    }
                #endregion
                #region Sit (:sit)
                case "sit":
                case "sentar":
                    {
                        if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOSIT"))
                        {
                            Session.SendWhisper("You are not allowed to sit in this room!");
                            return true;
                        }
                        RoomUser user = null;
                        Room room = Session.GetHabbo().CurrentRoom;

                        if (Params.Length == 2 && Session.GetHabbo().HasFuse("fuse_admin"))
                        {
                            GameClient Client = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (Client != null)
                                user = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                            if (user == null)
                                user = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                        }
                        else
                        {
                            user = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                        }

                        //Session.GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(0);



                        if (user.Statusses.ContainsKey("lie") || user.IsLyingDown || user.RidingHorse || user.IsWalking)
                        {
                            return true;
                        }

                        if (!user.Statusses.ContainsKey("sit"))
                        {
                            if ((user.RotBody % 2) == 0)
                            {
                                if (user == null)
                                    return true;

                                try
                                {
                                    user.Statusses.Add("sit", "1.0");
                                    user.Z -= 0.35;
                                    user.IsSitting = true;
                                    user.UpdateNeeded = true;
                                }
                                catch { }
                            }
                            else
                            {
                                user.RotBody--;
                                user.Statusses.Add("sit", "1.0");
                                user.Z -= 0.35;
                                user.IsSitting = true;
                                user.UpdateNeeded = true;
                            }
                        }
                        else if (user.IsSitting == true)
                        {
                            user.Z += 0.35;
                            user.Statusses.Remove("sit");
                            user.Statusses.Remove("1.0");
                            user.IsSitting = false;
                            user.UpdateNeeded = true;
                        }
                        return true;
                    }
                #endregion

                #region Pickall (:pickall)
                case "pickall":
                    {
                        if (RoleplayManager.BypassRights(Session) || Session.GetHabbo().CurrentRoom.RoomData.Owner == Session.GetHabbo().UserName)
                        {
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;

                            if (TargetRoom != null && TargetRoom.CheckRights(Session, true))
                            {

                                List<RoomItem> RemovedItems = TargetRoom.GetRoomItemHandler().RemoveAllFurniture(Session);

                                if (Session.GetHabbo().GetInventoryComponent() != null)
                                {
                                    Session.GetHabbo().GetInventoryComponent().AddItemArray(RemovedItems);
                                    Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
                                }
                            }
                            else
                            {
                                Session.SendNotif("An error occured.\n\nPlease contact your system administrator.");
                            }
                        }
                        return true;
                    }
                #endregion

                #region Update Bots (:ubots)
                case "ubots":
                case "initbots":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        Session.GetHabbo().CurrentRoom.InitUserBots();
                        Session.SendWhisper("Bots are updated!");

                        return true;
                    }
                #endregion

                #region Unload (:unload)
                case "unload":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_builder"))
                        {
                            return false;
                        }
                        var roomId = Session.GetHabbo().CurrentRoom.RoomId;
                        var users = new List<RoomUser>(Session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values);

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            Session.GetHabbo().CurrentRoom.GetRoomItemHandler().SaveFurniture(dbClient);

                        Plus.GetGame().GetRoomManager().UnloadRoom(Session.GetHabbo().CurrentRoom, "Unload command");

                        Plus.GetGame().GetRoomManager().LoadRoom(roomId);

                        var roomFwd = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
                        roomFwd.AppendInteger(roomId);

                        var data = roomFwd.GetReversedBytes();

                        foreach (var user in users.Where(user => user != null && user.GetClient() != null))
                        {
                            user.GetClient().SendMessage(data);
                            user.GetClient().SendNotifWithScroll("The room you were currently in was unloaded!\n\nYou should be transported back to the room you were just in.");
                        }
                        return true;
                    }
                #endregion

                #region User Info (:userinfo)
                case "userinfo":
                case "ui":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            string username = null;
                            bool UserOnline = true;

                            if (!RoleplayManager.ParamsMet(Params, 1))
                            {
                                Session.SendNotif("Sintaxe de comando inválida: :userinfo <user>");
                                return true;
                            }


                            username = Convert.ToString(Params[1]);

                            GameClient tTargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(username);

                            if (!RoleplayManager.CanInteract(Session, tTargetClient, false))
                            {
                                DataRow Row;
                                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                                {
                                    dbClient.SetQuery("SELECT username, rank, online, id, motto, credits FROM users WHERE username=@user LIMIT 1");
                                    dbClient.AddParameter("user", username);
                                    Row = dbClient.GetRow();

                                    int Rank = Convert.ToInt32(Row[1]) == 12 ? 1 : Convert.ToInt32(Row[1]);

                                    Session.SendNotif("User Info for " + username + ":\r" +
                                "Rank: " + Rank + " \r" +
                                "User Id: " + Row[3].ToString() + " \r" +
                                "Motto: " + Row[4].ToString() + " \r" +
                                "Credits: " + Row[5].ToString() + " \r");
                                }
                                return true;
                            }
                            Habbo User = tTargetClient.GetHabbo();

                            //Habbo User = Plus.GetGame().GetClientManager().GetClientByUserId(username).GetHabbo();
                            StringBuilder RoomInformation = new StringBuilder();

                            if (User.CurrentRoom != null)
                            {
                                RoomInformation.Append(" - ROOM INFORMAtiON [" + User.CurrentRoom.RoomId + "] - \r");
                                RoomInformation.Append("Owner: " + User.CurrentRoom.RoomData.Owner + "\r");
                                RoomInformation.Append("Room Name: " + User.CurrentRoom.RoomData.Name + "\r");
                                RoomInformation.Append("Current Users: " + User.CurrentRoom.UserCount + "/" + User.CurrentRoom.RoomData.UsersMax);
                            }
                            uint Rankk = User.Rank == 12 ? 1 : User.Rank;
                            Session.SendNotif("User info for: " + username + ":\r" +
                                "Rank: " + Rankk.ToString() + " \r" +
                                "Online: " + UserOnline.ToString() + " \r" +
                                "User Id: " + User.Id + " \r" +
                                "Current Room: " + User.CurrentRoomId + " \r" +
                                "Motto: " + User.Motto + " \r" +
                                "Credits: " + User.Credits + " \r" +
                                "Muted: " + User.Muted.ToString() + "\r" +
                                "\r\r" + RoomInformation.ToString());

                        }
                        return true;
                    }
                #endregion

                #region Disable Diagonal (:disablediagonal)
                case "disablediagonal":
                case "disablediag":
                case "togglediagonal":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            TargetRoom = Session.GetHabbo().CurrentRoom;

                            if (TargetRoom != null && TargetRoom.CheckRights(Session, true))
                            {
                                if (TargetRoom.GetGameMap().DiagonalEnabled)
                                    TargetRoom.GetGameMap().DiagonalEnabled = false;
                                else
                                    TargetRoom.GetGameMap().DiagonalEnabled = true;

                            }
                        }
                        return true;
                    }
                #endregion

                #region Freeze (:freeze)
                case "freeze":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_builder"))
                        {
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            RoomUser Target = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
                            if (Target != null)
                            {
                                Target.ClearMovement();
                                Target.CanWalk = false;
                                Target.Frozen = true;
                                Target.ApplyEffect(12);
                                RoleplayManager.Shout(Session, "*Uses their god-like powers to freeze " + Target.GetUserName() + "*");
                            }
                        }
                        return true;
                    }
                #endregion

                #region Unfreeze (:unfreeze)
                case "unfreeze":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_builder"))
                        {
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            RoomUser Target = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
                            if (Target != null)
                            {
                                Target.CanWalk = true;
                                Target.Frozen = false;
                                Target.ApplyEffect(0);
                                RoleplayManager.Shout(Session, "*Uses their god-like powers to un-freeze " + Target.GetUserName() + "*");
                            }
                        }
                        return true;
                    }
                #endregion

                #region SaveFurni (:savefurni)
                case "savefurni":
                case "save":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            Session.GetHabbo().CurrentRoom.GetRoomItemHandler().SaveFurniture(dbClient);
                        Session.SendWhisper("All room furni has been saved! Be sure to say :unload before taking any changes!");
                        return true;
                    }
                #endregion

                #region Set Speed (:setspeed)
                case "setspeed":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        Room TargetRoom = Session.GetHabbo().CurrentRoom;
                        TargetRoom = Session.GetHabbo().CurrentRoom;
                        if (TargetRoom != null && TargetRoom.CheckRights(Session, true))
                        {
                            uint speed;
                            if (uint.TryParse(Params[1], out speed)) Session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetSpeed(speed);
                            else Session.SendWhisper(Plus.GetLanguage().GetVar("command_setspeed_error_numbers"));
                        }

                        return true;
                    }
                #endregion

                #region Regenerate maps (:regenmaps)
                case "regenmaps":
                case "reloadmaps":
                case "fixroom":
                    {
                        #region Conditions

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("fixroom_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("fixroom_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["fixroom_cooldown"] > 0)
                        {
                            Session.SendWhisper("Fixroom cooldown [" + Session.GetRoleplay().MultiCoolDown["fixroom_cooldown"] + "/60]");
                            return true;
                        }

                        #endregion

                        #region Execute

                        Session.GetHabbo().CurrentRoom.GetGameMap().GenerateMaps();
                        Session.GetHabbo().CurrentRoom.GetRoomUserManager().ToSet.Clear();
                        lock (Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                        {
                            foreach (RoomUser User in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            {
                                if (User == null)
                                    continue;

                                User.ClearMovement();
                            }
                        }
                        Session.SendWhisper("The room map has been refreshed!");
                        Session.GetRoleplay().MultiCoolDown["fixroom_cooldown"] = 60;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        return true;

                        #endregion
                    }
                #endregion

                #region Empty Pets (:emptypets)
                case "emptypets":
                case "removepets":
                    {
                        Session.SendWhisper("Command is not coded yet!!");
                        return true;
                    }
                #endregion

                #region Mute Bots (:mutebots)
                case "mutebots":
                case "mutepets":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            if (Session.GetHabbo().CurrentRoom.CheckRights(Session, true))
                            {
                                Room Room = Session.GetHabbo().CurrentRoom;
                                if (Room.MutedBots)
                                    Room.MutedBots = false;
                                else
                                    Room.MutedBots = true;

                                SendChatMessage(Session, "Muted bots have been toggled");
                            }
                        }
                        return true;
                    }
                #endregion

                #region Dance (:dance)
                case "dance":
                    {

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisperBubble("Sintaxe de comando inválida: :dance <id>");
                            return true;
                        }
                        ushort result;
                        ushort.TryParse(Params[1], out result);

                        if (result > 4)
                        {
                            Session.SendWhisper(Plus.GetLanguage().GetVar("command_dance_false"));
                            result = 0;
                        }
                        var message = new ServerMessage();
                        message.Init(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
                        message.AppendInteger(Session.CurrentRoomUserId);
                        message.AppendInteger(result);
                        Session.GetHabbo().CurrentRoom.SendMessage(message);

                        return true;
                    }
                #endregion

                #region Say All (:sayall)
                case "sayall":
                    if (Session.GetHabbo().Id == 1)
                    {
                        Room currentRoom2 = Session.GetHabbo().CurrentRoom;
                        if (currentRoom2 != null)
                        {
                            string Message3 = ChatCommandHandler.MergeParams(Params, 1);
                            if (Message3 != "")
                            {
                                lock (currentRoom2.GetRoomUserManager().GetRoomUsers())
                                {
                                    foreach (RoomUser roomUser2 in currentRoom2.GetRoomUserManager().GetRoomUsers())
                                        roomUser2.Chat(roomUser2.GetClient(), Message3, false, 0);
                                }
                            }
                        }
                        return true;
                    }
                    else
                        return true;

                #endregion

                #endregion

                #region VIP Commands
                #region Moonwalk (:moonwalk)
                case "moonwalk":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_vip"))
                        {
                            Room room = Session.GetHabbo().CurrentRoom;
                            if (room == null)
                                return true;

                            RoomUser roomuser = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                            roomuser.IsMoonwalking = !roomuser.IsMoonwalking;

                            if (roomuser.IsMoonwalking)
                                SendChatMessage(Session, "Moonwalk enabled!");
                            else
                                SendChatMessage(Session, "Moonwalk disabled!");
                        }
                        else
                        {
                            Session.SendWhisper("You must be staff to do this.");
                        }
                        return true;
                    }
                #endregion

                #region Push (:push)
                case "push":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_vip") && !Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOPUSH"))
                        {
                            Room TargetRoom;
                            RoomUser TargetRoomUser;
                            RoomUser TargetRoomUser1;
                            TargetRoom = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);

                            if (TargetRoom == null)
                            {
                                return true;
                            }

                            if (Params.Length == 1)
                            {
                                SendChatMessage(Session, "Sintaxe de comando inválida: :push <user>");
                                return true;
                            }

                            TargetRoomUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Convert.ToString(Params[1]));

                            if (TargetRoomUser == null)
                            {
                                SendChatMessage(Session, "Could not find that user!");
                                return true;
                            }

                            if (TargetRoomUser.GetUserName() == Session.GetHabbo().UserName)
                            {
                                SendChatMessage(Session, "Come on, surely you don't want to push yourself!");
                                return true;
                            }

                            if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("push_cooldown"))
                            {
                                Session.GetRoleplay().MultiCoolDown.Add("push_cooldown", 0);
                            }
                            if (Session.GetRoleplay().MultiCoolDown["push_cooldown"] > 0)
                            {
                                Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["push_cooldown"] + "/3]");
                                return true;
                            }

                            TargetRoomUser1 = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                            if (TargetRoomUser1 == null || TargetRoomUser.TeleportEnabled)
                            {
                                return true;
                            }

                            Session.GetRoleplay().MultiCoolDown["push_cooldown"] = 3;
                            Session.GetRoleplay().CheckingMultiCooldown = true;
                            //if ((TargetRoomUser.X == TargetRoomUser1.X - 1) || (TargetRoomUser.X == TargetRoomUser1.X + 1) || (TargetRoomUser.Y == TargetRoomUser1.Y - 1) || (TargetRoomUser.Y == TargetRoomUser1.Y + 1))
                            if (!((Math.Abs((int)(TargetRoomUser.X - TargetRoomUser1.X)) >= 2) || (Math.Abs((int)(TargetRoomUser.Y - TargetRoomUser1.Y)) >= 2)))
                            {
                                if (TargetRoomUser1.RotBody == 4)
                                { TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 1); }

                                if (TargetRoomUser1.RotBody == 0)
                                { TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 1); }

                                if (TargetRoomUser1.RotBody == 6)
                                { TargetRoomUser.MoveTo(TargetRoomUser.X - 1, TargetRoomUser.Y); }

                                if (TargetRoomUser1.RotBody == 2)
                                { TargetRoomUser.MoveTo(TargetRoomUser.X + 1, TargetRoomUser.Y); }

                                if (TargetRoomUser1.RotBody == 3)
                                {
                                    TargetRoomUser.MoveTo(TargetRoomUser.X + 1, TargetRoomUser.Y);
                                    TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 1);
                                }

                                if (TargetRoomUser1.RotBody == 1)
                                {
                                    TargetRoomUser.MoveTo(TargetRoomUser.X + 1, TargetRoomUser.Y);
                                    TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 1);
                                }

                                if (TargetRoomUser1.RotBody == 7)
                                {
                                    TargetRoomUser.MoveTo(TargetRoomUser.X - 1, TargetRoomUser.Y);
                                    TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 1);
                                }

                                if (TargetRoomUser1.RotBody == 5)
                                {
                                    TargetRoomUser.MoveTo(TargetRoomUser.X - 1, TargetRoomUser.Y);
                                    TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 1);
                                }

                                TargetRoomUser.UpdateNeeded = true;
                                TargetRoomUser1.UpdateNeeded = true;
                                TargetRoomUser1.SetRot(Rotation.Calculate(TargetRoomUser1.X, TargetRoomUser1.Y, TargetRoomUser.GoalX, TargetRoomUser.GoalY));
                                Session.Shout("*Pushes " + TargetRoomUser.GetClient().GetHabbo().UserName + " [-5E]*");
                                Session.GetRoleplay().Energy -= 5;
                            }
                            else
                            {
                                SendChatMessage(Session, Params[1] + " is not close enough.");
                            }
                        }
                        else if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOPUSH"))
                        {
                            Session.SendWhisper("You cannot push in this room.");
                            return true;
                        }
                        else
                            Session.SendWhisper("You must be VIP to do this.");
                        return true;
                    }
                #endregion

                #region Pull (:pull)
                case "pull":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_vip") && !Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOPULL"))
                        {
                            Room room = Session.GetHabbo().CurrentRoom;
                            if (room == null)
                            {
                                return true;
                            }


                            RoomUser roomuser = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                            if (roomuser == null)
                            {
                                return true;
                            }
                            if (Params.Length == 1)
                            {
                                SendChatMessage(Session, "Unable to find user!");
                                return true;
                            }

                            GameClient Target = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (Target == null)
                                return true;

                            if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("pull_cooldown"))
                            {
                                Session.GetRoleplay().MultiCoolDown.Add("pull_cooldown", 0);
                            }
                            if (Session.GetRoleplay().MultiCoolDown["pull_cooldown"] > 0)
                            {
                                Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["pull_cooldown"] + "/5]");
                                return true;
                            }


                            if (Target.GetHabbo().Id == Session.GetHabbo().Id)
                            {
                                SendChatMessage(Session, "You cannot pull yourself!");
                                return true;
                            }
                            RoomUser TargetUser = room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Id);
                            if (TargetUser == null)
                                return true;


                            if (TargetUser.TeleportEnabled)
                                return true;

                            Session.GetRoleplay().MultiCoolDown["pull_cooldown"] = 5;
                            Session.GetRoleplay().CheckingMultiCooldown = true;

                            if (!((Math.Abs((int)(roomuser.X - TargetUser.X)) >= 3) || (Math.Abs((int)(roomuser.Y - TargetUser.Y)) >= 3)))
                            {
                                Session.Shout("*pulls " + Params[1] + " [-5E]*");
                                Session.GetRoleplay().Energy -= 5;
                                if (roomuser.RotBody % 2 != 0)
                                    roomuser.RotBody--;

                                if (roomuser.RotBody == 0)
                                    TargetUser.MoveTo(roomuser.X, roomuser.Y - 1);
                                else if (roomuser.RotBody == 2)
                                    TargetUser.MoveTo(roomuser.X + 1, roomuser.Y);
                                else if (roomuser.RotBody == 4)
                                    TargetUser.MoveTo(roomuser.X, roomuser.Y + 1);
                                else if (roomuser.RotBody == 6)
                                    TargetUser.MoveTo(roomuser.X - 1, roomuser.Y);

                            }
                            else
                            {
                                SendChatMessage(Session, "This user is too far away!");
                                return true;
                            }
                        }
                        else if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOPULL"))
                        {
                            Session.SendWhisper("You cannot pull in this room.");
                            return true;
                        }
                        else
                            Session.SendWhisper("You must be VIP to do this.");
                        return true;
                    }
                #endregion

                #region Enable (:enable)
                case "enable":
                case "effect":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            if (Params.Length == 1)
                            {
                                SendChatMessage(Session, "You must enter an effect ID");
                                return true;
                            }
                            RoomUser TargetRoomUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().UserName);
                            if (TargetRoomUser.RidingHorse)
                            {
                                SendChatMessage(Session, "You cannot enable effects whilst riding a horse!");
                                return true;
                            }
                            else if (TargetRoomUser.IsLyingDown)
                                return true;

                            string Effect = Params[1];
                            double EffectID;
                            bool isNum = double.TryParse(Effect, out EffectID);
                            if (isNum)
                            {
                                //if(EffectID != 97)
                                Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(int.Parse(EffectID.ToString()));
                            }
                            else
                            {
                                SendChatMessage(Session, "Try using a number for the effects!");
                                return true;
                            }
                        }
                        else
                        {
                            Session.SendWhisper("Access denied.");
                        }
                        return true;
                    }
                #endregion

                #region Empty (:empty)
                case "emptyitems":
                case "empty":
                    {
                        if (Params.Length == 1)
                        {
                            Session.SendNotif("Are you sure you want to clear your inventory? You will lose all the furniture!\n" +
                             "To confirm, type \":emptyitems yes\". \n\nOnce you do this, there is no going back!\n(If you do not want to empty it, just ignore this message!)\n\n" +
                             "PLEASE NOTE! If you have more than 2800 items, the hidden items will also be DELETED.");
                            return true;
                        }
                        else
                        {
                            if (Params.Length == 2)
                            {
                                if (Params[1].ToString() == "yes")
                                {
                                    //GameClient Client = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                                    Session.GetHabbo().GetInventoryComponent().ClearItems();
                                    Session.SendNotif("Your inventory has been cleared!");
                                    return true;
                                }
                            }
                        }
                        return true;
                    }
                #endregion

                #region Flag Me (:flagme)
                case "flagme":
                    {
                        #region Params

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :flagme <new name>");
                            return true;
                        }

                        string NewName = Convert.ToString(Params[1]);

                        #endregion

                        #region Conditions

                        if (!Session.GetHabbo().VIP)
                        {
                            return true;
                        }
                        if (Session.GetHabbo().BelCredits < 1)
                        {
                            Session.SendWhisper("You do not have 1 VIP token to change your name!");
                            return true;
                        }

                        if (Params.Length <= 1)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :flagme <newname>");
                            return true;
                        }

                        Regex r = new Regex("^[a-zA-Z]*$");
                        if (!r.IsMatch(NewName))
                        {
                            Session.SendWhisper("Invalid characters used!");
                            return true;
                        }
                        if (NewName.Length > 50 || NewName.Length < 3)
                        {
                            Session.SendWhisper("Your name must have more than 2 characters and less than 50!");
                            return true;
                        }
                        if (!NewName.Contains("-") || Regex.Matches(NewName, "-").Count != 1)
                        {
                            Session.SendWhisper("You must do First-Last format");
                            return true;
                        }
                        int indexOf = NewName.IndexOf("-");
                        String firstName = NewName.Substring(0, indexOf);
                        String lastName = NewName.Substring(indexOf + 1);
                        if (firstName.Length < 3 || lastName.Length < 3)
                        {
                            Session.SendWhisper("Invalid First or Last Name length");
                            return true;
                        }
                        #endregion

                        #region Execute
                        DataRow Row;
                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = '" + NewName + "'");
                            Row = dbClient.GetRow();
                        }
                        if (Row != null)
                        {
                            Session.SendWhisper("This name '" + NewName + "' you have chosen is taken!");
                            return true;
                        }
                        else
                        {
                            RoleplayManager.Shout(Session, "*Flags their character's name to: " + NewName + "*");
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                Session.GetHabbo().BelCredits -= 1;
                                Session.GetHabbo().UpdateActivityPointsBalance();
                                dbClient.RunFastQuery("UPDATE `users` SET `username` = '" + NewName + "' WHERE `id` = '" + Session.GetHabbo().Id + "'");
                                // Change owner's rooms (in-case they own an apartment)
                                dbClient.SetQuery("UPDATE `rooms_data` SET `owner` = @newowner WHERE `owner` = @oldowner");
                                dbClient.AddParameter("newowner", NewName);
                                dbClient.AddParameter("oldowner", Session.GetHabbo().UserName);
                                dbClient.RunQuery();

                                Session.SendWhisper("You have successfully changed your name! Reload for it to be displayed.");

                            }
                            return true;
                        }
                        #endregion

                    }

                #endregion

                #endregion

                #region Moderation Commands

                #region Room Mute (:roommute) / Room unmute (:roomunmute)
                case "roommute":
                    {
                        if ((Session.GetHabbo().HasFuse("fuse_events") && !Session.GetHabbo().HasFuse("fuse_builder") && Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("EVENTS")) || Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            var room = Session.GetHabbo().CurrentRoom;
                            if (room.RoomMuted)
                            {
                                Session.SendWhisper("Room is already muted.");
                                return true;
                            }

                            string Msg = MergeParams(Params, 1);

                            room.RoomMuted = true;

                            lock (Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            {
                                foreach (RoomUser client in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                                {

                                    if (client == null)
                                        continue;
                                    if (client.GetClient() == null)
                                        continue;
                                    if (client.GetClient().GetHabbo() == null)
                                        continue;
                                    if (client.GetClient().GetHabbo().CurrentRoom == null)
                                        continue;
                                    if (client.GetClient().GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                                        continue;

                                    client.GetClient().SendWhisperBubble("The room has been muted for the following reason: " + Msg, 34);
                                }
                            }

                            Plus.GetGame()
                                .GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, string.Empty,
                                    "Room Mute", "Room muted");
                            return true;
                        }
                        return true;
                    }
                case "roomunmute":
                    {
                        if ((Session.GetHabbo().HasFuse("fuse_events") && !Session.GetHabbo().HasFuse("fuse_builder") && Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("EVENTS")) || Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            var room = Session.GetHabbo().CurrentRoom;
                            if (!Session.GetHabbo().CurrentRoom.RoomMuted)
                            {
                                Session.SendWhisper("The room you are currently in isn't roommuted.");
                                return true;
                            }

                            Session.GetHabbo().CurrentRoom.RoomMuted = false;

                            lock (Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            {
                                foreach (RoomUser client in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                                {
                                    if (client == null)
                                        continue;
                                    if (client.GetClient().GetHabbo() == null)
                                        continue;
                                    if (client.GetClient().GetHabbo().CurrentRoom == null)
                                        continue;

                                    if (client.GetClient().GetHabbo().CurrentRoom == Session.GetHabbo().CurrentRoom)
                                    {
                                        client.GetClient().SendWhisperBubble("The room has been un-muted!", 34);
                                    }
                                }
                            }

                            Plus.GetGame()
                                .GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, string.Empty,
                                    "Room Unmute", "Room UnMuted");
                        }
                        return true;
                    }
                #endregion

                #region Mute (:mute)
                case "muteu":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            string TargetUser = null;
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            TargetUser = Params[1];
                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(TargetUser);

                            if (TargetClient == null || TargetClient.GetHabbo() == null)
                            {
                                Session.SendWhisper("User could not be found.");
                                return true;
                            }

                            if (TargetClient.GetHabbo().Rank >= 6)
                            {
                                Session.SendWhisper("You are not allowed to (un)mute that user.");
                                return true;
                            }
                            RoleplayManager.Shout(Session, "*Mutes " + TargetClient.GetHabbo().UserName + "*", 33);

                            Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, TargetClient.GetHabbo().UserName, "Mute", "Muted user");
                            TargetClient.GetHabbo().Mute();
                        }
                        return true;
                    }
                #endregion

                #region Unmute (:unmute)
                case "unmuteu":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            string TargetUser = null;
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            TargetUser = Params[1];
                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(TargetUser);

                            if (TargetClient == null || TargetClient.GetHabbo() == null)
                            {
                                Session.SendWhisper("User could not be found.");
                                return true;
                            }

                            if (TargetClient.GetHabbo().Rank >= 6)
                            {
                                Session.SendWhisper("You are not allowed to (un)mute that user.");
                                return true;
                            }
                            RoleplayManager.Shout(Session, "*Un-mutes " + TargetClient.GetHabbo().UserName + "*", 33);

                            Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, TargetClient.GetHabbo().UserName, "Unmute", "Unmuted user");
                            TargetClient.GetHabbo().UnMute();
                        }
                        return true;
                    }
                #endregion

                #region Flood (:flood) *NEW*

                case "flood":
                    {
                        #region Generate Instances / Sessions
                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :flood x <reason>");
                            return true;
                        }

                        string Target = Convert.ToString(Params[1]);
                        string Reason = Convert.ToString(Params[2]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            return false;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }
                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :flood x <reason>");
                            return true;
                        }
                        if (TargetSession.GetHabbo().Rank >= Session.GetHabbo().Rank)
                        {
                            Session.SendWhisper("You are not allowed to mute that user.");
                            return true;
                        }
                        #endregion

                        #region Execute

                        switch (Reason)
                        {
                            case "racism":
                                {
                                    #region Racism

                                    //FLOODS THE PLAYER
                                    int racism = 300;
                                    TargetSession.GetHabbo().FloodTime = Plus.GetUnixTimeStamp() + Convert.ToInt32(racism);
                                    ServerMessage Packet = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                                    Packet.AppendInteger(Convert.ToInt32(racism));
                                    TargetSession.SendMessage(Packet);

                                    //SESSION CALLS OUT THE PLAYER OH SHIT
                                    RoleplayManager.Shout(Session, "*Floods " + TargetSession.GetHabbo().UserName + " for " + TimeSpan.FromSeconds(racism).TotalMinutes + " minutes for racism*", 33);

                                    #endregion
                                }
                                break;

                            case "harassment":
                            case "harass":
                            case "assault":
                                {
                                    #region Harassment

                                    //FLOODS THE PLAYER
                                    int harass = 120;
                                    TargetSession.GetHabbo().FloodTime = Plus.GetUnixTimeStamp() + Convert.ToInt32(harass);
                                    ServerMessage Packet = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                                    Packet.AppendInteger(Convert.ToInt32(harass));
                                    TargetSession.SendMessage(Packet);

                                    //SESSION CALLS OUT THE PLAYER OH SHIT
                                    RoleplayManager.Shout(Session, "*Floods " + TargetSession.GetHabbo().UserName + " for " + TimeSpan.FromSeconds(harass).TotalMinutes + " minutes for harassment*", 33);

                                    #endregion
                                }
                                break;

                            case "language":
                            case "foul":
                            case "offensive":
                                {
                                    #region Language

                                    //FLOODS THE PLAYER
                                    int olang = 300;
                                    TargetSession.GetHabbo().FloodTime = Plus.GetUnixTimeStamp() + Convert.ToInt32(olang);
                                    ServerMessage Packet = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                                    Packet.AppendInteger(Convert.ToInt32(olang));
                                    TargetSession.SendMessage(Packet);

                                    //SESSION CALLS OUT THE PLAYER OH SHIT
                                    RoleplayManager.Shout(Session, "*Floods " + TargetSession.GetHabbo().UserName + " for " + TimeSpan.FromSeconds(olang).TotalMinutes + " minutes for offensive language*", 33);

                                    #endregion
                                }
                                break;

                            case "advertising":
                            case "advertise":
                                {
                                    #region Advertising

                                    //FLOODS THE PLAYER
                                    int adv = 31536000;
                                    TargetSession.GetHabbo().FloodTime = Plus.GetUnixTimeStamp() + Convert.ToInt32(adv);
                                    ServerMessage Packet = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                                    Packet.AppendInteger(Convert.ToInt32(adv));
                                    TargetSession.SendMessage(Packet);

                                    //SESSION CALLS OUT THE PLAYER OH SHIT
                                    RoleplayManager.Shout(Session, "*Floods " + TargetSession.GetHabbo().UserName + " for " + TimeSpan.FromSeconds(adv).TotalMinutes + " minutes for advertising other websites*", 33);

                                    #endregion
                                }
                                break;

                            case "spam":
                            case "spamming":
                                {
                                    #region Spamming

                                    //FLOODS THE PLAYER
                                    int spam = 60;
                                    TargetSession.GetHabbo().FloodTime = Plus.GetUnixTimeStamp() + Convert.ToInt32(spam);
                                    ServerMessage Packet = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                                    Packet.AppendInteger(Convert.ToInt32(spam));
                                    TargetSession.SendMessage(Packet);

                                    //SESSION CALLS OUT THE PLAYER OH SHIT
                                    RoleplayManager.Shout(Session, "*Floods " + TargetSession.GetHabbo().UserName + " for " + TimeSpan.FromSeconds(spam).TotalMinutes + " minute for spamming*", 33);

                                    #endregion
                                }
                                break;

                            case "other":
                                {
                                    #region Other

                                    //FLOODS THE PLAYER
                                    int ot = 180;
                                    TargetSession.GetHabbo().FloodTime = Plus.GetUnixTimeStamp() + Convert.ToInt32(ot);
                                    ServerMessage Packet = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                                    Packet.AppendInteger(Convert.ToInt32(ot));
                                    TargetSession.SendMessage(Packet);

                                    //SESSION CALLS OUT THE PLAYER OH SHIT
                                    RoleplayManager.Shout(Session, "*Floods " + TargetSession.GetHabbo().UserName + " for " + TimeSpan.FromSeconds(ot).TotalMinutes + " minutes for other*", 33);
                                    Session.SendWhisper("Please alert that user (:alert x <msg>) on why you have muted that user..");

                                    #endregion
                                }
                                break;



                            default:
                                {
                                    Session.SendWhisper("No reason found. The reasons are: racism, harassment, language, advertising, spam, feet and other.");
                                }
                                break;
                        }

                        return true;
                    }

                #endregion

                #endregion

                #region Summon (:summon)
                case "summon":
                case "come":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_builder"))
                        {
                            var userName = Params[1];
                            if (String.Equals(userName, Session.GetHabbo().UserName,
                                StringComparison.CurrentCultureIgnoreCase))
                            {
                                Session.SendNotif(Plus.GetLanguage().GetVar("summon_yourself"));
                                return true;
                            }
                            var client = Plus.GetGame().GetClientManager().GetClientByUserName(userName);
                            if (client == null)
                            {
                                Session.SendNotif(Plus.GetLanguage().GetVar("user_not_found"));
                                return true;
                            }
                            if (Session.GetHabbo().CurrentRoom != null &&
                                Session.GetHabbo().CurrentRoomId != client.GetHabbo().CurrentRoomId)
                                client.GetMessageHandler()
                                    .PrepareRoomForUser(Session.GetHabbo().CurrentRoom.RoomId,
                                        Session.GetHabbo().CurrentRoom.RoomData.PassWord);

                            client.SendNotif("You were summoned by " + Session.GetHabbo().UserName + "!");
                            Session.Shout("*Summons " + client.GetHabbo().UserName + " to the room*");
                            return true;
                        }
                        return true;
                    }
                #endregion

                #region Follow (:follow)
                case "follow":
                case "stalk":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_builder"))
                        {
                            GameClient TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);

                            if (TargetClient == null || TargetClient.GetHabbo() == null)
                            {
                                Session.SendWhisper("This user could not be found");
                                return true;
                            }

                            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom == null || TargetClient.GetHabbo().CurrentRoom == Session.GetHabbo().CurrentRoom)
                            {
                                Session.SendWhisper("This user is either not found, not in a room or in the same room as you.");
                                return true;
                            }

                            //Session.SendMessage(new RoomForwardComposer(false, TargetClient.GetHabbo().CurrentRoom.RoomId));

                            Session.GetMessageHandler().PrepareRoomForUser(TargetClient.GetHabbo().CurrentRoom.RoomId, "");
                            return true;
                        }
                        return true;
                    }
                #endregion

                #region Room kick (:roomkick)
                case "roomkick":
                    {
                        #region Conditions
                        if (!Session.GetHabbo().HasFuse("fuse_owner"))
                        {
                            return true;
                        }
                        #endregion

                        #region Execute
                        lock (Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                        {
                            foreach (RoomUser client in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            {
                                if (client == null)
                                    continue;
                                if (client.GetClient() == null)
                                    continue;
                                if (client.GetClient().GetHabbo() == null)
                                    continue;
                                if (client.GetClient().GetHabbo().CurrentRoom == null)
                                    continue;
                                if (client.GetClient().GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                                    continue;
                                if (client.GetClient() == Session)
                                    continue;
                                if (client.GetClient().GetMessageHandler() == null)
                                    continue;

                                client.GetClient().GetMessageHandler().PrepareRoomForUser(1, "");
                                client.GetClient().SendNotif("An administrator has kicked you from the room!");
                            }
                        }

                        RoleplayManager.Shout(Session, "*Kicks the whole room*", 33);

                        #endregion
                        return true;
                    }
                #endregion

                #region Super ban (:superban)
                case "superban":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            string Username = Params[1];
                            string Reason = "No reason specified.";

                            if (Params.Length >= 2)
                            {
                                Reason = MergeParams(Params, 2);
                            }
                            else
                            {
                                Reason = "No reason specified.";
                            }

                            GameClient TargetUser = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1].ToString());
                            if (TargetUser == null)
                            {
                                Session.SendWhisper("An unknown error occured whilst finding this user!");
                                return true;
                            }
                            if (TargetUser.GetHabbo().Rank >= Session.GetHabbo().Rank)
                            {
                                Session.SendWhisper("You are not allowed to ban that user.");
                                return true;
                            }
                            try
                            {
                                RoleplayManager.Shout(Session, "*Uses their god-like powers to user-ban '" + TargetUser.GetHabbo().UserName + "' for life*", 33);
                                Plus.GetGame().GetBanManager().BanUser(TargetUser, Session.GetHabbo().UserName, Support.ModerationBanType.USERNAME, TargetUser.GetHabbo().UserName, Reason, 360000000.0);
                                string Message = Session.GetHabbo().UserName + " super banned " + TargetUser.GetHabbo().UserName + " for life";
                                RoleplayManager.sendStaffAlert(Message, true);
                            }
                            catch (Exception e) { Console.WriteLine(e); }
                        }
                        return true;
                    }
                #endregion

                #region Ban (:ban)

                case "ban":
                case "banuser":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            string Reason = "No reason specified.";

                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);

                            if (TargetClient == null)
                            {
                                Session.SendWhisper("This user was not found.");
                                return true;
                            }

                            if (TargetClient.GetHabbo().Rank >= Session.GetHabbo().Rank)
                            {
                                Session.SendNotif("You're not allowed to ban that user.");
                                return true;
                            }

                            if (Params.Length >= 3)
                            {
                                Reason = MergeParams(Params, 3);
                            }
                            else
                            {
                                Reason = "No reason specified.";
                            }

                            double BanTime = 10;

                            try
                            {
                                BanTime = TimeSpan.FromMinutes(Convert.ToDouble(Params[2])).TotalSeconds;
                            }
                            catch (FormatException) { return true; }

                            if (BanTime < 10)
                            {
                                Session.SendNotif("The minimum ban time is 10 minutes.");
                                return true;
                            }
                            else
                            {
                                Session.Shout("*Uses their god-like powers to user-ban '" + TargetClient.GetHabbo().UserName + "' for " + Params[2] + " minute(s)*", 33);
                                Plus.GetGame().GetBanManager().BanUser(TargetClient, Session.GetHabbo().UserName, Support.ModerationBanType.USERNAME, TargetClient.GetHabbo().UserName, Reason, BanTime);
                                Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, TargetClient.GetHabbo().UserName, "Ban", "Ban for " + BanTime + " seconds with message " + Reason);
                                string Message = Session.GetHabbo().UserName + " banned " + TargetClient.GetHabbo().UserName + " for " + Params[2] + " mintues";
                                RoleplayManager.sendStaffAlert(Message, true);
                            }
                        }

                        return true;
                    }

                #endregion

                #region staff hub (:staffhub x)
                case "staffhub":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            uint StaffHubId = 88;

                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);

                            if (TargetClient == null)
                            {
                                Session.SendWhisper("This user was not found.");
                                return true;
                            }

                            if (TargetClient.GetHabbo().Rank >= Session.GetHabbo().Rank && TargetClient.GetHabbo().Rank != 12)
                            {
                                Session.SendNotif("You're not allowed to send that user to the staff hub.");
                                return true;
                            }

                            TargetClient.SendNotif(Session.GetHabbo().UserName + " sent you to the staff hub");
                            RoleplayManager.Shout(Session, "*Uses their god-like powers to send " + TargetClient.GetHabbo().UserName + " to the staff hub*", 33);
                            Room Room = RoleplayManager.GenerateRoom(StaffHubId);
                            TargetClient.GetRoleplay().RequestedTaxi_WaitTime = 0;
                            TargetClient.GetRoleplay().RequestedTaxi_Arrived = false;
                            TargetClient.GetRoleplay().RecentlyCalledTaxi = true;
                            TargetClient.GetRoleplay().RecentlyCalledTaxi_Timeout = 0;
                            TargetClient.GetRoleplay().RequestedTaxiDestination = Room;
                            TargetClient.GetRoleplay().RequestedTaxi = true;
                            TargetClient.GetRoleplay().taxiTimer = new taxiTimer(TargetClient);
                            string Message = Session.GetHabbo().UserName + " sent " + TargetClient.GetHabbo().UserName + " to the staff hub";
                            RoleplayManager.sendStaffAlert(Message, true);

                        }
                        return true;
                    }


                #endregion

                #region Pornban (:pornban)
                case "pb":
                case "pornban":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            string Link = "http://pornhub.com/";

                            if (!RoleplayManager.ParamsMet(Params, 2))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :pornban <user> <link>");
                                return true;
                            }

                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            Link = Params[2];

                            if (TargetClient == null)
                            {
                                Session.SendWhisper("This user was not found.");
                                return true;
                            }

                            if (TargetClient.GetHabbo().Rank >= Session.GetHabbo().Rank)
                            {
                                Session.SendNotif("You're not allowed to ban that user.");
                                return true;
                            }

                            if (Link == null || Link == "")
                            {
                                Session.SendNotif("Invalid link");
                                return true;
                            }




                            string Ip = "";

                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("SELECT ip_last FROM users WHERE username = '" + TargetClient.GetHabbo().UserName + "'");
                                DataRow User = dbClient.GetRow();
                                Ip = Convert.ToString(User["ip_last"]);
                            }


                            if (Ip == "")
                            {
                                Session.SendWhisper("Could not find the users IP address");
                                return true;
                            }


                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunFastQuery("INSERT INTO porn_bans(value,link,admin_msg) VALUES('" + Ip + "','" + Link + "','[" + Session.GetHabbo().UserName + " porn banned " + TargetClient.GetHabbo().UserName + " via client]')");
                            }

                            Session.SendWhisper("User '" + TargetClient.GetHabbo().UserName + "' successfully porn banned!");


                        }

                        return true;
                    }
                #endregion

                #region alert (:alert)
                case "alert":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            string Message = "";
                            string TargetUser = null;

                            if (!RoleplayManager.ParamsMet(Params, 2))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :alert <user> <message>");
                                return true;
                            }
                            else
                            {
                                TargetUser = Params[1];
                                Message = MergeParams(Params, 2);
                            }


                            GameClient TargetClient = null;
                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(TargetUser);


                            if (!RoleplayManager.CanInteract(Session, TargetClient, false))
                            {
                                Session.SendWhisper("User could not be found.");
                                return true;
                            }

                            Room TargetRoom = TargetClient.GetHabbo().CurrentRoom;

                            Session.SendWhisper("" + TargetClient.GetHabbo().UserName + " has been alerted!");
                            Plus.GetGame().GetClientManager().SendSuperNotif("Message from FluxRP - Warning", Message, "frank10", TargetClient, "event:", "ok", false, false);
                            //TargetClient.SendNotif(Params[2] + " -" + Session.GetHabbo().UserName);
                        }
                        return true;
                    }
                #endregion
                #endregion

                #region Administration Commands

                #region Fast Walk
                case "fastwalk":
                case "correr":
                    {
                        if (!RoleplayManager.BypassRights(Session))
                        {
                            Session.SendWhisper("You're not allowed to do this..");
                            return true;
                        }

                        Room Room = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                        if (!User.FastWalking)
                        {
                            User.FastWalking = true;
                        }
                        else
                        {
                            User.FastWalking = false;
                        }


                        return true;
                    }
                #endregion

                #region Flagother (:flagu x)
                case "flagu":
                case "flagother":
                    {
                        #region Params

                        GameClient Target = null;
                        string Username = Params[1].ToString();
                        string NewName = Convert.ToString(Params[2]);

                        #endregion

                        #region Conditions

                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return true;
                        }
                        if (Params.Length <= 1)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :flagother <user> <newname>");
                            return true;
                        }
                        else
                        {
                            Target = RoleplayManager.GenerateSession(Username);
                        }
                        if (NewName.Contains("'") || Enumerable.Contains<char>((IEnumerable<char>)NewName, '"') || (NewName.Contains(">") || NewName.Contains("<")) || (NewName.Contains("|") || NewName.Contains("`") || (NewName.Contains(";") || NewName.Contains("="))) || (NewName.Contains("+") || NewName.Contains("/") || (NewName.Contains("?") || NewName.Contains("/")) || (NewName.Contains(".") || NewName.Contains(",") || (NewName.Contains(")") || NewName.Contains("(")))) || (NewName.Contains("*") || NewName.Contains("&") || (NewName.Contains("^") || NewName.Contains("%")) || (NewName.Contains("$") || NewName.Contains("$") || (NewName.Contains("!") || NewName.Contains("@"))) || (NewName.Contains("#") || NewName.Contains("~") || (NewName.Contains("]") || NewName.Contains("[")) || (NewName.Contains("}") || NewName.Contains("{")))) || NewName.Contains(":"))
                        {
                            Session.SendWhisper("Invalid characters used!");
                            return true;
                        }
                        if (Target == null)
                        {
                            Session.SendWhisper("The user " + Username + ", was not found!");
                            return true;
                        }
                        #endregion

                        #region Execute
                        DataRow Row;
                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = '" + NewName + "'");
                            Row = dbClient.GetRow();
                        }
                        if (Row != null)
                        {
                            Session.SendWhisper("This name '" + NewName + "' you have chosen is taken!");
                            return true;
                        }
                        else
                        {
                            RoleplayManager.Shout(Session, "*Uses their god-like powers to change the " + Target.GetHabbo().UserName + "'s name from " + Target.GetHabbo().UserName + " to " + NewName + "*");
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunFastQuery("UPDATE `users` SET `username` = '" + NewName + "' WHERE `username` = '" + Target.GetHabbo().UserName + "'");
                                dbClient.RunFastQuery("UPDATE `rooms_data` SET `owner` = '" + NewName + "' WHERE `owner` = '" + Target.GetHabbo().UserName + "'");
                                Target.Disconnect("Updating Username");
                            }
                            return true;
                        }
                        #endregion

                    }

                #endregion

                #region Flag user (:flag x)

                case "flag":
                case "flaguser":
                    {
                        #region Generate Instances / Sessions

                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        #endregion

                        #region Conditions

                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return false;
                        }
                        if (Params.Length <= 1)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :flag x");
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }
                        if (TargetSession.GetHabbo().Rank >= Session.GetHabbo().Rank)
                        {
                            Session.SendWhisper("You are not allowed to flag this user!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_stats` SET `flagged` = '1' WHERE `id` = '" + TargetSession.GetHabbo().Id + "'");
                            dbClient.RunQuery();
                        }
                        RoleplayManager.Shout(Session, "*Uses my god-like powers to flag " + TargetSession.GetHabbo().UserName + "*", 33);
                        Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, TargetSession.GetHabbo().UserName, "Flag", "Flagged User [" + TargetSession.GetHabbo().UserName + "]");
                        TargetSession.Disconnect("Flagged as invalid username");

                        #endregion

                        return true;
                    }

                #endregion

                #region Give Diamonds (:diamonds x)
                case "givecrystals":
                case "crystals":
                case "tokens":
                case "givetokens":
                case "diamonds":
                case "givediamonds":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;

                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (TargetClient != null)
                            {
                                int creditsToAdd;
                                if (int.TryParse(Params[2], out creditsToAdd))
                                {
                                    TargetClient.GetHabbo().BelCredits += creditsToAdd;
                                    TargetClient.GetHabbo().UpdateActivityPointsBalance();
                                    TargetClient.SendWhisper(Session.GetHabbo().UserName + (" has awarded you ") + creditsToAdd.ToString() + (" VIP Tokens!"));
                                    Session.SendWhisper(("VIP Tokens balance updated successfully."));
                                    return true;
                                }
                                else
                                {
                                    Session.SendWhisper("Numbers Only!");
                                    return true;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("User could not be found.");
                                return true;
                            }
                        }
                        return true;
                    }
                #endregion

                #region Mass Enable (:massenable)
                case "massenable":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {
                            Room room28 = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
                            room28.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                            lock (room28.GetRoomUserManager().GetRoomUsers())
                            {
                                foreach (RoomUser user24 in room28.GetRoomUserManager().GetRoomUsers())
                                {
                                    if (!user24.RidingHorse)
                                    {
                                        user24.ApplyEffect(Convert.ToInt32(Params[1]));
                                    }
                                }
                            }
                        }
                        return true;
                    }
                #endregion

                #region Give Credits (:credits)
                case "givecredits":
                case "credits":
                case "coins":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {

                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;

                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (TargetClient != null)
                            {
                                int creditsToAdd;
                                if (int.TryParse(Params[2], out creditsToAdd))
                                {
                                    TargetClient.GetHabbo().Credits = TargetClient.GetHabbo().Credits + creditsToAdd;
                                    TargetClient.GetHabbo().UpdateCreditsBalance();
                                    TargetClient.SendWhisper("An administrator has given you $" + creditsToAdd + "!");
                                    Session.SendWhisper("Successfully gave " + TargetClient.GetHabbo().UserName + " $" + creditsToAdd);
                                    return true;
                                }
                                else
                                {
                                    Session.SendNotif("Numbers Only!");
                                    return true;
                                }
                            }
                            else
                            {
                                Session.SendNotif("User could not be found.");
                                return true;
                            }
                        }
                        return true;
                    }
                #endregion

                #region Give Pixels (:pixels)
                case "pixels":
                case "givepixels":
                case "duckets":
                case "giveduckets":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {

                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;

                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (TargetClient != null)
                            {
                                int creditsToAdd;
                                if (int.TryParse(Params[2], out creditsToAdd))
                                {
                                    TargetClient.GetHabbo().ActivityPoints = TargetClient.GetHabbo().ActivityPoints + creditsToAdd;
                                    Session.GetHabbo().NotifyNewPixels(creditsToAdd);
                                    TargetClient.GetHabbo().UpdateActivityPointsBalance();
                                    TargetClient.SendNotif(Session.GetHabbo().UserName + " has arwarded you " + creditsToAdd.ToString() + " pixels/duckets.");
                                    Session.SendWhisper("hi");
                                    return true;
                                }
                                else
                                {
                                    Session.SendNotif("Numbers Only!");
                                    return true;
                                }
                            }
                            else
                            {
                                Session.SendNotif("User could not be found.");
                                return true;
                            }
                        }
                        return true;
                    }
                #endregion

                #region Give Vests (:givevests)
                case "givevests":
                case "givevest":
                case "vests":
                case "vest":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {

                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;

                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (TargetClient != null)
                            {
                                int vestsToAdd;
                                if (int.TryParse(Params[2], out vestsToAdd))
                                {
                                    TargetClient.GetRoleplay().Vests += vestsToAdd;
                                    TargetClient.GetRoleplay().SaveQuickStat("vests", "" + TargetClient.GetRoleplay().Vests);
                                    TargetClient.SendWhisper("An administrator has given you " + vestsToAdd + " vests!");
                                    Session.SendWhisper("Successfully gave " + TargetClient.GetHabbo().UserName + " " + vestsToAdd + " vests");
                                    return true;
                                }
                                else
                                {
                                    Session.SendNotif("Numbers Only!");
                                    return true;
                                }
                            }
                            else
                            {
                                Session.SendNotif("User could not be found.");
                                return true;
                            }
                        }
                        return true;
                    }
                #endregion

                #region Hotel Alert (:ha)
                case "ha":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return false;
                        }

                        string Notice = MergeParams(Params, 1);
                        ServerMessage HotelAlert = new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
                        HotelAlert.AppendString(Notice + "\r\n" + "- " + Session.GetHabbo().UserName);


                        Plus.GetGame().GetClientManager().QueueBroadcaseMessage(HotelAlert);

                        Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, string.Empty, "HotelAlert", "Hotel alert [" + Notice + "]");
                    }
                    return true;
                #endregion

                #region Whisper Hotel Alert (:wha)

                case "wha":
                case "walert":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return false;
                        }

                        string Notice = MergeParams(Params, 1);

                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient mClient in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (mClient == null)
                                    continue;
                                if (mClient.GetHabbo() == null)
                                    continue;
                                if (mClient.GetHabbo().CurrentRoom == null)
                                    continue;
                                if (mClient.GetConnection() == null)
                                    continue;
                                mClient.SendWhisperBubble("[Hotel Alert][" + Session.GetHabbo().UserName + "]: " + Notice, 33);
                            }
                        }

                        return true;
                    }

                #endregion

                #region :vha <msg>

                case "vipaha":
                case "vipwha":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return false;
                        }

                        string Notice = MergeParams(Params, 1);

                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient mClient in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (mClient == null)
                                    continue;
                                if (mClient.GetHabbo() == null)
                                    continue;
                                if (mClient.GetHabbo().CurrentRoom == null)
                                    continue;
                                if (mClient.GetConnection() == null)
                                    continue;
                                if (!mClient.GetHabbo().HasFuse("fuse_vip"))
                                    continue;
                                mClient.SendWhisperBubble("[VIP Alert][" + Session.GetHabbo().UserName + "]: " + Notice, 19);
                            }
                        }

                        return true;
                    }

                #endregion

                #region Shutdown (:shutdown)
                case "shutdown":
                case "remulador":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {
                            Task ShutdownTask = new Task(Plus.PerformShutDown);
                            ShutdownTask.Start();
                            Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, string.Empty, "Shutdown", "Issued shutdown command");
                            return true;
                        }
                        return true;
                    }
                #endregion

                #region Disconnect (:dc)
                case "disconnect":
                case "dc":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);

                            if (TargetClient == null)
                            {
                                Session.SendWhisper("User not found.");
                                return true;
                            }

                            if (TargetClient.GetHabbo().Rank >= 4 && !Session.GetHabbo().HasFuse("fuse_disconnect_anyone"))
                            {
                                Session.SendNotif("You are not allowed to disconnect that user.");
                                return true;
                            }
                            Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, TargetClient.GetHabbo().UserName, "Disconnect", "User disconnected by user");

                            TargetClient.Disconnect("Disconnected by staff");
                            Session.SendWhisper("Successfully disconnected " + TargetClient.GetHabbo().UserName);
                        }
                        return true;
                    }
                #endregion

                #region Position (:coords)
                case "coord":
                case "coords":
                case "position":
                    {
                        Room TargetRoom = Session.GetHabbo().CurrentRoom;
                        RoomUser TargetRoomUser = null;
                        TargetRoom = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);

                        if (TargetRoom == null)
                        {
                            return true;
                        }

                        TargetRoomUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                        if (TargetRoomUser == null)
                        {
                            return true;
                        }

                        Session.SendNotif("X: " + TargetRoomUser.X + "\n - Y: " + TargetRoomUser.Y + "\n - Z: " + TargetRoomUser.Z + "\n - Rot: " + TargetRoomUser.RotBody + ", sqState: " + TargetRoom.GetGameMap().GameMap[TargetRoomUser.X, TargetRoomUser.Y].ToString() + "\n\n - RoomID: " + Session.GetHabbo().CurrentRoomId);
                        return true;
                    }
                #endregion

                #region Teleport (:teleport)
                case "teleport":
                case "tele":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            RoomUser TargetRoomUser = null;

                            TargetRoom = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);

                            TargetRoomUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                            if (TargetRoomUser.RidingHorse)
                            {
                                SendChatMessage(Session, "You cannot teleport whilst riding a horse!");
                                return true;
                            }
                            if (TargetRoomUser == null)
                                return true;

                            TargetRoomUser.TeleportEnabled = !TargetRoomUser.TeleportEnabled;

                            if (!TargetRoomUser.TeleportEnabled)
                            {
                                Session.SendWhisper("Teleport disabled!");
                            }
                            else if (TargetRoomUser.TeleportEnabled)
                            {
                                Session.SendWhisper("Teleport enabled!");
                            }

                            TargetRoom.GetGameMap().GenerateMaps();
                        }
                        return true;
                    }
                #endregion

                #region Update (:update_x)
                #region Update Catalog
                case "update_catalog":
                case "reload_catalog":
                case "recache_catalog":
                case "refresh_catalog":
                case "update_catalogue":
                case "reload_catalogue":
                case "recache_catalogue":
                case "refresh_catalogue":
                case "updatecatalog":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                Plus.GetGame().GetCatalog().Initialize(dbClient, Session);
                            }
                            try
                            {
                                Plus.GetGame().GetClientManager().QueueBroadcaseMessage(new ServerMessage(LibraryParser.OutgoingRequest("PublishShopMessageComposer")));
                            }
                            catch
                            {
                                Session.SendWhisper("PACKET PROBLEM");
                            }
                            Session.SendWhisper("The catalog has been refreshed!");

                        }
                        return true;
                    }
                #endregion

                #region Update Items
                case "update_items":
                case "reload_items":
                case "recache_items":
                case "refresh_items":
                case "updateitems":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                Plus.GetGame().GetItemManager().LoadItems(dbClient);
                            }
                            Session.SendWhisper("Item definitions have been refreshed!");
                        }
                        return true;
                    }
                #endregion

                #region Update Youtube TVs
                case "update_youtube":
                case "refresh_youtube":
                case "updatetvs":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        Session.SendWhisper("Please wait, updating YouTube playlists...");
                        Session.GetHabbo().GetYoutubeManager().RefreshVideos();
                        Session.SendWhisper("Done! YouTube playlists were reloaded.");
                        return true;
                    }
                #endregion

                #region Update Clothing
                case "update_clothing":
                case "refresh_clothing":
                case "updateclothing":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        Session.SendWhisper("Updating Clothing.. please wait!");
                        using (IQueryAdapter adapter5 = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            Plus.GetGame().GetClothingManager().Initialize(adapter5);
                        }
                        Session.SendWhisper("Clothing has successfully updated!");
                        return true;
                    }
                #endregion

                #region Update Polls
                case "reload_polls":
                case "refresh_polls":
                case "update_polls":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        using (IQueryAdapter adapter5 = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            Plus.GetGame().GetPollManager().Init(adapter5);
                        }
                        return true;
                    }
                #endregion

                #region Update Breeds
                case "update_breeds":
                case "refresh_petbreeds":
                case "updatebreeds":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        using (IQueryAdapter adapter6 = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            PetRace.Init(adapter6);
                        }
                        return true;
                    }
                #endregion

                #region Update Filter
                case "update_filter":
                case "refresh_bannedhotels":
                case "updatefilter":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }

                        using (var dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            Security.AntiPublicistas.Load(dbClient);
                        }

                        Session.SendWhisper(Plus.GetLanguage().GetVar("command_refresh_banned_hotels"));
                        return true;
                    }
                #endregion

                #region Update Songs
                case "update_songs":
                case "refresh_songs":
                case "updatesongs":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        HabboHotel.SoundMachine.SongManager.Initialize();
                        Session.SendWhisper("Successfully refreshed songs.");

                        return true;
                    }
                #endregion

                #region Update Achievements
                case "update_achievements":
                case "refresh_achievements":
                case "updateachievements":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return true;
                        }
                        Plus.GetGame().GetAchievementManager().LoadAchievements(Plus.GetDatabaseManager().GetQueryReactor());
                        Session.SendWhisper("Successfully refreshed achievements.");

                        return true;
                    }
                #endregion

                #region Update Navigator
                case "update_navigator":
                case "reload_navigator":
                case "recache_navigator":
                case "refresh_navigator":
                case "updatenavigator":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                Plus.GetGame().GetNavigator().Initialize(dbClient);
                            }
                            Session.SendWhisper("The navigator has been updated!");
                        }
                        return true;
                    }
                #endregion

                #region Update Ranks
                case "update_ranks":
                case "reload_ranks":
                case "recache_ranks":
                case "refresh_ranks":
                case "updateranks":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                Plus.GetGame().GetRoleManager().LoadRights(dbClient);
                            }
                            Session.SendWhisper("Ranks have been refreshed!");
                        }
                        return true;
                    }
                #endregion

                #region Update Settings
                case "update_settings":
                case "reload_settings":
                case "recache_settings":
                case "refresh_settings":
                case "updatesettings":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                                Plus.ConfigData = new ConfigData(dbClient);
                        }
                        return true;
                    }
                #endregion

                #region Update Groups
                case "update_groups":
                case "reload_groups":
                case "recache_groups":
                case "refresh_groups":
                case "updategroups":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Plus.GetGame().GetGroupManager().InitGroups();
                            Session.SendWhisper("Groups have been successfully reloaded");
                        }

                        return true;
                    }
                #endregion

                #region Update Bans
                case "update_bans":
                case "updatebans":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Plus.GetGame().GetBanManager().ReCacheBans();
                            Session.SendWhisper("Bans have been refreshed!");
                        }
                        return true;
                    }
                #endregion

                #region Update Quests
                case "update_quests":
                case "updatequests":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Plus.GetGame().GetQuestManager().Initialize(Plus.GetDatabaseManager().GetQueryReactor());
                            Session.SendWhisper("Quests have been successfully reloaed!");
                        }
                        return true;
                    }
                #endregion
                #endregion

                #region Super Pull (:spull)
                case "spull":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_builder"))
                        {
                            Room room = Session.GetHabbo().CurrentRoom;
                            if (room == null)
                            {
                                SendChatMessage(Session, "Error in finding room!");
                                return true;
                            }


                            RoomUser roomuser = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                            if (roomuser == null)
                            {
                                SendChatMessage(Session, "Unable to find user!");
                                return true;
                            }
                            if (Params.Length == 1)
                            {
                                SendChatMessage(Session, "Unable to find user!");
                                return true;
                            }


                            GameClient Target = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            RoomUser TargetUser = room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Id);
                            if (Target.GetHabbo().Id == Session.GetHabbo().Id)
                            {
                                SendChatMessage(Session, "You cannot pull yourself!");
                                return true;
                            }

                            if (TargetUser.TeleportEnabled)
                                return true;

                            if (roomuser.RotBody % 2 != 0)
                                roomuser.RotBody--;
                            Session.Shout("*spulls " + TargetUser.GetClient().GetHabbo().UserName + " to them*");
                            if (roomuser.RotBody == 0)
                                TargetUser.MoveTo(roomuser.X, roomuser.Y - 1);
                            else if (roomuser.RotBody == 2)
                                TargetUser.MoveTo(roomuser.X + 1, roomuser.Y);
                            else if (roomuser.RotBody == 4)
                                TargetUser.MoveTo(roomuser.X, roomuser.Y + 1);
                            else if (roomuser.RotBody == 6)
                                TargetUser.MoveTo(roomuser.X - 1, roomuser.Y);
                        }
                        return true;
                    }
                #endregion

                #region Super Push (:spush)
                case "spush":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            Room TargetRoom;
                            RoomUser TargetRoomUser;
                            RoomUser TargetRoomUser1;
                            TargetRoom = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);

                            if (TargetRoom == null)
                            {
                                return true;
                            }

                            if (Params.Length == 1)
                            {
                                SendChatMessage(Session, "Sintaxe de comando inválida: :spush <user>");
                                return true;
                            }

                            TargetRoomUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Convert.ToString(Params[1]));

                            if (TargetRoomUser == null)
                            {
                                SendChatMessage(Session, "Could not find that user!");
                                return true;
                            }

                            if (TargetRoomUser.GetUserName() == Session.GetHabbo().UserName)
                            {
                                SendChatMessage(Session, "Come on, surely you don't want to spush yourself!");
                                return true;
                            }

                            TargetRoomUser1 = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                            if (TargetRoomUser1 == null || TargetRoomUser.TeleportEnabled)
                            {
                                return true;
                            }

                            //if ((TargetRoomUser.X == TargetRoomUser1.X - 1) || (TargetRoomUser.X == TargetRoomUser1.X + 1) || (TargetRoomUser.Y == TargetRoomUser1.Y - 1) || (TargetRoomUser.Y == TargetRoomUser1.Y + 1))
                            if (!((Math.Abs((int)(TargetRoomUser.X - TargetRoomUser1.X)) >= 2) || (Math.Abs((int)(TargetRoomUser.Y - TargetRoomUser1.Y)) >= 2)))
                            {
                                if (TargetRoomUser1.RotBody == 4)
                                { TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 5); }

                                if (TargetRoomUser1.RotBody == 0)
                                { TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 5); }

                                if (TargetRoomUser1.RotBody == 6)
                                { TargetRoomUser.MoveTo(TargetRoomUser.X - 5, TargetRoomUser.Y); }

                                if (TargetRoomUser1.RotBody == 2)
                                { TargetRoomUser.MoveTo(TargetRoomUser.X + 5, TargetRoomUser.Y); }

                                if (TargetRoomUser1.RotBody == 3)
                                {
                                    TargetRoomUser.MoveTo(TargetRoomUser.X + 5, TargetRoomUser.Y);
                                    TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 5);
                                }

                                if (TargetRoomUser1.RotBody == 1)
                                {
                                    TargetRoomUser.MoveTo(TargetRoomUser.X + 5, TargetRoomUser.Y);
                                    TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 5);
                                }

                                if (TargetRoomUser1.RotBody == 7)
                                {
                                    TargetRoomUser.MoveTo(TargetRoomUser.X - 5, TargetRoomUser.Y);
                                    TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 5);
                                }

                                if (TargetRoomUser1.RotBody == 5)
                                {
                                    TargetRoomUser.MoveTo(TargetRoomUser.X - 5, TargetRoomUser.Y);
                                    TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 5);
                                }

                                TargetRoomUser.UpdateNeeded = true;
                                TargetRoomUser1.UpdateNeeded = true;
                                TargetRoomUser1.SetRot(Rotation.Calculate(TargetRoomUser1.X, TargetRoomUser1.Y, TargetRoomUser.GoalX, TargetRoomUser.GoalY));
                                Session.Shout("*spushes " + TargetRoomUser.GetClient().GetHabbo().UserName + "*");
                            }
                            else
                            {
                                SendChatMessage(Session, Params[1] + " is not close enough.");
                            }
                        }
                        else if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOPUSH"))
                        {
                            Session.SendWhisper("You cannot push in this room.");
                            return true;
                        }
                        else
                            Session.SendWhisper("Access denied!");
                        return true;
                    }
                #endregion

                #region Give Badge (:badge)
                case "badge":
                case "givebadge":
                    {
                        if (Session.GetHabbo().GotCommand("givebadge"))
                        {
                            if (Params.Length != 3)
                            {
                                Session.SendNotif("You must include a username and badgecode!");
                                return true;
                            }
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;

                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (TargetClient != null)
                            {
                                TargetClient.GetHabbo().GetBadgeComponent().GiveBadge(Params[2], true, TargetClient);

                                Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, TargetClient.GetHabbo().UserName, "Badge", "Badge given to user [" + Params[2] + "]");
                                TargetClient.SendNotif("You have just been given a badge!");
                                return true;
                            }
                            else
                            {
                                Session.SendNotif("This user was not found");
                                return true;
                            }
                        }
                        return true;
                    }
                #endregion

                #region Give Rank (:rank)

                case "rank":
                case "giverank":
                    {
                        #region Generate Instances / Sessions

                        string Target = Convert.ToString(Params[1]);
                        int RankID = Convert.ToInt32(Params[2]);
                        GameClient TargetSession = null;
                        TargetSession = RoleplayManager.GenerateSession(Target);

                        #endregion

                        #region Conditions

                        if (!RoleplayManager.BypassRights(Session))
                        {
                            return false;
                        }
                        if (!Plus.IsNum(Convert.ToString(RankID)))
                        {
                            Session.SendWhisper("The RankID you've put isn't a number!");
                            return true;
                        }
                        if (Params.Length <= 2)
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :rank x <rankid>");
                            return true;
                        }
                        if (TargetSession == null)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado!");
                            return true;
                        }

                        #endregion

                        #region Execute

                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `users` SET `rank` = '" + RankID + "' WHERE `username` = '" + TargetSession.GetHabbo().UserName + "'");
                            dbClient.RunQuery();
                        }
                        RoleplayManager.Shout(Session, "*Uses their god-like powers to rank " + TargetSession.GetHabbo().UserName + " (RankID: " + RankID + ")*", 33);
                        Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, TargetSession.GetHabbo().UserName, "Rank", "Rank Give To User [" + RankID + "]");
                        TargetSession.SendNotif("You have been ranked by an administrator, please reload the client for for your new rank!");

                        #endregion

                        return true;
                    }

                #endregion

                #region Give Ambassador (:ambas x)
                case "giveambassador":
                case "giveambas":
                case "ambas":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            if (!RoleplayManager.ParamsMet(Params, 1))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :ambas <user>");
                                return true;
                            }
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("UPDATE `users` SET `rank` = '3' WHERE `username` = '" + Params[1] + "'");
                                dbClient.RunQuery();
                            }
                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (TargetClient != null)
                            {
                                RoleplayManager.Shout(Session, "*Gives " + Params[1] + " Ambassador Rank*");
                                TargetClient.GetRoleplay().JobId = 22;
                                TargetClient.GetRoleplay().JobRank = 1;
                                TargetClient.GetRoleplay().SaveJobComponents();
                                RoleplayManager.Shout(Session, "*Superhires " + Params[1] + " as an Ambassador Representative*");
                                Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, TargetClient.GetHabbo().UserName, "Rank", "Ambassador Rank Given to User");
                                TargetClient.SendNotif("You just have been made an Ambassador, please reload the hotel.");
                                return true;
                            }
                            else
                            {
                                Session.SendNotif("This user was not found");
                                return true;
                            }
                        }
                        return true;
                    }
                #endregion

                #region Take Ambassador (:takeambas x)
                case "takeambassador":
                case "takeambas":
                case "tambas":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            if (!RoleplayManager.ParamsMet(Params, 1))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :takeambas <user>");
                                return true;
                            }
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                if (TargetClient.GetHabbo().VIP == true)
                                {
                                    dbClient.SetQuery("UPDATE `users` SET `rank` = '2' WHERE `username` = '" + Params[1] + "'");
                                    dbClient.RunQuery();
                                }
                                else
                                {
                                    dbClient.SetQuery("UPDATE `users` SET `rank` = '1' WHERE `username` = '" + Params[1] + "'");
                                    dbClient.RunQuery();
                                }
                            }
                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (TargetClient != null)
                            {
                                RoleplayManager.Shout(Session, "*Takes away Ambassador from: " + Params[1] + "*");
                                TargetClient.GetRoleplay().JobId = 1;
                                TargetClient.GetRoleplay().JobRank = 1;
                                TargetClient.GetRoleplay().SaveJobComponents();
                                Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, TargetClient.GetHabbo().UserName, "Rank", "Ambassador Rank Removed from User");
                                TargetClient.GetConnection().Dispose();
                                return true;
                            }
                            else
                            {
                                Session.SendNotif("This user was not found.");
                                return true;
                            }
                        }
                        return true;
                    }
                #endregion

                #region Give phone (:gphone x)
                case "gphone":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            if (!RoleplayManager.ParamsMet(Params, 1))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :gphone <user>");
                                return true;
                            }
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (TargetClient != null)
                            {
                                RoleplayManager.Shout(Session, "*Gives " + Params[1] + " a phone for FREE*");
                                TargetClient.GetRoleplay().Phone = 1;
                                TargetClient.GetRoleplay().SaveQuickStat("phone", "" + 1);
                                TargetClient.SendNotif("You just have been given a phone for FREE.");
                                return true;
                            }
                            else
                            {
                                Session.SendNotif("This user was not found");
                                return true;
                            }
                        }
                        return true;
                    }
                #endregion

                #region Give builder (:builder x)
                case "givebuilder":
                case "builder":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            if (!RoleplayManager.ParamsMet(Params, 1))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :builder <user>");
                                return true;
                            }
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("UPDATE `users` SET `rank` = '5' WHERE `username` = '" + Params[1] + "'");
                                dbClient.RunQuery();
                            }
                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (TargetClient != null)
                            {
                                RoleplayManager.Shout(Session, "*Gives " + Params[1] + " builder Rank*");
                                TargetClient.GetRoleplay().JobId = 11;
                                TargetClient.GetRoleplay().JobRank = 1;
                                TargetClient.GetRoleplay().SaveJobComponents();
                                RoleplayManager.Shout(Session, "*Superhires " + Params[1] + " a Builder*");
                                Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, TargetClient.GetHabbo().UserName, "Rank", "Builder Rank Given to User");
                                TargetClient.SendNotif("You just have been made a Builder, please reload the hotel.");
                                return true;
                            }
                            else
                            {
                                Session.SendNotif("This user was not found");
                                return true;
                            }
                        }
                        return true;
                    }
                #endregion

                #region Take Ambassador (:takeambas x)
                case "takebuilder":
                case "tbuilder":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            if (!RoleplayManager.ParamsMet(Params, 1))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :takebuilder <user>");
                                return true;
                            }
                            GameClient TargetClient = null;
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                            {
                                if (TargetClient.GetHabbo().VIP == true)
                                {
                                    dbClient.SetQuery("UPDATE `users` SET `rank` = '2' WHERE `username` = '" + Params[1] + "'");
                                    dbClient.RunQuery();
                                }
                                else
                                {
                                    dbClient.SetQuery("UPDATE `users` SET `rank` = '1' WHERE `username` = '" + Params[1] + "'");
                                    dbClient.RunQuery();
                                }
                            }
                            TargetClient = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                            if (TargetClient != null)
                            {
                                RoleplayManager.Shout(Session, "*Takes away Builder Rank from: " + Params[1] + "*");
                                TargetClient.GetRoleplay().JobId = 1;
                                TargetClient.GetRoleplay().JobRank = 1;
                                TargetClient.GetRoleplay().SaveJobComponents();
                                Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, TargetClient.GetHabbo().UserName, "Rank", "Builder Rank Removed from User");
                                TargetClient.GetConnection().Dispose();
                                return true;
                            }
                            else
                            {
                                Session.SendNotif("This user was not found.");
                                return true;
                            }
                        }
                        return true;
                    }
                #endregion

                #region Mass Badge (:massbadge)
                case "massbadge":
                    {
                        if (Session.GetHabbo().GotCommand("massbadge"))
                        {
                            if (Params.Length == 1)
                            {
                                Session.SendNotif("You must enter a badge code!");
                                return true;
                            }
                            Room room53 = Session.GetHabbo().CurrentRoom;
                            Plus.GetGame().GetClientManager().QueueBadgeUpdate(Params[1]);
                            Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, string.Empty, "Badge", "Mass badge with badge [" + Params[1] + "]");
                            new ServerMessage();
                            return true;//4D71;
                        }
                        return true;
                    }
                #endregion

                #region All eyes On Me (:alleyesonme)
                case "alleyesonme":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {
                            Room room35 = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
                            RoomUser user30 = room35.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                            lock (room35.GetRoomUserManager().GetRoomUsers())
                            {
                                foreach (RoomUser user31 in room35.GetRoomUserManager().GetRoomUsers())
                                {
                                    if (Session.GetHabbo().Id != user31.UserId)
                                    {
                                        user31.SetRot(Rotation.Calculate(user31.X, user31.Y, user30.X, user30.Y));
                                    }
                                }
                            }
                        }
                        return true;
                    }
                #endregion

                #region IP Ban (:ipban)
                case "ipban":
                case "banip":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_mod"))
                        {
                            String IPAddress = String.Empty;
                            string Username = Params[1];
                            string Reason = "No reason specified.";

                            if (Params.Length >= 2)
                            {
                                Reason = MergeParams(Params, 2);
                            }
                            else
                            {
                                Reason = "No reason specified.";
                            }

                            GameClient TargetUser = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1].ToString());
                            if (TargetUser == null)
                            {
                                Session.SendWhisper("An unknown error occured whilst finding this user!");
                                return true;
                            }
                            if (TargetUser.GetHabbo().Rank >= Session.GetHabbo().Rank)
                            {
                                Session.SendWhisper("You are not allowed to ban that user.");
                                return true;
                            }
                            try
                            {
                                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                                {
                                    dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = '" + TargetUser.GetHabbo().Id + "' LIMIT 1");
                                    IPAddress = dbClient.GetString();
                                }
                                RoleplayManager.Shout(Session, "*Uses their god-like powers to ip-ban '" + TargetUser.GetHabbo().UserName + "' for life*", 33);
                                if (!string.IsNullOrEmpty(IPAddress))
                                    Plus.GetGame().GetBanManager().BanUser(TargetUser, Session.GetHabbo().UserName, Support.ModerationBanType.IP, IPAddress, Reason, 360000000.0);
                                Plus.GetGame().GetBanManager().BanUser(TargetUser, Session.GetHabbo().UserName, Support.ModerationBanType.USERNAME, TargetUser.GetHabbo().UserName, Reason, 360000000.0);
                                string Message = Session.GetHabbo().UserName + " ip banned " + TargetUser.GetHabbo().UserName + " for life";
                                RoleplayManager.sendStaffAlert(Message, true);
                            }
                            catch (Exception e) { Console.WriteLine(e); }


                        }
                        return true;
                    }
                #endregion

                #region Machine Ban (:machineban)
                case "machineban":
                case "banmachine":
                case "mban":
                case "macban":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_admin"))
                        {
                            if (Params.Length == 1)
                            {
                                Session.SendNotif("You must include a username and reason!");
                            }
                            GameClient TargetUser = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1].ToString());
                            if (TargetUser == null)
                            {
                                Session.SendNotif("An unknown error occured whilst finding this user!");
                                return true;
                            }

                            if (string.IsNullOrWhiteSpace(TargetUser.MachineId))
                            {
                                Session.SendNotif("Unable to ban this user, they don't have a machine ID");
                                return true;
                            }

                            try
                            {
                                RoleplayManager.Shout(Session, "*Uses their god-like powers to machine ban (pc ban) " + TargetUser.GetHabbo().UserName + " for life*", 33);

                                Plus.GetGame().GetBanManager().BanUser(TargetUser, Session.GetHabbo().UserName, Support.ModerationBanType.USERNAME, TargetUser.GetHabbo().UserName, MergeParams(Params, 2), 360000000.0);

                                if (!string.IsNullOrEmpty(TargetUser.MachineId))
                                    Plus.GetGame().GetBanManager().BanUser(TargetUser, Session.GetHabbo().UserName, Support.ModerationBanType.MACHINE, TargetUser.MachineId, MergeParams(Params, 2), 360000000.0);

                                string Message = Session.GetHabbo().UserName + " machine banned " + TargetUser.GetHabbo().UserName + " for life";
                                RoleplayManager.sendStaffAlert(Message, true);
                            }
                            catch (Exception e) { Console.WriteLine(e); }


                        }
                        return true;
                    }
                #endregion

                #region Machine and IP ban (:mip)
                case "mip":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_admin"))
                        {
                            String IPAddress = String.Empty;

                            if (Params.Length == 1)
                            {
                                Session.SendNotif("You must include a username and reason!");
                            }
                            GameClient TargetUser = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1].ToString());
                            if (TargetUser == null)
                            {
                                Session.SendNotif("An unknown error occured whilst finding this user!");
                                return true;
                            }
                            try
                            {
                                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                                {
                                    dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = '" + TargetUser.GetHabbo().Id + "' LIMIT 1");
                                    IPAddress = dbClient.GetString();
                                }

                                if (string.IsNullOrWhiteSpace(TargetUser.MachineId))
                                {
                                    Session.SendNotif("Unable to ban this user, they don't have a machine ID");
                                    return true;
                                }
                                RoleplayManager.Shout(Session, "*Uses their god-like powers to ipban and machine ban (pc ban) " + TargetUser.GetHabbo().UserName + " for life*", 33);

                                if (!string.IsNullOrEmpty(IPAddress))
                                    Plus.GetGame().GetBanManager().BanUser(TargetUser, Session.GetHabbo().UserName, Support.ModerationBanType.IP, IPAddress, MergeParams(Params, 2), 360000000.0);
                                Plus.GetGame().GetBanManager().BanUser(TargetUser, Session.GetHabbo().UserName, Support.ModerationBanType.USERNAME, TargetUser.GetHabbo().UserName, MergeParams(Params, 2), 360000000.0);

                                if (!string.IsNullOrEmpty(TargetUser.MachineId))
                                    Plus.GetGame().GetBanManager().BanUser(TargetUser, Session.GetHabbo().UserName, Support.ModerationBanType.MACHINE, TargetUser.MachineId, MergeParams(Params, 2), 360000000.0);

                                string Message = Session.GetHabbo().UserName + " machine & ip banned " + TargetUser.GetHabbo().UserName + " for life";
                                RoleplayManager.sendStaffAlert(Message, true);
                            }
                            catch (Exception e) { Console.WriteLine(e); }
                        }
                        return true;
                    }
                #endregion

                #region All around me (:allaroundme)
                case "allaroundme":
                case "alltome":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {
                            Room TargetRoom = Session.GetHabbo().CurrentRoom;
                            if (Session.GetHabbo().CurrentRoom == null)
                            {
                                Session.SendNotif("An unknown error occured!");
                                return true;
                            }
                            Room Room = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);

                            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                            HashSet<RoomUser> users = Room.GetRoomUserManager().GetRoomUsers();
                            lock (users)
                            {
                                foreach (RoomUser Us in users)
                                {
                                    if (Session.GetHabbo().Id == Us.UserId)
                                        continue;
                                    Us.MoveTo(User.X, User.Y, true);
                                }
                            }

                            if (Params.Length == 2)
                            {
                                if (Params[1] == "override")
                                {
                                    foreach (RoomUser Us in users)
                                    {
                                        if (Session.GetHabbo().Id == Us.UserId)
                                            continue;
                                        Us.AllowOverride = true;
                                        Us.MoveTo(User.X, User.Y, true);
                                        Us.AllowOverride = false;
                                    }
                                }
                            }
                        }
                        return true;
                    }
                #endregion

                #region Hotel alert w/link (:hal)
                case "hal":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Room room56 = Session.GetHabbo().CurrentRoom;
                            string str21 = Params[1];
                            string str22 = MergeParams(Params, 2);

                            Plus.GetGame().GetClientManager().SendSuperNotif("Message from FluxRP - HAL", str22, "mercury_hgsmall_july", Session, str21, "Click for more information!", true, false);
                            Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, string.Empty, "HotelAlert Link", "Hotel alert Link [" + str21 + "]");
                        }
                        return true;
                    }
                #endregion

                #region Staff Alert (:sa)
                case "sa":
                case "sm":
                case "staffalert":
                    if (Session.GetHabbo().HasFuse("fuse_events"))
                    {
                        string message = "";
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :sa <message>");
                            return true;
                        }
                        else
                        {
                            message = "[" + Session.GetHabbo().UserName + "] " + MergeParams(Params, 1);
                        }

                        RoleplayManager.sendStaffAlert(message, false);

                        Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, string.Empty, "StaffAlert", "Staff alert [" + message + "]");
                    }
                    return true;
                #endregion

                #region Spectators Mode (:invisible)
                case "invisible":
                case "spec":
                case "spectatorsmode":
                case "invisivel":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {
                            if (Session.GetHabbo().SpectatorMode == false)
                            {
                                Session.GetHabbo().SpectatorMode = true;

                                Session.SendWhisper("Reload the room to be invisible.");

                            }
                            else
                            {
                                Session.GetHabbo().SpectatorMode = false;
                                Session.SendWhisper("Reload the room to be visible.");

                            }
                        }
                        return true;
                    }
                #endregion

                #region Unidle (:unidle)
                case "unidle":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_senior"))
                        {
                            Habbo Habbo = Plus.GetHabboForName(Params[1]);
                            if (Habbo == null)
                                return true;

                            RoomUser User = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Habbo.Id);

                            if (User == null)
                                return true;

                            User.UnIdle();
                        }
                        return true;
                    }
                #endregion

                #region Mass Action (:massact)
                case "massact":
                    {
                        if (RoleplayManager.BypassRights(Session))
                        {
                            try
                            {
                                Room room40 = Session.GetHabbo().CurrentRoom;
                                HashSet<RoomUser> list7 = room40.GetRoomUserManager().GetRoomUsers();
                                int action = short.Parse(Params[1]);
                                new ServerMessage();
                                foreach (RoomUser user37 in list7)
                                {
                                    if (user37 != null)
                                    {
                                        ServerMessage ActionMsg = new ServerMessage();
                                        ActionMsg.Init(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer"));
                                        ActionMsg.AppendInteger(user37.VirtualId);
                                        ActionMsg.AppendInteger(action);
                                        room40.SendMessage(ActionMsg);
                                    }
                                }
                            }
                            catch { }
                        }
                        return true;
                    }
                #endregion

                #region Blacklist (:blacklist)
                /*case "blacklist":
                    {
                        
                        #region Generate Instances / Sessions
                        string Target = Convert.ToString(Params[1]);
                        GameClient TargetSession = null;
                        TargetSession = Misc.GenerateSession(Target);
                        #endregion

                        #region Conditions
                        if (Session.GetHabbo().Rank <= 5)
                        {
                            Session.SendWhisper("Only a high ranking staff member can blacklist a user.");
                            return true;
                        }
                        if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                        {
                            Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                            return true;
                        }

                        #endregion

                        #region Execute

                        if (!TargetSession.GetRoleplay().Blacklist)
                        {
                            Session.Shout("*Uses their god-like powers to blacklist " + TargetSession.GetHabbo().UserName + "*");
                            TargetSession.GetRoleplay().Blacklist = true;
                            TargetSession.GetRoleplay().SaveQuickStat("blacklist", TargetSession.GetRoleplay().Blacklist + "");
                        }
                        else
                        {
                            Session.Shout("*Uses their god-like powers to un-blacklist " + TargetSession.GetHabbo().UserName + "*");
                            TargetSession.GetRoleplay().Blacklist = false;
                            TargetSession.GetRoleplay().SaveQuickStat("blacklist", TargetSession.GetRoleplay().Blacklist + "");
                        }
                        return true;

                        #endregion
                        
                        return true;
                    }*/
                #endregion

                #region Override (:override)
                case "override":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            Room currentRoom = Session.GetHabbo().CurrentRoom;
                            RoomUser roomUserByHabbo = null;
                            currentRoom = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
                            if (currentRoom != null)
                            {
                                roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                                if (roomUserByHabbo != null)
                                {
                                    if (roomUserByHabbo.AllowOverride)
                                    {
                                        roomUserByHabbo.AllowOverride = false;
                                        SendChatMessage(Session, "Override has been disabled!");
                                    }
                                    else
                                    {
                                        roomUserByHabbo.AllowOverride = true;
                                        SendChatMessage(Session, "Override has been enabled!");
                                    }
                                    currentRoom.GetGameMap().GenerateMaps(true);
                                }
                            }

                        }
                        return true;
                    }
                #endregion

                #region Roomalert (:roomalert)
                case "roomalert":
                case "ra":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_events"))
                        {

                            string Alert = MergeParams(Params, 1);

                            lock (Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            {
                                foreach (RoomUser user in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                                {
                                    if (user == null)
                                        continue;
                                    if (user.IsBot)
                                        continue;
                                    if (user.GetClient() == null)
                                        continue;

                                    user.GetClient().SendNotif(Alert + "\n\n- " + Session.GetHabbo().UserName);
                                }
                            }
                        }
                    }
                    return true;
                #endregion

                #region Roomalertsec (:roomalertsec)
                case "roomalertsec":
                    {
                        if (Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            string Alert = MergeParams(Params, 1);

                            lock (Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            {
                                foreach (RoomUser user in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                                {
                                    if (user == null)
                                        continue;
                                    if (user.GetClient() == null)
                                        continue;
                                    if (user.IsBot)
                                        continue;

                                    user.GetClient().SendNotif(Alert);
                                }
                            }
                        }
                    }
                    return true;
                    #endregion


                    #endregion

                    #endregion

                    #endregion

            }
            return false;
            #endregion

        }

        #region Extra Methods / Additional Parsers
        public static Boolean ParseColourWars(GameClient Session, string Input)
        {
            Input = Input.Substring(1);
            string[] Params = Input.Split(' ');

            switch (Params[0].ToLower())
            {


                #region Foton Minigame Commands

                #region :cwremove
                case "cwremove":
                case "cwremover":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            Session.SendWhisper("You must be event staff to use this command!");
                            return true;
                        }
                        GameClient Target = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                        if (Target == null)
                            return true;
                        Session.Shout("*Usa seus poderes divinos para remover " + Target.GetHabbo().UserName + " do jogo*");
                        ColourManager.RemovePlayerFromTeam(Target, Target.GetRoleplay().ColourWarTeam, true, "Você foi chutado por um administrador!");
                        return true;
                    }
                #endregion

                #region :wha <msg>

                case "wha":
                case "walert":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return false;
                        }

                        string Notice = MergeParams(Params, 1);

                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient mClient in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (mClient == null)
                                    continue;
                                if (mClient.GetHabbo() == null)
                                    continue;
                                if (mClient.GetHabbo().CurrentRoom == null)
                                    continue;
                                if (mClient.GetConnection() == null)
                                    continue;
                                mClient.SendWhisperBubble("[Hotel Alert][" + Session.GetHabbo().UserName + "]: " + Notice, 33);
                            }
                        }

                        return true;
                    }

                #endregion

                #region :ha <msg>
                case "ha":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return false;
                        }

                        string Notice = MergeParams(Params, 1);
                        ServerMessage HotelAlert = new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
                        HotelAlert.AppendString(Notice + "\r\n" + "- " + Session.GetHabbo().UserName);


                        Plus.GetGame().GetClientManager().QueueBroadcaseMessage(HotelAlert);

                        Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, string.Empty, "HotelAlert", "Hotel alert [" + Notice + "]");
                    }
                    return true;
                #endregion

                #region :sa <msg>
                case "sa":
                case "sm":
                case "staffalert":
                    if (Session.GetHabbo().HasFuse("fuse_events"))
                    {
                        string message = "";
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :sa <message>");
                            return true;
                        }
                        else
                        {
                            message = "[" + Session.GetHabbo().UserName + "] " + MergeParams(Params, 1);
                        }

                        RoleplayManager.sendStaffAlert(message, false);

                        Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, string.Empty, "StaffAlert", "Staff alert [" + message + "]");
                    }
                    return true;
                #endregion

                #region :pull x
                case "pull":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_vip"))
                        {
                            Session.SendWhisper("You must have VIP to do this!");
                            return true;
                        }

                        Room room = Session.GetHabbo().CurrentRoom;
                        if (room == null)
                        {
                            return true;
                        }


                        RoomUser roomuser = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                        if (roomuser == null)
                        {
                            return true;
                        }
                        if (Params.Length == 1)
                        {
                            SendChatMessage(Session, "Unable to find user!");
                            return true;
                        }

                        GameClient Target = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                        if (Target == null)
                            return true;

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("pull_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("pull_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["pull_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["pull_cooldown"] + "/5]");
                            return true;
                        }


                        if (Target.GetHabbo().Id == Session.GetHabbo().Id)
                        {
                            SendChatMessage(Session, "You cannot pull yourself!");
                            return true;
                        }
                        RoomUser TargetUser = room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Id);
                        if (TargetUser == null)
                            return true;


                        if (TargetUser.TeleportEnabled)
                            return true;

                        Session.GetRoleplay().MultiCoolDown["pull_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        if (!((Math.Abs((int)(roomuser.X - TargetUser.X)) >= 3) || (Math.Abs((int)(roomuser.Y - TargetUser.Y)) >= 3)))
                        {
                            Session.Shout("*pulls " + Params[1] + " [-5E]*");
                            Session.GetRoleplay().Energy -= 5;
                            if (roomuser.RotBody % 2 != 0)
                                roomuser.RotBody--;

                            if (roomuser.RotBody == 0)
                                TargetUser.MoveTo(roomuser.X, roomuser.Y - 1);
                            else if (roomuser.RotBody == 2)
                                TargetUser.MoveTo(roomuser.X + 1, roomuser.Y);
                            else if (roomuser.RotBody == 4)
                                TargetUser.MoveTo(roomuser.X, roomuser.Y + 1);
                            else if (roomuser.RotBody == 6)
                                TargetUser.MoveTo(roomuser.X - 1, roomuser.Y);

                        }
                        else
                        {
                            SendChatMessage(Session, "This user is too far away!");
                            return true;
                        }

                        break;
                    }

                #endregion

                #region :push x
                case "push":
                    {
                        Room TargetRoom;
                        RoomUser TargetRoomUser;
                        RoomUser TargetRoomUser1;
                        TargetRoom = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);

                        if (TargetRoom == null)
                        {
                            return true;
                        }

                        if (Params.Length == 1)
                        {
                            SendChatMessage(Session, "Sintaxe de comando inválida: :push <user>");
                            return true;
                        }

                        TargetRoomUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Convert.ToString(Params[1]));

                        if (TargetRoomUser == null)
                        {
                            SendChatMessage(Session, "Could not find that user!");
                            return true;
                        }

                        if (TargetRoomUser.GetUserName() == Session.GetHabbo().UserName)
                        {
                            SendChatMessage(Session, "Come on, surely you don't want to push yourself!");
                            return true;
                        }

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("push_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("push_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["push_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["push_cooldown"] + "/3]");
                            return true;
                        }

                        TargetRoomUser1 = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                        if (TargetRoomUser1 == null || TargetRoomUser.TeleportEnabled)
                        {
                            return true;
                        }

                        Session.GetRoleplay().MultiCoolDown["push_cooldown"] = 3;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        //if ((TargetRoomUser.X == TargetRoomUser1.X - 1) || (TargetRoomUser.X == TargetRoomUser1.X + 1) || (TargetRoomUser.Y == TargetRoomUser1.Y - 1) || (TargetRoomUser.Y == TargetRoomUser1.Y + 1))
                        if (!((Math.Abs((int)(TargetRoomUser.X - TargetRoomUser1.X)) >= 2) || (Math.Abs((int)(TargetRoomUser.Y - TargetRoomUser1.Y)) >= 2)))
                        {
                            if (TargetRoomUser1.RotBody == 4)
                            { TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 1); }

                            if (TargetRoomUser1.RotBody == 0)
                            { TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 1); }

                            if (TargetRoomUser1.RotBody == 6)
                            { TargetRoomUser.MoveTo(TargetRoomUser.X - 1, TargetRoomUser.Y); }

                            if (TargetRoomUser1.RotBody == 2)
                            { TargetRoomUser.MoveTo(TargetRoomUser.X + 1, TargetRoomUser.Y); }

                            if (TargetRoomUser1.RotBody == 3)
                            {
                                TargetRoomUser.MoveTo(TargetRoomUser.X + 1, TargetRoomUser.Y);
                                TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 1);
                            }

                            if (TargetRoomUser1.RotBody == 1)
                            {
                                TargetRoomUser.MoveTo(TargetRoomUser.X + 1, TargetRoomUser.Y);
                                TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 1);
                            }

                            if (TargetRoomUser1.RotBody == 7)
                            {
                                TargetRoomUser.MoveTo(TargetRoomUser.X - 1, TargetRoomUser.Y);
                                TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 1);
                            }

                            if (TargetRoomUser1.RotBody == 5)
                            {
                                TargetRoomUser.MoveTo(TargetRoomUser.X - 1, TargetRoomUser.Y);
                                TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 1);
                            }

                            TargetRoomUser.UpdateNeeded = true;
                            TargetRoomUser1.UpdateNeeded = true;
                            TargetRoomUser1.SetRot(Rotation.Calculate(TargetRoomUser1.X, TargetRoomUser1.Y, TargetRoomUser.GoalX, TargetRoomUser.GoalY));
                            Session.Shout("*Pushes " + TargetRoomUser.GetClient().GetHabbo().UserName + " [-5E]*");
                            Session.GetRoleplay().Energy -= 5;
                        }
                        else
                        {
                            SendChatMessage(Session, Params[1] + " is not close enough.");
                        }

                        return true;
                    }
                #endregion

                #region :setprize
                case "setprize":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("Sorry but you aren't an events staff!");
                            return true;
                        }

                        ColourManager.Prize = Convert.ToInt32(Params[1]);
                        Session.SendWhisper("Color wars prize set to " + ColourManager.Prize + " coins!");
                        return true;
                    }
                #endregion

                #region :hit x
                case "hit":
                    {
                        #region Punching
                        #region Generate Instances / Sessions

                        bool bypass = false;
                        GameClient TargetSession = null;
                        if (Session.GetRoleplay().LastHit != null)
                        {
                            TargetSession = Session.GetRoleplay().LastHit;
                        }

                        if (!bypass)
                        {
                            if (!RoleplayManager.ParamsMet(Params, 1))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :hit <user>");
                                return true;
                            }

                            string Target = Convert.ToString(Params[1]);
                            RoomUser T = null;

                            foreach (RoomUser User in Session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values)
                            {
                                if (User.IsBot && User.BotData.Name.ToLower() == Target.ToLower())
                                    T = User;
                            }

                            if (T != null)
                            {
                                HabboHotel.Roleplay.Combat.HandCombat.ExecuteAttackBot(Session, T, T.PetData, T.BotData);
                                return true;
                            }

                            TargetSession = RoleplayManager.GenerateSession(Target);

                            if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                            {
                                Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                                return true;
                            }

                            if (TargetSession.GetRoleplay().StaffDuty == true && !RoleplayManager.BypassRights(Session))
                            {
                                Session.SendWhisper("You cannot hit a staff that is on duty!");
                                return true;
                            }
                            else
                            {
                                Session.GetRoleplay().LastHit = TargetSession;
                                Session.GetRoleplay().ActionLast = "hit";
                            }
                        }
                        #endregion

                        #region Execute
                        HabboHotel.Roleplay.Combat.HandCombat.ExecuteAttack(Session, TargetSession);
                        #endregion
                        #endregion
                        return true;
                    }
                #endregion

                #region :leavegame
                case "leavegame":
                    {
                        ColourManager.RemovePlayerFromTeam(Session, Session.GetRoleplay().ColourWarTeam, true);
                        return true;
                    }
                #endregion

                #region :gameinfo


                #endregion

                #region :forcewin
                case "forcewin":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }




                        if (!ColourManager.Started)
                        {
                            Session.SendWhisper("There is currently not a color wars game running!");
                            return true;
                        }

                        lock (ColourManager.Teams.Values)
                        {
                            foreach (Team team in ColourManager.Teams.Values)
                            {
                                if (team != Session.GetRoleplay().ColourWarTeam)
                                {
                                    ColourManager.EliminateTeam(team);
                                }
                                //
                            }
                        }



                        return true;
                    }
                #endregion

                #region :forcestop
                case "forcestop":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            Session.SendWhisper("You must be event staff to use this command!");
                            return true;
                        }
                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {

                                if (client == null)
                                    continue;

                                if (client.GetHabbo() == null)
                                    continue;

                                if (client.GetHabbo().CurrentRoom == null)
                                    continue;

                                if (!client.GetHabbo().CurrentRoom.RoomData.Name.Contains("[CW]"))
                                {
                                    continue;
                                }

                                if (client.GetRoleplay() != null)
                                {
                                    if (client.GetRoleplay().inColourWars)
                                    {
                                        ColourManager.RemovePlayerFromTeam(client, client.GetRoleplay().ColourWarTeam, false, "An admin has stopped the game!");
                                    }
                                }


                                client.GetRoleplay().Transport(ColourManager.MainLobby, 10);

                            }
                        }

                        ColourManager.EndGame();
                        Session.Shout("*Uses their god-like powers to end the current color wars game*");

                        return true;
                    }
                #endregion

                #region :forcestart
                case "forcestart":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            Session.SendWhisper("You must be event staff to use this command!");
                            return true;
                        }

                        ColourManager.TryStart(true);
                        Session.Shout("*Uses their god-like powers to force start a color wars game*");

                        return true;
                    }

                #endregion

                #region :teamalert

                case "ta":
                    {
                        string TeamAlert = "";
                        TeamAlert = MergeParams(Params, 1);
                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (client == null)
                                    continue;

                                if (client.GetRoleplay() == null)
                                    continue;

                                if (!client.GetRoleplay().inColourWars)
                                    continue;

                                if (client.GetRoleplay().ColourWarTeam == null)
                                    continue;

                                if (client.GetRoleplay().ColourWarTeam == Session.GetRoleplay().ColourWarTeam)
                                {
                                    client.SendWhisper("[TEAM Alert][" + Session.GetHabbo().UserName + "]: " + TeamAlert);
                                }
                            }
                        }
                        return true;
                    }

                #endregion

                #region :addtoteam <username> <team>
                case "addtoteam":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            Session.SendWhisper("You must be event staff to use this command!");
                            return true;
                        }


                        string Username = null;
                        string Color = null;
                        GameClient TargetSession;

                        if (!RoleplayManager.ParamsMet(Params, 2))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :addtoteam <user> <teamcolor>");
                            return true;
                        }


                        Username = Params[1];
                        Color = Params[2];

                        if (!ColourManager.Teams.ContainsKey(Color.ToLower()))
                        {
                            Session.SendWhisper("The team called '" + Color.ToLower() + "' does not exist!");
                        }

                        TargetSession = RoleplayManager.GenerateSession(Username.ToLower());

                        if (TargetSession == null)
                        {
                            Session.SendWhisper("This user does not exist or is not online!");
                            return true;
                        }

                        if (TargetSession.GetHabbo() == null)
                        {
                            Session.SendWhisper("This user does not exist or is not online!");
                            return true;
                        }

                        if (TargetSession.GetHabbo().CurrentRoom == null)
                        {
                            Session.SendWhisper("This user does not exist or is not online!");
                            return true;
                        }

                        if (TargetSession.GetRoleplay().inColourWars)
                        {
                            Session.SendWhisper("This user is already playing color wars!");
                            return true;
                        }

                        if (ColourManager.Teams[Color.ToLower()].Players.ContainsKey(TargetSession))
                        {
                            Session.SendWhisper("This user is already in this team!");
                            return true;
                        }




                        Session.Shout("*Uses their god-like powers to add " + TargetSession.GetHabbo().UserName + " to the Color Wars " + Color.ToUpper() + " Team*");

                        ColourManager.ForceAddPlayerToTeam(TargetSession, Color.ToLower());

                        return true;
                    }

                #endregion

                #region :removefromteam <username> <team>
                case "removefromteam":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            Session.SendWhisper("You must be event staff to use this command!");
                            return true;
                        }


                        return true;
                    }
                #endregion

                #region :capture
                case "capture":
                    {
                        try
                        {
                            RoomUser Roomuser = Session.GetHabbo().GetRoomUser();
                            Team Team = ColourManager.GetTeamByBase(Roomuser);

                            if (Team != null)
                            {
                                if (Team == Session.GetRoleplay().ColourWarTeam)
                                {
                                    Session.SendWhisper("Oi, retard, this is your base!");
                                    return true;
                                }

                                if (Team.KnockedOut)
                                {
                                    Session.SendWhisper("This base has already been captured!");
                                    return true;
                                }

                                if (Team.BeingCaptured)
                                {
                                    Session.SendWhisper("This base is already being captured!");
                                    return true;
                                }

                                if (Team.KnockedOut)
                                {
                                    Session.SendWhisper("This team has already been eliminated from the game!");
                                    return true;
                                }
                                // start capturing
                                Team.BeingCaptured = true;
                                ColourManager.MessageTeam("[COLOR WARS] Your base is being captured!", Team);
                                Roomuser.ApplyEffect(59);
                                RoleplayManager.Shout(Session, "*Starts capturing the base from the " + Team.Colour.ToLower() + " team*");

                                new CaptureTimer(Session, Team);
                            }
                        }
                        catch (Exception ex) { Logging.LogPacketException(":capture", ex.Message); }
                        return true;
                    }
                    #endregion

                    #endregion

            }

            return false;
        }
        public static Boolean ParseMafiaWars(GameClient Session, string Input)
        {
            Input = Input.Substring(1);
            string[] Params = Input.Split(' ');

            switch (Params[0].ToLower())
            {


                #region Foton Minigame Commands

                #region :forcestart
                case "forcestart":
                case "fcomecar":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            Session.SendWhisper("You must be event staff to use this command!");
                            return true;
                        }

                        Plus.GetGame().MafiaWars.StartGame(true);
                        Session.Shout("*Usa seus poderes divinos para forçar o início de um jogo de guerra da máfia*");

                        return true;
                    }

                #endregion

                #region :wha <msg>

                case "wha":
                case "walert":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return false;
                        }

                        string Notice = MergeParams(Params, 1);

                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient mClient in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (mClient == null)
                                    continue;
                                if (mClient.GetHabbo() == null)
                                    continue;
                                if (mClient.GetHabbo().CurrentRoom == null)
                                    continue;
                                if (mClient.GetConnection() == null)
                                    continue;
                                mClient.SendWhisperBubble("[Hotel Alert][" + Session.GetHabbo().UserName + "]: " + Notice, 33);
                            }
                        }

                        return true;
                    }

                #endregion

                #region :ha <msg>
                case "ha":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            return false;
                        }

                        string Notice = MergeParams(Params, 1);
                        ServerMessage HotelAlert = new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
                        HotelAlert.AppendString(Notice + "\r\n" + "- " + Session.GetHabbo().UserName);


                        Plus.GetGame().GetClientManager().QueueBroadcaseMessage(HotelAlert);

                        Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, string.Empty, "HotelAlert", "Hotel alert [" + Notice + "]");
                    }
                    return true;
                #endregion

                #region :sa <msg>
                case "sa":
                case "sm":
                case "staffalert":
                    if (Session.GetHabbo().HasFuse("fuse_events"))
                    {
                        string message = "";
                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Sintaxe de comando inválida: :sa <message>");
                            return true;
                        }
                        else
                        {
                            message = "[" + Session.GetHabbo().UserName + "] " + MergeParams(Params, 1);
                        }

                        RoleplayManager.sendStaffAlert(message, false);

                        Plus.GetGame().GetModerationTool().LogStaffEntry(Session.GetHabbo().UserName, string.Empty, "StaffAlert", "Staff alert [" + message + "]");
                    }
                    return true;
                #endregion

                #region :pull x
                case "pull":
                    {

                        if (!Session.GetHabbo().HasFuse("fuse_vip"))
                        {
                            Session.SendWhisper("You must have VIP to do this!");
                            return true;
                        }

                        Room room = Session.GetHabbo().CurrentRoom;
                        if (room == null)
                        {
                            return true;
                        }


                        RoomUser roomuser = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                        if (roomuser == null)
                        {
                            return true;
                        }
                        if (Params.Length == 1)
                        {
                            SendChatMessage(Session, "Unable to find user!");
                            return true;
                        }

                        GameClient Target = Plus.GetGame().GetClientManager().GetClientByUserName(Params[1]);
                        if (Target == null)
                            return true;

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("pull_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("pull_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["pull_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["pull_cooldown"] + "/5]");
                            return true;
                        }


                        if (Target.GetHabbo().Id == Session.GetHabbo().Id)
                        {
                            SendChatMessage(Session, "You cannot pull yourself!");
                            return true;
                        }
                        RoomUser TargetUser = room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Id);
                        if (TargetUser == null)
                            return true;


                        if (TargetUser.TeleportEnabled)
                            return true;

                        Session.GetRoleplay().MultiCoolDown["pull_cooldown"] = 5;
                        Session.GetRoleplay().CheckingMultiCooldown = true;

                        if (!((Math.Abs((int)(roomuser.X - TargetUser.X)) >= 3) || (Math.Abs((int)(roomuser.Y - TargetUser.Y)) >= 3)))
                        {
                            Session.Shout("*pulls " + Params[1] + " [-5E]*");
                            Session.GetRoleplay().Energy -= 5;
                            if (roomuser.RotBody % 2 != 0)
                                roomuser.RotBody--;

                            if (roomuser.RotBody == 0)
                                TargetUser.MoveTo(roomuser.X, roomuser.Y - 1);
                            else if (roomuser.RotBody == 2)
                                TargetUser.MoveTo(roomuser.X + 1, roomuser.Y);
                            else if (roomuser.RotBody == 4)
                                TargetUser.MoveTo(roomuser.X, roomuser.Y + 1);
                            else if (roomuser.RotBody == 6)
                                TargetUser.MoveTo(roomuser.X - 1, roomuser.Y);

                        }
                        else
                        {
                            SendChatMessage(Session, "This user is too far away!");
                            return true;
                        }

                        break;
                    }

                #endregion

                #region :push x
                case "push":
                    {
                        Room TargetRoom;
                        RoomUser TargetRoomUser;
                        RoomUser TargetRoomUser1;
                        TargetRoom = Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);

                        if (TargetRoom == null)
                        {
                            return true;
                        }

                        if (Params.Length == 1)
                        {
                            SendChatMessage(Session, "Sintaxe de comando inválida: :push <user>");
                            return true;
                        }

                        TargetRoomUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Convert.ToString(Params[1]));

                        if (TargetRoomUser == null)
                        {
                            SendChatMessage(Session, "Could not find that user!");
                            return true;
                        }

                        if (TargetRoomUser.GetUserName() == Session.GetHabbo().UserName)
                        {
                            SendChatMessage(Session, "Come on, surely you don't want to push yourself!");
                            return true;
                        }

                        if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("push_cooldown"))
                        {
                            Session.GetRoleplay().MultiCoolDown.Add("push_cooldown", 0);
                        }
                        if (Session.GetRoleplay().MultiCoolDown["push_cooldown"] > 0)
                        {
                            Session.SendWhisper("Cooldown [" + Session.GetRoleplay().MultiCoolDown["push_cooldown"] + "/3]");
                            return true;
                        }

                        TargetRoomUser1 = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                        if (TargetRoomUser1 == null || TargetRoomUser.TeleportEnabled)
                        {
                            return true;
                        }

                        Session.GetRoleplay().MultiCoolDown["push_cooldown"] = 3;
                        Session.GetRoleplay().CheckingMultiCooldown = true;
                        //if ((TargetRoomUser.X == TargetRoomUser1.X - 1) || (TargetRoomUser.X == TargetRoomUser1.X + 1) || (TargetRoomUser.Y == TargetRoomUser1.Y - 1) || (TargetRoomUser.Y == TargetRoomUser1.Y + 1))
                        if (!((Math.Abs((int)(TargetRoomUser.X - TargetRoomUser1.X)) >= 2) || (Math.Abs((int)(TargetRoomUser.Y - TargetRoomUser1.Y)) >= 2)))
                        {
                            if (TargetRoomUser1.RotBody == 4)
                            { TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 1); }

                            if (TargetRoomUser1.RotBody == 0)
                            { TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 1); }

                            if (TargetRoomUser1.RotBody == 6)
                            { TargetRoomUser.MoveTo(TargetRoomUser.X - 1, TargetRoomUser.Y); }

                            if (TargetRoomUser1.RotBody == 2)
                            { TargetRoomUser.MoveTo(TargetRoomUser.X + 1, TargetRoomUser.Y); }

                            if (TargetRoomUser1.RotBody == 3)
                            {
                                TargetRoomUser.MoveTo(TargetRoomUser.X + 1, TargetRoomUser.Y);
                                TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 1);
                            }

                            if (TargetRoomUser1.RotBody == 1)
                            {
                                TargetRoomUser.MoveTo(TargetRoomUser.X + 1, TargetRoomUser.Y);
                                TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 1);
                            }

                            if (TargetRoomUser1.RotBody == 7)
                            {
                                TargetRoomUser.MoveTo(TargetRoomUser.X - 1, TargetRoomUser.Y);
                                TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y - 1);
                            }

                            if (TargetRoomUser1.RotBody == 5)
                            {
                                TargetRoomUser.MoveTo(TargetRoomUser.X - 1, TargetRoomUser.Y);
                                TargetRoomUser.MoveTo(TargetRoomUser.X, TargetRoomUser.Y + 1);
                            }

                            TargetRoomUser.UpdateNeeded = true;
                            TargetRoomUser1.UpdateNeeded = true;
                            TargetRoomUser1.SetRot(Rotation.Calculate(TargetRoomUser1.X, TargetRoomUser1.Y, TargetRoomUser.GoalX, TargetRoomUser.GoalY));
                            Session.Shout("*Pushes " + TargetRoomUser.GetClient().GetHabbo().UserName + " [-5E]*");
                            Session.GetRoleplay().Energy -= 5;
                        }
                        else
                        {
                            SendChatMessage(Session, Params[1] + " is not close enough.");
                        }

                        return true;
                    }
                #endregion

                #region :setprize
                case "setprize":
                    {
                        if (!Session.GetHabbo().HasFuse("fuse_events"))
                        {
                            return true;
                        }
                        if (Session.GetHabbo().HasFuse("fuse_builder") && !Session.GetHabbo().HasFuse("fuse_manager"))
                        {
                            Session.SendWhisper("Sorry but you aren't an events staff!");
                            return true;
                        }

                        Plus.GetGame().MafiaWars.Prize = Convert.ToInt32(Params[1]);
                        RoleplayManager.Shout(Session, "Mafia wars prize set to " + Plus.GetGame().MafiaWars.Prize + " coins!", 33);
                        return true;
                    }
                #endregion

                #region :hit x
                case "hit":
                    {
                        #region Punching
                        #region Generate Instances / Sessions

                        bool bypass = false;
                        GameClient TargetSession = null;
                        if (Session.GetRoleplay().LastHit != null)
                        {
                            TargetSession = Session.GetRoleplay().LastHit;
                        }

                        if (!bypass)
                        {
                            if (!RoleplayManager.ParamsMet(Params, 1))
                            {
                                Session.SendWhisper("Sintaxe de comando inválida: :hit <user>");
                                return true;
                            }

                            string Target = Convert.ToString(Params[1]);
                            RoomUser T = null;

                            foreach (RoomUser User in Session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values)
                            {
                                if (User.IsBot && User.BotData.Name.ToLower() == Target.ToLower())
                                    T = User;
                            }

                            if (T != null)
                            {
                                HabboHotel.Roleplay.Combat.HandCombat.ExecuteAttackBot(Session, T, T.PetData, T.BotData);
                                return true;
                            }

                            TargetSession = RoleplayManager.GenerateSession(Target);

                            if (TargetSession == null || TargetSession.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
                            {
                                Session.SendWhisper("Este usuário não foi encontrado nesta sala.");
                                return true;
                            }

                            if (TargetSession.GetRoleplay().StaffDuty == true && !RoleplayManager.BypassRights(Session))
                            {
                                Session.SendWhisper("You cannot hit a staff that is on duty!");
                                return true;
                            }
                            else
                            {
                                Session.GetRoleplay().LastHit = TargetSession;
                                Session.GetRoleplay().ActionLast = "hit";
                            }
                        }
                        #endregion

                        #region Execute
                        HabboHotel.Roleplay.Combat.HandCombat.ExecuteAttack(Session, TargetSession);
                        #endregion
                        #endregion
                        return true;
                    }
                #endregion

                #region :leavegame
                case "leavegame":
                    {
                        Plus.GetGame().MafiaWars.RemoveUserFromGame(Session, Session.GetRoleplay().TeamString, true);
                        return true;
                    }
                    #endregion

                    #endregion

            }

            return false;
        }
        public static Boolean Parse2(GameClient Session, string Input)
        {
            if (!Input.StartsWith("#"))
            {
                return false;
            }

            Input = Input.Substring(1);
            string[] Params = Input.Split(' ');

            #region Commands
            switch (Params[0].ToLower())
            {
                #region #accept
                case "accept":
                    {

                        if (Session.GetRoleplay().OfferData.Count <= 0 && !Session.GetRoleplay().WeaponOffered && !Session.GetRoleplay().marryReq)
                        {
                            Session.SendWhisper("Atualmente você não tem ofertas!");
                            return true;
                        }

                        #region Offer Pet
                        if (Session.GetRoleplay().OfferData.ContainsKey("pet"))
                        {
                            int Price = Session.GetRoleplay().OfferData["pet"].OfferPrice;

                            if (Session.GetHabbo().Credits < Price)
                            {
                                Session.SendWhisper("You do not have $" + Price + " to purchase this pet!");
                                Session.GetRoleplay().OfferData.Remove("pet");
                                return true;
                            }

                            string[] PetData = Session.GetRoleplay().OfferData["pet"].Offering.Split(':');
                            string PetName = PetData[0];
                            string PetType = PetData[1];

                            if (!RoleplayManager.CreateQuickPet(Session, PetType, PetName))
                            {
                                Session.SendWhisper("An error occured and the offer has been canceled, sorry!");
                                Session.GetRoleplay().OfferData.Remove("pet");
                                return true;
                            }

                            Session.Shout("*Purchases a pet " + PetType.ToUpper() + " called '" + PetName.ToLower() + "' [$-" + Price + "]*");

                            return true;
                        }
                        #endregion

                        #region Offer Drink
                        if (Session.GetRoleplay().OfferData.ContainsKey("drink"))
                        {
                            int Price = Session.GetRoleplay().OfferData["drink"].OfferPrice;

                            if (Session.GetHabbo().Credits < Price)
                            {
                                Session.SendWhisper("Você não tem $" + Price + " para aceitar esta oferta!");
                                Session.GetRoleplay().OfferData.Remove("drink");
                                return true;
                            }

                            Food Drink =
                                Substances.GetDrinkById(Convert.ToUInt32(
                                RoleplayManager.GetDrinkFromOffer(Session.GetRoleplay().OfferData["drink"].Offering)));

                            RoleplayManager.Shout(Session, "*Aceita a oferta e compra um " + Drink.DisplayName + " por $" + Price + " [+" + Drink.Energy + " Energia][-$" + Drink.Item_Price + "]*");
                            RoleplayManager.GiveMoney(Session, -Price);
                            Session.GetRoleplay().Energy += Drink.Energy;

                            switch (Drink.UniqueName.ToLower())
                            {
                                case "wine":
                                    #region Wine
                                    ProcessDrunk DrunkUser = new ProcessDrunk(Session);
                                    #endregion
                                    break;

                                default:

                                    break;
                            }

                            Session.GetHabbo().GetRoomUser().CarryItem(24);
                            Session.GetRoleplay().OfferData.Remove("bbj");


                            return true;
                        }
                        #endregion

                        #region Offer BBJ
                        if (Session.GetRoleplay().OfferData.ContainsKey("bbj"))
                        {
                            int Price = Session.GetRoleplay().OfferData["bbj"].OfferPrice;

                            if (Session.GetHabbo().Credits < Price)
                            {
                                Session.SendWhisper("You do not have $" + Price + " to accept this offer!");
                                Session.GetRoleplay().OfferData.Remove("bbj");
                                return true;
                            }

                            RoleplayManager.Shout(Session, "*Accepts the offer and purchases an BubbleJuice Cola for $" + Price + " [+" + Substances.SubstanceData["md_limukaappi"].Energy + " Energy][-$" + Substances.SubstanceData["md_limukaappi"].Item_Price + "]*");
                            RoleplayManager.GiveMoney(Session, -Price);
                            Session.GetRoleplay().Energy += Substances.SubstanceData["md_limukaappi"].Energy;
                            Session.GetHabbo().GetRoomUser().CarryItem(24);
                            Session.GetRoleplay().OfferData.Remove("bbj");


                            return true;
                        }
                        #endregion

                        #region Offer Icecream
                        if (Session.GetRoleplay().OfferData.ContainsKey("icecream"))
                        {
                            int Price = Session.GetRoleplay().OfferData["icecream"].OfferPrice;
                            if (Session.GetHabbo().Credits < Price)
                            {
                                Session.SendWhisper("You do not have $" + Price + " to accept this offer!");
                                Session.GetRoleplay().OfferData.Remove("icecream");
                                return true;
                            }

                            RoleplayManager.Shout(Session, "*Accepts the offer and purchases a Vanilla Icecream Cola for $" + Price + " [+" + Substances.SubstanceData["rare_icecream*1"].Energy + " Energy][-$" + Substances.SubstanceData["rare_icecream*1"].Item_Price + "]*");
                            RoleplayManager.GiveMoney(Session, -Price);
                            Session.GetRoleplay().Energy += Substances.SubstanceData["rare_icecream*1"].Energy;
                            Session.GetHabbo().GetRoomUser().CarryItem(24);
                            Session.GetRoleplay().OfferData.Remove("icecream");
                            return true;
                        }
                        #endregion

                        #region Offer Phone
                        if (Session.GetRoleplay().OfferData.ContainsKey("phone"))
                        {
                            int Price = Session.GetRoleplay().OfferData["phone"].OfferPrice;
                            GameClient Offerer = Session.GetRoleplay().OfferData["phone"].OfferUser;

                            if (Session.GetHabbo().Credits < Price)
                            {
                                Session.SendWhisper("You do not have $" + Price + " to accept this offer!");
                                Session.GetRoleplay().OfferData.Remove("phone");
                                return true;
                            }

                            if (Offerer == null && !Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("PHONE"))
                            {
                                Session.SendWhisper("The person who offered you this item is offline, hence the offer has been canceled!");
                                Session.GetRoleplay().OfferData.Remove("phone");
                                return true;
                            }

                            RoleplayManager.Shout(Session, "*Accepts the offer and purchases a Nokia for $" + Price + " [-$" + Price + "]*");
                            Session.GetRoleplay().Phone = 1;
                            Session.GetRoleplay().SaveQuickStat("phone", "" + 1);
                            RoleplayManager.GiveMoney(Session, -Price);
                            Session.GetRoleplay().OfferData.Remove("phone");
                            return true;
                        }
                        #endregion

                        #region Marriage Request
                        if (Session.GetRoleplay().marryReq && Session.GetRoleplay().marryReqer != 0)
                        {

                            GameClient TargetSession = null;
                            TargetSession = RoleplayManager.GenerateSession(Session.GetRoleplay().marryReqer);

                            RoleplayManager.Shout(Session, "*Yes yes yes, baby! I want to marry you!*", 17);

                            Session.GetRoleplay().Married_To = Convert.ToInt32(Session.GetRoleplay().marryReqer);
                            Session.GetRoleplay().SaveQuickStat("married_to", "" + Session.GetRoleplay().Married_To);

                            TargetSession.GetRoleplay().Married_To = Convert.ToInt32(Session.GetHabbo().Id);
                            TargetSession.GetRoleplay().SaveQuickStat("married_to", "" + TargetSession.GetRoleplay().Married_To);

                            Session.GetRoleplay().marryReq = false;
                            Session.GetRoleplay().marryReqer = 0;

                            TargetSession.GetRoleplay().marryReq = false;
                            TargetSession.GetRoleplay().marryReqer = 0;

                            return true;
                        }
                        #endregion


                        #region Offer Weed
                        if (Session.GetRoleplay().OfferData.ContainsKey("weed"))
                        {
                            int Price = Session.GetRoleplay().OfferData["weed"].OfferPrice;
                            GameClient Offerer = Session.GetRoleplay().OfferData["weed"].OfferUser;

                            if (Session.GetHabbo().Credits < Price)
                            {
                                Session.SendWhisper("You do not have $" + Price + " to accept this offer!");
                                Session.GetRoleplay().OfferData.Remove("weed");
                                return true;
                            }

                            if (Offerer == null)
                            {
                                Session.SendWhisper("The person who offered you this item is offline, hence the offer has been canceled!");
                                Session.GetRoleplay().OfferData.Remove("weed");

                                return true;
                            }

                            RoleplayManager.Shout(Session, "*Accepts the offer and purchases " + Session.GetRoleplay().OfferData["weed"].OfferAmount + "g of weed for $" + Price + " [-$" + Price + "]*");
                            Session.GetRoleplay().SaveQuickStat("weed", "" + (Session.GetRoleplay().Weed + 5));
                            Session.GetRoleplay().Weed += 5;
                            RoleplayManager.GiveMoney(Session, -Price);
                            RoleplayManager.GiveMoney(Offerer, Price);
                            if (Offerer != null)
                            {
                                Offerer.SendWhisper(Session.GetHabbo().UserName + " has accepted your offer. You have received $" + Price);
                                Offerer.GetRoleplay().SaveQuickStat("weed", "" + (Offerer.GetRoleplay().Weed - 5));
                                Offerer.GetRoleplay().Weed -= 5;
                            }
                            Session.GetRoleplay().OfferData.Remove("weed");


                            return true;
                        }
                        #endregion

                        #region Offer Carrots
                        if (Session.GetRoleplay().OfferData.ContainsKey("carrots"))
                        {
                            int Price = Session.GetRoleplay().OfferData["carrots"].OfferPrice;
                            GameClient Offerer = Session.GetRoleplay().OfferData["carrots"].OfferUser;

                            if (Session.GetHabbo().Credits < Price)
                            {
                                Session.SendWhisper("You do not have $" + Price + " to accept this offer!");
                                Session.GetRoleplay().OfferData.Remove("carrots");
                                return true;
                            }

                            if (Offerer == null)
                            {
                                Session.SendWhisper("The farmer who offered you this item is offline, hence the offer has been canceled!");
                                Session.GetRoleplay().OfferData.Remove("carrots");

                                return true;
                            }

                            RoleplayManager.Shout(Session, "*Accepts the offer and purchases " + Session.GetRoleplay().OfferData["carrots"].OfferAmount + " carrots for $" + Price + " [-$" + Price + "]*");
                            Session.GetRoleplay().SaveQuickStat("carrots", "" + (Session.GetRoleplay().Carrots + 3));
                            Session.GetRoleplay().Carrots += 3;
                            RoleplayManager.GiveMoney(Session, -Price);
                            RoleplayManager.GiveMoney(Offerer, Price);
                            if (Offerer != null)
                            {
                                Offerer.SendWhisper(Session.GetHabbo().UserName + " has accepted your offer. You have received $" + Price);
                                Offerer.GetRoleplay().SaveQuickStat("carrots", "" + (Offerer.GetRoleplay().Carrots - 3));
                                Offerer.GetRoleplay().Carrots -= 3;
                            }
                            Session.GetRoleplay().OfferData.Remove("carrots");


                            return true;
                        }
                        #endregion

                        #region Offer Weapon
                        if (WeaponManager.isWeapon(Session.GetRoleplay().WeaponOfferedSell.ToLower()))
                        {
                            if (Session.GetRoleplay().OfferData.ContainsKey(Session.GetRoleplay().WeaponOfferedSell.ToLower()))
                            {
                                string wep_name = Session.GetRoleplay().WeaponOfferedSell.ToLower();
                                int Price = WeaponManager.WeaponsData[Session.GetRoleplay().WeaponOfferedSell.ToLower()].Price;
                                GameClient Offerer = Session.GetRoleplay().OfferData[wep_name].OfferUser;

                                if (Session.GetHabbo().Credits < Price)
                                {
                                    Session.SendWhisper("Você não tem $" + Price + " para aceitar essa oferta!");
                                    Session.GetRoleplay().OfferData.Remove(wep_name);
                                    return true;
                                }


                                if (Offerer == null && !Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("AMMUNATION"))
                                {
                                    Session.SendWhisper("A pessoa que lhe ofereceu este item está off-line, por isso a oferta foi cancelada!");
                                    Session.GetRoleplay().OfferData.Remove(wep_name);
                                    return true;
                                }

                                RoleplayManager.GiveMoney(Session, -Price);
                                RoleplayManager.Shout(Session, "*Aceita a oferta e as compras " + Session.GetRoleplay().OfferData[wep_name].OfferAmount + " " + wep_name + " por $" + Price + " [-$" + Price + "]*");
                                Session.GetRoleplay().addWeapon(wep_name);
                                if (Offerer != null)
                                {
                                    RoleplayManager.GiveMoney(Offerer, +100);
                                    Offerer.SendWhisper(Session.GetHabbo().UserName + " aceitou sua oferta. Você recebeu $" + 100 + " como comissão de vendas!");
                                }
                                RoleplayManager.sendStaffAlert(Session.GetHabbo().UserName + " acaba de comprar " + Session.GetRoleplay().OfferData[wep_name].OfferAmount + " " + wep_name + " por $" + Price, true);
                                Session.GetRoleplay().OfferData.Remove(wep_name);
                            }

                            return true;
                        }
                        #endregion

                    }
                    break;
                #endregion

                #region #decline
                case "deny":
                case "decline":
                    {
                        if (Session.GetRoleplay().OfferData.Count <= 0 && !Session.GetRoleplay().marryReq)
                        {
                            Session.SendWhisper("You currently have no offers!");
                            return true;
                        }

                        Session.GetRoleplay().WeaponOffered = false;

                        Session.GetRoleplay().marryReq = false;
                        Session.GetRoleplay().marryReqer = 0;

                        Session.GetRoleplay().OfferData.Clear();

                        RoleplayManager.Shout(Session, "*Declines all offers*", 0);

                        return true;
                    }
                    #endregion
            }
            #endregion

            return false;
        }
        public static Boolean MiniGameCmds(GameClient Session, string Input)
        {
            if (!Input.StartsWith("#"))
            {
                return false;
            }

            Input = Input.Substring(1);
            string[] Params = Input.Split(' ');

            switch (Params[0].ToLower())
            {
                #region :craft <item>
                case "craft":
                    {

                        #region Params / Vars

                        string Item = "";

                        if (!RoleplayManager.ParamsMet(Params, 1))
                        {
                            Session.SendWhisper("Incorrect command syntax: :craft <item>");
                            return true;
                        }
                        else
                        {
                            Item = Convert.ToString(Params[1]);
                        }

                        #endregion

                        #region Conditions

                        #endregion

                        #region Execute

                        #endregion

                        return true;
                    }
                #endregion
                #region :dropmine
                case "dropmine":
                    {

                        #region Conditions
                        RoomUser User = Session.GetHabbo().GetRoomUser();
                        if (!Session.GetRoleplay().HungerGames)
                        {
                            Session.SendWhisper("You are not in Hunger Games!");
                            return true;
                        }
                        if (!Session.GetRoleplay().HungerGames_Inventory.ContainsKey("landmine"))
                        {
                            Session.SendWhisper("You do not have a landmine!");
                            return true;
                        }
                        if (Session.GetRoleplay().HungerGames_Inventory["landmine"] <= 0)
                        {
                            Session.SendWhisper("You do not have anymore landmines left!");
                            return true;
                        }
                        #endregion


                        #region Execute

                        Session.GetHabbo().CurrentRoom.GetGameMap().Model.SqState[User.X][User.Y] = SquareState.HungerGames_LandMine;
                        Session.Shout("*Drops a landmine*");
                        Session.GetRoleplay().HungerGames_Inventory["landmine"] -= 1;
                        #endregion

                        return true;
                    }
                #endregion
                #region :giveweapon <user> <weapon>
                case "giveweapon":
                case "oferecerarma":
                    {


                        #region Vars / Params
                        string Username = "";
                        string Weapon = "";
                        #endregion

                        #region Conditions
                        if (RoleplayManager.ParamsMet(Params, 2))
                        {
                            Username = Convert.ToString(Params[1]);
                            Weapon = Convert.ToString(Params[2]);
                        }
                        else
                        {
                            Session.SendWhisper("Parametro incorreto");
                            return true;
                        }

                        #endregion

                        #region Execute
                        GameClient Targ = RoleplayManager.GenerateSession(Username);
                        if (Targ == null)
                            return true;

                        Targ.GetRoleplay().HungerGames_Inventory.Add(Weapon.ToLower(), 30);
                        Session.SendWhisper("Sucesso você deu para " + Targ.GetHabbo().UserName + " uma " + Weapon.ToLower());
                        Targ.SendWhisper("Você recebeu uma " + Weapon.ToLower() + " de " + Session.GetHabbo().UserName);
                        #endregion

                        return true;
                    }
                #endregion
                #region :forcestop
                case "forcestop":
                    {

                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                        {
                            foreach (GameClient mClient in Plus.GetGame().GetClientManager().Clients.Values)
                            {
                                if (mClient == null)
                                    continue;
                                if (mClient.GetHabbo() == null)
                                    continue;
                                if (mClient.GetHabbo().CurrentRoom == null)
                                    continue;
                                if (mClient.GetRoleplay() == null)
                                    continue;

                                if (!mClient.GetRoleplay().InMiniGame)
                                    continue;

                                mClient.GetRoleplay().InMiniGame = false;
                                mClient.GetRoleplay().HungerGames = false;
                                mClient.GetRoleplay().HungerGames_Cash = 0;
                                mClient.GetRoleplay().HungerGames_Dead = false;
                                mClient.GetRoleplay().HungerGames_Inventory.Clear();
                                mClient.GetRoleplay().HungerGames_Item_Wielding = "";
                                mClient.GetRoleplay().HungerGames_Pts = 0;
                                mClient.GetHabbo().GetRoomUser().ApplyEffect(0);
                            }
                        }
                        Session.SendWhisper("Feito");


                        return true;
                    }
                    #endregion

            }

            return false;
        }
        public static string MergeParams(string[] Params, int Start)
        {
            StringBuilder MergedParams = new StringBuilder();

            for (int i = 0; i < Params.Length; i++)
            {
                if (i < Start)
                {
                    continue;
                }

                if (i > Start)
                {
                    MergedParams.Append(" ");
                }

                MergedParams.Append(Params[i]);
            }

            return MergedParams.ToString();
        }

        public static void SendChatMessage(GameClient Session, string Message)
        {
            RoomUser User = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().UserName);
            Session.SendWhisper(Message);
        }
        #endregion

    }
}
