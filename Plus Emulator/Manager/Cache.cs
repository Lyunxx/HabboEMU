using System;
using System.Collections.Generic;
using System.Threading;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.Manager
{
    public static class Cache
    {
        private static Thread _thread;
        public static bool Working;

        public static void StartProcess()
        {
            _thread = new Thread(Process);
            _thread.Start();
            Working = true;
        }

        public static void StopProcess()
        {
            _thread.Abort();
            Working = false;
        }

        private static void Process()
        {
            while (Working)
            {
                try
                {
                    ClearUserCache();
                    ClearRoomsCache();
                }
                catch (Exception e)
                {
                    Writer.Writer.LogCriticalException(e.ToString());
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Thread.Sleep(1000000);
            }
        }

        private static void ClearUserCache()
        {
            var toRemove = new List<uint>();
            foreach (var user in Plus.UsersCached)
            {
                if (Plus.GetGame().GetClientManager().GetClientByUserId(user.Key) != null) continue;
                if ((DateTime.Now - user.Value.LastUsed).TotalMilliseconds < 1800000) continue;

                toRemove.Add(user.Key);
            }

            foreach (var userId in toRemove)
            {
                Habbo nullHabbo;
                if (Plus.UsersCached.TryRemove(userId, out nullHabbo)) nullHabbo = null;
            }
        }

        private static void ClearRoomsCache()
        {
            if (Plus.GetGame() == null || Plus.GetGame().GetRoomManager() == null ||
                Plus.GetGame().GetRoomManager().LoadedRoomData == null) return;

            var toRemove = new List<uint>();
            foreach (var roomData in Plus.GetGame().GetRoomManager().LoadedRoomData)
            {
                if (roomData.Value == null || roomData.Value.UsersNow > 0) continue;
                if ((DateTime.Now - roomData.Value.LastUsed).TotalMilliseconds < 1800000) continue;

                toRemove.Add(roomData.Key);
            }

            foreach (var roomId in toRemove)
            {
                RoomData nullRoom;
                if (Plus.GetGame().GetRoomManager().LoadedRoomData.TryRemove(roomId, out nullRoom)) nullRoom = null;
            }
        }
    }
}