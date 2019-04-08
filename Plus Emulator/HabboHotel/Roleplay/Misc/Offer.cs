using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.RoomInvokedItems;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.RoomBots;
using Plus.HabboHotel.Pets;
using Plus.Messages;
using Plus.Database;
using System.Drawing;
using Plus.Util;
using System.Threading;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay;
using System.Text.RegularExpressions;
using System.Net;
namespace Plus.HabboHotel.Roleplay.Misc
{
    public class Offer
    {
        public GameClient OfferUser;
        public string Offering;
        public int OfferAmount;
        public int OfferPrice;
        
        public Offer(GameClient OfferUser, string Offering, int OfferAmount, int OfferPrice)
        {
            this.OfferUser = OfferUser;
            this.Offering = Offering;
            this.OfferAmount = OfferAmount;
            this.OfferPrice = OfferPrice;
        }
    }
}
