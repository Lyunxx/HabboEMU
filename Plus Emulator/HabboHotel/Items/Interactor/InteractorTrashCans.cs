using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Generic;
using Plus.HabboHotel.Roleplay.Misc;
using System.Linq;
using System.Text;

namespace Plus.HabboHotel.Items.Interactor
{
    class InteractorTrashCans : IFurniInteractor
    {
        public void OnPlace(GameClients.GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";
            Item.UpdateNeeded = true;
        }

        public void OnRemove(GameClients.GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";
        }

        public void OnTrigger(GameClients.GameClient Session, RoomItem Item, int Request, bool HasRights)
        {

            if (Session == null)
                return;
            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);


            if (Item.InteractingUser2 != User.UserId)
                Item.InteractingUser2 = User.UserId;

            if (User == null)
            {
                return;
            }

            if (!Gamemap.TilesTouching(User.X, User.Y, Item.X, Item.Y))
            {
                User.MoveTo(Item.SquareInFront);
                return;
            }

            if (RoleplayManager.EmptyTrashCans.ContainsKey(Item.Id))
            {

                DateTime nowTime = DateTime.Now;
                DateTime LastTime = RoleplayManager.EmptyTrashCans[Item.Id];
                TimeSpan span = nowTime.Subtract(LastTime);
                int timeleft = 5 - Convert.ToInt32(span.Minutes);

                if (timeleft > 0)
                {
                    User.GetClient().SendWhisper("This item has recently been interacted with.");
                    return;
                }
            }

            int Cash = new Random().Next(1, 350);
            int Weed = new Random().Next(1, 100);
            int Bullets = new Random().Next(1, 50);
            int PhoneCredits = new Random().Next(1, 1000);

            #region Trash Can Items

            List<string> trash = new List<string>();
            trash.Add("[CASH]");
            trash.Add("[CASH]");
            trash.Add("[CASH]");
            trash.Add("[WEED]");
            trash.Add("[WEED]");
            trash.Add("[WEED]");
            trash.Add("[WEED]");
            trash.Add("[WEED]");
            trash.Add("[AMMO]");
            trash.Add("[AMMO]");
            trash.Add("[AMMO]");
            trash.Add("[AMMO]");
            trash.Add("[AMMO]");
            trash.Add("[PHONEC]");
            trash.Add("[PHONEC]");
            trash.Add("[PHONEC]");
            trash.Add("[PHONEC]");
            trash.Add("[PHONEC]");
            trash.Add("[VESTS]");
            trash.Add("[VESTS]");
            trash.Add("[VESTS]");
            trash.Add("[VESTS]");
            trash.Add("[VESTS]");
            trash.Add("[HOBO]");
            trash.Add("[HOBO]");
            trash.Add("[HOBO]");
            trash.Add("[HOBO]");
            trash.Add("[HOBO]");
            trash.Add("[HOBO]");
            trash.Add("[HOBO]");
            trash.Add("[HOBO]");
            trash.Add("[HOBO]");
            
            #endregion

            int tc = new Random().Next(trash.Count);
            string foundItem = trash[tc];
            string foundmsg = "*Fails to find anything in the bin*";

            #region Rummage in bin task

            RoleplayManager.Shout(User.GetClient(), "*Begins to rummage in the bin*");
            User.ApplyEffect(4);

            if (RoleplayManager.EmptyTrashCans.ContainsKey(Item.Id))
            {
                DateTime trashJunk;
                RoleplayManager.EmptyTrashCans.TryRemove(Item.Id, out trashJunk);
            }

            RoleplayManager.EmptyTrashCans.TryAdd(Item.Id, DateTime.Now);
            Item.ExtraData = "1";
            Item.UpdateState(false, true);
            Item.ReqUpdate(4, true);

            int time = 300;
            time = time * 1000;
            System.Timers.Timer dispatcherTimer = new System.Timers.Timer(time);
            dispatcherTimer.Interval = time;
            dispatcherTimer.Elapsed += delegate
            {
                Item.InteractingUser = 0;
                Item.ExtraData = "0";
                Item.UpdateState(false, true);
                Item.ReqUpdate(4, true);
            };
            dispatcherTimer.Start();
            System.Threading.Thread.Sleep(2000);
            User.ApplyEffect(0);

            #endregion

            #region Rummage in bin completion (found items?)

            if (foundItem.Equals("[CASH]"))
            {
                foundmsg = "*Finds $" + Cash + " in the bin*";
                RoleplayManager.GiveMoney(Session, +Cash);
            }
            else if (foundItem.Equals("[WEED]"))
            {
                foundmsg = "*Finds " + Weed + "g of Weed in the bin*";
                Session.GetRoleplay().Weed += Weed;
                Session.GetRoleplay().SaveQuickStat("weed", "" + Session.GetRoleplay().Weed);
            }
            else if (foundItem.Equals("[AMMO]"))
            {
                foundmsg = "*Finds " + Bullets + " Bullets in the bin*";
                Session.GetRoleplay().Bullets += Bullets;
                Session.GetRoleplay().SaveQuickStat("bullets", "" + Session.GetRoleplay().Bullets);
            }
            else if (foundItem.Equals("[VESTS]"))
            {
                foundmsg = "*Finds an old kevlar vest in the bin*";
                Session.GetRoleplay().Vests += 1;
                Session.GetRoleplay().SaveQuickStat("vests", "" + Session.GetRoleplay().Vests);
                Session.SendWhisper("Use :ba to use your kevlar vest!");
            }
            else if (foundItem.Equals("[PHONEC]"))
            {
                foundmsg = "*Finds $" + PhoneCredits + " worth of phone credits in the bin*";
                RoleplayManager.GiveCredit(Session, +PhoneCredits);
            }
            else if (foundItem.Equals("[HOBO]"))
            {
                //Credits in hand stealer
                HabboHotel.RoomBots.RoomBot NewParamedic = RoleplayManager.MakeQuickBot("[HOBO] Jim", HabboHotel.RoomBots.AIType.MiscBot, User.GetClient().GetHabbo().CurrentRoomId, "hr-676-61.cp-3207-62-1408.ca-3292-110.lg-275-1408.hd-3103-97541.fa-1205-110.ch-255-1331.he-1601-62", Item.X, Item.Y, 0, "M");
                NewParamedic.InteractingWith = User;
                RoleplayManager.SendBotToRoom(NewParamedic, User.GetClient().GetHabbo().CurrentRoomId);

                //Credits in bank stealer
                HabboHotel.RoomBots.RoomBot NewParamedicBank = RoleplayManager.MakeQuickBot("[HOBO 2] John", HabboHotel.RoomBots.AIType.MiscBot, User.GetClient().GetHabbo().CurrentRoomId, "hr-676-61.cp-3207-62-1408.ca-3292-110.lg-275-1408.hd-3103-97541.fa-1205-110.ch-255-1331.he-1601-62", Item.X, Item.Y, 0, "M");
                NewParamedicBank.InteractingWith = User;
                RoleplayManager.SendBotToRoom(NewParamedicBank, User.GetClient().GetHabbo().CurrentRoomId);
            }

            RoleplayManager.Shout(Session, foundmsg);

            #endregion
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {

        }
        public void OnWiredTrigger(RoomItem Item)
        {
            if (Item.ExtraData == "0")
            {
                Item.ExtraData = "1";
                Item.UpdateState(false, true);
                Item.ReqUpdate(4, true);
            }
        }
    }
}
