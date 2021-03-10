using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;

namespace SortingChests
{
    /// <summary>
    /// Contains an item and the chest containing the item. The item is not fully stacked.
    /// </summary>
    public class ItemChest
    {
        public Item Item { get; }
        public Chest Chest { get; }

        public ItemChest(Item item, Chest chest)
        {
            this.Item = item;
            this.Chest = chest;
        }
    }
}
