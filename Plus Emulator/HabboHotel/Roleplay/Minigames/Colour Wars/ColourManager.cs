using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.Messages;
using Plus.Messages.Parsers;
using Plus.Configuration;
using Plus.Database.Manager.Database.Session_Details.Interfaces;

namespace Plus.HabboHotel.Roleplay.Minigames.Colour_Wars
{
    /// <summary>
    /// Manages everything to do with colour wars
    /// </summary>
    public static class ColourManager
    {
        #region Declares
        /// <summary>
        /// Dictionary containing all teams
        /// </summary>
        public static Dictionary<string, Team> Teams;

        /// <summary>
        /// Dictionary containing all of the players
        /// </summary>

        // public static Dictionary<GameClient, Team> AllPlayers;

        /// <summary>
        /// ID of the graveyard room
        /// </summary>
        public static uint GraveYardID;

        /// <summary>
        /// Timer which counts down till the game starts
        /// </summary>
        private static Countdown CountdownTimer;

        /// <summary>
        /// Boolean representing if the match has started
        /// </summary>
        public static bool Started = false;

        /// <summary>
        /// Boolean representing if the match is about to start
        /// </summary>
        private static bool Starting = false;

        /// <summary>
        /// The game lobby
        /// </summary>
        public static uint GameLobby = Convert.ToUInt32(RoleplayData.Data["cw.lobby"]);

        /// <summary>
        /// The game lobby
        /// </summary>
        public static uint MainLobby = Convert.ToUInt32(RoleplayData.Data["minigame.lobby"]);

        /// <summary>
        /// Prize the users get for winning default 1000
        /// </summary>
        public static int Prize = Convert.ToInt32(RoleplayData.Data["cw.default.prize"]);

        /// <summary>
        /// FUCKING BOOLEAN REP
        /// </summary>
        public static Team WinningTeam = null;

        /// <summary>
        /// Points the users get for winning
        /// </summary>
        public static int WinPoints = Convert.ToInt32(RoleplayData.Data["cw.win.pts"]);

        /// <summary>
        /// Points the users get for capturing a base
        /// </summary>
        public static int CapturePoints = Convert.ToInt32(RoleplayData.Data["cw.capture.pts"]);

        /// <summary>
        /// Points the users get for killing a player
        /// </summary>
        public static int KillPoints = Convert.ToInt32(RoleplayData.Data["cw.kill.pts"]);

        /// <summary>
        /// Loop
        /// </summary>
        public static ShittyLoopForYing LobbyLoop;
        #endregion

        #region Methods
        /// <summary>
        /// When this void is called it will initialize variables
        /// </summary>
        public static void Initialize()
        {
            // Initialzie variables
            Teams = new Dictionary<string, Team>();
            //AllPlayers = new Dictionary<GameClient, Team>();
            Started = false;
            Starting = false;
            GraveYardID = 121;

            // Now create some teams
            InitializeTeams();
        }

        /// <summary>
        /// Initializes all teams
        /// </summary>
        private static void InitializeTeams()
        {
            // Create instances
            Team PinkTeam = new Team("Pink", 1, "ha-3352-1291-1291.lg-275-1291.sh-290-1291.ch-3030-1291", 105, 1, 6, 0, 7, 9);
            Team BlueTeam = new Team("Blue", 2, "ha-3352-106-106.lg-275-106.sh-290-106.ch-3030-106", 111, 1, 14, 0, 12, 10);
            Team YellowTeam = new Team("Yellow", 3, "ch-3030-93.ha-3352-93-93.lg-275-93.sh-290-93", 123, 1, 30, 0, 7, 9);
            Team GreenTeam = new Team("Green", 4, "ha-3352-101-101.sh-290-101.lg-275-101.ch-3030-101", 104, 1, 22, 0, 12, 10);

            // Add them all to dictionary
            Teams.Add("pink", PinkTeam);
            Teams.Add("blue", BlueTeam);
            Teams.Add("yellow", YellowTeam);
            Teams.Add("green", GreenTeam);

            // Mats where the player will spawn on enter & death
            PinkTeam.SpawnMat = "anna_pill*2";
            BlueTeam.SpawnMat = "anna_pill*4";
            YellowTeam.SpawnMat = "anna_pill*1";
            GreenTeam.SpawnMat = "anna_pill*3";
        }

        /// <summary>
        /// Returns the team via the name
        /// </summary>
        /// <param name="name">The name of the team you want to get</param>
        /// <returns>The team</returns>
        public static Team GetTeamByName(string name)
        {
            // If the team exists
            if (Teams.ContainsKey(name.ToLower()))
            {
                // Return the team
                return Teams[name];
            }
            else
                return null;
        }

        /// <summary>
        /// Adds the player to the team
        /// </summary>
        /// <param name="Player">The player to add</param>
        public static void AddPlayerToTeam(GameClient Player, string team, RoomItem BanZaiTele = null)
        {
            try
            {
                // If we actually have a player
                if (Player != null && Player.GetRoleplay() != null && Player.GetHabbo() != null)
                {
                    // If the match has not started
                    if (!Started)
                    {
                        // If the player is not in colour wars
                        if (!Player.GetRoleplay().inColourWars)
                        {
                            if (getPlayerCount() < 24)
                            {
                                // Get the team selected
                                Team TeamSelected = GetTeamByName(team);
                                // Now get the team based on balance
                                Team ActualTeam = GetTeamBalanced(TeamSelected, Player);

                                // Lets attempt to add the user to the team
                                ActualTeam.AddPlayerToTeam(Player);

                                // TODO: Change user motto, game state
                                CustomizePlayer(Player, ActualTeam);

                                // Set the players set & game state
                                Player.GetRoleplay().ColourWarTeam = ActualTeam;
                                Player.GetRoleplay().inColourWars = true;

                                // Get the room user
                                RoomUser RUser = Player.GetHabbo().GetRoomUser();

                                // If it created the room user
                                if (RUser != null)
                                {
                                    
                                    // Set the users position
                                    RUser.SetPos(ActualTeam.WAX, ActualTeam.WAY, ActualTeam.WAZ);

                                    // Set effect
                                    if (Player.GetRoleplay() != null)
                                    {
                                        Player.GetRoleplay().EffectSeconds--;
                                    }
                                    RUser.ApplyEffect(4);

                                    // Clear movement
                                    RUser.ClearMovement();
                                }

                                // Hmm, let's see if we can actually start the game
                                TryStart();
                            }
                        }
                        else
                        {
                            Player.SendWhisper("No spaces left sorry!");
                        }

                    }
                    else
                    {
                        // Tell the user they can't join this late
                        Player.SendWhisper("You cannot join the match as it has started!");
                        if (BanZaiTele != null)
                        {
                            if (Player.GetHabbo() != null)
                            {
                                if (Player.GetHabbo().GetRoomUser() != null)
                                {
                                    Player.GetHabbo().GetRoomUser().MoveTo(BanZaiTele.SquareInFront);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logging.LogCriticalException("Error in AddPlayerToTeam() void " + ex.Message + "::" + ex.StackTrace); }
        }

        public static void ForceAddPlayerToTeam(GameClient Player, string team, RoomItem BanZaiTele = null, bool ForceAdd = false, bool sendToRoom = false)
        {
            try
            {
                // If we actually have a player
                if (Player != null && Player.GetRoleplay() != null && Player.GetHabbo() != null)
                {
                    // If the match has not started
                    if (!Started)
                    {
                        // If the player is not in colour wars
                        if (!Player.GetRoleplay().inColourWars)
                        {
                            // Get the team selected
                            Team ActualTeam = GetTeamByName(team);
                            // Now get the team based on balance

                            // Lets attempt to add the user to the team
                            ActualTeam.AddPlayerToTeam(Player);

                            // TODO: Change user motto, game state
                            CustomizePlayer(Player, ActualTeam);

                            // Set the players set & game state
                            Player.GetRoleplay().ColourWarTeam = ActualTeam;
                            Player.GetRoleplay().inColourWars = true;
                            if (Player.GetHabbo().CurrentRoomId != 96 && !sendToRoom)
                            {
                                Player.GetMessageHandler().PrepareRoomForUser(96, "");
                            }
                            // Get the room user
                            RoomUser RUser = Player.GetHabbo().GetRoomUser();

                            // If it created the room user
                            if (RUser != null)
                            {
                                // Set the users position
                                RUser.SetPos(ActualTeam.WAX, ActualTeam.WAY, ActualTeam.WAZ);

                                // Set effect
                                if (Player.GetRoleplay() != null)
                                {
                                    Player.GetRoleplay().EffectSeconds--;
                                }
                                RUser.ApplyEffect(4);

                                // Clear movement
                                RUser.ClearMovement();
                            }

                            // Hmm, let's see if we can actually start the game
                            TryStart();

                        }

                    }
                    else
                    {
                        if (!Player.GetRoleplay().inColourWars)
                        {
                            // Get the team selected
                            Team ActualTeam = GetTeamByName(team);
                            // Now get the team based on balance

                            // Lets attempt to add the user to the team
                            ActualTeam.AddPlayerToTeam(Player);

                            // TODO: Change user motto, game state
                            CustomizePlayer(Player, ActualTeam);

                            // Set the players set & game state
                            Player.GetRoleplay().ColourWarTeam = ActualTeam;
                            Player.GetRoleplay().inColourWars = true;

                            // Get the room user
                            RoomUser RUser = Player.GetHabbo().GetRoomUser();

                            // If it created the room user
                            if (RUser != null)
                            {
                                // Set the users position
                                if (!sendToRoom)
                                {
                                    Player.GetMessageHandler().PrepareRoomForUser(ActualTeam.BaseRID, "");
                                }

                                Player.GetRoleplay().CurHealth = 120;
                                Player.GetRoleplay().Energy = 100;
                                Player.GetRoleplay().Hunger = 0;
                            }

                        }
                    }
                }
            }
            catch (Exception ex) { Logging.LogCriticalException("Error in ForceAddPlayerToTeam() void " + ex.Message + "::" + ex.StackTrace); }
        }

        /// <summary>
        /// Removes a player from the team
        /// </summary>
        /// <param name="Player">The player to remove</param>
        /// <param name="TheTeam">The team that the player is currently in</param>
        public static void RemovePlayerFromTeam(GameClient Player, Team TheTeam, bool AlertMembers = true, string reason = "You have left color wars!", bool disconnect = false)
        {
            try
            {
                // If the team exists
                if (TheTeam != null)
                {
                    // If the match has not started
                    if (!Starting)
                    {

                        // Remove the user from team

                        if (Player == null)
                            return;

                        TheTeam.Players.Remove(Player);


                        if (Player.GetRoleplay() == null)
                            return;

                        if (disconnect)
                            return;

                        if (Player.GetHabbo() == null)
                            return;

                        // If it wasn't triggered by a disconnection and the player is not null
                        if (1 == 1)
                        {

                            // Now lets change the players look back to the old one!
                            Player.GetHabbo().Look = Player.GetRoleplay().figBeforeWar;

                            // Poof to refresh the look ;D
                            Player.GetRoleplay().RefreshVals();

                            // Set the players set & game state
                            Player.GetRoleplay().ColourWarTeam = null;
                            Player.GetRoleplay().inColourWars = false;

                            // Send the player to a diff room/spot
                            #region Send Player Away
                            if (Player.GetHabbo() != null)
                            {
                                if (Player.GetHabbo().GetRoomUser() != null)
                                {
                                    if (Player.GetHabbo().GetRoomUser().RoomId != null)
                                    {
                                        if (Player.GetHabbo().GetRoomUser().RoomId == GameLobby)
                                        {
                                            if (Player.GetRoleplay() != null)
                                            {
                                                Player.GetRoleplay().WarpToPos(Player.GetHabbo().CurrentRoom.RoomData.Model.DoorX, Player.GetHabbo().CurrentRoom.RoomData.Model.DoorY, true);
                                            }
                                            else
                                                Player.GetMessageHandler().PrepareRoomForUser(MainLobby, "");
                                        }
                                        else
                                            Player.GetMessageHandler().PrepareRoomForUser(MainLobby, "");
                                    }
                                    else
                                        Player.GetMessageHandler().PrepareRoomForUser(MainLobby, "");
                                }
                                else Player.GetMessageHandler().PrepareRoomForUser(MainLobby, "");
                            }
                            else Player.GetMessageHandler().PrepareRoomForUser(MainLobby, "");
                            #endregion

                            if (Player != null && Player.GetMessageHandler() != null)
                            {
                                Player.GetMessageHandler().PrepareRoomForUser(MainLobby, "");
                            }



                            // Tell the player
                            Player.SendNotif(reason);
                        }

                        // If we decided to alert the members
                        if (AlertMembers)
                        {
                            if (Player.GetHabbo() != null)
                            {
                                // Tell everyone that someone left :[
                                MessageTeam(Player.GetHabbo().UserName + " has left the game!", TheTeam);
                            }
                        }

                        // Lets check if the team is eliminated
                        if (TheTeam.Players.Count == 0 && ColourManager.Started)
                        {
                            // Elimate the team
                            if (TheTeam != WinningTeam)
                            {
                                EliminateTeam(TheTeam);
                            }
                            else
                            {
                                EliminateTeam(TheTeam, true);
                            }
                        }
                    }
                    else
                    {
                        // Tell the user they can't quit this late
                        Player.SendWhisper("You cannot leave as the match has started!");
                    }
                }
            }
            catch (Exception ex) { Logging.LogCriticalException("Error in RemovePlayerFromTeam() void " + ex.Message + "::" + ex.StackTrace); }
        }

        /// <summary>
        /// Eliminates a team
        /// </summary>
        /// <param name="TheTeam">Team to get eliminated</param>
        public static void EliminateTeam(Team TheTeam, bool Won = false)
        {
            // Attempt the following
            try
            {
                // Set the team to knocked out
                if (!Won)
                {
                    TheTeam.KnockedOut = true;
                }


                // Initialize this int to 0
                int TeamsStillAlive = 0;

                // For every team
                lock (Teams.Values)
                {
                    foreach (Team Team in Teams.Values)
                    {
                        // If the team isn't knocked out
                        if (Team.KnockedOut == false)
                        {
                            // Add to the alive count
                            TeamsStillAlive += 1;
                        }
                    }
                }

                // If we only have 1 team alvie
                if (TeamsStillAlive == 1)
                {
                    // It's the winner
                    TeamWon();
                    return;
                }

                lock (TheTeam.Players.Keys)
                {
                    Dictionary<GameClient, bool> TeamCopy = new Dictionary<GameClient, bool>();

                    foreach (KeyValuePair<GameClient, bool> Player in TheTeam.Players)
                    {

                        TeamCopy.Add(Player.Key, Player.Value);
                    }

                    lock (TeamCopy.Keys)
                    {
                        // For every player in the team
                        foreach (GameClient Player in TeamCopy.Keys)
                        {
                            RemovePlayerFromTeam(Player, TheTeam, false, "Your team's base was captured!");
                        }
                    }
                }

                // Alert players about the elimination
                MessageAllPlayers("[COLOR WARS] " + TheTeam.Colour + " team has been eliminated!");
            }
            catch (Exception ex) { Logging.LogCriticalException("Error in EliminateTeam() void " + ex.Message + "::" + ex.StackTrace); }
        }

        /// <summary>
        /// Gets the winning team, alerts the hotel & ends the game
        /// </summary>
        public static void TeamWon()
        {
            // Attempt the following
            try
            {
                lock (Teams.Values)
                {
                    Dictionary<string, Team> TeamCopiedLol = new Dictionary<string, Team>();

                    foreach (KeyValuePair<string, Team> Teamm in Teams)
                    {
                        TeamCopiedLol.Add(Teamm.Key, Teamm.Value);
                    }
                    // For every team
                    foreach (Team Team in TeamCopiedLol.Values)
                    {
                        // If it isn't knocked out assume it's the winning team
                        if (Team.KnockedOut == false)
                        {
                            // New winning team
                            WinningTeam = Team;
                        }

                        // IF IT FOUND ANY TEAMS
                        if (WinningTeam != null)
                        {
                            // DING DING DING WE HAVE A WINNER
                            Dictionary<GameClient, bool> PlayersCopied = new Dictionary<GameClient, bool>();

                            foreach (KeyValuePair<GameClient, bool> Player in WinningTeam.Players)
                            {
                                PlayersCopied.Add(Player.Key, Player.Value);
                            }

                            // Now fucking loop through the cancerous player
                            foreach (GameClient Player in PlayersCopied.Keys)
                            {
                                // IS THE CUNT STILL BREATHIN?
                                if (Player != null && Player.GetRoleplay() != null)
                                {
                                    // Make the cunt player happy
                                    Player.GetHabbo().Credits += Prize;
                                    Player.GetHabbo().UpdateCreditsBalance();
                                    // GIVE SHITTY POINTS TO THE CUNT
                                    Player.GetRoleplay().GiveColorWarPoints(WinPoints);
                                    // REMOVE THE BASTARD
                                    RemovePlayerFromTeam(Player, WinningTeam, true, "You were paid " + Prize + " coins and received " + WinPoints + " for winning! Good job!");

                                    // REMOVE THE CUNT AGAIN
                                    Player.GetRoleplay().inColourWars = false;
                                    Player.GetRoleplay().ColourWarTeam = null;
                                }
                            }
                        }

                        if (Team != WinningTeam)
                        {
                            // LETS TELL THE PLAYER THAT THEY ARE SHIT AND THEY HAVE CANCER
                            Dictionary<GameClient, bool> PlayersCopied = new Dictionary<GameClient, bool>();

                            foreach (KeyValuePair<GameClient, bool> Player in Team.Players)
                            {
                                PlayersCopied.Add(Player.Key, Player.Value);
                            }

                            // FOR EVERY CANCEROUS PLAYER
                            foreach (GameClient Player in PlayersCopied.Keys)
                            {
                                // IF THE PLAYER DIDN'T QUIT LIKE A PUSSY
                                if (Player != null && Player.GetRoleplay() != null)
                                {
                                    // MAKE THE CUNT SAD LOL!

                                    // REMOVE THE CUNT
                                    RemovePlayerFromTeam(Player, Team, true, "You have lost a game of Color Wars! Try again next time.");

                                    // REMOVE THE CUNT AGAIN
                                    Player.GetRoleplay().inColourWars = false;
                                    Player.GetRoleplay().ColourWarTeam = null;
                                }
                            }
                        }
                    }
                }

                // If we found a winner
            }
            catch (Exception ex) { Logging.LogCriticalException("Error in TeamWon() void " + ex.Message + "::" + ex.StackTrace); }

            // End the game
            EndGame();
        }

        /// <summary>
        /// Changes the player's motto, game state and clothing
        /// </summary>
        /// <param name="Player">The player to edit</param>
        private static void CustomizePlayer(GameClient Player, Team TheTeam)
        {
            // Look before we changed the player :P
            Player.GetRoleplay().figBeforeWar = Player.GetHabbo().Look;

            // Now lets set the players new look TODO: New split figure
            Player.GetHabbo().Look = HabboHotel.Roleplay.Misc.RoleplayManager.SplitFigure(Player.GetHabbo().Look) + TheTeam.Figure;
            // Poof to refresh the look ;D
            Player.GetRoleplay().RefreshVals();
        }

        /// <summary>
        /// Checks if the teams are balanced
        /// </summary>
        /// <param name="TheTeam">The team the user wants to join</param>
        /// <returns>The team</returns>
        private static Team GetTeamBalanced(Team TheTeam, GameClient Player)
        {
            try
            {
                lock (Teams.Values)
                {
                    Dictionary<string, Team> TeamsCopy = new Dictionary<string, Team>();

                    foreach (KeyValuePair<string, Team> KvP in Teams)
                    {

                        TeamsCopy.Add(KvP.Key, KvP.Value);
                    }

                    // For every instance of team in teams
                    foreach (Team Team in TeamsCopy.Values)
                    {
                        // If another team has 2 players less than the team it isn't balanced
                        if (TheTeam.Players.Count == 6)
                        {
                            // Tell the user what just happened
                            Player.SendWhisper("You have been automatically switched to a different team to balance the teams!");
                            // Return the new team woohoo!
                            return Team;
                        }
                    }
                }

            }
            catch (Exception ex) { Logging.LogCriticalException("Error in GetTeamBalanced() void " + ex.Message + "::" + ex.StackTrace); }

            return TheTeam;
        }

        /// <summary>
        /// Gets all of the players within a team
        /// </summary>
        /// <returns></returns>
        private static int getPlayerCount()
        {
            try
            {
                // Integer count of all players
                int Players = 0;

                lock (Teams.Values)
                {
                    // For every team in our dictionary
                    foreach (Team TheTeam in Teams.Values)
                    {
                        // Add to the count
                        Players += TheTeam.Players.Count;
                    }
                }

                // Return the count
                return Players;
            }
            catch { return 0; }
        }

        /// <summary>
        /// Attempts to start our game
        /// </summary>
        public static void TryStart(bool bypass = false)
        {
            if (!Starting)
            {

                // Number of players in game
                int Players = getPlayerCount();

                // If there aren't enough players
                if (Players < 24 && !bypass)
                {
                    // Don't proceed
                    return;
                }

                // Set starting boolean to true
                Starting = true;

                // Create a countdown to start
                CountdownTimer = new Countdown();
            }
        }

        /// <summary>
        /// Sends a message to all of the players IG
        /// </summary>
        /// <param name="Message">Message to be displayed</param>
        public static void MessageAllPlayers(string Message)
        {
            try
            {
                lock (Teams.Values)
                {
                    // For every team in teams
                    foreach (Team Team in Teams.Values)
                    {
                        lock (Team.Players.Keys)
                        {
                            // For every player in team.players
                            foreach (GameClient User in Team.Players.Keys)
                            {
                                // If the player exists
                                if (User != null)
                                {
                                    // Send them the message
                                    User.SendWhisper(Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logging.LogCriticalException("Error in MessageAllPlayers() void " + ex.Message + "::" + ex.StackTrace); }
        }

        public static void MessageTeam(string Message, Team Team)
        {
            try
            {
                lock (Teams.Values)
                {
                    // For every team in teams
                    foreach (Team team in Teams.Values)
                    {
                        if (team != Team)
                            continue;

                        lock (Team.Players.Keys)
                        {
                            // For every player in team.players
                            foreach (GameClient User in Team.Players.Keys)
                            {
                                // If the player exists
                                if (User != null)
                                {
                                    // Send them the message
                                    User.SendWhisper(Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logging.LogCriticalException("Error in MessageTeam() void " + ex.Message + "::" + ex.StackTrace); }
        }

        /// <summary>
        /// Sends all of the players to the roomid
        /// </summary>
        /// <param name="RoomId">The room to send them to</param>
        public static void SendAllPlayersToRoom(uint RoomId, bool ToBase = false)
        {
            try
            {
                lock (Teams.Values)
                {
                    // For every team in teams
                    foreach (Team Team in Teams.Values)
                    {
                        lock (Team.Players.Keys)
                        {
                            // For every player in team.players
                            foreach (GameClient User in Team.Players.Keys)
                            {
                                // If the player exists
                                if (User != null && User.GetRoleplay() != null && User.GetMessageHandler() != null)
                                {
                                    if (ToBase)
                                    {
                                        // Send them to base
                                        User.GetMessageHandler().PrepareRoomForUser(User.GetRoleplay().ColourWarTeam.BaseRID, "");
                                    }
                                    else
                                    {
                                        // Send them to the room
                                        User.GetMessageHandler().PrepareRoomForUser(RoomId, "");
                                    }
                                    User.GetRoleplay().CurHealth = 120;
                                    User.GetRoleplay().Energy = 100;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logging.LogCriticalException("Error in SendAllPlayersToRoom() void " + ex.Message + "::" + ex.StackTrace); }
        }

        public static void ResetAllPlayers()
        {
            try
            {
                lock (Teams.Values)
                {
                    // For every team in teams
                    foreach (Team Team in Teams.Values)
                    {
                        lock (Team.Players.Keys)
                        {
                            // For every player in team.players
                            foreach (GameClient User in Team.Players.Keys)
                            {
                                // If the player exists
                                if (User != null && User.GetRoleplay() != null && User.GetMessageHandler() != null)
                                {
                                    User.GetRoleplay().CurHealth = User.GetRoleplay().MaxHealth;
                                    User.GetRoleplay().Energy = 100;
                                    // Now lets change the players look back to the old one!
                                    User.GetHabbo().Look = User.GetRoleplay().figBeforeWar;

                                    // Poof to refresh the look ;D
                                    User.GetRoleplay().RefreshVals();

                                    // Set the players set & game state
                                    User.GetRoleplay().ColourWarTeam = null;
                                    User.GetRoleplay().inColourWars = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logging.LogCriticalException("Error in ResetPlayers() void " + ex.Message + "::" + ex.StackTrace); }
        }

        /// <summary>
        /// This will transport all of the users to the starting point
        /// </summary>
        public static void Start()
        {
            // Set started boolean to true
            Started = true;
            // Set starting boolean to false
            Starting = false;
            // Send user to graveyard
            SendAllPlayersToRoom(GraveYardID, true);
        }

        /// <summary>
        /// Ends the game when it is finished
        /// </summary>
        public static void EndGame()
        {

            // Send all players that are still in the game to the loby
            ResetAllPlayers();
            SendAllPlayersToRoom(ColourManager.GameLobby);

            // Null all teams
            Teams["red"] = null;
            Teams["blue"] = null;
            Teams["yellow"] = null;
            Teams["pink"] = null;

            // NU LTHE WINNING TEAM YING U FORGOT THIS U BTEC CUNT
            WinningTeam = null;

            // Remove them from dictionary
            Teams.Clear();

            // Recreate teams etc
            Initialize();

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE rp_stats SET last_cw_team = '" + String.Empty + "' WHERE last_cw_team != '" +  String.Empty + "'");
            }
        }

        /// <summary>
        /// Gets the team by the base position
        /// </summary>
        /// <param name="RoomUser"></param>
        /// <returns></returns>
        public static Team GetTeamByBase(RoomUser RoomUser)
        {
            // Attempt to the do the following
            try
            {
                lock (Teams.Values)
                {
                    // For every team
                    foreach (Team Team in Teams.Values)
                    {
                        if (Team == null)
                            continue;

                        if (RoomUser == null)
                            continue;

                        // If the user is on a team's base return the team
                        if (Team.BaseRID == RoomUser.RoomId && Team.BAX == RoomUser.X && Team.BAY == RoomUser.Y)
                        {
                            return Team;
                        }
                    }
                }
                // Else the user is not on a team base
                return null;
            }
            catch (Exception ex) { Logging.LogCriticalException("Error in GetTeamByBase() void " + ex.Message + "::" + ex.StackTrace); return null; }
        }
        #endregion
    }
}
