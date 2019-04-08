using Plus.Configuration;
using Plus.Connection.Connection;
using Plus.HabboHotel.Users.Messenger;
using Plus.Messages;
using Plus.Messages.Parsers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Plus.HabboHotel.Roleplay.Components;
using Plus.HabboHotel.Roleplay.Misc;
using System.Threading;
using Plus.Database.Manager.Database.Session_Details.Interfaces;

namespace Plus.HabboHotel.GameClients
{
    /// <summary>
    /// Class GameClientManager.
    /// </summary>
    internal class GameClientManager
    {
        /// <summary>
        /// The clients
        /// </summary>
        internal ConcurrentDictionary<uint, GameClient> Clients;

        /// <summary>
        /// The _clients add queue
        /// </summary>
        private readonly Queue _clientsAddQueue;

        /// <summary>
        /// The _clients to remove
        /// </summary>
        private readonly Queue _clientsToRemove;

        /// <summary>
        /// The _badge queue
        /// </summary>
        private readonly Queue _badgeQueue;

        /// <summary>
        /// The _broadcast queue
        /// </summary>
        private readonly ConcurrentQueue<byte[]> _broadcastQueue;

        /// <summary>
        /// The _user name register
        /// </summary>
        private readonly HybridDictionary _userNameRegister;

        /// <summary>
        /// The _user identifier register
        /// </summary>
        private readonly HybridDictionary _userIdRegister;

        /// <summary>
        /// The _user name identifier register
        /// </summary>
        private readonly HybridDictionary _userNameIdRegister;

        /// <summary>
        /// The _id user name register
        /// </summary>
        private readonly HybridDictionary _idUserNameRegister;

        /// <summary>
        /// The _timed out connections
        /// </summary>
        private readonly Queue _timedOutConnections;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameClientManager"/> class.
        /// </summary>
        internal GameClientManager()
        {
            Clients = new ConcurrentDictionary<uint, GameClient>();
            _clientsAddQueue = new Queue();
            _clientsToRemove = new Queue();
            _badgeQueue = new Queue();
            _broadcastQueue = new ConcurrentQueue<byte[]>();
            _timedOutConnections = new Queue();
            _userNameRegister = new HybridDictionary();
            _userIdRegister = new HybridDictionary();
            _userNameIdRegister = new HybridDictionary();
            _idUserNameRegister = new HybridDictionary();

            var thread = new Thread(HandleTimeouts);
            thread.Name = "Time Out Handler";
            thread.Start();
        }

        /// <summary>
        /// Gets the client count.
        /// </summary>
        /// <value>The client count.</value>
        internal int ClientCount
        {
            get { return Clients.Count; }
        }

        /// <summary>
        /// Gets the client by user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>GameClient.</returns>
        internal GameClient GetClientByUserId(uint userId)
        {
            return _userIdRegister.Contains(userId) ? (GameClient)_userIdRegister[userId] : null;
        }

        /// <summary>
        /// Gets the name of the client by user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>GameClient.</returns>
        internal GameClient GetClientByUserName(string userName)
        {
            return _userNameRegister.Contains(userName.ToLower())
                ? (GameClient)_userNameRegister[userName.ToLower()]
                : null;
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>GameClient.</returns>
        internal GameClient GetClient(uint clientId)
        {
            return Clients.ContainsKey(clientId) ? Clients[clientId] : null;
        }

        /// <summary>
        /// Gets the name by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>System.String.</returns>
        internal string GetNameById(uint id)
        {
            var clientByUserId = GetClientByUserId(id);
            if (clientByUserId != null)
                return clientByUserId.GetHabbo().UserName;
            string String;
            using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT username FROM users WHERE id = " + id);
                String = queryReactor.GetString();
            }
            return String;
        }

        /// <summary>
        /// Gets the clients by identifier.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns>IEnumerable&lt;GameClient&gt;.</returns>
        internal IEnumerable<GameClient> GetClientsById(Dictionary<uint, MessengerBuddy>.KeyCollection users)
        {
            return users.Select(GetClientByUserId).Where(clientByUserId => clientByUserId != null);
        }

        /// <summary>
        /// Sends the super notif.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="notice">The notice.</param>
        /// <param name="picture">The picture.</param>
        /// <param name="client">The client.</param>
        /// <param name="link">The link.</param>
        /// <param name="linkTitle">The link title.</param>
        /// <param name="broadCast">if set to <c>true</c> [broad cast].</param>
        /// <param name="Event">if set to <c>true</c> [event].</param>
        internal void SendSuperNotif(string title, string notice, string picture, GameClient client, string link,
            string linkTitle, bool broadCast, bool Event)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            serverMessage.AppendString(picture);
            serverMessage.AppendInteger(4);
            serverMessage.AppendString("title");
            serverMessage.AppendString(title);
            serverMessage.AppendString("message");
            if (broadCast)
                if (Event)
                {
                    var text1 = Plus.GetLanguage().GetVar("ha_event_one");
                    var text2 = Plus.GetLanguage().GetVar("ha_event_two");
                    var text3 = Plus.GetLanguage().GetVar("ha_event_three");
                    serverMessage.AppendString(string.Format("<b>{0} {1}!</b>\r\n {2} .\r\n<b>{3}</b>\r\n{4}", text1,
                        client.GetHabbo().CurrentRoom.RoomData.Owner, text2, text3, notice));
                }
                else
                {
                    var text4 = Plus.GetLanguage().GetVar("ha_title");
                    serverMessage.AppendString(string.Concat("<b>" + text4 + "</b>\r\n", notice, "\r\n- <i>", client.GetHabbo().UserName, "</i>"));
                }
            else
                serverMessage.AppendString(notice);
            if (link != string.Empty)
            {
                serverMessage.AppendString("linkUrl");
                serverMessage.AppendString(link);
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString(linkTitle);
            }
            else
            {
                serverMessage.AppendString("linkUrl");
                serverMessage.AppendString("event:");
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString("ok");
            }

            if (broadCast)
            {
                QueueBroadcaseMessage(serverMessage);
                return;
            }
            client.SendMessage(serverMessage);
        }

        /// <summary>
        /// Called when [cycle].
        /// </summary>
        internal void OnCycle()
        {
            try
            {
                GiveBadges();
                BroadcastPackets();
                Plus.GetGame().ClientManagerCycleEnded = true;
            }
            catch (Exception ex)
            {
                Logging.LogThreadException(ex.ToString(), "GameClientManager.OnCycle Exception --> Not inclusive");
            }
        }

        /// <summary>
        /// Staffs the alert.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exclude">The exclude.</param>
        internal void StaffAlert(ServerMessage message, uint exclude = 0u)
        {
            var gameClients =
                Clients.Values
                    .Where(
                        x =>
                            x.GetHabbo() != null && x.GetHabbo().Rank >= Plus.StaffAlertMinRank &&
                            x.GetHabbo().Id != exclude);
            foreach (var current in gameClients)
                current.SendMessage(message);
        }



        internal void VIPAlert(ServerMessage message, uint exclude = 999999999u)
        {
            var gameClients =
                Clients.Values
                    .Where(
                        x =>
                            x.GetHabbo() != null && x.GetHabbo().Rank >= 2 &&
                            x.GetHabbo().Id != exclude);
            foreach (var current in gameClients)
                current.SendMessage(message);
        }

        /// <summary>
        /// Mods the alert.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void ModAlert(ServerMessage message)
        {
            var bytes = message.GetReversedBytes();
            foreach (
                var current in
                    Clients.Values.Where(current => current != null && current.GetHabbo() != null))
            {
                if (current.GetHabbo().Rank != 4u && current.GetHabbo().Rank != 5u)
                    if (current.GetHabbo().Rank != 6u)
                        continue;
                try
                {
                    current.GetConnection().SendData(bytes);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Creates the and start client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="connection">The connection.</param>
        internal void CreateAndStartClient(uint clientId, ConnectionInformation connection)
        {
            var gameClient = new GameClient(clientId, connection);
            Clients.AddOrUpdate(clientId, gameClient, (key, value) => gameClient);
            gameClient.StartConnection();
        }

        /// <summary>
        /// Disposes the connection.
        /// </summary>
        /// <param name="clientID"></param>
        internal void DisposeConnection(uint clientID)
        {
            GameClient Client = null;
            if (!Clients.TryRemove(clientID, out Client))
                return;

            if (Client != null)
            { 
                ProcessLogout Logout = new ProcessLogout(Client);
            }

            Clients.TryRemove(clientID, out Client);
        }

        /// <summary>
        /// Queues the broadcase message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void QueueBroadcaseMessage(ServerMessage message)
        {
            _broadcastQueue.Enqueue(message.GetReversedBytes());
        }

        /// <summary>
        /// Queues the badge update.
        /// </summary>
        /// <param name="badge">The badge.</param>
        internal void QueueBadgeUpdate(string badge)
        {
            lock (_badgeQueue.SyncRoot)
                _badgeQueue.Enqueue(badge);
        }

        /// <summary>
        /// Logs the clones out.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal void LogClonesOut(uint userId)
        {
            var clientByUserId = GetClientByUserId(userId);
            if (clientByUserId != null)
                clientByUserId.Disconnect("user null LogClonesOut");
        }

        /// <summary>
        /// Registers the client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userName">Name of the user.</param>
        internal void RegisterClient(GameClient client, uint userId, string userName)
        {
            if (_userNameRegister.Contains(userName.ToLower()))
                _userNameRegister[userName.ToLower()] = client;
            else
                _userNameRegister.Add(userName.ToLower(), client);

            if (_userIdRegister.Contains(userId))
                _userIdRegister[userId] = client;
            else
                _userIdRegister.Add(userId, client);
        }

        /// <summary>
        /// Unregisters the client.
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <param name="userName">The username.</param>
        internal void UnregisterClient(uint userid, string userName)
        {
            _userIdRegister.Remove(userid);
            _userNameRegister.Remove(userName.ToLower());
            using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
                queryReactor.SetQuery(string.Format("UPDATE users SET online='0' WHERE id={0} LIMIT 1", userid));
        }

        /// <summary>
        /// Closes all & saves all statistics.
        /// </summary>
        public void CloseAll()
        {
            RoleplayManager.saveAllStats();
            Out.WriteLine("Done saving users stats!");

            Bounties.SaveBounty();
            Out.WriteLine("Done saving all bounties!");

            foreach (GameClient client in Clients.Values.Where(client => client.GetHabbo() != null))
            {
                if (client == null)
                    continue;

                if (client.GetHabbo() != null)
                {
                    try
                    {
                        using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunFastQuery(client.GetHabbo().GetQueryString);
                        }
                        Console.Clear();
                        Out.WriteLine("<<- SERVER SHUTDOWN ->> IVNENTORY IS SAVING");
                    }
                    catch
                    {
                    }
                }
            }

            Out.WriteLine("Done saving users inventory!");
            Out.WriteLine("Closing server connections...");
            try
            {
                foreach (GameClient client2 in Clients.Values.Where(client2 => client2.GetHabbo() != null))
                {
                    if (client2 == null || client2.GetConnection() == null)
                        continue;

                    try
                    {
                        client2.GetConnection().Dispose();
                    }
                    catch { }

                    Console.Clear();
                    Out.WriteLine("<<- SERVER SHUTDOWN ->> CLOSING CONNECTIONS");

                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException(e.ToString());
            }

            if (Clients.Count > 0)
                Clients.Clear();

            Out.WriteLine("Connections closed!");
        }

        /// <summary>
        /// Updates the client.
        /// </summary>
        /// <param name="oldName">The old name.</param>
        /// <param name="newName">The new name.</param>
        internal void UpdateClient(string oldName, string newName)
        {
            if (!_userNameRegister.Contains(oldName.ToLower()))
                return;
            var old = (GameClient)_userNameRegister[oldName.ToLower()];
            _userNameRegister.Remove(oldName.ToLower());
            _userNameRegister.Add(newName.ToLower(), old);
        }

        /// <summary>
        /// Handles the timeouts.
        /// </summary>
        private void HandleTimeouts()
        {
            while (true)
            {
                try
                {
                    while (_timedOutConnections != null && _timedOutConnections.Count > 0)
                    {
                        GameClient gameClient = null;
                        lock (_timedOutConnections.SyncRoot)
                            if (_timedOutConnections.Count > 0)
                                gameClient = (GameClient)_timedOutConnections.Dequeue();
                        if (gameClient != null)
                            gameClient.Disconnect("user null HandleTimeouts");
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogThreadException(ex.ToString(), "HandleTimeoutsVoid");
                }
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// Gives the badges.
        /// </summary>
        private void GiveBadges()
        {
            try
            {
                var now = DateTime.Now;
                if (_badgeQueue.Count > 0)
                    lock (_badgeQueue.SyncRoot)
                        while (_badgeQueue.Count > 0)
                        {
                            var badge = (string)_badgeQueue.Dequeue();
                            foreach (
                                var current in
                                    Clients.Values.Where(current => current.GetHabbo() != null))
                                try
                                {
                                    current.GetHabbo().GetBadgeComponent().GiveBadge(badge, true, current, false);
                                    current.SendNotif(Plus.GetLanguage().GetVar("user_earn_badge"));
                                }
                                catch
                                {
                                }
                        }
                var timeSpan = DateTime.Now - now;
                if (timeSpan.TotalSeconds > 3.0)
                    Console.WriteLine("GameClientManager.GiveBadges spent: {0} seconds in working.",
                        timeSpan.TotalSeconds);
            }
            catch (Exception ex)
            {
                Logging.LogThreadException(ex.ToString(), "GameClientManager.GiveBadges Exception --> Not inclusive");
            }
        }

        /// <summary>
        /// Broadcasts the packets.
        /// </summary>
        private void BroadcastPackets()
        {
            try
            {
                if (!_broadcastQueue.Any()) return;
                var now = DateTime.Now;
                byte[] bytes;

                _broadcastQueue.TryDequeue(out bytes);

                foreach (GameClient current in Clients.Values)
                {
                    if (current == null || current.GetConnection() == null)
                        continue;
                    current.GetConnection().SendData(bytes);
                }


                var timeSpan = DateTime.Now - now;
                if (timeSpan.TotalSeconds > 3.0)
                    Console.WriteLine("GameClientManager.BroadcastPackets spent: {0} seconds in working.",
                        timeSpan.TotalSeconds);
            }
            catch (Exception ex)
            {
                Logging.LogThreadException(ex.ToString(),
                    "GameClientManager.BroadcastPackets Exception --> Not inclusive");
            }
        }
    }
}