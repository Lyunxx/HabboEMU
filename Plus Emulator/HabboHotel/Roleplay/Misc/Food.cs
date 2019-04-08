using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.RoomInvokedItems;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.RoomBots;
using Plus.HabboHotel.Pets;
using Plus.Messages;
using System.Drawing;
using Plus.Util;
using System.Threading;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Roleplay.Misc
{
    public class Substances
    {
        public static ConcurrentDictionary<string, Food> SubstanceData;
        public static void load()
        {
            SubstanceData = new ConcurrentDictionary<string, Food>();
            SubstanceData.Clear();
            DataTable substances = null;
            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rp_food");
                substances = dbClient.GetTable();
                foreach(DataRow sub in substances.Rows)
                {
                    Food food = new Food(
                        Convert.ToString(sub[0]), 
                        Convert.ToInt32(sub[1]), 
                        Convert.ToInt32(sub[2]), 
                        Convert.ToInt32(sub[3]), 
                        Convert.ToInt32(sub[4]), 
                        Convert.ToInt32(sub[5]), 
                        Convert.ToString(sub[6]), 
                        Convert.ToString(sub[7]), 
                        Convert.ToString(sub[8]), 
                        Convert.ToUInt32(sub[9]), 
                        Convert.ToDouble(sub[10]),
                        Convert.ToString(sub[11]),
                        Convert.ToInt32(sub[12])
                        );
                    SubstanceData.TryAdd(Convert.ToString(sub[0]), food);
                }
            }
        }

        public static Food GetFoodByUName(string uniquename)
        {
            Food ret = null;
            foreach(Food Food in SubstanceData.Values)
            {
                if (Food.UniqueName != uniquename)
                    continue;

                else
                    ret = Food;
            }

            return ret;
        }

        public static Food GetFoodByDName(string displayname)
        {
            Food ret = null;
            foreach (Food Food in SubstanceData.Values)
            {
                if (Food.DisplayName != displayname)
                    continue;

                else
                    ret = Food;
            }

            return ret;
        }

        public static Food GetFoodById(uint Id)
        {
            Food ret = null;
            lock (SubstanceData.Values)
            {
                foreach (Food Food in SubstanceData.Values)
                {
                    if (Food.BaseItemId != Id)
                        continue;

                    else
                        ret = Food;
                }
            }

            return ret;
        }

        public static Food GetDrinkById(uint Id)
        {
            Food ret = null;

            lock (SubstanceData.Values)
            {
                foreach (Food Food in SubstanceData.Values)
                {
                    if (Food.DrinkId != Id)
                        continue;

                    else
                        ret = Food;
                }
            }

            return ret;
        }

        public static bool IsAFood(RoomItem Item)
        {
            Food fo = null;
           foreach(Food Food in SubstanceData.Values)
           {
               if(Food.Item_Name == Item.GetBaseItem().Name)
               {
                   fo = Food;
               }
           }

           return (fo == null) ? false : true;
        }
    }
    public class Food
    {
        public string Item_Name;
        public int Item_Price;
        public int Energy;
        public int Hunger;
        public int Health;
        public int Required_Intel;
        public string Speech;

        public string UniqueName;
        public string DisplayName;
        public uint BaseItemId;
        public double StackHeight;

        public string Drink;
        public int DrinkId;

        public Food(string Item_Name, int Item_Price, int Energy, int Hunger, int Health, int Required_Intel, string Speech, string UniqueName, string DisplayName, uint BaseItemId, double StackHeight, string Drink, int DrinkId)
        {
            this.Item_Name = Item_Name;
            this.Item_Price = Item_Price;
            this.Energy = Energy;
            this.Hunger = Hunger;
            this.Required_Intel = Required_Intel;
            this.Speech = Speech;
            this.UniqueName = UniqueName;
            this.DisplayName = DisplayName;
            this.BaseItemId = BaseItemId;
            this.StackHeight = StackHeight;
            this.Drink = Drink;
            this.DrinkId = DrinkId;
        }
    }

}
