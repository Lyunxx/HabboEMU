using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.PathFinding;
using Plus.Database;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.RoomBots;
// game.cs
// roomusermanager.cs
// Roleplayinstance.cs
// RoomUser.cs
// aitype.cs
// botai.cs
// botmanager.cs
// dunno wat else

namespace Plus.HabboHotel.Roleplay.Minigames.Mafia_Wars
{
    public class MafiaWarManager : IGame
    {
        /// <summary>
        /// Contains MWTeams
        /// </summary>
        public Dictionary<string, MWTeam> MWTeams = new Dictionary<string, MWTeam>();

        /// <summary>
        /// Loop for the lobby
        /// </summary>
        public MWLoop LobbyLoop;

        /// <summary>
        /// Dictionary containing bots
        /// </summary>
        internal Dictionary<RoomBot, MWTeam> MWBots = new Dictionary<RoomBot, MWTeam>();

        /// <summary>
        /// Has the game started
        /// </summary>
        public bool Started = false;

        /// <summary>
        /// Array containing data
        /// </summary>
        public string[] DataArray;

        /// <summary>
        /// Id of the graveyard
        /// </summary>
        public uint GraveYardID = 149;

        /// <summary>
        /// Prize for winning the match
        /// </summary>
        public int Prize = 1000;

        /// <summary>
        /// Points the users get for killing a player
        /// </summary>
        public static int KillPoints = Convert.ToInt32(RoleplayData.Data["mw.kill.pts"]);

        /// <summary>
        /// Points the users get for killing a player
        /// </summary>
        public static int ThugKillPoints = Convert.ToInt32(RoleplayData.Data["mw.thug.kill.pts"]);

        /// <summary>
        /// Points the users get for killing a player
        /// </summary>
        public static int WinPoints = Convert.ToInt32(RoleplayData.Data["mw.win.pts"]);

        /// <summary>
        /// Points the users get for killing a player
        /// </summary>
        public static int BossKillPoints = Convert.ToInt32(RoleplayData.Data["mw.boss.kill.pts"]);

        /// <summary>
        /// Constructor
        /// </summary>
        public MafiaWarManager()
        {
            string GameData;

            using (var dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM game_settings WHERE type = 'mafiawars'");
                DataTable GameTable = dbClient.GetTable();

                GameData = GameTable.Rows[0]["data"].ToString();
            }

            if (GameData == null)
            {
                Console.WriteLine("FAILED TO INITIALIZE MAFIA WARS");
                return;
            }

            DataArray = GameData.Split(',');

            // Split string by , and make a new vector instance | X,Y,X,Y
            Vector2D WaitingAreaGreen = new Vector2D(Convert.ToInt32(DataArray[0]), Convert.ToInt32(DataArray[1]));
            Vector2D WaitingAreaBlue = new Vector2D(Convert.ToInt32(DataArray[2]), Convert.ToInt32(DataArray[3]));

            MWTeam Green = new MWTeam("Green", "ha-3352-101-101.sh-290-101.lg-275-101.ch-3030-101", WaitingAreaGreen);
            MWTeam Blue = new MWTeam("Blue", "ha-3352-106-106.lg-275-106.sh-290-106.ch-3030-106", WaitingAreaBlue);

            MWTeams.Add("Green", Green);
            MWTeams.Add("Blue", Blue);
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        public void StartGame(bool Forced = false)
        {
            if (GetPlayerCount() < 20 && !Forced)
                 return;

            // Spawn bots
            LoadBots();

            Started = true;
            SendPlayersToRoom(GraveYardID);
        }

        /// <summary>
        /// Ends the game
        /// </summary>
        public void EndGame()
        {
            // Later me wants 2 test sumthin
            if (Started == false)
                return;

            Started = false;

            MWTeams["Green"].Players.Clear();
            MWTeams["Blue"].Players.Clear();

            foreach (RoomBot Bot in MWBots.Keys)
            {
                if (Bot != null && Bot.RoomUser != null && Bot.RoomUser.BotAI != null)
                {
                    Bot.RoomUser.BotAI._Victim = null;
                }
            }

            lock (Plus.GetGame().GetRoomManager().LoadedRooms.Values)
            {
                foreach (Room Room in Plus.GetGame().GetRoomManager().LoadedRooms.Values.ToList())
                {
                    if (Room == null)
                        continue;

                    if (Room.RoomData.Description.Contains("MAFIAWARS"))
                    {
                        Plus.GetGame().GetRoomManager().UnloadRoom(Room, "fixrooms");
                    }
                }
            }

            MWBots.Clear();
        }

        /// <summary>
        /// Gives the winning players a prize
        /// </summary>
        /// <param name="Team"></param>
        public void TeamWon(string Team)
        {
            MWTeam WinningTeam = MWTeams[Team];

            lock (MWTeams.Values)
            {
                foreach (MWTeam TheTeam in MWTeams.Values.ToList())
                {
                    if (TheTeam == null || TheTeam.Players == null)
                        continue;

                    lock (TheTeam.Players.Keys)
                    {
                        foreach (GameClient User in TheTeam.Players.Keys.ToList())
                        {
                            if (User == null || User.GetHabbo() == null || User.GetRoleplay() == null)
                                continue;

                            string LosingTeam = "none";

                            if (WinningTeam.TeamName.ToLower() == "green")
                            {
                                LosingTeam = "Blue";
                            }
                            else
                            {
                                LosingTeam = "Green";
                            }

                            User.SendNotif("The " + WinningTeam.TeamName.ToLower() + " Team won the match by kiling the " + LosingTeam + " Team's Boss, and received " + Prize + " coins!");
                           
                            if (User.GetRoleplay().TeamString == Team)
                            {
                                User.GetRoleplay().GiveMafiaWarPoints(WinPoints);
                                User.GetHabbo().Credits += Prize;
                                User.GetHabbo().UpdateCreditsBalance();
                            }
                         
                            RemoveUserFromGame(User, User.GetRoleplay().TeamString, false);
                            User.GetMessageHandler().PrepareRoomForUser(124, "");
                        }
                    }
                }
            }

            EndGame();
        }

        /// <summary>
        /// Adds user to a MWTeam
        /// </summary>
        public bool AddUserToGame(GameClient User, string TeamString)
        {
            if (User == null || User.GetHabbo() == null || User.GetRoleplay() == null)
                return false;
            if (User.GetRoleplay().InMafiaWars == true)
                return false;

            // Gets the MWTeam the user requested to join via the dictionary
            MWTeam MWTeamRequested = MWTeams[TeamString];
            if (MWTeamRequested == null)
                return false;
            if (MWTeamRequested.WaitingAreaPOS == null)
                return false;

            RoomUser RUser = User.GetHabbo().GetRoomUser();
            if (RUser == null)
                return false;

            User.GetRoleplay().TeamString = TeamString;
            User.GetRoleplay().InMafiaWars = true;
            MWTeamRequested.Players.Add(User, 0);

            User.GetRoleplay().figBeforeWar = User.GetHabbo().Look;
            User.GetHabbo().Look = RoleplayManager.SplitFigure(User.GetHabbo().Look) + MWTeamRequested.Look;
            User.GetRoleplay().RefreshVals();

            RUser.SetPos(MWTeamRequested.WaitingAreaPOS.X, MWTeamRequested.WaitingAreaPOS.Y, 0);
            RUser.ClearMovement();

            // Attempt to start the game
            StartGame();
            return true;
        }

        /// <summary>
        /// Removes the user from game
        /// </summary>
        public bool RemoveUserFromGame(GameClient User, string TeamString, bool Alert = true, bool disconnect = false)
        {
            if (User == null || User.GetHabbo() == null || User.GetRoleplay() == null)
            {
                MWTeams[TeamString].Players.Remove(User);
                return false;
            }

            MWTeam UserTeam = MWTeams[TeamString];
            if (UserTeam == null)
                return false;

            UserTeam.Players.Remove(User);
            User.GetRoleplay().TeamString = String.Empty;
            User.GetRoleplay().InMafiaWars = false;
            User.GetRoleplay().LastHitBot = null;

            User.GetHabbo().Look = User.GetRoleplay().figBeforeWar;

            if (!disconnect)
            {
                User.GetRoleplay().RefreshVals();

                User.GetMessageHandler().PrepareRoomForUser(124, "");
            }

            if (Alert)
                WhisperToTeam(User.GetHabbo().UserName + " has left the game!", UserTeam);

            if (GetPlayerCount() <= 0)
            {
                TeamWon("Blue");
            }
            return true;
        }

        #region Non interface methods
        /// <summary>
        /// Gets the total players in the game
        /// </summary>
        /// <returns></returns>
        public int GetPlayerCount()
        {
            int TotalPlayers = 0;

            lock (MWTeams.Values)
            {
                foreach (MWTeam MWTeam in MWTeams.Values)
                {
                    if (MWTeam == null)
                        continue;

                    TotalPlayers += MWTeam.Players.Count;
                }
            }
            return TotalPlayers;
        }

        /// <summary>
        /// Sends all players to room
        /// </summary>
        public void SendPlayersToRoom(uint RoomID)
        {
            lock (MWTeams.Values)
            {
                foreach (MWTeam Team in MWTeams.Values)
                {
                    lock (Team.Players.Keys)
                    {
                        foreach (GameClient User in Team.Players.Keys)
                        {
                            User.GetMessageHandler().PrepareRoomForUser(RoomID, "");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends a whisper to all players
        /// </summary>
        public void WhisperAllPlayers(string Message)
        {
            lock (MWTeams.Values)
            {
                foreach (MWTeam Team in MWTeams.Values)
                {
                    lock (Team.Players.Keys)
                    {
                        foreach (GameClient User in Team.Players.Keys)
                        {
                            User.SendWhisper("[MAFIA WARS] " + Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message to the team members
        /// </summary>
        public void WhisperToTeam(string Message, MWTeam Team)
        {
            lock (Team.Players.Keys)
            {
                foreach (GameClient User in Team.Players.Keys)
                {
                    User.SendWhisper("[MAFIA WARS] " + Message);
                }
            }
        }

        /// <summary>
        /// Loads the mafia war bots
        /// </summary>
        /// <returns></returns>
        public bool LoadBots()
        {
            DataTable CurrentBots;
            DataTable MafiaBots;

            using (var QueryAdapter = Plus.GetDatabaseManager().GetQueryReactor())
            {
                QueryAdapter.SetQuery("SELECT * FROM bots");
                CurrentBots = QueryAdapter.GetTable();

                QueryAdapter.SetQuery("SELECT * FROM rp_mafiawars_bots");
                MafiaBots = QueryAdapter.GetTable();
            }

            if (CurrentBots == null || MafiaBots == null)
                return false;

            uint CurrentID = Convert.ToUInt32(CurrentBots.Rows.Count + 1);

            foreach (DataRow Row in MafiaBots.Rows)
            {
                RoomBot MafiaBot = new RoomBot(CurrentID, 1, 478, AIType.MafiaWarBot,
                                        "stand", Row["name"].ToString(), Row["motto"].ToString(), Row["look"].ToString(),
                                        Convert.ToInt32(Row["x"]), Convert.ToInt32(Row["y"]), 0, 1, 4, 4, 4, 4, null, null, "M", 0, false);

                Room BotRoomBase = Plus.GetGame().GetRoomManager().LoadRoom(Convert.ToUInt32(Row["rid"]));

                if (MafiaBot == null || BotRoomBase == null)
                    continue;

                BotRoomBase.GetRoomUserManager().DeployBot(MafiaBot, null);

                MafiaBot.RoomUser.BotData._Team = MWTeams[Row["team"].ToString()];
                MafiaBot.RoomUser.BotData._Boss = Convert.ToBoolean(Row["boss"]);
                MafiaBot.RoomUser.BotData.cur_Health = Convert.ToInt32(Row["health"]);
                MafiaBot.RoomUser.BotData.max_Health = Convert.ToInt32(Row["maxhealth"]);
                MafiaBot.RoomUser.BotData.strength = Convert.ToInt32(Row["strength"]);

                MWBots.Add(MafiaBot, MafiaBot.RoomUser.BotData._Team);
                CurrentID++;
            }
            return true;
        }
        #endregion
    }
}