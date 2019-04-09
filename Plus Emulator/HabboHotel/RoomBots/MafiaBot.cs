using System;
using System.Linq;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Roleplay.Minigames.Mafia_Wars;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.Roleplay.Misc;

namespace Plus.HabboHotel.RoomBots
{
    /// <summary>
    /// Class GenericBot.
    /// </summary>
    internal class MafiaBot : BotAI
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
        /// Times the attack
        /// </summary>
        private Timer _AttackTimer;

        /// <summary>
        /// Times the cooldown
        /// </summary>
        private Timer _CoolDownTimer;

        /// <summary>
        /// The victim
        /// </summary>
        public RoomUser _Victim;



        /// <summary>
        /// Is the bot cooling down
        /// </summary>
        public bool _CoolingDown = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericBot"/> class.
        /// </summary>
        /// <param name="roomBot">The room bot.</param>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="botId">The bot identifier.</param>
        /// <param name="type">The type.</param>
        /// <param name="isBartender">if set to <c>true</c> [is bartender].</param>
        /// <param name="speechInterval">The speech interval.</param>
        internal MafiaBot(RoomBot roomBot, int virtualId, int botId, AIType type, bool isBartender, int speechInterval)
        {
            _id = botId;
            _virtualId = virtualId;
            _isBartender = isBartender;
            _speechInterval = speechInterval < 2 ? 2000 : speechInterval * 1000;

            // Get random speach
            if (roomBot != null && roomBot.AutomaticChat && roomBot.RandomSpeech != null && roomBot.RandomSpeech.Any()) _chatTimer = new Timer(ChatTimerTick, null, _speechInterval, _speechInterval);
            _actionCount = Random.Next(10, 30 + virtualId);

            if (roomBot != null)
                _AttackTimer = new Timer(TryAttack, null, 2500, 2500);
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
            if (_Victim == null)
            { LookForVictim();
            System.Drawing.Point Pos = GetRoom().GetGameMap().GetRandomValidWalkableSquare();
            GetRoomUser().MoveTo(Pos);
                return;}


            if (_Victim.Attacker == GetRoomUser())
            GetRoomUser().MoveTo(_Victim.SquareBehind);
            else
            {
                System.Drawing.Point Pos = GetRoom().GetGameMap().GetRandomValidWalkableSquare();
                GetRoomUser().MoveTo(Pos);
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
            if (_Victim == null)
            {
                GameClient Session = user.GetClient();
                if (user == null
                    || Session == null || Session.GetRoleplay() == null)
                    return;

                if (!GetBotData()._Team.Players.ContainsKey(Session) && Session.GetRoleplay().InMafiaWars && user.Attacker == null)
                {
                    _Victim = user;
                    _Victim.Attacker = GetRoomUser();
                }
            }
        }

        /// <summary>
        /// Called when [user leave room].
        /// </summary>
        /// <param name="client">The client.</param>
        internal override void OnUserLeaveRoom(GameClient client)
        {
            if (_Victim == null)
            {
                LookForVictim();
                return;
            }

            _Victim.Attacker = null;
            _Victim = null;
            LookForVictim();
        }

        /// <summary>
        /// Called when [user say].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        internal override void OnUserSay(RoomUser user, string message)
        {

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
                    .Chat(null, "Eu não confio em você gritando. Se você quer que eu sirva algo, diga bem.", false, 0, 0);
            }
        }

        /// <summary>
        /// Attacks the victim
        /// </summary>
        internal void TryAttack(object StateInfo)
        {
            try
            {
                if (GetRoomUser() == null || GetRoom() == null
                                || _AttackTimer == null || _Victim == null
                                || _Victim.RoomId != GetRoom().RoomId)
                { _Victim = null; return; }

                if (GetBotData().CoolDown != null)
                    return;

                GameClient Session = _Victim.GetClient();
                if (Session == null || Session.GetHabbo() == null || Session.GetRoleplay() == null || Session.GetHabbo().CurrentRoom == null)
                { _AttackTimer.Change(2000, 2000); _Victim = null; return; }

                Vector2D VictimPOS = new Vector2D(_Victim.X, _Victim.Y);
                Vector2D MyPOS = new Vector2D(GetRoomUser().X, GetRoomUser().Y);

                if (RoleplayManager.BotWithinAttackDistance(GetRoomUser(), _Victim))
                {
                    int ExtraDamage = (new Random()).Next(5, 15);
                    int Damage = GetBotData().strength + ExtraDamage;

                    // Kill this punk
                    if (Session.GetRoleplay().CurHealth - Damage <= 0)
                    {
                        GetRoomUser().Chat(null, "*Da um soco em " + _Victim.GetUserName() + ", nocauteando-o*", true, 1);
                        Session.GetRoleplay().CurHealth = 120;
                        Session.GetRoleplay().Energy = 120;
                        Session.GetRoleplay().Hunger = 0;
                        Session.GetRoleplay().UpdateStats++;

                        // Tell this nigga he got fucked up by our bot
                        Session.SendNotif("Você foi nocauteado pelo BOT-" + GetBotData().Name);
                        _Victim = null;
                        // SEND THIS NIGGA BACK TO HIS SHIT HOLE
                        Session.GetMessageHandler().PrepareRoomForUser(Plus.GetGame().MafiaWars.GraveYardID, "");

                        // Get next victim init
                        LookForVictim();
                        _AttackTimer.Change(1000, 1000);
                        return;
                    }

                    GetRoomUser().Chat(null, "*Bate em " + _Victim.GetUserName() + ", causando " + Damage + " dano*", true, 1);
                    RoleplayManager.Shout(Session, "*[" + Session.GetRoleplay().CurHealth + "/" + Session.GetRoleplay().MaxHealth + "]*");

                    Session.GetRoleplay().CurHealth -= Damage;
                    Session.GetRoleplay().UpdateStats++;

                    GetBotData().ApplyCooldown();
                    _AttackTimer.Change(1000, 1000);
                    return;
                }

                if (RoleplayManager.Distance(MyPOS, VictimPOS) >= 1 && RoleplayManager.Distance(MyPOS, VictimPOS) <= 2)
                {
                    // Miss
                    GetRoomUser().Chat(null, "*Tenta bater em " + _Victim.GetUserName() + " mas erra*", true, 1);

                    GetBotData().ApplyCooldown(2000);
                    _AttackTimer.Change(1000, 1000);
                    return;
                }

                if (RoleplayManager.Distance(MyPOS, VictimPOS) >= 5)
                {
                    _AttackTimer.Change(1000, 1000);
                    return; // Don't even bother
                }
            }
            catch { }
        }

        private void LookForVictim()
        {
            try
            {
                Room MyRoom = GetRoom();
                if (MyRoom == null)
                    return;

                lock (MyRoom.GetRoomUserManager().UserList.Values)
                {
                    foreach (RoomUser RUser in MyRoom.GetRoomUserManager().UserList.Values.ToList())
                    {
                        if (RUser == null || RUser.GetClient() == null || RUser.GetClient().GetRoleplay() == null)
                            continue;
                        if (!GetBotData()._Team.Players.ContainsKey(RUser.GetClient()) && RUser.GetClient().GetRoleplay().InMafiaWars && RUser.Attacker == null)
                        {
                            _Victim = RUser;
                            _Victim.Attacker = GetRoomUser();
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Disposes of all timers
        /// </summary>
        private void EndTimers()
        {
            if (_AttackTimer == null)
                return;

            if (GetRoomUser() == null || GetBotData() == null)
            {
                _AttackTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _AttackTimer.Dispose();
                _AttackTimer = null;
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