using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.RoomInvokedItems;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.RoomBots;
using Plus.HabboHotel.Pets;
using Plus.Messages;
using Plus.Messages.Parsers;
using Plus.HabboHotel.Roleplay.Bots;
using Plus.HabboHotel.Roleplay.Combat;
using Plus.Database;
using System.Drawing;
using Plus.Util;
using System.Threading;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay;
using Plus.HabboHotel.Roleplay.Timers;
using Plus.HabboHotel.Roleplay.Radio;
using System.Text.RegularExpressions;
using System.Net;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Roleplay.Misc
{
    public class RoleplayManager
    {
        public static int GlobalWorkSecondsInterval = 60;
        public static bool GlobalChargeForTaxi = false;
        public static int GlobalTaxiWaitTime = 25;
        public static int VaultRobbery = 5000;
        public static int NukesOccurred = 0;
        public static int SlotsMachineCost = 50;
        public static int SlotsMachinePrizeDivider = 8;
        public static int SlotsMachineJackpotDivider = 3;

        public static ConcurrentDictionary<uint, DateTime> EmptyTrashCans = new ConcurrentDictionary<uint, DateTime>();
        internal static ConcurrentDictionary<RoomBot, RoomUser> ParamedicBots = new ConcurrentDictionary<RoomBot, RoomUser>();
        public static ConcurrentDictionary<string, int> TimeIntensities = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, string> WantedListData = new ConcurrentDictionary<string, string>();
        public static bool PurgeTime = false;
        public static bool ZombieInfection = false;
        public static bool GVIPAlertsDisabled = false;


        public static void saveAllStats()
        {
            lock (Plus.GetGame().GetClientManager().Clients.Values)
            {
                foreach (GameClient mClient in Plus.GetGame().GetClientManager().Clients.Values)
                {
                    if (mClient == null)
                        continue;
                    if (mClient.GetHabbo() == null)
                        continue;
                    if (mClient.GetRoleplay() == null)
                        continue;

                    mClient.GetRoleplay().SaveStats();
                }
            }
        }

        public static bool WithinAttackDistance(RoomUser roomUser, RoomUser TargetroomUser)
        {
            if (roomUser.X + 1 == TargetroomUser.X && roomUser.Y == TargetroomUser.Y
                  || roomUser.X == TargetroomUser.X - 1 && roomUser.Y == TargetroomUser.Y + 1
                  || roomUser.X == TargetroomUser.X + 1 && roomUser.Y == TargetroomUser.Y - 1
                  || roomUser.X - 1 == TargetroomUser.X && roomUser.Y == TargetroomUser.Y ||
                  roomUser.Y + 1 == TargetroomUser.Y && roomUser.X == TargetroomUser.X ||
                  roomUser.Y - 1 == TargetroomUser.Y && roomUser.X == TargetroomUser.X ||
                  roomUser.X == TargetroomUser.X && roomUser.Y == TargetroomUser.Y)
            {
                return true;

            }

           return false;
        }

        public static bool isOnMoon(RoomItem item, RoomUser roomuser, GameClient session, uint BaseID = 1638)
        {
            uint baseItemID = BaseID;
            if (item != null && roomuser != null && session != null)
            {
                if (item.BaseItem == baseItemID)
                {
                    if (item.X == roomuser.X && item.Y == roomuser.Y || item.X == roomuser.X && item.Y + 1 == roomuser.Y ||
                        item.X + 1 == roomuser.X && item.Y + 1 == roomuser.Y ||
                        item.X + 1 == roomuser.X && item.Y == roomuser.Y)
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static bool isOnForrest(RoomItem item, RoomUser roomuser, GameClient session, uint BaseID = 1638)
        {
            uint baseItemID = BaseID;
            if (item != null && roomuser != null && session != null)
            {
                if (item.BaseItem == baseItemID)
                {
                    if (item.X == roomuser.X && item.Y == roomuser.Y || item.X == roomuser.X && item.Y + 1 == roomuser.Y ||
                        item.X + 1 == roomuser.X && item.Y + 1 == roomuser.Y ||
                        item.X + 1 == roomuser.X && item.Y == roomuser.Y)
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }


        public static bool isOnMine(RoomItem item, RoomUser roomuser, GameClient session)
        {
            uint baseItemID = 555444;
            if (item != null && roomuser != null && session != null)
            {
                if (item.BaseItem == baseItemID)
                {
                    if (item.X == roomuser.X && item.Y == roomuser.Y)
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static void init()
        {
            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE rp_stats SET last_cw_team = '" + String.Empty + "' WHERE last_cw_team != '" + String.Empty + "'");
            }

            FightPetManager.load();
            Substances.load();
            Bounties.LoadBounty();
        }
        public static Point GetPosSquare(string pos, RoomUser User)
        {

            Point Sq = new Point(User.X, User.Y);
            int Rot = User.RotBody;

            switch(pos)
            {
                case "behind":

                    if (Rot == 0)
                    {
                        Sq.Y++;
                    }
                    else if (Rot == 2)
                    {
                        Sq.X--;
                    }
                    else if (Rot == 4)
                    {
                        Sq.Y--;
                    }
                    else if (Rot == 6)
                    {
                        Sq.X++;
                    }
                    break;

                case "infront":

                    
                if (Rot == 0)
                {
                    Sq.Y--;
                }
                else if (Rot == 2)
                {
                    Sq.X++;
                }
                else if (Rot == 4)
                {
                    Sq.Y++;
                }
                else if (Rot == 6)
                {
                    Sq.X--;
                }

                    break;
            }

            return Sq;
        }

        public static int Distance(Vector2D Pos1, Vector2D Pos2)
        {
            return Math.Abs(Pos1.X - Pos2.X) + Math.Abs(Pos1.Y - Pos2.Y);
        }

        public static int UserDistance(GameClient User1, GameClient User2)
        {
            RoomUser one = null;
            RoomUser two = null;
            one = User1.GetHabbo().GetRoomUser();
            two = User2.GetHabbo().GetRoomUser();

            Vector2D Pos1 = new Vector2D(one.X,one.Y);
            Vector2D Pos2 = new Vector2D(two.X,two.Y);

            return Math.Abs(Pos1.X - Pos2.X) + Math.Abs(Pos1.Y - Pos2.Y);
        }

        public static int UserDistance(RoomUser one, RoomUser two)
        {
            Vector2D Pos1 = new Vector2D(one.X, one.Y);
            Vector2D Pos2 = new Vector2D(two.X, two.Y);

            return Math.Abs(Pos1.X - Pos2.X) + Math.Abs(Pos1.Y - Pos2.Y);
        }

        public static bool BypassRights(GameClient Session)
        {
            DataRow Row = null;

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rp_special_rights WHERE userid = " + Session.GetHabbo().Id + "");
                Row = dbClient.GetRow();
                if(Row != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool PetExists(string PetName)
        {
            Dictionary<string, uint> TPets = new Dictionary<string, uint>();
            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM bots");
                DataTable TData = dbClient.GetTable();

                foreach (DataRow TPetRow in TData.Rows)
                {
                    if (!TPets.ContainsKey(Convert.ToString(TPetRow["name"]).ToLower()))
                    {
                        TPets.Add(Convert.ToString(TPetRow["name"]).ToLower(), Convert.ToUInt32(TPetRow["id"]));
                    }
                }
            }

            if (TPets.ContainsKey(PetName.ToLower()))
            {
                return true;
            }

            return false;
        }

        public static bool CreateQuickPet(GameClient Session, string PetType, string PetName)
        {
            string ExtraData = null;
            string Itemname = "a0 pet0";
            var petId = int.Parse(Itemname.Replace("a0 pet", string.Empty));
            var PetData = ":1:D98961";

            Itemname = FightPetManager.PetData[PetType.ToLower()].Data1;
            petId = int.Parse(Itemname.Replace("a0 pet", string.Empty));
            PetData = FightPetManager.PetData[PetType.ToLower()].Data2;

            //  

            ExtraData = PetName + PetData;

            #region Check if pet exists
            if (PetExists(PetName.ToLower()))
            {
                return false;
            }
            #endregion

            #region Create pet

            var petData = PetData.Split(':');

            Session.SendWhisper("PETID: " + petId + " ;; PETDATA1: " + petData[1] + " ;; PETDATA2: " + petData[2]);


            var generatedPet = Catalogs.Catalog.CreatePet(Session.GetHabbo().Id, PetName, petId, petData[1], petData[2], 0);

            var list = new List<UserItem>();

            //
            // BOOKMARK TODO: REDO PET LOADING @ LOGIN
            Session.GetHabbo().GetInventoryComponent().AddPet(generatedPet);
            // BOOKMARK
            list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0, 320, "0", 0u, true, false, 0, 0, string.Empty));


            #endregion

            #region Refresh roleplay pets
            Dictionary<string, uint> Pets = new Dictionary<string, uint>();

            try
            {
                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM bots WHERE user_id = '" + Session.GetHabbo().Id + "'");
                    DataTable Data = dbClient.GetTable();


                    foreach (DataRow PetRow in Data.Rows)
                    {
                        if (!Pets.ContainsKey(Convert.ToString(PetRow["name"]).ToLower()))
                            Pets.Add(Convert.ToString(PetRow["name"]).ToLower(), Convert.ToUInt32(PetRow["id"]));
                    }

                    dbClient.RunFastQuery("UPDATE bots SET ai_type = 'fightpet', rp_type='" + PetType.ToLower() + "' WHERE id = " + generatedPet.PetId + "");


                }

                lock (Session.GetRoleplay().MyPets)
                {
                    Session.GetRoleplay().MyPets.Clear();
                    Session.GetRoleplay().MyPets = Pets;
                }
            }
            catch (Exception e)
            {

            }

            #endregion

            #region Final changes

            Session.GetMessageHandler().GetResponse().AppendInteger(list.Count);
            foreach (var current3 in list) Session.GetMessageHandler().GetResponse().AppendInteger(current3.Id);
            Session.GetMessageHandler().SendResponse();
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
            Session.GetHabbo().GetInventoryComponent().SerializePetInventory();


            #endregion

            #region Bring out pet
            uint PetId = Session.GetRoleplay().MyPets[PetName.ToLower()];

            FightPetManager Manager = new FightPetManager();

            RoomBot Pet = Manager.DeployBotToRoom(Session, PetName.ToLower(), Session.GetHabbo().CurrentRoomId);


            if (Pet == null)
            {
                return true;
            }


            Pet.RoomUser.Chat(null, "Thank you for adopting me master!! I will serve you forever and always!", true, 0, 0);
            #endregion


            return true;
        }

        public static bool SendParamedicBot(GameClient Session)
        {

            #region Null Checks
            if (Session == null)
                return false;
            if (Session.GetHabbo() == null)
                return false;
            if (Session.GetHabbo().CurrentRoom == null)
                return false;
            if (Session.GetHabbo().CurrentRoom.GetGameMap() == null)
                return false;
            if (Session.GetHabbo().CurrentRoom.GetGameMap().Model == null)
                return false;
            #endregion

            int ParaMedic = new Random().Next(1, 3);
            string Name = "Sam";
            string Figure = "sh-290-62.hd-185-1.lg-280-82.ca-1819-62.ch-225-82.hr-851-40";

            if (RoleplayData.Data.ContainsKey("paramedic_name" + ParaMedic) && RoleplayData.Data.ContainsKey("paramedic_figure" + ParaMedic))
            {
                Name = Convert.ToString(RoleplayData.Data["paramedic_name" + ParaMedic]);
                Figure = Convert.ToString(RoleplayData.Data["paramedic_figure" + ParaMedic]);
            }

            RoomBot NewParamedic = MakeQuickBot("[Paramedico] " + Name,
                AIType.HospitalBot,
                Session.GetHabbo().CurrentRoomId,
                Figure,
               Session.GetHabbo().CurrentRoom.GetGameMap().Model.DoorX,
               Session.GetHabbo().CurrentRoom.GetGameMap().Model.DoorY);

            NewParamedic.InteractingWith = Session.GetHabbo().GetRoomUser();
            NewParamedic.HospBotType = "paramedic_bot";
            SendBotToRoom(NewParamedic, Session.GetHabbo().CurrentRoomId);

            return true;
        }

        public static void HandleDeath(GameClient TargetSession, string msg = "")
        {
            if(msg.Length > 3)
            {
                TargetSession.SendNotif(msg);
            }
            RoomUser User = null;

            if (TargetSession.GetHabbo() != null)
            {
                if (TargetSession.GetHabbo().GetRoomUser() != null)
                {
                    User = TargetSession.GetHabbo().GetRoomUser();
                }
            }

            if (User != null || TargetSession != null)
            {

                if (TargetSession.GetRoleplay().Equiped != null)
                {
                    TargetSession.GetRoleplay().Equiped = null;
                    User.ApplyEffect(0);
                }


                if (User == null)
                    return;


                RoleplayManager.MakeLay(User);

                User.Frozen = true;
                TargetSession.GetRoleplay().SaveStatusComponents("dead");
                /*RoomBot NewParamedic = Misc.MakeQuickBot("[Paramedic] Sam", AIType.HospitalBot, TargetSession.GetHabbo().CurrentRoomId, "ch-225-82.-180-2.ca-1819-63.sh-290-92.lg-280-82.-3163-45", User.GetClient().GetHabbo().CurrentRoom.GetGameMap().Model.DoorX, User.GetClient().GetHabbo().CurrentRoom.GetGameMap().Model.DoorY);
                NewParamedic.InteractingWith = User;
                NewParamedic.HospBotType = "paramedic_bot";
                Misc.SendBotToRoom(NewParamedic, TargetSession.GetHabbo().CurrentRoomId);*/

                if (TargetSession == null)
                {
                    //int lol = 1;
                }
                else
                {
                    if (TargetSession.GetHabbo().CurrentRoom != null)
                    {
                        RoleplayManager.Shout(TargetSession, "*Desmaiou e é entregue no hospital*");
                    }
                }

                TargetSession.GetRoleplay().Transport(1, 1);

            }
            else
            {

                if (User == null)
                    return;

                RoleplayManager.MakeLay(User);
                User.Frozen = true;
                TargetSession.GetRoleplay().SaveStatusComponents("dead");
                /*RoomBot NewParamedic = Misc.MakeQuickBot("[Paramedic] Sam", AIType.HospitalBot, TargetSession.GetHabbo().CurrentRoomId, "ch-225-82.-180-2.ca-1819-63.sh-290-92.lg-280-82.-3163-45", User.GetClient().GetHabbo().CurrentRoom.GetGameMap().Model.DoorX, User.GetClient().GetHabbo().CurrentRoom.GetGameMap().Model.DoorY);
                NewParamedic.InteractingWith = User;
                NewParamedic.HospBotType = "paramedic_bot";
                Misc.SendBotToRoom(NewParamedic, TargetSession.GetHabbo().CurrentRoomId);*/
                if (TargetSession.GetHabbo().CurrentRoom != null)
                {
                    RoleplayManager.Shout(TargetSession, "*Desmaiou e é entregue no hospital*");
                }
                TargetSession.GetMessageHandler().PrepareRoomForUser(1, "");
            }
        }

        public static RoomUser GetBot(string Name, Room Room)
        {
            RoomUser Targ = null;

            foreach (RoomUser User in Room.GetRoomUserManager().GetBots())
            {
                if (User == null)
                    continue;
                if (!User.IsBot)
                    continue;
                if (User.BotData == null)
                    continue;
                if (User.BotData.Name.ToLower() == Name.ToLower())
                    Targ = User;
            }


            return Targ;
        }

        public static bool ParamsMet(string[] Params, int LengthNeeded)
        {
            if (Params == null)
                return false;

            if(Params.Length < LengthNeeded + 1)
            {
                return false;
            }
            return true;
        }

        public static void Shout(GameClient Session, string Speech, int Bubble = 0)
        {
            if (Speech == null)
                return;

            if (Session == null)
                return;

            if (Session.GetHabbo() == null)
                return;

            if (Session.GetHabbo().CurrentRoom == null)
                return;

            Room Room = GenerateRoom(Session.GetHabbo().CurrentRoomId);

            if (Room == null)
                return;

            RoomUser RoomUser = Session.GetHabbo().GetRoomUser();

            if (RoomUser == null)
                return;

            if (!RoomUser.IsBot && RoomUser.IsFlooded && RoomUser.FloodExpiryTime <= Plus.GetUnixTimeStamp())
                RoomUser.IsFlooded = false;
            else if (!RoomUser.IsBot && RoomUser.IsFlooded)
                return; // ciao flooders!

            if (!RoomUser.IsBot && RoomUser.IsGunReloaded && RoomUser.ReloadExpiryTime <= Plus.GetUnixTimeStamp())
            {
                RoomUser.IsGunReloaded = false;
            }
            else if (!RoomUser.IsBot && RoomUser.IsGunReloaded)
                return; // ciao flooders!

            RoomUser.Chat(Session, Speech, true, -1, Bubble);                         
        }

        public static void commandShout(GameClient Session, string Speech, int Bubble = 0)
        {
            if (Speech == null)
                return;

            if (Session == null)
                return;

            if (Session.GetHabbo() == null)
                return;

            if (Session.GetHabbo().CurrentRoom == null)
                return;

            Room Room = GenerateRoom(Session.GetHabbo().CurrentRoomId);

            if (Room == null)
                return;

            RoomUser RoomUser = Session.GetHabbo().GetRoomUser();

            if (RoomUser == null)
                return;

            RoomUser.commandChat(Session, Speech, true, -1, RoomUser.LastBubble);
        }


        public static GameClient GenerateSession(string Username)
        {
            GameClient Client = null;
            try
            {
                Client = Plus.GetGame().GetClientManager().GetClientByUserName(Username);
            }
            catch(Exception e)
            {
                Client = null;
            }

            return Client;
        }
        public static GameClient GenerateSession(uint Id)
        {
            GameClient Client = null;
            Client = Plus.GetGame().GetClientManager().GetClientByUserId(Id);
            return Client;
        }
        public static Room GenerateRoom(uint RoomId)
        {
            Room Room = Plus.GetGame().GetRoomManager().LoadRoom(RoomId);
            return Room;
        }
        public static string GetStatByID(int userid, string inf)
        {
            DataRow Row = null;
            string ret = "";

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM users WHERE id = '" + userid + "'");
                Row = dbClient.GetRow();
                ret = (string)Row[inf];
                if (Row != null)
                {
                    return ret;
                }
            }
            return ret;
        }

        public static bool CanInteract(GameClient Me, GameClient TargetSession, bool SameRoom = false)
        {
            if (TargetSession == null)
                return false;

            if (TargetSession.GetHabbo() == null)
                return false;

            if (TargetSession.GetHabbo().GetRoomUser() == null)
                return false;

            if (TargetSession.GetHabbo().CurrentRoom == null)
                return false;


            if(SameRoom)
            {
                if(TargetSession.GetHabbo().CurrentRoom != Me.GetHabbo().CurrentRoom)
                {
                    return false;
                }
            }

            return true;
        }

        public static string SplitFigure(string _Figure)
        {
            string _Uni;
            string FigurePartHair = _Figure;
            string GetHairPart;

            if (_Figure.ToLower().Contains("hr"))
            {
                GetHairPart = Regex.Split(_Figure, "hr")[1];
                FigurePartHair = GetHairPart.Split('.')[0];
            }
            string FigurePartHead = _Figure;
            string GetHeadPart;

            if (_Figure.ToLower().Contains("hd"))
            {
                GetHeadPart = Regex.Split(_Figure, "hd")[1];
                FigurePartHead = GetHeadPart.Split('.')[0];
            }
            string FigurePartHeadAcc = _Figure;
            string GetHeadAccPart;

            if (_Figure.ToLower().Contains("he"))
            {
                GetHeadAccPart = Regex.Split(_Figure, "he")[1];
                FigurePartHeadAcc = GetHeadAccPart.Split('.')[0];
            }
            string FigurePartHat = _Figure;
            string GetHatPart;

            if (_Figure.ToLower().Contains("ha"))
            {
                GetHatPart = Regex.Split(_Figure, "ha")[1];
                FigurePartHat = GetHatPart.Split('.')[0];
            }
            string FigurePartGlasses = _Figure;
            string GetGlassesPart;

            if (_Figure.ToLower().Contains("ea"))
            {
                GetGlassesPart = Regex.Split(_Figure, "ea")[1];
                FigurePartHead = GetGlassesPart.Split('.')[0];
            }
            string FigurePartBeard = _Figure;
            string GetBeardPart;

            if (_Figure.ToLower().Contains("fa"))
            {
                GetBeardPart = Regex.Split(_Figure, "fa")[1];
                FigurePartBeard = GetBeardPart.Split('.')[0];
            }
            string FigurePartShirt = _Figure;
            string GetShirtPart;

            if (_Figure.ToLower().Contains("ch"))
            {
                GetShirtPart = Regex.Split(_Figure, "ch")[1];
                FigurePartShirt = GetShirtPart.Split('.')[0];
            }
            string FigurePartJacket = _Figure;
            string GetJacketPart;

            if (_Figure.ToLower().Contains("cc"))
            {
                GetJacketPart = Regex.Split(_Figure, "cc")[1];
                FigurePartJacket = GetJacketPart.Split('.')[0];
            }
            string FigurePartNecklace = _Figure;
            string GetNecklacePart;

            if (_Figure.ToLower().Contains("ca"))
            {
                GetNecklacePart = Regex.Split(_Figure, "ca")[1];
                FigurePartNecklace = GetNecklacePart.Split('.')[0];
            }
            string FigurePartPants = _Figure;
            string GetPantsPart;

            if (_Figure.ToLower().Contains("lg"))
            {
                GetPantsPart = Regex.Split(_Figure, "lg")[1];
                FigurePartPants = GetPantsPart.Split('.')[0];
            }
            string FigurePartBelt = _Figure;
            string GetBeltPart;

            if (_Figure.ToLower().Contains("wa"))
            {
                GetBeltPart = Regex.Split(_Figure, "wa")[1];
                FigurePartBelt = GetBeltPart.Split('.')[0];
            }
            string FigurePartShoes = _Figure;
            string GetShoesPart;

            if (_Figure.ToLower().Contains("sh"))
            {
                GetShoesPart = Regex.Split(_Figure, "sh")[1];
                FigurePartShoes = GetShoesPart.Split('.')[0];
            }

            _Uni = Convert.ToString("ha" + FigurePartHat + "." + "he" + FigurePartHeadAcc + "." + "hr" + FigurePartHair + "." + "hd" + FigurePartHead + "." + "ea" + FigurePartGlasses + "." + "fa" + FigurePartBeard + "." + "ch" + FigurePartShirt + "." + "cc" + FigurePartJacket + "." + "ca" + FigurePartNecklace + "." + "lg" + FigurePartPants + "." + "wa" + FigurePartBelt + "." + "sh" + FigurePartShoes + ".");

            return _Uni;
        }

        public static string getSkinColor(string figure)
        {
            string Skin = "-180-1";
            string GetSkin = null;
            if (figure.Contains("hd"))
            {
                GetSkin = Regex.Split(figure, "hd")[1];
                Skin = GetSkin.Split('.')[0];
            }
            else
            {
                Skin = "-180-1";
            }
            string[] SkinColor = Skin.Split('-');

            return SkinColor[2];
        }

        public static bool isBlack(string color)
        {
            string[] Blacks = new string[17] { "1370", "19", "20", "1371", "30", "1001", "1002", "1003", "6", "1372", "1373", "21", "1374", "1385", "1386", "1387", "5" };
            if (Blacks.Contains(color))
            {
                return true;
            }
            return false;
        }

        

        public static void GiveCredit(GameClient TargetClient, int credit)
        {
            if (TargetClient != null)
            {
                TargetClient.GetHabbo().ActivityPoints = TargetClient.GetHabbo().ActivityPoints + credit;

                TargetClient.GetHabbo().UpdateActivityPointsBalance();
                TargetClient.GetHabbo().NotifyNewPixels(credit);
            }
        }

        public static int GetDrinkFromOffer(string Offer)
        {
            string[] Texts = Offer.Split('_');

            return Convert.ToInt32(Texts[1]);

        }

        public static void GenerateWantedList()
        {
            DataTable Table = null;

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {

                DataRow User = null;

                dbClient.SetQuery("SELECT id,wanted FROM rp_stats WHERE wanted > 0");
                Table = dbClient.GetTable();


               foreach(DataRow Row in Table.Rows)
               {

                   dbClient.SetQuery("SELECT username, home_room FROM users WHERE id = '" + Convert.ToInt32(Row["id"]) + "'");
                   User = dbClient.GetRow();

                   string Username = User["username"].ToString();
                   int Time = Convert.ToInt32(Row["wanted"]);
                   int Homeroom = Convert.ToInt32(User["home_room"]);

                   RoleplayManager.WantedListData.TryAdd(Username, Time + "|" + Homeroom);
               }


            }


        }


        public static bool BotWithinAttackDistance(RoomUser roomUser, RoomUser TargetroomUser)
        {
            if (roomUser.X + 1 == TargetroomUser.X && roomUser.Y == TargetroomUser.Y
                || roomUser.X == TargetroomUser.X - 1 && roomUser.Y == TargetroomUser.Y + 1
                || roomUser.X == TargetroomUser.X + 1 && roomUser.Y == TargetroomUser.Y - 1
                || roomUser.X - 1 == TargetroomUser.X && roomUser.Y == TargetroomUser.Y ||
                roomUser.Y + 1 == TargetroomUser.Y && roomUser.X == TargetroomUser.X ||
                roomUser.Y - 1 == TargetroomUser.Y && roomUser.X == TargetroomUser.X ||
                roomUser.X == TargetroomUser.X && roomUser.Y == TargetroomUser.Y)
            {
                return true;
            }

            return false;
        }


        public static void GiveMoney(GameClient TargetClient, int credits)
        {
            if (TargetClient != null)
            {
                TargetClient.GetHabbo().Credits = TargetClient.GetHabbo().Credits + credits;

                TargetClient.GetHabbo().UpdateCreditsBalance();
            }
        }

        public static void KickBotFromRoom(string Name, uint RoomId)
        {
            lock (Plus.GetGame().GetRoomManager().GetRoom(RoomId).GetRoomUserManager().GetBots())
            {
                foreach (RoomUser User in Plus.GetGame().GetRoomManager().GetRoom(RoomId).GetRoomUserManager().GetBots())
                {
                    if (User == null)
                        continue;
                    if (User.BotData == null)
                        continue;
                    if (User.BotData.Name.ToLower() != Name.ToLower())
                        continue;

                    Plus.GetGame().GetRoomManager().GetRoom(RoomId).GetRoomUserManager().RemoveBot(User.VirtualId, true);
                }
            }
        }

        internal static RoomBot MakeQuickBot(string Name = "Undefined", AIType BotAI = AIType.HospitalBot, uint RoomId = 1, string Figure = "hr-3020-34.hd-3091-2.ch-225-92.lg-3058-100.sh-3089-1338.ca-3084-78-108.wa-2005", int X = 0, int Y = 0, int Rot = 0, string Gender = "M", int RP_Roam = 5, int RP_Attack = 3, int RP_Follow = 5, int RP_TryAction = 5, int RP_Type = 1)
        {
            Random rand = new Random();
            uint id = Convert.ToUInt32(rand.Next(28374, 294857));
            double Double = 0;
            RoomBot Botz = new RoomBot(id, 0, RoomId, BotAI, "freeroam", Name, "", Figure, X, Y, Double, Rot, 0, 0, 0, 0, null, null, Gender, 0, false);
            return Botz;
        }

        public static RoomItem GetNearItem(RoomUser User , string Item, int MaxDistance)
        {
            RoomItem Inter = null;
            lock (Plus.GetGame().GetRoomManager().GetRoom(User.RoomId).GetRoomItemHandler().FloorItems.Values)
            {
                foreach (RoomItem item in Plus.GetGame().GetRoomManager().GetRoom(User.RoomId).GetRoomItemHandler().FloorItems.Values)
                {

                    if (item == null)
                        continue;

                    HabboHotel.PathFinding.Vector2D Pos1 = new HabboHotel.PathFinding.Vector2D(item.X, item.Y);
                    HabboHotel.PathFinding.Vector2D Pos2 = new HabboHotel.PathFinding.Vector2D(User.X, User.Y);

                    if (item.GetBaseItem() == null)
                        continue;

                    if (RoleplayManager.Distance(Pos1, Pos2) <= MaxDistance)
                    {
                        if (!item.GetBaseItem().Name.Contains(Item))
                        {
                            continue;
                        }


                        Inter = item;
                    }
                }
            }

            return Inter;
        }

        public static RoomItem GetNearItem(string Item, Room MRoom)
        {
            RoomItem Inter = null;
            lock (MRoom.GetRoomItemHandler().FloorItems.Values)
            {
                foreach (RoomItem item in MRoom.GetRoomItemHandler().FloorItems.Values)
                {

                    if (item == null)
                        continue;

                    if (item.GetBaseItem() == null)
                        continue;

                        if (!item.GetBaseItem().Name.Contains(Item))
                        {
                            continue;
                        }


                        Inter = item;
                }
            }

            return Inter;
        }

        internal static void SendBotToRoom(RoomBot Bot, uint room)
        {
            Plus.GetGame().GetRoomManager().GetRoom(room).GetRoomUserManager().DeployBot(Bot, null);
        }

        public static Room GetRandomHospital()
        {
            Room Place = null;

            lock (Plus.GetGame().GetRoomManager().ReturnRooms())
            {
                foreach (Room Room in Plus.GetGame().GetRoomManager().ReturnRooms())
                {
                    if (Room == null)
                        continue;
                    if (Room.RoomData == null)
                        continue;

                    if (Room.RoomData.Description.Contains("HOSP"))
                    {
                        Place = Room;
                    }
                }
            }

            return Place;
        }
        public static Room GetRandomJail()
        {
            Room Place = null;

            lock (Plus.GetGame().GetRoomManager().ReturnRooms())
            {
                foreach (Room Room in Plus.GetGame().GetRoomManager().ReturnRooms())
                {
                    if (Room == null)
                        continue;
                    if (Room.RoomData == null)
                        continue;

                    if (!Room.RoomData.Description.Contains("JAIL") || Room.RoomId == 9)
                        continue;

                    Place = Room;
                }
            }

            return Place;
        }

        public static void AlertGang(string Msg, int Gang, bool DeleteGang = false)
        {
            lock (Plus.GetGame().GetClientManager().Clients.Values)
            {
                foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                {
                    if (client == null)
                        continue;
                    if (client.GetRoleplay() == null)
                        continue;
                    if (client.GetRoleplay().GangId != Gang)
                        continue;

                    client.SendWhisper(Msg);

                    if (DeleteGang)
                    {
                        client.GetRoleplay().GangId = 0;
                        client.GetRoleplay().GangRank = 0;
                        client.GetRoleplay().SaveGangComponents();
                    }
                }
            }
        }
        public static void MakeLay(RoomUser user)
        {
            if (user == null)
                return;

            if (user.Statusses == null)
                return;

            if (!user.Statusses.ContainsKey("lay"))
            {
                if ((user.RotBody % 2) == 0)
                {
                    if (user == null)
                        return;

                    try
                    {
                        user.Statusses.Add("lay", "1.0 null");
                        user.Z -= 0.35;
                        user.IsLyingDown = true;
                        user.UpdateNeeded = true;
                    }
                    catch { }
                }
                else
                {
                    user.RotBody--;
                    user.Statusses.Add("lay", "1.0 null");
                    user.Z -= 0.35;
                    user.IsLyingDown = true;
                    user.UpdateNeeded = true;
                }

            }
            else
            {
                user.Z += 0.35;
                user.Statusses.Remove("lay");
                user.Statusses.Remove("1.0");
                user.IsLyingDown = false;
                user.UpdateNeeded = true;
            }
        
        }
        public static void ActiveNight(int Intensity = 76)
        {
            //disabled
            return;

            foreach (Room Room in Plus.GetGame().GetRoomManager().ReturnRooms())
            {

                #region Manage Landscape
                DateTime date1 = DateTime.Now;
                string time = date1.ToString("h tt");
                string data = "1.1";

                if (time == "7 PM") { data = "5.1"; }
                else if (time == "8 PM") { data = "5.1"; }
                else if (time == "9 PM") { data = "6.1"; }
                else if (time == "10 PM") { data = "6.1"; }
                else if (time == "11 PM") { data = "6.1"; }
                else if (time == "12 AM") { data = "6.1"; }
                else if (time == "1 AM") { data = "6.1"; }
                else if (time == "2 AM") { data = "6.1"; }
                else if (time == "3 AM") { data = "6.1"; }
                else if (time == "4 AM") { data = "6.1"; }
                else if (time == "5 AM") { data = "1.1"; }
                else if (time == "6 AM") { data = "1.1"; }
                else if (time == "7 AM") { data = "5.1"; }
                else if (time == "8 AM") { data = "5.1"; }
                else if (time == "9 AM") { data = "5.1"; }
                else if (time == "10 AM") { data = "4.1"; }
                else if (time == "11 AM") { data = "4.1"; }
                else if (time == "12 PM") { data = "4.1"; }
                else if (time == "1 PM") { data = "2.1"; }
                else if (time == "2 PM") { data = "2.1"; }
                else if (time == "3 PM") { data = "2.1"; }
                else if (time == "4 PM") { data = "2.1"; }
                else if (time == "5 PM") { data = "2.1"; }
                else if (time == "6 PM") { data = "2.1"; }
                Room.RoomData.LandScape = data;

                ServerMessage Message = new ServerMessage(LibraryParser.OutgoingRequest("RoomSpacesMessageComposer"));
                Message.AppendString("landscape");
                Message.AppendString(data);
                Room.SendMessage(Message);
                #endregion
                if (Room.RoomData.Description.Contains("STREET"))
                {
                    foreach (RoomItem item in Room.GetRoomItemHandler().WallItems.Values)
                    {
                        if (item.GetBaseItem().InteractionType == Interaction.Dimmer)
                        {
                            //Console.WriteLine(Intensity);
                            Room.MoodlightData = new MoodlightData(item.Id);
                            Room.MoodlightData.UpdatePreset(1, "#000000", Intensity, false, true);
                            Room.MoodlightData.Enabled = true;
                            item.ExtraData = Room.MoodlightData.GenerateExtraData();
                            item.UpdateState();
                            Room.MoodlightData.Enable();
                            item.ExtraData = Room.MoodlightData.GenerateExtraData();
                            item.UpdateState();
                            //Room.MoodlightData.UpdatePreset();
                        }
                    }
                }
            }
        }

        internal static void AddItem(GameClient Session, Item Item)
        {
            Plus.GetGame().GetCatalog().DeliverItems(Session, Item, 1, "0", 0, 0, "");
            Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
        }

        public static void PickRock(RoomItem Item, uint roomid)
        {
            Room Room = GenerateRoom(roomid);

            Room.GetRoomItemHandler().RemoveRock(Item.Id, false);

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("DELETE FROM items_rooms WHERE id = '" + Item.Id + "'");
            }
        }

        public static void PickTree(RoomItem Item, uint roomid)
        {
            Room Room = GenerateRoom(roomid);

            Room.GetRoomItemHandler().RemoveTree(Item.Id, false);

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("DELETE FROM items_rooms WHERE id = '" + Item.Id + "'");
            }
        }
        public static void PickFarmingSpot(RoomItem Item, uint roomid)
        {
            Room Room = GenerateRoom(roomid);

            Room.GetRoomItemHandler().RemoveFarmingSpot(Item.Id, false);

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("DELETE FROM items_rooms WHERE id = '" + Item.Id + "'");
            }
        }

        public static void PickSlotTilePrize(RoomItem Item, uint roomid)
        {
            Room Room = GenerateRoom(roomid);

            Room.GetRoomItemHandler().RemoveSlotSpot(Item.Id, false);

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("DELETE FROM items_rooms WHERE id = '" + Item.Id + "'");
            }
        }


        public static RoomItem PlaceItemToRoomReturn(uint BaseId, int X, int Y, double Z, int Rot, bool FromInventory, uint roomid, bool ToDB = true)
        {

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                Room Room = GenerateRoom(roomid);
                int ItDemId = 0;
                uint ItemId = 0;

                if (ToDB)
                {
                    dbClient.SetQuery("INSERT INTO items_rooms (user_id,base_item) VALUES (1, " + BaseId + ")");
                    dbClient.RunQuery();
                    dbClient.SetQuery("SELECT id FROM items_rooms WHERE user_id = '1' AND room_id = 0 AND base_item = '" + BaseId + "' ORDER BY id DESC LIMIT 1");
                    ItDemId = dbClient.GetInteger();
                   ItemId = Convert.ToUInt32(ItDemId);
                }
                else
                {
                    dbClient.SetQuery("SELECT id FROM items_rooms WHERE user_id = '1' AND room_id = 0 ORDER BY id DESC LIMIT 1");
                    ItDemId = dbClient.GetInteger();
                    ItemId = Convert.ToUInt32(ItDemId) + 1;
                }
                

                RoomItem RoomItem = new RoomItem(ItemId, Room.RoomId, BaseId, "", X, Y, Z, Rot, Room, Convert.ToUInt32(Room.RoomData.OwnerId), 0, 3537, "", false);


                Room.GetRoomItemHandler().SetRockItem(RoomItem, X, Y, Rot, true, false, true, false, false);

                return RoomItem;
            }

        }

        public static void PickItem(GameClient Session, RoomItem Item, bool toinv = false, Room Ruum = null)
        {
            if (Session != null)
            {
                Room Room = GenerateRoom(Session.GetHabbo().CurrentRoomId);
            
                Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id,false);

                if (Item.GetBaseItem().InteractionType != Interaction.Teleport)
                {
                    if (toinv)
                    {
                       // Session.GetHabbo().GetInventoryComponent().AddNewItem(Item.Id, Item.BaseItem, Item.ExtraData, Item.GroupId, true, true, 0, 0, 0);

                         //Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
                    }
                }

                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery("DELETE FROM items_rooms WHERE id = '" + Item.Id + "'");
                    Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
                }
            }
            else if(Session == null)
            {
                Ruum.GetRoomItemHandler().RemoveFurniture(Session, Item.Id, false);
                using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery("DELETE FROM items_rooms WHERE id = '" + Item.Id + "'");
                }
            }
        }

        public static void ReplaceItem(GameClient Session, RoomItem Item, string newitemname)
        {          
            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                
                dbClient.SetQuery("SELECT id FROM catalog_furnis WHERE item_name = '" + newitemname + "'");
                int newitemid = dbClient.GetInteger();
                uint itemid = Convert.ToUInt32(newitemid);
                PlaceItemToCord(Session, itemid, Item.X, Item.Y, Item.Z, Item.Rot, false);
                PickItem(Session, Item, false);
                
            }
        }



        public static void PlaceItemToCord(GameClient Session, uint BaseId, int X, int Y, double Z, int Rot = 0, bool FromInventory = false)
        {

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                Session.GetRoleplay().DebugStacking = true;
                Room Room = GenerateRoom(Session.GetHabbo().CurrentRoomId);

                dbClient.SetQuery("INSERT INTO items_rooms (user_id,base_item) VALUES ( " + Session.GetHabbo().Id +   " , " + BaseId + ")");
                dbClient.RunQuery();
                Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
                dbClient.SetQuery("SELECT id FROM items_rooms WHERE user_id = '" + Session.GetHabbo().Id + "' AND room_id = 0 AND base_item = '" + BaseId + "' ORDER BY id DESC LIMIT 1");
                int ItDemId = dbClient.GetInteger();
                uint ItemId = Convert.ToUInt32(ItDemId);

                Session.GetRoleplay().DebugStack = Z;
                UserItem Item = Session.GetHabbo().GetInventoryComponent().GetItem(ItemId);

                if (Item == null)
                {
                    Session.SendWhisperBubble("Ocorreu um erro e esta operação não pôde continuar!");
                        return;
                }

                if (Room == null)
                {
                    Session.SendWhisperBubble("Ocorreu um erro e esta operação não pôde continuar!");
                        return;
                }

                if(Item.BaseItem == null)
                {
                    Session.SendWhisperBubble("Ocorreu um erro e esta operação não pôde continuar!");
                        return;
                }

                if(Room.RoomData == null)
                {
                    Session.SendWhisperBubble("Ocorreu um erro e esta operação não pôde continuar!");
                        return;
                }

                RoomItem RoomItem = new RoomItem(Item.Id, Room.RoomId, Item.BaseItemId, Item.ExtraData, X, Y, Z, Rot, Room, Convert.ToUInt32(Room.RoomData.OwnerId), Item.GroupId, Item.BaseItem.FlatId, "", false);
                
                if (Room.GetRoomItemHandler().SetFloorItem(Session, RoomItem, X, Y, Rot, true, false, true))
                {
                    Session.GetHabbo().GetInventoryComponent().RemoveItem(ItemId, true);

                } Session.GetRoleplay().DebugStacking = false;

                Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            }

        }

        public static object ReturnOfflineInfo(uint UserId, string Info)
        {
            object info = null;

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT " + Info + " FROM users WHERE id = '" + UserId + "'");
                info = dbClient.GetString();
            }

            return info;
        }

       
        public static void ActiveDay(int intensity = 0)
        {
            foreach (Room Room in Plus.GetGame().GetRoomManager().ReturnRooms())
            {
               
                    #region Manage Landscape
                    DateTime date1 = DateTime.Now;
                    string time = date1.ToString("h tt");
                    string data = "1.1";

                    if (time == "7 PM") { data = "5.1"; }
                    else if (time == "8 PM") { data = "5.1"; }
                    else if (time == "9 PM") { data = "6.1"; }
                    else if (time == "10 PM") { data = "6.1"; }
                    else if (time == "11 PM") { data = "6.1"; }
                    else if (time == "12 AM") { data = "6.1"; }
                    else if (time == "1 AM") { data = "6.1"; }
                    else if (time == "2 AM") { data = "6.1"; }
                    else if (time == "3 AM") { data = "6.1"; }
                    else if (time == "4 AM") { data = "6.1"; }
                    else if (time == "5 AM") { data = "1.1"; }
                    else if (time == "6 AM") { data = "1.1"; }
                    else if (time == "7 AM") { data = "5.1"; }
                    else if (time == "8 AM") { data = "5.1"; }
                    else if (time == "9 AM") { data = "5.1"; }
                    else if (time == "10 AM") { data = "4.1"; }
                    else if (time == "11 AM") { data = "4.1"; }
                    else if (time == "12 PM") { data = "4.1"; }
                    else if (time == "1 PM") { data = "2.1"; }
                    else if (time == "2 PM") { data = "2.1"; }
                    else if (time == "3 PM") { data = "2.1"; }
                    else if (time == "4 PM") { data = "2.1"; }
                    else if (time == "5 PM") { data = "2.1"; }
                    else if (time == "6 PM") { data = "2.1"; }
                    //Room.RoomData.LandScape = data;
                    Room.RoomData.LandScape = data;

                    ServerMessage Message = new ServerMessage(LibraryParser.OutgoingRequest("RoomSpacesMessageComposer"));
                    Message.AppendString("landscape");
                    Message.AppendString(data);
                    Room.SendMessage(Message);
                    #endregion
                    foreach (RoomItem item in Room.GetRoomItemHandler().WallItems.Values)
                    {
                        if (Room.RoomData.Description.Contains("STREET"))
                        {
                            if (item.GetBaseItem().InteractionType == Interaction.Dimmer)
                            {
                                Room.MoodlightData = new MoodlightData(item.Id);
                                Room.MoodlightData.UpdatePreset(1, "#000000", intensity, false, true);
                                Room.MoodlightData.Enabled = true;
                                item.ExtraData = Room.MoodlightData.GenerateExtraData();
                                item.UpdateState();
                                Room.MoodlightData.Enable();
                                item.ExtraData = Room.MoodlightData.GenerateExtraData();
                                item.UpdateState();
                                //Room.MoodlightData.UpdatePreset();
                            }
                        }
                }
            }
        }

        public static void saveSlotLook(GameClient Session, int slotid, string newfig)
        {
            DataRow check;
            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM user_slots WHERE userid = '" + Session.GetHabbo().Id + "'");
                check = dbClient.GetRow();

                if (check == null)
                {
                    dbClient.RunFastQuery("INSERT INTO user_slots(userid,slot1,slot2,slot3,slot4,slot5) VALUES('" + Session.GetHabbo().Id + "','','','','','')");
                }

                dbClient.RunFastQuery("UPDATE user_slots SET slot" + slotid + " = '" + newfig + "' WHERE userid = '" + Session.GetHabbo().Id + "' ");
            }
        }

        public static void useSlotLook(GameClient Session, int slotid)
        {

            string slot = "slot" + slotid;
            DataRow check;
            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM user_slots WHERE userid = '" + Session.GetHabbo().Id + "'");
                check = dbClient.GetRow();

                if (check == null)
                {
                    dbClient.RunFastQuery("INSERT INTO user_slots(userid,slot1,slot2,slot3,slot4,slot5) VALUES('" + Session.GetHabbo().Id + "','','','','','')");
                }

                if (Convert.ToString(check[slot]) == null || Convert.ToString(check[slot]) == "")
                {
                    Session.SendWhisper("O slot selecionado " + slotid + " está vazio!");
                }
                else
                {
                    string figure = Convert.ToString(check[slot]);
                    savePermLook(Session, figure);
                }
            }
        }

        public static void savePermLook(GameClient Session, string NewFigure, string NewGender = "")
        {
            if (NewGender == "")
            {
                NewGender = Session.GetHabbo().Gender.ToString().Substring(0, 1).ToUpper();
            }

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET look = @look, gender = @gender WHERE id = " + Session.GetHabbo().Id);
                dbClient.AddParameter("look", NewFigure);
                dbClient.AddParameter("gender", NewGender);
                dbClient.RunQuery();
            }

            DataRow User = null;

            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT look,gender FROM users WHERE id = '" + Session.GetHabbo().Id + "'");
                User = dbClient.GetRow();
            }

            Session.GetHabbo().Look = Convert.ToString(User["look"]);
            Session.GetHabbo().Gender = Convert.ToString(User["gender"]);
            Session.GetRoleplay().RefreshVals();
        }

        public static void EjectBot(GameClient Me, GameClient Session, uint BotId)
        {
            var room =
                Plus.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
            if (Session == null || Session.GetHabbo() == null ||
                Session.GetHabbo().GetInventoryComponent() == null)
                return;
            if (room == null)
                return;
            var petId = BotId;
            var pet = room.GetRoomUserManager().GetPet(petId);
            if (pet == null)
                return;
            if (Session.GetRoleplay() != null && Session.GetHabbo().GetRoomUser() != null)
            {
                Session.GetRoleplay().UsingPet = false;
                Session.GetHabbo().GetRoomUser().MyPet = null;
            }

            if (pet.RidingHorse)
            {
                var roomUserByVirtualId =
                    room.GetRoomUserManager().GetRoomUserByVirtualId(Convert.ToInt32(pet.HorseId));
                if (roomUserByVirtualId != null)
                {
                    roomUserByVirtualId.RidingHorse = false;
                    roomUserByVirtualId.ApplyEffect(-1);
                    roomUserByVirtualId.MoveTo((new Point(roomUserByVirtualId.X + 1, roomUserByVirtualId.Y + 1)));
                }
            }
            if (pet.PetData.DbState != DatabaseUpdateState.NeedsInsert)
                pet.PetData.DbState = DatabaseUpdateState.NeedsUpdate;
            pet.PetData.RoomId = 0u;
            Session.GetHabbo().GetInventoryComponent().AddPet(pet.PetData);
            using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
                room.GetRoomUserManager().SavePets(queryReactor);
            room.GetRoomUserManager().RemoveBot(pet.VirtualId, false);
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
            Session.GetHabbo().GetRoomUser().MyPet = null;
            Session.GetRoleplay().UsingPet = false;
            Session.SendWhisper(Me.GetHabbo().UserName + " has ejected your pet " + Session.GetHabbo().GetRoomUser().MyPet);
       
        }

        public static void sendVIPAlert(GameClient Session, string msg, bool StaffMSG = false)
        {
            lock (Plus.GetGame().GetClientManager().Clients.Values)
            {
                foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                {

                    if (client == null)
                        continue;
                    if (client.GetHabbo() == null)
                        continue;
                    if (!client.GetHabbo().VIP)
                        continue;
                    if (client.GetHabbo().vipAlertsOff)
                        continue;
                    //Misc.GenerateRoomUser(client).LastBubble = 1;
                    if (!StaffMSG)
                    {
                        client.SendWhisper("[Alerta VIP][" + Session.GetHabbo().UserName + "]: " + msg);
                    }
                    else
                    {
                        client.SendWhisper("[Alerta VIP]*" + msg + "*");
                    }
                    //Misc.GenerateRoomUser(client).LastBubble = 0;
                }
            }
        }
        public static void sendStaffAlert(string msg, bool ondutyonly = false)
        {
            lock (Plus.GetGame().GetClientManager().Clients.Values)
            {
                foreach (GameClient client in Plus.GetGame().GetClientManager().Clients.Values)
                {
                    if (client == null)
                        continue;
                    if (client.GetHabbo() == null)
                        continue;
                    if (client.GetHabbo().GetRoomUser() == null)
                        continue;
                    if (client.GetRoleplay() == null)
                        continue;

                    if (ondutyonly)
                    {
                        if (client.GetHabbo().HasFuse("fuse_events") && client.GetRoleplay().StaffDuty)
                        {
                            client.GetHabbo().GetRoomUser().LastBubble = 23;
                            client.SendWhisper("[Staff Alert] " + msg);
                            client.GetHabbo().GetRoomUser().LastBubble = 0;
                        }
                    }
                    else
                    {
                        if (client.GetHabbo().HasFuse("fuse_events"))
                        {
                            client.GetHabbo().GetRoomUser().LastBubble = 23;
                            client.SendWhisper("[Staff Alert] " + msg);
                            client.GetHabbo().GetRoomUser().LastBubble = 0;
                        }
                    }
                }
            }
        }
    }
}
