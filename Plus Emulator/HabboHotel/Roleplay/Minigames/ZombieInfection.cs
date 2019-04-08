using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.RoomInvokedItems;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.RoomBots;
using Plus.HabboHotel.Pets;
using Plus.Messages;
using System.Drawing;
using Plus.Util;
using System.Threading;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay;
using System.Text.RegularExpressions;
using System.Net;

namespace Plus.HabboHotel.Roleplay.Minigames
{
    public class ZombieInfection
    {
        public static void AlertParticipants(string sendmsg)
        {
            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
            {

                if (client == null)
                    continue;

                if (client.GetHabbo() == null)
                    continue;

                if (client.GetHabbo().CurrentRoom == null)
                    continue;

                if (client.GetRoleplay() == null)
                    continue;

                if (client.GetMessageHandler() == null)
                    continue;

                if (!client.GetRoleplay().inZombieInfection)
                    continue;


                client.SendNotifWithScroll(sendmsg);
            }

        }
        public static void StopGlobalInfection(bool tosendmsg = true)
        {

            try
            {

                string sendmsg = "";
                sendmsg += "==========================\nHotel Evento Iniciado!\n==========================\n";
                sendmsg += "Evento: Apocalipse Zumbi\n\n";
                sendmsg += "O Apocalipse zumbi foi finalizado!\n";

                RoleplayManager.ZombieInfection = false;

                foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                {

                    if (client == null)
                        continue;

                    if (client.GetHabbo() == null)
                        continue;

                    if (client.GetRoleplay() == null)
                        continue;

                    if (client.GetMessageHandler() == null)
                        continue;

                    if (tosendmsg)
                    {
                        client.SendNotifWithScroll(sendmsg);
                    }

                    if (client.GetRoleplay().Infected)
                    {
                        client.GetRoleplay().Infected = false;
                        if (client.GetRoleplay().FigBeforeSpecial != null)
                        {
                            client.GetHabbo().Look = client.GetRoleplay().FigBeforeSpecial;
                            client.GetHabbo().Motto = client.GetRoleplay().MottBeforeSpecial;
                        }

                        client.GetRoleplay().FigBeforeSpecial = null;
                        client.GetRoleplay().RefreshVals();
                    }

                    client.GetRoleplay().inZombieInfection = false;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error in attempt to stop zombie infection: " + e.StackTrace);
            }
        }
        public static void StartGlobalInfection()
        {

            try
            {
                double UserCount = Plus.GetGame().GetClientManager().ClientCount;
                int UserLimit = Convert.ToInt32(Math.Round(UserCount / 3));
                int zombieinfects = 0;

                RoleplayManager.ZombieInfection = true;

                string sendmsg = "";
                sendmsg += "==========================\nHotel Event Initialised!\n==========================\n";
                sendmsg += "Event: Zombie Infection\n\n";
                sendmsg += "-What is Zombie Infection?\n";
                sendmsg += "A group of randomly selected users have been infected with a new virus strain!";
                sendmsg += "They have been let out to roam the city and now they are going around infecting users! ";
                sendmsg += "It's your job to stop these Zombies, cure them and stop the disease from spreading entirely! ";
                sendmsg += "";
                sendmsg += "\n\nGood luck!";
                sendmsg += "\n\n";
                sendmsg += " Commands: \n";
                sendmsg += " - :bite <user> - If you are a zombie use this to bite/infect other users\n";
                sendmsg += " - :cure <user> - Cure a user of the zombie infection\n";
                sendmsg += "\n\n";
                sendmsg += " Points Distrubution / Rewards: \n";
                sendmsg += " - 1 point for biting a user\n";
                sendmsg += " - 1 point for curing a user\n";
                sendmsg += " - 15 points for stopping a zombie infection\n";
                sendmsg += " - 15 points for finalizing a zombie infection (i.e causing everyone in hotel to be infected)\n";

                foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                {

                    if (client == null)
                        continue;

                    if (client.GetHabbo() == null)
                        continue;

                    if (client.GetRoleplay() == null)
                        continue;

                    if (client.GetMessageHandler() == null)
                        continue;

                    if (HabboHotel.Roleplay.Jobs.JobManager.validJob(client.GetRoleplay().JobId, client.GetRoleplay().JobRank))
                    {
                        if (client.GetRoleplay().JobId == 3 || client.GetRoleplay().JobId == 5)
                        {
                            client.GetRoleplay().StopWork();
                        }
                    }


                    client.SendNotifWithScroll(sendmsg);

                    if (zombieinfects < UserLimit) //Set to 10 instead of UserLimit if this fails
                    {
                        client.SendNotifWithScroll("You were chosen as one of the Zombies to infect other users, quickly run around and bite users and spread the zombie infection, and receive a reward!!\n\n Commands: \n\n - :bite <user>\n");
                        client.GetRoleplay().MakeZombie();
                        client.GetRoleplay().Infected = true;
                        zombieinfects++;
                    }

                    client.GetRoleplay().inZombieInfection = true;

                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error in attempt to start zombie infection: " + e.StackTrace);
            }
        }
        public static void CheckForWin(GameClient Session)
        {

            #region Check Zombies Win
            int totalusers = 0;
            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
            {

                if (client == null)
                    continue;

                if (client.GetHabbo() == null)
                    continue;

                if (client.GetRoleplay() == null)
                    continue;

                if (client.GetMessageHandler() == null)
                    continue;

                if (client.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOTAXI"))
                    continue;

                if (!client.GetRoleplay().inZombieInfection)
                    continue;

                totalusers++;
            }


            int zombieusers = 0;

            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
            {

                if (client == null)
                    continue;

                if (client.GetHabbo() == null)
                    continue;

                if (client.GetRoleplay() == null)
                    continue;

                if (client.GetMessageHandler() == null)
                    continue;

                if (!client.GetRoleplay().Infected)
                    continue;

                zombieusers++;
            }

            if (zombieusers >= totalusers)
            {
                Session.GetRoleplay().Infection_Pts += 15;
                Session.GetRoleplay().SaveQuickStat("infection_pts", Session.GetRoleplay().Infection_Pts + "");
                Session.SendWhisper("You have been awarded 15 pts for finalizing the Zombie Infection!");
                AlertParticipants("The Zombie Infection is over and the Zombies have won thanks to " + Session.GetHabbo().UserName + "!");
                StopGlobalInfection();
            }
            else if(zombieusers <= 0)
            {
                Session.GetRoleplay().Infection_Pts += 15;
                Session.GetRoleplay().SaveQuickStat("infection_pts", Session.GetRoleplay().Infection_Pts + "");
                Session.SendWhisper("You have been awarded 15 pts for stopping the Zombie Infection!");
                AlertParticipants("The Zombie Infection is over and the Humans have won thanks to " + Session.GetHabbo().UserName + "!");
                StopGlobalInfection();
            }
            #endregion

        }
    }
}
