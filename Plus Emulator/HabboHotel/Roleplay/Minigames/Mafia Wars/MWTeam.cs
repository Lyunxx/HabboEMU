using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.PathFinding;

namespace Plus.HabboHotel.Roleplay.Minigames.Mafia_Wars
{
    public class MWTeam
    {
        /// <summary>
        /// Name of the team
        /// </summary>
        public string TeamName;

        /// <summary>
        /// The look of the players
        /// </summary>
        public string Look;

        /// <summary>
        /// Waiting area
        /// </summary>
        public Vector2D WaitingAreaPOS;

        /// <summary>
        /// Dictionary of players
        /// </summary>
        public Dictionary<GameClient, int> Players = new Dictionary<GameClient, int>();

        /// <summary>
        /// Constructor
        /// </summary>
        public MWTeam(string TeamName, string Look, Vector2D WaitingArea)
        {
            this.TeamName = TeamName;
            this.Look = Look;
            this.WaitingAreaPOS = WaitingArea;
        }
    }
}
