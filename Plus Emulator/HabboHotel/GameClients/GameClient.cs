using Plus.Configuration;
using Plus.Connection.Connection;
using Plus.Connection.Net;
using Plus.HabboHotel.Misc;
using Plus.HabboHotel.Roleplay.Instance;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.UserDataManagement;
using Plus.Messages;
using Plus.Messages.Handlers;
using Plus.Messages.Parsers;
using System;
using System.Data;
using System.Linq;
using Plus.HabboHotel.Support;

namespace Plus.HabboHotel.GameClients
{
    /// <summary>
    /// Class GameClient.
    /// </summary>
    public class GameClient
    {
        /// <summary>
        /// The publicist count
        /// </summary>
        internal byte PublicistCount;

        /// <summary>
        /// The time pinged received
        /// </summary>
        internal DateTime TimePingedReceived;

        /// <summary>
        /// The packet parser
        /// </summary>
        internal GamePacketParser PacketParser;

        public bool LoggingOut = false;

        /// <summary>
        /// The designed handler
        /// </summary>
        internal int DesignedHandler = 1;

        /// <summary>
        /// The current room user identifier
        /// </summary>
        internal int CurrentRoomUserId;



        /// <summary>
        /// The machine identifier
        /// </summary>
        internal string MachineId;

        /// <summary>
        /// The _connection
        /// </summary>
        private ConnectionInformation _connection;

        /// <summary>
        /// The _message handler
        /// </summary>
        private GameClientMessageHandler _messageHandler;

        /// <summary>
        /// The _habbo
        /// </summary>
        private Habbo _habbo;
        public RoleplayInstance _roleplay;

        /// <summary>
        /// The _disconnected
        /// </summary>
        private bool _disconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameClient"/> class.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="connection">The connection.</param>
        internal GameClient(uint clientId, ConnectionInformation connection)
        {
            ConnectionId = clientId;
            _connection = connection;
            CurrentRoomUserId = -1;
            PacketParser = new GamePacketParser(this);
        }

        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        /// <value>The connection identifier.</value>
        internal uint ConnectionId { get; private set; }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>ConnectionInformation.</returns>
        internal ConnectionInformation GetConnection()
        {
            return _connection;
        }

        /// <summary>
        /// Gets the message handler.
        /// </summary>
        /// <returns>GameClientMessageHandler.</returns>
        internal GameClientMessageHandler GetMessageHandler()
        {
            return _messageHandler;
        }

        /// <summary>
        /// Gets the habbo.
        /// </summary>
        /// <returns>Habbo.</returns>
        internal Habbo GetHabbo()
        {
            return _habbo;
        }
        internal RoleplayInstance GetRoleplay()
        {
            return _roleplay;
        }

        /// <summary>
        /// Starts the connection.
        /// </summary>
        public void StartConnection()
        {
            if (_connection == null)
                return;

            TimePingedReceived = DateTime.Now;

            (_connection.parser as InitialPacketParser).PolicyRequest += PolicyRequest;
            (_connection.parser as InitialPacketParser).SwitchParserRequest += SwitchParserRequest;
            _connection.startPacketProcessing();
        }

        /// <summary>
        /// Initializes the handler.
        /// </summary>
        internal void InitHandler()
        {
            _messageHandler = new GameClientMessageHandler(this);
        }

        /// <summary>
        /// Tries the login.
        /// </summary>
        /// <param name="authTicket">The authentication ticket.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool TryLogin(string authTicket)
        {
            try
            {
                string ip = GetConnection().getIp();
                uint errorCode = 0;
                UserData userData = UserDataFactory.GetUserData(authTicket, out errorCode);
                if (errorCode == 1 || errorCode == 2)
                {
                    Disconnect("error");
                    return false;
                }

                #region Ban Checking
                //Let's have a quick search for a ban before we successfully authenticate..
                ModerationBan BanRecord;
                if (!string.IsNullOrEmpty(MachineId))
                {
                    if (Plus.GetGame().GetBanManager().IsBanned(MachineId, out BanRecord))
                    {
                        if (Plus.GetGame().GetBanManager().MachineBanCheck(MachineId))
                        {
                            Disconnect("machine banned - login attempt");
                            return false;
                        }
                    }
                }

                if (userData.User != null)
                {
                    //Now let us check for a username ban record..
                    BanRecord = null;
                    if (Plus.GetGame().GetBanManager().IsBanned(userData.User.UserName, out BanRecord))
                    {
                        if (Plus.GetGame().GetBanManager().UsernameBanCheck(userData.User.UserName))
                        {
                            Disconnect("banned - login attempt");
                            return false;
                        }
                    }
                }
                #endregion

                Plus.GetGame().GetClientManager().RegisterClient(this, userData.UserId, userData.User.UserName);
                _habbo = userData.User;

                if (_habbo != null)
                {
                    userData.User.LoadData(userData);

                    using (Database.Manager.Database.Session_Details.Interfaces.IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT * FROM `rp_stats` WHERE `id` = '" + userData.UserId + "'");
                        DataRow Row = dbClient.GetRow();

                        RoleplayInstance RPData = CreateRoleplayInstance.GenerateInstance(Row);
                        _roleplay = RPData;

                        _roleplay.setRoleplayComponents();
                    }

                    userData.User.Init(this, userData);
                    var queuedServerMessage = new QueuedServerMessage(_connection);
                    var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UniqueMachineIDMessageComposer"));
                    serverMessage.AppendString(MachineId);
                    queuedServerMessage.AppendResponse(serverMessage);
                    queuedServerMessage.AppendResponse(
                        new ServerMessage(LibraryParser.OutgoingRequest("AuthenticationOKMessageComposer")));
                    if (_habbo != null)
                    {
                        var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("HomeRoomMessageComposer"));
                        serverMessage2.AppendInteger(_habbo.HomeRoom);
                        serverMessage2.AppendInteger(_habbo.HomeRoom);
                        queuedServerMessage.AppendResponse(serverMessage2);
                    }
                    serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("MinimailCountMessageComposer"));
                    serverMessage.AppendInteger(_habbo.MinimailUnreadMessages);
                    queuedServerMessage.AppendResponse(serverMessage);

                    serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FavouriteRoomsMessageComposer"));
                    serverMessage.AppendInteger(30);

                    if (userData.User.FavoriteRooms == null || !userData.User.FavoriteRooms.Any()) serverMessage.AppendInteger(0);
                    else
                    {
                        serverMessage.AppendInteger(userData.User.FavoriteRooms.Count);
                        foreach (uint i in userData.User.FavoriteRooms) serverMessage.AppendInteger(i);
                    }
                    queuedServerMessage.AppendResponse(serverMessage);

                    var rightsMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserClubRightsMessageComposer"));
                    rightsMessage.AppendInteger(userData.User.GetSubscriptionManager().HasSubscription ? 2 : 0);
                    rightsMessage.AppendInteger(userData.User.Rank);
                    rightsMessage.AppendInteger(0);
                    queuedServerMessage.AppendResponse(rightsMessage);

                    serverMessage =
                        new ServerMessage(LibraryParser.OutgoingRequest("EnableNotificationsMessageComposer"));
                    serverMessage.AppendBool(true); //isOpen
                    serverMessage.AppendBool(false);
                    queuedServerMessage.AppendResponse(serverMessage);

                    serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("EnableTradingMessageComposer"));
                    serverMessage.AppendBool(true);
                    queuedServerMessage.AppendResponse(serverMessage);
                    userData.User.UpdateCreditsBalance();

                    serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ActivityPointsMessageComposer"));
                    serverMessage.AppendInteger(2);
                    serverMessage.AppendInteger(0);
                    serverMessage.AppendInteger(userData.User.ActivityPoints);
                    serverMessage.AppendInteger(5);
                    serverMessage.AppendInteger(userData.User.BelCredits);
                    queuedServerMessage.AppendResponse(serverMessage);

                    if (userData.User.HasFuse("fuse_mod")) queuedServerMessage.AppendResponse(Plus.GetGame().GetModerationTool().SerializeTool(this));
                    queuedServerMessage.AppendResponse(
                        Plus.GetGame().GetAchievementManager().AchievementDataCached);

                    if (!GetHabbo().NuxPassed && ExtraSettings.NEW_users_gifts_ENABLED)
                        queuedServerMessage.AppendResponse(
                            new ServerMessage(LibraryParser.OutgoingRequest("NuxSuggestFreeGiftsMessageComposer")));
                    queuedServerMessage.AppendResponse(GetHabbo().GetAvatarEffectsInventoryComponent().GetPacket());
                    queuedServerMessage.SendResponse();
                    Plus.GetGame().GetAchievementManager().TryProgressHabboClubAchievements(this);
                    Plus.GetGame().GetAchievementManager().TryProgressRegistrationAchievements(this);
                    Plus.GetGame().GetAchievementManager().TryProgressLoginAchievements(this);
                    return true;
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Bug during user login: " + e);
            }
            return false;
        }

        /// <summary>
        /// Sends the notif with scroll.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendNotifWithScroll(string message)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("MOTDNotificationMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendString(message);
            SendMessage(serverMessage);
        }

        /// <summary>
        /// Sends the broadcast message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendBroadcastMessage(string message)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
            serverMessage.AppendString(message);
            serverMessage.AppendString(string.Empty);
            SendMessage(serverMessage);
        }

        internal void Shout(string Message, int Bubble = 0, bool RemoveAfter = false)
        {
            try
            {
                if (GetHabbo().CurrentRoom == null)
                    return;
                RoomUser User = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().UserName);
                if (User == null)
                    return;

                if (Bubble != 0)
                {
                    User.LastBubble = Bubble;
                }

                User.Chat(this, Message, true, 0, User.LastBubble);

                if (RemoveAfter)
                {
                    User.LastBubble = 0;
                }
            }
            catch { }
        }

        internal void commandShout(string Message, int Bubble = 0, bool RemoveAfter = false)
        {
            try
            {
                if (GetHabbo().CurrentRoom == null)
                    return;
                RoomUser User = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().UserName);
                if (User == null)
                    return;

                if (Bubble != 0)
                {
                    User.LastBubble = Bubble;
                }

                User.commandChat(this, Message, true, 0, User.LastBubble);

                if (RemoveAfter)
                {
                    User.LastBubble = 0;
                }
            }
            catch { }
        }

        /// <summary>
        /// Sends the moderator message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendModeratorMessage(string message)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ModerationAlertMessageComposer"));
            serverMessage.AppendString(message);
            serverMessage.AppendString(string.Empty);
            SendMessage(serverMessage);
        }

        internal void SendWhisperBubble(string message, int Bubble = 0)
        {
            if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;
            var roomUserByHabbo =
                GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().UserName);
            if (roomUserByHabbo == null)
                return;

            var whisp = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
            whisp.AppendInteger(roomUserByHabbo.VirtualId);
            whisp.AppendString(message);
            whisp.AppendInteger(0);
            whisp.AppendInteger(false ? 34 : Bubble);
            whisp.AppendInteger(0);
            whisp.AppendInteger(false);
            SendMessage(whisp);
        }

        /// <summary>
        /// Sends the whisper.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendWhisper(string message, bool fromWired = false, int Bubble = 0, bool RemoveAfter = true)
        {
            if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;
            var roomUserByHabbo =
                GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().UserName);
            if (roomUserByHabbo == null)
                return;
            if (Bubble != 0)
            {
                roomUserByHabbo.LastBubble = Bubble;
            }
            var whisp = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
            whisp.AppendInteger(roomUserByHabbo.VirtualId);
            whisp.AppendString(message);
            whisp.AppendInteger(0);
            whisp.AppendInteger(fromWired ? 34 : roomUserByHabbo.LastBubble);
            whisp.AppendInteger(0);
            whisp.AppendInteger(fromWired);
            SendMessage(whisp);
            if (RemoveAfter)
            {
                roomUserByHabbo.LastBubble = 0;
            }
        }

        /// <summary>
        /// Sends the notif.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <param name="picture">The picture.</param>
        internal void SendNotif(string message, string title = "Notification", string picture = "")
        {
            SendMessage(GetBytesNotif(message, title, picture));
        }

        /// <summary>
        /// Gets the bytes notif.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <param name="picture">The picture.</param>
        /// <returns>System.Byte[].</returns>
        public static byte[] GetBytesNotif(string message, string title = "Notification", string picture = "")
        {
            using (
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"))
                )
            {
                serverMessage.AppendString(picture);
                serverMessage.AppendInteger(4);
                serverMessage.AppendString("title");
                serverMessage.AppendString(title);
                serverMessage.AppendString("message");
                serverMessage.AppendString(message);
                serverMessage.AppendString("linkUrl");
                serverMessage.AppendString("event:");
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString("ok");

                return serverMessage.GetReversedBytes();
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        internal void Stop()
        {
            if (GetMessageHandler() != null)
            {
                _messageHandler.Destroy();
            }

            if (GetHabbo() != null)
            {
                _habbo.OnDisconnect("Logged out");
            }

            CurrentRoomUserId = -1;
            _messageHandler = null;
            _habbo = null;
            _connection = null;
        }

        /// <summary>
        /// Disconnects the specified reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        internal void Disconnect(string reason)
        {

            if (GetHabbo() != null)
            {
                using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
                    queryReactor.RunFastQuery(GetHabbo().GetQueryString);
                GetHabbo().OnDisconnect(reason);
            }
            if (_disconnected)
                return;
            if (_connection != null)
                _connection.Dispose();
            _disconnected = true;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessage(ServerMessage message)
        {
            if (message == null)
                return;
            var bytes = message.GetReversedBytes();
            if (GetConnection() == null)
                return;
            GetConnection().SendData(bytes);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        internal void SendMessage(byte[] bytes)
        {
            if (GetConnection() == null)
                return;
            GetConnection().SendData(bytes);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="type">The type.</param>
        internal void SendMessage(StaticMessage type)
        {
            if (GetConnection() == null)
                return;
            GetConnection().SendData(StaticMessagesManager.Get(type));
        }

        /// <summary>
        /// Switches the parser request.
        /// </summary>
        private void SwitchParserRequest()
        {
            if (_messageHandler == null)
                InitHandler();
            PacketParser.SetConnection(_connection);
            PacketParser.OnNewPacket += parser_onNewPacket;
            var currentData = (_connection.parser as InitialPacketParser).CurrentData;
            _connection.parser.Dispose();
            _connection.parser = PacketParser;
            _connection.parser.HandlePacketData(currentData);
        }

        /// <summary>
        /// Parser_ons the new packet.
        /// </summary>
        /// <param name="message">The message.</param>
        private void parser_onNewPacket(ClientMessage message)
        {
            try
            {

                if (message == null)
                    return;

                if (_messageHandler == null)
                    return;

                _messageHandler.HandleRequest(message);
                //Cnsole.WriteLine("Handled Packet: " + Message, Mercury.ConsoleColor.Red);
                //Logging.LogMessage("Handled Packet: " + message.Id);
            }
            catch (Exception ex)
            {
                Logging.LogPacketException("[" + message.Id + "] " + message, ex.ToString());
            }
        }

        /// <summary>
        /// Policies the request.
        /// </summary>
        private void PolicyRequest()
        {
            _connection.SendData(CrossDomainPolicy.XmlPolicyBytes);
        }

        internal void HandlePublicista(string Message)
        {
            bool flag = false;
            if (PublicistCount < 2)
            {
                Plus.GetGame().GetClientManager().SendSuperNotif("Hey!", "Please stop advertising other hotels. You will be muted if you do it again.<br /><br />Message/Word blacklisted: <b>" + Message + "</b>", "frank10", this, "event:", "ok", false, false);
            }
            else if (PublicistCount < 3)
            {
                Plus.GetGame().GetClientManager().SendSuperNotif("You have been muted!", "Sorry but you were muted for <b>advertising other hotels/roleplays</b>.<br /><br />Message/Word blacklisted: <b>" + Message + "</b>", "frank10", this, "event:", "ok", false, false);
                GetHabbo().Mute();
                flag = true;
            }
            else if (PublicistCount >= 3)
            {
                Plus.GetGame().GetClientManager().SendSuperNotif("Warning!", "If you continue to mention and/or advertise other hotels/roleplays, you will be banned!<br /><br />Message/Word blacklisted: <b>" + Message + "</b>", "frank10", this, "event:", "ok", false, false);
                return;
            }
            ServerMessage serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            serverMessage.AppendString("idk");
            serverMessage.AppendInteger(4);
            serverMessage.AppendString("title");
            serverMessage.AppendString("Possible advertiser found!");
            serverMessage.AppendString("message");
            serverMessage.AppendString(string.Concat(new string[]
			{
				"The following citizen has been detected as advertising for a rival hotel:  <b>" + GetHabbo().UserName + "!</b><br /><br />Do you think " + GetHabbo().UserName + " is advertising for a hotel/roleplay like this?:\"<b>",
				Message,
				"</b>\".<br /><br />",
				flag ? "<i>The user was automatically muted.</i>" : "<i>The user was automatically warned.</i>"
			}));
            serverMessage.AppendString("link");
            serverMessage.AppendString("event:");
            serverMessage.AppendString("linkTitle");
            serverMessage.AppendString("ok");

            Plus.GetGame().GetClientManager().StaffAlert(serverMessage, 0);
        }
    }
}