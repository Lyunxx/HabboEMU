using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Roleplay.Jobs;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorOneWayGate : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser != 0)
            {
                var User = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement();
                    User.UnlockWalking();
                }

                item.InteractingUser = 0;
            }
        }

        public void OnRemove(GameClient Session, RoomItem item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser != 0)
            {
                RoomUser User = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement();
                    User.UnlockWalking();
                }

                item.InteractingUser = 0;
            }
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            return;
        }
        public void OnWiredTrigger(RoomItem item)
        {
            return;
        }
        public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (Item.InteractingUser2 != User.UserId)
                Item.InteractingUser2 = User.UserId;

            if (User == null)
            {
                return;
            }
            if (Item.GetBaseItem().InteractionType == Interaction.OneWayGate)
            {

                if (User.Coordinate != Item.SquareInFront && User.CanWalk)
                {
                    User.MoveTo(Item.SquareInFront);
                    return;
                }

                if (Session.GetRoleplay().Dead || Session.GetRoleplay().Jailed)
                {
                    if (Session.GetRoleplay().JobId != 3 && !Session.GetRoleplay().Working)
                        Session.SendWhisper("You are dead or jailed");
                        return;
                    }
                }

                if (Item.BaseItem == 1010 && Session.GetRoleplay().Working == false)
                {
                    Session.SendWhisper("You must be working to enter this gate");
                    return;
                }

                if (Item.BaseItem == 1013 && (!JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank].canSendHome() || !Session.GetRoleplay().Working))
                {
                    if (Session.GetRoleplay().JobId == 3 && Session.GetRoleplay().Working)
                    {
                        Session.Shout("*Slides police badge and enters the managers gate*", 4, false);
                    }else{
                    Session.SendWhisper("You must be a(n) manager to enter this gate");
                    return;
                    }
                }

                if (Item.BaseItem == 1008)
                {
                    if (Session.GetHabbo().Credits < 10)
                    {
                        Session.SendWhisper("Insufficient funds - Requires $10");
                        return;
                    }else{
                    if (Session.GetRoleplay().JobId != 3 || !Session.GetRoleplay().Working)
                    {
                         Session.GetHabbo().Credits = Session.GetHabbo().Credits - 10;
                         Session.GetHabbo().UpdateCreditsBalance();
                         Session.SendWhisper("Charged $10 to enter this gate");
                    }
                    else
                    {
                        Session.Shout("*Slides police badge and enters the gate for free*", 4, false);
                    }
                    }
                }

                if (!Item.GetRoom().GetGameMap().ValidTile(Item.SquareBehind.X, Item.SquareBehind.Y) ||
                    !Item.GetRoom().GetGameMap().CanWalk(Item.SquareBehind.X, Item.SquareBehind.Y, false)
                    || !Item.GetRoom().GetGameMap().SquareIsOpen(Item.SquareBehind.X, Item.SquareBehind.Y, false))
                {
                    return;
                }

                if ((User.LastInteraction - Plus.GetUnixTimestamp() < 0) && User.InteractingGate &&
                    User.GateId == Item.Id)
                {
                    User.InteractingGate = false;
                    User.GateId = 0;
                }


                if (!Item.GetRoom().GetGameMap().CanWalk(Item.SquareBehind.X, Item.SquareBehind.Y, User.AllowOverride))
                {
                    return;
                }

                if (Item.GetRoom().GetGameMap().CanWalk(Item.SquareBehind.X, Item.SquareBehind.Y, User.AllowOverride) && User.Coordinate == Item.SquareInFront && (Item.InteractingUser == 0))
                {
                    Item.ExtraData = "1";
                    System.Threading.Thread.Sleep(1000);
                    Item.InteractingUser = User.HabboId;
                    User.CanWalk = false;
                    User.AllowOverride = true;
                    Item.UpdateState(false, true);
                    User.MoveTo(Item.Coordinate);
                    Item.ExtraData = "0";
                    User.MoveTo(Item.SquareBehind);
                    Item.UpdateState(false, true);
                    User.CanWalk = true;
                    System.Threading.Thread.Sleep(1000);
                    User.AllowOverride = false;
                    Item.InteractingUser = 0;
                }
                else if (Item.GetRoom().GetGameMap().CanWalk(Item.SquareInFront.X, Item.SquareInFront.Y, User.AllowOverride) && User.Coordinate == Item.SquareBehind && (Item.InteractingUser == 0))
                {
                    Item.ExtraData = "1";
                    System.Threading.Thread.Sleep(1000);
                    Item.InteractingUser = User.HabboId;
                    User.CanWalk = false;
                    User.AllowOverride = true;
                    Item.UpdateState(false, true);
                    User.MoveTo(Item.Coordinate);
                    Item.ExtraData = "0";
                    User.MoveTo(Item.SquareInFront);
                    Item.UpdateState(false, true);
                    User.CanWalk = true;
                    System.Threading.Thread.Sleep(1000);
                    User.AllowOverride = false;
                    Item.InteractingUser = 0;
                }
            }
        }
    }