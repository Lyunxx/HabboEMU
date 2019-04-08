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
using Plus.Collections;
using System.Net;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Roleplay.Gangs
{
    public class GangManager
    {
        public static ConcurrentDictionary<int, Gang> GangData;
        public static ConcurrentDictionary<int, Turf> TurfData;
        public static MultiKeyDictionary<int, int, GangRank> GangRankData;
        public static void init()
        {
            DataTable Gangs = null;
            DataTable GangsRanks = null;

            GangData = new ConcurrentDictionary<int, Gang>();
            GangRankData = new MultiKeyDictionary<int, int, GangRank>();
            GangData.Clear();
            GangRankData.Clear();

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rp_gangs");

                Gangs = dbClient.GetTable();

                foreach (DataRow Gang in Gangs.Rows)
                {
                    dbClient.SetQuery("SELECT * FROM rp_gangs_ranks WHERE gangid = " + Convert.ToInt32(Gang[0]) + "");

                    GangsRanks = dbClient.GetTable();

                    foreach (DataRow Rank in GangsRanks.Rows)
                    {
                        GangRankData.Add(Convert.ToInt32(Rank[0]), Convert.ToInt32(Rank[1]), new GangRank(Convert.ToInt32(Rank[0]), Convert.ToInt32(Rank[1]), Convert.ToString(Rank[2]), Convert.ToInt32(Rank[3]), Convert.ToInt32(Rank[4]), Convert.ToInt32(Rank[5]), Convert.ToInt32(Rank[6]), Convert.ToInt32(Rank[7]) ));
                    }

                    GangData.TryAdd(Convert.ToInt32(Gang[0]), new Gang(Convert.ToInt32(Gang[0]), Convert.ToString(Gang[1]), Convert.ToInt32(Gang[2]), Convert.ToInt32(Gang[3]), Convert.ToInt32(Gang[4]), Convert.ToInt32(Gang[5])));
                }
            }

            initTurfs();
        }
        public static void initTurfs()
        {
            DataTable Turfs = null;
            TurfData = new ConcurrentDictionary<int, Turf>();
            TurfData.Clear();

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rp_gangs_turfs");
                Turfs = dbClient.GetTable();

                foreach(DataRow Turf in Turfs.Rows)
                {
                    TurfData.TryAdd(Convert.ToInt32(Turf[0]), new Turf(Convert.ToInt32(Turf[0]),Convert.ToInt32(Turf[1]), Convert.ToInt32(Turf[2]), Convert.ToInt32(Turf[3])));
                }
            }
        }
        public static bool GangExists(string Name)
        {
            foreach (KeyValuePair<int, Gang> Gang in GangManager.GangData)
            {
                if (GangManager.GangData[Gang.Key].Name == Name)
                {
                    return true;
                }
            }
            return false;
        }
        public static int GetGangId(string Name)
        {
            foreach (KeyValuePair<int, Gang> Gang in GangManager.GangData)
            {
                if (GangManager.GangData[Gang.Key].Name == Name)
                {
                    return GangManager.GangData[Gang.Key].Id;
                }
            }
            return 0;
        }
        public static bool validGang(int GangId, int RankId)
        {
            return GangRankData.ContainsKey(GangId,RankId) ? true : false;
        }
        public static bool IsTurfSpot(int RoomId, int X, int Y)
        {
            foreach (KeyValuePair<int, Turf> Turf in TurfData)
            {
                if (TurfData.ContainsKey(RoomId))
                {
                    if (TurfData[RoomId].X == X && TurfData[RoomId].Y == Y)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    public class Gang
    {

        #region Variables
        public int Id;
        public string Name;
        public int Owner;
        public int Kills;
        public int Deaths;
        public int Points;
        #endregion

        #region Constructor
        public Gang(int Id, string Name, int Owner, int Kills, int Deaths, int Points)
        {
            this.Id = Id;
            this.Name = Name;
            this.Owner = Owner;
            this.Kills = Kills;
            this.Deaths = Deaths;
            this.Points = Points;
        }
        #endregion

        #region Methods
       
        #endregion

    }
    public class GangRank
    {

        #region Variables
        public int GangId;
        public int RankId;
        public string Name;
        public int Pwr_Recruit;
        public int Pwr_Demote;
        public int Pwr_Promote;
        public int Pwr_Kick;
        public int Pwr_Alert;
        #endregion

        #region Constructor
        public GangRank(int GangId, int RankId, string Name, int Pwr_Recruit, int Pwr_Demote, int Pwr_Promote, int Pwr_Kick, int Pwr_Alert)
        {
            this.GangId = GangId;
            this.RankId = RankId;
            this.Name = Name;
            this.Pwr_Recruit = Pwr_Recruit;
            this.Pwr_Demote = Pwr_Demote;
            this.Pwr_Promote = Pwr_Promote;
            this.Pwr_Kick = Pwr_Kick;
            this.Pwr_Alert = Pwr_Alert;
        }
        #endregion

        #region Methods
        public bool CanRecruit()
        {
            return Pwr_Recruit == 1 ? true : false;
        }
        public bool CanDemote()
        {
            return Pwr_Demote == 1 ? true : false;
        }
        public bool CanPromote()
        {
            return Pwr_Promote == 1 ? true : false;
        }
        public bool CanKick()
        {
            return Pwr_Kick == 1 ? true : false;
        }
        public bool CanAlert()
        {
            return Pwr_Alert == 1 ? true : false;
        }
        #endregion
    }

    public class Turf
    {

        #region Variables
        public int TurfId;
        public int GangId;
        public int X;
        public int Y;
        #endregion

        #region Constructor
        public Turf(int TurfId, int GangId, int X, int Y)
        {
            this.TurfId = TurfId;
            this.GangId = GangId;
            this.X = X;
            this.Y = Y;
        }
        #endregion

        #region Methods

        #endregion

    }
}
