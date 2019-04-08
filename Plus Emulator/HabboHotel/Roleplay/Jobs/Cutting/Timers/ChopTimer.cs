#region Librarys

using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.PathFinding;

#endregion

namespace Plus.HabboHotel.Roleplay.Jobs.Cutting
{
    internal class chopTimer
    {
        #region Constructor

        public chopTimer(GameClient theHabbo, Tree tree)
        {
            this.tree = tree;
            Session = theHabbo;
            timeleft = 60000;
        }

        #endregion

        #region Declares

        private readonly Tree tree;
        private Timer timer;
        private readonly GameClient Session;

        private int timeleft;

        #endregion

        #region Methods

        public void startTimer()
        {
            var autoRE = new AutoResetEvent(false);
            TimerCallback timerCB = timerDone;

            timer = new Timer(timerCB, autoRE, 2000, Timeout.Infinite);

            Misc.RoleplayManager.Shout(Session, "*Grabs their axe and begins hitting the tree*", 4);
            Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(5);
            tree.beingMined = true;
        }

        public void timerDone(object info)
        {
            if (Session != null && Session.GetHabbo() != null)
            {
                try
                {
                    var user = Session.GetHabbo().GetRoomUser();
                    var room = Misc.RoleplayManager.GenerateRoom(tree.roomid);

                    if (Misc.RoleplayManager.Distance(new Vector2D(tree.x, tree.y), new Vector2D(user.X, user.Y)) <= 2 &&
                        user.RoomId == tree.roomid)
                    {
                        if (timeleft <= 0)
                        {
                            if (tree != null)
                            {
                                var respawntree = new respawnTimer(tree.id, tree.x, tree.y, tree.roomid, tree.data);
                                respawntree.startTimer();

                                woodManager.removeTree(tree, Session, room);
                                timer.Dispose();

                                Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                                Misc.RoleplayManager.Shout(Session, "*Breaks the tree into timber, collecting the wood*", 4);
                                Session.SendWhisper(
                                    "The wood has been added to your inventory! Go to the sell point and type :sell wood.");

                                xpManagerCutting.addXP(Session);
                            }
                        }

                        else
                        {
                            

                            if (timeleft == 50000)
                            {
                                Misc.RoleplayManager.Shout(Session, "*Swings their axe into the tree*", 4);
                            }
                            else if (timeleft == 40000)
                            {
                                Misc.RoleplayManager.Shout(Session, "*Swings their axe into the tree*", 4);
                            }
                            else if (timeleft == 30000)
                            {
                                Misc.RoleplayManager.Shout(Session, "*Swings their axe into the tree, chipping away a chunk*", 4);
                            }
                            else if (timeleft == 20000)
                            {
                                Misc.RoleplayManager.Shout(Session, "*Swings their axe into the tree, chipping away a bigger chunk*", 4);
                            }
                            else if(timeleft == 10000){
                                Misc.RoleplayManager.Shout(Session, "*Swings their axe into the tree, the tree begins to fall!*", 4);
                            }

                            timeleft -= 2000;
                            timer.Change(2000, Timeout.Infinite);
                        }
                    }
                    else
                    {
                        Misc.RoleplayManager.Shout(Session, "*Stopped cutting the tree down*", 4);
                        Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                        tree.beingMined = false;
                        timer.Dispose();
                    }
                }
                catch
                {
                    tree.beingMined = false;
                    timer.Dispose();
                }
            }
            else
            {
                tree.beingMined = false;
                timer.Dispose();
            }
        }

        #endregion
    }
}