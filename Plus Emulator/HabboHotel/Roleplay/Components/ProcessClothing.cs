using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Quests.Composer;
using System.Threading.Tasks;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.RoomInvokedItems;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.RoomBots;
using Plus.HabboHotel.Pets;
using Plus.Messages;
using Plus.Messages.Parsers;
using Plus.Database;
using System.Drawing;
using Plus.Util;
using System.Threading;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay;
using Plus.HabboHotel.Roleplay.Timers;
using Plus.HabboHotel.Roleplay.Radio;
using System.Text.RegularExpressions;
using System.Net;
using Plus.Database.Manager.Database.Session_Details.Interfaces;

namespace Plus.HabboHotel.Roleplay.Components
{
    public class ProcessClothing
    {

        /// <summary>
        /// Timer for our operation
        /// </summary>
        private Timer Timer;
        public bool On = false;
        public int ChangeTime;
        public int ElapsedChangeTime;
        public GameClient Session;
        public RoomItem Booth;
        public bool blnWarned = false;
        public string NewClothing;
        public string Gender;
        public bool Changed;
        public bool hairOnly;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessClothing(GameClient Session, string NewClothing, string Gender, bool blnHairOnly = false)
        {
            this.hairOnly = blnHairOnly;
            this.Gender = Gender;
            this.NewClothing = NewClothing;
            this.Session = Session;
            this.LocateBooth();
            this.ChangeTime = Convert.ToInt32(RoleplayData.Data["roleplay.change.time"]);
            this.ElapsedChangeTime = 0;
            this.Changed = false;

            // Method to call when completed
            TimerCallback TimerCallback = Ticked;

            // Create a new instance of timer
            Timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);
        }

        /// <summary>
        /// Method is call when timer is finished
        /// </summary>
        /// <param name="info">The information</param>
        public void Ticked(object info)
        {
            try
            {
                {
                    if (isNull())
                    { stopProcess("Something was null!"); return; }

                    // Check if booth was found
                   
                    if (LocatedBooth())
                    {

                        if (!Session.GetRoleplay().Changing) { stopProcess(""); return; }

                        
                        if (!InBooth())
                        {
                            WalkToBooth();
                        }
                        else
                        {
                            if (!Changed)
                            {

                                if (!Session.GetRoleplay().Changing) { Session.GetRoleplay().Changing = true; }

                                if (ElapsedChangeTime < ChangeTime)
                                {
                                    ElapsedChangeTime++;
                                }
                                else
                                {
                                    ProcessChange();
                                    Changed = true;
                                }
                            }
                            else
                            {
                                if (InBooth())
                                {
                                    WalkOutBooth();
                                    stopProcess("");
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        LocateBooth();
                        Timer.Change(1000, Timeout.Infinite);
                        return;
                    }

                    Timer.Change(1000, Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.StackTrace + "|" + e.ToString());
            }
        }
        public bool isNull()
        {
            if (Session == null)
            {
               // Console.WriteLine("Session was null!");
                return true;
            }
            if (Session.GetHabbo() == null)
            {
                //Console.WriteLine("Session.GetHabbo() was null!");
                return true;
            }
            if (Session.GetHabbo().CurrentRoom == null)
            {
                //Console.WriteLine("Session.GetHabbo().CurrentRoom was null!");
                return true;
            }
            if (Session.GetRoleplay() == null)
            {
                //Console.WriteLine("Session.GetRoleplay() was null!");
                return true;
            }
            if (Session.GetHabbo().GetRoomUser() == null)
            {
                //Console.WriteLine("Session.GetHabbo().GetRoomUser() was null!");
                return true;
            }
            if (Booth != null)
            {
                if (Booth.RoomId != Session.GetHabbo().GetRoomUser().RoomId)
                {
                    //Console.WriteLine("Sessionleft room!");
                    return true;
                }
            }
            return false;
        }
        public void LocateBooth()
        {
            if (isNull())
            {
                return;
            }
            string BoothFurni;
            if (hairOnly)
            {
            BoothFurni = RoleplayData.Data["roleplay.furni.chaircut"];
            }else{
            BoothFurni = RoleplayData.Data["roleplay.furni.cbooth"];
            }

            ICollection<RoomItem> Items = Session.GetHabbo().CurrentRoom.GetRoomItemHandler().FloorItems.Values;

            lock (Items)
            {

                List<RoomItem> Booths = new List<RoomItem>();


                    foreach (RoomItem Item in Items)
                    {
                        if (Item == null)
                            continue;
                        if (Item.GetBaseItem() == null)
                            continue;
                        if (!Item.GetBaseItem().Name.ToLower().Contains(BoothFurni))
                            continue;
                        if (Item.InteractingUser > 0)
                            continue;

                        Booths.Add(Item);
                    }
                    if (Booths.Count() > 0)
                    {
                        Booth = Booths.First();
                        Booth.InteractingUser = Session.GetHabbo().Id;
                    }
                    else
                    {
                        if (blnWarned == false)
                        {
                            if (hairOnly)
                            {
                                Session.SendWhisper("É preciso haver uma cadeira de estilo na sala para se obter estilo");
                                blnWarned = true;
                            }else {
                                Session.SendWhisper("É preciso haver um estande de mudança na sala para mudar");
                                blnWarned = true;
                            }
                            
                        }
                        return;
                    }
            }
        }
        public bool LocatedBooth()
        {
            if (Booth != null)
                return true;

            return false;
        }
        public void WalkToBooth()
        {

            RoomUser RoomUser = Session.GetHabbo().GetRoomUser();
            RoomUser.MoveTo(new Point(Booth.X, Booth.Y));

        }
        public void WalkOutBooth()
        {
            RoomUser RoomUser = Session.GetHabbo().GetRoomUser();
            RoomUser.MoveTo(new Point(Booth.SquareInFront.X, Booth.SquareInFront.Y));

        }
        public void ProcessChange()
        {

            string text2 = NewClothing;
            string text = Gender;

            Plus.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.ProfileChangeLook, 0u);
            Session.GetHabbo().Look = text2;
            Session.GetHabbo().Gender = text.ToLower();
            using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("UPDATE users SET look = @look, gender = @gender WHERE id = {0}",
                    Session.GetHabbo().Id));
                queryReactor.AddParameter("look", text2);
                queryReactor.AddParameter("gender", text);
                queryReactor.RunQuery();
            }
            Plus.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_AvatarLooks", 1, false);
            if (Session.GetHabbo().Look.Contains("ha-1006"))
                Plus.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.WearHat, 0u);
            Session.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("UpdateAvatarAspectMessageComposer"));
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToUpper());
            Session.GetMessageHandler().SendResponse();
            Session.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            Session.GetMessageHandler().GetResponse().AppendInteger(-1);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToLower());
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Motto);
            Session.GetMessageHandler().GetResponse().AppendInteger(Session.GetHabbo().AchievementPoints);
            Session.GetMessageHandler().SendResponse();
            if (!Session.GetHabbo().InRoom)
                return;
            var currentRoom = Session.GetHabbo().CurrentRoom;
            if (currentRoom == null)
                return;
            var roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            serverMessage.AppendInteger(roomUserByHabbo.VirtualId); //BUGG
            //serverMessage.AppendInt32(-1);
            serverMessage.AppendString(Session.GetHabbo().Look);
            serverMessage.AppendString(Session.GetHabbo().Gender.ToLower());
            serverMessage.AppendString(Session.GetHabbo().Motto);
            serverMessage.AppendInteger(Session.GetHabbo().AchievementPoints);
            currentRoom.SendMessage(serverMessage);

            if (Session.GetHabbo().GetMessenger() != null) Session.GetHabbo().GetMessenger().OnStatusChanged(true);

            Session.Shout("*Muda de roupa/penteado e paga pelo serviço [-$2]*");
            Roleplay.Misc.RoleplayManager.GiveMoney(Session, -2);
        }
        public bool InBooth()
        {
            RoomUser RoomUser = Session.GetHabbo().GetRoomUser();
            if (Booth.X == RoomUser.X && Booth.Y == RoomUser.Y)
                return true;

            return false;
        }
        public void stopProcess(string error)
        {
            //running = false

            if (RoleplayData.Data["debug.show.stop.timer.msg"] == "true")
            {
                Console.WriteLine("process was stopped:: " + error);
            }

            if (!isNull())
            {
                Session.GetRoleplay().Changing = false;
            }

            if(Booth != null)
            {
                Booth.InteractingUser = 0;
            }


            On = false;
            ChangeTime = 0;
            ElapsedChangeTime = 0;
            Session = null;
            Booth = null;

            NewClothing = null;
            Gender = null;
            Changed = false;

            Timer.Dispose();
            return;
        }
    }
}
