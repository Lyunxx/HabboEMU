using Plus.HabboHotel.Rooms;
using System.Collections.Generic;
using System.Linq;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using System;
using System.Data;

namespace Plus.HabboHotel.RoomBots
{
    /// <summary>
    /// Class RoomBot.
    /// </summary>
    internal class RoomBot
    {
        /// <summary>
        /// The bot identifier
        /// </summary>
        internal uint BotId;

        /// <summary>
        /// The room identifier
        /// </summary>
        internal uint RoomId;

        /// <summary>
        /// The virtual identifier
        /// </summary>
        internal int VirtualId;

        /// <summary>
        /// The owner identifier
        /// </summary>
        internal uint OwnerId;

        /// <summary>
        /// The ai type
        /// </summary>
        internal AIType AiType;

        /// <summary>
        /// The walking mode
        /// </summary>
        internal string WalkingMode;

        /// <summary>
        /// The name
        /// </summary>
        internal string Name;

        /// <summary>
        /// The motto
        /// </summary>
        internal string Motto;

        /// <summary>
        /// The look
        /// </summary>
        internal string Look;

        /// <summary>
        /// The gender
        /// </summary>
        internal string Gender;

        /// <summary>
        /// The x
        /// </summary>
        internal int X;

        /// <summary>
        /// The y
        /// </summary>
        internal int Y;

        /// <summary>
        /// The z
        /// </summary>
        internal double Z;

        /// <summary>
        /// The rot
        /// </summary>
        internal int Rot;

        /// <summary>
        /// The minimum x
        /// </summary>
        internal int MinX;

        /// <summary>
        /// The maximum x
        /// </summary>
        internal int MaxX;

        /// <summary>
        /// The minimum y
        /// </summary>
        internal int MinY;

        /// <summary>
        /// The maximum y
        /// </summary>
        internal int MaxY;

        /// <summary>
        /// The dance identifier
        /// </summary>
        internal int DanceId;

        /// <summary>
        /// The room user
        /// </summary>
        internal RoomUser RoomUser;

        /// <summary>
        /// The last spoken phrase
        /// </summary>
        internal int LastSpokenPhrase;

        /// <summary>
        /// The was picked
        /// </summary>
        internal bool WasPicked;

        /// <summary>
        /// The is bartender
        /// </summary>
        internal bool IsBartender;

        /// <summary>
        /// The random speech
        /// </summary>
        internal List<string> RandomSpeech;

        /// <summary>
        /// The responses
        /// </summary>
        internal List<string> Responses;

        /// <summary>
        /// The speech interval
        /// </summary>
        internal int SpeechInterval;

        /// <summary>
        /// The automatic chat
        /// </summary>
        internal bool AutomaticChat, MixPhrases;

        /// <summary>
        /// The minimum x
        /// </summary>
        private readonly int minX;

        public System.Threading.Timer CoolDown;

        /// <summary>
        /// The minimum y
        /// </summary>
        private readonly int minY;

        /// <summary>
        /// The maximum x
        /// </summary>
        private readonly int maxX;

        /// <summary>
        /// The maximum y
        /// </summary>
        private readonly int maxY;

        internal string HospBotType;
        internal RoomUser InteractingWith;
        internal int TimesTried;
        internal int chatnumber = 0;

        #region RP

        public int Str;
        public int MaxHealth;
        public int CurHealth;
        public string Type;
        public int FollowInterval;
        public int Cooldown;

        public bool Attacking = false;
        public int cur_Health = 100;
        public int max_Health = 100;
        public int strength;
        internal bool _Boss = false;
        internal Roleplay.Minigames.Mafia_Wars.MWTeam _Team;

        #endregion


        #region CoolDown
        /// <summary>
        /// Starts the CD
        /// </summary>
        internal void ApplyCooldown()
        {
            if (!IsPet)
            {
                if (CoolDown != null)
                { CoolDown.Change(3000, 3000); return; }

                CoolDown = new System.Threading.Timer(CooledDown, null, 3000, 3000);
            }
            else
            {
                if (CoolDown != null)
                { CoolDown.Change(this.Cooldown*1000, this.Cooldown*1000); return; }

                CoolDown = new System.Threading.Timer(CooledDown, null, this.Cooldown*1000, this.Cooldown*1000);
            }
        }

        public void SaveRPStats()
        {
            try
            {
                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery(
                        "UPDATE bots SET rp_str = '" +
                        this.Str + "', rp_maxhealth = '" +
                        this.MaxHealth + "', rp_type = '" +
                        this.Type + "', rp_follow_interval = '" +
                        this.FollowInterval + "', rp_cooldown = '" +
                        this.Cooldown + "'"
                        );
                }
            }
            catch(Exception e)
            {

            }
        }

        public void InitRPStats()
        {
            try
            {
                this.Str = Convert.ToInt32(this.GetRPStat("rp_str"));
                this.MaxHealth = Convert.ToInt32(this.GetRPStat("rp_maxhealth"));
                this.CurHealth = Convert.ToInt32(this.GetRPStat("rp_maxhealth"));
                this.Type = Convert.ToString(this.GetRPStat("rp_type"));
                this.FollowInterval = Convert.ToInt32(this.GetRPStat("rp_follow_interval"));
                this.Cooldown = Convert.ToInt32(this.GetRPStat("rp_cooldown"));

                this.strength = this.Str;
                this.cur_Health = this.CurHealth;
                this.max_Health = this.MaxHealth;
            }
            catch(Exception e)
            {
                this.Str = 5;
                this.MaxHealth = 100;
                this.CurHealth = 100;
                this.Type = "none";
                this.FollowInterval = 5;
                this.Cooldown = 3;

                this.strength = this.Str;
                this.cur_Health = this.CurHealth;
                this.max_Health = this.MaxHealth;
            }
        }

        internal object GetRPStat(string Stat)
        {
            
            object RetStat = 0;

            try
            {
                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT " + Stat + " FROM bots WHERE id = '" + this.BotId + "'");
                    DataRow Row = dbClient.GetRow();

                    if (Row == null)
                        return RetStat;

                    RetStat = Row[Stat];
                }
            }
            catch(Exception e)
            {
                return RetStat;
            }

            return RetStat;
        }

        /// <summary>
        /// An override void to ApplyDown, custom time
        /// </summary>
        internal void ApplyCooldown(int Time)
        {
            if (CoolDown != null)
            { CoolDown.Change(Time, Time); return; }

            CoolDown = new System.Threading.Timer(CooledDown, null, Time, Time);
        }

        /// <summary>
        /// Called when the timers elapses
        /// </summary>
        internal void CooledDown(object StateInfo)
        {
            CoolDown.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            CoolDown.Dispose();
            CoolDown = null;
        }
        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="RoomBot"/> class.
        /// </summary>
        /// <param name="botId">The bot identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="aiType">Type of the ai.</param>
        /// <param name="bartender">if set to <c>true</c> [bartender].</param>
        internal RoomBot(uint botId, uint ownerId, AIType aiType, bool bartender)
        {
            OwnerId = ownerId;
            BotId = botId;
            AiType = aiType;
            VirtualId = -1;
            RoomUser = null;
            LastSpokenPhrase = 1;
            IsBartender = bartender;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomBot"/> class.
        /// </summary>
        /// <param name="botId">The bot identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="aiType">Type of the ai.</param>
        /// <param name="walkingMode">The walking mode.</param>
        /// <param name="name">The name.</param>
        /// <param name="motto">The motto.</param>
        /// <param name="look">The look.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="rot">The rot.</param>
        /// <param name="minX">The minimum x.</param>
        /// <param name="minY">The minimum y.</param>
        /// <param name="maxX">The maximum x.</param>
        /// <param name="maxY">The maximum y.</param>
        /// <param name="speeches">The speeches.</param>
        /// <param name="responses">The responses.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="dance">The dance.</param>
        /// <param name="bartender">if set to <c>true</c> [bartender].</param>
        internal RoomBot(uint botId, uint ownerId, uint roomId, AIType aiType, string walkingMode, string name,
            string motto, string look, int x, int y, double z, int rot, int minX, int minY, int maxX, int maxY,
            List<string> speeches, List<string> responses, string gender, int dance, bool bartender)
        {
            OwnerId = ownerId;
            BotId = botId;
            RoomId = roomId;
            AiType = aiType;
            WalkingMode = walkingMode;
            Name = name;
            Motto = motto;
            Look = look;
            X = x;
            Y = y;
            Z = z;
            Rot = rot;
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
            Gender = gender.ToUpper();
            VirtualId = -1;
            RoomUser = null;
            DanceId = dance;
            RandomSpeech = speeches;
            Responses = responses;
            LastSpokenPhrase = 1;
            IsBartender = bartender;
            WasPicked = roomId == 0;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pet.
        /// </summary>
        /// <value><c>true</c> if this instance is pet; otherwise, <c>false</c>.</value>
        internal bool IsPet
        {
            get { return AiType == AIType.Pet || AiType == AIType.FightPet; }
        }

        /// <summary>
        /// Updates the specified room identifier.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="walkingMode">The walking mode.</param>
        /// <param name="name">The name.</param>
        /// <param name="motto">The motto.</param>
        /// <param name="look">The look.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="rot">The rot.</param>
        /// <param name="minX">The minimum x.</param>
        /// <param name="minY">The minimum y.</param>
        /// <param name="maxX">The maximum x.</param>
        /// <param name="maxY">The maximum y.</param>
        /// <param name="speeches">The speeches.</param>
        /// <param name="responses">The responses.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="dance">The dance.</param>
        /// <param name="speechInterval">The speech interval.</param>
        /// <param name="automaticChat">if set to <c>true</c> [automatic chat].</param>
        /// <param name="mixPhrases">if set to <c>true</c> [mix phrases].</param>
        internal void Update(uint roomId, string walkingMode, string name, string motto, string look, int x, int y,
            double z, int rot, int minX, int minY, int maxX, int maxY, List<string> speeches,
            List<string> responses, string gender, int dance, int speechInterval, bool automaticChat,
            bool mixPhrases)
        {
            RoomId = roomId;
            WalkingMode = walkingMode;
            Name = name;
            Motto = motto;
            Look = look;
            X = x;
            Y = y;
            Z = z;
            Rot = rot;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            Gender = gender.ToUpper();
            VirtualId = -1;
            RoomUser = null;
            DanceId = dance;
            RandomSpeech = speeches;
            Responses = responses;
            WasPicked = (roomId == 0);
            MixPhrases = mixPhrases;
            AutomaticChat = automaticChat;
            SpeechInterval = speechInterval;
        }

        /// <summary>
        /// Gets the random speech.
        /// </summary>
        /// <param name="mixPhrases">if set to <c>true</c> [mix phrases].</param>
        /// <returns>System.String.</returns>
        internal string GetRandomSpeech(bool mixPhrases)
        {
            if (!RandomSpeech.Any())
                return "";

            {
                if (mixPhrases)
                    return RandomSpeech[Plus.GetRandomNumber(0, RandomSpeech.Count - 1)];
                if (LastSpokenPhrase >= RandomSpeech.Count)
                    LastSpokenPhrase = 1;
                var result = RandomSpeech[LastSpokenPhrase - 1];
                LastSpokenPhrase++;
                return result;
            }
        }

        /// <summary>
        /// Generates the bot ai.
        /// </summary>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="botId">The bot identifier.</param>
        /// <returns>BotAI.</returns>
        internal BotAI GenerateBotAI(int virtualId, int botId)
        {
            switch (this.AiType)
            {
                case AIType.Pet:
                    return new PetBot(VirtualId);

                case AIType.FightPet:
                    return new FightPet(VirtualId);

                case AIType.HospitalBot:
                    return new HospitalBot(this, virtualId, botId, AiType, IsBartender, SpeechInterval);

                case AIType.MafiaWarBot:
                    return new MafiaBot(this, virtualId, botId, AiType, IsBartender, SpeechInterval);

                case AIType.MiscBot:
                    return new MiscBot(this, virtualId, botId, AiType, IsBartender, SpeechInterval);

                case AIType.WeedBot:
                    return new WeedBot(this, virtualId, botId, AiType, IsBartender, SpeechInterval);

                default:
                    return new GenericBot(this, virtualId, botId, AiType, IsBartender, SpeechInterval);
            }
        }
    }
}