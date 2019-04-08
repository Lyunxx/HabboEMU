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

namespace Plus.HabboHotel.Roleplay.Jobs
{
    public class JobManager
    {
        public static ConcurrentDictionary<int, Job> JobData;
        public static MultiKeyDictionary<int, int, Rank> JobRankData;
        public static void init()
        {

            DataTable Jobs = null;
            DataTable JobsRanks = null;
           
            JobData = new ConcurrentDictionary<int, Job>();
            JobRankData = new MultiKeyDictionary<int, int, Rank>();
            JobData.Clear();
            JobRankData.Clear();

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rp_jobs");

                Jobs = dbClient.GetTable();

                foreach(DataRow Job in Jobs.Rows)
                {
                    dbClient.SetQuery("SELECT * FROM rp_jobs_ranks WHERE job_id = " + Convert.ToInt32(Job[0]) + "");

                    JobsRanks = dbClient.GetTable();

                    foreach(DataRow Rank in JobsRanks.Rows)
                    {
                        JobRankData.Add(Convert.ToInt32(Rank[0]),Convert.ToInt32(Rank[1]), new Rank(Convert.ToInt32(Rank[0]), Convert.ToInt32(Rank[1]), Rank[2].ToString(), Rank[3].ToString(), Rank[4].ToString(), Convert.ToInt32(Rank[5]), Convert.ToInt32(Rank[6]), Rank[7].ToString(), Rank[9].ToString(), Convert.ToInt32(Rank[10]), Convert.ToInt32(Rank[11]), Convert.ToInt32(Rank[12]), Convert.ToInt32(Rank[13]), Convert.ToInt32(Rank[14]), Convert.ToInt32(Rank[15]), Convert.ToInt32(Rank[16])));
                    }

                    JobData.TryAdd(Convert.ToInt32(Job[0]), new Job(Convert.ToInt32(Job[0]),Job[1].ToString(), Job[2].ToString(), Convert.ToInt32(Job[4]), Convert.ToInt32(Job[6]), Job[7].ToString(), Convert.ToInt32(Job[8]), Convert.ToInt32(Job[9]), Convert.ToInt32(Job[10]), Convert.ToInt32(Job[11]), Job[3].ToString()));
                }
            }
        }
        public static bool validJob(int JobId, int RankId)
        {
            return JobRankData.ContainsKey(JobId, RankId) ? true : false;
        }

    }

    public class Job
    {

        #region Variables
        public int Id;
        public string Name;
        public string Desc;
        public int OwnerId;
        public int Headquarters;
        public string Type;
        public int Balance;
        public int Stock;
        public int Tax_PRSI;
        public int Tax_PAYE;
        public string Badge;
        #endregion

        #region Methods

        #endregion

        #region Constructor
        public Job(int Id, string Name, string Desc, int OwnerId, int Headquarters, string Type, int Balance, int Stock, int Tax_PRSI, int Tax_PAYE, string Badge)
        {
            this.Id = Id;
            this.Name = Name;
            this.Desc = Desc;
            this.OwnerId = OwnerId;
            this.Headquarters = Headquarters;
            this.Type = Type;
            this.Balance = Balance;
            this.Stock = Stock;
            this.Tax_PAYE = Tax_PAYE;
            this.Tax_PRSI = Tax_PRSI;
            this.Badge = Badge;
        }
        #endregion

    }

    public class Rank
    {

        #region Variables
        public int JobId;
        public int RankId;
        public string Name;
        public string MaleFig;
        public string FemaleFig;
        public int Pay;
        public int PayTime;
        public string WorkRooms;
        public string SpecialCommands;
        public int UniqueId;
        public int Pwr_Hire;
        public int Pwr_Fire;
        public int Pwr_Demote;
        public int Pwr_Promote;
        public int Pwr_SendHome;
        public int Pwr_Alert;
        #endregion

        #region Methods
        public bool canHire()
        {
            return Pwr_Hire == 1 ? true : false;
        }
        public bool canFire()
        {
            return Pwr_Fire == 1 ? true : false;
        }
        public bool canPromote()
        {
            return Pwr_Promote == 1 ? true : false;
        }
        public bool canDemote()
        {
            return Pwr_Demote == 1 ? true : false;
        }
        public bool canSendHome()
        {
            return Pwr_SendHome == 1 ? true : false;
        }
        public bool canAlert()
        {
            return Pwr_Alert == 1 ? true : false;
        }
        public bool isWorkRoom(uint RoomId)
        {
            List<uint> WorkRoomsList = new List<uint>();

            if (!WorkRooms.Contains("*"))
            {
                string[] Rooms = WorkRooms.Split(',');
                
                foreach (string Room in Rooms)
                {
                    uint Id = Convert.ToUInt32(Room);
                    WorkRoomsList.Add(Id);
                }
            }

            return (WorkRoomsList.Contains(RoomId) ? true : false) || (WorkRooms.Contains("*"));
        }
        public bool hasRights(string right)
        {
            if (SpecialCommands.ToLower().Contains(right))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Constructor
        public Rank(int JobId, int RankId, string Name, string MaleFig, string FemaleFig, int Pay, int PayTime, string WorkRooms, string SpecialCommands, int UniqueId, int Pwr_Hire, int Pwr_Fire, int Pwr_Demote, int Pwr_Promote, int Pwr_SendHome, int Pwr_Alert)
        {
            this.JobId = JobId;
            this.RankId = RankId;
            this.Name = Name;
            this.MaleFig = MaleFig;
            this.FemaleFig = FemaleFig;
            this.Pay = Pay;
            this.PayTime = PayTime;
            this.WorkRooms = WorkRooms;
            this.SpecialCommands = SpecialCommands;
            this.UniqueId = UniqueId;
            this.Pwr_Hire = Pwr_Hire;
            this.Pwr_Fire = Pwr_Fire;
            this.Pwr_Demote = Pwr_Demote;
            this.Pwr_Promote = Pwr_Promote;
            this.Pwr_SendHome = Pwr_SendHome;
            this.Pwr_Alert = Pwr_Alert;
        }
        #endregion

    }
}
