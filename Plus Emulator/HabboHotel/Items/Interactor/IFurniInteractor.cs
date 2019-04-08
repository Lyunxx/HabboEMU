using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Interactor
{
    internal interface IFurniInteractor
    {
        void OnPlace(GameClient session, RoomItem item);

        void OnRemove(GameClient session, RoomItem item);

        void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights);

        void OnUserWalk(GameClient session, RoomItem item, RoomUser user);

        void OnWiredTrigger(RoomItem item);
    }
}