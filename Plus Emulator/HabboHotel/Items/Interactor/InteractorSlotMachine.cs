using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Roleplay.Casino.Slots;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Generic;
using Plus.HabboHotel.Roleplay.Misc;
using System.Linq;
using System.Text;

namespace Plus.HabboHotel.Items.Interactor
{
    class InteractorSlotMachine : IFurniInteractor
    {
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
            int SlotsCost = RoleplayManager.SlotsMachineCost;

            Slot theSlot = SlotsManager.getSlotbyItem(Item);

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (Item.InteractingUser2 != User.UserId)
                Item.InteractingUser2 = User.UserId;

            if (User == null)
            {
                return;
            }

            if (Session.GetRoleplay().Working && Session.GetHabbo().Rank <= 2)
            {
                Session.SendWhisperBubble("You cannot roll the slots while working!");
                return;
            }

            if (!Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("CASINO") == true)
            {
                Session.SendWhisperBubble("You must be in the casino to roll slots! [Room ID: 200 & 201]", 1);
                return;
            }

            if (Session.GetRoleplay().inSlotMachine == true)
            {
                Session.SendWhisperBubble("[SLOT MACHINE] You have already pulled the handle!", 1);
                return;
            }

            if (Session.GetHabbo().Credits < SlotsCost)
            {
                Session.SendWhisperBubble("[SLOT MACHINE] You don't have enough money! This machine cost: $" + SlotsCost + " to use.", 1);
                return;
            }

            if (theSlot != null && SlotsManager.isUserNearMachine(theSlot, User))
            {
                if (theSlot.beingRolled == false)
                {
                    slotsTimer timer = new slotsTimer(Session, theSlot);
                    timer.startTimer();

                    Session.GetRoleplay().inSlotMachine = true;
                    Session.GetHabbo().GetRoomUser().CanWalk = false;
                    RoleplayManager.GiveMoney(Session, -SlotsCost);
                }
                else
                {
                    Session.SendWhisperBubble("[SLOT MACHINE] This machine is already in use!", 1);
                }
            }
            else
            {
                Session.SendWhisperBubble("[SLOT MACHINE] You aren't close enough to pull the handle!", 1);
            }
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {

        }

        public void OnWiredTrigger(RoomItem Item)
        {
        }
    }
}
