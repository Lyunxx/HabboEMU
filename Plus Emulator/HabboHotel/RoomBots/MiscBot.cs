using System;
using System.Linq;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System.Collections.Generic;
using System.Drawing;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.Roleplay.Combat;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Roleplay.Jobs;
using Plus.Messages;
using Plus.Messages.Parsers;

namespace Plus.HabboHotel.RoomBots
{
    class MiscBot : BotAI
    {
        private static readonly Random Random = new Random();

        private readonly int _id;
        private readonly int _virtualId;
        private readonly bool _isBartender;

        private int _actionCount = 30;
        private int _speechInterval = 7;
        private Timer _chatTimer;

        private int LActionTimer;
        private bool Robbed;
        private bool Done;
        //private bool RandomChat;

        private int attack = 0;
        private int gatherSpeed;
        private int gatherMsg;
        private int gatherNum;
        private int amount;
        private GameClient robber;
        private int lastRobbed;
        private int attackCount;
        private bool disableRob;
        private bool beingRobbed;
        private int robberCount = 0;

        private GameClient serving = null;
        private bool event_serving = false;
        private bool event_serving_atTable = false;
        private bool event_serving_orderPlaced = false;
        private bool event_set_items = false;
        private HabboHotel.Items.RoomItem event_serving_plate = null;
        private string event_serving_fooditem = "";
        private bool event_serving_delivered = false;
        private bool event_serving_passedGate1 = false;
        private bool event_serving_passedGate2 = false;
        private HabboHotel.Items.RoomItem event_serving_oneway = null;
        private HabboHotel.Items.RoomItem event_serving_oneway2 = null;
        private Vector2D OriginalPosition = null;
        private int order_tick = 0;
        private int wrong_order_strike = 0;
        private int Gate_Tick = 0;
        private bool passing_cur_gate = false;
        private bool resetted = false;
        private int Undefined_Timer_Tick_1 = 0;
        private int Undefined_Timer_Tick_2 = 0;
        private int Undefined_Timer_Tick_3 = 0;


        internal HabboHotel.Items.RoomItem retrieveItem(string item_name, bool nearUser = false, RoomUser User = null)
        {
            HabboHotel.Items.RoomItem Item = null;


            if (!nearUser)
            {
                foreach (HabboHotel.Items.RoomItem item in GetRoom().GetRoomItemHandler().FloorItems.Values)
                {

                    if (item.GetBaseItem().Name.ToLower() == item_name.ToLower())
                    {
                        //   Console.WriteLine(item.GetBaseItem().Name);

                        Item = item;
                    }
                }
            }
            else
            {

                foreach (HabboHotel.Items.RoomItem item in GetRoom().GetRoomItemHandler().FloorItems.Values)
                {
                    Vector2D Pos1 = new Vector2D(item.X, item.Y);
                    Vector2D Pos2 = new Vector2D(User.X, User.Y);

                    if (RoleplayManager.Distance(Pos1, Pos2) <= 1 && item.GetBaseItem().Name.ToLower() == item_name.ToLower())
                    {
                        Item = item;
                    }
                }

            }

            return Item;
        }
        internal MiscBot(RoomBot roomBot, int virtualId, int botId, AIType type, bool isBartender, int speechInterval)
        {
            _id = botId;
            _virtualId = virtualId;
            _isBartender = isBartender;
            _speechInterval = speechInterval < 2 ? 2000 : speechInterval * 1000;

            // Get random speach
            if (roomBot.AutomaticChat && roomBot.RandomSpeech != null && roomBot.RandomSpeech.Any()) _chatTimer = new Timer(ChatTimerTick, null, _speechInterval, _speechInterval);
            _actionCount = Random.Next(10, 30 + virtualId);
        }

        internal override void Modified()
        {
            if (GetBotData() == null) return;
            if (!GetBotData().AutomaticChat || GetBotData().RandomSpeech == null || !GetBotData().RandomSpeech.Any())
            {
                StopTimerTick();
                return;
            }
            _speechInterval = GetBotData().SpeechInterval < 2 ? 2000 : GetBotData().SpeechInterval * 1000;

            if (_chatTimer == null)
            {
                _chatTimer = new Timer(ChatTimerTick, null, _speechInterval, _speechInterval);
                return;
            }
            _chatTimer.Change(_speechInterval, _speechInterval);
        }

        internal override void OnSelfEnterRoom()
        {

        }

        internal override void OnSelfLeaveRoom(bool Kicked)
        {

        }

        internal override void OnUserEnterRoom(Rooms.RoomUser User)
        {

        }

        internal override void OnUserLeaveRoom(GameClients.GameClient Client)
        {

        }

        internal override void OnUserSay(Rooms.RoomUser User, string Message)
        {
            string[] special = { "pedir panquecas", "pedir bife", "pedir batata", "pedir ovos", "pedir spaghetti", "pedir milkshakes" };
            bool SpecialMsg = false;
            string specialMsg = "";

            foreach (string spe in special)
            {
                if (Message.ToLower() == spe.ToLower())
                {
                    SpecialMsg = true;
                    specialMsg = spe;
                }
            }

            /*
            if (Gamemap.TileDistance(GetRoomUser().X, GetRoomUser().Y, User.X, User.Y) > 8 && SpecialMsg)
            {
                User.GetClient().SendWhisper("You must get closer to do this!");
                return;
            }*/

            #region Ammunation


            if (base.GetRoom().RoomData.Description.Contains("AMMUNATION"))
            {


                if (SpecialMsg)
                {
                    bool HasWeapon = false;

                    foreach (KeyValuePair<string, Weapon> Wep in User.GetClient().GetRoleplay().Weapons)
                    {
                        if (HabboHotel.Roleplay.Combat.WeaponManager.GetWeaponName(Wep.Key) == specialMsg)
                        {
                            HasWeapon = true;
                        }
                    }

                    if (HasWeapon)
                    {
                        User.GetClient().SendWhisper("You already have a " + specialMsg + "!");
                        return;
                    }

                    User.GetClient().GetRoleplay().OfferData.Clear();

                    User.GetClient().SendWhisper("You were offered a " + specialMsg + " for $" + WeaponManager.WeaponsData[specialMsg].Price + ". Type #accept to accept or #deny to deny!");
                    base.GetRoomUser().Chat(null, "*Offers " + User.GetClient().GetHabbo().UserName + " a " + specialMsg + " for $" + WeaponManager.WeaponsData[specialMsg].Price + "*", false, 1);
                    User.GetClient().GetRoleplay().OfferData.Add(specialMsg, new Offer(null, specialMsg, 1, WeaponManager.WeaponsData[specialMsg].Price));

                }
            }

            #endregion

            #region Phone Shop
            if (base.GetRoom().RoomData.Description.Contains("PHONE"))
            {
                if (SpecialMsg)
                {
                    if (User.GetClient().GetRoleplay().Phone == 1)
                    {
                        User.GetClient().SendWhisper("You already have a phone!");
                        return;
                    }

                    base.GetRoomUser().Chat(null, "*Offers " + User.GetClient().GetHabbo().UserName + "a Nokia phone for $50*", true, 1);
                    User.GetClient().SendWhisper("You are being offered a phone for $50. Type #accept to accept or #deny to deny!");
                    User.GetClient().GetRoleplay().OfferData.Add("phone", new Offer(null, "phone", 1, 50));


                }
            }
            #endregion

            #region Store Rob

            if (base.GetRoom().RoomData.Description.Contains("STORE"))
            {

                #region Activate Store Robbing
                if (Message == "activate")
                {

                    if (RoleplayManager.BypassRights(User.GetClient()))
                    {
                        lastRobbed = 5001;
                        User.GetClient().SendWhisper("Store robbing re-activated!");
                    }

                }
                #endregion

                #region Stop Store Robbing
                if (Message == "stop")
                {

                    if (RoleplayManager.BypassRights(User.GetClient()))
                    {
                        robber = null;
                        beingRobbed = false;
                        User.GetClient().GetRoleplay().robbingStore = 0;
                        User.GetClient().SendWhisper("Store robbing de-activated/restarted!");
                    }

                }
                #endregion

                #region Faster
                if (Message == "faster")
                {
                    if (beingRobbed)
                    {
                        if (robber.GetHabbo().Id == User.GetClient().GetHabbo().Id)
                        {
                            RoleplayManager.Shout(User.GetClient(), "Go faster or else ill blow you to bits!");
                            User.GetClient().GetHabbo().GetRoomUser().ApplyEffect(164);
                            base.GetRoomUser().Chat(null, "I'm sorry, please wait, im going my fastest!!", true, 1);
                            gatherSpeed = 2;
                        }
                    }
                }
                #endregion

                #region Rob

                if (Message == "rob")
                {

                    if (beingRobbed)
                    {
                        User.GetClient().SendWhisper("This place is currently being robbed!");
                        return;
                    }
                    else
                    {
                        if (disableRob)
                        {
                            User.GetClient().SendWhisper("This place was recently robbed!");
                            return;
                        }
                        else
                        {

                            if (!base.GetRoom().RoomData.Description.Contains("STOREROB"))
                            {
                                User.GetClient().SendWhisper("This cannot rob this place!");
                                return;
                            }
                            else
                            {

                                if (robberCount > 0)
                                {
                                    User.GetClient().SendWhisper("This place is already being robbed!");
                                    return;
                                }
                                else
                                {
                                    robber = User.GetClient();

                                    RoleplayManager.Shout(User.GetClient(), "Give me all your cash, before I blow your head to bits!");

                                    robber.GetRoleplay().robbingStore = Convert.ToInt32(robber.GetHabbo().CurrentRoomId);
                                    if (!RoleplayManager.PurgeTime)
                                    {
                                        robber.GetRoleplay().Wanted = 10;
                                        robber.GetRoleplay().SaveQuickStat("wanted", "" + 10);

                                        if (!RoleplayManager.WantedListData.ContainsKey(robber.GetHabbo().UserName.ToLower()))
                                        {
                                            RoleplayManager.WantedListData.TryAdd(robber.GetHabbo().UserName.ToLower(), 10 + "|" + Convert.ToInt32(robber.GetHabbo().CurrentRoomId));
                                        }

                                        robber.SendWhisper("You are now wanted for store robbery!");

                                        lock (Plus.GetGame().GetClientManager().Clients.Values)
                                        {
                                            foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                                            {

                                                if (client == null)
                                                    continue;
                                                if (client.GetRoleplay() == null)
                                                    continue;
                                                if (!JobManager.validJob(client.GetRoleplay().JobId, client.GetRoleplay().JobRank))
                                                    continue;
                                                if (!client.GetRoleplay().JobHasRights("police"))
                                                    continue;
                                                if (!client.GetRoleplay().Working)
                                                    continue;

                                                client.GetHabbo().GetRoomUser().LastBubble = 30;
                                                client.SendWhisper("<b>[Police Radio]:</b> " + robber.GetHabbo().UserName + " is robbing the store at " + robber.GetHabbo().CurrentRoomId + ", get there fast!");
                                                client.GetHabbo().GetRoomUser().LastBubble = 0;
                                            }
                                        }
                                    }

                                    base.GetRoomUser().Chat(null, "Okay, okay! Hold on please!!", true, 1);

                                    gatherSpeed = 0;
                                    gatherMsg = 20;
                                    gatherNum = 1;
                                    beingRobbed = true;
                                    attack = new Random().Next(1, 10);
                                }
                            }

                        }

                    }
                }


                #endregion

            #endregion

                #region Eatery
                if (base.GetRoom().RoomData.Description.Contains("EATERY"))
                {



                    if (!event_serving)
                    {
                        if (SpecialMsg)
                        {
                            if (!User.GetClient().GetRoleplay().MultiCoolDown.ContainsKey("order_cooldown"))
                            {
                                User.GetClient().GetRoleplay().MultiCoolDown.Add("order_cooldown", 0);
                            }
                            if (User.GetClient().GetRoleplay().MultiCoolDown["order_cooldown"] > 0)
                            {
                                User.GetClient().SendWhisper("Você recentemente pediu algo, por favor, aguarde um pouco para encomendar novamente!");
                                return;
                            }

                            if (specialMsg.Contains("pedir"))
                            {

                                if (OriginalPosition == null)
                                {
                                    OriginalPosition = new Vector2D(GetRoomUser().X, GetRoomUser().Y);
                                }

                                serving = User.GetClient();
                                string food = "";
                                string[] sp = specialMsg.Split(' ');
                                foreach (string param in sp)
                                {
                                    food = sp[1];
                                }


                                event_serving_plate = retrieveItem("diner_tray_0", true, User);


                                if (event_serving_plate == null)
                                {
                                    User.GetClient().SendWhisper("Você precisa se sentar perto de um prato vazio!");
                                    return;
                                }

                                if (User.GetClient().GetRoleplay().Hunger <= 0)
                                {
                                    User.GetClient().SendWhisper("Parece que você já está cheio, você não precisa mais de comida!");
                                    return;
                                }

                                GetRoomUser().Chat(null, "Olá " + serving.GetHabbo().UserName + ", uma porção de " + food + " chegando!", true, 0);


                                event_serving_orderPlaced = true;
                                event_serving_fooditem = food;
                                event_serving = true;

                                //User.GetClient().GetRoleplay().MultiCoolDown["order_cooldown"] = 30;
                            }
                        }
                    }
                    else
                    {
                        if (SpecialMsg)
                        {
                            User.GetClient().SendWhisper("Alguém está fazendo o pedido, por favor, aguarde a sua vez!");

                            #region Delete Serve Session
                            if (serving == null)
                            {
                                event_serving = false;
                                return;
                            }
                            if (serving.GetHabbo() == null)
                            {
                                event_serving = false;
                                return;
                            }
                            if (serving.GetRoleplay() == null)
                            {
                                event_serving = false;
                                return;
                            }
                            if (serving.GetHabbo().CurrentRoom == null)
                            {
                                event_serving = false;
                                return;
                            }
                            if (serving.GetHabbo().CurrentRoom != GetRoom())
                            {
                                event_serving = false;
                                return;
                            }
                            #endregion
                        }
                    }
                }
                #endregion

            }
        }



        internal override void OnUserShout(Rooms.RoomUser User, string Message)
        {
            /*
            if (PlusEnvironment.GetRandomNumber(0, 10) >= 5)
            {
                GetRoomUser().Chat(null, LanguageLocale.GetValue("onusershout"), true); // shout nag
            }*/
        }

        internal void MoveToOriginalPosition()
        {

        }

        internal void HandleGatePass(HabboHotel.Items.RoomItem Gate)
        {
            if (!passing_cur_gate)
            {
                // GetRoomUser().AllowOverride = true;
                if (new Point(GetRoomUser().X, GetRoomUser().Y) == Gate.SquareInFront)
                {
                    // GetRoomUser().AllowOverride = true;
                    Gate.ExtraData = "1";
                    Gate.UpdateNeeded = true;
                    Gate.UpdateState();
                    GetRoomUser().MoveTo(Gate.X, Gate.Y);
                    passing_cur_gate = true;
                    Undefined_Timer_Tick_1 = 0;
                }
                else
                {
                    // GetRoomUser().AllowOverride = true;
                    GetRoomUser().MoveTo(Gate.SquareInFront);
                }
            }
            else
            {
                if (Undefined_Timer_Tick_1 > 0)
                {
                    Gate.ExtraData = "1";
                    Gate.UpdateNeeded = true;
                    Gate.UpdateState();
                    Undefined_Timer_Tick_1--;
                }
                else
                {
                    Gate.ExtraData = "1";
                    Gate.UpdateNeeded = true;
                    Gate.UpdateState();
                    GetRoomUser().AllowOverride = true;
                    GetRoomUser().MoveTo(Gate.SquareBehind);

                    if (event_serving_passedGate1)
                    {
                        event_serving_passedGate2 = true;

                    }
                    else
                    {
                        event_serving_passedGate1 = true;

                    }
                }


            }

        }

        internal void ResetVariablesFor(string type)
        {
            switch (type)
            {
                case "eatery":
                    GetRoomUser().AllowOverride = true;
                    GetRoomUser().MoveTo(OriginalPosition.X, OriginalPosition.Y);
                    GetRoomUser().AllowOverride = false;
                    event_serving = false;
                    event_serving_atTable = false;
                    event_serving_delivered = false;
                    event_serving_fooditem = "";
                    event_serving_oneway = null;
                    order_tick = 0;
                    event_serving_oneway2 = null;
                    event_serving_orderPlaced = false;
                    event_serving_passedGate1 = false;
                    event_serving_passedGate2 = false;
                    event_serving_plate = null;
                    event_set_items = false;
                    serving = null;
                    resetted = false;
                    passing_cur_gate = false;
                    Undefined_Timer_Tick_2 = 0;
                    break;
            }
        }

        internal override void OnTimerTick()
        {
            if (base.GetBotData() != null)
            {
                
                #region Ammunation
                if (base.GetRoom().RoomData.Description.Contains("AMMUNATION"))
                {

                    if (base.GetRoomUser().RotBody != 2)
                    {
                        base.GetRoomUser().RotBody = 2;
                        base.GetRoomUser().RotHead = 2;

                    }
                }
                #endregion

                #region Hobo

                if (base.GetBotData().InteractingWith != null && base.GetBotData().Name.Contains("HOBO"))
                {
                    HabboHotel.PathFinding.Vector2D Pos1 = new HabboHotel.PathFinding.Vector2D(base.GetRoomUser().X, base.GetRoomUser().Y);

                    HabboHotel.PathFinding.Vector2D Pos2 = new HabboHotel.PathFinding.Vector2D(base.GetBotData().InteractingWith.X, base.GetBotData().InteractingWith.Y);

                    if (LActionTimer <= 5)
                    {
                        List<Point> list = base.GetRoom().GetGameMap().getWalkableList();
                        if (list.Count == 0)
                            return;
                        int randomNumber = new Random(DateTime.Now.Millisecond + this._virtualId ^ 2).Next(0, list.Count - 1);
                        base.GetRoomUser().MoveTo(list[randomNumber].X, list[randomNumber].Y, true);

                        LActionTimer++;
                    }
                    else
                    {
                        if (!Robbed)
                        {
                            if (RoleplayManager.Distance(Pos1, Pos2) > 1)
                            {
                                base.GetRoomUser().MoveTo(Pos2.X, Pos2.Y + 1, true);

                            }
                            else
                            {
                                base.GetRoomUser().MoveTo(Pos2.X, Pos2.Y + 1, true);
                                Robbed = true;
                            }
                        }

                        if (!Robbed && !base.GetRoomUser().IsWalking)
                        {
                            base.GetBotData().TimesTried++;
                        }

                        if (base.GetBotData().TimesTried >= 20)
                        {
                            base.GetRoomUser().Chat(null, "Filho da puta!", true, 1);
                            System.Threading.Thread.Sleep(2000);
                            RoleplayManager.KickBotFromRoom(base.GetRoomUser().BotData.Name, base.GetBotData().RoomId);
                        }

                        if (Robbed && !Done)
                        {
                            Random _s = new Random();
                            int Amnt = _s.Next(5, 100);

                            if (base.GetBotData().InteractingWith.GetClient() == null)
                            {
                                Done = true;
                            }

                            if (base.GetBotData().InteractingWith.GetClient().GetHabbo() == null)
                            {
                                Done = true;
                            }

                            if (base.GetBotData().InteractingWith.GetClient().GetHabbo().Credits - Amnt <= 0)
                            {
                                base.GetRoomUser().Chat(null, "Droga, você está sem dinheiro, vou deixar você ir desta vez!", true, 1);
                                Done = true;
                                return;
                            }

                            base.GetRoomUser().Chat(null, "*Rouba $" + Amnt + " de " + base.GetBotData().InteractingWith.GetClient().GetHabbo().UserName + " e foge*", true, 1);
                            RoleplayManager.GiveMoney(base.GetBotData().InteractingWith.GetClient(), -Amnt);
                            Done = true;
                        }

                        if (Done)
                        {
                            RoleplayManager.KickBotFromRoom(base.GetRoomUser().BotData.Name, base.GetBotData().RoomId);

                        }

                    }

                }
                else
                {
                    if (base.GetBotData() != null)
                    {
                        if (base.GetBotData().Name.Contains("HOBO"))
                        {
                            if (base.GetBotData().InteractingWith == null && base.GetBotData().Name.Contains("HOBO") || base.GetBotData().InteractingWith.GetClient().GetHabbo().CurrentRoomId != base.GetBotData().RoomId)
                            {

                                RoleplayManager.KickBotFromRoom(base.GetRoomUser().BotData.Name, base.GetBotData().RoomId);
                            }
                        }
                    }
                }

                #endregion

                #region Hobo 2

                if (base.GetBotData().InteractingWith != null && base.GetBotData().Name.Contains("HOBO 2"))
                {
                    HabboHotel.PathFinding.Vector2D Pos1 = new HabboHotel.PathFinding.Vector2D(base.GetRoomUser().X, base.GetRoomUser().Y);

                    HabboHotel.PathFinding.Vector2D Pos2 = new HabboHotel.PathFinding.Vector2D(base.GetBotData().InteractingWith.X, base.GetBotData().InteractingWith.Y);

                    if (LActionTimer <= 5)
                    {
                        List<Point> list = base.GetRoom().GetGameMap().getWalkableList();
                        if (list.Count == 0)
                            return;
                        int randomNumber = new Random(DateTime.Now.Millisecond + this._virtualId ^ 2).Next(0, list.Count - 1);
                        base.GetRoomUser().MoveTo(list[randomNumber].X, list[randomNumber].Y, true);

                        LActionTimer++;
                    }
                    else
                    {
                        if (!Robbed)
                        {
                            if (RoleplayManager.Distance(Pos1, Pos2) > 1)
                            {
                                base.GetRoomUser().MoveTo(Pos2.X, Pos2.Y + 1, true);

                            }
                            else
                            {
                                base.GetRoomUser().MoveTo(Pos2.X, Pos2.Y + 1, true);
                                Robbed = true;
                            }
                        }

                        if (!Robbed && !base.GetRoomUser().IsWalking)
                        {
                            base.GetBotData().TimesTried++;
                        }

                        if (base.GetBotData().TimesTried >= 20)
                        {
                            base.GetRoomUser().Chat(null, "Filho da puta!", true, 1);
                            System.Threading.Thread.Sleep(2000);
                            RoleplayManager.KickBotFromRoom(base.GetRoomUser().BotData.Name, base.GetBotData().RoomId);
                        }

                        if (Robbed && !Done)
                        {
                            Random _s = new Random();
                            int AmntBank = _s.Next(5, 500);

                            if (base.GetBotData().InteractingWith.GetClient() == null)
                            {
                                Done = true;
                            }

                            if (base.GetBotData().InteractingWith.GetClient().GetHabbo() == null)
                            {
                                Done = true;
                            }

                            if (base.GetBotData().InteractingWith.GetClient().GetRoleplay().Bank - AmntBank <= 0)
                            {
                                base.GetRoomUser().Chat(null, "Droga, você está sem dinheiro, vou deixar você ir desta vez!", true, 1);
                                Done = true;
                                return;
                            }

                            base.GetRoomUser().Chat(null, "*Rouba $" + AmntBank + " de " + base.GetBotData().InteractingWith.GetClient().GetHabbo().UserName + " e foge*", true, 1);
                            base.GetBotData().InteractingWith.GetClient().GetRoleplay().SaveQuickStat("bank", "" + (base.GetBotData().InteractingWith.GetClient().GetRoleplay().Bank - AmntBank));
                            base.GetBotData().InteractingWith.GetClient().GetRoleplay().Bank -= AmntBank;
                            Done = true;
                        }

                        if (Done)
                        {
                            RoleplayManager.KickBotFromRoom(base.GetRoomUser().BotData.Name, base.GetBotData().RoomId);
                        }

                    }

                }
                else
                {
                    if (base.GetBotData() != null)
                    {
                        if (base.GetBotData().Name.Contains("HOBO 2"))
                        {
                            if (base.GetBotData().InteractingWith == null && base.GetBotData().Name.Contains("HOBO 2") || base.GetBotData().InteractingWith.GetClient().GetHabbo().CurrentRoomId != base.GetBotData().RoomId)
                            {
                                RoleplayManager.KickBotFromRoom(base.GetRoomUser().BotData.Name, base.GetBotData().RoomId);
                            }
                        }
                    }
                }

                #endregion

                #region Free Roam Bot



                #endregion

                #region Store Bot

                if (robber != null)
                {

                    if (robber.GetHabbo() == null)
                    {
                        beingRobbed = false;
                        gatherNum = 0;
                        gatherMsg = 0;
                        amount = 0;
                        return;
                    }

                    if (robber.GetHabbo().GetRoomUser() == null)
                    {
                        beingRobbed = false;
                        gatherNum = 0;
                        gatherMsg = 0;
                        amount = 0;
                        return;
                    }


                    if (robber.GetRoleplay() == null)
                    {
                        beingRobbed = false;
                        gatherNum = 0;
                        gatherMsg = 0;
                        amount = 0;
                        return;
                    }

                    if (robber.GetHabbo().GetRoomUser().Frozen || robber.GetRoleplay().Jailed || robber.GetRoleplay().Dead)
                    {

                        amount = 0;
                        gatherMsg = 0;
                        gatherNum = 0;
                        robber.GetRoleplay().robbingStore = 0;
                        robber = null;
                        base.GetRoomUser().ApplyEffect(13);
                        disableRob = true;
                        beingRobbed = false;
                        return;
                    }

                    if (gatherMsg <= 0)
                    {
                        base.GetRoomUser().Chat(null, "*Begins taking money from cash register*", true, 1);

                        if (gatherSpeed == 1)
                        {
                            gatherMsg = 10;
                        }

                        if (gatherSpeed == 2)
                        {
                            gatherMsg = 5;
                        }

                        if (gatherSpeed == 0)
                        {
                            gatherMsg = 20;
                        }


                        gatherNum++;
                    }
                    else
                    {
                        gatherMsg--;
                    }


                    if (gatherNum == 5)
                    {
                        int amount = new Random().Next(0, 100);

                        if (attack <= 6)
                        {
                            if (robber == null)
                            {
                                amount = 0;
                                gatherMsg = 0;
                                gatherNum = 0;
                                robber.GetRoleplay().robbingStore = 0;
                                robber = null;
                                base.GetRoomUser().ApplyEffect(13);
                                disableRob = true;
                                beingRobbed = false;
                                return;
                            }
                            else
                            {
                                attackCount++;

                                base.GetRoomUser().Chat(null, "*Takes out their pump shotgun*", true, 1);
                                base.GetRoomUser().ApplyEffect(164);

                                base.GetRoomUser().Chat(null, "*Shoots " + robber.GetHabbo().UserName + ", killing them!*", true, 1);

                                robber.GetRoleplay().DeadFigSet = false;
                                robber.GetRoleplay().DeadSeconds = 60;
                                robber.GetRoleplay().DeadTimer = 2;
                                robber.GetRoleplay().Dead = true;
                                robber.GetRoleplay().SaveStatusComponents("dead");
                                RoleplayManager.HandleDeath(robber);
                                robber.SendNotif("You have been killed by a store clerk.");
                                return;
                            }
                        }

                        base.GetRoomUser().Chat(null, "*Drops money* Theres your money, now leave me alone!! [-$" + amount + "]", true, 1);
                        RoleplayManager.GiveMoney(robber, +amount);

                        amount = 0;
                        gatherMsg = 0;
                        gatherNum = 0;
                        robber.GetRoleplay().robbingStore = 0;
                        robber = null;
                        base.GetRoomUser().ApplyEffect(13);
                        disableRob = true;
                        beingRobbed = false;
                    }
                }
                else
                {
                    beingRobbed = false;
                    gatherNum = 0;
                    gatherMsg = 0;
                    amount = 0;
                }

                lastRobbed++;
                if (lastRobbed > 5000 && disableRob)
                {
                    disableRob = false;
                    lastRobbed = 0;
                    base.GetRoomUser().ApplyEffect(0);
                }


            }

                #endregion
            
            #region Eatery Bot
            if (base.GetRoom() == null)
            {
                return;
            }
            if (base.GetRoom().RoomData.Description.Contains("EATERY"))
            {
                if (event_serving && event_serving_orderPlaced && !event_serving_delivered && !GetRoomUser().IsWalking)
                {

                    if (!resetted)
                    {
                        if (order_tick >= 15)
                        {
                            resetted = true;
                            GetRoomUser().Chat(null, "An error occured with pathfinding and I was unable to get to you, sorry about that!", true, 1);
                            serving.SendWhisper("Order has been aborted! Try sitting closer maybe?");
                            ResetVariablesFor("eatery");
                        }
                        else
                        {
                            order_tick++;
                        }
                    }
                }

                if (event_serving)
                {

                    if (event_serving_orderPlaced && !event_serving_delivered)
                    {
                        RoomUser User = serving.GetHabbo().GetRoomUser();


                        #region Delete Serve Session
                        if (serving == null)
                        {
                            event_serving = false;
                            ResetVariablesFor("eatery");
                            return;
                        }
                        if (serving.GetHabbo() == null)
                        {
                            event_serving = false;
                            ResetVariablesFor("eatery");
                            return;
                        }
                        if (serving.GetRoleplay() == null)
                        {
                            event_serving = false;
                            ResetVariablesFor("eatery");
                            return;
                        }
                        if (serving.GetHabbo().CurrentRoom == null)
                        {
                            event_serving = false;
                            ResetVariablesFor("eatery");
                            return;
                        }
                        if (serving.GetHabbo().CurrentRoom != GetRoom())
                        {
                            event_serving = false;
                            ResetVariablesFor("eatery");
                            return;
                        }
                        #endregion
                        if (!event_set_items)
                        {
                            event_serving_plate = retrieveItem("diner_tray_0", true, User);
                            event_serving_oneway = retrieveItem("one_way_door*1");
                            event_serving_oneway2 = retrieveItem("one_way_door*4");
                            event_set_items = true;

                            if (event_serving_plate == null)
                            {
                                GetRoomUser().Chat(null, "Can you sit near an empty plate please?", true, 1);
                                ResetVariablesFor("eatery");
                                return;
                            }
                        }
                        else
                        {

                            /*
                            Vector2D TargetVector = new Vector2D(event_serving_plate.GetX,event_serving_plate.GetY);
                            Vector2D CurrentPos = new Vector2D(GetRoomUser().X,GetRoomUser().Y);
                            if(Plus.Misc.Distance(TargetVector,CurrentPos) > 2)
                            {
                                    
                            }*/

                            if (Undefined_Timer_Tick_2 != 2)
                            {
                                passing_cur_gate = false;
                                Undefined_Timer_Tick_2 = 2;
                            }

                            if (!event_serving_passedGate1)
                            {

                                HandleGatePass(event_serving_oneway);
                            }
                            else
                            {
                                GetRoomUser().AllowOverride = false;
                                if (!event_serving_atTable)
                                {

                                    if (GetRoomUser().X == RoleplayManager.GetPosSquare("behind", User).X && GetRoomUser().Y == RoleplayManager.GetPosSquare("behind", User).Y)
                                    {
                                        GetRoomUser().RotBody = User.RotBody;
                                        GetRoomUser().RotHead = User.RotBody;
                                        event_serving_atTable = true;
                                    }
                                    else
                                    {
                                        GetRoomUser().MoveTo(RoleplayManager.GetPosSquare("behind", User));
                                    }
                                }
                                else
                                {

                                    if (!event_serving_delivered)
                                    {
                                        Food nFood = Substances.GetFoodByUName(event_serving_fooditem);

                                        RoleplayManager.ReplaceItem(serving, event_serving_plate, nFood.Item_Name);
                                        event_serving_delivered = true;
                                        GetRoomUser().Chat(null, "There you go, enjoy!", true, 1);

                                        ServerMessage action = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer"));
                                        action.AppendInteger(GetRoomUser().VirtualId);
                                        action.AppendInteger(2);
                                        GetRoom().SendMessage(action);

                                        passing_cur_gate = false;

                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        if (!event_serving_passedGate2)
                        {
                            HandleGatePass(event_serving_oneway2);
                        }
                        else
                        {
                            ResetVariablesFor("eatery");
                        }
                    }
                }
            }
            #endregion

        }
        private void StopTimerTick()
        {
            if (_chatTimer == null) return;
            _chatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _chatTimer.Dispose();
            _chatTimer = null;
        }

        /// <summary>
        /// Chats the timer tick.
        /// </summary>
        /// <param name="o">The o.</param>
        private void ChatTimerTick(object o)
        {
            if (GetBotData() == null || GetRoomUser() == null || GetBotData().WasPicked || GetBotData().RandomSpeech == null ||
                !GetBotData().RandomSpeech.Any())
            {
                StopTimerTick();
                return;
            }

            if (GetRoom() != null && GetRoom().MutedBots)
                return;

            var randomSpeech = GetBotData().GetRandomSpeech(GetBotData().MixPhrases);

            try
            {
                switch (randomSpeech)
                {
                    case ":sit":
                        {
                            var user = GetRoomUser();
                            if (user.RotBody % 2 != 0) user.RotBody--;

                            user.Z = GetRoom().GetGameMap().SqAbsoluteHeight(user.X, user.Y);
                            if (!user.Statusses.ContainsKey("sit"))
                            {
                                user.UpdateNeeded = true;
                                user.Statusses.Add("sit", "0.55");
                            }
                            user.IsSitting = true;
                            return;
                        }
                    case ":stand":
                        {
                            var user = GetRoomUser();
                            if (user.IsSitting)
                            {
                                user.Statusses.Remove("sit");
                                user.IsSitting = false;
                                user.UpdateNeeded = true;
                            }
                            else if (user.IsLyingDown)
                            {
                                user.Statusses.Remove("lay");
                                user.IsLyingDown = false;
                                user.UpdateNeeded = true;
                            }
                            return;
                        }
                }

                if (GetRoom() != null)
                {
                    randomSpeech = randomSpeech.Replace("%user_count%",
                        GetRoom().GetRoomUserManager().GetRoomUserCount().ToString());
                    randomSpeech = randomSpeech.Replace("%item_count%",
                        GetRoom().GetRoomItemHandler().TotalItems.ToString());
                    randomSpeech = randomSpeech.Replace("%floor_item_count%",
                        GetRoom().GetRoomItemHandler().FloorItems.Keys.Count.ToString());
                    randomSpeech = randomSpeech.Replace("%wall_item_count%",
                        GetRoom().GetRoomItemHandler().WallItems.Keys.Count.ToString());

                    if (GetRoom().RoomData != null)
                    {
                        randomSpeech = randomSpeech.Replace("%roomname%", GetRoom().RoomData.Name);
                        randomSpeech = randomSpeech.Replace("%owner%", GetRoom().RoomData.Owner);
                    }
                }
                if (GetBotData() != null) randomSpeech = randomSpeech.Replace("%name%", GetBotData().Name);

                GetRoomUser().Chat(null, randomSpeech, false, 0, 0);
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
            }
        }
    }
}