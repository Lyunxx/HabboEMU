﻿using Plus.HabboHotel.Items;
using System.Collections.Generic;
using System.Linq;

namespace Plus.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class FurniHasNotUsers : IWiredItem
    {
        public FurniHasNotUsers(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type
        {
            get { return Interaction.ConditionFurnisHaveNotUsers; }
        }

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public string OtherString
        {
            get { return ""; }
            set { }
        }

        public string OtherExtraString
        {
            get { return ""; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return ""; }
            set { }
        }

        public bool OtherBool
        {
            get { return true; }
            set { }
        }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            if (!Items.Any())
                return true;

            foreach (
                var current in
                    Items.Where(
                        current => current != null && Room.GetRoomItemHandler().FloorItems.ContainsKey(current.Id)))
            {
                if (
                    current.AffectedTiles.Values.Any(
                        current2 => Room.GetGameMap().SquareHasUsers(current2.X, current2.Y)))
                    return false;
                if (Room.GetGameMap().SquareHasUsers(current.X, current.Y))
                    return false;
            }
            return true;
        }
    }
}