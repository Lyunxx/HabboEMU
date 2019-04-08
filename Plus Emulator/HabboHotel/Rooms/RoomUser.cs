using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Misc;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.Pets;
using Plus.HabboHotel.RoomBots;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Rooms.Wired;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.Messages;
using Plus.Messages.Parsers;
using Plus.Security;
using Plus.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Plus.HabboHotel.Rooms
{
    /// <summary>
    /// Class RoomUser.
    /// </summary>
    public class RoomUser : IEquatable<RoomUser>
    {
        /// <summary>
        /// The habbo identifier
        /// </summary>
        internal uint HabboId;

        public RoomUser Attacker;

        /// <summary>
        /// The virtual identifier
        /// </summary>
        internal int VirtualId;

        /// <summary>
        /// My pet
        /// </summary>
        internal RoomUser MyPet;

        /// <summary>
        /// My pet
        /// </summary>
        internal RoomUser AttackPet;

        /// <summary>
        /// The room identifier
        /// </summary>
        internal uint RoomId;

        /// <summary>
        /// The user identifier
        /// </summary>
        internal uint UserId;

        /// <summary>
        /// The following owner
        /// </summary>
        internal RoomUser FollowingOwner;

        /// <summary>
        /// The interacting gate
        /// </summary>
        internal bool InteractingGate;

        /// <summary>
        /// The gate identifier
        /// </summary>
        internal uint GateId;

        /// <summary>
        /// The last interaction
        /// </summary>
        internal int LastInteraction;

        /// <summary>
        /// The locked tiles count
        /// </summary>
        internal int LockedTilesCount;

        /// <summary>
        /// The carry item identifier
        /// </summary>
        internal int CarryItemId;

        /// <summary>
        /// The carry timer
        /// </summary>
        internal int CarryTimer;

        /// <summary>
        /// The sign time
        /// </summary>
        internal int SignTime;

        /// <summary>
        /// The idle time
        /// </summary>
        internal int IdleTime;

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
        /// The sq state
        /// </summary>
        internal byte SqState;

        /// <summary>
        /// The rot head
        /// </summary>
        internal int RotHead;

        /// <summary>
        /// The rot body
        /// </summary>
        internal int RotBody;

        /// <summary>
        /// The can walk
        /// </summary>
        internal bool CanWalk;

        /// <summary>
        /// The allow override
        /// </summary>
        internal bool AllowOverride;

        /// <summary>
        /// The teleport enabled
        /// </summary>
        internal bool TeleportEnabled;

        /// <summary>
        /// The goal x
        /// </summary>
        internal int GoalX;

        /// <summary>
        /// The goal y
        /// </summary>
        internal int GoalY;

        internal int LastSelectedX;
        internal int LastSelectedY;

        /// <summary>
        /// The love lock partner
        /// </summary>
        internal uint LoveLockPartner;

        /// <summary>
        /// The path
        /// </summary>
        internal List<Vector2D> Path = new List<Vector2D>();

        /// <summary>
        /// The path recalc needed
        /// </summary>
        internal bool PathRecalcNeeded;

        /// <summary>
        /// The path step
        /// </summary>
        internal int PathStep = 1;

        /// <summary>
        /// The set step
        /// </summary>
        internal bool SetStep;

        /// <summary>
        /// The set x
        /// </summary>
        internal int SetX;

        /// <summary>
        /// The set y
        /// </summary>
        internal int SetY;

        /// <summary>
        /// The set z
        /// </summary>
        internal double SetZ;

        /// <summary>
        /// The bot data
        /// </summary>
        internal RoomBot BotData;

        /// <summary>
        /// The bot ai
        /// </summary>
        internal BotAI BotAI;

        /// <summary>
        /// The current item effect
        /// </summary>
        internal ItemEffectType CurrentItemEffect;

        /// <summary>
        /// The freezed
        /// </summary>
        internal bool Freezed; //En el freeze

        /// <summary>
        /// The frozen
        /// </summary>
        internal bool Frozen; //por comando
        internal bool Stunned { get { return Frozen; } set { Frozen = value; } }

        /// <summary>
        /// The freeze counter
        /// </summary>
        internal int FreezeCounter;

        /// <summary>
        /// The team
        /// </summary>
        internal Team Team;

        /// <summary>
        /// The banzai power up
        /// </summary>
        internal FreezePowerUp BanzaiPowerUp;

        /// <summary>
        /// The freeze lives
        /// </summary>
        internal int FreezeLives;

        /// <summary>
        /// The shield active
        /// </summary>
        internal bool ShieldActive;

        /// <summary>
        /// The shield counter
        /// </summary>
        internal int ShieldCounter;

        /// <summary>
        /// The throw ball at goal
        /// </summary>
        internal bool ThrowBallAtGoal;

        /// <summary>
        /// The is moonwalking
        /// </summary>
        internal bool IsMoonwalking;

        /// <summary>
        /// The is sitting
        /// </summary>
        internal bool IsSitting;

        /// <summary>
        /// The is lying down
        /// </summary>
        internal bool IsLyingDown;

        /// <summary>
        /// The has path blocked
        /// </summary>
        internal bool HasPathBlocked;

        /// <summary>
        /// The is flooded
        /// </summary>
        internal bool IsFlooded;
        internal bool IsGunReloaded;

        /// <summary>
        /// The flood expiry time
        /// </summary>
        internal int FloodExpiryTime;
        internal int ReloadExpiryTime;

        /// <summary>
        /// The riding horse
        /// </summary>
        internal bool RidingHorse;

        /// <summary>
        /// The horse identifier
        /// </summary>
        internal uint HorseId;

        /// <summary>
        /// The last item
        /// </summary>
        internal uint LastItem;

        internal bool NeedCheck = false;

        /// <summary>
        /// The on camping tent
        /// </summary>
        internal bool OnCampingTent;

        /// <summary>
        /// The fast walking
        /// </summary>
        internal bool FastWalking;

        /// <summary>
        /// The last bubble
        /// </summary>
        internal int LastBubble = 0;

        /// <summary>
        /// The pet data
        /// </summary>
        internal Pet PetData;

        /// <summary>
        /// The is walking
        /// </summary>
        internal bool IsWalking;

        /// <summary>
        /// The update needed
        /// </summary>
        internal bool UpdateNeeded;

        /// <summary>
        /// The is asleep
        /// </summary>
        internal bool IsAsleep;

        /// <summary>
        /// The statusses
        /// </summary>
        internal Dictionary<string, string> Statusses;

        /// <summary>
        /// The dance identifier
        /// </summary>
        internal int DanceId;

        /// <summary>
        /// The tele delay
        /// </summary>
        internal int TeleDelay;

        /// <summary>
        /// The is spectator
        /// </summary>
        internal bool IsSpectator;

        /// <summary>
        /// The internal room identifier
        /// </summary>
        internal int InternalRoomId;

        /// <summary>
        /// The _events
        /// </summary>
        private readonly Queue _events;

        /// <summary>
        /// The _flood count
        /// </summary>
        private int _floodCount;

        /// <summary>
        /// The _m client
        /// </summary>
        private GameClient _mClient;

        /// <summary>
        /// The _m room
        /// </summary>
        private Room _mRoom;

        /// <summary>
        /// The handeling ball status
        /// </summary>
        internal int HandelingBallStatus = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomUser"/> class.
        /// </summary>
        /// <param name="habboId">The habbo identifier.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="room">The room.</param>
        /// <param name="isSpectator">if set to <c>true</c> [is spectator].</param>
        internal RoomUser(uint habboId, uint roomId, int virtualId, Room room, bool isSpectator)
        {
            Freezed = false;
            HabboId = habboId;
            RoomId = roomId;
            VirtualId = virtualId;
            IdleTime = 0;
            X = 0;
            Y = 0;
            Z = 0.0;
            RotHead = 0;
            RotBody = 0;
            UpdateNeeded = true;
            Statusses = new Dictionary<string, string>();
            TeleDelay = -1;
            _mRoom = room;
            AllowOverride = false;
            CanWalk = true;
            IsSpectator = isSpectator;
            SqState = 3;
            InternalRoomId = 0;
            CurrentItemEffect = ItemEffectType.None;
            _events = new Queue();
            FreezeLives = 0;
            InteractingGate = false;
            GateId = 0u;
            LastInteraction = 0;
            LockedTilesCount = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomUser"/> class.
        /// </summary>
        /// <param name="habboId">The habbo identifier.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="pClient">The p client.</param>
        /// <param name="room">The room.</param>
        internal RoomUser(uint habboId, uint roomId, int virtualId, GameClient pClient, Room room)
        {
            _mClient = pClient;
            Freezed = false;
            HabboId = habboId;
            RoomId = roomId;
            VirtualId = virtualId;
            IdleTime = 0;
            X = 0;
            Y = 0;
            Z = 0.0;
            RotHead = 0;
            RotBody = 0;
            UpdateNeeded = true;
            Statusses = new Dictionary<string, string>();
            TeleDelay = -1;
            LastInteraction = 0;
            AllowOverride = false;
            CanWalk = true;
            IsSpectator = GetClient().GetHabbo().SpectatorMode;
            SqState = 3;
            InternalRoomId = 0;
            CurrentItemEffect = ItemEffectType.None;
            _mRoom = room;
            _events = new Queue();
            InteractingGate = false;
            GateId = 0u;
            LockedTilesCount = 0;
        }

        /// <summary>
        /// Gets the coordinate.
        /// </summary>
        /// <value>The coordinate.</value>
        internal Point Coordinate
        {
            get { return new Point(X, Y); }
        }

        /// <summary>
        /// Gets the square behind.
        /// </summary>
        /// <value>The square behind.</value>
        internal Point SquareBehind
        {
            get
            {
                var x = X;
                var y = Y;

                switch (RotBody)
                {
                    case 0:
                        y++;
                        break;

                    case 1:
                        x--;
                        y++;
                        break;

                    case 2:
                        x--;
                        break;

                    case 3:
                        x--;
                        y--;
                        break;

                    case 4:
                        y--;
                        break;

                    case 5:
                        x++;
                        y--;
                        break;

                    case 6:
                        x++;
                        break;

                    case 7:
                        x++;
                        y++;
                        break;
                }

                return new Point(x, y);
            }
        }

        /// <summary>
        /// Gets the square in front.
        /// </summary>
        /// <value>The square in front.</value>
        internal Point SquareInFront
        {
            get
            {
                {
                    var x = X + 1;
                    var y = 0;
                    switch (RotBody)
                    {
                        case 0:
                            x = X;
                            y = Y - 1;
                            break;

                        case 1:
                            x = X + 1;
                            y = Y - 1;
                            break;

                        case 2:
                            x = X + 1;
                            y = Y;
                            break;

                        case 3:
                            x = X + 1;
                            y = Y + 1;
                            break;

                        case 4:
                            x = X;
                            y = Y + 1;
                            break;

                        case 5:
                            x = X - 1;
                            y = Y + 1;
                            break;

                        case 6:
                            x = X - 1;
                            y = Y;
                            break;

                        case 7:
                            x = X - 1;
                            y = Y - 1;
                            break;
                    }
                    return new Point(x, y);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pet.
        /// </summary>
        /// <value><c>true</c> if this instance is pet; otherwise, <c>false</c>.</value>
        internal bool IsPet
        {
            get { return IsBot && BotData.IsPet; }
        }

        /// <summary>
        /// Gets the current effect.
        /// </summary>
        /// <value>The current effect.</value>
        internal int CurrentEffect
        {
            get
            {
                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return 0;

                return GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is dancing.
        /// </summary>
        /// <value><c>true</c> if this instance is dancing; otherwise, <c>false</c>.</value>
        internal bool IsDancing
        {
            get { return DanceId >= 1; }
        }

        /// <summary>
        /// Gets a value indicating whether [needs autokick].
        /// </summary>
        /// <value><c>true</c> if [needs autokick]; otherwise, <c>false</c>.</value>
        internal bool NeedsAutokick
        {
            get
            {
                return !IsBot &&
                       (GetClient() == null || GetClient().GetHabbo() == null ||
                        (GetClient().GetHabbo().Rank <= 6u && IdleTime >= 1800));
            }
        }

        /// <summary>
        /// MOVEEE YA CUNT
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal void Move(int x, int y, int ownerROT)
        {
            // hush lil baby dont u cry
            System.Threading.Thread.Sleep(450);
            try
            {
                // Make this nigga walk next 2 him
                int Width;
                int Height;

                byte[,] GMap = _mRoom.GetGameMap().GameMap;
                Width = _mRoom.GetGameMap().GameMap.GetLength(0);
                Height = _mRoom.GetGameMap().GameMap.GetLength(1);

                // check if thingy majingy is outside bounds
                if (x + 1 > Width || y + 1 > Height || x - 1 < 0 || y - 1 < 0)
                    return;
                NeedCheck = true;
                // Rot check
                if (ownerROT == 2 && GMap[x, y - 1] != 0)
                { y -= 1; MoveTo(x, y); return; }
                if (ownerROT == 6 && GMap[x, y + 1] != 0)
                { y += 1; MoveTo(x, y); return; }


                // Find a nice place for this nigga
                if (GMap[x + 1, y] != 0)
                { x += 1; MoveTo(x, y); return; }
                if (GMap[x - 1, y] != 0)
                { x -= 1; MoveTo(x, y); return; }

                // Y
                if (GMap[x, y + 1] != 0)
                { y += 1; MoveTo(x, y); return; }
                if (GMap[x, y - 1] != 0)
                { y -= 1; MoveTo(x, y); return; }

                // X & Y
                if (GMap[x - 1, y + 1] != 0)
                { y += 1; x -= 1; MoveTo(x, y); return; }
                if (GMap[x + 1, y - 1] != 0)
                { y -= 1; x += 1; MoveTo(x, y); return; }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DAMN U FUCKED UP");
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is trading.
        /// </summary>
        /// <value><c>true</c> if this instance is trading; otherwise, <c>false</c>.</value>
        internal bool IsTrading
        {
            get { return !IsBot && Statusses.ContainsKey("trd"); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is bot.
        /// </summary>
        /// <value><c>true</c> if this instance is bot; otherwise, <c>false</c>.</value>
        internal bool IsBot
        {
            get { return BotData != null; }
        }

        /// <summary>
        /// Equalses the specified compared user.
        /// </summary>
        /// <param name="comparedUser">The compared user.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Equals(RoomUser comparedUser)
        {
            return comparedUser.HabboId == HabboId;
        }

        /// <summary>
        /// Gets the speech emotion.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>System.Int32.</returns>
        internal static int GetSpeechEmotion(string message)
        {
            message = message.ToLower();
            if (message.Contains(":)") || message.Contains(":d") || message.Contains("=]") || message.Contains("=d") ||
                message.Contains(":>"))
                return 1;
            if (message.Contains(">:(") || message.Contains(":@")) return 2;
            if (message.Contains(":o")) return 3;
            if (message.Contains(":(") || message.Contains("=[") || message.Contains(":'(") || message.Contains("='[")) return 4;
            return 0;
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <returns>System.String.</returns>
        internal string GetUserName()
        {
            if (!IsBot)
                return GetClient() != null ? GetClient().GetHabbo().UserName : string.Empty;
            if (!IsPet)
                return BotData == null ? string.Empty : BotData.Name;
            return PetData.Name;
        }

        /// <summary>
        /// Determines whether this instance is owner.
        /// </summary>
        /// <returns><c>true</c> if this instance is owner; otherwise, <c>false</c>.</returns>
        internal bool IsOwner()
        {
            return !IsBot && GetUserName() == GetRoom().RoomData.Owner;
        }

        /// <summary>
        /// Uns the idle.
        /// </summary>
        internal void UnIdle()
        {
            IdleTime = 0;
            if (!IsAsleep)
                return;
            IsAsleep = false;
            var sleep = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserIdleMessageComposer"));
            sleep.AppendInteger(VirtualId);
            sleep.AppendBool(false);
            GetRoom().SendMessage(sleep);

            GetClient().Shout("*Wakes Up*");
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        internal void Dispose()
        {
            Statusses.Clear();
            _mRoom = null;
            _mClient = null;
        }

        /// <summary>
        /// Chats the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="shout">if set to <c>true</c> [shout].</param>
        /// <param name="count">The count.</param>
        /// <param name="textColor">Color of the text.</param>
        internal void Chat(GameClient session, string msg, bool shout, int count, int textColor = 0)
        {

            if (IsPet || IsBot)
            {
                if (!IsPet)
                    textColor = 2;

                var botChatmsg = new ServerMessage();
                botChatmsg.Init(shout
                    ? LibraryParser.OutgoingRequest("ShoutMessageComposer")
                    : LibraryParser.OutgoingRequest("ChatMessageComposer"));
                botChatmsg.AppendInteger(VirtualId);
                botChatmsg.AppendString(msg);
                botChatmsg.AppendInteger(0);
                botChatmsg.AppendInteger(textColor);
                botChatmsg.AppendInteger(0);
                botChatmsg.AppendInteger(count);

                GetRoom().SendMessage(botChatmsg);
                return;
            }

            if (msg.Length > 150) // help prevent scripters
                return;

            if (session == null || session.GetHabbo() == null)
                return;

            if (!session.GetHabbo().HasFuse("fuse_owner") && AntiPublicistas.CheckPublicistas(msg))
            {
                session.PublicistCount++;
                session.HandlePublicista(msg);
                return;
            }

            if (!IsBot && IsFlooded && FloodExpiryTime <= Plus.GetUnixTimeStamp())
                IsFlooded = false;
            else if (!IsBot && IsFlooded)
                return; // ciao flooders!

            if (!IsBot && IsGunReloaded && ReloadExpiryTime <= Plus.GetUnixTimeStamp())
            {
                IsGunReloaded = false;
            }
            else if (!IsBot && IsGunReloaded)
                return; // ciao flooders!

            if (session.GetHabbo().Rank < 4 && GetRoom().CheckMute(session))
                return;

            if (!msg.StartsWith("*")
                && !session.GetRoleplay().WorkingOut
                && !session.GetRoleplay().WeightLifting)
            {
                UnIdle();
            }
            if (!IsPet && !IsBot)
            {
                #region Emotions

                if (!session.GetRoleplay().MultiCoolDown.ContainsKey("emotion_chat"))
                {
                    session.GetRoleplay().MultiCoolDown.Add("emotion_chat", 0);
                }

                if (session.GetRoleplay().MultiCoolDown["emotion_chat"] <= 0)
                {

                    if (msg.Equals("*") || msg.Equals(" *") || msg.Equals(" * ") || msg.Equals("* "))
                    {
                        return;
                    }
                    if (msg.Equals(":S") || msg.Equals(";S") || msg.Equals(";-S") || msg.Equals(":-S"))
                    {
                        Chat(session, "*Confused*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":O") || msg.Equals(";O") || msg.Equals(";-O") || msg.Equals(":-O"))
                    {
                        Chat(session, "*Shocked*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":D") || msg.Equals(";D") || msg.Equals(":-D") || msg.Equals(";-D"))
                    {
                        Chat(session, "*Grins*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":)") || msg.Equals(":-)") || msg.Equals("=)") || msg.Equals("=]") || msg.Equals("(:") || msg.Equals("[=") || msg.Equals(":-]") || msg.Equals(":]"))
                    {
                        Chat(session, "*Smiles*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":L") || msg.Equals(":-L") || msg.Equals(";L") || msg.Equals(";-L"))
                    {
                        Chat(session, "*Awks*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":(") || msg.Equals(":-(") || msg.Equals("=[") || msg.Equals(";-(") || msg.Equals(":[") || msg.Equals(":-["))
                    {
                        Chat(session, "*Frowns*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":'(") || msg.Equals(":'-(") || msg.Equals(":-'("))
                    {
                        Chat(session, "*Cries*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(";)") || msg.Equals(";-)") || msg.Equals(";]") || msg.Equals(";-]"))
                    {
                        Chat(session, "*Winks*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(";@") || msg.Equals(":@") || msg.Equals(":-@") || msg.Equals(";-@"))
                    {
                        Chat(session, "*Angry*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }
                }
                else
                {
                    session.SendWhisper("Emotion cooldown [" + session.GetRoleplay().MultiCoolDown["emotion_chat"] + "/3]");
                    return;
                }

                #endregion

                if (session.GetHabbo().GetRoomUser().Frozen && session.GetRoleplay().MuteSeconds > 0)
                {
                    session.SendWhisper("The effect of the stun gun is preventing you from talking! Please wait " + session.GetRoleplay().MuteSeconds + " Seconds!");
                    return;
                }

                if (msg == "x")
                {
                    if (RoleplayManager.CanInteract(session, session.GetRoleplay().LastHit, true))
                    {
                        if (session.GetRoleplay().ActionLast == "hit")
                        {
                            ChatCommandHandler.Parse(session, ":hit " + session.GetRoleplay().LastHit.GetHabbo().UserName);
                        }
                        else if (session.GetRoleplay().ActionLast == "melee")
                        {
                            ChatCommandHandler.Parse(session, ":hit " + session.GetRoleplay().LastHit.GetHabbo().UserName);
                        }
                        else if (session.GetRoleplay().ActionLast == "shoot")
                        {
                            ChatCommandHandler.Parse(session, ":shoot " + session.GetRoleplay().LastHit.GetHabbo().UserName);
                        }
                        else if (session.GetRoleplay().ActionLast == "bomb")
                        {
                            ChatCommandHandler.Parse(session, ":bomb " + session.GetRoleplay().LastHit.GetHabbo().UserName);
                        }
                    }
                    else
                    {
                        session.SendWhisperBubble("The last user you attacked was not found in this room!");
                        return;
                    }
                    return;
                }

                if (session.GetRoleplay().InMiniGame && ChatCommandHandler.MiniGameCmds(session, msg))
                {
                    return;
                }
                else if (!session.GetRoleplay().InMiniGame)
                {
                    if (msg.StartsWith("#") && ChatCommandHandler.Parse2(session, msg))
                    {
                        GetRoom().AddChatlog(session.GetHabbo().Id, msg, true);
                        return;
                    }

                    //No need to log text messages
                    if (msg.StartsWith(":") && ChatCommandHandler.Parse(session, msg))
                    {
                        GetRoom().AddChatlog(session.GetHabbo().Id, msg, true);
                        return;
                    }
                }

                var habbo = GetClient().GetHabbo();

                if (GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerOnUserSay, this, msg))
                    return;

                GetRoom().AddChatlog(session.GetHabbo().Id, msg, true);

                uint rank = 1;

                if (session.GetHabbo() != null)
                    rank = session.GetHabbo().Rank;

                msg = GetRoom()
                    .WordFilter
                    .Aggregate(msg,
                        (current1, current) => Regex.Replace(current1, current, "bobba", RegexOptions.IgnoreCase));

                if (!msg.StartsWith("*"))
                {

                    if (rank < 4)
                    {
                        var span = DateTime.Now - habbo.SpamFloodTime;
                        if ((span.TotalSeconds > habbo.SpamProtectionTime) && habbo.SpamProtectionBol)
                        {
                            _floodCount = 0;
                            habbo.SpamProtectionBol = false;
                            habbo.SpamProtectionAbuse = 0;
                        }
                        else if (span.TotalSeconds > 4.0)
                        {
                            _floodCount = 0;
                        }
                        ServerMessage message;
                        if (((span.TotalSeconds < 4.0) && (_floodCount > 5)) && (rank < 5))
                        {
                            message = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
                            message.AppendInteger(10);
                            IsFlooded = true;
                            FloodExpiryTime = Plus.GetUnixTimeStamp() + 10;
                            GetClient().SendMessage(message);
                            return;
                        }
                        habbo.SpamFloodTime = DateTime.Now;
                        _floodCount++;
                    }
                }
                if (session.GetRoleplay().StaffDuty && session.GetHabbo().Rank > 3 && textColor == 0)
                {
                    textColor = 33;

                }
                else if (session.GetRoleplay().StaffDuty && session.GetHabbo().Rank == 3 && textColor == 0)
                {
                    textColor = 6;
                }
            }
            else if (!IsPet)
                textColor = 2;

            var chatMsg = new ServerMessage();
            chatMsg.Init(shout
                ? LibraryParser.OutgoingRequest("ShoutMessageComposer")
                : LibraryParser.OutgoingRequest("ChatMessageComposer"));
            chatMsg.AppendInteger(VirtualId);
            chatMsg.AppendString(msg);
            chatMsg.AppendInteger(GetSpeechEmotion(msg));
            chatMsg.AppendInteger(textColor);
            chatMsg.AppendInteger(0);// links count (foreach string string bool)
            chatMsg.AppendInteger(count);
            GetRoom().BroadcastChatMessage(chatMsg, this, session.GetHabbo().Id);

            GetRoom().OnUserSay(this, msg, shout);

            GetRoom().GetRoomUserManager().TurnHeads(X, Y, HabboId);
        }

        internal void commandChat(GameClient session, string msg, bool shout, int count, int textColor = 0)
        {
            if (session == null)
                return;

            if (IsPet || IsBot)
            {
                if (!IsPet)
                    textColor = 2;

                var botChatmsg = new ServerMessage();
                botChatmsg.Init(shout
                    ? LibraryParser.OutgoingRequest("ShoutMessageComposer")
                    : LibraryParser.OutgoingRequest("ChatMessageComposer"));
                botChatmsg.AppendInteger(VirtualId);
                botChatmsg.AppendString(msg);
                botChatmsg.AppendInteger(0);
                botChatmsg.AppendInteger(textColor);
                botChatmsg.AppendInteger(0);
                botChatmsg.AppendInteger(count);

                GetRoom().SendMessage(botChatmsg);
                return;
            }

            if (msg.Length > 100) // help prevent scripters
                return;

            if (session == null || session.GetHabbo() == null)
                return;

            if (session.GetHabbo().Rank < 4 && GetRoom().CheckMute(session))
                return;

            if (!msg.StartsWith("*"))
            {
                UnIdle();
            }

            if (!IsPet && !IsBot)
            {
                #region Emotions

                if (!session.GetRoleplay().MultiCoolDown.ContainsKey("emotion_chat"))
                {
                    session.GetRoleplay().MultiCoolDown.Add("emotion_chat", 0);
                }

                if (session.GetRoleplay().MultiCoolDown["emotion_chat"] <= 0)
                {

                    if (msg.Equals(":S") || msg.Equals(";S") || msg.Equals(";-S") || msg.Equals(":-S"))
                    {
                        Chat(session, "*Confused*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":O") || msg.Equals(";O") || msg.Equals(";-O") || msg.Equals(":-O"))
                    {
                        Chat(session, "*Shocked*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":D") || msg.Equals(";D") || msg.Equals(":-D") || msg.Equals(";-D"))
                    {
                        Chat(session, "*Grins*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":)") || msg.Equals(":-)") || msg.Equals("=)") || msg.Equals("=]") || msg.Equals("(:") || msg.Equals("[=") || msg.Equals(":-]") || msg.Equals(":]"))
                    {
                        Chat(session, "*Smiles*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":L") || msg.Equals(":-L") || msg.Equals(";L") || msg.Equals(";-L"))
                    {
                        Chat(session, "*Awks*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":(") || msg.Equals(":-(") || msg.Equals("=[") || msg.Equals(";-(") || msg.Equals(":[") || msg.Equals(":-["))
                    {
                        Chat(session, "*Frowns*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(":'(") || msg.Equals(":'-(") || msg.Equals(":-'("))
                    {
                        Chat(session, "*Cries*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(";)") || msg.Equals(";-)") || msg.Equals(";]") || msg.Equals(";-]"))
                    {
                        Chat(session, "*Winks*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }

                    if (msg.Equals(";@") || msg.Equals(":@") || msg.Equals(":-@") || msg.Equals(";-@"))
                    {
                        Chat(session, "*Angry*", true, -1);
                        session.GetRoleplay().MultiCoolDown["emotion_chat"] = 3;
                        session.GetRoleplay().CheckingMultiCooldown = true;
                        return;
                    }
                }
                else
                {
                    session.SendWhisper("Emotion cooldown [" + session.GetRoleplay().MultiCoolDown["emotion_chat"] + "/3]");
                    return;
                }

                #endregion

                if (session.GetHabbo().GetRoomUser().Frozen && session.GetRoleplay().MuteSeconds > 0)
                {
                    session.SendWhisper("The effect of the stun gun is preventing you from talking! Please wait " + session.GetRoleplay().MuteSeconds + " Seconds!");
                    return;
                }

                if (msg == "x")
                {
                    if (session.GetRoleplay().LastHitBot != null)
                    {
                        if (session.GetRoleplay().LastHitBot.BotData != null)
                        {
                           if(session.GetRoleplay().InMafiaWars)
                           {
                               ChatCommandHandler.ParseMafiaWars(session, ":hit " + session.GetRoleplay().LastHitBot.BotData.Name);
                           }
                           else
                           {
                               ChatCommandHandler.Parse(session, ":hit " + session.GetRoleplay().LastHitBot.BotData.Name);
                           }
                        }
                        else
                        {
                            session.SendWhisper("The last bot you hit was not found!");
                            session.GetRoleplay().LastHitBot = null;
                        }
                    }
                    else
                    {
                        if (!RoleplayManager.CanInteract(session, session.GetRoleplay().LastHit, true))
                        {
                            if (session.GetRoleplay().ActionLast == "hit")
                            {
                                ChatCommandHandler.Parse(session, ":hit " + session.GetRoleplay().LastHit.GetHabbo().UserName);
                            }
                            else if (session.GetRoleplay().ActionLast == "melee")
                            {
                                ChatCommandHandler.Parse(session, ":hit " + session.GetRoleplay().LastHit.GetHabbo().UserName);
                            }
                            else if (session.GetRoleplay().ActionLast == "shoot")
                            {
                                ChatCommandHandler.Parse(session, ":shoot " + session.GetRoleplay().LastHit.GetHabbo().UserName);
                            }
                            else if (session.GetRoleplay().ActionLast == "bomb")
                            {
                                ChatCommandHandler.Parse(session, ":bomb " + session.GetRoleplay().LastHit.GetHabbo().UserName);
                            }
                        }
                        else
                        {
                            session.GetRoleplay().LastHit = null;
                            session.SendWhisper("The last person you hit was not found!");
                        }
                    }
                    return;
                }

                if (1==1)
                {
                    if (msg.StartsWith("#") && ChatCommandHandler.Parse2(session, msg))
                    {
                        GetRoom().AddChatlog(session.GetHabbo().Id, msg, true);
                        return;
                    }

                    // Dont log combat, run less queries..
                    if (msg.StartsWith(":") &&
                        ChatCommandHandler.Parse(session, msg) &&
                        !msg.ToLower().Contains("shoot") &&
                        !msg.ToLower().Contains("hit") &&
                        !msg.ToLower().Contains("bomb"))
                    {
                        GetRoom().AddChatlog(session.GetHabbo().Id, msg, true);
                        return;
                    }
                }

                var habbo = GetClient().GetHabbo();

                if (GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerOnUserSay, this, msg))
                    return;

                //Do not log combat msgs..
                if (!msg.StartsWith("*"))
                {
                    GetRoom().AddChatlog(session.GetHabbo().Id, msg, true);
                }
                uint rank = 1;

                if (session.GetHabbo() != null)
                    rank = session.GetHabbo().Rank;

                msg = GetRoom()
                    .WordFilter
                    .Aggregate(msg,
                        (current1, current) => Regex.Replace(current1, current, "bobba", RegexOptions.IgnoreCase));

                if (!msg.StartsWith("*"))
                {

                }
            }
            else if (!IsPet)
                textColor = 2;

            var chatMsg = new ServerMessage();
            chatMsg.Init(shout
                ? LibraryParser.OutgoingRequest("ShoutMessageComposer")
                : LibraryParser.OutgoingRequest("ChatMessageComposer"));
            chatMsg.AppendInteger(VirtualId);
            chatMsg.AppendString(msg);
            chatMsg.AppendInteger(GetSpeechEmotion(msg));
            chatMsg.AppendInteger(textColor);
            chatMsg.AppendInteger(0);// links count (foreach string string bool)
            chatMsg.AppendInteger(count);
            GetRoom().BroadcastChatMessage(chatMsg, this, session.GetHabbo().Id);

            GetRoom().OnUserSay(this, msg, shout);

            GetRoom().GetRoomUserManager().TurnHeads(X, Y, HabboId);
        }

        /// <summary>
        /// Increments the and check flood.
        /// </summary>
        /// <param name="muteTime">The mute time.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool IncrementAndCheckFlood(out int muteTime)
        {
            muteTime = 0;
            var timeSpan = DateTime.Now - GetClient().GetHabbo().SpamFloodTime;
            if (timeSpan.TotalSeconds > GetClient().GetHabbo().SpamProtectionTime &&
                GetClient().GetHabbo().SpamProtectionBol)
            {
                _floodCount = 0;
                GetClient().GetHabbo().SpamProtectionBol = false;
                GetClient().GetHabbo().SpamProtectionAbuse = 0;
            }
            else if (timeSpan.TotalSeconds > 2.0)
                _floodCount = 0;

            {
                if (timeSpan.TotalSeconds < 2.0 && _floodCount > 6 && GetClient().GetHabbo().Rank < 5u)
                {
                    muteTime = 10;
                    return true;
                }
                GetClient().GetHabbo().SpamFloodTime = DateTime.Now;
                _floodCount++;
                return false;
            }
        }

        /// <summary>
        /// Clears the movement.
        /// </summary>
        /// <param name="update">if set to <c>true</c> [update].</param>
        internal void ClearMovement()
        {
            IsWalking = false;
            GoalX = 0;
            GoalY = 0;
            SetStep = false;
            try
            {
                GetRoom().GetRoomUserManager().ToSet.Remove(new Point(SetX, SetY));
            }
            catch (Exception)
            {
            }
            SetX = 0;
            SetY = 0;
            SetZ = 0.0;

            if (!Statusses.ContainsKey("mv")) return;
            Statusses.Remove("mv");
            UpdateNeeded = true;
        }
        /// <summary>
        /// Moves to.
        /// </summary>
        /// <param name="c">The c.</param>
        internal void MoveTo(Point c)
        {
            MoveTo(c.X, c.Y);
        }

        /// <summary>
        /// Moves to.
        /// </summary>
        /// <param name="x">The p x.</param>
        /// <param name="y">The p y.</param>
        /// <param name="pOverride">if set to <c>true</c> [p override].</param>
        internal void MoveTo(int x, int y, bool pOverride)
        {
            if (TeleportEnabled)
            {
                UnIdle();
                GetRoom()
                    .SendMessage(GetRoom()
                        .GetRoomItemHandler()
                        .UpdateUserOnRoller(this, new Point(x, y), 0u,
                            GetRoom().GetGameMap().SqAbsoluteHeight(GoalX, GoalY)));
                if (Statusses.ContainsKey("sit")) Z -= 0.35;
                UpdateNeeded = true;
                GetRoom().GetRoomUserManager().UpdateUserStatus(this, false);
                return;
            }
            if (GetRoom().GetGameMap().SquareHasUsers(x, y) && !pOverride) return;
            if (Frozen) return;

            var coordItemSearch = new CoordItemSearch(GetRoom().GetGameMap().CoordinatedItems);
            var allRoomItemForSquare = coordItemSearch.GetAllRoomItemForSquare(x, y);
            if ((RidingHorse && !IsBot && allRoomItemForSquare.Any()) || (IsPet && allRoomItemForSquare.Any()))
                if (
                    allRoomItemForSquare.Any(
                        current =>
                            (current.GetBaseItem().IsSeat ||
                             current.GetBaseItem().InteractionType == Interaction.LowPool ||
                             current.GetBaseItem().InteractionType == Interaction.Pool ||
                             current.GetBaseItem().InteractionType == Interaction.HaloweenPool ||
                             current.GetBaseItem().InteractionType == Interaction.Bed ||
                             current.GetBaseItem().InteractionType == Interaction.Guillotine || current.GetBaseItem().InteractionType == Interaction.BanzaiGateYellow
                                ))) return;
            UnIdle();

            if (_mRoom.GetGameMap().MapOk(new Vector2D(x, y)))
            {
                GoalX = x;
                GoalY = y;
                LastSelectedX = GoalX;
                LastSelectedY = GoalY;

                PathRecalcNeeded = true;
                ThrowBallAtGoal = false;



                if (AttackPet != null && AttackPet.BotAI._Victim == null)
                {
                    System.Threading.Thread MoveThread = new System.Threading.Thread(() => AttackPet.Move(x, y, RotBody));
                    MoveThread.Start();
                }

                if (MyPet != null && MyPet.BotAI._Victim == null)
                {
                    System.Threading.Thread MoveThread = new System.Threading.Thread(() => MyPet.Move(x, y, RotBody));
                    MoveThread.Start();
                }
            }
            else
            {
                /* if (_mRoom.GetGameMap().squareBlocked(new Vector2D(x, y)))
                 {
                     Vector2D NewCords = _mRoom.GetGameMap().squareBlockedAlternativeSquare(new Vector2D(x, y));

                     if (NewCords != null)
                     {
                         GoalX = NewCords.X;
                         GoalY = NewCords.Y;
                         LastSelectedX = GoalX;
                         LastSelectedY = GoalY;

                         PathRecalcNeeded = true;
                         ThrowBallAtGoal = false;
                     }
                 }*/
            }
        }

        /// <summary>
        /// Moves to.
        /// </summary>
        /// <param name="pX">The p x.</param>
        /// <param name="pY">The p y.</param>
        internal void MoveTo(int pX, int pY)
        {
            MoveTo(pX, pY, false);
        }

        /// <summary>
        /// Unlocks the walking.
        /// </summary>
        internal void UnlockWalking()
        {
            AllowOverride = false;
            CanWalk = true;
        }

        /// <summary>
        /// Sets the position.
        /// </summary>
        /// <param name="pX">The p x.</param>
        /// <param name="pY">The p y.</param>
        /// <param name="pZ">The p z.</param>
        internal void SetPos(int pX, int pY, double pZ)
        {
            // Fix banzai tile
            _mRoom.GetGameMap().UpdateUserMovement(new Point(X, Y), new Point(pX, pY), this);
            X = pX;
            Y = pY;
            Z = pZ;
        }

        /// <summary>
        /// Carries the item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void CarryItem(int item)
        {
            CarryItemId = item;
            CarryTimer = item > 0 ? 240 : 0;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ApplyHanditemMessageComposer"));
            serverMessage.AppendInteger(VirtualId);
            serverMessage.AppendInteger(item);
            GetRoom().SendMessage(serverMessage);
        }

        /// <summary>
        /// Sets the rot.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        internal void SetRot(int rotation)
        {
            SetRot(rotation, false);
        }

        /// <summary>
        /// Sets the rot.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        /// <param name="headOnly">if set to <c>true</c> [head only].</param>
        internal void SetRot(int rotation, bool headOnly)
        {
            if (Statusses.ContainsKey("lay") || IsWalking) return;
            var num = RotBody - rotation;
            RotHead = RotBody;
            if (Statusses.ContainsKey("sit") || headOnly)
                switch (RotBody)
                {
                    case 4:
                    case 2:
                        if (num > 0) RotHead = RotBody - 1;
                        else if (num < 0) RotHead = RotBody + 1;
                        break;

                    case 6:
                    case 0:
                        if (num > 0) RotHead = RotBody - 1;
                        else if (num < 0) RotHead = RotBody + 1;
                        break;
                }
            else if (num <= -2 || num >= 2)
            {
                RotHead = rotation;
                RotBody = rotation;
            }
            else RotHead = rotation;
            UpdateNeeded = true;
        }

        /// <summary>
        /// Sets the status.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        internal void SetStatus(string key, string value)
        {
            if (Statusses.ContainsKey(key))
            {
                Statusses[key] = value;
                return;
            }
            AddStatus(key, value);
        }

        /// <summary>
        /// Adds the status.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        internal void AddStatus(string key, string value)
        {
            Statusses[key] = value;
        }

        /// <summary>
        /// Removes the status.
        /// </summary>
        /// <param name="key">The key.</param>
        internal void RemoveStatus(string key)
        {
            if (Statusses.ContainsKey(key))
                Statusses.Remove(key);
        }

        /// <summary>
        /// Applies the effect.
        /// </summary>
        /// <param name="effectId">The effect identifier.</param>
        internal void ApplyEffect(int effectId)
        {
            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null ||
                GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() == null)
                return;
            GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectId);
        }

        /// <summary>
        /// Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="gotPublicRoom">if set to <c>true</c> [got public room].</param>
        internal void Serialize(ServerMessage message, bool gotPublicRoom)
        {
            if (message == null)
                return;
            if (IsSpectator)
                return;
            if (!IsBot)
            {
                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return;
                var group = Plus.GetGame().GetGroupManager().GetGroup(GetClient().GetHabbo().FavouriteGroup);
                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return;
                var habbo = GetClient().GetHabbo();

                if (habbo == null)
                    return;

                message.AppendInteger(habbo.Id);
                message.AppendString(habbo.UserName);
                message.AppendString(habbo.Motto);
                message.AppendString(habbo.Look);
                message.AppendInteger(VirtualId);
                message.AppendInteger(X);
                message.AppendInteger(Y);
                message.AppendString(TextHandling.GetString(Z));
                message.AppendInteger(0);
                message.AppendInteger(1);
                message.AppendString(habbo.Gender.ToLower());
                if (@group != null)
                {
                    message.AppendInteger(@group.Id);
                    message.AppendInteger(0);
                    message.AppendString(@group.Name);
                }
                else
                {
                    message.AppendInteger(0);
                    message.AppendInteger(0);
                    message.AppendString("");
                }
                message.AppendString("");
                message.AppendInteger(habbo.AchievementPoints);
                message.AppendBool(false);
                return;
            }
            message.AppendInteger(BotAI.BaseId);
            message.AppendString(BotData.Name);
            message.AppendString(BotData.Motto);
            if (BotData.AiType == AIType.Pet || BotData.AiType == AIType.FightPet)
                if (PetData.Type == 16u)
                    message.AppendString(PetData.MoplaBreed.PlantData);
                else if (PetData.HaveSaddle == Convert.ToBoolean(2))
                    message.AppendString(string.Concat(new object[]
                    {
                        BotData.Look.ToLower(),
                        " 3 4 10 0 2 ",
                        PetData.PetHair,
                        " ",
                        PetData.HairDye,
                        " 3 ",
                        PetData.PetHair,
                        " ",
                        PetData.HairDye
                    }));
                else if (PetData.HaveSaddle == Convert.ToBoolean(1))
                    message.AppendString(string.Concat(new object[]
                    {
                        BotData.Look.ToLower(),
                        " 3 2 ",
                        PetData.PetHair,
                        " ",
                        PetData.HairDye,
                        " 3 ",
                        PetData.PetHair,
                        " ",
                        PetData.HairDye,
                        " 4 9 0"
                    }));
                else
                    message.AppendString(string.Concat(new object[]
                    {
                        BotData.Look.ToLower(),
                        " 2 2 ",
                        PetData.PetHair,
                        " ",
                        PetData.HairDye,
                        " 3 ",
                        PetData.PetHair,
                        " ",
                        PetData.HairDye
                    }));
            else
                message.AppendString(BotData.Look.ToLower());
            message.AppendInteger(VirtualId);
            message.AppendInteger(X);
            message.AppendInteger(Y);
            message.AppendString(TextHandling.GetString(Z));
            message.AppendInteger(0);
            message.AppendInteger((BotData.AiType == AIType.Generic || BotData.AiType == AIType.MiscBot || BotData.AiType == AIType.MafiaWarBot || BotData.AiType == AIType.WeedBot || BotData.AiType == AIType.HospitalBot) ? 4 : 2);
            if (BotData.AiType == AIType.Pet || BotData.AiType == AIType.FightPet)
            {
                message.AppendInteger(PetData.Type);
                message.AppendInteger(PetData.OwnerId);
                message.AppendString(PetData.OwnerName);
                message.AppendInteger((PetData.Type == 16u) ? 0 : 1);
                message.AppendBool(PetData.HaveSaddle);
                message.AppendBool(RidingHorse);
                message.AppendInteger(0);
                message.AppendInteger((PetData.Type == 16u) ? 1 : 0);
                message.AppendString((PetData.Type == 16u) ? PetData.MoplaBreed.GrowStatus : "");
                return;
            }
            message.AppendString(BotData.Gender.ToLower());
            message.AppendInteger(BotData.OwnerId);
            message.AppendString(Plus.GetGame().GetClientManager().GetNameById(BotData.OwnerId));
            message.AppendInteger(5);
            message.AppendShort(1);
            message.AppendShort(2);
            message.AppendShort(3);
            message.AppendShort(4);
            message.AppendShort(5);
        }

        /// <summary>
        /// Serializes the status.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SerializeStatus(ServerMessage message)
        {
            message.AppendInteger(VirtualId);
            message.AppendInteger(X);
            message.AppendInteger(Y);
            message.AppendString(TextHandling.GetString(Z));
            message.AppendInteger(RotHead);
            message.AppendInteger(RotBody);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("/");
            if (IsPet && PetData.Type == 16u)
                stringBuilder.AppendFormat("/{0}{1}", PetData.MoplaBreed.GrowStatus, (Statusses.Count >= 1) ? "/" : "");
            lock (Statusses)
            {
                foreach (var current in Statusses)
                {
                    stringBuilder.Append(current.Key);
                    if (!string.IsNullOrEmpty(current.Value))
                    {
                        stringBuilder.Append(" ");
                        stringBuilder.Append(current.Value);
                    }
                    stringBuilder.Append("/");
                }
            }
            stringBuilder.Append("/");
            message.AppendString(stringBuilder.ToString());

            if (!Statusses.ContainsKey("sign"))
                return;
            RemoveStatus("sign");
            UpdateNeeded = true;
        }

        /// <summary>
        /// Serializes the status.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="status">The status.</param>
        internal void SerializeStatus(ServerMessage message, string status)
        {
            if (IsSpectator)
                return;
            message.AppendInteger(VirtualId);
            message.AppendInteger(X);
            message.AppendInteger(Y);
            message.AppendString(TextHandling.GetString(SetZ));
            message.AppendInteger(RotHead);
            message.AppendInteger(RotBody);
            message.AppendString(status);
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <returns>GameClient.</returns>
        internal GameClient GetClient()
        {
            if (IsBot)
                return null;

            if (_mClient != null)
                return _mClient;

            return _mClient = Plus.GetGame().GetClientManager().GetClientByUserId(HabboId);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessage(byte[] message)
        {
            if (GetClient() == null || GetClient().GetConnection() == null) return;
            GetClient().GetConnection().SendData(message);
        }

        /// <summary>
        /// Gets the room.
        /// </summary>
        /// <returns>Room.</returns>
        private Room GetRoom()
        {
            return _mRoom ?? (_mRoom = Plus.GetGame().GetRoomManager().GetRoom(RoomId));
        }
    }
}