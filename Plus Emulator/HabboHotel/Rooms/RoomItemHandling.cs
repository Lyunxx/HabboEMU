using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Plus.Collections;
using Plus.Configuration;
using Plus.Database.Manager.Database.Session_Details.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.PathFinding;
using Plus.HabboHotel.Pets;
using Plus.Messages;
using Plus.Messages.Parsers;
using Plus.Util;

namespace Plus.HabboHotel.Rooms
{
    /// <summary>
    /// Class RoomItemHandling.
    /// </summary>
    internal class RoomItemHandling
    {
        /// <summary>
        /// The hopper count
        /// </summary>
        public int HopperCount;

        /// <summary>
        /// The floor items
        /// </summary>
        internal ConcurrentDictionary<uint, RoomItem> FloorItems, WallItems, Rollers;

        /// <summary>
        /// The breeding terrier
        /// </summary>
        internal Dictionary<uint, RoomItem> BreedingTerrier, BreedingBear;

        /// <summary>
        /// The _roller items moved
        /// </summary>
        private readonly List<uint> _rollerItemsMoved, _rollerUsersMoved;

        /// <summary>
        /// The _roller messages
        /// </summary>
        private readonly List<ServerMessage> _rollerMessages;

        /// <summary>
        /// The _room
        /// </summary>
        private Room _room;

        private List<uint> _updatedItems, _removedItems; 

        /// <summary>
        /// The _roller speed
        /// </summary>
        private uint _rollerSpeed, _roolerCycle;

        /// <summary>
        /// The _room item update queue
        /// </summary>
        private Queue _roomItemUpdateQueue;

        public int TotalItems { get { return WallItems.Keys.Count + FloorItems.Keys.Count; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomItemHandling"/> class.
        /// </summary>
        /// <param name="room">The room.</param>
        public RoomItemHandling(Room room)
        {
            _room = room;
            _removedItems = new List<uint>();
            _updatedItems = new List<uint>();
            Rollers = new ConcurrentDictionary<uint, RoomItem>();
            WallItems = new ConcurrentDictionary<uint, RoomItem>();
            FloorItems = new ConcurrentDictionary<uint, RoomItem>();
            _roomItemUpdateQueue = new Queue();
            BreedingBear = new Dictionary<uint, RoomItem>();
            BreedingTerrier = new Dictionary<uint, RoomItem>();
            GotRollers = false;
            _roolerCycle = 0;
            _rollerSpeed = 4;
            HopperCount = 0;
            _rollerItemsMoved = new List<uint>();
            _rollerUsersMoved = new List<uint>();
            _rollerMessages = new List<ServerMessage>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [got rollers].
        /// </summary>
        /// <value><c>true</c> if [got rollers]; otherwise, <c>false</c>.</value>
        internal bool GotRollers { get; set; }

        internal RoomItem GetItem(uint itemId)
        {
            if (FloorItems.ContainsKey(itemId)) return FloorItems[itemId];
            return WallItems.ContainsKey(itemId) ? WallItems[itemId] : null;
        }

        /// <summary>
        /// Gets the random breeding bear.
        /// </summary>
        /// <param name="pet">The pet.</param>
        /// <returns>Point.</returns>
        internal Point GetRandomBreedingBear(Pet pet)
        {
            if (!BreedingBear.Any())
                return new Point();
            var keys = new List<uint>(BreedingBear.Keys);
            var size = BreedingBear.Count;
            var rand = new Random();
            var randomKey = keys[rand.Next(size)];

            BreedingBear[randomKey].PetsList.Add(pet);
            pet.WaitingForBreading = BreedingBear[randomKey].Id;
            pet.BreadingTile = BreedingBear[randomKey].Coordinate;

            return BreedingBear[randomKey].Coordinate;
        }

        /// <summary>
        /// Gets the random breeding terrier.
        /// </summary>
        /// <param name="pet">The pet.</param>
        /// <returns>Point.</returns>
        internal Point GetRandomBreedingTerrier(Pet pet)
        {
            if (!BreedingTerrier.Any())
                return new Point();
            var keys = new List<uint>(BreedingTerrier.Keys);
            var size = BreedingTerrier.Count;
            var rand = new Random();
            var randomKey = keys[rand.Next(size)];

            BreedingTerrier[randomKey].PetsList.Add(pet);
            pet.WaitingForBreading = BreedingTerrier[randomKey].Id;
            pet.BreadingTile = BreedingTerrier[randomKey].Coordinate;

            return BreedingTerrier[randomKey].Coordinate;
        }

        /// <summary>
        /// Saves the furniture.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="session">The session.</param>
        public void SaveFurniture(IQueryAdapter dbClient, GameClient session = null)
        {
            try
            {
                if (!_updatedItems.Any() && !_removedItems.Any() &&
                    _room.GetRoomUserManager().PetCount <= 0) return;

                var queryChunk = new QueryChunk();
                var queryChunk2 = new QueryChunk();

                foreach (var itemId in _removedItems)
                {
                    queryChunk.AddQuery("UPDATE items_rooms SET room_id='0', x='0', y='0', z='0', rot='0' WHERE id = " +
                                        itemId);
                }

                foreach (var roomItem in _updatedItems.Select(GetItem).Where(roomItem => roomItem != null))
                {
                    if (roomItem.GetBaseItem() != null && roomItem.GetBaseItem().IsGroupItem)
                    {
                        try
                        {
                            var gD = roomItem.GroupData.Split(';');
                            roomItem.ExtraData = roomItem.ExtraData + ";" + gD[1] + ";" +
                                                 gD[2] + ";" + gD[3];
                        }
                        catch
                        {
                            roomItem.ExtraData = string.Empty;
                        }
                    }

                    if (roomItem.RoomId == 0) continue;

                    if (roomItem.GetBaseItem().Name.Contains("wallpaper_single") ||
                        roomItem.GetBaseItem().Name.Contains("floor_single") ||
                        roomItem.GetBaseItem().Name.Contains("landscape_single"))
                    {
                        queryChunk.AddQuery("DELETE FROM items_rooms WHERE id = " + roomItem.Id + " LIMIT 1");
                        continue;
                    }

                    var query = "UPDATE items_rooms SET room_id = " + roomItem.RoomId;
                    if (!string.IsNullOrEmpty(roomItem.ExtraData))
                    {
                        query += ", extra_data = @extraData" + roomItem.Id;
                        queryChunk2.AddParameter("extraData" + roomItem.Id, roomItem.ExtraData);
                    }

                    if (roomItem.IsFloorItem)
                    {
                        query += string.Format(", x={0}, y={1}, z='{2}', rot={3}", roomItem.X, roomItem.Y,
                            roomItem.Z.ToString(CultureInfo.InvariantCulture).Replace(',', '.'), roomItem.Rot);
                    }
                    else
                    {
                        query += ", wall_pos = @wallPos" + roomItem.Id;
                        queryChunk2.AddParameter("wallPos" + roomItem.Id, roomItem.WallCoord);
                    }

                    query += " WHERE id = " + roomItem.Id;
                    queryChunk2.AddQuery(query);
                }

                _room.GetRoomUserManager().AppendPetsUpdateString(dbClient);

                if (session != null) session.GetHabbo().GetInventoryComponent().RunDbUpdate();

                _updatedItems.Clear();
                _removedItems.Clear();
                queryChunk.Execute(dbClient);
                queryChunk2.Execute(dbClient);
                queryChunk.Dispose();
                queryChunk2.Dispose();
                queryChunk = null;
                queryChunk2 = null;
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException("Error during saving furniture for room " + _room.RoomId + ". Stack: " + ex);
            }
        }

        /// <summary>
        /// Checks the position item.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="rItem">The r item.</param>
        /// <param name="newX">The new x.</param>
        /// <param name="newY">The new y.</param>
        /// <param name="newRot">The new rot.</param>
        /// <param name="newItem">if set to <c>true</c> [new item].</param>
        /// <param name="sendNotify">if set to <c>true</c> [send notify].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CheckPosItem(GameClient session, RoomItem rItem, int newX, int newY, int newRot, bool newItem,
            bool sendNotify = true)
        {
            try
            {
                var dictionary = Gamemap.GetAffectedTiles(rItem.GetBaseItem().Length, rItem.GetBaseItem().Width, newX,
                    newY, newRot);
                if (!_room.GetGameMap().ValidTile(newX, newY))
                    return false;
                if (
                    dictionary.Values.Any(
                        coord =>
                            (_room.GetGameMap().Model.DoorX == coord.X) && (_room.GetGameMap().Model.DoorY == coord.Y)))
                    return false;
                if ((_room.GetGameMap().Model.DoorX == newX) && (_room.GetGameMap().Model.DoorY == newY))
                    return false;
                if (dictionary.Values.Any(coord => !_room.GetGameMap().ValidTile(coord.X, coord.Y)))
                    return false;
                double num = _room.GetGameMap().Model.SqFloorHeight[newX][newY];
                if (rItem.Rot == newRot && rItem.X == newX && rItem.Y == newY && rItem.Z != num)
                    return false;
                if (_room.GetGameMap().Model.SqState[newX][newY] != SquareState.Open)
                    return false;
                if (
                    dictionary.Values.Any(
                        coord => _room.GetGameMap().Model.SqState[coord.X][coord.Y] != SquareState.Open))
                    return false;
                if (!rItem.GetBaseItem().IsSeat)
                {
                    if (_room.GetGameMap().SquareHasUsers(newX, newY))
                        return false;
                    if (dictionary.Values.Any(coord => _room.GetGameMap().SquareHasUsers(coord.X, coord.Y)))
                        return false;
                }
                var furniObjects = GetFurniObjects(newX, newY);
                var collection = new List<RoomItem>();
                var list3 = new List<RoomItem>();
                foreach (
                    var list4 in
                        dictionary.Values
                            .Select(coord => GetFurniObjects(coord.X, coord.Y))
                            .Where(list4 => list4 != null))
                    collection.AddRange(list4);
                if (furniObjects == null)
                    furniObjects = new List<RoomItem>();
                list3.AddRange(furniObjects);
                list3.AddRange(collection);
                return list3.All(item => (item.Id == rItem.Id) || item.GetBaseItem().Stackable);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Queues the room item update.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void QueueRoomItemUpdate(RoomItem item)
        {
            lock (_roomItemUpdateQueue.SyncRoot)
            {
                _roomItemUpdateQueue.Enqueue(item);
            }
        }

        internal void RemoveRock(uint id, bool wasPicked = true)
        {
            var item = GetItem(id);

            if (item == null)
                return;

            RemoveRoomItem(item, wasPicked);
        }
        internal void RemoveTree(uint id, bool wasPicked = true)
        {
            var item = GetItem(id);

            if (item == null)
                return;

            RemoveRoomItem(item, wasPicked);
        }
        internal void RemoveFarmingSpot(uint id, bool wasPicked = true)
        {
            var item = GetItem(id);

            if (item == null)
                return;

            RemoveRoomItem(item, wasPicked);
        }
        internal void RemoveSlotSpot(uint id, bool wasPicked = true)
        {
            var item = GetItem(id);

            if (item == null)
                return;

            RemoveRoomItem(item, wasPicked);
        }
        internal bool SetRockItem(RoomItem item, int newX, int newY, int newRot, bool newItem,
                               bool onRoller, bool sendMessage, bool updateRoomUserStatuses, bool specialMove)
        {
            var flag = false;
            if (!newItem) flag = _room.GetGameMap().RemoveFromMap(item);

            var affectedTiles = Gamemap.GetAffectedTiles(item.GetBaseItem().Length,
                item.GetBaseItem().Width, newX, newY, newRot);

            var oldCoord = item.Coordinate;


            double height = _room.GetGameMap().Model.SqFloorHeight[newX][newY];


            var furniObjects = GetFurniObjects(newX, newY);
            var list = new List<RoomItem>();
            var list2 = new List<RoomItem>();
            foreach (
                var furniObjects2 in
                    affectedTiles.Values
                        .Select(current4 => GetFurniObjects(current4.X, current4.Y))
                        .Where(furniObjects2 => furniObjects2 != null))
                list.AddRange(furniObjects2);
            list2.AddRange(furniObjects);
            list2.AddRange(list);

            var stackMagic = list2.FirstOrDefault(
                roomItem =>
                    roomItem != null && roomItem.GetBaseItem() != null &&
                    roomItem.GetBaseItem().InteractionType == Interaction.TileStackMagic);

            if (stackMagic != null)
            {
                height = stackMagic.Z;
            }
            else if (!onRoller && item.GetBaseItem().InteractionType != Interaction.TileStackMagic)
            {
                if (list2.Any(
                        current5 =>
                            current5 != null && current5.Id != item.Id && current5.GetBaseItem() != null &&
                            !current5.GetBaseItem().Stackable))
                {
                    if (!flag) return false;
                    AddOrUpdateItem(item.Id);
                    _room.GetGameMap().AddToMap(item);
                    return false;
                }

                if (item.Rot != newRot && item.X == newX && item.Y == newY) height = item.Z;
                foreach (var current6 in list2) if (current6.Id != item.Id && current6.TotalHeight > height) height = current6.TotalHeight;
            }

            if (item.GetBaseItem().Name == "boutique_mannequin1" || item.GetBaseItem().Name == "gld_wall_tall")
            {
                if (newRot < 0 || newRot > 12)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "pirate_stage2" || item.GetBaseItem().Name == "pirate_stage2_g" || item.GetBaseItem().Name == "cpunk_c15_stage1" || item.GetBaseItem().Name == "paris_c15_pavement" || item.GetBaseItem().Name == "lm_crystal_skull" || item.GetBaseItem().Name == "horse_track_fence_cor" || item.GetBaseItem().Name == "paris_c15_roof" || item.GetBaseItem().Name == "hween_c15_road" || item.GetBaseItem().Name == "hween_c15_pavement")
            {
                if (newRot < 0 || newRot > 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "gh_div_cor" || item.GetBaseItem().Name == "hblooza14_duckcrn" || item.GetBaseItem().Name == "hween13_dwarfcrn")
            {
                if (newRot != 1 && newRot != 3 && newRot != 5 && newRot != 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "val14_b_roof" || item.GetBaseItem().Name == "val14_g_roof" || item.GetBaseItem().Name == "val14_y_roof")
            {
                if (newRot != 2 && newRot != 3 && newRot != 4 && newRot != 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "val13_div_1")
            {
                if (newRot < 0 || newRot > 6)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "room_info15_shrub1")
            {
                if (newRot != 0 && newRot != 2 && newRot != 3 && newRot != 4 && newRot != 6)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "room_info15_div")
            {
                if (newRot < 0 || newRot > 5)
                {
                    newRot = 0;
                }
            }
            else
            {
                if (newRot != 0 && newRot != 2 && newRot != 4 && newRot != 6 && newRot != 8)
                {
                    newRot = 0;
                }
            }
            item.Rot = newRot;


            item.SetState(newX, newY, height, affectedTiles);

            if (newItem)
            {
                if (FloorItems.ContainsKey(item.Id)) return true;
                if (item.IsFloorItem) FloorItems.TryAdd(item.Id, item);
                else if (item.IsWallItem) WallItems.TryAdd(item.Id, item);

                AddOrUpdateItem(item.Id);
                if (sendMessage)
                {
                    var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AddFloorItemMessageComposer"));
                    item.Serialize(serverMessage);
                    serverMessage.AppendString(_room.RoomData.Group != null
                        ? "Jerry"
                        : _room.RoomData.Owner);
                    _room.SendMessage(serverMessage);
                }
            }
            else
            {
                AddOrUpdateItem(item.Id);
                if (!onRoller && sendMessage)
                {
                    if (specialMove)
                    {
                        var message = new ServerMessage(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
                        message.AppendInteger(oldCoord.X);
                        message.AppendInteger(oldCoord.Y);
                        message.AppendInteger(newX);
                        message.AppendInteger(newY);
                        message.AppendInteger(1);
                        message.AppendInteger(item.Id);
                        message.AppendString(TextHandling.GetString(item.Z));
                        message.AppendString(TextHandling.GetString(item.Z));
                        message.AppendInteger(-1);
                        _room.SendMessage(message);
                    }
                    else
                    {
                        var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer"));
                        item.Serialize(message);
                        _room.SendMessage(message);
                    }
                }
                if (item.IsWired) _room.GetWiredHandler().MoveWired(item);
            }
            _room.GetGameMap().AddToMap(item);
            if (item.GetBaseItem().IsSeat) updateRoomUserStatuses = true;
            if (updateRoomUserStatuses)
            {
                _room.GetRoomUserManager().OnUserUpdateStatus(oldCoord.X, oldCoord.Y);
                _room.GetRoomUserManager().OnUserUpdateStatus(item.X, item.Y);
            }
            if (newItem) OnHeightMapUpdate(affectedTiles);

            return true;
        }

        internal bool SetFarmingItem(RoomItem item, int newX, int newY, int newRot, bool newItem, bool onRoller, bool sendMessage, bool updateRoomUserStatuses, bool specialMove)
        {
            var flag = false;
            if (!newItem) flag = _room.GetGameMap().RemoveFromMap(item);

            var affectedTiles = Gamemap.GetAffectedTiles(item.GetBaseItem().Length,
                item.GetBaseItem().Width, newX, newY, newRot);

            var oldCoord = item.Coordinate;


            double height = _room.GetGameMap().Model.SqFloorHeight[newX][newY];


            var furniObjects = GetFurniObjects(newX, newY);
            var list = new List<RoomItem>();
            var list2 = new List<RoomItem>();
            foreach (
                var furniObjects2 in
                    affectedTiles.Values
                        .Select(current4 => GetFurniObjects(current4.X, current4.Y))
                        .Where(furniObjects2 => furniObjects2 != null))
                list.AddRange(furniObjects2);
            list2.AddRange(furniObjects);
            list2.AddRange(list);

            var stackMagic = list2.FirstOrDefault(
                roomItem =>
                    roomItem != null && roomItem.GetBaseItem() != null &&
                    roomItem.GetBaseItem().InteractionType == Interaction.TileStackMagic);

            if (stackMagic != null)
            {
                height = stackMagic.Z;
            }
            else if (!onRoller && item.GetBaseItem().InteractionType != Interaction.TileStackMagic)
            {
                if (list2.Any(
                        current5 =>
                            current5 != null && current5.Id != item.Id && current5.GetBaseItem() != null &&
                            !current5.GetBaseItem().Stackable))
                {
                    if (!flag) return false;
                    AddOrUpdateItem(item.Id);
                    _room.GetGameMap().AddToMap(item);
                    return false;
                }

                if (item.Rot != newRot && item.X == newX && item.Y == newY) height = item.Z;
                foreach (var current6 in list2) if (current6.Id != item.Id && current6.TotalHeight > height) height = current6.TotalHeight;
            }

            if (item.GetBaseItem().Name == "boutique_mannequin1" || item.GetBaseItem().Name == "gld_wall_tall")
            {
                if (newRot < 0 || newRot > 12)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "pirate_stage2" || item.GetBaseItem().Name == "pirate_stage2_g" || item.GetBaseItem().Name == "cpunk_c15_stage1" || item.GetBaseItem().Name == "paris_c15_pavement" || item.GetBaseItem().Name == "lm_crystal_skull" || item.GetBaseItem().Name == "horse_track_fence_cor" || item.GetBaseItem().Name == "paris_c15_roof" || item.GetBaseItem().Name == "hween_c15_road" || item.GetBaseItem().Name == "hween_c15_pavement")
            {
                if (newRot < 0 || newRot > 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "gh_div_cor" || item.GetBaseItem().Name == "hblooza14_duckcrn" || item.GetBaseItem().Name == "hween13_dwarfcrn")
            {
                if (newRot != 1 && newRot != 3 && newRot != 5 && newRot != 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "val14_b_roof" || item.GetBaseItem().Name == "val14_g_roof" || item.GetBaseItem().Name == "val14_y_roof")
            {
                if (newRot != 2 && newRot != 3 && newRot != 4 && newRot != 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "val13_div_1")
            {
                if (newRot < 0 || newRot > 6)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "room_info15_shrub1")
            {
                if (newRot != 0 && newRot != 2 && newRot != 3 && newRot != 4 && newRot != 6)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "room_info15_div")
            {
                if (newRot < 0 || newRot > 5)
                {
                    newRot = 0;
                }
            }
            else
            {
                if (newRot != 0 && newRot != 2 && newRot != 4 && newRot != 6 && newRot != 8)
                {
                    newRot = 0;
                }
            }
            item.Rot = newRot;


            item.SetState(newX, newY, height, affectedTiles);

            if (newItem)
            {
                if (FloorItems.ContainsKey(item.Id)) return true;
                if (item.IsFloorItem) FloorItems.TryAdd(item.Id, item);
                else if (item.IsWallItem) WallItems.TryAdd(item.Id, item);

                AddOrUpdateItem(item.Id);
                if (sendMessage)
                {
                    var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AddFloorItemMessageComposer"));
                    item.Serialize(serverMessage);
                    serverMessage.AppendString(_room.RoomData.Group != null
                        ? "Jerry"
                        : _room.RoomData.Owner);
                    _room.SendMessage(serverMessage);
                }
            }
            else
            {
                AddOrUpdateItem(item.Id);
                if (!onRoller && sendMessage)
                {
                    if (specialMove)
                    {
                        var message = new ServerMessage(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
                        message.AppendInteger(oldCoord.X);
                        message.AppendInteger(oldCoord.Y);
                        message.AppendInteger(newX);
                        message.AppendInteger(newY);
                        message.AppendInteger(1);
                        message.AppendInteger(item.Id);
                        message.AppendString(TextHandling.GetString(item.Z));
                        message.AppendString(TextHandling.GetString(item.Z));
                        message.AppendInteger(-1);
                        _room.SendMessage(message);
                    }
                    else
                    {
                        var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer"));
                        item.Serialize(message);
                        _room.SendMessage(message);
                    }
                }
                if (item.IsWired) _room.GetWiredHandler().MoveWired(item);
            }
            _room.GetGameMap().AddToMap(item);
            if (item.GetBaseItem().IsSeat) updateRoomUserStatuses = true;
            if (updateRoomUserStatuses)
            {
                _room.GetRoomUserManager().OnUserUpdateStatus(oldCoord.X, oldCoord.Y);
                _room.GetRoomUserManager().OnUserUpdateStatus(item.X, item.Y);
            }
            if (newItem) OnHeightMapUpdate(affectedTiles);

            return true;
        }

        /// <summary>
        /// Removes all furniture.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal List<RoomItem> RemoveAllFurniture(GameClient session)
        {
            var items = new List<RoomItem>();
            foreach (var item in FloorItems.Values.ToArray())
            {
                item.Interactor.OnRemove(session, item);
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PickUpFloorItemMessageComposer"));
                serverMessage.AppendString(item.Id.ToString());
                serverMessage.AppendBool(false); //expired
                serverMessage.AppendInteger(item.UserId); //pickerId
                serverMessage.AppendInteger(0); // delay
                _room.SendMessage(serverMessage);

                items.Add(item);
            }

            foreach (var item in WallItems.Values.ToArray())
            {
                item.Interactor.OnRemove(session, item);
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PickUpWallItemMessageComposer"));
                serverMessage.AppendString(item.Id.ToString());
                serverMessage.AppendInteger(item.UserId);
                _room.SendMessage(serverMessage);

                items.Add(item);
            }

            _removedItems.Clear();
            _updatedItems.Clear();
            WallItems.Clear();
            FloorItems.Clear();
            Rollers.Clear();

            using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Format("UPDATE items_rooms SET room_id='0' WHERE room_id='{0}'", _room.RoomId));
            }
            _room.GetGameMap().GenerateMaps(true);

            _room.GetRoomUserManager().OnUserUpdateStatus();
            return items;
        }

        /// <summary>
        /// Sets the speed.
        /// </summary>
        /// <param name="p">The p.</param>
        internal void SetSpeed(uint p)
        {
            _rollerSpeed = p;
        }

        /// <summary>
        /// Loads the furniture.
        /// </summary>
        internal void LoadFurniture()
        {
            if (FloorItems == null)
                FloorItems = new ConcurrentDictionary<uint, RoomItem>();
            else
                FloorItems.Clear();

            if (WallItems == null)
                WallItems = new ConcurrentDictionary<uint, RoomItem>();
            else
                WallItems.Clear();

            {
                using (var queryReactor = Plus.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery(
                        "SELECT `items_rooms`.* , COALESCE(`items_groups`.`group_id`, 0) AS group_id FROM `items_rooms` LEFT OUTER JOIN `items_groups` ON `items_rooms`.`id` = `items_groups`.`id` WHERE items_rooms.room_id=@roomid LIMIT 20000");
                    queryReactor.AddParameter("roomid", _room.RoomId);
                    var table = queryReactor.GetTable();
                    if (table.Rows.Count == 20000)
                    {
                        var clientByUserId = Plus.GetGame().GetClientManager().GetClientByUserId((uint)_room.RoomData.OwnerId);
                        if (clientByUserId != null)
                            clientByUserId.SendNotif(
                                "Your room has more than 20000 items in it. The current limit of items per room is 20000.\nTo view the rest, pick some of these items up!");
                    }
                    foreach (DataRow dataRow in table.Rows)
                        try
                        {
                            var num = Convert.ToUInt32(dataRow[0]);
                            var x = Convert.ToInt32(dataRow[5]);
                            var y = Convert.ToInt32(dataRow[6]);
                            var num2 = Convert.ToDouble(dataRow[7]);
                            var rot = Convert.ToSByte(dataRow[8]);
                            var num3 = Convert.ToUInt32(dataRow[1]);
                            if (num3 == 0u)
                            {
                                queryReactor.SetQuery(string.Format("UPDATE items_rooms SET user_id='{0}' WHERE id='{1}' LIMIT 1;", _room.RoomData.OwnerId, num));
                                queryReactor.RunQuery();
                            }
                            string locationData;
                            if (string.IsNullOrWhiteSpace(dataRow[9].ToString()))
                            {
                                queryReactor.SetQuery(string.Format("SELECT type FROM catalog_furnis WHERE id='{0}' LIMIT 1;", dataRow[3]));
                                var StringMe = queryReactor.GetString();
                                if (StringMe == "i")
                                {
                                    try
                                    {
                                        locationData = ":w=0,2 l=11,53 l";
                                        queryReactor.RunFastQuery(string.Format("UPDATE items_rooms SET wall_pos='{0}' WHERE id='{1}' LIMIT 1;", locationData, num));
                                    }
                                    catch
                                    {
                                        Out.WriteLine("Wall Default Position Trouble", "Plus.Items", ConsoleColor.DarkCyan);
                                    }
                                }
                            }
                            locationData = Convert.ToString(dataRow[9]);
                            var num4 = Convert.ToUInt32(dataRow[3]);
                            string extraData;
                            if (DBNull.Value.Equals(dataRow[4]))
                                extraData = string.Empty;
                            else
                                extraData = (string)dataRow[4];
                            string songCode;
                            if (DBNull.Value.Equals(dataRow["songcode"]))
                                songCode = string.Empty;
                            else
                                songCode = (string)dataRow["songcode"];
                            var eGroup = Convert.ToUInt32(dataRow["group_id"]);
                            if (!string.IsNullOrWhiteSpace(locationData))
                            {
                                var wallCoord = new WallCoordinate(":" + locationData.Split(':')[1]);
                                var value = new RoomItem(num, _room.RoomId, num4, extraData, wallCoord, _room, num3,
                                    eGroup, Plus.GetGame().GetItemManager().GetItem(num4).FlatId,
                                    Plus.EnumToBool((string)dataRow["builders"]));

                                WallItems.TryAdd(num, value);
                            }
                            else
                            {
                               // if (Plus.GetGame().GetItemManager().GetItem(num4) != null)
                               // {


                                if (num == null)
                                    continue;

                                if (_room.RoomId == null)
                                    continue;

                                if (num4 == null)
                                    continue;
                                
                                if (songCode == null)
                                    continue;

                                if (extraData == null)
                                    continue;

                                if (x == null)
                                    continue;

                                if (y == null)
                                    continue;

                                if (num2 == null)
                                    continue;

                                if (rot == null)
                                    continue;

                                if (_room == null)
                                    continue;

                                if (num == null)
                                    continue;

                                if (eGroup == null)
                                    continue;

                                if (num4 == null)
                                    continue;


                                if (Plus.GetGame() == null)
                                    continue;

                                if (Plus.GetGame().GetItemManager() == null)
                                    continue;

                                if (Plus.GetGame().GetItemManager().GetItem(num4) == null)
                                    continue;

                                if (Plus.GetGame().GetItemManager().GetItem(num4).FlatId == null)
                                    continue;

                                if (Plus.EnumToBool((string)dataRow["builders"]) == null)
                                    continue;


                                    RoomItem roomItem = new RoomItem(num, _room.RoomId, num4, extraData, x, y, num2, rot, _room, num3, eGroup, Plus.GetGame().GetItemManager().GetItem(num4).FlatId, songCode, Plus.EnumToBool((string)dataRow["builders"]));
                               // }

                                if (!_room.GetGameMap().ValidTile(x, y))
                                {
                                    GameClient clientByUserID2 = Plus.GetGame().GetClientManager().GetClientByUserId(num3);
                                    if (clientByUserID2 != null)
                                    {
                                        clientByUserID2.GetHabbo().GetInventoryComponent().AddNewItem(roomItem.Id, roomItem.BaseItem, roomItem.ExtraData, eGroup, true, true, 0, 0, "");
                                        queryReactor.RunFastQuery("UPDATE items_rooms SET room_id='0' WHERE id='" + roomItem.Id + "' LIMIT 1");
                                        clientByUserID2.GetHabbo().GetInventoryComponent().UpdateItems(true);
                                    }
                                    else
                                    {
                                        queryReactor.RunFastQuery("UPDATE items_rooms SET room_id='0' WHERE id='" + roomItem.Id + "' LIMIT 1");
                                    }
                                }
                                else
                                {
                                    if (roomItem.GetBaseItem().InteractionType == Interaction.Hopper)
                                    {
                                        this.HopperCount++;
                                    }
                                    if (!FloorItems.ContainsKey(num))
                                    {
                                        FloorItems.TryAdd(num, roomItem);
                                    }
                                }
                            }
                        }
                        catch (Exception value2)
                        {
                            Console.WriteLine(value2);
                        }

                    foreach (var current in FloorItems.Values)
                    {
                        if (current.IsWired)
                            _room.GetWiredHandler().LoadWired(_room.GetWiredHandler().GenerateNewItem(current));
                        if (current.IsRoller)
                            GotRollers = true;
                        else if (current.GetBaseItem().InteractionType == Interaction.Dimmer)
                        {
                            if (_room.MoodlightData == null)
                                _room.MoodlightData = new MoodlightData(current.Id);
                        }
                        else if (current.GetBaseItem().InteractionType == Interaction.RoomBg &&
                                 _room.TonerData == null)
                            _room.TonerData = new TonerData(current.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the furniture.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="wasPicked">if set to <c>true</c> [was picked].</param>
        internal void RemoveFurniture(GameClient session, uint id, bool wasPicked = true)
        {
            var item = GetItem(id);
            if (item == null)
                return;
            if (item.GetBaseItem().InteractionType == Interaction.FootballGate)
                _room.GetSoccer().UnRegisterGate(item);
            if (item.GetBaseItem().InteractionType != Interaction.Gift)
                item.Interactor.OnRemove(session, item);
            RemoveRoomItem(item, wasPicked);
        }

        /// <summary>
        /// Removes the room item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="wasPicked">if set to <c>true</c> [was picked].</param>
        internal void RemoveRoomItem(RoomItem item, bool wasPicked)
        {
            if (item.IsWallItem)
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PickUpWallItemMessageComposer"));
                serverMessage.AppendString(item.Id.ToString());
                serverMessage.AppendInteger(wasPicked ? item.UserId : 0);
                _room.SendMessage(serverMessage);
            }
            else if (item.IsFloorItem)
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("PickUpFloorItemMessageComposer"));
                serverMessage.AppendString(item.Id.ToString());
                serverMessage.AppendBool(false); //expired
                serverMessage.AppendInteger(wasPicked ? item.UserId : 0); //pickerId
                serverMessage.AppendInteger(0); // delay
                _room.SendMessage(serverMessage);
            }

            RoomItem junkItem;
            if (item.IsWallItem)
            {
                WallItems.TryRemove(item.Id, out junkItem);
            }
            else
            {
                FloorItems.TryRemove(item.Id, out junkItem);
                _room.GetGameMap().RemoveFromMap(item);
            }

            RemoveItem(item.Id);
            _room.GetRoomUserManager().OnUserUpdateStatus(item.X, item.Y);
        }

        /// <summary>
        /// Updates the item on roller.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="nextCoord">The next coord.</param>
        /// <param name="rolledId">The rolled identifier.</param>
        /// <param name="nextZ">The next z.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage UpdateItemOnRoller(RoomItem item, Point nextCoord, uint rolledId, double nextZ)
        {
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
            serverMessage.AppendInteger(item.X);
            serverMessage.AppendInteger(item.Y);
            serverMessage.AppendInteger(nextCoord.X);
            serverMessage.AppendInteger(nextCoord.Y);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(item.Id);
            serverMessage.AppendString(TextHandling.GetString(item.Z));
            serverMessage.AppendString(TextHandling.GetString(nextZ));
            serverMessage.AppendInteger(rolledId);
            SetFloorItem(item, nextCoord.X, nextCoord.Y, nextZ);
            return serverMessage;
        }

        /// <summary>
        /// Updates the user on roller.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="nextCoord">The next coord.</param>
        /// <param name="rollerId">The roller identifier.</param>
        /// <param name="nextZ">The next z.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage UpdateUserOnRoller(RoomUser user, Point nextCoord, uint rollerId, double nextZ)
        {
            var serverMessage = new ServerMessage(0);
            serverMessage.Init(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
            serverMessage.AppendInteger(user.X);
            serverMessage.AppendInteger(user.Y);
            serverMessage.AppendInteger(nextCoord.X);
            serverMessage.AppendInteger(nextCoord.Y);
            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(rollerId);
            serverMessage.AppendInteger(2);
            serverMessage.AppendInteger(user.VirtualId);
            serverMessage.AppendString(TextHandling.GetString(user.Z));
            serverMessage.AppendString(TextHandling.GetString(nextZ));
            _room.GetGameMap()
                .UpdateUserMovement(new Point(user.X, user.Y), new Point(nextCoord.X, nextCoord.Y), user);
            _room.GetGameMap().GameMap[user.X, user.Y] = 1;
            user.X = nextCoord.X;
            user.Y = nextCoord.Y;
            user.Z = nextZ;
            _room.GetGameMap().GameMap[user.X, user.Y] = 0;
            return serverMessage;
        }

        /// <summary>
        /// Sets the floor item.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="item">The item.</param>
        /// <param name="newX">The new x.</param>
        /// <param name="newY">The new y.</param>
        /// <param name="newRot">The new rot.</param>
        /// <param name="newItem">if set to <c>true</c> [new item].</param>
        /// <param name="onRoller">if set to <c>true</c> [on roller].</param>
        /// <param name="sendMessage">if set to <c>true</c> [send message].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetFloorItem(GameClient session, RoomItem item, int newX, int newY, int newRot, bool newItem,
            bool onRoller, bool sendMessage)
        {
            return SetFloorItem(session, item, newX, newY, newRot, newItem, onRoller, sendMessage, true, false);
        }

        /// <summary>
        /// Sets the floor item.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="item">The item.</param>
        /// <param name="newX">The new x.</param>
        /// <param name="newY">The new y.</param>
        /// <param name="newRot">The new rot.</param>
        /// <param name="newItem">if set to <c>true</c> [new item].</param>
        /// <param name="onRoller">if set to <c>true</c> [on roller].</param>
        /// <param name="sendMessage">if set to <c>true</c> [send message].</param>
        /// <param name="updateRoomUserStatuses">if set to <c>true</c> [update room user statuses].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetFloorItem(GameClient session, RoomItem item, int newX, int newY, int newRot, bool newItem,
                                   bool onRoller, bool sendMessage, bool updateRoomUserStatuses, bool specialMove)
        {
            var flag = false;
            if (!newItem) flag = _room.GetGameMap().RemoveFromMap(item);

            var affectedTiles = Gamemap.GetAffectedTiles(item.GetBaseItem().Length,
                item.GetBaseItem().Width, newX, newY, newRot);

            var oldCoord = item.Coordinate;

            if (!_room.GetGameMap().ValidTile(newX, newY) && !session.GetRoleplay().DebugStacking ||
                (_room.GetGameMap().SquareHasUsers(newX, newY) && !item.GetBaseItem().IsSeat && !session.GetRoleplay().DebugStacking))
            {
                if (!flag) return false;

                AddOrUpdateItem(item.Id);
                _room.GetGameMap().AddToMap(item);
                return false;
            }

            if (
                affectedTiles.Values.Any(
                    current =>
                        !_room.GetGameMap().ValidTile(current.X, current.Y) ||
                        (_room.GetGameMap().SquareHasUsers(current.X, current.Y) && !item.GetBaseItem().IsSeat)))
            {
                if (!flag) return false;
                AddOrUpdateItem(item.Id);
                _room.GetGameMap().AddToMap(item);
                return false;
            }
            double height = _room.GetGameMap().Model.SqFloorHeight[newX][newY];
            if (!onRoller)
            {
                if (_room.GetGameMap().Model.SqState[newX][newY] != SquareState.Open && !item.GetBaseItem().IsSeat && !session.GetRoleplay().DebugStacking)
                {
                    if (!flag) return false;
                    AddOrUpdateItem(item.Id);
                    return false;
                }
                if (
                    affectedTiles.Values.Any(
                        current2 =>
                            !item.GetBaseItem().IsSeat &&
                            _room.GetGameMap().Model.SqState[current2.X][current2.Y] != SquareState.Open && !session.GetRoleplay().DebugStacking))
                {
                    if (!flag) return false;

                    AddOrUpdateItem(item.Id);
                    _room.GetGameMap().AddToMap(item);
                    return false;
                }
                if (!item.GetBaseItem().IsSeat && !item.IsRoller || session.GetRoleplay().DebugStacking)
                    if (
                        affectedTiles.Values.Any(
                            current3 => _room.GetGameMap().GetRoomUsers(new Point(current3.X, current3.Y)).Any()))
                    {
                        if (!flag) return false;
                        AddOrUpdateItem(item.Id);
                        _room.GetGameMap().AddToMap(item);
                        return false;
                    }
            }

            var furniObjects = GetFurniObjects(newX, newY);
            var list = new List<RoomItem>();
            var list2 = new List<RoomItem>();
            foreach (
                var furniObjects2 in
                    affectedTiles.Values
                        .Select(current4 => GetFurniObjects(current4.X, current4.Y))
                        .Where(furniObjects2 => furniObjects2 != null))
                list.AddRange(furniObjects2);
            list2.AddRange(furniObjects);
            list2.AddRange(list);

            var stackMagic = list2.FirstOrDefault(
                roomItem =>
                    roomItem != null && roomItem.GetBaseItem() != null &&
                    roomItem.GetBaseItem().InteractionType == Interaction.TileStackMagic);

            if (stackMagic != null)
            {
                height = stackMagic.Z;
            }
            else if (!onRoller && item.GetBaseItem().InteractionType != Interaction.TileStackMagic)
            {
                if (list2.Any(
                        current5 =>
                            current5 != null && current5.Id != item.Id && current5.GetBaseItem() != null &&
                            !current5.GetBaseItem().Stackable && !session.GetRoleplay().DebugStacking))
                {
                    if (!flag) return false;
                    AddOrUpdateItem(item.Id);
                    _room.GetGameMap().AddToMap(item);
                    return false;
                }

                if (item.Rot != newRot && item.X == newX && item.Y == newY) height = item.Z;
                foreach (var current6 in list2) if (current6.Id != item.Id && current6.TotalHeight > height) height = current6.TotalHeight;
            }

            if (item.GetBaseItem().Name == "boutique_mannequin1" || item.GetBaseItem().Name == "gld_wall_tall")
            {
                if (newRot < 0 || newRot > 12)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "pirate_stage2" || item.GetBaseItem().Name == "pirate_stage2_g" || item.GetBaseItem().Name == "cpunk_c15_stage1" || item.GetBaseItem().Name == "paris_c15_pavement" || item.GetBaseItem().Name == "lm_crystal_skull" || item.GetBaseItem().Name == "horse_track_fence_cor" || item.GetBaseItem().Name == "paris_c15_roof" || item.GetBaseItem().Name == "hween_c15_road" || item.GetBaseItem().Name == "hween_c15_pavement")
            {
                if (newRot < 0 || newRot > 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "gh_div_cor" || item.GetBaseItem().Name == "hblooza14_duckcrn" || item.GetBaseItem().Name == "hween13_dwarfcrn")
            {
                if (newRot != 1 && newRot != 3 && newRot != 5 && newRot != 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "val14_b_roof" || item.GetBaseItem().Name == "val14_g_roof" || item.GetBaseItem().Name == "val14_y_roof")
            {
                if (newRot != 2 && newRot != 3 && newRot != 4 && newRot != 7)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "val13_div_1")
            {
                if (newRot < 0 || newRot > 6)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "room_info15_shrub1")
            {
                if (newRot != 0 && newRot != 2 && newRot != 3 && newRot != 4 && newRot != 6)
                {
                    newRot = 0;
                }
            }
            else if (item.GetBaseItem().Name == "room_info15_div")
            {
                if (newRot < 0 || newRot > 5)
                {
                    newRot = 0;
                }
            }
            else
            {
                if (newRot != 0 && newRot != 2 && newRot != 4 && newRot != 6 && newRot != 8)
                {
                    newRot = 0;
                }
            }
            item.Rot = newRot;

            if (session.GetRoleplay().DebugStacking)
            {
                height = session.GetRoleplay().DebugStack;
            }

            item.SetState(newX, newY, height, affectedTiles);
            if (!onRoller && session != null) item.Interactor.OnPlace(session, item);
            if (newItem)
            {
                if (FloorItems.ContainsKey(item.Id)) return true;
                if (item.IsFloorItem) FloorItems.TryAdd(item.Id, item);
                else if (item.IsWallItem) WallItems.TryAdd(item.Id, item);

                AddOrUpdateItem(item.Id);
                if (sendMessage)
                {
                    var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AddFloorItemMessageComposer"));
                    item.Serialize(serverMessage);
                    serverMessage.AppendString(_room.RoomData.Group != null
                        ? session.GetHabbo().UserName
                        : _room.RoomData.Owner);
                    _room.SendMessage(serverMessage);
                }
            }
            else
            {
                AddOrUpdateItem(item.Id);
                if (!onRoller && sendMessage)
                {
                    if (specialMove)
                    {
                        var message = new ServerMessage(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
                        message.AppendInteger(oldCoord.X);
                        message.AppendInteger(oldCoord.Y);
                        message.AppendInteger(newX);
                        message.AppendInteger(newY);
                        message.AppendInteger(1);
                        message.AppendInteger(item.Id);
                        message.AppendString(TextHandling.GetString(item.Z));
                        message.AppendString(TextHandling.GetString(item.Z));
                        message.AppendInteger(-1);
                        _room.SendMessage(message);
                    }
                    else
                    {
                        var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer"));
                        item.Serialize(message);
                        _room.SendMessage(message);
                    }
                }
                if (item.IsWired) _room.GetWiredHandler().MoveWired(item);
            }
            _room.GetGameMap().AddToMap(item);
            if (item.GetBaseItem().IsSeat) updateRoomUserStatuses = true;
            if (updateRoomUserStatuses)
            {
                _room.GetRoomUserManager().OnUserUpdateStatus(oldCoord.X, oldCoord.Y);
                _room.GetRoomUserManager().OnUserUpdateStatus(item.X, item.Y);
            }
            if (newItem) OnHeightMapUpdate(affectedTiles);

            return true;
        }

        /// <summary>
        /// Called when [height map update].
        /// </summary>
        /// <param name="affectedTiles">The affected tiles.</param>
        internal void OnHeightMapUpdate(Dictionary<int, ThreeDCoord> affectedTiles)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateFurniStackMapMessageComposer"));
            message.AppendByte((byte)affectedTiles.Count);
            foreach (var coord in affectedTiles.Values)
            {
                message.AppendByte((byte)coord.X);
                message.AppendByte((byte)coord.Y);
                message.AppendShort((short)(_room.GetGameMap().SqAbsoluteHeight(coord.X, coord.Y) * 256));
            }
            _room.SendMessage(message);
        }

        /// <summary>
        /// Called when [height map update].
        /// </summary>
        /// <param name="affectedTiles">The affected tiles.</param>
        internal void OnHeightMapUpdate(ICollection affectedTiles)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateFurniStackMapMessageComposer"));
            message.AppendByte((byte)affectedTiles.Count);
            foreach (Point coord in affectedTiles)
            {
                message.AppendByte((byte)coord.X);
                message.AppendByte((byte)coord.Y);
                message.AppendShort((short)(_room.GetGameMap().SqAbsoluteHeight(coord.X, coord.Y) * 256));
            }
            _room.SendMessage(message);
        }

        /// <summary>
        /// Called when [height map update].
        /// </summary>
        /// <param name="oldCoords">The old coords.</param>
        /// <param name="newCoords">The new coords.</param>
        internal void OnHeightMapUpdate(List<Point> oldCoords, List<Point> newCoords)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateFurniStackMapMessageComposer"));
            message.AppendByte((byte)(oldCoords.Count + newCoords.Count));
            foreach (var coord in oldCoords)
            {
                message.AppendByte((byte)coord.X);
                message.AppendByte((byte)coord.Y);
                message.AppendShort((short)(_room.GetGameMap().SqAbsoluteHeight(coord.X, coord.Y) * 256));
            }
            foreach (var nCoord in newCoords)
            {
                message.AppendByte((byte)nCoord.X);
                message.AppendByte((byte)nCoord.Y);
                message.AppendShort((short)(_room.GetGameMap().SqAbsoluteHeight(nCoord.X, nCoord.Y) * 256));
            }
            _room.SendMessage(message);
        }

        /// <summary>
        /// Gets the furni objects.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal List<RoomItem> GetFurniObjects(int x, int y)
        {
            return _room.GetGameMap().GetCoordinatedItems(new Point(x, y));
        }

        /// <summary>
        /// Sets the floor item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="newX">The new x.</param>
        /// <param name="newY">The new y.</param>
        /// <param name="newZ">The new z.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetFloorItem(RoomItem item, int newX, int newY, double newZ)
        {
            _room.GetGameMap().RemoveFromMap(item);
            item.SetState(newX, newY, newZ,
                Gamemap.GetAffectedTiles(item.GetBaseItem().Length,
                    item.GetBaseItem().Width, newX, newY, item.Rot));
            if (item.GetBaseItem().InteractionType == Interaction.RoomBg && _room.TonerData == null)
                _room.TonerData = new TonerData(item.Id);
            AddOrUpdateItem(item.Id);
            _room.GetGameMap().AddItemToMap(item, true);
            return true;
        }

        /// <summary>
        /// Sets the floor item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="newX">The new x.</param>
        /// <param name="newY">The new y.</param>
        /// <param name="newZ">The new z.</param>
        /// <param name="rot">The rot.</param>
        /// <param name="sendUpdate">if set to <c>true</c> [sendupdate].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetFloorItem(RoomItem item, int newX, int newY, double newZ, int rot, bool sendUpdate)
        {
            _room.GetGameMap().RemoveFromMap(item);
            item.SetState(newX, newY, newZ,
                Gamemap.GetAffectedTiles(item.GetBaseItem().Length,
                    item.GetBaseItem().Width, newX, newY, rot));
            if (item.GetBaseItem().InteractionType == Interaction.RoomBg && _room.TonerData == null)
                _room.TonerData = new TonerData(item.Id);
            AddOrUpdateItem(item.Id);
            _room.GetGameMap().AddItemToMap(item, true);
            if (!sendUpdate)
                return true;
            var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomItemMessageComposer"));
            item.Serialize(message);
            _room.SendMessage(message);
            return true;
        }

        /// <summary>
        /// Sets the wall item.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetWallItem(GameClient session, RoomItem item)
        {
            if (!item.IsWallItem || WallItems.ContainsKey(item.Id))
                return false;
            if (FloorItems.ContainsKey(item.Id))
                return true;
            item.Interactor.OnPlace(session, item);
            if (item.GetBaseItem().InteractionType == Interaction.Dimmer && _room.MoodlightData == null)
            {
                _room.MoodlightData = new MoodlightData(item.Id);
                item.ExtraData = _room.MoodlightData.GenerateExtraData();
            }

            WallItems.TryAdd(item.Id, item);
            AddOrUpdateItem(item.Id);

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AddWallItemMessageComposer"));
            item.Serialize(serverMessage);
            serverMessage.AppendString(_room.RoomData.Owner);
            _room.SendMessage(serverMessage);
            return true;
        }

        /// <summary>
        /// Updates the item.
        /// </summary>
        /// <param name="itemId">The item.</param>
        internal void AddOrUpdateItem(uint itemId)
        {
            if (_removedItems.Contains(itemId))
                _removedItems.Remove(itemId);
            if (_updatedItems.Contains(itemId))
                return;

            _updatedItems.Add(itemId);
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="itemId"></param>
        internal void RemoveItem(uint itemId)
        {
            if (_updatedItems.Contains(itemId))
                _updatedItems.Remove(itemId);

            if (!_removedItems.Contains(itemId))
                _removedItems.Add(itemId);

            RoomItem junkItem;
            Rollers.TryRemove(itemId, out junkItem);
        }

        /// <summary>
        /// Called when [cycle].
        /// </summary>
        internal void OnCycle()
        {
            if (GotRollers)
                try
                {
                    _room.SendMessage(CycleRollers());
                }
                catch (Exception ex)
                {
                    Logging.LogThreadException(ex.ToString(),
                        string.Format("rollers for room with ID {0}", _room.RoomId));
                    GotRollers = false;
                }
            if (_roomItemUpdateQueue.Count <= 0) return;
            var list = new List<RoomItem>();
            lock (_roomItemUpdateQueue.SyncRoot)
            {
                while (_roomItemUpdateQueue.Count > 0)
                {
                    var roomItem = (RoomItem)_roomItemUpdateQueue.Dequeue();
                    roomItem.ProcessUpdates();
                    if (roomItem.IsTrans || roomItem.UpdateCounter > 0)
                        list.Add(roomItem);
                }
                foreach (var current in list)
                    _roomItemUpdateQueue.Enqueue(current);
            }
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            FloorItems.Clear();
            WallItems.Clear();
            _removedItems.Clear();
            _updatedItems.Clear();
            lock (_roomItemUpdateQueue.SyncRoot)
            {
                _roomItemUpdateQueue.Clear();
                _roomItemUpdateQueue = null;
            }
            _room = null;
            FloorItems = null;
            WallItems = null;
            _removedItems = null;
            _updatedItems = null;
            BreedingBear.Clear();
            BreedingTerrier.Clear();
            WallItems = null;
            BreedingBear.Clear();
            BreedingTerrier.Clear();
        }

        /// <summary>
        /// Cycles the rollers.
        /// </summary>
        /// <returns>List&lt;ServerMessage&gt;.</returns>
        private List<ServerMessage> CycleRollers()
        {
            if (!GotRollers)
                return new List<ServerMessage>();
            if (_roolerCycle >= _rollerSpeed || _rollerSpeed == 0)
            {
                _rollerItemsMoved.Clear();
                _rollerUsersMoved.Clear();
                _rollerMessages.Clear();
                foreach (var current in Rollers.Values)
                {
                    var squareInFront = current.SquareInFront;
                    var roomItemForSquare = _room.GetGameMap().GetRoomItemForSquare(current.X, current.Y);
                    var userForSquare = _room.GetRoomUserManager().GetUserForSquare(current.X, current.Y);
                    if (!roomItemForSquare.Any() && userForSquare == null)
                        continue;
                    var coordinatedItems = _room.GetGameMap().GetCoordinatedItems(squareInFront);
                    var nextZ = 0.0;
                    var num = 0;
                    var flag = false;
                    var num2 = 0.0;
                    var flag2 = true;
                    foreach (var current2 in coordinatedItems.Where(current2 => current2.IsRoller))
                    {
                        flag = true;
                        if (current2.TotalHeight > num2)
                            num2 = current2.TotalHeight;
                    }
                    if (flag)
                        using (var enumerator3 = coordinatedItems.GetEnumerator())
                        {
                            while (enumerator3.MoveNext())
                            {
                                var current3 = enumerator3.Current;
                                if (current3.TotalHeight > num2)
                                    flag2 = false;
                            }
                            goto IL_192;
                        }
                    goto IL_17C;
                IL_192:
                    nextZ = num2;
                    var flag3 = num > 0 ||
                                _room.GetRoomUserManager().GetUserForSquare(squareInFront.X, squareInFront.Y) != null;
                    foreach (var current4 in roomItemForSquare)
                    {
                        var num3 = current4.Z - current.TotalHeight;
                        if (_rollerItemsMoved.Contains(current4.Id) ||
                            !_room.GetGameMap().CanRollItemHere(squareInFront.X, squareInFront.Y) || !flag2 ||
                            !(current.Z < current4.Z) ||
                            _room.GetRoomUserManager().GetUserForSquare(squareInFront.X, squareInFront.Y) != null)
                            continue;
                        _rollerMessages.Add(UpdateItemOnRoller(current4, squareInFront, current.Id, num2 + num3));
                        _rollerItemsMoved.Add(current4.Id);
                    }
                    if (userForSquare != null && !userForSquare.IsWalking && flag2 && !flag3 &&
                        _room.GetGameMap().CanRollItemHere(squareInFront.X, squareInFront.Y) &&
                        _room.GetGameMap().GetFloorStatus(squareInFront) != 0 &&
                        !_rollerUsersMoved.Contains(userForSquare.HabboId))
                    {
                        _rollerMessages.Add(UpdateUserOnRoller(userForSquare, squareInFront, current.Id, nextZ));
                        _rollerUsersMoved.Add(userForSquare.HabboId);
                    }
                    continue;
                IL_17C:
                    num2 += _room.GetGameMap().GetHeightForSquareFromData(squareInFront);
                    goto IL_192;
                }
                _roolerCycle = 0;
                return _rollerMessages;
            }

            {
                _roolerCycle++;
            }
            return new List<ServerMessage>();
        }
    }
}