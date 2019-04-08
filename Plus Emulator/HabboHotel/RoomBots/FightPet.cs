using Plus.Configuration;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pets;
using Plus.HabboHotel.Rooms;
using Plus.Messages;
using Plus.Messages.Parsers;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Data;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.Roleplay.Combat;
using Plus.HabboHotel.Roleplay.Bots;

namespace Plus.HabboHotel.RoomBots
{
    /// <summary>
    /// Class PetBot.
    /// </summary>
    internal class FightPet : BotAI
    {
        /// <summary>
        /// The _speech timer
        /// </summary>
        private int _speechTimer;

        /// <summary>
        /// The _action timer
        /// </summary>
        private int _actionTimer;

        /// <summary>
        /// The _energy timer
        /// </summary>
        private int _energyTimer;

        /// <summary>
        /// The owner of the pitbull
        /// </summary>
        public RoomUser MyOwner;


        /// <summary>
        /// Times the attack
        /// </summary>
        private Timer _AttackTimer;

        /// <summary>
        /// Times the cooldown
        /// </summary>
        private Timer _CoolDownTimer;



        /// <summary>
        /// Is the bot cooling down
        /// </summary>
        public bool _CoolingDown = false;


        int CurHealth;
        int MaxHealth;
        int Str;
        int FollowInterval;
        int CurFollowInterval;
        int CoolDown;
        int CurCoolDown;
        string RPType;

        int Difficulty_IncrementNum;
        int cur_Difficulty;
        bool Stunned;
        bool Arrested;
        bool Killed;
        int WalkInterval;
        int CurWalkInterval;

        /// <summary>
        /// Initializes a new instance of the <see cref="PetBot"/> class.
        /// </summary>
        /// <param name="virtualId">The virtual identifier.</param>
        /// 
        internal FightPet(int virtualId)
        {
            Difficulty_IncrementNum = 1;
            cur_Difficulty = 0;
            Stunned = false;
            Arrested = false;
            Killed = false;
            WalkInterval = 2;
            CurWalkInterval = 0;
            CurFollowInterval = 0;

            {
                _speechTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
                _actionTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 30 + virtualId);
                _energyTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
            }
            // this.GetBotData().InitRPStats();
        }

        /// <summary>
        /// Called when [self enter room].
        /// </summary>
        internal override void OnSelfEnterRoom()
        {
            ApplyEffect(1);
            RoomBot Pet = this.GetBotData();

            if (RPType == "dragon")
            {
                Shout("*Breathes a giant gust of fire into the air*");
            }
            // Initialize RP Stats
            this.GetBotData().InitRPStats();
            this.MaxHealth = (int)Pet.GetRPStat("rp_maxhealth");
            this.Str = (int)Pet.GetRPStat("rp_str");
            this.FollowInterval = (int)Pet.GetRPStat("rp_follow_interval");
            this.CoolDown = (int)Pet.GetRPStat("rp_cooldown");
            this.CurCoolDown = 0;
            this.RPType = (string)Pet.GetRPStat("rp_type");
            this.CurHealth = this.MaxHealth;

            // Initialize some shit init
            GameClient OwnerSession = HabboHotel.Roleplay.Misc.RoleplayManager.GenerateSession(GetRoomUser().PetData.OwnerName);

            if (OwnerSession == null)
                return;

            MyOwner = OwnerSession.GetHabbo().GetRoomUser();

            if (MyOwner == null)
                return;

            GetRoomUser().FollowingOwner = MyOwner;
            GetRoomUser().FollowingOwner.MyPet = GetRoomUser();
        }

        internal void ApplyEffect(uint Effect)
        {
            int VirtualId = this.GetRoomUser().VirtualId;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ApplyEffectMessageComposer"));
            serverMessage.AppendInteger(VirtualId);
            serverMessage.AppendInteger(Effect);
            serverMessage.AppendInteger(0);
            this.GetRoom().SendMessage(serverMessage);
        }

        /// <summary>
        /// Called when [self leave room].
        /// </summary>
        /// <param name="kicked">if set to <c>true</c> [kicked].</param>
        internal override void OnSelfLeaveRoom(bool kicked)
        {
            if (MyOwner != null)
            {
                if (MyOwner.MyPet != null)
                {
                    MyOwner.MyPet = null;
                }
            }

            MyOwner = null;
            _Victim = null;
            EndTimers();
        }

        internal void OnVictimEnterRoom()
        {

        }

        internal void OnArrest()
        {

        }

        internal void OnVictimDeath()
        {

        }

        internal void OnOwnerLeaveRoom()
        {
            OnSelfLeaveRoom(true);
            GameClient sess = GetRoomUser().FollowingOwner.GetClient();
            GetRoom().GetRoomUserManager().RemoveBot(GetRoomUser().VirtualId, true);
        }

        /// <summary>
        /// Modifieds this instance.
        /// </summary>
        internal override void Modified()
        {
        }

        /// <summary>
        /// Called when [user enter room].
        /// </summary>
        /// <param name="user">The user.</param>
        internal override void OnUserEnterRoom(RoomUser user)
        {
        }

        /// <summary>
        /// Called when [user leave room].
        /// </summary>
        /// <param name="client">The client.</param>
        internal override void OnUserLeaveRoom(GameClient client)
        {
            if (_Victim == null || _Victim.GetClient() == null || client == null || client.GetHabbo() == null || client.GetHabbo().GetRoomUser() == null)
            { _Victim = null; return; }

            if (_Victim.GetClient() == client)
                _Victim = null;
        }

        /// <summary>
        /// Called when [user say].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="msg">The MSG.</param>
        internal override void OnUserSay(RoomUser user, string msg)
        {

        }

        /// <summary>
        /// Called when [user shout].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        internal override void OnUserShout(RoomUser user, string message)
        {
        }

        /// <summary>
        /// Calculates pet damage
        /// </summary>
        /// <param name="Session"></param>
        /// <param name="isWeapon"></param>
        /// <returns></returns>
        public int DamageCalculator()
        {

            int Damage = 0;
            int Bonus = 0;
            int str = 0;

            if (1 == 1)
            {

                str = Str;

                int rndNum = new Random().Next(1, 20);

                if (rndNum == 1)
                {
                    Damage += (str + Bonus + 1) * 3;
                }

                else if (rndNum < 5)
                {
                    Damage += (str + Bonus + 1) * 2;
                }

                else if (rndNum < 10)
                {
                    Damage += str + Bonus + 3;
                }

                else
                {
                    Damage += str + Bonus + 1;
                }

            }

            return Damage;
        }

        internal uint GetUsersRoom(RoomUser User)
        {
            if (User == null) return 0;
            if (User.GetClient() == null) return 0;
            if (User.GetClient().GetHabbo() == null) return 0;

            return User.GetClient().GetHabbo().CurrentRoomId;
        }

        /// <summary>
        /// Called when [timer tick].
        /// </summary>
        internal override void OnTimerTick()
        {

            #region Variables

            uint OwnerRoom = 0;
            uint MyRoom = 0;

            #endregion

            #region Check Null
            if (GetRoomUser() == null) return;
            if (GetRoom() == null) return;
            #endregion

            #region Check for dragons
            if (RPType == "dragon")
            {
                if (!GetRoomUser().Statusses.ContainsKey("mv"))
                {
                    GetRoomUser().AddStatus("mv", "");
                    GetRoomUser().UpdateNeeded = true;
                }
            }
            #endregion

            #region Check if owners here
            if (!InSameRoom(GetRoomUser().FollowingOwner))
            {
                OnOwnerLeaveRoom();
                return;
            }

            if (GetRoomUser().FollowingOwner.GetClient().GetRoleplay().Dead)
            {
                OnOwnerLeaveRoom();
                return;
            }
            #endregion

            if (_Victim == null)
            {
                FollowPlayer(GetRoomUser().FollowingOwner);
            }
            else
            {
                if (!InSameRoom(_Victim))
                {
                    FollowPlayer(GetRoomUser().FollowingOwner);
                    _Victim = null;
                    return;
                }

                TryAttack();
            }
        }

        /// <summary>
        /// Follows a specified player
        /// </summary>
        /// <param name="User"></param>
        internal void FollowPlayer(RoomUser User)
        {
            if (InSameRoom(User))
            {
                FightPetManager.WalkToPlayer(this.GetRoomUser(), User);
            }
            else
            {
                _Victim = null;
                return;
            }
        }

        /// <summary>
        /// Causes pet to shout
        /// </summary>
        /// <param name="Msg"></param>
        internal void Shout(string Msg)
        {
            GetRoomUser().Chat(null, Msg, true, 0, 0);
        }

        /// <summary>
        /// Manages attack speeches / and / or effects
        /// </summary>
        /// <param name="Attacking"></param>
        /// <param name="Damage"></param>
        /// <param name="DeathMisc"></param>
        /// <param name="MissedMisc"></param>
        internal void AttackMisc(RoomUser Attacking, int Damage, bool DeathMisc = false, bool MissedMisc = false)
        {

            string NormMsg = null;
            string DeathMsg = null;
            string MissMsg;
            switch (RPType)
            {
                case "dragon":

                    if (!MissedMisc)
                    {
                        Attacking.GetClient().GetRoleplay().EffectSeconds = 5;
                        Attacking.ApplyEffect(25);
                    }
                    NormMsg = "*Breathes fire at " + Attacking.GetUserName() + ", causing " + Damage + " damage*";
                    DeathMsg = "*Breathes fire at " + Attacking.GetUserName() + ", causing " + Damage + " damage and killing them!*";
                    MissMsg = "*Breathes fire at " + Attacking.GetUserName() + ", but misses!*";
                    break;

                default:
                    NormMsg = "*Bites " + Attacking.GetUserName() + ", causing " + Damage + " damage*";
                    DeathMsg = "*Bites " + Attacking.GetUserName() + ", causing " + Damage + " damage and killing them!*";
                    MissMsg = "*Tries to bite " + Attacking.GetUserName() + ", but misses!*";
                    break;
            }

            if (MissedMisc)
            {
                Shout(MissMsg);
                return;

            }

            if (DeathMisc)
            {
                Shout(DeathMsg);
                return;
            }

            if (!DeathMisc)
            {
                Shout(NormMsg);
                return;
            }



        }

        /// <summary>
        /// Actually does the attack
        /// </summary>
        /// <param name="User"></param>
        internal void ExecuteAttack(RoomUser User)
        {
            if (User == null)
                return;

            if (!User.IsBot)
            {
                if (_Victim == null)
                    return;

                if (_Victim.GetClient().GetRoleplay().Dead)
                {
                    _Victim = null;
                    return;
                }

                if (!InSameRoom(_Victim))
                {
                    _Victim = null;
                    return;
                }

                GameClient Session = User.GetClient();

                int Damage = DamageCalculator();

                // Kill this punk
                if (Session.GetRoleplay().CurHealth - Damage <= 0)
                {
                    AttackMisc(User, Damage, true);
                    GameClient TargetSession = Session;

                    TargetSession.GetRoleplay().DeadFigSet = false;
                    TargetSession.GetRoleplay().DeadSeconds = 60;
                    TargetSession.GetRoleplay().DeadTimer = 10;
                    TargetSession.GetRoleplay().Dead = true;
                    TargetSession.GetRoleplay().SaveStatusComponents("dead");
                    RoleplayManager.HandleDeath(TargetSession);

                    // Tell this nigga he got fucked up by our bot
                    Session.SendNotif("You got killed by PET-" + GetBotData().Name);
                    //_Victim = null;
                    FollowPlayer(GetRoomUser().FollowingOwner);
                    GetRoomUser().ClearMovement();
                    return;
                }

                AttackMisc(User, Damage);
                RoleplayManager.Shout(Session, "*[" + Session.GetRoleplay().CurHealth + "/" + Session.GetRoleplay().MaxHealth + "]*");

                Session.GetRoleplay().CurHealth -= Damage;
                Session.GetRoleplay().UpdateStats++;



            }
            else
                AttackBot(User);
        }

        /// <summary>
        /// Attacks a bot [Bot vs Bot combat]
        /// </summary>
        /// <param name="User"></param>
        internal void AttackBot(RoomUser User)
        {

        }

        /// <summary>
        /// Checks if targetted roomuser is in same room as bot
        /// </summary>
        /// <param name="Target"></param>
        /// <returns></returns>
        internal bool InSameRoom(RoomUser Target)
        {
            uint victimroom = 0;
            uint petroom = 0;

            if (Target == null) return false;
            if (Target.GetClient() == null) return false;
            if (Target.GetClient().GetHabbo() == null) return false;
            if (Target.GetClient().GetRoleplay() == null) return false;
            if (Target.GetClient().GetRoleplay().Dead) return false;
            if (Target.GetClient().GetRoleplay().Jailed) return false;


            victimroom = Target.GetClient().GetHabbo().CurrentRoomId;
            petroom = GetRoom().RoomId;

            if (petroom == 0) return false;
            if (victimroom == 0) return false;
            if (victimroom != petroom) return false;

            return true;
        }

        /// <summary>
        /// Attacks the victim
        /// </summary>
        internal void TryAttack()
        {

            Vector2D VictimPOS = new Vector2D(_Victim.X, _Victim.Y);
            Vector2D MyPOS = new Vector2D(GetRoomUser().X, GetRoomUser().Y);

            #region Conditions
            if (_Victim == null)
            {
                _Victim = null;
                FollowPlayer(GetRoomUser().FollowingOwner);
                return;
            }

            if (_Victim.IsAsleep)
            {
                _Victim = null;
                Shout("Dammnit, hes asleep..");
                FollowPlayer(GetRoomUser().FollowingOwner);
                return;
            }

            if (_Victim.GetClient() != null)
            {
                if (_Victim.GetClient().GetRoleplay().Dead)
                {
                    _Victim = null;
                    FollowPlayer(GetRoomUser().FollowingOwner);
                    return;
                }
            }

            if (!InSameRoom(_Victim))
            {
                _Victim = null;
                FollowPlayer(GetRoomUser().FollowingOwner);
                return;
            }
            #endregion

            #region Following Cooldown
            if (CurCoolDown > 0)
            {

                if (CurWalkInterval > 0)
                {
                    RunRandomly();
                    CurWalkInterval--;
                }
                else
                {
                    CurWalkInterval = WalkInterval;
                }

                CurCoolDown--;
                return;
            }
            else
            {
                if (CurFollowInterval > FollowInterval)
                {
                    CurFollowInterval--;
                    return;
                }
                else
                {
                    CurFollowInterval++;
                    FollowPlayer(_Victim);
                }
            }
            #endregion

            #region Lands Attack
            if (RoleplayManager.BotWithinAttackDistance(GetRoomUser(), _Victim))
            {

                uint victimroom = 0;
                uint petroom = 0;
                if (_Victim.GetClient() != null)
                {
                    if (_Victim.GetClient().GetHabbo() != null)
                    {
                        victimroom = _Victim.GetClient().GetHabbo().CurrentRoomId;
                        petroom = GetRoom().RoomId;

                        if (petroom != 0)
                        {
                            if (victimroom != petroom)
                            {
                                _Victim = null;
                                FollowPlayer(GetRoomUser().FollowingOwner);
                                return;
                            }
                        }
                    }
                }

                CurCoolDown = CoolDown;
                ExecuteAttack(_Victim);
                RunRandomly();
                return;
            }
            #endregion

            #region Misses Attack
            if (RoleplayManager.Distance(MyPOS, VictimPOS) >= 1 && RoleplayManager.Distance(MyPOS, VictimPOS) <= 2 && _Victim.IsWalking)
            {
                // Miss              
                FollowInterval -= Difficulty_IncrementNum;
                AttackMisc(_Victim, 0, false, true);
            }
            #endregion

            #region Pursues Attack
            if (RoleplayManager.Distance(MyPOS, VictimPOS) >= 5)
            {
                if (CurFollowInterval > FollowInterval)
                {
                    CurFollowInterval--;
                    return;
                }
                else
                {
                    CurFollowInterval++;
                    FollowPlayer(_Victim);
                }

                return; // Don't even bother
            }
            #endregion

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
        /// Runs to a random spot in room
        /// </summary>
        private void RunRandomly()
        {
            int randomX = Plus.GetRandomNumber(0, GetRoom().GetGameMap().Model.MapSizeX);
            int randomY = Plus.GetRandomNumber(0, GetRoom().GetGameMap().Model.MapSizeY);
            GetRoomUser().MoveTo(randomX, randomY);
        }

        /// <summary>
        /// Removes the pet status.
        /// </summary>
        private void RemovePetStatus()
        {
            RoomUser roomUser = GetRoomUser();

            if (roomUser == null) return;
            roomUser.Statusses.Clear();
            roomUser.UpdateNeeded = true;
        }

        /// <summary>
        /// Subtracts the attributes.
        /// </summary>
        private void SubtractAttributes()
        {
            RoomUser roomUser = GetRoomUser();
            if (roomUser == null) return;

            if (roomUser.PetData.Energy < 11)
                roomUser.PetData.Energy = 0;
            else
                roomUser.PetData.Energy -= 10;
            if (roomUser.PetData.Nutrition < 6)
                roomUser.PetData.Nutrition = 0;
            else
                roomUser.PetData.Nutrition -= 5;
        }
    }
}