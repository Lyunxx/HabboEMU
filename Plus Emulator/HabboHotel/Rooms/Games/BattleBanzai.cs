using Plus.Collections;
using Plus.Enclosure;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.Wired;
using Plus.Messages;
using Plus.Messages.Parsers;
using Plus.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;

namespace Plus.HabboHotel.Rooms.Games
{
    internal class BattleBanzai
    {
        internal HybridDictionary BanzaiTiles;
        private Room _room;
        private QueuedDictionary<uint, RoomItem> _pucks;
        private byte[,] _floorMap;
        private GameField _field;
        private double _timestarted;

        public BattleBanzai(Room room)
        {
            _room = room;
            BanzaiTiles = new HybridDictionary();
            IsBanzaiActive = false;
            _pucks = new QueuedDictionary<uint, RoomItem>();
            _timestarted = 0.0;
        }

        internal bool IsBanzaiActive { get; private set; }

        internal void AddTile(RoomItem item, uint itemId)
        {
            if (BanzaiTiles.Contains(itemId)) return;
            BanzaiTiles.Add(itemId, item);
        }

        internal void RemoveTile(uint itemId)
        {
            BanzaiTiles.Remove(itemId);
        }

        internal void OnCycle()
        {
            _pucks.OnCycle();
        }

        internal void AddPuck(RoomItem item)
        {
            if (_pucks.ContainsKey(item.Id)) return;

            _pucks.Add(item.Id, item);
        }

        internal void RemovePuck(uint itemId)
        {
            _pucks.Remove(itemId);
        }

        internal void OnUserWalk(RoomUser user)
        {
            if (user == null) return;

            foreach (var roomItem in _pucks.Values)
            {
                var differenceX = user.X - roomItem.X;
                var differenceY = user.Y - roomItem.Y;
                if (differenceX > 1 || differenceX < -1 || differenceY > 1 || differenceY < -1) continue;

                var newX = (differenceX * -1) + roomItem.X;
                var newY = (differenceY * -1) + roomItem.Y;

                if (roomItem.InteractingBallUser == user.UserId && _room.GetGameMap().ValidTile(newX, newY))
                {
                    roomItem.InteractingBallUser = 0;
                    MovePuck(roomItem, user.GetClient(), user.Coordinate, roomItem.Coordinate, 6, user.Team);
                }
                else if (_room.GetGameMap().ValidTile(newX, newY)) MovePuck(roomItem, user.GetClient(), newX, newY, user.Team);
            }
            if (!IsBanzaiActive) return;
            HandleBanzaiTiles(user.Coordinate, user.Team, user);
        }

        internal void BanzaiStart()
        {
            if (IsBanzaiActive) return;
            _room.GetGameManager().StartGame();
            _floorMap = new byte[_room.GetGameMap().Model.MapSizeY, _room.GetGameMap().Model.MapSizeX];
            _field = new GameField(_floorMap, true);
            _timestarted = Plus.GetUnixTimeStamp();
            _room.GetGameManager().LockGates();
            var index = 1;
            while (index < 5)
            {
                _room.GetGameManager().Points[index] = 0;

                {
                    ++index;
                }
            }
            foreach (RoomItem roomItem in BanzaiTiles.Values)
            {
                roomItem.ExtraData = "1";
                roomItem.Value = 0;
                roomItem.Team = Team.none;
                roomItem.UpdateState();
            }

            ResetTiles();
            IsBanzaiActive = true;
            _room.GetWiredHandler().ExecuteWired(Interaction.TriggerGameStart);
            foreach (var roomUser in _room.GetRoomUserManager().GetRoomUsers()) roomUser.LockedTilesCount = 0;
        }

        internal void ResetTiles()
        {
            foreach (var roomItem in _room.GetRoomItemHandler().FloorItems.Values)
            {
                switch (roomItem.GetBaseItem().InteractionType)
                {
                    case Interaction.BanzaiScoreBlue:
                    case Interaction.BanzaiScoreRed:
                    case Interaction.BanzaiScoreYellow:
                    case Interaction.BanzaiScoreGreen:
                        roomItem.ExtraData = "0";
                        roomItem.UpdateState();
                        break;
                }
            }
        }

        internal void BanzaiEnd()
        {
            IsBanzaiActive = false;
            _room.GetGameManager().StopGame();
            _floorMap = null;
            _room.GetWiredHandler().ExecuteWired(Interaction.TriggerGameEnd);
            var winningTeam = _room.GetGameManager().GetWinningTeam();
            _room.GetGameManager().UnlockGates();
            foreach (RoomItem roomItem in BanzaiTiles.Values)
            {
                if (roomItem.Team == winningTeam)
                {
                    roomItem.InteractionCount = 0;
                    roomItem.InteractionCountHelper = 0;
                    roomItem.UpdateNeeded = true;
                }
                else if (roomItem.Team == Team.none)
                {
                    roomItem.ExtraData = "0";
                    roomItem.UpdateState();
                }
            }
            if (winningTeam == Team.none) return;
            foreach (var avatar in _room.GetRoomUserManager().GetRoomUsers())
            {
                if (avatar.Team != Team.none && Plus.GetUnixTimeStamp() - _timestarted > 5.0)
                {
                    Plus.GetGame()
                        .GetAchievementManager()
                        .ProgressUserAchievement(avatar.GetClient(), "ACH_BattleBallTilesLocked",
                            avatar.LockedTilesCount, false);
                    Plus.GetGame()
                        .GetAchievementManager()
                        .ProgressUserAchievement(avatar.GetClient(), "ACH_BattleBallPlayer", 1, false);
                }

                if ((winningTeam == Team.red && avatar.CurrentEffect != 33) ||
                    (winningTeam == Team.green && avatar.CurrentEffect != 34) ||
                    (winningTeam == Team.blue && avatar.CurrentEffect != 35) ||
                    (winningTeam == Team.yellow && avatar.CurrentEffect != 36))
                    continue;
                if (Plus.GetUnixTimeStamp() - _timestarted > 5.0)
                {
                    Plus.GetGame()
                        .GetAchievementManager()
                        .ProgressUserAchievement(avatar.GetClient(), "ACH_BattleBallWinner", 1, false);
                }
                var waveAtWin = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer"));
                waveAtWin.AppendInteger(avatar.VirtualId);
                waveAtWin.AppendInteger(1);
                _room.SendMessage(waveAtWin);
            }
            _field.Destroy();
        }

        internal void MovePuck(RoomItem item, GameClient client, int newX, int newY, Team team)
        {
            if (!_room.GetGameMap().ItemCanBePlacedHere(newX, newY)) return;

            var oldRoomCoord = item.Coordinate;

            double newZ = _room.GetGameMap().Model.SqFloorHeight[newX][newY];
            if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY) return;

            item.ExtraData = ((int)team).ToString();
            item.UpdateNeeded = true;
            item.UpdateState();

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
            serverMessage.AppendInteger(oldRoomCoord.X);
            serverMessage.AppendInteger(oldRoomCoord.Y);
            serverMessage.AppendInteger(newX);
            serverMessage.AppendInteger(newY);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(item.Id);
            serverMessage.AppendString(TextHandling.GetString(item.Z));
            serverMessage.AppendString(TextHandling.GetString(newZ));
            serverMessage.AppendInteger(-1);
            _room.SendMessage(serverMessage);

            _room.GetRoomItemHandler().SetFloorItem(client, item, newX, newY, item.Rot, false, false, false, false, false);
            if (client == null || client.GetHabbo() == null) return;
            var roomUserByHabbo =
                client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);
            if (!IsBanzaiActive) return;

            HandleBanzaiTiles(new Point(newX, newY), team, roomUserByHabbo);
        }

        internal void MovePuck(RoomItem item, GameClient client, Point user, Point ball, int length, Team team)
        {
            var differenceX = user.X - ball.X;
            var differenceY = user.Y - ball.Y;

            if (differenceX > 1 || differenceX < -1 || differenceY > 1 || differenceY < -1) return;

            var affectedTiles = new List<Point>();
            var newX = ball.X;
            var newY = ball.Y;

            for (var i = 1; i < length; i++)
            {
                newX = (differenceX * -i) + item.X;
                newY = (differenceY * -i) + item.Y;
                if (!_room.GetGameMap().ItemCanBePlacedHere(newX, newY))
                {
                    if (i == 1) break;
                    if (i != length) affectedTiles.Add(new Point(newX, newY));

                    i = i - 1;
                    newX = differenceX * -i;
                    newY = differenceY * -i;

                    newX = newX + item.X;
                    newY = newY + item.Y;
                    break;
                }
                if (i != length) affectedTiles.Add(new Point(newX, newY));
            }
            if (client == null || client.GetHabbo() == null) return;
            var roomUserByHabbo =
                client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);

            foreach (var coord in affectedTiles) HandleBanzaiTiles(coord, team, roomUserByHabbo);
            if (newX != ball.X || newY != ball.Y) MovePuck(item, client, newX, newY, team);
        }

        internal void Destroy()
        {
            BanzaiTiles.Clear();
            _pucks.Clear();
            Array.Clear(_floorMap, 0, _floorMap.Length);
            _field.Destroy();
            _room = null;
            BanzaiTiles = null;
            _pucks = null;
            _floorMap = null;
            _field = null;
        }

        private static void SetMaxForTile(RoomItem item, Team team, RoomUser user)
        {
            if (item.Value < 3)
            {
                item.Value = 3;
                item.Team = team;
            }
            var num = item.Value + ((int)item.Team) * 3 - 1;

            item.ExtraData = num.ToString();
        }

        private void SetTile(RoomItem item, Team team, RoomUser user)
        {
            if (item.Team != team)
            {
                if (item.Value < 3)
                {
                    item.Team = team;
                    item.Value = 1;
                }
            }
            else
            {
                if (item.Value < 3)
                {
                    ++item.Value;
                    if (item.Value == 3)
                    {
                        ++user.LockedTilesCount;
                        _room.GetGameManager().AddPointToTeam(item.Team, user);
                        _field.UpdateLocation(item.X, item.Y, (byte)(uint)team);

                        foreach (var pointField in _field.DoUpdate(false))
                        {
                            if(pointField == null) continue;
                            var team1 = (Team)pointField.ForValue;
                            foreach (var point in pointField.GetPoints())
                            {
                                HandleMaxBanzaiTiles(new Point(point.X, point.Y), team1, user);
                                _floorMap[point.Y, point.X] = pointField.ForValue;
                            }
                        }
                    }
                }
            }
            var num = item.Value + ((int)item.Team) * 3 - 1;

            item.ExtraData = num.ToString();
        }

        private void HandleBanzaiTiles(Point coord, Team team, RoomUser user)
        {
            if (team == Team.none) return;
            _room.GetGameMap().GetCoordinatedItems(coord);
            var num = 0;
            foreach (RoomItem roomItem in BanzaiTiles.Values)
            {
                if (roomItem.GetBaseItem().InteractionType != Interaction.BanzaiFloor)
                {
                    user.Team = Team.none;
                    user.ApplyEffect(0);
                }
                else if (roomItem.ExtraData.Equals("5") || roomItem.ExtraData.Equals("8") ||
                         roomItem.ExtraData.Equals("11") || roomItem.ExtraData.Equals("14"))
                {
                    ++num;
                }
                else if (roomItem.X == coord.X && roomItem.Y == coord.Y)
                {
                    SetTile(roomItem, team, user);
                    if (roomItem.ExtraData.Equals("5") || roomItem.ExtraData.Equals("8") ||
                        roomItem.ExtraData.Equals("11") || roomItem.ExtraData.Equals("14"))
                    {
                        ++num;
                    }
                    roomItem.UpdateState(false, true);
                }
            }
            if (num != BanzaiTiles.Count) return;
            BanzaiEnd();
        }

        private void HandleMaxBanzaiTiles(Point coord, Team team, RoomUser user)
        {
            if (team == Team.none) return;
            _room.GetGameMap().GetCoordinatedItems(coord);
            foreach (
                var roomItem in
                    BanzaiTiles.Values.Cast<RoomItem>()
                        .Where(
                            roomItem =>
                                roomItem.GetBaseItem().InteractionType == Interaction.BanzaiFloor &&
                                (roomItem.X == coord.X && roomItem.Y == coord.Y)))
            {
                SetMaxForTile(roomItem, team, user);
                _room.GetGameManager().AddPointToTeam(team, user);
                roomItem.UpdateState(false, true);
            }
        }
    }
}