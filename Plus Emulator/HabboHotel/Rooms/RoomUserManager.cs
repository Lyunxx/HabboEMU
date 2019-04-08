using Plus.Collections;
using Plus.Configuration;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Navigators;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.Pets;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.RoomBots;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Rooms.Games;
using Plus.Messages;
using Plus.Messages.Parsers;
using Plus.Util;
using System;
using System.Data;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Plus.HabboHotel.Roleplay.Minigames.Colour_Wars;
using Plus.HabboHotel.Roleplay.Bots;

namespace Plus.HabboHotel.Rooms
{
    /// <summary>
    /// Class RoomUserManager.
    /// </summary>
    internal class RoomUserManager
    {

        public static int LastUpdateTime;
        private int ActionTick = 0;
        private int ActionTick2 = 0;
        private int ActionTick3 = 0;
        private bool ActionBool = false;
        private bool ActionBool2 = false;
        private bool ActionBool3 = false;

        /// <summary>
        /// The users by user name
        /// </summary"
        internal HybridDictionary UsersByUserName;

        /// <summary>
        /// The users by user identifier
        /// </summary>
        internal HybridDictionary UsersByUserId;

        /// <summary>
        /// To set
        /// </summary>
        internal Dictionary<Point, RoomUser> ToSet;

        /// <summary>
        /// The _to remove
        /// </summary>
        private readonly List<RoomUser> _toRemove;

        /// <summary>
        /// The _room
        /// </summary>
        private Room _room;

        /// <summary>
        /// The _pets
        /// </summary>
        private HybridDictionary _pets;

        /// <summary>
        /// The _bots
        /// </summary>
        private HybridDictionary _bots;

        /// <summary>
        /// The _user count
        /// </summary>
        private uint _userCount;

        /// <summary>
        /// The _primary private user identifier
        /// </summary>
        private int _primaryPrivateUserId;

        /// <summary>
        /// The _secondary private user identifier
        /// </summary>
        private int _secondaryPrivateUserId;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomUserManager"/> class.
        /// </summary>
        /// <param name="room">The room.</param>
        public RoomUserManager(Room room)
        {
            _room = room;
            UserList = new ConcurrentDictionary<int, RoomUser>();
            _pets = new HybridDictionary();
            _bots = new HybridDictionary();
            UsersByUserName = new HybridDictionary();
            UsersByUserId = new HybridDictionary();
            _primaryPrivateUserId = 0;
            _secondaryPrivateUserId = 0;
            _toRemove = new List<RoomUser>((int)room.RoomData.UsersMax);
            ToSet = new Dictionary<Point, RoomUser>();
            PetCount = 0;
            _userCount = 0;
        }

        internal event RoomEventDelegate OnUserEnter;

        /// <summary>
        /// Gets the pet count.
        /// </summary>
        /// <value>The pet count.</value>
        internal int PetCount { get; private set; }

        /// <summary>
        /// Gets the user list.
        /// </summary>
        /// <value>The user list.</value>
        internal ConcurrentDictionary<int, RoomUser> UserList { get; private set; }

        /// <summary>
        /// Gets the room user by habbo.
        /// </summary>
        /// <param name="pId">The p identifier.</param>
        /// <returns>RoomUser.</returns>
        public RoomUser GetRoomUserByHabbo(uint pId)
        {
            return UsersByUserId.Contains(pId) ? (RoomUser)UsersByUserId[pId] : null;
        }

        /// <summary>
        /// Gets the room user count.
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal int GetRoomUserCount()
        {
            return (UserList.Count - _bots.Count - _pets.Count);
        }

        internal List<RoomUser> GetBots()
        {
            List<RoomUser> Bots = new List<RoomUser>();

            foreach (RoomUser roomUser in UserList.Values)
            {
                if (roomUser.IsBot)
                    Bots.Add(roomUser);
            }

            return Bots;
        }

        /// <summary>
        /// Deploys the bot.
        /// </summary>
        /// <param name="bot">The bot.</param>
        /// <param name="petData">The pet data.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser DeployBot(RoomBot bot, Pet petData)
        {
            var virtualId = _primaryPrivateUserId++;
            var roomUser = new RoomUser(0u, _room.RoomId, virtualId, _room, false);
            var num = _secondaryPrivateUserId++;
            roomUser.InternalRoomId = num;
            UserList.TryAdd(num, roomUser);
            OnUserAdd(roomUser);

            var model = _room.GetGameMap().Model;
            var coord = new Point(bot.X, bot.Y);
            if (bot.X > 0 && bot.Y > 0 && bot.X < model.MapSizeX && bot.Y < model.MapSizeY)
            {
                _room.GetGameMap().AddUserToMap(roomUser, coord);
                roomUser.SetPos(bot.X, bot.Y, bot.Z);
                roomUser.SetRot(bot.Rot, false);
            }
            else
            {
                bot.X = model.DoorX;
                bot.Y = model.DoorY;
                roomUser.SetPos(model.DoorX, model.DoorY, model.DoorZ);
                roomUser.SetRot(model.DoorOrientation, false);
            }

            bot.RoomUser = roomUser;
            roomUser.BotData = bot;

            {
                roomUser.BotAI = bot.GenerateBotAI(roomUser.VirtualId, (int)bot.BotId);
                if (roomUser.IsPet)
                {
                    roomUser.BotAI.Init(bot.BotId, roomUser.VirtualId, _room.RoomId, roomUser, _room);
                    roomUser.PetData = petData;
                    roomUser.PetData.VirtualId = roomUser.VirtualId;
                }
                else
                    roomUser.BotAI.Init(bot.BotId, roomUser.VirtualId, _room.RoomId, roomUser, _room);

                UpdateUserStatus(roomUser, false);
                roomUser.UpdateNeeded = true;
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
                serverMessage.AppendInteger(1);
                roomUser.Serialize(serverMessage, _room.GetGameMap().GotPublicPool);
                _room.SendMessage(serverMessage);
                roomUser.BotAI.OnSelfEnterRoom();
                if (roomUser.IsPet)
                {
                    if (_pets.Contains(roomUser.PetData.PetId))
                        _pets[roomUser.PetData.PetId] = roomUser;
                    else
                        _pets.Add(roomUser.PetData.PetId, roomUser);
                    PetCount++;
                }

                roomUser.BotAI.Modified();
                if (roomUser.BotData.AiType == AIType.MafiaWarBot || roomUser.BotData.AiType == AIType.Generic || roomUser.BotData.AiType == AIType.MiscBot || roomUser.BotData.AiType == AIType.HospitalBot || roomUser.BotData.AiType == AIType.WeedBot)
                {
                    if (_bots.Contains(roomUser.BotData.BotId))
                        _bots[roomUser.BotData.BotId] = roomUser;
                    else
                    {
                        _bots.Add(roomUser.BotData.BotId, roomUser);
                    }


                    serverMessage.Init(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
                    serverMessage.AppendInteger(roomUser.VirtualId);
                    serverMessage.AppendInteger(roomUser.BotData.DanceId);
                    _room.SendMessage(serverMessage);
                    PetCount++;
                }

                return roomUser;
            }
        }

        /// <summary>
        /// Updates the bot.
        /// </summary>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="roomUser">The room user.</param>
        /// <param name="name">The name.</param>
        /// <param name="motto">The motto.</param>
        /// <param name="look">The look.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="speech">The speech.</param>
        /// <param name="responses">The responses.</param>
        /// <param name="speak">if set to <c>true</c> [speak].</param>
        /// <param name="speechDelay">The speech delay.</param>
        /// <param name="mix">if set to <c>true</c> [mix].</param>
        internal void UpdateBot(int virtualId, RoomUser roomUser, string name, string motto, string look, string gender,
            List<string> speech, List<string> responses, bool speak, int speechDelay, bool mix)
        {
            var bot = GetRoomUserByVirtualId(virtualId);
            if (bot == null || !bot.IsBot) return;

            var rBot = bot.BotData;

            rBot.Name = name;
            rBot.Motto = motto;
            rBot.Look = look;
            rBot.Gender = gender;
            rBot.RandomSpeech = speech;
            rBot.Responses = responses;
            rBot.AutomaticChat = speak;
            rBot.SpeechInterval = speechDelay;
            rBot.RoomUser = roomUser;
            rBot.MixPhrases = mix;

            if (rBot.RoomUser == null || rBot.RoomUser.BotAI == null) return;

            rBot.RoomUser.BotAI.Modified();
        }

        /// <summary>
        /// Removes the bot.
        /// </summary>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="kicked">if set to <c>true</c> [kicked].</param>
        internal void RemoveBot(int virtualId, bool kicked)
        {
            var roomUserByVirtualId = GetRoomUserByVirtualId(virtualId);
            if (roomUserByVirtualId == null || !roomUserByVirtualId.IsBot) return;

            if (roomUserByVirtualId.IsPet)
            {
                _pets.Remove(roomUserByVirtualId.PetData.PetId);
                PetCount--;
            }
            roomUserByVirtualId.BotAI.OnSelfLeaveRoom(kicked);
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserLeftRoomMessageComposer"));
            serverMessage.AppendString(roomUserByVirtualId.VirtualId.ToString());
            _room.SendMessage(serverMessage);

            RoomUser roomUser;
            UserList.TryRemove(roomUserByVirtualId.InternalRoomId, out roomUser);
        }

        /// <summary>
        /// Gets the user for square.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetUserForSquare(int x, int y)
        {
            return _room.GetGameMap().GetRoomUsers(new Point(x, y)).FirstOrDefault();
        }

        /// <summary>
        /// Adds the user to room.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="spectator">if set to <c>true</c> [spectator].</param>
        /// <param name="snow">if set to <c>true</c> [snow].</param>
        internal void AddUserToRoom(GameClient session, bool spectator, bool snow = false)
        {
            if (session == null || session.GetHabbo() == null)
                return;
            var roomUser = new RoomUser(session.GetHabbo().Id, _room.RoomId, _primaryPrivateUserId++, _room, spectator);
            if (roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null)
                return;



            roomUser.UserId = session.GetHabbo().Id;
            var userName = session.GetHabbo().UserName;
            var userId = roomUser.UserId;
            if (UsersByUserName.Contains(userName.ToLower()))
                UsersByUserName.Remove(userName.ToLower());
            if (UsersByUserId.Contains(userId))
                UsersByUserId.Remove(userId);
            UsersByUserName.Add(session.GetHabbo().UserName.ToLower(), roomUser);
            UsersByUserId.Add(session.GetHabbo().Id, roomUser);
            var num = _secondaryPrivateUserId++;
            roomUser.InternalRoomId = num;
            session.CurrentRoomUserId = num;
            session.GetHabbo().CurrentRoomId = _room.RoomId;
            UserList.TryAdd(num, roomUser);
            OnUserAdd(roomUser);

            session.GetHabbo().LoadingRoom = 0;

            if (Plus.GetGame().GetNavigator().PrivateCategories.Contains(_room.RoomData.Category))
                ((FlatCat)Plus.GetGame().GetNavigator().PrivateCategories[_room.RoomData.Category]).UsersNow++;
        }

        /// <summary>
        /// Updates the user.
        /// </summary>
        /// <param name="oldName">The old name.</param>
        /// <param name="newName">The new name.</param>
        internal void UpdateUser(string oldName, string newName)
        {
            if (oldName == newName)
                return;

            if (!UsersByUserName.Contains(oldName))
                return;
            UsersByUserName.Add(newName, UsersByUserName[oldName]);
            UsersByUserName.Remove(oldName);
            //
            Plus.GetGame().GetClientManager().UpdateClient(oldName, newName);
        }

        /// <summary>
        /// Removes the user from room.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="notifyClient">if set to <c>true</c> [notify client].</param>
        /// <param name="notifyKick">if set to <c>true</c> [notify kick].</param>
        internal void RemoveUserFromRoom(GameClient session, bool notifyClient, bool notifyKick)
        {
            try
            {
                if (session == null || session.GetHabbo() == null || _room == null)
                    return;
                var userId = session.GetHabbo().Id;



                session.GetHabbo().GetAvatarEffectsInventoryComponent().OnRoomExit();

                var roomUserByHabbo = GetRoomUserByHabbo(userId);
                if (roomUserByHabbo == null)
                    return;
                if (notifyKick)
                {
                    var room = Plus.GetGame().GetRoomManager().GetRoom(roomUserByHabbo.RoomId);
                    var model = room.GetGameMap().Model;
                    roomUserByHabbo.MoveTo(model.DoorX, model.DoorY);
                    roomUserByHabbo.CanWalk = false;
                    session.GetMessageHandler()
                        .GetResponse()
                        .Init(LibraryParser.OutgoingRequest("RoomErrorMessageComposer"));
                    session.GetMessageHandler().GetResponse().AppendInteger(4008);
                    session.GetMessageHandler().SendResponse();

                    session.GetMessageHandler()
                        .GetResponse()
                        .Init(LibraryParser.OutgoingRequest("OutOfRoomMessageComposer"));
                    session.GetMessageHandler().GetResponse().AppendShort(2);
                    session.GetMessageHandler().SendResponse();
                }
                else if (notifyClient)
                {

                }

                if (roomUserByHabbo.RidingHorse)
                {
                    roomUserByHabbo.RidingHorse = false;
                    var horse = GetRoomUserByVirtualId((int)roomUserByHabbo.HorseId);
                    if (horse != null)
                    {
                        horse.RidingHorse = false;
                        horse.HorseId = 0u;
                    }
                }
                if (roomUserByHabbo.IsLyingDown || roomUserByHabbo.IsSitting)
                {
                    roomUserByHabbo.IsSitting = false;
                    roomUserByHabbo.IsLyingDown = false;
                }
                RemoveRoomUser(roomUserByHabbo);
                if (session.GetHabbo() != null && !roomUserByHabbo.IsSpectator)
                {
                    if (roomUserByHabbo.CurrentItemEffect != ItemEffectType.None)
                        roomUserByHabbo.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect = -1;
                    if (session.GetHabbo() != null)
                    {
                        if (_room.HasActiveTrade(session.GetHabbo().Id))
                            _room.TryStopTrade(session.GetHabbo().Id);
                        session.GetHabbo().CurrentRoomId = 0;
                        if (session.GetHabbo().GetMessenger() != null)
                            session.GetHabbo().GetMessenger().OnStatusChanged(true);
                    }

                }
                UsersByUserId.Remove(roomUserByHabbo.UserId);
                if (session.GetHabbo() != null)
                    UsersByUserName.Remove(session.GetHabbo().UserName.ToLower());
                roomUserByHabbo.Dispose();
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(string.Format("Error during removing user from room:{0}", ex));
            }
        }

        /// <summary>
        /// Removes the room user.
        /// </summary>
        /// <param name="user">The user.</param>
        internal void RemoveRoomUser(RoomUser user)
        {
            RoomUser junk;
            if (!UserList.TryRemove(user.InternalRoomId, out junk)) return;

            user.InternalRoomId = -1;
            _room.GetGameMap().GameMap[user.X, user.Y] = user.SqState;
            _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserLeftRoomMessageComposer"));
            serverMessage.AppendString(user.VirtualId.ToString());
            _room.SendMessage(serverMessage);

            OnRemove(junk);
        }

        /// <summary>
        /// Gets the pet.
        /// </summary>
        /// <param name="petId">The pet identifier.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetPet(uint petId)
        {
            if (_pets.Contains(petId))
                return (RoomUser)_pets[petId];
            return null;
        }

        /// <summary>
        /// Gets the bot.
        /// </summary>
        /// <param name="botId">The bot identifier.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetBot(uint BotId)
        {
            if (_bots.Contains(BotId))
            {
                return (RoomUser)_bots[BotId];
            }
            Console.WriteLine("Couldn't get BOT: " + BotId, ConsoleColor.Gray);
            return null;
        }

        internal RoomUser GetBotByName(string name)
        {
            var roomUser = UserList.Values.FirstOrDefault(b => b.BotData != null && b.BotData.Name == name);
            return roomUser;
        }

        /// <summary>
        /// Updates the user count.
        /// </summary>
        /// <param name="count">The count.</param>
        internal void UpdateUserCount(uint count)
        {
            _userCount = count;
            _room.RoomData.UsersNow = count;
            using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery("UPDATE rooms_data SET users_now = " + count + " WHERE id = " + _room.RoomId + " LIMIT 1");
            Plus.GetGame().GetRoomManager().QueueActiveRoomUpdate(_room.RoomData);
        }

        /// <summary>
        /// Gets the room user by virtual identifier.
        /// </summary>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetRoomUserByVirtualId(int virtualId)
        {
            return UserList.ContainsKey(virtualId) ? UserList[virtualId] : null;
        }

        /// <summary>
        /// Gets the users in camping tent.
        /// </summary>
        /// <returns>List&lt;RoomUser&gt;.</returns>
        internal List<RoomUser> GetUsersInCampingTent()
        {
            return GetRoomUsers().Where(x => x.OnCampingTent).ToList();
        }

        /// <summary>
        /// Gets the room users.
        /// </summary>
        /// <returns>HashSet&lt;RoomUser&gt;.</returns>
        internal HashSet<RoomUser> GetRoomUsers()
        {
            return new HashSet<RoomUser>(UserList.Values.Where(x => x.IsBot == false));
        }

        /// <summary>
        /// Gets the room user by rank.
        /// </summary>
        /// <param name="minRank">The minimum rank.</param>
        /// <returns>List&lt;RoomUser&gt;.</returns>
        internal List<RoomUser> GetRoomUserByRank(int minRank)
        {
            return
                UserList.Values.Where(
                    current =>
                        !current.IsBot && current.GetClient() != null && current.GetClient().GetHabbo() != null &&
                        current.GetClient().GetHabbo().Rank > (ulong)minRank).ToList();
        }

        /// <summary>
        /// Gets the room user by habbo.
        /// </summary>
        /// <param name="pName">Name of the p.</param>
        /// <returns>RoomUser.</returns>
        internal RoomUser GetRoomUserByHabbo(string pName)
        {
            lock (UsersByUserName)
            {
                if (UsersByUserName.Contains(pName.ToLower()))
                    return (RoomUser)UsersByUserName[pName.ToLower()];
            }

            return null;
        }

        /// <summary>
        /// Saves the pets.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void SavePets(IQueryAdapter dbClient)
        {
            try
            {
                if (GetPets().Any())
                    AppendPetsUpdateString(dbClient);
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(string.Concat(new object[]
                {
                    "Error during saving furniture for room ",
                    _room.RoomId,
                    ". Stack: ",
                    ex.ToString()
                }));
            }
        }

        /// <summary>
        /// Appends the pets update string.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void AppendPetsUpdateString(IQueryAdapter dbClient)
        {
            var queryChunk = new QueryChunk("INSERT INTO bots (id,user_id,room_id,name,x,y,z) VALUES ");
            var queryChunk2 =
                new QueryChunk(
                    "INSERT INTO pets_data (type,race,color,experience,energy,createstamp,nutrition,respect) VALUES ");
            var queryChunk3 = new QueryChunk();
            var list = new List<uint>();
            foreach (var current in GetPets().Where(current => !list.Contains(current.PetId)))
            {
                list.Add(current.PetId);
                switch (current.DbState)
                {
                    case DatabaseUpdateState.NeedsInsert:
                        queryChunk.AddParameter(string.Format("{0}name", current.PetId), current.Name);
                        queryChunk2.AddParameter(string.Format("{0}race", current.PetId), current.Race);
                        queryChunk2.AddParameter(string.Format("{0}color", current.PetId), current.Color);
                        queryChunk.AddQuery(string.Concat(new object[]
                        {
                            "(",
                            current.PetId,
                            ",",
                            current.OwnerId,
                            ",",
                            current.RoomId,
                            ",@",
                            current.PetId,
                            "name,", current.X, ",", current.Y, ",", current.Z, ")"
                        }));
                        queryChunk2.AddQuery(string.Concat(new object[]
                        {
                            "(",
                            current.Type,
                            ",@",
                            current.PetId,
                            "race,@",
                            current.PetId,
                            "color,0,100,'",
                            current.CreationStamp,
                            "',0,0)"
                        }));
                        break;

                    case DatabaseUpdateState.NeedsUpdate:
                        queryChunk3.AddParameter(string.Format("{0}name", current.PetId), current.Name);
                        queryChunk3.AddParameter(string.Format("{0}race", current.PetId), current.Race);
                        queryChunk3.AddParameter(string.Format("{0}color", current.PetId), current.Color);
                        queryChunk3.AddQuery(string.Concat(new object[]
                        {
                            "UPDATE bots SET room_id = ",
                            current.RoomId,
                            ", name = @",
                            current.PetId,
                            "name, x = ",
                            current.X,
                            ", Y = ",
                            current.Y,
                            ", Z = ",
                            current.Z,
                            " WHERE id = ",
                            current.PetId
                        }));
                        queryChunk3.AddQuery(string.Concat(new object[]
                        {
                            "UPDATE pets_data SET race = @",
                            current.PetId,
                            "race, color = @",
                            current.PetId,
                            "color, type = ",
                            current.Type,
                            ", experience = ",
                            current.Experience,
                            ", energy = ",
                            current.Energy,
                            ", nutrition = ",
                            current.Nutrition,
                            ", respect = ",
                            current.Respect,
                            ", createstamp = '",
                            current.CreationStamp,
                            "' WHERE id = ",
                            current.PetId
                        }));
                        break;
                }
                current.DbState = DatabaseUpdateState.Updated;
            }
            queryChunk.Execute(dbClient);
            queryChunk3.Execute(dbClient);
            queryChunk.Dispose();
            queryChunk3.Dispose();
            queryChunk = null;
            queryChunk3 = null;
        }

        /// <summary>
        /// Gets the pets.
        /// </summary>
        /// <returns>List&lt;Pet&gt;.</returns>
        internal List<Pet> GetPets()
        {
            var list = UserList.ToList();
            return
                (from current in list select current.Value into value where value.IsPet select value.PetData).ToList();
        }

        /// <summary>
        /// Serializes the status updates.
        /// </summary>
        /// <param name="all">if set to <c>true</c> [all].</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeStatusUpdates(bool all)
        {
            var list = new List<RoomUser>();
            foreach (var current in UserList.Values.ToList())
            {
                if (!all)
                {
                    if (!current.UpdateNeeded)
                        continue;
                    current.UpdateNeeded = false;
                }
                list.Add(current);
            }
            if (!list.Any())
                return null;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserStatusMessageComposer"));
            serverMessage.AppendInteger(list.Count);

            lock (list)
            {
                foreach (var current2 in list)
                {
                    if (current2 == null) return null;
                    current2.SerializeStatus(serverMessage);
                }
            }

            return serverMessage;
        }

        /// <summary>
        /// Backups the counters.
        /// </summary>
        /// <param name="primaryCounter">The primary counter.</param>
        /// <param name="secondaryCounter">The secondary counter.</param>
        internal void BackupCounters(ref int primaryCounter, ref int secondaryCounter)
        {
            primaryCounter = _primaryPrivateUserId;
            secondaryCounter = _secondaryPrivateUserId;
        }

        /// <summary>
        /// Updates the user status.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cycleGameItems">if set to <c>true</c> [cyclegameitems].</param>
        internal void UpdateUserStatus(RoomUser user, bool cycleGameItems)
        {
            if (user == null) return;

            bool isBot = user.IsBot;
            if (1 == 1) cycleGameItems = false;

            try
            {
                var coordItemSearch = new CoordItemSearch(_room.GetGameMap().CoordinatedItems);
                var allRoomItemForSquare = coordItemSearch.GetAllRoomItemForSquare(user.X, user.Y);
                var itemsOnSquare = _room.GetGameMap().GetCoordinatedItems(new Point(user.X, user.Y));

                double newZ;
                if (user.RidingHorse == true && user.IsPet == false)
                {
                    newZ = _room.GetGameMap().SqAbsoluteHeight(user.X, user.Y, itemsOnSquare) + 1;
                }
                else
                {
                    newZ = _room.GetGameMap().SqAbsoluteHeight(user.X, user.Y, itemsOnSquare);
                }

                if (Math.Abs(newZ - user.Z) > 0)
                {
                    user.Z = newZ;
                    user.UpdateNeeded = true;
                }

                if (!allRoomItemForSquare.Any()) user.LastItem = 0;
                using (var enumerator = allRoomItemForSquare.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var item = enumerator.Current;
                        if (cycleGameItems)
                        {
                            item.UserWalksOnFurni(user);
                            Plus.GetGame()
                                .GetQuestManager()
                                .ProgressUserQuest(user.GetClient(), QuestType.StandOn, item.GetBaseItem().ItemId);
                        }

                        if (item.GetBaseItem().IsSeat)
                        {
                            if (!user.Statusses.ContainsKey("sit"))
                                if (item.GetBaseItem().StackMultipler && !string.IsNullOrWhiteSpace(item.ExtraData))
                                    if (item.ExtraData != "0")
                                    {
                                        var num2 = Convert.ToInt32(item.ExtraData);
                                        user.Statusses.Add("sit",
                                            item.GetBaseItem().ToggleHeight[num2].ToString(CultureInfo.InvariantCulture)
                                                .Replace(',', '.'));
                                    }
                                    else user.Statusses.Add("sit", TextHandling.GetString(item.GetBaseItem().Height));
                                else user.Statusses.Add("sit", TextHandling.GetString(item.GetBaseItem().Height));

                            if (Math.Abs(user.Z - item.Z) > 0 || user.RotBody != item.Rot)
                            {
                                user.Z = item.Z;
                                user.RotHead = item.Rot;
                                user.RotBody = item.Rot;
                                user.UpdateNeeded = true;
                            }
                        }

                        DynamicRoomModel model = _room.GetGameMap().Model;

                        #region Showers

                        if (item.GetBaseItem().Name.ToLower().Contains("shower") || (item.GetBaseItem().PublicName.ToLower().Contains("bath") && !item.GetBaseItem().PublicName.ToLower().Contains("bathroom")))
                        {
                            if (user != null)
                            {
                                if (user.GetClient().GetRoleplay().Hygiene >= 100)
                                {
                                    user.MoveTo(user.X, user.Y - 1);
                                    user.GetClient().SendWhisper("You are already fully clean!");
                                    return;

                                }
                                if (!user.GetClient().GetRoleplay().InShower)
                                {
                                    user.GetClient().Shout("*Starts to bathe*");
                                }
                                user.GetClient().GetRoleplay().InShower = true;
                                user.GetClient().GetRoleplay().Shower = item;
                            }
                        }

                        #endregion

                        var interactionType = item.GetBaseItem().InteractionType;

                        switch (interactionType)
                        {
                            case Interaction.QuickTeleport:
                            case Interaction.GuildGate:
                            case Interaction.minefield:
                                {
                                    item.Interactor.OnUserWalk(user.GetClient(), item, user);
                                    break;
                                }

                            case Interaction.None:
                                break;

                            case Interaction.Bed:
                                {
                                    if (!user.Statusses.ContainsKey("lay"))
                                        user.Statusses.Add("lay", TextHandling.GetString(item.GetBaseItem().Height));
                                    else if (user.Statusses["lay"] != TextHandling.GetString(item.GetBaseItem().Height)) user.Statusses["lay"] = TextHandling.GetString(item.GetBaseItem().Height);

                                    user.Z = item.Z;
                                    user.RotBody = item.Rot;
                                    break;
                                }

                            case Interaction.Guillotine:
                                {
                                    if (!user.Statusses.ContainsKey("lay")) user.Statusses.Add("lay", TextHandling.GetString(item.GetBaseItem().Height));
                                    else if (user.Statusses["lay"] != TextHandling.GetString(item.GetBaseItem().Height)) user.Statusses["lay"] = TextHandling.GetString(item.GetBaseItem().Height);

                                    user.Z = item.Z;
                                    user.RotBody = item.Rot;

                                    var avatarEffectsInventoryComponent =
                                        user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent();

                                    avatarEffectsInventoryComponent.ActivateCustomEffect(133);
                                    break;
                                }

                            case Interaction.FootballGate:
                                break;

                            case Interaction.BanzaiGateBlue:
                                {

                                    if (_room.RoomData.Description.Contains("MAFIAWARLOBBY"))
                                    {
                                        if (Plus.GetGame().MafiaWars.Started)
                                        {
                                            user.GetClient().SendWhisper("[MAFIA WARS] Match has already started!");
                                            user.GetClient().GetHabbo().GetRoomUser().MoveTo(item.SquareInFront);
                                            break;
                                        }

                                        if (Plus.GetGame().MafiaWars.MWTeams["Blue"].Players.Count >= 10)
                                        {
                                            user.GetClient().SendWhisper("[MAFIA WARS] This team is full!");
                                            user.GetClient().GetHabbo().GetRoomUser().MoveTo(item.SquareInFront);
                                            break;
                                        }

                                        Plus.GetGame().MafiaWars.AddUserToGame(user.GetClient(), "Blue");
                                        break;
                                    }

                                    if (ColourManager.Teams["blue"].Players.Count >= 6)
                                    {
                                        user.GetClient().SendWhisper("This team is full!");
                                        user.GetClient().GetHabbo().GetRoomUser().MoveTo(item.SquareInFront);
                                        break;
                                    }

                                    // If the user is not in colour wars
                                    if (!user.GetClient().GetRoleplay().inColourWars)
                                    {
                                        ColourManager.AddPlayerToTeam(user.GetClient(), "blue", item);
                                    }
                                    else
                                    {
                                        ColourManager.RemovePlayerFromTeam(user.GetClient(), user.GetClient().GetRoleplay().ColourWarTeam, false);

                                        // Set the users position
                                        user.SetPos(15, 15, 0);
                                        // Clear movement
                                        user.ClearMovement();
                                    }
                                    break;
                                }

                            case Interaction.BanzaiGateRed:
                                {

                                    if (ColourManager.Teams["pink"].Players.Count >= 6)
                                    {
                                        user.GetClient().SendWhisper("This team is full!");
                                        user.GetClient().GetHabbo().GetRoomUser().MoveTo(item.SquareInFront);
                                        break;
                                    }

                                    // If the user is not in colour wars
                                    if (!user.GetClient().GetRoleplay().inColourWars)
                                    {
                                        ColourManager.AddPlayerToTeam(user.GetClient(), "pink", item);
                                    }
                                    else
                                    {
                                        ColourManager.RemovePlayerFromTeam(user.GetClient(), user.GetClient().GetRoleplay().ColourWarTeam, false);

                                        // Set the users position
                                        user.SetPos(15, 15, 0);
                                        // Clear movement
                                        user.ClearMovement();
                                    }
                                    break;
                                }

                            case Interaction.BanzaiGateYellow:
                                {

                                    if (ColourManager.Teams["yellow"].Players.Count >= 6)
                                    {
                                        user.GetClient().SendWhisper("This team is full!");
                                        user.GetClient().GetHabbo().GetRoomUser().MoveTo(item.SquareInFront);
                                        break;
                                    }

                                    // If the user is not in colour wars
                                    if (!user.GetClient().GetRoleplay().inColourWars)
                                    {
                                        ColourManager.AddPlayerToTeam(user.GetClient(), "yellow", item);
                                    }
                                    else
                                    {
                                        ColourManager.RemovePlayerFromTeam(user.GetClient(), user.GetClient().GetRoleplay().ColourWarTeam, false);

                                        // Set the users position
                                        user.SetPos(15, 15, 0);
                                        // Clear movement
                                        user.ClearMovement();
                                    }
                                    break;
                                }
                            case Interaction.BanzaiGateGreen:
                                {

                                    if (_room.RoomData.Description.Contains("MAFIAWARLOBBY"))
                                    {
                                        if (Plus.GetGame().MafiaWars.Started)
                                        {
                                            user.GetClient().SendWhisper("[MAFIA WARS] Match has already started!");
                                            user.GetClient().GetHabbo().GetRoomUser().MoveTo(item.SquareInFront);
                                            break;
                                        }

                                        if (Plus.GetGame().MafiaWars.MWTeams["Green"].Players.Count >= 10)
                                        {
                                            user.GetClient().SendWhisper("[MAFIA WARS] This team is full!");
                                            user.GetClient().GetHabbo().GetRoomUser().MoveTo(item.SquareInFront);
                                            break;
                                        }

                                        Plus.GetGame().MafiaWars.AddUserToGame(user.GetClient(), "Green");
                                        break;
                                    }

                                    if (ColourManager.Teams["green"].Players.Count >= 6)
                                    {
                                        user.GetClient().SendWhisper("This team is full!");
                                        user.GetClient().GetHabbo().GetRoomUser().MoveTo(item.SquareInFront);
                                        break;
                                    }

                                    // If the user is not in colour wars
                                    if (!user.GetClient().GetRoleplay().inColourWars)
                                    {
                                        ColourManager.AddPlayerToTeam(user.GetClient(), "green", item);
                                    }
                                    else
                                    {
                                        ColourManager.RemovePlayerFromTeam(user.GetClient(), user.GetClient().GetRoleplay().ColourWarTeam, false);

                                        // Set the users position
                                        user.SetPos(15, 15, 0);
                                        // Clear movement
                                        user.ClearMovement();
                                    }
                                    break;
                                }
                            case Interaction.Jump:
                                break;

                            case Interaction.Pinata:
                                {
                                    if (!user.IsWalking || item.ExtraData.Length <= 0) break;
                                    var num5 = int.Parse(item.ExtraData);
                                    if (num5 >= 100 || user.CurrentEffect != 158) break;
                                    var num6 = num5 + 1;

                                    if (num6 == 1)
                                    {
                                        user.GetClient().Shout("*Takes out my Piñata Stick, and starts whacking the Piñata*");
                                    }

                                    item.ExtraData = num6.ToString();
                                    item.UpdateState();
                                    Plus.GetGame()
                                        .GetAchievementManager()
                                        .ProgressUserAchievement(user.GetClient(), "ACH_PinataWhacker", 1, false);
                                    if (num6 == 100)
                                    {
                                        Plus.GetGame().GetPinataHandler().DeliverRandomPinataItem(user, _room, item);
                                        Plus.GetGame()
                                            .GetAchievementManager()
                                            .ProgressUserAchievement(user.GetClient(), "ACH_PinataBreaker", 1, false);
                                    }
                                    break;
                                }
                            case Interaction.TileStackMagic:
                            case Interaction.Poster:
                                break;

                            case Interaction.Tent:
                            case Interaction.BedTent:
                                if (user.LastItem == item.Id) break;
                                if (!user.IsBot && !user.OnCampingTent)
                                {
                                    var serverMessage22 = new ServerMessage();
                                    serverMessage22.Init(
                                        LibraryParser.OutgoingRequest("UpdateFloorItemExtraDataMessageComposer"));
                                    serverMessage22.AppendString(item.Id.ToString());
                                    serverMessage22.AppendInteger(0);
                                    serverMessage22.AppendString("1");
                                    user.GetClient().SendMessage(serverMessage22);
                                    user.OnCampingTent = true;
                                    user.LastItem = item.Id;
                                }
                                break;

                            case Interaction.RunWaySage:
                                {
                                    var num7 = new Random().Next(1, 4);
                                    item.ExtraData = num7.ToString();
                                    item.UpdateState();
                                    break;
                                }
                            case Interaction.Shower:
                            case Interaction.ChairState:
                                {
                                    item.ExtraData = "1";
                                    item.UpdateState();
                                    break;
                                }
                            case Interaction.BanzaiTele:
                                {
                                    if (user.IsWalking)
                                        _room.GetGameItemHandler().OnTeleportRoomUserEnter(user, item);
                                    break;
                                }


                            case Interaction.Teleport:
                                {
                                    if (item.GetBaseItem().Name == "arow" && user.GoalX == item.X && user.GoalY == item.Y && !user.IsAsleep && !user.Stunned)
                                    {
                                        if (((!user.IsBot && (user != null)) && ((user.GetClient() != null) && (user.GetClient().GetHabbo() != null))) && (user.GetClient().GetMessageHandler() != null))
                                        {
                                            user.ClearMovement();
                                            uint linkedTele = TeleHandler.GetLinkedTele(item.Id, user.GetClient().GetHabbo().CurrentRoom);
                                            uint teleRoomId = TeleHandler.GetTeleRoomId(linkedTele, user.GetClient().GetHabbo().CurrentRoom);
                                            if (linkedTele != 0 && teleRoomId != 0)
                                            {
                                                if (teleRoomId != _room.RoomId)
                                                {
                                                    if (linkedTele != 0 && teleRoomId != 0)
                                                    {
                                                        if (((!user.IsBot && (user != null)) && ((user.GetClient() != null) && (user.GetClient().GetHabbo() != null))) && (user.GetClient().GetMessageHandler() != null))
                                                        {
                                                            user.GetClient().GetHabbo().IsTeleporting = true;
                                                            user.GetClient().GetHabbo().TeleportingRoomId = teleRoomId;
                                                            user.GetClient().GetHabbo().TeleporterId = linkedTele;
                                                            user.GetClient().GetMessageHandler().PrepareRoomForUser(teleRoomId, "");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    RoomItem item2 = _room.GetRoomItemHandler().GetItem(linkedTele);
                                                    if (item2 != null && linkedTele != 0)
                                                    {
                                                        item.InteractingUser = user.HabboId;
                                                        item2.InteractingUser2 = user.HabboId;
                                                        _room.GetGameMap().TeleportToItem(user, item2);
                                                    }
                                                    else
                                                    {
                                                        user.UnlockWalking();
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //user.GetClient().SendWhisper("This arrow is not linked!: " + linkedTele + ";;" + teleRoomId);
                                        }
                                    }
                                    continue;
                                }
                        }

                        if (item.GetBaseItem().InteractionType == Interaction.BedTent) user.OnCampingTent = true;

                        user.LastItem = item.Id;
                    }
                }

                if (user.IsSitting && user.TeleportEnabled)
                {
                    user.Z -= 0.35;
                    user.UpdateNeeded = true;
                }

                if (!cycleGameItems) return;
                if (_room.GotSoccer()) _room.GetSoccer().OnUserWalk(user);
                if (_room.GotBanzai()) _room.GetBanzai().OnUserWalk(user);
                _room.GetFreeze().OnUserWalk(user);
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "RoomUserManager.cs:UpdateUserStatus");
            }
        }

        /// <summary>
        /// Turns the heads.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="senderId">The sender identifier.</param>
        internal void TurnHeads(int x, int y, uint senderId)
        {
            foreach (
                var current in
                    UserList.Values.Where(
                        current => current.HabboId != senderId && !current.RidingHorse && !current.IsPet))
                current.SetRot(PathFinder.CalculateRotation(current.X, current.Y, x, y), true);
        }

        /// <summary>
        /// Called when [cycle].
        /// </summary>
        /// <param name="idleCount">The idle count.</param>
        internal void OnCycle(ref int idleCount)
        {
            uint count = 0;

            lock (_toRemove)
                _toRemove.Clear();

            try
            {
                // Update room items!!
                if (_room != null && _room.DiscoMode && _room.TonerData != null && _room.TonerData.Enabled == 1)
                {
                    var item = _room.GetRoomItemHandler().GetItem(_room.TonerData.ItemId);
                    if (item != null)
                    {
                        _room.TonerData.Data1 = Plus.GetRandomNumber(0, 255);
                        _room.TonerData.Data2 = Plus.GetRandomNumber(0, 255);
                        _room.TonerData.Data3 = Plus.GetRandomNumber(0, 255);

                        var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer"));
                        item.Serialize(message);
                        _room.SendMessage(message);
                    }
                }
            }
            catch (Exception e)
            {
                Writer.Writer.LogException("Disco mode: " + e.StackTrace);
            }

            // If there are no users then don't even bother
            /*if (UserList.Values.Count <= 0) { }
            else
            {*/
            foreach (var roomUser in UserList.Values)
            {
                roomUser.IdleTime++;

                if (!IsValid(roomUser))
                {
                    if (roomUser.GetClient() != null) RemoveUserFromRoom(roomUser.GetClient(), false, false);
                    else RemoveRoomUser(roomUser);
                }

                if (!roomUser.IsAsleep && roomUser.IdleTime >= 600 && !roomUser.IsBot && !roomUser.IsPet && roomUser.GetClient() != null)
                {
                    roomUser.IsAsleep = true;
                    var sleepMsg = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserIdleMessageComposer"));
                    if (sleepMsg != null)
                    {
                        sleepMsg.AppendInteger(roomUser.VirtualId);
                        sleepMsg.AppendBool(true);
                        _room.SendMessage(sleepMsg);

                        roomUser.GetClient().Shout("*Falls Asleep [AFK]*");
                    }
                }

                // Lets change this
                if (roomUser.CarryItemId > 0)
                {
                    roomUser.CarryTimer--;
                    if (roomUser.CarryTimer <= 0) roomUser.CarryItem(0);
                }

                if (_room.GotFreeze()) Freeze.CycleUser(roomUser);


                #region Roleplay Timers


                #endregion

                bool invalidStep = false;

                if (roomUser.SetStep)
                {
                    if (_room.GetGameMap().CanWalk(roomUser.SetX, roomUser.SetY, roomUser.AllowOverride) || roomUser.RidingHorse)
                    //if (_room.GetGameMap().IsValidStep(roomUser, new Vector2D(roomUser.X, roomUser.Y), new Vector2D(roomUser.SetX, roomUser.SetY), (roomUser.GoalX == roomUser.SetX && roomUser.GoalY == roomUser.SetY), roomUser.AllowOverride))
                    {

                        //  _room.GetGameMap().UpdateUserMovement(new Point(roomUser.Coordinate.X, roomUser.Coordinate.Y), new Point(roomUser.SetX, roomUser.SetY), roomUser);
                        var items = _room.GetGameMap().GetCoordinatedItems(new Point(roomUser.X, roomUser.Y));

                        roomUser.X = roomUser.SetX;
                        roomUser.Y = roomUser.SetY;
                        roomUser.Z = roomUser.SetZ;

                        try
                        {
                            foreach (var itemE in items.ToArray())
                            {
                                itemE.UserWalksOffFurni(roomUser);

                                switch (itemE.GetBaseItem().InteractionType)
                                {
                                    case Interaction.Tent:
                                    case Interaction.BedTent:
                                        if (!roomUser.IsBot && roomUser.OnCampingTent)
                                        {
                                            var serverMessage = new ServerMessage();
                                            serverMessage.Init(
                                                LibraryParser.OutgoingRequest("UpdateFloorItemExtraDataMessageComposer"));
                                            serverMessage.AppendString(itemE.Id.ToString());
                                            serverMessage.AppendInteger(0);
                                            serverMessage.AppendString("0");
                                            roomUser.GetClient().SendMessage(serverMessage);
                                            roomUser.OnCampingTent = false;
                                        }
                                        break;

                                    case Interaction.RunWaySage:
                                    case Interaction.ChairState:
                                    case Interaction.Shower:
                                        {
                                            itemE.ExtraData = "0";
                                            itemE.UpdateState();
                                        }
                                        break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Writer.Writer.LogException("RoomUserManager OnCycle: " + Environment.NewLine + e.StackTrace);
                        }

                        lock (_toRemove)
                        {
                            if (roomUser.X == _room.GetGameMap().Model.DoorX && roomUser.Y == _room.GetGameMap().Model.DoorY &&
                                !_toRemove.Contains(roomUser) && !roomUser.IsBot)
                            {

                            }
                        }



                        UpdateUserStatus(roomUser, true);
                    }
                    else invalidStep = true;

                    roomUser.SetStep = false;
                }

                if (!roomUser.IsPet && !roomUser.IsBot) count++;

                if ((roomUser.GoalX == roomUser.X && roomUser.GoalY == roomUser.Y) || roomUser.Freezed)
                {
                    roomUser.IsWalking = false;
                    roomUser.ClearMovement();
                    roomUser.SetStep = false;
                    UpdateUserStatus(roomUser, false);
                }

                if (roomUser.PathRecalcNeeded)
                {
                    roomUser.Path.Clear();
                    roomUser.Path = PathFinder.FindPath(roomUser, _room.GetGameMap().DiagonalEnabled, _room.GetGameMap(), new Vector2D(roomUser.X, roomUser.Y), new Vector2D(roomUser.GoalX, roomUser.GoalY));

                    if (roomUser.Path.Count > 1)
                    {
                        roomUser.PathStep = 1;
                        roomUser.IsWalking = true;
                        roomUser.PathRecalcNeeded = false;
                    }
                    else
                    {
                        roomUser.PathRecalcNeeded = false;
                        roomUser.Path.Clear();

                        roomUser.IsWalking = false;
                        roomUser.ClearMovement();
                        roomUser.SetStep = false;
                        UpdateUserStatus(roomUser, false);
                    }
                }

                if (roomUser.IsWalking && !roomUser.Freezed)
                {
                    if ((!roomUser.HasPathBlocked) && (invalidStep || (roomUser.PathStep >= roomUser.Path.Count) || (roomUser.GoalX == roomUser.X && roomUser.GoalY == roomUser.Y)))
                    {
                        roomUser.HandelingBallStatus = 0;
                        UpdateUserStatus(roomUser, false);
                    }
                    else
                    {

                        {
                            var pathCountTotal = (roomUser.Path.Count - roomUser.PathStep) - 1;
                            var NextStep = roomUser.Path[(roomUser.Path.Count - roomUser.PathStep) - 1];
                            roomUser.PathStep++;

                            if (roomUser.FastWalking && roomUser.PathStep < roomUser.Path.Count)
                            {
                                int s2 = (roomUser.Path.Count - roomUser.PathStep) - 1;
                                NextStep = roomUser.Path[s2];
                                roomUser.PathStep++;
                            }

                            int nextX = NextStep.X;
                            int nextY = NextStep.Y;

                            roomUser.RemoveStatus("mv");

                            if (_room.GetGameMap()
                                .IsValidStep3(roomUser, new Vector2D(roomUser.X, roomUser.Y), new Vector2D(nextX, nextY),
                                    (roomUser.GoalX == nextX && roomUser.GoalY == nextY), roomUser.AllowOverride,
                                    roomUser.GetClient()))
                            {
                                string user = string.Empty, mascote = string.Empty;
                                var nextZ = _room.GetGameMap().SqAbsoluteHeight(nextX, nextY);
                                var newRot = Rotation.Calculate(roomUser.X, roomUser.Y, nextX, nextY, roomUser.IsMoonwalking);

                                roomUser.RotBody = newRot;
                                roomUser.RotHead = newRot;
                                roomUser.SetStep = true;

                                roomUser.SetX = nextX;
                                roomUser.SetY = nextY;
                                roomUser.SetZ = nextZ;
                                roomUser.AddStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));

                                if (roomUser.Statusses.ContainsKey("lay") || roomUser.IsLyingDown)
                                {
                                    roomUser.Statusses.Remove("lay");
                                    roomUser.IsLyingDown = false;
                                    roomUser.UpdateNeeded = true;
                                }

                                if (roomUser.Statusses.ContainsKey("sit") || roomUser.IsSitting)
                                {
                                    roomUser.Statusses.Remove("sit");
                                    roomUser.IsSitting = false;
                                    roomUser.UpdateNeeded = true;
                                }

                                UpdateUserEffect(roomUser, roomUser.SetX, roomUser.SetY);

                                _room.GetGameMap().GameMap[roomUser.X, roomUser.Y] = roomUser.SqState;

                                roomUser.SqState = _room.GetGameMap().GameMap[roomUser.SetX, roomUser.SetY];
                            }
                            else roomUser.PathRecalcNeeded = true;

                            if (!roomUser.RidingHorse) roomUser.UpdateNeeded = true;

                        }

                    }

                }
                else
                {
                    if (roomUser.Statusses.ContainsKey("mv"))
                        roomUser.ClearMovement();
                }

                if (roomUser.IsBot) roomUser.BotAI.OnTimerTick();
                UpdateUserEffect(roomUser, roomUser.X, roomUser.Y);
            }

            if (count == 0) idleCount++;

            lock (_toRemove)
            {
                foreach (var toRemove in _toRemove)
                {
                    var client = Plus.GetGame().GetClientManager().GetClientByUserId(toRemove.HabboId);
                    if (client != null) RemoveUserFromRoom(client, true, false);
                    else RemoveRoomUser(toRemove);
                }
                if (_userCount != count) UpdateUserCount(count);
            }
            //}
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            _room = null;
            UsersByUserName.Clear();
            UsersByUserName = null;
            UsersByUserId.Clear();
            UsersByUserId = null;
            OnUserEnter = null;
            _pets.Clear();
            _bots.Clear();
            _pets = null;
            _bots = null;
            UserList = null;
        }

        /// <summary>
        /// Updates the user effect.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        private void UpdateUserEffect(RoomUser user, int x, int y)
        {
            if (user.IsBot)
                return;
            try
            {
                var b = _room.GetGameMap().EffectMap[x, y];
                if (b > 0)
                {
                    if (user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect == 0)
                        user.CurrentItemEffect = ItemEffectType.None;
                    var itemEffectType = ByteToItemEffectEnum.Parse(b);
                    if (itemEffectType == user.CurrentItemEffect)
                        return;
                    switch (itemEffectType)
                    {
                        case ItemEffectType.None:
                            user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(-1);
                            user.CurrentItemEffect = itemEffectType;
                            break;

                        case ItemEffectType.Swim:
                            user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(28);
                            user.CurrentItemEffect = itemEffectType;
                            break;

                        case ItemEffectType.SwimLow:
                            user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(30);
                            user.CurrentItemEffect = itemEffectType;
                            break;

                        case ItemEffectType.SwimHalloween:
                            user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(37);
                            user.CurrentItemEffect = itemEffectType;
                            break;

                        case ItemEffectType.Iceskates:
                            user.GetClient()
                                .GetHabbo()
                                .GetAvatarEffectsInventoryComponent()
                                .ActivateCustomEffect(user.GetClient().GetHabbo().Gender.ToUpper() == "M" ? 38 : 39);
                            user.CurrentItemEffect = ItemEffectType.Iceskates;
                            break;

                        case ItemEffectType.Normalskates:
                            user.GetClient()
                                .GetHabbo()
                                .GetAvatarEffectsInventoryComponent()
                                .ActivateCustomEffect(user.GetClient().GetHabbo().Gender.ToUpper() == "M" ? 55 : 56);
                            user.CurrentItemEffect = itemEffectType;
                            break;

                        case ItemEffectType.SnowBoard:
                            {
                                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(97);
                                user.CurrentItemEffect = itemEffectType;
                            }
                            break;
                    }
                }
                else
                {
                    if (user.CurrentItemEffect == ItemEffectType.None || b != 0)
                        return;
                    user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(-1);
                    user.CurrentItemEffect = ItemEffectType.None;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Handles the <see cref="E:UserAdd" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <param name="user"></param>
        private void OnUserAdd(RoomUser user)
        {
            try
            {
                if (user != null)
                {
                    bool cond1 = false;
                    bool cond2 = false;
                    bool cond3 = false;
                    bool condsmet = false;

                    int LoadX = 0;
                    int LoadY = 0;
                    double LoadZ = 0;

                    if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null) return;
                    var client = user.GetClient();
                    if (client == null || client.GetHabbo() == null || _room == null) return;

                    if (!user.IsSpectator)
                    {
                        var model = _room.GetGameMap().Model;
                        if (model == null) return;


                        #region Roleplay


                        #region Generate Needed Spawn Items

                        bool changedload = false;
                        List<RoomItem> DoorMat = new List<RoomItem>();
                        List<RoomItem> JailBeds = new List<RoomItem>();
                        List<RoomItem> DeadBeds = new List<RoomItem>();

                        foreach (RoomItem Item in _room.ReturnRoleplaySpawn_Component("hospital_bed"))
                        {
                            DeadBeds.Add(Item);
                        }

                        foreach (RoomItem Item in _room.ReturnRoleplaySpawn_Component("jail_bed"))
                        {
                            JailBeds.Add(Item);
                        }

                        #endregion

                        cond1 = user != null ? true : false;
                        cond2 = user.GetClient() != null ? true : false;
                        cond3 = user.GetClient().GetRoleplay() != null ? true : false;
                        condsmet = cond1 == true && cond2 == true && cond3 == true ? true : false;

                        #region Death bed spawner
                        if (condsmet)
                        {
                            if (user.GetClient().GetRoleplay().Dead)
                            {
                                #region Spawn on Bed if Dead
                                RoomItem LandItem = null;

                                if (DeadBeds.Count >= 1)
                                {
                                    if (DeadBeds.Count == 1)
                                    {
                                        LandItem = DeadBeds.First<RoomItem>();
                                    }
                                    else
                                    {
                                        LandItem = DeadBeds[new Random().Next(0, DeadBeds.Count)];
                                    }
                                    LoadX = LandItem.X;
                                    LoadY = LandItem.Y;
                                    LoadZ = user.Z;

                                    #region Set Rotation & Sit Action
                                    if ((user.RotBody % 2) == 0)
                                    {

                                        try
                                        {
                                            user.Statusses.Add("lay", "1.0 null");
                                            user.Z = 0.75;
                                            user.IsLyingDown = true;
                                            user.UpdateNeeded = true;
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        user.RotBody--;
                                        user.Statusses.Add("lay", "1.0 null");
                                        user.Z = 0.75;
                                        user.IsLyingDown = true;
                                        user.UpdateNeeded = true;
                                    }

                                    LoadZ = user.Z;
                                    user.RotBody = LandItem.Rot;
                                    #endregion

                                    //   user.SetPos(LandItem.GetX, LandItem.GetY, LandItem.GetZ);
                                    user.UpdateNeeded = true;
                                }
                                #endregion

                                changedload = true;
                            }
                        }
                        #endregion

                        if (condsmet)
                        {

                            #region Jail bed spawner
                            if (user.GetClient().GetRoleplay().Jailed)
                            {
                                #region Spawn on Jail Bed if Jailed
                                RoomItem LandItem = null;


                                if (JailBeds.Count >= 1)
                                {
                                    if (JailBeds.Count == 1)
                                    {
                                        LandItem = JailBeds.First<RoomItem>();
                                    }
                                    else
                                    {
                                        LandItem = JailBeds[new Random().Next(0, JailBeds.Count)];
                                    }
                                    LoadX = LandItem.X;
                                    LoadY = LandItem.Y;
                                    LoadZ = LandItem.Z;

                                    #region Set Rotation & Sit Action
                                    if ((user.RotBody % 2) == 0)
                                    {

                                        try
                                        {
                                            user.Statusses.Add("lay", "1.0 null");
                                            user.Z = 0.75;
                                            user.IsLyingDown = true;
                                            user.UpdateNeeded = true;
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        user.RotBody--;
                                        user.Statusses.Add("lay", "1.0 null");
                                        user.Z = 0.75;
                                        user.IsLyingDown = true;
                                        user.UpdateNeeded = true;
                                    }

                                    LoadZ = user.Z;
                                    user.RotBody = LandItem.Rot;
                                    #endregion

                                    //   user.SetPos(LandItem.GetX, LandItem.GetY, LandItem.GetZ);
                                    user.UpdateNeeded = true;
                                }
                                #endregion
                                changedload = true;
                            }
                            #endregion

                            #region Last coords spawner
                            /*
                            if (!user.GetClient().GetRoleplay().Jailed && !user.GetClient().GetRoleplay().Dead && !user.GetClient().GetRoleplay().RecentlyCalledTaxi)
                            {
                                LoadX = user.GetClient().GetRoleplay().LastX;
                                LoadY = user.GetClient().GetRoleplay().LastY;
                                LoadZ = user.GetClient().GetRoleplay().LastZ;

                                changedload = true;
                            }*/

                            if (!user.GetClient().GetRoleplay().Dead && !user.GetClient().GetRoleplay().Jailed)
                            {
                                LoadX = _room.RoomData.Model.DoorX;
                                LoadY = _room.RoomData.Model.DoorY;
                                LoadZ = _room.RoomData.Model.DoorZ;
                                user.RotBody = _room.RoomData.Model.DoorOrientation;
                            }

                            #endregion

                            #region Taxi spawner
                            if (user.GetClient().GetRoleplay().RecentlyCalledTaxi)
                            {
                                #region Spawn on Mat on Taxi
                                RoomItem LandItem = null;

                                if (DoorMat.Count >= 1)
                                {
                                    if (DoorMat.Count == 1)
                                    {
                                        LandItem = DoorMat.First<RoomItem>();
                                    }
                                    else
                                    {
                                        LandItem = DoorMat[new Random().Next(0, DoorMat.Count)];
                                    }
                                    LoadX = LandItem.X;
                                    LoadY = LandItem.Y;
                                    LoadZ = LandItem.Z;
                                    //   user.SetPos(LandItem.GetX, LandItem.GetY, LandItem.GetZ);
                                    user.UpdateNeeded = true;
                                }
                                #endregion

                                user.GetClient().GetRoleplay().RecentlyCalledTaxi = false;
                                user.GetClient().GetRoleplay().RecentlyCalledTaxi_Timeout = 0;
                                user.GetClient().GetRoleplay().RequestedTaxi = false;
                                user.GetClient().GetRoleplay().RequestedTaxiDestination = null;
                                changedload = true;
                            }
                            else
                            {
                                user.SetPos(LoadX, LoadY, LoadZ);
                                user.SetRot(model.DoorOrientation, false);
                            }
                            #endregion

                            #region Color Wars spawner

                            if (user.GetClient().GetRoleplay().InMafiaWars && _room.RoomId == Plus.GetGame().MafiaWars.GraveYardID)
                            {
                                if (user.GetClient().GetRoleplay().TeamString == "Green")
                                {
                                    LoadX = 6;
                                    LoadY = 3;
                                    LoadZ = 0;
                                }
                                else
                                {
                                    LoadX = 9;
                                    LoadY = 12;
                                    LoadZ = 0;
                                }
                                changedload = true;
                            }

                            if (user.GetClient().GetRoleplay().inColourWars && _room.RoomId == ColourManager.GraveYardID)
                            {


                                foreach (RoomItem Item in _room.ReturnRoleplaySpawn_Component(user.GetClient().GetRoleplay().ColourWarTeam.SpawnMat))
                                {
                                    LoadX = Item.X;
                                    LoadY = Item.Y;
                                    LoadZ = Item.Z;
                                }

                                changedload = true;

                            }



                            #endregion

                            #region Sleeping Bag

                            if (user.GetClient().GetRoleplay().Bag != null)
                            {
                                RoleplayManager.PickRock(user.GetClient().GetRoleplay().Bag, user.GetClient().GetRoleplay().Bag.RoomId);
                                user.RemoveStatus("lay");
                                user.GetClient().GetRoleplay().Bag = null;
                                user.GetClient().Shout("*Puts back their sleeping bag*");

                                user.CanWalk = true;
                            }
                            #endregion

                            user.GetClient().GetRoleplay().setRoleplayComponents();


                        }


                        #endregion



                        user.SetPos(LoadX, LoadY, LoadZ);
                        user.SetRot(model.DoorOrientation, false);

                        user.AddStatus(_room.CheckRights(client, true) ? "flatctrl 4" : "flatctrl 1", string.Empty);

                        if (condsmet)
                        {
                            user.GetClient().GetRoleplay().SaveCurrentRoom();
                        }

                        user.CurrentItemEffect = ItemEffectType.None;
                        if (!user.IsBot && client.GetHabbo().IsTeleporting)
                        {
                            client.GetHabbo().IsTeleporting = false;
                            client.GetHabbo().TeleportingRoomId = 0;

                            var item = _room.GetRoomItemHandler().GetItem(client.GetHabbo().TeleporterId);
                            if (item != null)
                            {
                                item.ExtraData = "2";
                                item.UpdateState(false, true);
                                user.SetPos(item.X, item.Y, item.Z);
                                user.SetRot(item.Rot, false);
                                item.InteractingUser2 = client.GetHabbo().Id;
                                item.ExtraData = "0";
                                item.UpdateState(false, true);
                            }
                        }
                        if (!user.IsBot && client.GetHabbo().IsHopping)
                        {
                            client.GetHabbo().IsHopping = false;
                            client.GetHabbo().HopperId = 0;

                            var item2 = _room.GetRoomItemHandler().GetItem(client.GetHabbo().HopperId);
                            if (item2 != null)
                            {
                                item2.ExtraData = "1";
                                item2.UpdateState(false, true);
                                user.SetPos(item2.X, item2.Y, item2.Z);
                                user.SetRot(item2.Rot, false);
                                user.AllowOverride = false;
                                item2.InteractingUser2 = client.GetHabbo().Id;
                                item2.ExtraData = "2";
                                item2.UpdateState(false, true);
                            }
                        }
                        if (!user.IsSpectator)
                        {
                            var serverMessage =
                                new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
                            serverMessage.AppendInteger(1);
                            user.Serialize(serverMessage, _room.GetGameMap().GotPublicPool);
                            _room.SendMessage(serverMessage);
                        }
                        if (!user.IsBot)
                        {
                            var serverMessage2 = new ServerMessage();
                            serverMessage2.Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                            serverMessage2.AppendInteger(user.VirtualId);
                            serverMessage2.AppendString(client.GetHabbo().Look);
                            serverMessage2.AppendString(client.GetHabbo().Gender.ToLower());
                            serverMessage2.AppendString(client.GetHabbo().Motto);
                            serverMessage2.AppendInteger(client.GetHabbo().AchievementPoints);
                            _room.SendMessage(serverMessage2);
                        }
                        if (_room.RoomData != null)
                        {
                            if (_room.RoomData.Owner != client.GetHabbo().UserName)
                            {
                                Plus.GetGame()
                                    .GetQuestManager()
                                    .ProgressUserQuest(client, QuestType.SocialVisit, 0u);
                                Plus.GetGame()
                                    .GetAchievementManager()
                                    .ProgressUserAchievement(client, "ACH_RoomEntry", 1, false);
                            }
                        }
                    }
                    if (client.GetHabbo().GetMessenger() != null) client.GetHabbo().GetMessenger().OnStatusChanged(true);
                    user.GetClient().GetMessageHandler().OnRoomUserAdd();

                    if (OnUserEnter != null) OnUserEnter(user, null);
                    if (_room.GotMusicController() && _room.GotMusicController()) _room.GetRoomMusicController().OnNewUserEnter(user);
                    _room.OnUserEnter(user);

                    if (user.GetClient().GetRoleplay().KnockedOut)
                    {
                        user.ApplyEffect(13);// Set effect
                        user.Frozen = true;
                        user.CanWalk = false;
                    }

                    if (condsmet)
                    {


                        string PetName = user.GetClient().GetRoleplay().LastPetName;

                        if (PetName != null)
                        {

                            // Console.WriteLine("Preparing to load " + PetName);
                            FightPetManager Manager = new FightPetManager();
                            RoomBot Pet = Manager.DeployBotToRoom(user.GetClient(), PetName, _room.RoomId);
                        }
             

                        /*
                        if (user.GetClient().GetRoleplay().Pet_Summoned > 0)
                        {

                            user.GetClient().GetHabbo().CurrentRoomId = room.RoomId;
                            Pet Pet = user.GetClient().GetRoleplay().MyPet1;

                            if (Pet != null)
                            {

                                Pet.PlacedInRoom = true;
                                Pet.RoomId = room.RoomId;


                                List<RandomSpeech> RndSpeechList = new List<RandomSpeech>();
                                List<BotResponse> BotResponse = new List<Mercury.HabboHotel.RoomBots.BotResponse>();
                                RoomUser PetUser = room.GetRoomUserManager().DeployBot(new RoomBot(Pet.PetId, Convert.ToUInt32(Pet.OwnerId), Pet.RoomId, AIType.Pet, "freeroam", Pet.Name, "", Pet.Look, user.X, user.Y, 0, 0, 0, 0, 0, 0, ref RndSpeechList, ref BotResponse, "", 0, 0, 0, 0, 0, 0), Pet);
                                PetUser.SetPos(user.X, user.Y, user.Z);
                            }
                        }
                        */

                        if (user.GetClient().GetRoleplay().IsBoxing)
                        {
                            if (user.GetClient().GetRoleplay().BoxingRoom != null)
                            {
                                Room BoxingRoom = user.GetClient().GetRoleplay().BoxingRoom;
                                if (BoxingRoom.SoloQueue != null)
                                {
                                    if (BoxingRoom.SoloQueue.PlayerQueue.Contains(user.GetClient()))
                                    {
                                        BoxingRoom.SoloQueue.PlayerQueue.Remove(user.GetClient());
                                        user.GetClient().GetRoleplay().IsBoxing = false;
                                    }

                                    if (BoxingRoom.SoloQueue.Boxers.ContainsKey(user.GetClient()))
                                        BoxingRoom.SoloQueue.RemoveBoxer(user.GetClient());
                                }
                            }
                        }


                        if (user.GetClient().GetRoleplay().inColourWars)
                        {

                            if (!_room.RoomData.Description.Contains("COLOR"))
                            {

                                ColourManager.RemovePlayerFromTeam(user.GetClient(), user.GetClient().GetRoleplay().ColourWarTeam, true);
                            }
                        }

                        if (user.GetClient().GetRoleplay().InMafiaWars)
                        {
                            if (!_room.RoomData.Description.Contains("MAFIAWARS"))
                            {
                                Plus.GetGame().MafiaWars.RemoveUserFromGame(user.GetClient(), user.GetClient().GetRoleplay().TeamString);
                            }
                        }

                        #region Pet
                        if (user.GetClient().GetRoleplay().UsingPet)
                        {
                            // GENERATE PET INIT BRUVVV KNOW THAT FAM
                            //user.GetClient().GetMessageHandler().PlacePet(user.GetClient());
                        }
                        #endregion
                        if (user.GetClient().GetRoleplay().Dead)
                        {
                            if (!_room.RoomData.Description.Contains("HOSP"))
                            {
                                client.SendNotif("You cannot leave the hospital while you are dead!");
                                RoleplayManager.HandleDeath(client);
                            }
                        }

                        if (user.GetClient().GetRoleplay().Jailed && !user.GetClient().GetRoleplay().Dead)
                        {
                            if (!_room.RoomData.Description.Contains("JAIL"))
                            {
                                client.SendNotif("You cannot leave the jail while you are arrested!");
                                client.GetRoleplay().Transport(9, 2);

                            }
                        }
                    }
                }


                user.GetClient().GetRoleplay().setRoleplayComponents(true);

            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(ex.ToString());
            }
        }

        /// <summary>
        /// Handles the <see cref="E:Remove" /> event.
        /// </summary>
        /// <param name="user"></param>
        private void OnRemove(RoomUser user)
        {
            try
            {
                if (user == null || user.GetClient() == null) return;
                var client = user.GetClient();
                var list = UserList.Values.Where(current => current.IsBot && !current.IsPet);
                var list2 = new List<RoomUser>();
                foreach (var current2 in list)
                {
                    current2.BotAI.OnUserLeaveRoom(client);
                    if (current2.IsPet && current2.PetData.OwnerId == user.UserId &&
                        !_room.CheckRights(client, true, false))
                        list2.Add(current2);
                }
                foreach (
                    var current3 in
                        list2.Where(
                            current3 =>
                                user.GetClient() != null && user.GetClient().GetHabbo() != null &&
                                user.GetClient().GetHabbo().GetInventoryComponent() != null))
                {
                    user.GetClient().GetHabbo().GetInventoryComponent().AddPet(current3.PetData);
                    RemoveBot(current3.VirtualId, false);
                }
                _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(ex.ToString());
            }
        }

        /// <summary>
        /// Called when [user update status].
        /// </summary>
        public void OnUserUpdateStatus()
        {
            foreach (var current in UserList.Values)
                UpdateUserStatus(current, false);
        }

        /// <summary>
        /// Called when [user update status].
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// </summary>
        public void OnUserUpdateStatus(int x, int y)
        {
            foreach (var current in UserList.Values.Where(current => current.X == x && current.Y == y))
                UpdateUserStatus(current, false);
        }

        /// <summary>
        /// Determines whether the specified user is valid.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if the specified user is valid; otherwise, <c>false</c>.</returns>
        private bool IsValid(RoomUser user)
        {
            return user != null && (user.IsBot ||
                                    (user.GetClient() != null && user.GetClient().GetHabbo() != null &&
                                     user.GetClient().GetHabbo().CurrentRoomId == _room.RoomId));
        }
    }
}