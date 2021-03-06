﻿using Plus.HabboHotel.Items;
using System;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class HowManyUsers : IWiredItem
    {
        public HowManyUsers(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            OtherString = string.Empty;
        }

        public Interaction Type
        {
            get { return Interaction.ConditionHowManyUsersInRoom; }
        }

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public string OtherString { get; set; }

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
            var approved = false;

            var minimum = 1;
            var maximum = 50;

            if (!String.IsNullOrWhiteSpace(OtherString))
            {
                var integers = OtherString.Split(',');
                minimum = int.Parse(integers[0]);
                maximum = int.Parse(integers[1]);
            }

            if (Room.RoomData.UsersNow >= minimum && Room.RoomData.UsersNow <= maximum)
                approved = true;

            return approved;
        }
    }
}