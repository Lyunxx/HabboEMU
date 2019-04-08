using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.Database.Manager.Database.Session_Details.Interfaces;

namespace Plus.HabboHotel.Items.Crafting
{
    class CraftingManager
    {
        internal Dictionary<string, CraftingRecipe> CraftingRecipes;
        internal List<string> CraftableItems;
        public CraftingManager()
        {
            CraftingRecipes = new Dictionary<string, CraftingRecipe>();
            CraftableItems = new List<string>();
        }
        internal void Initialize(IQueryAdapter dbClient)
        {
            CraftingRecipes.Clear();
            dbClient.SetQuery("SELECT * FROM crafting_recipes");
            var recipes = dbClient.GetTable();
            foreach (DataRow recipe in recipes.Rows)
            {
                CraftingRecipe value = new CraftingRecipe((string)recipe["id"], (string)recipe["items"], (string)recipe["result"]);
                CraftingRecipes.Add((string)recipe["id"], value);
            }

            CraftableItems.Clear();
            dbClient.SetQuery("SELECT * FROM crafting_items");
            var items = dbClient.GetTable();
            foreach (DataRow item in items.Rows)
            {
                CraftableItems.Add((string)item["itemName"]);
            }
        }
        internal CraftingRecipe GetRecipe(string name)
        {
            if (CraftingRecipes.ContainsKey(name))
                return CraftingRecipes[name];
            else
                return null;
        }
    }
}
