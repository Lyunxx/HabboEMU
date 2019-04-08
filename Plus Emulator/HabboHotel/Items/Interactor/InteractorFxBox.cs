using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System;

namespace Plus.HabboHotel.Items.Interactor
{
    internal class InteractorFxBox : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights) return;
            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null) return;
            Room room = session.GetHabbo().CurrentRoom;
            if (room == null) return;
            var effectId = Convert.ToInt32(item.GetBaseItem().Name.Replace("fxbox_fx", ""));
            session.GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(effectId, -1, 0);
            room.GetRoomItemHandler().RemoveFurniture(session, item.Id, false);
            using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery("DELETE FROM items_rooms WHERE id = " + item.Id);
            }
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
        {
        }
    }
}