#region Librarys

using System;
using System.Security.Policy;
using System.Threading;
using Plus.Connection.ServerManager;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Wired.Handlers.Conditions;

#endregion

namespace Plus.HabboHotel.Roleplay.Jobs.Space
{
    internal class oxygenTimer
    {
        #region Declares

        private Timer timer;
        private uint roomID;

        #endregion

        #region Constructor

        public oxygenTimer(uint roomid)
        {
            this.roomID = roomid;
        }

        #endregion

        #region Methods

        public void startTimer()
        {
            var autoRE = new AutoResetEvent(false);

            TimerCallback timerCB = timerDone;

            timer = new Timer(timerCB, autoRE, 3000, Timeout.Infinite);
        }

        public void timerDone(object info)
        {
           checkOxygen();
        }


        public void checkOxygen()
        {
            try
            {
                if (spaceManager.initiated)
                {
                    Room room = Misc.RoleplayManager.GenerateRoom(roomID);

                    foreach (RoomUser User in room.GetRoomUserManager().GetRoomUsers())
                    {
                        if (User != null && User.RoomId == roomID)
                        {
                            GameClient Session = Misc.RoleplayManager.GenerateSession(User.GetUserName());

                            foreach (var Item in room.GetRoomItemHandler().FloorItems.Values)
                            {

                                if (Item == null)
                                    continue;

                                if (room == null)
                                    continue;

                                if (Session != null && Misc.RoleplayManager.isOnMoon(Item, User, Session))

                                {
                                    if (
                                        !JobManager.JobRankData[Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank]
                                            .hasRights("spaceminer") || Session.GetRoleplay().Working == false)
                                    {
                                        Session.SendWhisper(
                                            "You are dying from lack of oxygen as you are not wearing a space suit! ;startwork if you're a space miner! A");
                                        startKilling(Session, User);
                                    }
                                }
                            }
                        }
                    }
                    timer.Change(8000, Timeout.Infinite);
                }
                else
                    timer.Dispose();
            }
            catch
            {

            }
        }

        public void startKilling(GameClient Session, RoomUser RUser)
        {
            if (!Session.GetRoleplay().Dead)
            {
                Random rand = new Random();
                int health = rand.Next(40, 70);
                if (Session.GetRoleplay().CurHealth - health > 0)
                {
                    Session.GetRoleplay().CurHealth -= health;
                    Session.GetRoleplay().UpdateStats++;
                }
                else
                {
                    GameClient TargetSession = Session;

                    Session.Shout("*Dies from lack of oxygen*");

                    Session.SendNotif("You died from lack of oxygen! Always wear a space suit when in space!");

                    TargetSession.GetRoleplay().DeadFigSet = false;
                    TargetSession.GetRoleplay().DeadSeconds = 60;
                    TargetSession.GetRoleplay().DeadTimer = 2;
                    TargetSession.GetRoleplay().Dead = true;
                    TargetSession.GetRoleplay().SaveStatusComponents("dead");
                    RoleplayManager.HandleDeath(TargetSession);

                }
            }
        }
        #endregion
    }
}