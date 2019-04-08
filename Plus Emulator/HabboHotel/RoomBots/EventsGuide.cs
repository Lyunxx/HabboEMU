using System;
using System.Linq;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.RoomBots
{
    /// <summary>
    /// Class GenericBot.
    /// </summary>
    internal class EventsGuide : BotAI
    {
        /// <summary>
        /// The random
        /// </summary>
        private static readonly Random Random = new Random();

        /// <summary>
        /// The _id
        /// </summary>
        private readonly int _id;

        /// <summary>
        /// The _virtual identifier
        /// </summary>
        private readonly int _virtualId;

        /// <summary>
        /// The _is bartender
        /// </summary>
        private readonly bool _isBartender;

        /// <summary>
        /// The _action count
        /// </summary>
        private int _actionCount;

        /// <summary>
        /// The _speech interval
        /// </summary>
        private int _speechInterval;

        /// <summary>
        /// The _chat timer
        /// </summary>
        private Timer _chatTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericBot"/> class.
        /// </summary>
        /// <param name="roomBot">The room bot.</param>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="botId">The bot identifier.</param>
        /// <param name="type">The type.</param>
        /// <param name="isBartender">if set to <c>true</c> [is bartender].</param>
        /// <param name="speechInterval">The speech interval.</param>
        internal EventsGuide(RoomBot roomBot, int virtualId, int botId, AIType type, bool isBartender, int speechInterval)
        {
            _id = botId;
            _virtualId = virtualId;
            _isBartender = isBartender;
            _speechInterval = speechInterval < 2 ? 2000 : speechInterval * 1000;

            // Get random speach
            if (roomBot != null && roomBot.AutomaticChat && roomBot.RandomSpeech != null && roomBot.RandomSpeech.Any()) _chatTimer = new Timer(ChatTimerTick, null, _speechInterval, _speechInterval);
            _actionCount = Random.Next(10, 30 + virtualId);
        }

        /// <summary>
        /// Modifieds this instance.
        /// </summary>
        internal override void Modified()
        {
            if (GetBotData() == null) return;
            if (!GetBotData().AutomaticChat || GetBotData().RandomSpeech == null || !GetBotData().RandomSpeech.Any())
            {
                StopTimerTick();
                return;
            }
            _speechInterval = GetBotData().SpeechInterval < 2 ? 2000 : GetBotData().SpeechInterval * 1000;

            if (_chatTimer == null)
            {
                _chatTimer = new Timer(ChatTimerTick, null, _speechInterval, _speechInterval);
                return;
            }
            _chatTimer.Change(_speechInterval, _speechInterval);
        }

        /// <summary>
        /// Called when [timer tick].
        /// </summary>
        internal override void OnTimerTick()
        {
        }

        /// <summary>
        /// Called when [self enter room].
        /// </summary>
        internal override void OnSelfEnterRoom()
        {
        }

        /// <summary>
        /// Called when [self leave room].
        /// </summary>
        /// <param name="kicked">if set to <c>true</c> [kicked].</param>
        internal override void OnSelfLeaveRoom(bool kicked)
        {
            GetRoomUser().Chat(null, "I hate it when you pick me up <b>*Cries*</b>", false, 0, 0);
        }

        /// <summary>
        /// Called when [user enter room].
        /// </summary>
        /// <param name="user">The user.</param>
        internal override void OnUserEnterRoom(RoomUser user)
        {
        }

        /// <summary>
        /// Called when [user leave room].
        /// </summary>
        /// <param name="client">The client.</param>
        internal override void OnUserLeaveRoom(GameClient client)
        {
        }

        /// <summary>
        /// Called when [user say].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        internal override void OnUserSay(RoomUser user, string message)
        {

            try
            {
                message = message.Substring(1);
            }
            catch
            {
                return;
            }
            switch (message.ToLower())
            {
                case "help":
                    GetRoomUser().Chat(null, "What can I help you with? Say <b><u>brawl</u></b> or <b><u>cw</u></b>", false, 0, 0);
                    return;
                case "brawl":
                    GetRoomUser().Chat(null, "In <b>BRAWL</b> the object of the game is to knockout everyone until you are the last man standing!", false, 0, 0);
                    return;
                case "cw":
                    GetRoomUser().Chat(null, "In <b>COLOR WARS</b> the object of the game is to eliminate the other teams.", false, 0, 0);
                    GetRoomUser().Chat(null, "Inorder todo that you must Capture the other team(s) bases.", false, 0, 0);
                    GetRoomUser().Chat(null, "To capture a base you must type <b>:capture</b> while standing on the tile and stay there for 3 minutes.", false, 0, 0);
                    return;
            }
        }

        /// <summary>
        /// Called when [user shout].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        internal override void OnUserShout(RoomUser user, string message)
        {
            try
            {
                message = message.Substring(1);
            }
            catch
            {
                return;
            }
            switch (message.ToLower())
            {
                case "help":
                    GetRoomUser().Chat(null, "What can I help you with? Say <b><u>brawl</u></b> or <b><u>cw</u></b>", false, 0, 0);
                    return;
                case "brawl":
                    GetRoomUser().Chat(null, "In <b>BRAWL</b> the object of the game is to knockout everyone until you are the last man standing!", false, 0, 0);
                    return;
                case "cw":
                    GetRoomUser().Chat(null, "In <b>COLOR WARS</b> the object of the game is to eliminate the other teams.", false, 0, 0);
                    GetRoomUser().Chat(null, "Inorder todo that you must Capture the other team(s) bases.", false, 0, 0);
                    GetRoomUser().Chat(null, "To capture a base you must type <b>:capture</b> while standing on the tile and stay there for 3 minutes.", false, 0, 0);
                    return;
            }
        }

        /// <summary>
        /// Stops the timer tick.
        /// </summary>
        private void StopTimerTick()
        {
            if (_chatTimer == null) return;
            _chatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _chatTimer.Dispose();
            _chatTimer = null;
        }

        /// <summary>
        /// Chats the timer tick.
        /// </summary>
        /// <param name="o">The o.</param>
        private void ChatTimerTick(object o)
        {
            if (GetBotData() == null || GetRoomUser() == null || GetBotData().WasPicked || GetBotData().RandomSpeech == null ||
                !GetBotData().RandomSpeech.Any())
            {
                StopTimerTick();
                return;
            }

            if (GetRoom() != null && GetRoom().MutedBots)
                return;

            var randomSpeech = GetBotData().GetRandomSpeech(GetBotData().MixPhrases);

            try
            {
                switch (randomSpeech)
                {
                    case ":sit":
                        {
                            var user = GetRoomUser();
                            if (user.RotBody % 2 != 0) user.RotBody--;

                            user.Z = GetRoom().GetGameMap().SqAbsoluteHeight(user.X, user.Y);
                            if (!user.Statusses.ContainsKey("sit"))
                            {
                                user.UpdateNeeded = true;
                                user.Statusses.Add("sit", "0.55");
                            }
                            user.IsSitting = true;
                            return;
                        }
                    case ":stand":
                        {
                            var user = GetRoomUser();
                            if (user.IsSitting)
                            {
                                user.Statusses.Remove("sit");
                                user.IsSitting = false;
                                user.UpdateNeeded = true;
                            }
                            else if (user.IsLyingDown)
                            {
                                user.Statusses.Remove("lay");
                                user.IsLyingDown = false;
                                user.UpdateNeeded = true;
                            }
                            return;
                        }
                }

                if (GetRoom() != null)
                {
                    randomSpeech = randomSpeech.Replace("%user_count%",
                        GetRoom().GetRoomUserManager().GetRoomUserCount().ToString());
                    randomSpeech = randomSpeech.Replace("%item_count%",
                        GetRoom().GetRoomItemHandler().TotalItems.ToString());
                    randomSpeech = randomSpeech.Replace("%floor_item_count%",
                        GetRoom().GetRoomItemHandler().FloorItems.Keys.Count.ToString());
                    randomSpeech = randomSpeech.Replace("%wall_item_count%",
                        GetRoom().GetRoomItemHandler().WallItems.Keys.Count.ToString());

                    if (GetRoom().RoomData != null)
                    {
                        randomSpeech = randomSpeech.Replace("%roomname%", GetRoom().RoomData.Name);
                        randomSpeech = randomSpeech.Replace("%owner%", GetRoom().RoomData.Owner);
                    }
                }
                if (GetBotData() != null) randomSpeech = randomSpeech.Replace("%name%", GetBotData().Name);

                GetRoomUser().Chat(null, randomSpeech, false, 0, 0);
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
            }
        }
    }
}