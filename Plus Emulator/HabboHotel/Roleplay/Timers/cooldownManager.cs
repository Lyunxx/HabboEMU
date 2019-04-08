using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
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
using System.Net;

namespace Plus.HabboHotel.Roleplay.Timers
{
    public class CooldownManager
    {
        public static void CooldownTick(GameClient Session)
        {
            if (Session.GetRoleplay().CoolDown > 0)
            {
                Session.GetRoleplay().CoolDown--;
            }
            else
            {
                Session.GetRoleplay().CheckingCoolDown = false;
            }

            Dictionary<string, int> Temp = new Dictionary<string, int>();
            List<string> KeysToNuke = new List<string>();
           // bool loopdone = false;

            lock (Session.GetRoleplay().MultiCoolDown.Keys)
            {
                foreach (var Key in Session.GetRoleplay().MultiCoolDown.Keys)
                {
                    KeysToNuke.Add(Key);
                }

                lock (KeysToNuke)
                {
                    foreach (string Key in KeysToNuke)
                    {
                        if(Session.GetRoleplay().MultiCoolDown[Key] > 0)
                        {
                            Session.GetRoleplay().MultiCoolDown[Key]--;
                        }
                        else
                        {
                            Session.GetRoleplay().CheckingMultiCooldown = false;
                        }
                    }
                }
            }

               

            /*
            foreach(KeyValuePair<string,int> Cd in Session.GetRoleplay().MultiCoolDown)
            {
                
                if(!Session.GetRoleplay().MultiCoolDown_Destroy.ContainsKey(Cd.Key))
                {
                    Session.GetRoleplay().MultiCoolDown_Destroy.Add(Cd.Key,Cd.Value);
                }              
                else
                {
                    if(Session.GetRoleplay().MultiCoolDown_Destroy[Cd.Key] > 0)
                    {
                    Session.GetRoleplay().MultiCoolDown_Destroy[Cd.Key]--;
                   
                    }
                    else
                    {
                        Console.WriteLine(Session.GetRoleplay().MultiCoolDown_Destroy[Cd.Key].ToString());
                        Session.GetRoleplay().MultiCoolDown.Clear();
                        foreach (KeyValuePair<string, int> NewCd in Session.GetRoleplay().MultiCoolDown_Destroy)
                        {
                            Session.GetRoleplay().MultiCoolDown.Add(NewCd.Key, NewCd.Value);
                            Session.GetRoleplay().CheckingMultiCooldown = false;
                            Session.GetRoleplay().MultiCoolDown_Destroy.Clear();
                        }
                    }
                }
             * */
        }
    }
}
