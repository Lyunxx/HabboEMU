#region

using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.Messages;
using Plus.Messages.Parsers;

#endregion

namespace Plus.HabboHotel.Items.Interactor
{
    internal class InteractorCrafting : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "";
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("CraftableProductsMessageComposer"));
            var crafting = Plus.GetGame().GetCraftingManager();
            message.AppendInteger(crafting.CraftingRecipes.Count);
            foreach(var recipe in crafting.CraftingRecipes.Values)
            {
                message.AppendString(recipe.Result);
                message.AppendString(recipe.Result);
            }
            message.AppendInteger(crafting.CraftableItems.Count);
            foreach (var itemName in crafting.CraftableItems)
            {
                message.AppendString(itemName);
            }
            session.SendMessage(message);
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
        {
        }
    }
}