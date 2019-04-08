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
    internal class GenericBot : BotAI
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
        internal GenericBot(RoomBot roomBot, int virtualId, int botId, AIType type, bool isBartender, int speechInterval)
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
            if (GetBotData() == null) return;

            if (_actionCount > 0)
            {
                _actionCount--;
                return;
            }
            _actionCount = Random.Next(5, 45);

            switch (GetBotData().WalkingMode.ToLower())
            {
                case "freeroam":
                    {
                        var randomPoint = GetRoom().GetGameMap().GetRandomWalkableSquare();
                        if (randomPoint.X == 0 || randomPoint.Y == 0) return;

                        GetRoomUser().MoveTo(randomPoint.X, randomPoint.Y);
                        break;
                    }
                case "specified_range":
                    {
                        var list = GetRoom().GetGameMap().WalkableList.ToList();
                        if (!list.Any()) return;

                        var randomNumber = new Random(DateTime.Now.Millisecond + _virtualId ^ 2).Next(0, list.Count - 1);
                        GetRoomUser().MoveTo(list[randomNumber].X, list[randomNumber].Y);
                        break;
                    }
            }
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
            if (Gamemap.TileDistance(GetRoomUser().X, GetRoomUser().Y, user.X, user.Y) > 16) return;

            if (!_isBartender) return;

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
                case "comehere":
                case "come here":
                case "come":
                    GetRoomUser().Chat(null, "Will do!", false, 0, 0);
                    GetRoomUser().MoveTo(user.SquareInFront);
                    return;

                case "serve":
                    if (GetRoom().CheckRights(user.GetClient()))
                    {
                        foreach (var current in GetRoom().GetRoomUserManager().GetRoomUsers()) current.CarryItem(Random.Next(1, 38));
                        GetRoomUser().Chat(null, "Worth. Now you have something to gobble all.", false, 0, 0);
                        return;
                    }
                    return;

                case "tea":
                case "juice":
                case "water":
                    GetRoomUser().Chat(null, "Here you go.", false, 0, 0);
                    user.CarryItem(Random.Next(1, 3));
                    return;

                case "icecream":
                case "ice cream":
                    GetRoomUser().Chat(null, "Here you go. That is not the language you fit close, hehe!", false, 0, 0);
                    user.CarryItem(4);
                    return;

                case "rose":
                case "rosa":
                    GetRoomUser().Chat(null, "Here you go... you do well on your appointment.", false, 0, 0);
                    user.CarryItem(Random.Next(1000, 1002));
                    return;

                case "sunflower":
                    GetRoomUser().Chat(null, "Here's some very nice nature sunflower.", false, 0, 0);
                    user.CarryItem(1002);
                    return;

                case "flower":
                    GetRoomUser().Chat(null, "Here's some very nice nature flower.", false, 0, 0);
                    if (Random.Next(1, 3) == 2)
                    {
                        user.CarryItem(Random.Next(1019, 1024));
                        return;
                    }
                    user.CarryItem(Random.Next(1006, 1010));
                    return;

                case "carrot":
                    GetRoomUser().Chat(null, "Here's a good vegetable. Enjoy!", false, 0, 0);
                    user.CarryItem(3);
                    return;

                case "capuccino":
                case "coffee":
                case "latte":
                case "mocha":
                case "espresso":
                case "expreso":
                    GetRoomUser().Chat(null, "Here's your coffee. It's sparkling!", false, 0, 0);
                    user.CarryItem(Random.Next(11, 18));
                    return;

                case "fruit":
                    GetRoomUser().Chat(null, "Here's some healthy, fresh and natural. Enjoy it!", false, 0, 0);
                    user.CarryItem(Random.Next(36, 40));
                    return;

                case "orange":
                    GetRoomUser().Chat(null, "Here's some healthy, fresh and natural. Enjoy it!", false, 0, 0);
                    user.CarryItem(38);
                    return;

                case "manzana":
                case "apple":
                    GetRoomUser().Chat(null, "Here's some healthy, fresh and natural. Enjoy it!", false, 0, 0);
                    user.CarryItem(37);
                    return;

                case "cola":
                case "habbocola":
                case "habbo cola":
                case "coca cola":
                case "cocacola":
                    GetRoomUser().Chat(null, "Here is some quite famous soft drink.", false, 0, 0);
                    user.CarryItem(19);
                    return;

                case "pear":
                    GetRoomUser().Chat(null, "Here's some healthy, fresh and natural. Enjoy it!", false, 0, 0);
                    user.CarryItem(36);
                    return;

                case "pineapple":
                    GetRoomUser().Chat(null, "Here's some healthy, fresh and natural. Enjoy it!", false, 0, 0);
                    user.CarryItem(39);
                    return;

                case "penis":
                case "idiot":
                case "fuck":
                case "bitch":
                case "jerk":
                case "bullshit":
                case "dick":
                case "pussy":
                case "deez nuts":
                case "stfu":
                case "gtfo":
                    GetRoomUser().Chat(null, "Do not treat me well, eh!", true, 0, 0);
                    return;

                case "beautiful":
                case "handsome":
                case "love":
                case "i love you":
                    GetRoomUser().Chat(null, "I am a bot, err ... this is getting uncomfortable, you know?", false, 0, 0);
                    return;
            }
            GetRoomUser().Chat(null, "Do you need something?", false, 0, 0);
        }

        /// <summary>
        /// Called when [user shout].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        internal override void OnUserShout(RoomUser user, string message)
        {
            if (_isBartender)
            {
                GetRoomUser()
                    .Chat(null, "I do not rely you screaming. If you want me to serve something, say it well.", false, 0, 0);
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