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

            if (Item.BaseItem == 1943) // prison_stones baseID
            {
                handleRock(Session, Item);
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

        public void handleRock(GameClient Session, RoomItem Item)
        {
            RoomUser User =Session.GetHabbo().GetRoomUser();
            Rock theRock = spaceManager.getRockByItem(Item);

            if (!JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].hasRights("spaceminer"))
            {
                Session.SendWhisper("You must be a space miner to mine this rock!");
                return;
            }

            if (!Session.GetRoleplay().Working)
            {
                Session.SendWhisper("You must be working to do this!");
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
                    Session.SendWhisper("This rock is already being mined!");
                }
            }
            else
            {
                Session.SendWhisper("You aren't close enough to the rock!");
            }
        }

        public void handleFarmingSpot(GameClient Session, RoomItem Item)
        {
            RoomUser User = Session.GetHabbo().GetRoomUser();
            FarmingSpot theFarmingSpot = farmingManager.getFarmingSpotByItem(Item);

            if (!JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].hasRights("farming"))
            {
                if (theFarmingSpot.type == "weed") { Session.SendWhisper("You must be a farmer to plant weed here!"); }
                else if (theFarmingSpot.type == "carrot") { Session.SendWhisper("You must be a farmer to plant carrots here!"); }
                else { Session.SendWhisper("You must be a farmer to plant here!"); }
                return;
            }

            if (!Session.GetRoleplay().Working)
            {
                if (theFarmingSpot.type == "weed") { Session.SendWhisper("You must be working to plant weed here!"); }
                else if (theFarmingSpot.type == "carrot") { Session.SendWhisper("You must be working to plant carrots here!"); }
                else { Session.SendWhisper("You must be working to plant here!"); }
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
                    Session.SendWhisper("This spot is already being farmed!");
                }
            }
            else
            {
                Session.SendWhisper("You aren't close enough to the farming spot!");
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
                Session.SendWhisper("[ATM MSG] You are already logged in!");
                return;
            }

            Session.GetRoleplay().inATM = true;

            Session.SendWhisper("[ATM MSG] Processing transaction please wait...");

            int amount = Session.GetRoleplay().AtmSetAmount;

            if (amount > Session.GetRoleplay().Bank)
            {
                Session.SendWhisper("[ATM MSG] Transaction failed, insufficient funds!");
               Session.GetHabbo().GetRoomUser().UnlockWalking();
                Session.GetRoleplay().inATM = false;
                return;
            }
            else
            {

                System.Threading.Thread.Sleep(2000);

                Session.SendWhisper("[ATM MSG] Transaction succesful!");
                RoleplayManager.Shout(Session, "*Uses the ATM to withdraw $" + amount + " from their account [+$" + amount + "]*");
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
                Session.SendWhisper("You must wait until you can pull the switch! [" + Session.GetRoleplay().MultiCoolDown["rp_vswitch"] + "/15]");
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
                    /*if (!client.GetRoleplay().JobHasRights("gov"))
                        continue;*/
                    if (!client.GetRoleplay().Working)
                        continue;

                    client.GetHabbo().GetRoomUser().LastBubble = 30;
                    client.SendWhisper("<b>[Police Radio]:</b> " + Session.GetHabbo().UserName + " has pulled the switch at " + Session.GetHabbo().CurrentRoomId + ", acquiring help!");
                    client.GetHabbo().GetRoomUser().LastBubble = 0;
                }
            }
            RoleplayManager.Shout(Session, "*Pulls the trigger of the switch, notifying the cops*");
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
                User.GetClient().SendWhisper("Someone is already using this machine to nuke the city!");
                return;
            }

            if (RoleplayManager.NukesOccurred > 5)
            {
                User.GetClient().SendWhisper("The system has reached a maximum amount of nukes per emulator reboot. Please try again later.");
                return;
            }

            User.GetClient().Shout("*Starts the process in nuking the city*");
            User.GetClient().GetRoleplay().npaTimer = new nukeTimer(User.GetClient());
            User.GetClient().GetRoleplay().NPA = true;
            User.GetClient().SendWhisper("You have " + User.GetClient().GetRoleplay().npaTimer.getTime() + " minutes until you nuke the city.");
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
