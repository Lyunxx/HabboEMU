using System;
using Plus.Messages;
using Plus.Util;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms
{
    /// <summary>
    /// Class Chatlog.
    /// </summary>
    internal class Chatlog
    {
        /// <summary>
        /// The user identifier
        /// </summary>
        internal uint UserId;

        /// <summary>
        /// The message
        /// </summary>
        internal string Message;

        /// <summary>
        /// The timestamp
        /// </summary>
        internal DateTime TimeStamp;

        /// <summary>
        /// The is saved
        /// </summary>
        internal bool IsSaved, GlobalMessage;

        List<string> CheckIfItIsAllreadyExist_text = new List<string>();
        List<double> CheckIfItIsAllreadyExist_timestamp = new List<double>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Chatlog"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="time">The time.</param>
        /// <param name="globalMessage"></param>
        /// <param name="fromDatabase">if set to <c>true</c> [from database].</param>
        internal Chatlog(uint user, string msg, DateTime time, bool globalMessage, bool fromDatabase = false)
        {
            UserId = user;
            Message = msg;
            TimeStamp = time;
            GlobalMessage = true;
            IsSaved = fromDatabase;
        }

        /// <summary>
        /// Checks if a value is a string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IsString(object value)
        {
            return value is string;
        }

        /// <summary>
        /// Saves the specified room identifier.
        /// </summary>
        /// <param name="queryChunk"></param>
        /// <param name="id">Auto increment</param>
        internal void Save(uint RoomId)
        {

            if(!IsString(Message))
            {
                return;
            }

            if (CheckIfItIsAllreadyExist_text.Contains(Message) && CheckIfItIsAllreadyExist_timestamp.Contains(Plus.DateTimeToUnix(TimeStamp)))
            {
                //Console.WriteLine("[INFO] -> Chatlogs Duplicate attempt!");
            }
            else
            {
                using (var dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("INSERT INTO users_chatlogs (user_id, room_id, message, timestamp) VALUES (@user, @room, @message, @time)");
                    dbClient.AddParameter("user", UserId);
                    dbClient.AddParameter("room", RoomId);
                    dbClient.AddParameter("message", Message);
                    dbClient.AddParameter("time", Plus.DateTimeToUnix(TimeStamp));
                    dbClient.RunQuery();
                    CheckIfItIsAllreadyExist_text.Add(Message);
                    CheckIfItIsAllreadyExist_timestamp.Add(Plus.DateTimeToUnix(TimeStamp));
                }
            }
        }

        internal void Serialize(ref ServerMessage message)
        {
            var habbo = Plus.GetHabboById(UserId);
            message.AppendInteger(Plus.DifferenceInMilliSeconds(TimeStamp, DateTime.Now));
            message.AppendInteger(UserId);
            message.AppendString(habbo == null ? "*User not found*" : habbo.UserName);
            message.AppendString(Message);
            message.AppendBool(GlobalMessage);
        }
    }
}