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
using Plus.HabboHotel.Roleplay.Gangs;
using Plus.HabboHotel.Roleplay.Misc;
using Plus.HabboHotel.Roleplay.Jobs;
using Plus.HabboHotel.Roleplay.Timers;
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
using Plus.Messages.Parsers;
using Plus.HabboHotel.Achievements.Composer;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.Roleplay.Combat.WeaponExtras;
using Plus.HabboHotel.Roleplay.Minigames.Colour_Wars;


namespace Plus.HabboHotel.Roleplay.Bots
{
    public class FightPetManager
    {

        public static Dictionary<string, RPPet> PetData;
        public static void load()
        {
            PetData = new Dictionary<string, RPPet>();
            PetData.Clear();
            DataTable pets = null;
            using (IQueryAdapter dbClient = Plus.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rp_petshop");
                pets = dbClient.GetTable();
                foreach (DataRow pet in pets.Rows)
                {
                    RPPet NewPet = new RPPet(
                        Convert.ToString(pet[0]),
                        Convert.ToString(pet[1]),
                        Convert.ToString(pet[2]),
                        Convert.ToInt32(pet[3]),
                        Convert.ToInt32(pet[4]),
                        Convert.ToInt32(pet[5])
                        );
                    PetData.Add(Convert.ToString(pet[0]), NewPet);
                }
            }
        }
        public static void WalkToOwner(RoomUser Pet)
        {
            GameClient FollowingOwner = null;
            FollowingOwner = Plus.GetGame().GetClientManager().GetClientByUserId(Pet.BotData.OwnerId);

            if (FollowingOwner == null)
            {
                Console.WriteLine("Owner was null");
                return;
            }

            if (FollowingOwner.GetHabbo().GetRoomUser() == null)
            {
                Console.WriteLine("Owner roomuser was null");
                return;
            }

            if (FollowingOwner.GetHabbo().GetRoomUser().RoomId != Pet.RoomId)
            {
                Console.WriteLine("Owner is not in the room");
                return;
            }

            Pet.Chat(null, "Just a second master, I'm coming!", true, 0, 0);
            Pet.Move(FollowingOwner.GetHabbo().GetRoomUser().GoalX, FollowingOwner.GetHabbo().GetRoomUser().GoalY, FollowingOwner.GetHabbo().GetRoomUser().FollowingOwner.RotBody);
        }
        public static void WalkToPlayer(RoomUser Pet, RoomUser User)
        {
            int NewX = User.X;
            int NewY = User.Y;

            #region Rotation
            if (User.RotBody == 4)
            {
                NewY = User.Y + 1;
            }
            else if (User.RotBody == 0)
            {
                NewY = User.Y - 1;
            }
            else if (User.RotBody == 6)
            {
                NewX = User.X - 1;
            }
            else if (User.RotBody == 2)
            {
                NewX = User.X + 1;
            }
            else if (User.RotBody == 3)
            {
                NewX = User.X + 1;
                NewY = User.Y + 1;
            }
            else if (User.RotBody == 1)
            {
                NewX = User.X + 1;
                NewY = User.Y - 1;
            }
            else if (User.RotBody == 7)
            {
                NewX = User.X - 1;
                NewY = User.Y - 1;
            }
            else if (User.RotBody == 5)
            {
                NewX = User.X - 1;
                NewY = User.Y + 1;
            }
            #endregion

            Pet.MoveTo(NewX, NewY);
        }
        internal void RemovePetFromOtherRooms(string PetName)
        {

        }
        internal RoomBot DeployBotToRoom(GameClient Session, string PetName, uint RoomId)
        {

            if (!Session.GetRoleplay().MyPets.ContainsKey(PetName.ToLower()))
            {
                return null;
            }

            Session.GetHabbo().GetInventoryComponent().RefreshPets();

            uint petId = Session.GetRoleplay().MyPets[PetName.ToLower()];
            var pet = Session.GetHabbo().GetInventoryComponent().GetPet(petId);
            if (pet == null)
            {
                Session.GetRoleplay().LastPetName = null;
                // Console.WriteLine("Pet was null");
                return null;
            }

            #region Conditions
            if (Session.GetHabbo().CurrentRoom.RoomData.Description.Contains("NOPET"))
            {
                Session.GetRoleplay().LastPetName = null;
                Session.GetHabbo().GetRoomUser().LastBubble = 34;
                Session.SendWhisper("Sorry but this room doesnt allow pets!");
                Session.GetHabbo().GetRoomUser().LastBubble = 0;
                return null;
            }
            #endregion

            Room room = Plus.GetGame().GetRoomManager().GetRoom(RoomId);
            int x = Session.GetHabbo().GetRoomUser().X;
            int y = Session.GetHabbo().GetRoomUser().Y;


            if (!room.GetGameMap().CanWalk(x, y, false, 0u))
            {
                //  Console.WriteLine("spawnpoint was shit");
                return null;
            }

            AIType Type = AIType.Pet;
            string PetType = "pet";

            using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery("UPDATE bots SET room_id = '" + room.RoomId + "', x = '" + x + "', y = '" + y + "' WHERE id = '" + petId + "'");
                queryReactor.SetQuery("SELECT ai_type FROM bots WHERE id = " + petId + "");
                DataRow Row = queryReactor.GetRow();
                if (Row != null)
                {
                    PetType = Convert.ToString(Row["ai_type"]);
                }
            }

            switch (PetType)
            {
                case "fight_pet":
                case "fightpet":
                    Type = AIType.FightPet;
                    break;

                case "pet":
                    Type = AIType.Pet;
                    break;
            }

            if (Type == AIType.FightPet)
            {
                x = Session.GetHabbo().GetRoomUser().SquareBehind.X;
                y = Session.GetHabbo().GetRoomUser().SquareBehind.Y;

                if (Session.GetHabbo().GetRoomUser().MyPet != null)
                {
                    Session.SendWhisper("Please pickup your current pet!");
                    return null;
                }

                // RoleplayManager.Shout(Session, "*Places their pet down*");

                Session.GetRoleplay().UsingPet = true;
                Session.GetRoleplay().mPetID = petId;
            }

            pet.PlacedInRoom = true;
            pet.RoomId = room.RoomId;


            RoomBot newRoomBot = new RoomBot(pet.PetId, Convert.ToUInt32(pet.OwnerId), pet.RoomId, Type, "freeroam", pet.Name,
                        "", pet.Look, x, y, 0.0, 4, 0, 0, 0, 0, null, null, "", 0, false);

            room.GetRoomUserManager()
                .DeployBot(newRoomBot, pet);
            Session.GetHabbo().GetInventoryComponent().MovePetToRoom(pet.PetId);
            if (pet.DbState != DatabaseUpdateState.NeedsInsert)
                pet.DbState = DatabaseUpdateState.NeedsUpdate;
            using (var queryreactor2 = Plus.GetDatabaseManager().GetQueryReactor())
                room.GetRoomUserManager().SavePets(queryreactor2);
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());

            Session.GetHabbo().GetRoomUser().MyPet = newRoomBot.RoomUser;
            Session.GetHabbo().GetRoomUser().MyPet.FollowingOwner = Session.GetHabbo().GetRoomUser();
            Session.GetRoleplay().LastPetName = pet.Name;
            //Session.SendWhisper("Your bot should be here...");



            return newRoomBot;
        }
        public static void LoadRoomAndDeploy(uint RoomId)
        {

        }
    }

    public class RPPet
    {
        public string Type;
        public string Data1;
        public string Data2;
        public int Price_Coins;
        public int Price_VIP;
        public int Special_Pet;

        public RPPet(string Type, string Data1, string Data2, int Price_Coins, int Price_VIP, int Special_Pet)
        {
            this.Type = Type;
            this.Data1 = Data1;
            this.Data2 = Data2;
            this.Price_Coins = Price_Coins;
            this.Price_VIP = Price_VIP;
            this.Special_Pet = Special_Pet;
        }
    }
}
