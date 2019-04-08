#region

using System;
using System.Diagnostics;
using System.Threading;
using Plus.Database.Manager.Database.Session_Details.Interfaces;

#endregion

namespace Plus.HabboHotel.Misc
{
    /// <summary>
    /// Class LowPriorityWorker.
    /// </summary>
    internal class LowPriorityWorker
    {
        /// <summary>
        /// The _user peak
        /// </summary>
        public static int _userPeak;

        private static string _lastDate;

        private static bool isExecuted;

        private static Stopwatch lowPriorityProcessWatch;

        /// <summary>
        /// The _m timer
        /// </summary>
        private static Timer _mTimer;

        /// <summary>
        /// Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal static void Init(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT `userpeak` FROM `server_status`");
            _userPeak = dbClient.GetInteger();

            lowPriorityProcessWatch = new Stopwatch();
            lowPriorityProcessWatch.Start();
        }

        /// <summary>
        /// Starts the processing.
        /// </summary>
        internal static void StartProcessing()
        {
            _mTimer = new Timer(Process, null, 0, 10000);
        }

        /// <summary>
        /// Processes the specified caller.
        /// </summary>
        /// <param name="caller">The caller.</param>
        internal static void Process(object caller)
        {
            if (lowPriorityProcessWatch.ElapsedMilliseconds >= 10000 || !isExecuted)
            {
                isExecuted = true;
                lowPriorityProcessWatch.Restart();

                var clientCount = Plus.GetGame().GetClientManager().ClientCount;
                var loadedRoomsCount = Plus.GetGame().GetRoomManager().LoadedRoomsCount;
                var Uptime = DateTime.Now - Plus.ServerStarted;

                Console.Title = "[FluxRP]  Uptime: " + Uptime.Days + " days and " + Uptime.Hours + " hours | " +
                "Online users: " + clientCount + " | Loaded rooms: " + loadedRoomsCount;

                using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    if (clientCount > _userPeak) _userPeak = clientCount;

                    _lastDate = DateTime.Now.ToShortDateString();
                    queryReactor.RunFastQuery(string.Concat("UPDATE `server_status` SET `stamp` = '", Plus.GetUnixTimeStamp(), "', `users_online` = '", clientCount, "', `rooms_loaded` = '", loadedRoomsCount, "', `server_ver` = 'Plus Emulator', `userpeak` = ", _userPeak));
                }
            }
        }
    }
}