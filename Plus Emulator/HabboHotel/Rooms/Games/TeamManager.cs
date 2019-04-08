using System.Collections.Generic;
using System.Drawing;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Games
{
    public class TeamManager
    {
        public string Game;
        public List<RoomUser> BlueTeam;
        public List<RoomUser> RedTeam;
        public List<RoomUser> YellowTeam;
        public List<RoomUser> GreenTeam;

        public static TeamManager CreateTeamforGame(string game)
        {
            return new TeamManager
            {
                Game = game,
                BlueTeam = new List<RoomUser>(),
                RedTeam = new List<RoomUser>(),
                GreenTeam = new List<RoomUser>(),
                YellowTeam = new List<RoomUser>()
            };
        }

        public bool CanEnterOnTeam(Team t)
        {
            if (t.Equals(Team.blue)) return BlueTeam.Count < 5;
            if (t.Equals(Team.red)) return RedTeam.Count < 5;
            if (t.Equals(Team.yellow)) return YellowTeam.Count < 5;
            return t.Equals(Team.green) && GreenTeam.Count < 5;
        }

        public void AddUser(RoomUser user)
        {
            if (user == null || user.GetClient() == null) return;
            if (user.Team.Equals(Team.blue)) BlueTeam.Add(user);
            else
            {
                if (user.Team.Equals(Team.red)) RedTeam.Add(user);
                else
                {
                    if (user.Team.Equals(Team.yellow)) YellowTeam.Add(user);
                    else if (user.Team.Equals(Team.green)) GreenTeam.Add(user);
                }
            }

            if (string.IsNullOrEmpty(Game)) return;
            switch (Game.ToLower())
            {
               
                case "freeze":
                    var currentRoom2 = user.GetClient().GetHabbo().CurrentRoom;
                    foreach (var current6 in currentRoom2.GetRoomItemHandler().FloorItems.Values)
                    {
                        switch (current6.GetBaseItem().InteractionType)
                        {
                            case Interaction.FreezeBlueGate:
                                current6.ExtraData = BlueTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeRedGate:
                                current6.ExtraData = RedTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeGreenGate:
                                current6.ExtraData = GreenTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeYellowGate:
                                current6.ExtraData = YellowTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                        }
                    }
                    break;
            }
        }

        public void OnUserLeave(RoomUser user)
        {
            if (user == null) return;
            if (user.Team.Equals(Team.blue)) BlueTeam.Remove(user);
            else
            {
                if (user.Team.Equals(Team.red)) RedTeam.Remove(user);
                else
                {
                    if (user.Team.Equals(Team.yellow)) YellowTeam.Remove(user);
                    else if (user.Team.Equals(Team.green)) GreenTeam.Remove(user);
                }
            }
            if (string.IsNullOrEmpty(Game)) return;

            var currentRoom = user.GetClient().GetHabbo().CurrentRoom;
            if (currentRoom == null) return;

            switch (Game.ToLower())
            {
               
                case "freeze":
                    foreach (var current6 in currentRoom.GetRoomItemHandler().FloorItems.Values)
                    {
                        switch (current6.GetBaseItem().InteractionType)
                        {
                            case Interaction.FreezeBlueGate:
                                current6.ExtraData = BlueTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeRedGate:
                                current6.ExtraData = RedTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeGreenGate:
                                current6.ExtraData = GreenTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeYellowGate:
                                current6.ExtraData = YellowTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                        }
                    }
                    break;
            }
        }
    }
}