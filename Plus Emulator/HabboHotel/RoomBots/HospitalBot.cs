using System;
using System.Linq;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System.Collections.Generic;
using System.Drawing;
using Plus.HabboHotel.Roleplay.Timers;
using Plus.HabboHotel.Roleplay.Misc;

namespace Plus.HabboHotel.RoomBots
{
    class HospitalBot : BotAI
    {
        private static readonly Random Random = new Random();

        private readonly int _id;
        private readonly int _virtualId;
        private readonly bool _isBartender;

        private int _actionCount = 30;
        private int _speechInterval = 7;
        private Timer _chatTimer;

        private string HospBotType = "heal_bot";

        #region Normal Hospital Bot
        public int HospStaffOn_LastCheck = 0;
        #endregion

        #region Paramedic Bot
        internal RoomUser InteractingWith;
        bool LoadedUser;
        bool TransferedUser;
        int LActionTimer;
        #endregion

        internal HospitalBot(RoomBot roomBot, int virtualId, int botId, AIType type, bool isBartender, int speechInterval)
        {
            _id = botId;
            _virtualId = virtualId;
            _isBartender = isBartender;
            _speechInterval = speechInterval < 2 ? 2000 : speechInterval * 1000;

            // Get random speach
            if (roomBot.AutomaticChat && roomBot.RandomSpeech != null && roomBot.RandomSpeech.Any()) _chatTimer = new Timer(ChatTimerTick, null, _speechInterval, _speechInterval);
            _actionCount = Random.Next(10, 30 + virtualId);
        }

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

        internal override void OnSelfEnterRoom()
        {

        }

        internal override void OnSelfLeaveRoom(bool Kicked)
        {

        }

        internal override void OnUserEnterRoom(Rooms.RoomUser User)
        {

        }

        internal override void OnUserLeaveRoom(GameClients.GameClient Client)
        {

        }

        internal override void OnUserSay(Rooms.RoomUser User, string Message)
        {
            if (User.IsBot || User.GetClient() == null)
            {
                return;
            }
            if (User.GetClient().GetHabbo() == null)
            {
                return;
            }

            if (!User.GetClient().GetHabbo().CurrentRoom.RoomData.Description.Contains("HOSP"))
            {
                return;
            }

            if (Message != "curar" && Message != "medical aid" && Message != "aid")
            {
                return;
            }

            GameClients.GameClient Session = User.GetClient();

            if (Session.GetRoleplay().CurHealth < Session.GetRoleplay().MaxHealth || Session.GetRoleplay().Dead)
            {
                if (!Session.GetRoleplay().Dead)
                {
                    if (Session.GetRoleplay().BeingHealed)
                    {
                        Session.SendWhisper("You are already being healed!");
                        return;
                    }
                    else
                        base.GetRoomUser().Chat(null, "*Applies some bandages to " + User.GetClient().GetHabbo().UserName + "'s wounds*", true, 1);
                }
                else
                {
                    int yolo = (RoleplayManager.PurgeTime == true) ? 2 : 1;
                    if (Session.GetRoleplay().DeadTimer <= yolo)
                    {
                        base.GetRoomUser().Chat(null, "*Discharges " + Session.GetHabbo().UserName + " from the hospital*", true, 1);
                        Session.GetRoleplay().DeadTimer = 0;
                        Session.GetRoleplay().DeadSeconds = 0;
                        Session.GetRoleplay().SaveStatusComponents("dead");
                        Session.GetRoleplay().healTimer = new healTimer(Session);
                        Session.GetRoleplay().BeingHealed = true;
                        User.ApplyEffect(23);
                        return;
                    }
                    else
                    {
                        Session.SendWhisper("You must wait at least 1 minute before you can request to be released from the hospital!");
                        return;
                    }
                }

                Session.GetRoleplay().healTimer = new healTimer(Session);
                Session.GetRoleplay().BeingHealed = true;
                User.ApplyEffect(23);
                return;
            }
            else
                User.GetClient().SendWhisper("Your health is already full!");
            return;
                    
                    
                
                /*
                if (Gamemap.TileDistance(GetRoomUser().X, GetRoomUser().Y, User.X, User.Y) > 8)
                {
                    return;
                }

                BotResponse Response = GetBotData().GetResponse(Message);

                if (Response == null)
                {
                    return;
                }

                switch (Response.ResponseType.ToLower())
                {
                    case "say":

                        GetRoomUser().Chat(null, Response.ResponseText, false, 0);
                        break;

                    case "shout":

                        GetRoomUser().Chat(null, Response.ResponseText, true, 0);
                        break;

                    case "whisper":

                        User.GetClient().SendMessage(new WhisperComposer(GetRoomUser(), Response.ResponseText, 0, 0));
                        break;
                }

                if (Response.ServeId >= 1)
                {
                    User.CarryItem(Response.ServeId);
                }*/
            
        }

        internal override void OnUserShout(Rooms.RoomUser User, string Message)
        {
            /*
            if (PlusEnvironment.GetRandomNumber(0, 10) >= 5)
            {
                GetRoomUser().Chat(null, LanguageLocale.GetValue("onusershout"), true); // shout nag
            }*/
        }

        internal override void OnTimerTick()
        {
            if (base.GetBotData() != null)
            {

                if (base.GetBotData().Name.ToLower().Contains("doctor"))
                {

                    if (base.GetRoomUser().RotBody != 4)
                    {
                        base.GetRoomUser().RotBody = 4;
                        base.GetRoomUser().RotHead = 4;
                      
                    }

                    if (!base.GetRoomUser().Statusses.ContainsKey("sit"))
                    {
                        base.GetRoomUser().Statusses.Add("sit", "1.0");
                        base.GetRoomUser().Z = 0.00;
                        base.GetRoomUser().IsSitting = true;
                        base.GetRoomUser().UpdateNeeded = true;
                    }
                }

                switch (base.GetBotData().HospBotType)
                {

                    #region Paramedic Bot
                    case "paramedic_bot":
                        {

                            bool LeaveRoom = false;

                            if (base.GetBotData().InteractingWith == null)
                            {
                                LeaveRoom = true;
                            }

                            if (!base.GetBotData().InteractingWith.GetClient().GetRoleplay().Dead)
                            {
                                LeaveRoom = true;
                            }

                            if (base.GetBotData() != null)
                            {
                                if (base.GetBotData().InteractingWith != null)
                                {
                                    if (base.GetBotData().InteractingWith.GetClient() != null)
                                    {
                                        if (base.GetBotData().InteractingWith.GetClient().GetHabbo() != null)
                                        {
                                            if (base.GetBotData().InteractingWith.GetClient().GetHabbo().CurrentRoomId != base.GetBotData().RoomId)
                                            {
                                                LeaveRoom = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if(base.GetRoom() == null)
                            {
                                LeaveRoom = true;
                            }

                            #region Leaveroom
                            if (LeaveRoom)
                            {

                                if (LActionTimer < 20)
                                {
                                    base.GetRoomUser().ApplyEffect(20);
                                    List<Point> list = base.GetRoom().GetGameMap().getWalkableList();
                                    if (list.Count == 0)
                                        return;
                                    int randomNumber = new Random(DateTime.Now.Millisecond + this._virtualId ^ 2).Next(0, list.Count - 1);

                                    base.GetRoomUser().MoveTo(list[randomNumber].X, list[randomNumber].Y);
                                    LActionTimer++;
                                }
                                else
                                {

                                    RoleplayManager.KickBotFromRoom(base.GetRoomUser().BotData.Name, base.GetBotData().RoomId);
                                }


                            }
                            #endregion


                            if (!LeaveRoom)
                            {
                                base.GetRoomUser().ApplyEffect(20);


                                HabboHotel.PathFinding.Vector2D Pos1 = new HabboHotel.PathFinding.Vector2D(base.GetRoomUser().X, base.GetRoomUser().Y);
                                HabboHotel.PathFinding.Vector2D Pos2 = new HabboHotel.PathFinding.Vector2D(base.GetBotData().InteractingWith.X, base.GetBotData().InteractingWith.Y);

                                if (RoleplayManager.Distance(Pos1, Pos2) > 3)
                                {

                                    if (!base.GetRoomUser().IsWalking)
                                    {
                                        base.GetBotData().TimesTried++;
                                    }

                                    base.GetRoomUser().MoveTo(Pos2.X, Pos2.Y + 1, true);

                                    if (base.GetBotData().TimesTried >= 7)
                                    {
                                        base.GetRoomUser().SetPos(Pos2.X, Pos2.Y, 0);
                                        base.GetRoomUser().UpdateNeeded = true;
                                    }

                                }
                                else
                                {
                                    if (LActionTimer < 5)
                                    {
                                        LActionTimer++;
                                    }
                                    else
                                    {
                                        if (!LoadedUser)
                                        {
                                            base.GetRoomUser().Chat(null, "*Loads " + base.GetBotData().InteractingWith.GetClient().GetHabbo().UserName + " into the ambulance strecher*", true, 1);
                                            base.GetBotData().InteractingWith.ApplyEffect(23);
                                            LoadedUser = true;
                                        }
                                        else
                                        {
                                            if (LActionTimer < 10)
                                            {
                                                LActionTimer++;
                                            }
                                            else
                                            {
                                                if (!TransferedUser)
                                                {
                                                    base.GetBotData().InteractingWith.GetClient().GetMessageHandler().PrepareRoomForUser(2, null);
                                                    base.GetRoomUser().Chat(null, "*Takes " + base.GetBotData().InteractingWith.GetClient().GetHabbo().UserName + " to the hospital in their ambulance*", true, 1);
                                                    base.GetRoomUser().ApplyEffect(3);
                                                    TransferedUser = true;
                                                }
                                                else
                                                {
                                                    if (LActionTimer < 15)
                                                    {
                                                        if (LActionTimer == 13)
                                                        {
                                                            List<Point> list = base.GetRoom().GetGameMap().getWalkableList();
                                                            if (list.Count == 0)
                                                                return;
                                                            int randomNumber = new Random(DateTime.Now.Millisecond + this._virtualId ^ 2).Next(0, list.Count - 1);

                                                            base.GetRoomUser().MoveTo(list[randomNumber].X, list[randomNumber].Y, true);
                                                        }
                                                        LActionTimer++;
                                                    }
                                                    else
                                                    {
                                                        if (LActionTimer < 20)
                                                        {
                                                            base.GetRoomUser().ApplyEffect(20);
                                                            List<Point> list = base.GetRoom().GetGameMap().getWalkableList();
                                                            if (list.Count == 0)
                                                                return;
                                                            int randomNumber = new Random(DateTime.Now.Millisecond + this._virtualId ^ 2).Next(0, list.Count - 1);

                                                            base.GetRoomUser().MoveTo(list[randomNumber].X, list[randomNumber].Y);
                                                            LActionTimer++;
                                                        }
                                                        else
                                                        {
                                                            RoleplayManager.KickBotFromRoom(base.GetRoomUser().BotData.Name, base.GetBotData().RoomId);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                        }


                        break;

                    #endregion

                }
            }
        }

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
                    case ":sentar":
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
                    case ":levantar":
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