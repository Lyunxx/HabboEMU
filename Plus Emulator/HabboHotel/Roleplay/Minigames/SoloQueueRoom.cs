using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Roleplay.Minigames
{
    public class SoloQueueRoom
    {
        /// <summary>
        /// Room instance
        /// </summary>
        public Room MyRoom;

        /// <summary>
        /// Dictionary containing boxers
        /// </summary>
        public Dictionary<GameClient, string> Boxers;

        /// <summary>
        /// Queue
        /// </summary>
        public LinkedList<GameClient> PlayerQueue;

        /// <summary>
        /// Count down timer
        /// </summary>
        private Timer CountDown;

        /// <summary>
        /// Time left
        /// </summary>
        private int TimeLeft = 3;

        /// <summary>
        /// Constructor
        /// </summary>
        public SoloQueueRoom(Room MyRoom)
        {
            this.MyRoom = MyRoom;
            this.Boxers = new Dictionary<GameClient, string>();
            this.PlayerQueue = new LinkedList<GameClient>();
        }

        /// <summary>
        /// Add user to queue
        /// </summary>
        public void AddToQueue(GameClient Session)
        {
            if (this.PlayerQueue == null || Session == null || this.MyRoom == null) return;
            if (this.PlayerQueue.Contains(Session)) return;

            this.PlayerQueue.AddLast(Session);

            Session.GetRoleplay().IsBoxing = true;
            Session.GetRoleplay().BoxingRoom = MyRoom;
            Session.GetRoleplay().figBeforeWar = Session.GetHabbo().Look;

            if (this.Boxers.Count > 0)
            {
                int PlayersAhead = this.PlayerQueue.Count - 1;
                Session.SendWhisper("You were added to the queue! [" + PlayersAhead + " Players ahead of you]");
                return;
            }

            if (this.PlayerQueue.Count < 2)
            {
                Session.SendWhisper("Waiting for an opponent...");
                return;
            }

            TryStart();
        }

        /// <summary>
        /// Add a boxer
        /// </summary>
        public void AddBoxer(GameClient Session)
        {
            if (Session == null || this.MyRoom == null || this.Boxers == null) return;
            if (this.Boxers.Count >= 2 || this.Boxers.ContainsKey(Session)) return;

            string Colour = this.GetColour();
            string Figure = this.GetFigure();
            var Habbo = Session.GetHabbo();
            var Roleplay = Session.GetRoleplay();

            if (Habbo == null || Roleplay == null) return;

            this.Boxers.Add(Session, Colour);
            Roleplay.IsBoxing = true;
            Roleplay.BoxingRoom = MyRoom;

            Roleplay.figBeforeWar = Habbo.Look;
            Roleplay.CurHealth = Roleplay.MaxHealth;
            Roleplay.Energy = 100;
            Habbo.Look = RoleplayManager.SplitFigure(Habbo.Look) + Figure;
            Roleplay.RefreshVals();

            if (this.Boxers.Count < 2) return;

            StartMatch();
        }

        /// <summary>
        /// Removes a user from boxing match & ends it
        /// </summary>
        public void RemoveBoxer(GameClient Session, bool LoggingOut = false, bool ChangedRoom = false, bool End = true)
        {

            if (PlayerQueue.Contains(Session))
                PlayerQueue.Remove(Session);

            if (Boxers.ContainsKey(Session))
                Boxers.Remove(Session);

            if (Session == null || Session.GetHabbo() == null || Session.GetRoleplay() == null)
                return;

            Session.GetRoleplay().IsBoxing = false;
            Session.GetHabbo().Look = Session.GetRoleplay().figBeforeWar;
            Session.GetRoleplay().CurHealth = Session.GetRoleplay().MaxHealth;
            Session.GetRoleplay().Energy = 100;

            if (!LoggingOut)
            {
                Session.GetRoleplay().RefreshVals();

                if (!ChangedRoom)
                {
                    if (Session.GetHabbo().GetRoomUser() == null) return;
                    Session.GetHabbo().GetRoomUser().SetPos(MyRoom.RoomData.Model.DoorX, MyRoom.RoomData.Model.DoorY, 0);
                    Session.GetHabbo().GetRoomUser().ClearMovement();
                }
            }

            if (End)
                EndMatch();
        }

        /// <summary>
        /// Checks to see if we can start the match
        /// </summary>
        public void TryStart()
        {
            if (this.PlayerQueue.Count < 2)
            {
                return;
            }

            GameClient FirstPlayer = this.PlayerQueue.First.Value;
            this.PlayerQueue.RemoveFirst();

            GameClient SecondPlayer = this.PlayerQueue.First.Value;
            this.PlayerQueue.RemoveFirst();

            if (FirstPlayer == null || SecondPlayer == null)
            { TryStart(); return; }

            this.AddBoxer(FirstPlayer);
            this.AddBoxer(SecondPlayer);

            MessageRoom("[BOXING] " + FirstPlayer.GetHabbo().UserName + " vs " + SecondPlayer.GetHabbo().UserName);
        }

        /// <summary>
        /// Starts the fight
        /// </summary>
        public void StartMatch()
        {
            foreach (KeyValuePair<GameClient, string> KvP in this.Boxers)
            {
                var Session = KvP.Key;
                var RoomUser = Session.GetHabbo().GetRoomUser();
                string Colour = KvP.Value;

                if (Session == null || RoomUser == null || Session.GetRoleplay() == null || Session.GetHabbo() == null)
                    return;

                Point FightPos = this.GetFightPos(Colour);
                RoomUser.SetPos(FightPos.X, FightPos.Y, 0);
                RoomUser.ClearMovement();

                RoomUser.Freezed = true;
                RoomUser.CanWalk = false;
            }

            CountDown = new Timer(CountdownTicked, null, 1000, 1000);
        }

        private void CountdownTicked(object RoomUser)
        {
            try
            {
                foreach (KeyValuePair<GameClient, string> KvP in this.Boxers)
                {
                    if (KvP.Key == null || KvP.Key.GetHabbo() == null) continue;

                    RoomUser RUser = KvP.Key.GetHabbo().GetRoomUser();
                    if (RUser != null)
                    {
                        if (TimeLeft <= 0)
                        {
                            RUser.GetClient().SendWhisper("FIGHT!");
                            RUser.Freezed = false;
                            RUser.CanWalk = true;

                            continue;
                        }

                        KvP.Key.SendWhisper(TimeLeft.ToString());

                    }
                }

                if (CountDown == null)
                {
                    foreach (KeyValuePair<GameClient, string> KvP in this.Boxers)
                    {
                        if (KvP.Key == null || KvP.Key.GetHabbo() == null) continue;

                        RoomUser RUser = KvP.Key.GetHabbo().GetRoomUser();
                        if (RUser != null)
                        {

                            RUser.GetClient().SendWhisper("FIGHT!");
                            RUser.Freezed = false;
                            RUser.CanWalk = true;
                        }
                    }

                    return;
                }

                if (TimeLeft > 0)
                {
                    TimeLeft--;
                    CountDown.Change(1000, 1000);
                    return;
                }

                TimeLeft = 3;
                CountDown.Change(Timeout.Infinite, Timeout.Infinite);
                CountDown.Dispose();
                CountDown = null;
            }
            catch { }
        }

        /// <summary>
        /// End the match & look for new one
        /// </summary>
        public void EndMatch(string WinnerName = "", bool Winner = false)
        {
            foreach (GameClient Session in this.Boxers.Keys.ToList())
            {
                if (Session == null) return;
                if (Session.GetRoleplay() != null) Session.GetRoleplay().IsBoxing = false;

                RemoveBoxer(Session, false, false, false);
            }

            this.Boxers.Clear();
            if (Winner)
                MessageRoom("[BOXING] " + WinnerName + " won the round!");

            TryStart();
        }

        public void MessageRoom(string Msg)
        {
            if (MyRoom == null) return;
            lock (MyRoom.GetRoomUserManager().UserList.Values)
            {
                foreach (RoomUser RUser in MyRoom.GetRoomUserManager().UserList.Values.ToList())
                {
                    if (RUser == null) continue;
                    GameClient RUSession = RUser.GetClient();
                    if (RUSession == null) continue;

                    RUSession.SendWhisper(Msg);
                }
            }
        }

        /// <summary>
        /// Gets le figure
        /// </summary>
        public string GetFigure()
        {
            if (this.Boxers.ContainsValue("blue"))
                return "ha-3352-101-101.sh-290-101.lg-275-101.ch-3030-101";

            return "ha-3352-106-106.lg-275-106.sh-290-106.ch-3030-106";
        }

        /// <summary>
        /// Get position to start
        /// </summary>
        public Point GetFightPos(string Colour)
        {
            RoomItem Green = RoleplayManager.GetNearItem("doormat_plain*7", MyRoom);
            RoomItem Blue = RoleplayManager.GetNearItem("doormat_plain*5", MyRoom);

            if (Green == null || Blue == null)
                return new Point(5, 5);

            // RoleplayManager.GetNearItem
            if (Colour == "green")
                return new Point(Green.X, Green.Y);
            else
                return new Point(Blue.X, Blue.Y);
        }

        /// <summary>
        /// Gets boxers colour
        /// </summary>
        public string GetColour()
        {
            if (this.Boxers.ContainsValue("blue"))
                return "green";

            return "blue";
        }
    }
}
