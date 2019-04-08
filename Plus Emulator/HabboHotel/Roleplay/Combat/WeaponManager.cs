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
using System.Drawing;
using Plus.Util;
using System.Threading;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay;
using System.Text.RegularExpressions;
using System.Net;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Roleplay.Combat
{
    public class WeaponManager
    {
        public static ConcurrentDictionary<string, Weapon> WeaponsData;
        public static void init()
        {
            WeaponsData = new ConcurrentDictionary<string, Weapon>();
            WeaponsData.Clear();
            DataTable Weps = null;
            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rp_weapons");
                Weps = dbClient.GetTable();
                foreach(DataRow Weapon in Weps.Rows)
                {
                    WeaponsData.TryAdd(Convert.ToString(Weapon[1]),
                        new Weapon(Convert.ToInt32(Weapon[0]),
                            Convert.ToString(Weapon[1]),
                            Convert.ToInt32(Weapon[2]),
                            Convert.ToInt32(Weapon[3]),
                            Convert.ToInt32(Weapon[4]),
                            Convert.ToInt32(Weapon[5]),
                            Convert.ToInt32(Weapon[6]),
                            Convert.ToInt32(Weapon[7]), 
                            Convert.ToInt32(Weapon[8]), 
                            Convert.ToInt32(Weapon[9]), 
                            Convert.ToString(Weapon[10]),
                            Convert.ToInt32(Weapon[11]),
                            Convert.ToInt32(Weapon[12]), 
                            Convert.ToString(Weapon[13]), 
                            Convert.ToString(Weapon[14]), 
                            Convert.ToInt32(Weapon[15]),
                            Convert.ToInt32(Weapon[16]), 
                            Convert.ToString(Weapon[17]), 
                            Convert.ToString(Weapon[18])
                            ));
                }
            }
        }

        public static bool isWeapon(string name)
        {
            if (WeaponsData.ContainsKey(name))
                return true;

            return false;
        }

        public static string GetWeaponName(string WeaponData)
        {
            string[] data = WeaponData.Split(':');
            return data[0];
        }
    }

    public class Weapon
    {
        public int Id;
        public string Name;
        public int Max_Damage;
        public int Min_Damage;
        public int Range;
        public int Spread;
        public int Price;
        public int Reload_Time;
        public int Min_Intel;
        public int Gun_Training_Multiplyer;
        public string Type;
        public int Need_Energy;
        public int Need_Str;
        public string Weapon_Units;
        public string Speech;
        public int Effect_Id;
        public int HandItem;
        public string Equip_Msg;
        public string DisplayName;
        public Weapon(int Id, string Name, int Max_Damage, int Min_Damage, int Range, int Spread, int Price, int Reload_Time, int Min_Intel, int Gun_Training_Multiplyer, string Type, int Need_Energy, int Need_Str, string Weapon_Units, string Speech, int Effect_Id, int HandItem, string Equip_Msg, string DisplayName)
        {
            this.Id = Id;
            this.Name = Name;
            this.Max_Damage = Max_Damage;
            this.Min_Damage = Min_Damage;
            this.Range = Range;
            this.Spread = Spread;
            this.Price = Price;
            this.Reload_Time = Reload_Time;
            this.Min_Intel = Min_Intel;
            this.Gun_Training_Multiplyer = Gun_Training_Multiplyer;
            this.Type = Type;
            this.Need_Energy = Need_Energy;
            this.Need_Str = Need_Str;
            this.Weapon_Units = Weapon_Units;
            this.Speech = Speech;
            this.Effect_Id = Effect_Id;
            this.HandItem = HandItem;
            this.Equip_Msg = Equip_Msg;
            this.DisplayName = DisplayName;
        }
    }
}
