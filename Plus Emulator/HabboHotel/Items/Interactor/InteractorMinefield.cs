using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Roleplay.Casino.Slots;

namespace Plus.HabboHotel.Items.Interactor
{
    internal class InteractorMinefield : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
            item.UpdateNeeded = true;
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
        }

        public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
        {
            if (Session == null)
                return;

            if (Session.GetRoleplay().usingCar == true || Session.GetRoleplay().usingPlane == true)
            {
                Session.GetRoleplay().usingCar = false;
                Session.GetRoleplay().usingPlane = false;
                Session.GetHabbo().GetRoomUser().FastWalking = false;
            }

            User.ClearMovement();

            CryptoRandom rand = new CryptoRandom();
            int Chance = rand.Next(1, 101);
            int Damage = rand.Next(50, 150);

            if (Chance >= 8 && Session.GetRoleplay().StaffDuty == false)
            {
                Item.ExtraData = "2";
                Item.UpdateNeeded = true;
                Item.UpdateState();

                if (Session.GetRoleplay().Armor >= 1)
                {
                    Session.GetRoleplay().Armor = 0;
                    Session.GetRoleplay().Armored = false;
                    Session.GetRoleplay().ArmoredFigSet = false;
                    RoleplayManager.Shout(Session, "*Body-armor shatters*", 4);
                }

                if (Session.GetRoleplay().CurHealth - Damage <= 0)
                {
                    RoleplayManager.Shout(Session, "*Steps onto a mine, causing it to blow up kill them*", 32);
                    Session.GetRoleplay().DeadFigSet = false;
                    Session.GetRoleplay().DeadSeconds = 60;
                    Session.GetRoleplay().DeadTimer = 2;
                    Session.GetRoleplay().Dead = true;
                    Session.GetRoleplay().SaveStatusComponents("dead");
                    Session.GetRoleplay().Deaths++;
                    RoleplayManager.HandleDeath(Session);
                }
                else
                {
                    Session.GetRoleplay().CurHealth = Session.GetRoleplay().CurHealth - Damage;
                    Session.GetRoleplay().SaveQuickStat("curhealth", "" + Session.GetRoleplay().CurHealth);
                    RoleplayManager.Shout(Session, "*Steps onto a mine, causing it to blow up and dealing " + Damage + " damage*", 22);
                }
            }
            else
            {
                RoleplayManager.Shout(Session, "*Steps onto a mine, but nothing happens*", 3);
                Item.ExtraData = "1";
                Item.UpdateNeeded = true;
                Item.UpdateState();
            }
            Item.ExtraData = "0";
            Item.UpdateNeeded = true;
            Item.UpdateState();
        }

        public void OnWiredTrigger(RoomItem item)
        {
        }
    }
}