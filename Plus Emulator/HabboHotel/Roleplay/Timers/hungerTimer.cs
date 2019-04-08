using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class hungerTimer
    {
        Timer timer;
        GameClient Session;
        int timeLeft; // 3 minutes (milliseconds)

        public hungerTimer(GameClient Session)
        {
            this.Session = Session;

            int time = 5;
            timeLeft = time * 60000;

            startTimer();
        }

        public void startTimer()
        {
            TimerCallback timerFinished = timerDone;

            timer = new Timer(timerFinished, null, 60000, Timeout.Infinite);
        }

        public void timerDone(object info)
        {
            try
            {
                timeLeft -= 60000;

                #region Conditions
                if (Session == null)
                { stopTimer(); return; }

                if (timeLeft > 0)
                {
                    if (Session.GetRoleplay().Hunger >= 100)
                    {
                        Random rand = new Random();
                        int Randhealth = rand.Next(5, 9);
                        int Randenergy = rand.Next(5, 10);
                        if (Session.GetRoleplay().CurHealth - Randhealth > 0)
                        {
                            Session.GetRoleplay().CurHealth -= Randhealth;
                            if (Session.GetRoleplay().Energy - Randenergy >= 0) { Session.GetRoleplay().Energy -= Randenergy; }
                            Session.SendWhisper("You have lost some energy and health from lack of food, get some quick before you die!");
                            Session.SendWhisper("Food can be found at the following room: 24");
                            Session.GetRoleplay().CurHealth -= 10;
                            Session.GetRoleplay().UpdateStats++;
                            RoleplayManager.MakeLay(Session.GetHabbo().GetRoomUser());
                        }
                        else
                        {
                            Session.GetRoleplay().UpdateStats++;
                            Session.SendNotif("You have died from starvation!");
                            if (Session.GetRoleplay().Working)
                            {
                                Session.GetRoleplay().StopWork();
                            }
                            Session.GetRoleplay().Hunger = 0;
                            Session.GetRoleplay().DeadFigSet = false;
                            Session.GetRoleplay().DeadSeconds = 60;
                            Session.GetRoleplay().DeadTimer = 2;
                            Session.GetRoleplay().Dead = true;
                            RoleplayManager.HandleDeath(Session);
                        }
                    }
                    timer.Change(60000, Timeout.Infinite);
                    return;
                }

                #endregion

                #region Execute

                if (Session.GetRoleplay().Hunger < 100)
                {
                    if (Session.GetRoleplay().Hunger + 2 <= 100)
                    {
                        Session.GetRoleplay().UpdateStats++;
                        Session.GetRoleplay().Hunger += 2;
                    }
                    else
                    {
                        Session.GetRoleplay().Hunger = 100;
                    }
                }
                Session.GetRoleplay().SaveQuickStat("hunger", "" + Session.GetRoleplay().Hunger);

                stopTimer();

                Session.GetRoleplay().HungerDecrement = false;
                #endregion
            }
            catch { stopTimer(); }
        }

        public int getTime()
        {
            int minutesRemaining = timeLeft / 60000;
            return minutesRemaining;
        }

        public void stopTimer()
        {
            timer.Dispose();
        }
    }
}
