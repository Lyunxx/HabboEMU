using Plus.HabboHotel.Items;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class BotHasHanditem : IWiredItem
    {
        public BotHasHanditem(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type
        {
            get { return Interaction.ConditionBotHasHanditem; }
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
            return true;
        }
    }
}