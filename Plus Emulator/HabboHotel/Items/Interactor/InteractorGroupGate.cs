using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Roleplay;

namespace Plus.HabboHotel.Items.Interactor
{
    internal class InteractorGroupGate : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {

        }

        public void OnRemove(GameClient session, RoomItem item)
        {

        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            {
                var user = session.GetHabbo();
                int MyJobId = session.GetRoleplay().JobId;
                if (item.GroupId != MyJobId)
                {
                    session.SendNotif("You don't work here!", "Business Message", "");
                    return;
                }

                if (item == null || item.GetBaseItem() == null ||
                    item.GetBaseItem().InteractionType != Interaction.GuildGate)
                    return;

                var modes = item.GetBaseItem().Modes - 1;
                if (modes <= 0)
                    item.UpdateState(false, true);

                int currentMode;
                int.TryParse(item.ExtraData, out currentMode);
                int newMode;
                if (currentMode <= 0)
                    newMode = 1;
                else if (currentMode >= modes)
                    newMode = 0;
                else
                    newMode = currentMode + 1;

                if (newMode == 0 && !item.GetRoom().GetGameMap().ItemCanBePlacedHere(item.X, item.Y))
                    return;

                item.ExtraData = newMode.ToString();
                item.UpdateState();
                item.GetRoom().GetGameMap().UpdateMapForItem(item);
                item.GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerStateChanged, new object[]
                {
                    item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id),
                    item
                });
            }
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            if (session == null || item == null || user == null) return;

            var distance = PathFinder.GetDistance(user.X, user.Y, item.X, item.Y);
            if (distance > 0 || user.GoalX == 0 && user.GoalY == 0) return;

            item.ExtraData = "0";
            item.UpdateState(false, true);
            item.InteractingUser = 1;

            if (user.GoalX != item.X || user.GoalY != item.Y) return;
            switch (user.RotBody)
            {
                case 3:
                case 4:
                case 5:
                    user.MoveTo(item.GetRoom()
                        .GetGameMap()
                        .CanWalk(item.SquareBehind.X, item.SquareBehind.Y, user.AllowOverride)
                        ? item.SquareBehind
                        : item.SquareInFront);
                    break;
                default:
                    user.MoveTo(item.GetRoom()
                        .GetGameMap()
                        .CanWalk(item.SquareInFront.X, item.SquareInFront.Y, user.AllowOverride)
                        ? item.SquareInFront
                        : item.SquareBehind);
                    break;
            }
        }

        public void OnWiredTrigger(RoomItem item)
        {
            {
                var num = item.GetBaseItem().Modes - 1;
                if (num <= 0)
                {
                    item.UpdateState(false, true);
                }
                int num2 = 0;
                int.TryParse(item.ExtraData, out num2);
                int num3;
                if (num2 <= 0)
                {
                    num3 = 1;
                }
                else
                {
                    if (num2 >= num)
                    {
                        num3 = 0;
                    }
                    else
                    {
                        num3 = num2 + 1;
                    }
                }
                if (num3 == 0 && !item.GetRoom().GetGameMap().ItemCanBePlacedHere(item.X, item.Y))
                {
                    return;
                }
                item.ExtraData = num3.ToString();
                item.UpdateState();
                item.GetRoom().GetGameMap().UpdateMapForItem(item);
            }
        }
    }
}