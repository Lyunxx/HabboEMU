using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.GameClients;


namespace Plus.HabboHotel.Support
{
    public sealed class ModerationBanManager
    {
        private Dictionary<string, ModerationBan> _bans = new Dictionary<string, ModerationBan>();

        public ModerationBanManager()
        {
            this.Init();
        }

        public void Init()
        {
            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                DataTable GetBans = null;
                dbClient.SetQuery("SELECT `bantype`,`value`,`reason`,`expire` FROM `users_bans` WHERE `bantype` = 'machine' OR `bantype` = 'user'");
                GetBans = dbClient.GetTable();

                if (GetBans != null)
                {
                    foreach (DataRow dRow in GetBans.Rows)
                    {
                        string value = Convert.ToString(dRow["value"]);
                        string reason = Convert.ToString(dRow["reason"]);
                        double expires = (double)dRow["expire"];
                        string type = Convert.ToString(dRow["bantype"]);

                        ModerationBan Ban = new ModerationBan(BanTypeUtility.GetModerationBanType(type), value, reason, expires);
                        if (Ban != null)
                        {
                            if (expires > Plus.GetUnixTimeStamp())
                            {
                                if (!this._bans.ContainsKey(value))
                                    this._bans.Add(value, Ban);
                            }
                        }
                    }
                }
            }
        }

        public void ReCacheBans()
        {
            if (this._bans.Count > 0)
                this._bans.Clear();

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                DataTable GetBans = null;
                dbClient.SetQuery("SELECT `bantype`,`value`,`reason`,`expire` FROM `users_bans` WHERE `bantype` = 'machine' OR `bantype` = 'user'");
                GetBans = dbClient.GetTable();

                if (GetBans != null)
                {
                    foreach (DataRow dRow in GetBans.Rows)
                    {
                        string value = Convert.ToString(dRow["value"]);
                        string reason = Convert.ToString(dRow["reason"]);
                        double expires = (double)dRow["expire"];
                        string type = Convert.ToString(dRow["bantype"]);

                        ModerationBan Ban = new ModerationBan(BanTypeUtility.GetModerationBanType(type), value, reason, expires);
                        if (Ban != null)
                        {
                            if (expires > Plus.GetUnixTimeStamp())
                            {
                                if (!this._bans.ContainsKey(value))
                                    this._bans.Add(value, Ban);
                            }
                        }
                    }
                }
            }
        }

        public void BanUser(GameClient client, string Mod, ModerationBanType Type, string BanValue, string Reason, double ExpireTimestamp)
        {
            string BanType = (Type == ModerationBanType.IP ? "ip" : Type == ModerationBanType.MACHINE ? "machine" : "user");
            double BanExpire = Plus.GetUnixTimeStamp() + ExpireTimestamp;

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `users_bans` (`bantype`, `value`, `reason`, `expire`, `added_by`,`added_date`) VALUES ('" + BanType + "', '" + BanValue + "', @reason, " + BanExpire + ", '" + Mod + "', '" + DateTime.Now.ToLongDateString() + "');");
                dbClient.AddParameter("reason", Reason);
                dbClient.RunQuery();
            }

            if (Type == ModerationBanType.MACHINE || Type == ModerationBanType.USERNAME)
            {
                if (!this._bans.ContainsKey(BanValue))
                    this._bans.Add(BanValue, new ModerationBan(Type, BanValue, Reason, BanExpire));
            }

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE `users_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + client.GetHabbo().Id + "' LIMIT 1");
            }

            client.Disconnect("banned");
        }

        /// <summary>
        /// Runs a quick check to see if a ban record is cached in the server.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Ban"></param>
        /// <returns></returns>
        public bool IsBanned(string Key, out ModerationBan Ban)
        {
            if (this._bans.TryGetValue(Key, out Ban))
            {
                if (!Ban.Expired)
                    return true;

                //And finally, let us remove the ban record from the cache.
                if (this._bans.ContainsKey(Key))
                    this._bans.Remove(Key);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Run a quick database check to see if this ban exists in the database.
        /// </summary>
        /// <param name="MachineId">The value of the ban.</param>
        /// <returns></returns>
        public bool MachineBanCheck(string MachineId)
        {
            ModerationBan MachineBanRecord = null;
            if (Plus.GetGame().GetBanManager().IsBanned(MachineId, out MachineBanRecord))
            {
                DataRow BanRow = null;
                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `users_bans` WHERE `bantype` = 'machine' AND `value` = @value LIMIT 1");
                    dbClient.AddParameter("value", MachineId);
                    BanRow = dbClient.GetRow();

                    //If there is no more ban record, then we can simply remove it from our cache!
                    if (BanRow == null)
                    {
                        Plus.GetGame().GetBanManager().RemoveBan(MachineId);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Run a quick database check to see if this ban exists in the database.
        /// </summary>
        /// <param name="Username">The value of the ban.</param>
        /// <returns></returns>
        public bool UsernameBanCheck(string Username)
        {
            ModerationBan UsernameBanRecord = null;
            if (Plus.GetGame().GetBanManager().IsBanned(Username, out UsernameBanRecord))
            {
                DataRow BanRow = null;
                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `users_bans` WHERE `bantype` = 'user' AND `value` = @value LIMIT 1");
                    dbClient.AddParameter("value", Username);
                    BanRow = dbClient.GetRow();

                    //If there is no more ban record, then we can simply remove it from our cache!
                    if (BanRow == null)
                    {
                        Plus.GetGame().GetBanManager().RemoveBan(Username);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Remove a ban from the cache based on a given value.
        /// </summary>
        /// <param name="Value"></param>
        public void RemoveBan(string Value)
        {
            this._bans.Remove(Value);
        }
    }
}
