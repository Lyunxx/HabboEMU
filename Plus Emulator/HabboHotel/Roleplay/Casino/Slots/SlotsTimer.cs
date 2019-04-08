using System.Threading;
using Plus.HabboHotel.GameClients;
using System.Collections.Generic;
using Plus.HabboHotel.Roleplay.Misc;
using System;

namespace Plus.HabboHotel.Roleplay.Casino.Slots
{
    internal class slotsTimer
    {
        public slotsTimer(GameClient theHabbo, Slot slot)
        {
            this.slot = slot;
            Session = theHabbo;
            timeleft = 10000;
        }

        #region Declares
        private readonly Slot slot;
        private Timer timer;
        private readonly GameClient Session;
        private int timeleft;

        CryptoRandom rng = new CryptoRandom();

        private int slot1; private int number1;
        private int slot2; private int number2;
        private int slot3; private int number3;
        private int slot4; private int number4;
        private int slot5; private int number5;

        private int prize1;
        private int prize2;
        private int prize3;
        private int prize4;
        private int prize5;

        private int Prize = 0;
        private bool Jackpot = false;
        private string JackpotType = "";
        #endregion

        /// <summary>
        /// Starts the timer
        /// </summary>
        public void startTimer()
        {
            var autoRE = new AutoResetEvent(false);
            TimerCallback timerCB = timerDone;

            timer = new Timer(timerCB, autoRE, 500, Timeout.Infinite);

            Misc.RoleplayManager.Shout(Session, "*Pull the slot machines handle and watches as the slots change [-$" + RoleplayManager.SlotsMachineCost + "]*", 4);
            Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(8);
            slot.beingRolled = true;

            SlotUpdater();
            SlotCheck();

            RemoveTiles1();
            RemoveTiles2();
            PlaceTiles1();
        }

        /// <summary>
        /// Checks if timer completed
        /// </summary>
        public void timerDone(object info)
        {
            if (Session != null && Session.GetHabbo() != null)
            {
                try
                {
                    var user = Session.GetHabbo().GetRoomUser();
                    var room = Misc.RoleplayManager.GenerateRoom(slot.RoomId);

                    if (timeleft <= 0)
                    {
                        if (slot != null)
                        {
                            RemoveTiles1();
                            CalculatePrize();

                            #region Debug Check
                            /*
                            timeleft = 8000; PlaceTiles2();
                            timeleft = 6000; PlaceTiles2();
                            timeleft = 4000; PlaceTiles2();
                            timeleft = 2000; PlaceTiles2();
                            timeleft = 0; PlaceTiles2();
                            */
                            #endregion

                            RoleplayManager.GiveMoney(Session, +Prize);
                            if (Prize != 0) RoleplayManager.Shout(Session, "*Looks at the bottom of the slot machine and collects their reward of $" + Prize + "!*", 5);
                            if (Prize == 0) RoleplayManager.Shout(Session, "*Feels ashamed to look at the bottom of the slot machine as they have won nothing*", 5);

                            #region Jackpot Check
                            if (Jackpot == true)
                            {
                                string Notice = Session.GetHabbo().UserName + " has just won the " + JackpotType + " Jackpot of: $" + Prize + "!";

                                lock (Plus.GetGame().GetClientManager().Clients.Values)
                                {
                                    foreach (GameClient mClient in Plus.GetGame().GetClientManager().Clients.Values)
                                    {
                                        if (mClient == null)
                                            continue;
                                        if (mClient.GetHabbo() == null)
                                            continue;
                                        if (mClient.GetHabbo().CurrentRoom == null)
                                            continue;
                                        if (mClient.GetConnection() == null)
                                            continue;
                                        mClient.GetHabbo().GetRoomUser().LastBubble = 33;
                                        mClient.SendWhisper("[Casino]: " + Notice);
                                        mClient.GetHabbo().GetRoomUser().LastBubble = 0;
                                    }
                                }

                            }
                            #endregion

                            Session.GetRoleplay().inSlotMachine = false;
                            Session.GetHabbo().GetRoomUser().CanWalk = true;

                            slot.beingRolled = false;
                            timer.Dispose();
                        }
                    }
                    else
                    {
                        timeleft -= 500;
                        timer.Change(500, Timeout.Infinite);

                        PlaceTiles2();
                        SlotUpdater();
                        SlotCheck();
                        RemoveTiles1();
                        PlaceTiles1();
                    }
                }
                catch
                {
                    slot.beingRolled = false;
                    timer.Dispose();
                }
            }
            else
            {
                slot.beingRolled = false;
                timer.Dispose();
            }
        }

        /// <summary>
        /// Checks random slot number and updates slot furni
        /// </summary>
        public void SlotCheck()
        {
            if (timeleft > 0)
            {
                if (timeleft > 2000)
                {
                    if (timeleft > 4000)
                    {
                        if (timeleft > 6000)
                        {
                            if (timeleft > 8000)
                            {
                                if (slot1 == 1) prize1 = 761;
                                else if (slot1 == 2) prize1 = 3855;
                                else if (slot1 == 3) prize1 = 6877;
                                else if (slot1 == 4) prize1 = 6876;
                                else if (slot1 == 5) prize1 = 55005;
                                else if (slot1 == 6) prize1 = 6878;
                            }
                            if (slot2 == 1) prize2 = 761;
                            else if (slot2 == 2) prize2 = 3855;
                            else if (slot2 == 3) prize2 = 6877;
                            else if (slot2 == 4) prize2 = 6876;
                            else if (slot2 == 5) prize2 = 55005;
                            else if (slot2 == 6) prize2 = 6878;
                        }
                        if (slot3 == 1) prize3 = 761;
                        else if (slot3 == 2) prize3 = 3855;
                        else if (slot3 == 3) prize3 = 6877;
                        else if (slot3 == 4) prize3 = 6876;
                        else if (slot3 == 5) prize3 = 55005;
                        else if (slot3 == 6) prize3 = 6878;
                    }
                    if (slot4 == 1) prize4 = 761;
                    else if (slot4 == 2) prize4 = 3855;
                    else if (slot4 == 3) prize4 = 6877;
                    else if (slot4 == 4) prize4 = 6876;
                    else if (slot4 == 5) prize4 = 55005;
                    else if (slot4 == 6) prize4 = 6878;
                }
                if (slot5 == 1) prize5 = 761;
                else if (slot5 == 2) prize5 = 3855;
                else if (slot5 == 3) prize5 = 6877;
                else if (slot5 == 4) prize5 = 6876;
                else if (slot5 == 5) prize5 = 55005;
                else if (slot5 == 6) prize5 = 6878;
            }
        }

        /// <summary>
        /// Updates the random slot number used in SlotCheck()
        /// </summary>
        public void SlotUpdater()
        {
            if (Session.GetRoleplay().RigJackpot != true)
            {
                number1 = rng.Next(1, 121);
                number2 = rng.Next(1, 121);
                number3 = rng.Next(1, 121);
                number4 = rng.Next(1, 121);
                number5 = rng.Next(1, 121);

                if (number1 <= 20) slot1 = 1;
                else if (number1 > 20 && number1 <= 40) slot1 = 2;
                else if (number1 > 40 && number1 <= 60) slot1 = 3;
                else if (number1 > 60 && number1 <= 80) slot1 = 4;
                else if (number1 > 80 && number1 <= 100) slot1 = 5;
                else if (number1 > 100 && number1 <= 120) slot1 = 6;

                if (number2 <= 20) slot2 = 1;
                else if (number2 > 20 && number2 <= 40) slot2 = 2;
                else if (number2 > 40 && number2 <= 60) slot2 = 3;
                else if (number2 > 60 && number2 <= 80) slot2 = 4;
                else if (number2 > 80 && number2 <= 100) slot2 = 5;
                else if (number2 > 100 && number2 <= 120) slot2 = 6;

                if (number3 <= 20) slot3 = 1;
                else if (number3 > 20 && number3 <= 40) slot3 = 2;
                else if (number3 > 40 && number3 <= 60) slot3 = 3;
                else if (number3 > 60 && number3 <= 80) slot3 = 4;
                else if (number3 > 80 && number3 <= 100) slot3 = 5;
                else if (number3 > 100 && number3 <= 120) slot3 = 6;

                if (number4 <= 20) slot4 = 1;
                else if (number4 > 20 && number4 <= 40) slot4 = 2;
                else if (number4 > 40 && number4 <= 60) slot4 = 3;
                else if (number4 > 60 && number4 <= 80) slot4 = 4;
                else if (number4 > 80 && number4 <= 100) slot4 = 5;
                else if (number4 > 100 && number4 <= 120) slot4 = 6;

                if (number5 <= 20) slot5 = 1;
                else if (number5 > 20 && number5 <= 40) slot5 = 2;
                else if (number5 > 40 && number5 <= 60) slot5 = 3;
                else if (number5 > 60 && number5 <= 80) slot5 = 4;
                else if (number5 > 80 && number5 <= 100) slot5 = 5;
                else if (number5 > 100 && number5 <= 120) slot5 = 6;
            }
            else
            {
                slot1 = 6;
                slot2 = 6;
                slot3 = 6;
                slot4 = 6;
                slot5 = 6;
            }
        }

        /// <summary>
        /// Places the prizes ontop of tile set 1
        /// </summary>
        public void PlaceTiles1()
        {
            try
            {
                if (timeleft > 8000) RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(prize1), SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile1.X, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile1.Y, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile1.Z + 0.1, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile1.Rot, false, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile1.RoomId);
                if (timeleft > 6000) RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(prize2), SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile1.X, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile1.Y, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile1.Z + 0.1, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile1.Rot, false, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile1.RoomId);
                if (timeleft > 4000) RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(prize3), SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile1.X, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile1.Y, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile1.Z + 0.1, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile1.Rot, false, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile1.RoomId);
                if (timeleft > 2000) RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(prize4), SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile1.X, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile1.Y, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile1.Z + 0.1, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile1.Rot, false, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile1.RoomId);
                if (timeleft > 0) RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(prize5), SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile1.X, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile1.Y, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile1.Z + 0.1, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile1.Rot, false, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile1.RoomId);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Places the prizes ontop of tile set 2
        /// </summary>
        public void PlaceTiles2()
        {
            try
            {
                if (timeleft == 8000) RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(prize1), SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile2.X, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile2.Y, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile2.Z + 0.1, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile2.Rot, false, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile2.RoomId);
                else if (timeleft == 6000) RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(prize2), SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile2.X, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile2.Y, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile2.Z + 0.1, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile2.Rot, false, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile2.RoomId);
                else if (timeleft == 4000) RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(prize3), SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile2.X, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile2.Y, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile2.Z + 0.1, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile2.Rot, false, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile2.RoomId);
                else if (timeleft == 2000) RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(prize4), SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile2.X, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile2.Y, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile2.Z + 0.1, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile2.Rot, false, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile2.RoomId);
                else if (timeleft == 0) RoleplayManager.PlaceItemToRoomReturn(Convert.ToUInt32(prize5), SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile2.X, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile2.Y, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile2.Z + 0.1, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile2.Rot, false, SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile2.RoomId);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Removes the prizes ontop of tile set 1
        /// </summary>
        public void RemoveTiles1()
        {
            try
            {
                var currentRoom = Plus.GetGame().GetRoomManager().GetRoom(slot.RoomId);

                foreach (var Item in currentRoom.GetRoomItemHandler().FloorItems.Values)
                {
                    if (Item.X == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile1.X && Item.Y == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile1.Y && Item.RoomId == slot.RoomId && (Item.BaseItem == 761 || Item.BaseItem == 6877 || Item.BaseItem == 6876 || Item.BaseItem == 3855 || Item.BaseItem == 6878 || Item.BaseItem == 55005))
                    {
                        Misc.RoleplayManager.PickSlotTilePrize(Item, slot.RoomId);
                    }
                    if (Item.X == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile1.X && Item.Y == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile1.Y && Item.RoomId == slot.RoomId && (Item.BaseItem == 761 || Item.BaseItem == 6877 || Item.BaseItem == 6876 || Item.BaseItem == 3855 || Item.BaseItem == 6878 || Item.BaseItem == 55005))
                    {
                        Misc.RoleplayManager.PickSlotTilePrize(Item, slot.RoomId);
                    }
                    if (Item.X == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile1.X && Item.Y == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile1.Y && Item.RoomId == slot.RoomId && (Item.BaseItem == 761 || Item.BaseItem == 6877 || Item.BaseItem == 6876 || Item.BaseItem == 3855 || Item.BaseItem == 6878 || Item.BaseItem == 55005))
                    {
                        Misc.RoleplayManager.PickSlotTilePrize(Item, slot.RoomId);
                    }
                    if (Item.X == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile1.X && Item.Y == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile1.Y && Item.RoomId == slot.RoomId && (Item.BaseItem == 761 || Item.BaseItem == 6877 || Item.BaseItem == 6876 || Item.BaseItem == 3855 || Item.BaseItem == 6878 || Item.BaseItem == 55005))
                    {
                        Misc.RoleplayManager.PickSlotTilePrize(Item, slot.RoomId);
                    }
                    if (Item.X == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile1.X && Item.Y == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile1.Y && Item.RoomId == slot.RoomId && (Item.BaseItem == 761 || Item.BaseItem == 6877 || Item.BaseItem == 6876 || Item.BaseItem == 3855 || Item.BaseItem == 6878 || Item.BaseItem == 55005))
                    {
                        Misc.RoleplayManager.PickSlotTilePrize(Item, slot.RoomId);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Removes the prizes ontop of tile set 2
        /// </summary>
        public void RemoveTiles2()
        {
            try
            {
                var currentRoom = Plus.GetGame().GetRoomManager().GetRoom(slot.RoomId);

                foreach (var Item in currentRoom.GetRoomItemHandler().FloorItems.Values)
                {
                    if (Item.X == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile2.X && Item.Y == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 1].Tile2.Y && Item.RoomId == slot.RoomId && (Item.BaseItem == 761 || Item.BaseItem == 6877 || Item.BaseItem == 6876 || Item.BaseItem == 3855 || Item.BaseItem == 6878 || Item.BaseItem == 55005))
                    {
                        Misc.RoleplayManager.PickFarmingSpot(Item, slot.RoomId);
                    }
                    if (Item.X == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile2.X && Item.Y == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 2].Tile2.Y && Item.RoomId == slot.RoomId && (Item.BaseItem == 761 || Item.BaseItem == 6877 || Item.BaseItem == 6876 || Item.BaseItem == 3855 || Item.BaseItem == 6878 || Item.BaseItem == 55005))
                    {
                        Misc.RoleplayManager.PickFarmingSpot(Item, slot.RoomId);
                    }
                    if (Item.X == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile2.X && Item.Y == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 3].Tile2.Y && Item.RoomId == slot.RoomId && (Item.BaseItem == 761 || Item.BaseItem == 6877 || Item.BaseItem == 6876 || Item.BaseItem == 3855 || Item.BaseItem == 6878 || Item.BaseItem == 55005))
                    {
                        Misc.RoleplayManager.PickFarmingSpot(Item, slot.RoomId);
                    }
                    if (Item.X == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile2.X && Item.Y == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 4].Tile2.Y && Item.RoomId == slot.RoomId && (Item.BaseItem == 761 || Item.BaseItem == 6877 || Item.BaseItem == 6876 || Item.BaseItem == 3855 || Item.BaseItem == 6878 || Item.BaseItem == 55005))
                    {
                        Misc.RoleplayManager.PickFarmingSpot(Item, slot.RoomId);
                    }
                    if (Item.X == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile2.X && Item.Y == SlotsManager.SlotsTilesData[Convert.ToInt32(slot.Id), 5].Tile2.Y && Item.RoomId == slot.RoomId && (Item.BaseItem == 761 || Item.BaseItem == 6877 || Item.BaseItem == 6876 || Item.BaseItem == 3855 || Item.BaseItem == 6878 || Item.BaseItem == 55005))
                    {
                        Misc.RoleplayManager.PickFarmingSpot(Item, slot.RoomId);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Calculates the actual user reward based on prizes ontop of tile set 2
        /// </summary>
        public void CalculatePrize()
        {
            try
            {
                int[] PrizeArray = { prize1, prize2, prize3, prize4, prize5 };
                var PrizeDictionary = new Dictionary<int, int>();

                foreach (var value in PrizeArray)
                {
                    if (PrizeDictionary.ContainsKey(value))
                    {
                        PrizeDictionary[value]++;
                    }
                    else
                    {
                        PrizeDictionary[value] = 1;
                    }
                }
                foreach (var pair in PrizeDictionary)
                {
                    if (pair.Value == 2)
                    {
                        if (Prize < 1440)
                        {
                            if (pair.Key == 6878) { Prize = 1440; }
                            else if (pair.Key == 55005 && Prize < 1440) { Prize = 1200; }
                            else if (pair.Key == 6876 && Prize < 1200) { Prize = 960; }
                            else if (pair.Key == 6877 && Prize < 960) { Prize = 720; }
                            else if (pair.Key == 3855 && Prize < 720) { Prize = 480; }
                            else if (Prize < 480) { Prize = 240; }
                        }
                    }
                    if (pair.Value == 3)
                    {
                        if (Prize < 7200)
                        {
                            if (pair.Key == 6878) { Prize = 7200; }
                            else if (pair.Key == 55005 && Prize < 7200) { Prize = 6000; }
                            else if (pair.Key == 6876 && Prize < 6000) { Prize = 4800; }
                            else if (pair.Key == 6877 && Prize < 4800) { Prize = 3600; }
                            else if (pair.Key == 3855 && Prize < 3600) { Prize = 2400; }
                            else if (Prize < 2400) { Prize = 1200; }
                        }
                    }
                    else if (pair.Value == 4)
                    {
                        if (Prize < 36000)
                        {
                            if (pair.Key == 6878) { Prize = 36000; }
                            else if (pair.Key == 55005 && Prize < 36000) { Prize = 30000; }
                            else if (pair.Key == 6876 && Prize < 30000) { Prize = 24000; }
                            else if (pair.Key == 6877 && Prize < 24000) { Prize = 18000; }
                            else if (pair.Key == 3855 && Prize < 18000) { Prize = 12000; }
                            else if (Prize < 12000) { Prize = 6000; }
                        }
                    }
                    else if (pair.Value == 5)
                    {
                        if (Prize < 216000)
                        {
                            if (pair.Key == 6878) { Prize = 216000; JackpotType = "Diamond Bar"; }
                            else if (pair.Key == 55005 && Prize < 180000) { Prize = 180000; JackpotType = "Emerald Bar"; }
                            else if (pair.Key == 6876 && Prize < 144000) { Prize = 144000; JackpotType = "Ruby Bar"; }
                            else if (pair.Key == 6877 && Prize < 108000) { Prize = 108000; JackpotType = "Sapphire Bar"; }
                            else if (pair.Key == 3855 && Prize < 72000) { Prize = 72000; JackpotType = "Amethyst Bar"; }
                            else if (Prize < 36000) { Prize = 36000; JackpotType = "Gold Bar"; }
                            Jackpot = true;
                        }
                    }
                    #region Debug Check
                    /*
                    if (pair.Key == 6878) Session.SendWhisper("Diamond Bar = " + pair.Value);
                    if (pair.Key == 55005) Session.SendWhisper("Emerald Bar = " + pair.Value);
                    if (pair.Key == 6876) Session.SendWhisper("Ruby Bar = " + pair.Value);
                    if (pair.Key == 6877) Session.SendWhisper("Sapphire Bar = " + pair.Value);
                    if (pair.Key == 3855) Session.SendWhisper("Amethyst Bar = " + pair.Value);
                    if (pair.Key == 761) Session.SendWhisper("Gold Bar = " + pair.Value);
                    */
                    #endregion
                }
                if (Jackpot == false) Prize = Prize / RoleplayManager.SlotsMachinePrizeDivider;
                if (Jackpot == true) Prize = Prize / RoleplayManager.SlotsMachineJackpotDivider;
            }
            catch
            {
            }
        }
    }
}