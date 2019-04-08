using System;
using System.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Plus.Configuration;
using Plus.Database.Manager.Database.Session_Details.Interfaces;

namespace Plus.HabboHotel.Roleplay.Apartments
{
    public class ApartmentManager
    {
        /// <summary>
        /// Thread-safe dictionary containing apartments
        /// </summary>
        public static ConcurrentDictionary<uint, Apartment> Apartments = new ConcurrentDictionary<uint, Apartment>();

        /// <summary>
        /// Boolean representing if the apartments have been initialized
        /// </summary>
        public static bool initiated = false;

        /// <summary>
        /// Caches apartments
        /// </summary>
        public static void init()
        {
            Apartments.Clear();

            string Query = "SELECT * FROM rp_apartments";

            try
            {
                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    
                    dbClient.SetQuery(Query);
                    DataTable apartmentTable = dbClient.GetTable();

                    foreach (DataRow row in apartmentTable.Rows)
                    {
                        uint RoomID = Convert.ToUInt32(row["roomid"]);
                        uint OwnerID = Convert.ToUInt32(row["ownerid"]);

                        bool IsVIP = Convert.ToBoolean(row["vip"]);
                        bool IsForSale = Convert.ToBoolean(row["forsale"]);

                        var apartment = new Apartment(RoomID, OwnerID, IsVIP, IsForSale);
                        Apartments.TryAdd(RoomID, apartment);
                    }
                }
            }
            catch (Exception ex) { Logging.LogQueryError(ex, Query); }
        }

        /// <summary>
        /// Checks if the room is an apart
        /// </summary>
        /// <param name="RoomId">The room to check</param>
        /// <returns>True if it is, otherwise false</returns>
        public static bool isApartment(uint RoomId)
        {
            return Apartments.ContainsKey(RoomId);
        }
    }
}
