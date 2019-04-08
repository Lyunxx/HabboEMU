using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.PathFinding;

namespace Plus.HabboHotel.Roleplay.Jobs.Farming
{
    internal class farmingTimer1
    {
        public farmingTimer1(GameClient theHabbo, FarmingSpot farmingspot)
        {
            this.farmingspot = farmingspot;
            Session = theHabbo;
            timeleft = 60000;
        }

        private readonly FarmingSpot farmingspot;
        private Timer timer;
        private readonly GameClient Session;

        private int timeleft;

        public void startTimer()
        {
            var autoRE = new AutoResetEvent(false);
            TimerCallback timerCB = timerDone;

            timer = new Timer(timerCB, autoRE, 2000, Timeout.Infinite);

            Misc.RoleplayManager.Shout(Session, "*Grabs their trowel and begins making room for a seed*", 4);
            Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(8);
            farmingspot.beingFarmed = true;
        }

        public void timerDone(object info)
        {
            if (Session != null && Session.GetHabbo() != null)
            {
                try
                {
                    var user = Session.GetHabbo().GetRoomUser();
                    var room = Misc.RoleplayManager.GenerateRoom(farmingspot.roomid);

                    if (Misc.RoleplayManager.Distance(new Vector2D(farmingspot.x, farmingspot.y), new Vector2D(user.X, user.Y)) <= 1 && user.RoomId == farmingspot.roomid)
                    {
                        if (timeleft <= 0)
                        {
                            if (farmingspot != null)
                            {
                                Misc.RoleplayManager.Shout(Session, "*Finishes planting their seed into the dirt pile*", 4);

                                var FarmingTimer2 = new farmingTimer2(Session, farmingspot);
                                FarmingTimer2.startTimer();

                                farmingManager.updateFarmingSpot(farmingspot, Session, room);
                                timer.Dispose();

                                Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                                //Session.SendWhisper("Get ready to start watering the plant!");

                                xpManagerFarming.addXP(Session);
                                farmingspot.Part1Complete = true;
                            }
                        }
                        else
                        {

                            timeleft -= 2000;
                            timer.Change(2000, Timeout.Infinite);
                        }
                    }
                    else
                    {
                        Misc.RoleplayManager.Shout(Session, "*Stops planting the seed*", 3);
                        Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                        farmingspot.beingFarmed = false;
                        timer.Dispose();
                    }
                }
                catch
                {
                    farmingspot.beingFarmed = false;
                    timer.Dispose();
                }
            }
            else
            {
                farmingspot.beingFarmed = false;
                timer.Dispose();
            }
        }
    }
}