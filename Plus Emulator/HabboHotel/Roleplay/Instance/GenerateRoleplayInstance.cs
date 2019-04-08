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
using Plus.HabboHotel.Roleplay.Combat;
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
using Plus.HabboHotel.Roleplay.Instance;
using System.Net;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Roleplay.Instance
{
    public class CreateRoleplayInstance
    {
        public static RoleplayInstance GenerateInstance(DataRow RPStats)
        {

            int UserID = Convert.ToInt32(RPStats["id"]);

            ConcurrentDictionary<string, Weapon> Weapons = LoadAndReturnWeapons(UserID);
            List<uint> BlockedTexters = LoadAndReturnTexters(UserID);
            Dictionary<string, uint> MyPets = LoadAndReturnPets(UserID);

            //Console.WriteLine(Weapons.Count);

            return new RoleplayInstance(Convert.ToUInt32(RPStats["id"]), Convert.ToInt32(RPStats["curhealth"]), Convert.ToInt32(RPStats["maxhealth"]),
                Convert.ToInt32(RPStats["energy"]), Convert.ToInt32(RPStats["hunger"]), Convert.ToInt32(RPStats["hygiene"]),
                Convert.ToInt32(RPStats["st_deaths"]), Convert.ToInt32(RPStats["st_kills"]), Convert.ToInt32(RPStats["st_punches"]),
                Convert.ToInt32(RPStats["st_arrested"]), Convert.ToInt32(RPStats["st_arrests"]), (Convert.ToInt32(RPStats["dead"]) == 1) ? true : false,
                (Convert.ToInt32(RPStats["jailed"]) == 1) ? true : false, (Convert.ToInt32(RPStats["armored"]) == 1) ? true : false, Convert.ToInt32(RPStats["dead_timer"]), Convert.ToInt32(RPStats["jail_timer"]),
                Convert.ToInt32(RPStats["last_x"]), Convert.ToInt32(RPStats["last_y"]), Convert.ToInt32(RPStats["last_z"]), Convert.ToInt32(RPStats["work_timer"]),
                (Convert.ToInt32(RPStats["is_noob"]) == 1) ? true : false, Convert.ToInt32(RPStats["stamina"]), Convert.ToInt32(RPStats["constitution"]), Convert.ToInt32(RPStats["strength"]),
                Convert.ToInt32(RPStats["intelligence"]), Convert.ToInt32(RPStats["job_id"]), Convert.ToInt32(RPStats["job_rank"]),
                Convert.ToInt32(RPStats["sendhome_timer"]), Convert.ToInt32(RPStats["gang_id"]), Convert.ToInt32(RPStats["gang_rank"]),
                Convert.ToInt32(RPStats["phone"]), Convert.ToInt32(RPStats["phone_credit"]), Convert.ToInt32(RPStats["bank"]), Convert.ToInt32(RPStats["weed"]), Weapons,
                Convert.ToInt32(RPStats["bullets"]), Convert.ToInt32(RPStats["vests"]), Convert.ToInt32(RPStats["armor"]), Convert.ToInt32(RPStats["wanted"]),
                Convert.ToInt32(RPStats["workout_cur_timer"]), Convert.ToInt32(RPStats["workout_need_timer"]), Convert.ToInt32(RPStats["weightlift_cur_timer"]), Convert.ToInt32(RPStats["weightlift_need_timer"]),
                (Convert.ToInt32(RPStats["robbery"]) == 1) ? true : false, (Convert.ToInt32(RPStats["atmrobbery"]) == 1) ? true : false, (Convert.ToInt32(RPStats["learning"]) == 1) ? true : false, 
                Convert.ToInt32(RPStats["plane"]), Convert.ToInt32(RPStats["fuel"]), Convert.ToInt32(RPStats["car"]), 
                Convert.ToInt32(RPStats["dragon_green"]), Convert.ToInt32(RPStats["dragon_blue"]), Convert.ToInt32(RPStats["dragon_silver"]), Convert.ToInt32(RPStats["dragon_erosphere1"]), Convert.ToInt32(RPStats["dragon_erosphere2"]), 
                Convert.ToInt32(RPStats["bombs"]), Convert.ToInt32(RPStats["brawl_pts"]), Convert.ToInt32(RPStats["infection_pts"]), Convert.ToInt32(RPStats["married_to"]), Convert.ToInt32(RPStats["crowbar"]),
                Convert.ToInt32(RPStats["meleekills"]), Convert.ToInt32(RPStats["punchkills"]), Convert.ToInt32(RPStats["gunkills"]), Convert.ToInt32(RPStats["bombkills"]), Convert.ToString(RPStats["lastkilled"]),
                Convert.ToInt32(RPStats["woodLevel"]), Convert.ToInt32(RPStats["woodXP"]), Convert.ToInt32(RPStats["spaceLevel"]), Convert.ToInt32(RPStats["spaceXP"]), Convert.ToInt32(RPStats["farmingLevel"]), Convert.ToInt32(RPStats["farmingXP"]), Convert.ToInt32(RPStats["carrots"]),
                Convert.ToString(RPStats["class"]), BlockedTexters, (Convert.ToInt32(RPStats["vip_a_banned"]) == 1) ? true : false, Convert.ToString(RPStats["last_cw_team"]), Convert.ToInt32(RPStats["shifts_completed"]),
                MyPets);
        }
        internal static List<uint> LoadAndReturnTexters(int UserID)
        {
            DataTable TexterT = null;
            List<uint> BlockedTexters = new List<uint>();
            BlockedTexters.Clear();

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rp_user_textblocks WHERE receiver_id = '" + UserID + "'");
                TexterT = dbClient.GetTable();
                
                if (TexterT != null)
                {
                    foreach (DataRow Row in TexterT.Rows)
                    {
                        if (!BlockedTexters.Contains(Convert.ToUInt32(Row["blocked_id"])))
                        {
                            BlockedTexters.Add(Convert.ToUInt32(Row["blocked_id"]));
                        }
                    }
                }
            }

            return BlockedTexters;
        }
        internal static ConcurrentDictionary<string, Weapon> LoadAndReturnWeapons(int UserID)
        {
            DataTable Weps = null;
            ConcurrentDictionary<string, Weapon> Weapons = new ConcurrentDictionary<string, Weapon>();
            Weapons.Clear();

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rp_user_weapons WHERE id = '" + UserID + "'");
                Weps = dbClient.GetTable();
                Weapons = new ConcurrentDictionary<string,Weapon>();

                if (Weps != null)
                {
                    foreach (DataRow Row in Weps.Rows)
                    {
                        if (!Weapons.ContainsKey(Convert.ToString(Row["weapon_data"])))
                        {
                            string Name = WeaponManager.GetWeaponName(Convert.ToString(Row["weapon_data"]));
                            if (WeaponManager.WeaponsData.ContainsKey(Name))
                            {
                                Weapons.TryAdd(Convert.ToString(Row["weapon_data"]), WeaponManager.WeaponsData[Name]);
                            }
                        }
                    }
                }
            }

            return Weapons;
        }
        internal static Dictionary<string, uint> LoadAndReturnPets(int UserID)
        {


            Dictionary<string, uint> Pets = new Dictionary<string, uint>();

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM bots WHERE user_id = '" + UserID + "'");
                DataTable Data = dbClient.GetTable();

                foreach (DataRow PetRow in Data.Rows)
                {
                    string PetName = Convert.ToString(PetRow["name"]).ToLower();
                    uint PetId = Convert.ToUInt32(PetRow["id"]);


                    if (!Pets.ContainsKey(PetName))
                    {
                        Pets.Add(PetName, PetId);
                    }
                }
            }

            return Pets;
        }
    }
}
