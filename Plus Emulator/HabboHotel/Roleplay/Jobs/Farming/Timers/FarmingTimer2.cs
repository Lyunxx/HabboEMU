using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.PathFinding;

namespace Plus.HabboHotel.Roleplay.Jobs.Farming
{
    internal class farmingTimer2
    {
        public farmingTimer2(GameClient theHabbo, FarmingSpot farmingspot)
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

            Misc.RoleplayManager.Shout(Session, "*Grabs their watering can and begins watering the plant, almost ready to harvest*", 4);
            Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(8);
            farmingspot.beingFarmed2 = true;
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
                                farmingspot.Part1Complete = false;
                                var respawnTimer = new respawnTimer(farmingspot.id, farmingspot.x, farmingspot.y, farmingspot.roomid, farmingspot.type);
                                respawnTimer.startTimer();

                                farmingManager.removeFarmingSpot(farmingspot, Session, room);
                                timer.Dispose();

                                Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                                Misc.RoleplayManager.Shout(Session, "*Grabs a scythe and quickly harvests their plant*", 6);

                                xpManagerFarming.addXP(Session);
                                if (farmingspot.type == "weed")
                                {
                                    Session.GetRoleplay().Weed += 2;
                                    Session.GetRoleplay().SaveQuickStat("weed", "" + Session.GetRoleplay().Weed);
                                    Session.SendWhisper("You have farmed 2 more weed buds. Your weed count is now: " + Session.GetRoleplay().Weed + "");
                                }
                                if (farmingspot.type == "carrot")
                                {
                                    Session.GetRoleplay().Carrots += 2;
                                    Session.GetRoleplay().SaveQuickStat("carrots", "" + Session.GetRoleplay().Carrots);
                                    Session.SendWhisper("You have farmed 2 more carrots. Your carrot count is now: " + Session.GetRoleplay().Carrots + "");
                                }
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
                        Misc.RoleplayManager.Shout(Session, "*Stops watering the plant*", 3);
                        Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                        farmingspot.beingFarmed2 = false;
                        timer.Dispose();
                    }
                }
                catch
                {
                    farmingspot.beingFarmed2 = false;
                    timer.Dispose();
                }
            }
            else
            {
                farmingspot.beingFarmed2 = false;
                timer.Dispose();
            }
        }
    }
}