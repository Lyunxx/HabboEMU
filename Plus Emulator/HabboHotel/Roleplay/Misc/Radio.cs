using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.Messages;
using System.Drawing;
using Plus.Util;
using System.Threading;
using System.Text.RegularExpressions;
using Plus.HabboHotel.Roleplay.Misc;
using System.Net;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.Roleplay.Jobs;

namespace Plus.HabboHotel.Roleplay.Radio
{
    public class Radio
    {
        public static string canUseDept(int jobid, int jobrank)
        {
            if (JobManager.JobRankData[jobid, jobrank].hasRights("police"))
            {
                return "POLICE";
            }
            else if (JobManager.JobRankData[jobid, jobrank].hasRights("swat"))
            {
                return "S.W.A.T";
            }
            //else if (JobManager.JobRankData[jobid, jobrank].hasRights("hospital"))
            //{
            //    return "HOSP";
            //}
            else if (JobManager.JobRankData[jobid, jobrank].hasRights("gov"))
            {
                return "STAFF";
            }
            //else if (JobManager.JobRankData[jobid, jobrank].hasRights("court"))
            //{
            //    return "COURT";
            //}
            //else if (JobManager.JobRankData[jobid, jobrank].hasRights("BANK"))
            //{
            //    return "BANK";
            //}
            else
            {
                return "NO";
            }
        }
        public static void send(string msg, GameClient user, bool Dispatch = false)
        {
            string permm;
            if (!Dispatch)
                permm = canUseDept(user.GetRoleplay().JobId, user.GetRoleplay().JobRank);
            else
                permm = "yes";
            if (permm != "NO")
            {
                lock (Plus.GetGame().GetClientManager().Clients.Values)
                {
                    foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                    {
                        try
                        {
                            if (client == null)
                                continue;

                            string perm = canUseDept(client.GetRoleplay().JobId, client.GetRoleplay().JobRank);
                            if (Dispatch)
                            {

                                if (perm != "NO" && !client.GetRoleplay().RadioOff)
                                {
                                    client.GetHabbo().GetRoomUser().LastBubble = 30;
                                    client.SendWhisper("<b>{RADIO} (DISPATCH):</b> " + msg);
                                    client.GetHabbo().GetRoomUser().LastBubble = 0;
                                }
                                continue;
                            }
                            if (perm != "NO" && !client.GetRoleplay().RadioOff)
                            {
                                client.GetHabbo().GetRoomUser().LastBubble = 30;
                                client.SendWhisper("<b>{RADIO} [" + permm + "] (" + user.GetHabbo().UserName + "):</b> " + msg);
                                client.GetHabbo().GetRoomUser().LastBubble = 0;
                            }
                        }
                        catch (Exception)
                        {


                        }

                    }
                }
            }
            else
            {
                user.SendNotif("Você não pode usar o rádio do departamento!");
            }
        }

    }
}
