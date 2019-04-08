using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;

namespace Plus.HabboHotel.Guides
{
    /// <summary>
    /// Class GuideManager.
    /// </summary>
    internal class GuideManager
    {
        /// <summary>
        /// The en cours
        /// </summary>
        public Dictionary<uint, GameClient> EnCours = new Dictionary<uint, GameClient>();

        //internal int HelpersCount = 0;
        //internal int GuardiansCount = 0;
        /// <summary>
        /// The guides on duty
        /// </summary>
        internal List<GameClient> GuidesOnDuty = new List<GameClient>();
        internal List<GameClient> OfficersOnDuty = new List<GameClient>();
        internal List<GameClient> ChiefsOnDuty = new List<GameClient>();
        /// <summary>
        /// Gets or sets the guides count.
        /// </summary>
        /// <value>The guides count.</value>
        public int GuidesCount
        {
            get
            {
                return this.GuidesOnDuty.Count;
            }
            set
            {
            }
        }
        public int OfficerCount
        {
            get
            {
                return this.OfficersOnDuty.Count;
            }
            set
            {
            }
        }
        public int ChiefCount
        {
            get
            {
                return this.ChiefsOnDuty.Count;
            }
            set
            {
            }
        }
        /*public int Helpers
        {
        get { return HelpersCount; }
        set { HelpersCount = value; }
        }
        public int Guardians
        {
        get { return GuardiansCount; }
        set { GuardiansCount = value; }
        }*/

        /// <summary>
        /// Gets the random guide.
        /// </summary>
        /// <returns>GameClient.</returns>
        public GameClient GetRandomGuide()
        {
            var random = new Random();
            int intRand = random.Next(1, 6);
            if (intRand < 3){
                return this.GuidesOnDuty[random.Next(0, this.GuidesCount - 1)];
            }else if (intRand < 5){
                return this.OfficersOnDuty[random.Next(0, this.GuidesCount - 1)];
            }else{
                return this.ChiefsOnDuty[random.Next(0, this.GuidesCount - 1)];
            }
        }

        /// <summary>
        /// Adds the guide.
        /// </summary>
        /// <param name="guide">The guide.</param>
        public void AddGuide(GameClient guide)
        {
            if (!this.GuidesOnDuty.Contains(guide))
            {
                this.GuidesOnDuty.Add(guide);
            }
        }
        public void AddOfficer(GameClient guide)
        {
            if (!this.OfficersOnDuty.Contains(guide))
            {
                this.OfficersOnDuty.Add(guide);
            }
        }
        public void AddChief(GameClient guide)
        {
            if (!this.ChiefsOnDuty.Contains(guide))
            {
                this.ChiefsOnDuty.Add(guide);
            }
        }
        /// <summary>
        /// Removes the guide.
        /// </summary>
        /// <param name="guide">The guide.</param>
        public void RemoveGuide(GameClient guide)
        {
            if (this.GuidesOnDuty.Contains(guide))
            {
                this.GuidesOnDuty.Remove(guide);
            }
        }

        public void RemoveOfficer(GameClient guide)
        {
            if (this.OfficersOnDuty.Contains(guide))
            {
                this.OfficersOnDuty.Remove(guide);
            }
        }

        public void RemoveChief(GameClient guide)
        {
            if (this.ChiefsOnDuty.Contains(guide))
            {
                this.ChiefsOnDuty.Remove(guide);
            }
        }
    }
}