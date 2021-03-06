using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System.Timers;

namespace Plus.HabboHotel.Items.Interactor
{
    internal class InteractorVikingCotie : IFurniInteractor
    {
        private RoomItem _mItem;

        public void OnPlace(GameClient session, RoomItem item)
        {
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            if (user.CurrentEffect != 172 && user.CurrentEffect != 5 && user.CurrentEffect != 173)
            {
                return;
            }
            if (item.ExtraData != "5")
            {
                if (item.VikingCotieBurning)
                {
                    return;
                }
                item.ExtraData = "1";
                item.UpdateState();

                item.VikingCotieBurning = true;
                GameClient clientByUsername = Plus.GetGame().GetClientManager().GetClientByUserName(item.GetRoom().RoomData.Owner);

                if (clientByUsername != null)
                {
                    if (clientByUsername.GetHabbo().UserName != item.GetRoom().RoomData.Owner)
                    {
                        clientByUsername.SendNotif(string.Format(Plus.GetLanguage().GetVar("viking_burn_started"), user.GetUserName()));
                    }
                }

                this._mItem = item;

                var timer = new Timer(5000);
                timer.Elapsed += this.OnElapse;
                timer.Enabled = true;
            }
            else
            {
                session.SendNotif(Plus.GetLanguage().GetVar("user_viking_error"));
            }
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
        {
        }

        private void OnElapse(object sender, ElapsedEventArgs e)
        {
            if (_mItem == null) return;
            switch (this._mItem.ExtraData)
            {
                case "1":
                    this._mItem.ExtraData = "2";
                    this._mItem.UpdateState();
                    return;

                case "2":
                    this._mItem.ExtraData = "3";
                    this._mItem.UpdateState();
                    return;

                case "3":
                    this._mItem.ExtraData = "4";
                    this._mItem.UpdateState();
                    return;

                case "4":
                    try
                    {
                        ((Timer) sender).Stop();
                    }
                    catch (Exception)
                    {
                    }
                    this._mItem.ExtraData = "5";
                    this._mItem.UpdateState();
                    return;
            }
        }
    }
}