#region Librarys

using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.PathFinding;

#endregion

namespace Plus.HabboHotel.Roleplay.Jobs.Space
{
    internal class mineTimer
    {
        #region Constructor

        public mineTimer(GameClient theHabbo, Rock rock)
        {
            this.rock = rock;
            Session = theHabbo;
            timeleft = 60000;
        }

        #endregion

        #region Declares

        private readonly Rock rock;
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

            Misc.RoleplayManager.Shout(Session, "*Grabs their sledge hammer and begins smashing the rock*");
            Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(8);
            rock.beingMined = true;
        }

        public void timerDone(object info)
        {
            if (Session != null && Session.GetHabbo() != null)
            {
                try
                {
                    var user = Session.GetHabbo().GetRoomUser();
                    var room = Misc.RoleplayManager.GenerateRoom(rock.roomid);

                    if (Misc.RoleplayManager.Distance(new Vector2D(rock.x, rock.y), new Vector2D(user.X, user.Y)) <= 1 &&
                        user.RoomId == rock.roomid)
                    {
                        if (timeleft <= 0)
                        {
                            if (rock != null)
                            {
                                var respawnRock = new respawnTimer(rock.id, rock.x, rock.y, rock.roomid, rock.data);
                                respawnRock.startTimer();

                                spaceManager.removeRock(rock, Session, room);
                                timer.Dispose();

                                Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                                Misc.RoleplayManager.Shout(Session, "*Breaks into the rock and collects all of the ores*");
                                Session.SendWhisper(
                                    "The rock ores have been added to your inventory! Go to the sell point and type :sell rocks.");

                                xpManagerSpace.addXP(Session);
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
                        Misc.RoleplayManager.Shout(Session, "*Stops mining the rock*");
                        Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                        rock.beingMined = false;
                        timer.Dispose();
                    }
                }
                catch
                {
                    rock.beingMined = false;
                    timer.Dispose();
                }
            }
            else
            {
                rock.beingMined = false;
                timer.Dispose();
            }
        }

        #endregion
    }
}