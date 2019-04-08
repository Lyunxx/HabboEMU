using Plus.Database.Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plus.Security
{
    internal class AntiPublicistas
    {
        internal static string[] bannedHotels;

        internal static void Load(IQueryAdapter DBClient)
        {
            DBClient.SetQuery("SELECT * FROM server_bannedhotels");
            DataTable Table = DBClient.GetTable();
            bannedHotels = new string[Table.Rows.Count];

            int i = 0;
            foreach (DataRow dataRow in DBClient.GetTable().Rows)
            {
                AntiPublicistas.bannedHotels[i] = (dataRow[0].ToString());
                i++;
            }
        }

        internal static bool CheckPublicistas(string Message)
        {
            if (string.IsNullOrWhiteSpace(Message))
            {
                return false;
            }

            Message = Message.ToLower().Replace(" ", "").Replace(".", "").Replace("/", "").Replace("-", "").Replace("_", "").Replace("*", "").Replace("0", "o").Replace("4", "a").Replace("1", "i");


            return bannedHotels.Any(Message.Contains) && !Message.Contains("ban") && !Message.Contains("mip");
        }

        internal static void AddBannedHotel(string Message)
        {
            using (var adapter = Plus.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("INSERT INTO server_bannedhotels VALUES (@word)");
                adapter.AddParameter("word", Message);
                adapter.RunQuery();

                AntiPublicistas.Load(adapter);
            }
        }
    }
}
