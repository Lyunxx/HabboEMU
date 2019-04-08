using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Generic;
using Plus.HabboHotel.Roleplay.Misc;
using System.Linq;
using System.Text;
using Plus.HabboHotel.Roleplay.Jobs.Space;
using Plus.HabboHotel.Roleplay.Jobs.Farming;
using Plus.HabboHotel.Roleplay.Jobs;
using Plus.HabboHotel.Roleplay.Gangs;
using Plus.HabboHotel.Roleplay.Combat;
using Plus.HabboHotel.Roleplay.Timers;
using Plus.HabboHotel.Roleplay.Radio;
using Plus.HabboHotel.Roleplay.Jobs.Cutting;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using System.Data;
using Plus.HabboHotel.PathFinding;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorRP : IFurniInteractor
    {
        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {

        }

        public void OnPlace(GameClients.GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";
            Item.UpdateNeeded = true;
        }

        public void OnRemove(GameClients.GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";
        }

        public void OnTrigger(GameClients.GameClient Session, RoomItem Item, int Request, bool HasRights)
        {
            if (Item.GetBaseItem().Name.ToLower().Contains("atm"))
            {
                HandleATM(Session, Item, Request, HasRights);
            }

            if (Item.BaseItem == 3514)
            {
                handleEnergyDrinkPurchase(Session, Item);
            }
            if (Item.BaseItem == 8039)
            {
                handleMedKitPurchase(Session, Item);
            }
            if (Item.BaseItem == 4495)
            {
                handleBroteinPurchase(Session, Item);
            }
            if (Item.BaseItem == 1623) // prison_stones baseID
            {
                handleConsole(Session, Item);
            }
            if (Item.BaseItem == 1943) // prison_stones baseID
            {
                handleRock(Session, Item);
            }

            if (Item.BaseItem == 7024) // pine tree baseID
            {
                handleTree(Session, Item);
            }

            if (Item.BaseItem == 1737) // dirt nest baseID
            {
                handleFarmingSpot(Session, Item);
            }

            if (Item.GetBaseItem().Name.ToLower().Contains("wf_floor_switch1"))
            {
                HandlePullTheHandleForPolice(Session, Item, Request, HasRights);
            }

            if (Item.GetBaseItem().Name.ToLower().Contains("hc_machine"))
            {
                HandleNPA(Session, Item, Request, HasRights);
            }

            uint Modes = Item.GetBaseItem().Modes - 1;

            if (Session == null || !HasRights || Modes <= 0)
            {
                return;
            }

            Plus.GetGame().GetQuestManager().ProgressUserQuest(Session, HabboHotel.Quests.QuestType.FurniSwitch);

            int CurrentMode = 0;
            int NewMode = 0;

            if (!int.TryParse(Item.ExtraData, out CurrentMode))
            {

            }

            if (CurrentMode <= 0)
            {
                NewMode = 1;
            }
            else if (CurrentMode >= Modes)
            {
                NewMode = 0;
            }
            else
            {
                NewMode = CurrentMode + 1;
            }

            Item.ExtraData = NewMode.ToString();
            Item.UpdateState();

        }
        public void handleConsole(GameClient Session, RoomItem Item) //1623
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
        }
        public void handleBroteinPurchase(GameClient Session, RoomItem Item) //4495
        {
            //+2 STR Health
            if (Session.GetHabbo().Credits < 16)
            {
                Session.SendWhisper("Brotein costs $16, you do not have enough money");
                return;
            }
            if (Session.GetHabbo().CurrentRoomId != 460)
            {
                return;
            }
            if (RoleplayManager.Distance(new Vector2D(Item.X, Item.Y), new Vector2D(Session.GetHabbo().GetRoomUser().X, Session.GetHabbo().GetRoomUser().Y)) > 2)
            {
                Session.SendWhisper("You must be closer to the object you wish to purchase");
                return;
            }
            Session.GetHabbo().Credits -= 16;
            Session.GetHabbo().UpdateCreditsBalance();
            Session.Shout("*Purchases Brotein for $16 [-$16]*");
            Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, Item.BaseItem, "0", 0u, true, false, 0, 0, string.Empty);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
        }
        public void handleMedKitPurchase(GameClient Session, RoomItem Item) //8039
        {
            //80 Health
            if (Session.GetHabbo().Credits < 20)
            {
                Session.SendWhisper("This kit costs $20, you do not have enough money");
                return;
            }
            if(Session.GetHabbo().CurrentRoomId != 460){
                return;
            }
            if (RoleplayManager.Distance(new Vector2D(Item.X, Item.Y), new Vector2D(Session.GetHabbo().GetRoomUser().X, Session.GetHabbo().GetRoomUser().Y)) > 2)
            {
                Session.SendWhisper("You must be closer to the object you wish to purchase");
                return;
            }
            Session.GetHabbo().Credits -= 20;
            Session.GetHabbo().UpdateCreditsBalance();
            Session.Shout("*Purchases Medkit for $20 [-$20]*");
            Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, Item.BaseItem, "0", 0u, true, false, 0, 0, string.Empty);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
        }
        public void handleEnergyDrinkPurchase(GameClient Session, RoomItem Item) //3514
        {
            // 25 health
            if (Session.GetHabbo().Credits < 4)
            {
                Session.SendWhisper("This drink costs $4, you do not have enough money");
                return;
            }
            if (Session.GetHabbo().CurrentRoomId != 460)
            {
                return;
            }
            if (RoleplayManager.Distance(new Vector2D(Item.X, Item.Y), new Vector2D(Session.GetHabbo().GetRoomUser().X, Session.GetHabbo().GetRoomUser().Y)) > 2)
            {
                Session.SendWhisper("You must be closer to the object you wish to purchase");
                return;
            }
            Session.GetHabbo().Credits -= 4;
            Session.GetHabbo().UpdateCreditsBalance();
            Session.Shout("*Purchases Energy Drink for $4 [-$4]*");
            Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, Item.BaseItem, "0", 0u, true, false, 0, 0, string.Empty);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
        }
        public void handleRock(GameClient Session, RoomItem Item)
        {
            RoomUser User = Session.GetHabbo().GetRoomUser();
            Rock theRock = spaceManager.getRockByItem(Item);

            if (!JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].hasRights("miner"))
            {
                Session.SendWhisperBubble("You must be a miner to mine this rock!", 1);
                return;
            }

            if (!Session.GetRoleplay().Working)
            {
                Session.SendWhisperBubble("You must be working to do this!", 1);
                return;
            }
            if (theRock != null && spaceManager.isUserNearRock(theRock, User))
            {
                if (theRock.beingMined == false)
                {
                    mineTimer timer = new mineTimer(Session, theRock);
                    timer.startTimer();
                }
                else
                {
                    Session.SendWhisperBubble("This rock is already being mined!", 1);
                }
            }
            else
            {
                Session.SendWhisperBubble("You aren't close enough to the rock!", 1);
            }
        }

        public void handleTree(GameClient Session, RoomItem Item)
        {
            RoomUser User = Session.GetHabbo().GetRoomUser();
            Tree theTree = woodManager.getTreeByItem(Item);

            if (!JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].hasRights("woodcutter"))
            {
                Session.SendWhisperBubble("You must be a lumberjack to chop this tree", 1);
                return;
            }

            if (!Session.GetRoleplay().Working)
            {
                Session.SendWhisperBubble("You must be working to do this!", 1);
                return;
            }
            if (theTree != null && woodManager.isUserNearTree(theTree, User))
            {
                if (theTree.beingMined == false)
                {
                    chopTimer timer = new chopTimer(Session, theTree);
                    timer.startTimer();
                }
                else
                {
                    Session.SendWhisperBubble("This tree is already being chopped!", 1);
                }
            }
            else
            {
                Session.SendWhisperBubble("You aren't close enough to the tree!", 1);
            }
        }

        public void handleFarmingSpot(GameClient Session, RoomItem Item)
        {
            RoomUser User = Session.GetHabbo().GetRoomUser();
            FarmingSpot theFarmingSpot = farmingManager.getFarmingSpotByItem(Item);

            if (!JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].hasRights("farming"))
            {
                if (theFarmingSpot.type == "weed") { Session.SendWhisperBubble("You must be a farmer to plant weed here!", 1); }
                else if (theFarmingSpot.type == "carrot") { Session.SendWhisperBubble("You must be a farmer to plant carrots here!", 1); }
                else { Session.SendWhisperBubble("You must be a farmer to plant here!", 1); }
                return;
            }

            if (!Session.GetRoleplay().Working)
            {
                if (theFarmingSpot.type == "weed") { Session.SendWhisperBubble("You must be working to plant weed here!", 1); }
                else if (theFarmingSpot.type == "carrot") { Session.SendWhisperBubble("You must be working to plant carrots here!", 1); }
                else { Session.SendWhisperBubble("You must be working to plant here!", 1); }
                return;
            }

            if (theFarmingSpot != null && farmingManager.isUserNearFarmingSpot(theFarmingSpot, User))
            {
                if (theFarmingSpot.beingFarmed2 == false && theFarmingSpot.Part1Complete == true)
                {
                    farmingTimer2 timer = new farmingTimer2(Session, theFarmingSpot);
                    timer.startTimer();
                }
                else if (theFarmingSpot.beingFarmed == false)
                {
                    farmingTimer1 timer = new farmingTimer1(Session, theFarmingSpot);
                    timer.startTimer();
                }
                else
                {
                    Session.SendWhisperBubble("This spot is already being farmed!", 1);
                }
            }
            else
            {
                Session.SendWhisperBubble("You aren't close enough to the farming spot!", 1);
            }
        }

        public void HandleATM(GameClients.GameClient Session, RoomItem Item, int Request, bool HasRights)
        {
            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);


            if (Item.InteractingUser2 != User.UserId)
                Item.InteractingUser2 = User.UserId;

            if (User == null)
            {
                return;
            }

            if (User.Coordinate != Item.SquareInFront && User.CanWalk)
            {
                User.MoveTo(Item.SquareInFront);
                return;
            }
            if (Session.GetRoleplay().inATM == true)
            {
                Session.SendWhisperBubble("[ATM] You are already logged in!", 1);
                return;
            }

            Session.GetRoleplay().inATM = true;

            Session.SendWhisperBubble("[ATM] Processing transaction please wait...", 1);

            int amount = Session.GetRoleplay().AtmSetAmount;

            if (amount > Session.GetRoleplay().Bank)
            {
                Session.SendWhisperBubble("[ATM] Transaction failed, insufficient funds!", 3);
                Session.GetHabbo().GetRoomUser().UnlockWalking();
                Session.GetRoleplay().inATM = false;
                return;
            }
            else
            {

                System.Threading.Thread.Sleep(2000);

                Session.SendWhisperBubble("[ATM] Transaction succesful!", 6);
                RoleplayManager.Shout(Session, "*Uses the ATM to withdraw $" + amount + " from their account [+$" + amount + "]*", 6);
                Session.GetHabbo().GetRoomUser().UnlockWalking();
                Session.GetRoleplay().inATM = false;
                Session.GetRoleplay().Bank -= amount;
                Session.GetRoleplay().SaveQuickStat("bank", "" + Session.GetRoleplay().Bank);
                RoleplayManager.GiveMoney(Session, +amount);
                Session.GetRoleplay().AtmSetAmount = 20;
            }
        }

        public void HandlePullTheHandleForPolice(GameClients.GameClient Session, RoomItem Item, int Request, bool HasRights)
        {

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);


            if (Item.InteractingUser2 != User.UserId)
                Item.InteractingUser2 = User.UserId;

            if (User == null)
            {
                return;
            }

            if (User.Coordinate != Item.SquareInFront && User.CanWalk)
            {
                User.MoveTo(Item.SquareInFront);
                return;
            }

            if (!Session.GetRoleplay().MultiCoolDown.ContainsKey("rp_vswitch"))
            {
                Session.GetRoleplay().MultiCoolDown.Add("rp_vswitch", 0);
            }
            if (Session.GetRoleplay().MultiCoolDown["rp_vswitch"] > 0)
            {
                Session.SendWhisperBubble("You must wait until you can pull the switch! [" + Session.GetRoleplay().MultiCoolDown["rp_vswitch"] + "/15]", 1);
                return;
            }

            lock (Plus.GetGame().GetClientManager().Clients.Values)
            {
                foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                {

                    if (client == null)
                        continue;
                    if (client.GetRoleplay() == null)
                        continue;
                    if (!JobManager.validJob(client.GetRoleplay().JobId, client.GetRoleplay().JobRank))
                        continue;
                    if (!client.GetRoleplay().JobHasRights("police"))
                        continue;
                    if (!client.GetRoleplay().JobHasRights("swat"))
                        continue;
                    if (!client.GetRoleplay().Working)
                        continue;

                    string msg = Session.GetHabbo().UserName + " has pulled the switch at [Room ID: " + Session.GetHabbo().CurrentRoomId + "], asking for help!";
                    Radio.send(msg, Session);
                }
            }
            RoleplayManager.Shout(Session, "*Pulls the trigger of the switch, notifying the cops*", 4);
            Session.GetRoleplay().MultiCoolDown["rp_vswitch"] = 15;
            Session.GetRoleplay().CheckingMultiCooldown = true;
        }

        public void HandleNPA(GameClients.GameClient Session, RoomItem Item, int Request, bool HasRights)
        {

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);


            if (Item.InteractingUser2 != User.UserId)
                Item.InteractingUser2 = User.UserId;

            if (User == null)
            {
                return;
            }

            if (User.Coordinate != Item.SquareInFront && User.CanWalk)
            {
                User.MoveTo(Item.SquareInFront);
                return;
            }

            if (Item.OnNPAUsing)
            {
                User.GetClient().SendWhisperBubble("Someone is already using this machine to nuke the city!", 1);
                return;
            }

            if (RoleplayManager.NukesOccurred > 5)
            {
                User.GetClient().SendWhisperBubble("The system has reached a maximum amount of nukes per emulator reboot. Please try again later.", 1);
                return;
            }

            RoleplayManager.Shout(User.GetClient(), "*Starts the process in nuking the city*", 4);
            User.GetClient().GetRoleplay().npaTimer = new nukeTimer(User.GetClient());
            User.GetClient().GetRoleplay().NPA = true;
            User.GetClient().SendWhisperBubble("You have " + User.GetClient().GetRoleplay().npaTimer.getTime() + " minutes until you nuke the city.", 1);
            Item.OnNPAUsing = true;
            RoleplayManager.NukesOccurred++;


        }

        public void OnWiredTrigger(RoomItem Item)
        {
            if (Item.ExtraData == "0")
            {
                Item.ExtraData = "1";
                Item.UpdateState(false, true);
                Item.ReqUpdate(4, true);
            }
        }
    }
}