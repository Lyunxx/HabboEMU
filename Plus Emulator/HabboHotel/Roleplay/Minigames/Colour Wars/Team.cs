using System;
using System.Collections.Generic;


using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Roleplay.Minigames.Colour_Wars
{
    /// <summary>
    /// A class representing a team
    /// </summary>
    public class Team
    {
        #region Declares
        /// <summary>
        /// ID of the team
        /// </summary>
        public int ID;

        /// <summary>
        /// Represents the colour of the team
        /// </summary>
        public string Colour;

        /// <summary>
        /// Dictionary containg all players
        /// Key: Player ID | Value: Session
        /// </summary>
        public Dictionary<GameClient, bool> Players;

        /// <summary>
        /// The figure which the users wear when they are on the team
        /// </summary>
        public string Figure;

        /// <summary>
        /// Represents if the team is still in the round
        /// </summary>
        public bool KnockedOut;

        /// <summary>
        /// The roomid of the teams base
        /// </summary>
        public uint BaseRID;

        /// <summary>
        /// Position of the base group waiting area
        /// </summary>
        public int WAX, WAY, WAZ;

        /// <summary>
        /// Position of the base capture point
        /// </summary>
        public int BAX, BAY;

        /// <summary>
        /// Base name of spawn furniture for graveyard
        /// </summary>
        public string SpawnMat { get; set; }

        /// <summary>
        /// Checks if the base is being captured
        /// </summary>
        public bool BeingCaptured { get; set; }
        #endregion

        #region Consturctor
        /// <summary>
        /// Initializes all of the variables above
        /// </summary>
        public Team(string Colour, int ID, string Figure, uint BaseRID, int WAX, int WAY, int WAZ, int BAX, int BAY)
        {
            // Initialize variables
            this.ID = ID;
            this.Colour = Colour;
            this.Figure = Figure;
            this.KnockedOut = false;
            this.BeingCaptured = false;
            this.BaseRID = BaseRID;

            // Set position for waiting area spawn
            this.WAX = WAX;
            this.WAY = WAY;
            this.WAZ = WAZ;

            // Set position for base capture point
            this.BAX = BAX;
            this.BAY = BAY;

            // Initialize the players dictionary
            Players = new Dictionary<GameClient, bool>();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Adds a player to the team
        /// </summary>
        /// <param name="Player">The player to add</param>
        public void AddPlayerToTeam(GameClient Player)
        {
            // Check if the player exists already
            if (Players.ContainsKey(Player))
            {
                return;
            }
            else
            {
                // Add the player
                Players.Add(Player, true);
            }
        }
        #endregion
    }
}
