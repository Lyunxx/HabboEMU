using Plus.HabboHotel.Items;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Plus.HabboHotel.Catalogs
{
    /// <summary>
    /// Class CatalogItem.
    /// </summary>
    internal class CatalogItem
    {
        /// <summary>
        /// The identifier
        /// </summary>
        internal readonly uint Id;

        /// <summary>
        /// The item identifier string
        /// </summary>
        internal readonly string ItemIdString;

        /// <summary>
        /// The name
        /// </summary>
        internal readonly string Name;

        /// <summary>
        /// The credits cost
        /// </summary>
        internal readonly uint CreditsCost;

        /// <summary>
        /// The bel credits cost
        /// </summary>
        internal readonly uint BelCreditsCost;

        /// <summary>
        /// The loyalty cost
        /// </summary>
        internal readonly uint LoyaltyCost;

        /// <summary>
        /// The duckets cost
        /// </summary>
        internal readonly uint DucketsCost;

        /// <summary>
        /// The page identifier
        /// </summary>
        internal readonly int PageId;

        /// <summary>
        /// The song identifier
        /// </summary>
        internal readonly uint SongId;

        /// <summary>
        /// The is limited
        /// </summary>
        internal readonly bool IsLimited;

        /// <summary>
        /// The limited stack
        /// </summary>
        internal readonly int LimitedStack;

        /// <summary>
        /// The have offer
        /// </summary>
        internal readonly bool HaveOffer;

        /// <summary>
        /// The club only
        /// </summary>
        internal readonly bool ClubOnly;

        /// <summary>
        /// The extra data
        /// </summary>
        internal readonly string ExtraData;

        /// <summary>
        /// The items
        /// </summary>
        internal Dictionary<Item, uint> Items;

        /// <summary>
        /// The limited selled
        /// </summary>
        internal int LimitedSelled;

        /// <summary>
        /// The badge
        /// </summary>
        internal string Badge;

        /// <summary>
        /// The base identifier
        /// </summary>
        internal uint BaseId;

        /// <summary>
        /// The first amount
        /// </summary>
        internal uint FirstAmount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogItem"/> class.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="name">The name.</param>
        internal CatalogItem(DataRow row, string name)
        {
            Id = Convert.ToUInt32(row["id"]);
            Name = name;
            ItemIdString = row["item_ids"].ToString();
            Items = new Dictionary<Item, uint>();
            var itemIds = ItemIdString.Split(';');
            var amounts = row["amounts"].ToString().Split(';');
            for (var i = 0; i < itemIds.Length; i++)
            {
                uint id;
                uint amount;

                if (!uint.TryParse(itemIds[i], out id) || GetBaseItem(id) == null) continue;
                uint.TryParse(amounts[i], out amount);

                Items.Add(GetBaseItem(id), amount);
            }

            BaseId = Items.Keys.First().ItemId;
            FirstAmount = Items.Values.First();
            PageId = (int) row["page_id"];
            uint.TryParse(row["cost_credits"].ToString(), out CreditsCost);
            uint.TryParse(row["cost_belcredits"].ToString(), out BelCreditsCost);
            uint.TryParse(row["cost_loyalty"].ToString(), out LoyaltyCost);
            uint.TryParse(row["cost_duckets"].ToString(), out DucketsCost);

            LimitedSelled = (int) row["limited_sells"];
            LimitedStack = (int) row["limited_stack"];
            IsLimited = (LimitedStack > 0);
            Badge = (string) row["badge"];
            HaveOffer = ((string) row["offer_active"] == "1");
            ClubOnly = ((string) row["club_only"] == "1");
            ExtraData = (string) row["extradata"];
            SongId = (uint) row["song_id"];
        }

        /// <summary>
        /// Gets the base item.
        /// </summary>
        /// <param name="itemIds">The item ids.</param>
        /// <returns>Item.</returns>
        internal Item GetBaseItem(uint itemIds)
        {
            var item = Plus.GetGame().GetItemManager().GetItem(itemIds);
            if (item == null)
                Out.WriteLine(string.Format("UNKNOWN ItemIds: {0}", itemIds), "", ConsoleColor.DarkRed);
            return item;
        }

        /// <summary>
        /// Gets the first base item.
        /// </summary>
        /// <returns>Item.</returns>
        internal Item GetFirstBaseItem()
        {
            return GetBaseItem(BaseId);
        }
    }
}