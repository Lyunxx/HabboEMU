using Plus.Configuration;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pets;
using Plus.HabboHotel.Rooms;
using Plus.Messages;
using Plus.Messages.Parsers;
using Plus.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Plus.HabboHotel.RoomBots
{
    /// <summary>
    /// Class PetBot.
    /// </summary>
    internal class PetBot : BotAI
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
        /// Initializes a new instance of the <see cref="PetBot"/> class.
        /// </summary>
        /// <param name="virtualId">The virtual identifier.</param>
        internal PetBot(int virtualId)
        {
            {
                _speechTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
                _actionTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 30 + virtualId);
                _energyTimer = new Random((virtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
            }
        }

        /// <summary>
        /// Called when [self enter room].
        /// </summary>
        internal override void OnSelfEnterRoom()
        {
            Point randomWalkableSquare = GetRoom().GetGameMap().GetRandomWalkableSquare();
            if (GetRoomUser() != null && GetRoomUser().PetData.Type != 16u)
            {
                GetRoomUser().MoveTo(randomWalkableSquare.X, randomWalkableSquare.Y);
            }
        }

        /// <summary>
        /// Called when [self leave room].
        /// </summary>
        /// <param name="kicked">if set to <c>true</c> [kicked].</param>
        internal override void OnSelfLeaveRoom(bool kicked)
        {
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
            if (user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            RoomUser roomUser = GetRoomUser();
            if (roomUser == null || user.GetClient().GetHabbo().UserName != roomUser.PetData.OwnerName)
                return;

            var random = new Random();
            string[] value = PetLocale.GetValue("welcome.speech.pet");
            string message = value[random.Next(0, (value.Length - 1))];

            message += user.GetUserName();
            roomUser.Chat(null, message, false, 0, 0);
        }

        /// <summary>
        /// Called when [user leave room].
        /// </summary>
        /// <param name="client">The client.</param>
        internal override void OnUserLeaveRoom(GameClient client)
        {
        }

        /// <summary>
        /// Called when [user say].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="msg">The MSG.</param>
        internal override void OnUserSay(RoomUser user, string msg)
        {
            RoomUser roomUser = GetRoomUser();

            if (roomUser.PetData.OwnerId != user.GetClient().GetHabbo().Id)
            {
                return;
            }
            if (string.IsNullOrEmpty(msg))
            {
                msg = " ";
            }
            string strPetName = roomUser.PetData.Name;
            int intLngPetName = strPetName.Length;
            string strCommand = msg;
            if (!strCommand.Substring(0, intLngPetName).Equals(strPetName))
            {
                return;
            }
            
            msg = msg.Substring(intLngPetName);
            msg = msg.Trim();
            bool lazy = false;
            bool unknown = false;
            bool sleeping = false;
            try
            {
                int command = PetCommandHandler.TryInvoke(msg);
                switch (command)
                {
                    case 7:
                        RemovePetStatus();
                        GameClient OwnerSession = HabboHotel.Roleplay.Misc.RoleplayManager.GenerateSession(GetRoomUser().PetData.OwnerName);
                         if (OwnerSession == null)
                         return;
                         RoomUser MyOwner;
                         MyOwner = OwnerSession.GetHabbo().GetRoomUser();
                         roomUser.FollowingOwner = MyOwner;
                        break;

                    default:
                        RemovePetStatus();
                        roomUser.FollowingOwner = null;
                        break;
                }
            }
            catch (Exception)
            {
                lazy = true;
                SubtractAttributes();
            }

            if (sleeping)
            {
               // string[] value = PetLocale.GetValue("tired");
               // string message = value[new Random().Next(0, (value.Length - 1))];

               // roomUser.Chat(null, message, false, 0, 0);
            }
            else if (unknown)
            {
                //string[] value = PetLocale.GetValue("pet.unknowncommand");
                //string message = value[new Random().Next(0, (value.Length - 1))];

               // roomUser.Chat(null, message, false, 0, 0);
            }
            else if (lazy)
            {
                //string[] value = PetLocale.GetValue("pet.lazy");
                //string message = value[new Random().Next(0, (value.Length - 1))];

                //roomUser.Chat(null, message, false, 0, 0);
            }
            else
            {
                roomUser.Chat(null, "Okay!", false, 0, 0);
            }
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
        /// Called when [timer tick].
        /// </summary>
        internal override void OnTimerTick()
        {
            if (GetRoomUser().FollowingOwner != null)
            {
                GetRoomUser().MoveTo(GetRoomUser().FollowingOwner.SquareBehind);
            }
            if (_speechTimer <= 0)
            {
                RoomUser roomUser = GetRoomUser();
                if (roomUser != null)
                {
                    if (roomUser.PetData.DbState != DatabaseUpdateState.NeedsInsert) roomUser.PetData.DbState = DatabaseUpdateState.NeedsUpdate;
                    var random = new Random();
                    RemovePetStatus();
                    string[] value = PetLocale.GetValue(string.Format("speech.pet{0}", roomUser.PetData.Type));
                    string text = value[random.Next(0, value.Length - 1)];
                    if (GetRoom() != null && !GetRoom().MutedPets) roomUser.Chat(null, text, false, 0, 0);
                    else roomUser.Statusses.Add(text, TextHandling.GetString(roomUser.Z));
                }
                _speechTimer = Plus.GetRandomNumber(20, 120);
            }
            else _speechTimer--;
            if (_actionTimer <= 0 && GetRoomUser() != null)
            {
                try
                {
                    _actionTimer = GetRoomUser().FollowingOwner != null
                        ? 2
                        : Plus.GetRandomNumber(15, 40 + GetRoomUser().PetData.VirtualId);
                    RemovePetStatus();
                    _actionTimer = Plus.GetRandomNumber(15, 40 + GetRoomUser().PetData.VirtualId);
                    if (GetRoomUser().RidingHorse != true)
                    {
                        RemovePetStatus();

                        if (GetRoomUser().FollowingOwner != null)
                        {
                            GetRoomUser().MoveTo(GetRoomUser().FollowingOwner.SquareBehind);
                        }
                        else
                        {
                            if (GetRoomUser().PetData.Type == 16) return; //Monsterplants can't move
                            var nextCoord = GetRoom().GetGameMap().GetRandomValidWalkableSquare();
                            GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                        }
                    }
                    if (new Random().Next(2, 15) % 2 == 0)
                    {
                        if (GetRoomUser().PetData.Type == 16)
                        {
                            MoplaBreed breed = GetRoomUser().PetData.MoplaBreed;
                            GetRoomUser().PetData.Energy--;
                            GetRoomUser().AddStatus("gst", (breed.LiveState == MoplaState.Dead) ? "sad" : "sml");
                            GetRoomUser()
                                .PetData.MoplaBreed.OnTimerTick(GetRoomUser().PetData.LastHealth,
                                    GetRoomUser().PetData.UntilGrown);
                        }
                        else
                        {
                            if (GetRoomUser().PetData.Energy < 30) GetRoomUser().AddStatus("lay", "");
                            else
                            {
                                GetRoomUser().AddStatus("gst", "joy");
                                if (new Random().Next(1, 7) == 3) GetRoomUser().AddStatus("snf", "");
                            }
                        }
                        GetRoomUser().UpdateNeeded = true;
                    }
                    goto IL_1B5;
                }
                catch (Exception pException)
                {
                    Logging.HandleException(pException, "PetBot.OnTimerTick");
                    goto IL_1B5;
                }
            }
            _actionTimer--;
            IL_1B5:
            if (_energyTimer <= 0)
            {
                RemovePetStatus();
                var roomUser2 = GetRoomUser();
                if (roomUser2 != null) roomUser2.PetData.PetEnergy(true);
                _energyTimer = Plus.GetRandomNumber(30, 120);
                return;
            }
            _energyTimer--;
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