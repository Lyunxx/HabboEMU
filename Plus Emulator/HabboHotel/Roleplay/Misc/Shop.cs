using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HabboHotel.Roleplay.Misc
{
    public enum ShopType
    {
        GunShop,
        Food,
        Drinks,
        Uncategorised,
        Properties
    }
    public class Shop
    {

    }

    public class ShopData
    {
        public int RoomId;
        public uint OwnerId;
        public string Type;
        public string WeaponData;
        public string DrinkData;
        public string FoodData;

        public ShopData(int RoomId, uint OwnerId, string Type, string WeaponData, string DrinkData, string FoodData)
        {
            this.RoomId = RoomId;
            this.OwnerId = OwnerId;
            this.Type = Type;
            this.WeaponData = WeaponData;
            this.DrinkData = DrinkData;
            this.FoodData = FoodData;
        }

        //public ShopType ShopType
        //{
        //    get
        //    {
        //        ShopType type = (this.Type == "properties") : 
        //    }
        //}
    }
}
