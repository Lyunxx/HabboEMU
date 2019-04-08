using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System;

namespace Plus.HabboHotel.Items.Interactor
{
    internal class InteractorCrackableEgg : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            RoomUser roomUser = null;
            if (session != null) roomUser = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUser == null) return;
            if (Gamemap.TilesTouching(item.X, item.Y, roomUser.X, roomUser.Y))
            {
                var cracks = 0;
                if (Plus.IsNum(item.ExtraData))
                    cracks = Convert.ToInt16(item.ExtraData);
                cracks++;
                item.ExtraData = Convert.ToString(cracks);
                item.UpdateState(false, true);
                return;
            }
            roomUser.MoveTo(item.SquareInFront);
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
        {
        }
    }
}