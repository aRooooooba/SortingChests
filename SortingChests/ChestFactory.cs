﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace SortingChests
{
    class ChestFactory
    {
        private readonly IMultiplayerHelper multiplayer;
        public IDictionary<GameLocation, IDictionary<string, ItemChest>> contentDict;
        private IMonitor monitor;

        public ChestFactory(IMultiplayerHelper multiplayer, IMonitor monitor)
        {
            this.multiplayer = multiplayer;
            contentDict = new Dictionary<GameLocation, IDictionary<string, ItemChest>>();
            this.monitor = monitor;
        }

        private IEnumerable<GameLocation> GetAccessibleLocations()
        {
            if (Context.IsMainPlayer)
            {
                var locations = Game1.locations
                    .Concat
                    (
                        from location in Game1.locations.OfType<BuildableGameLocation>()
                        from building in location.buildings
                        where building.indoors.Value != null
                        select building.indoors.Value
                    );
                return locations;
            }
            return multiplayer.GetActiveLocations();
        }

        private IEnumerable<Chest> GetChests(GameLocation location)
        {
            IEnumerable<Chest> Search()
            {
                foreach (KeyValuePair<Vector2, StardewValley.Object> pair in location.Objects.Pairs)
                {
                    if (pair.Value is Chest chest && chest.playerChest.Value)
                    {
                        yield return chest;
                    }
                }
            }
            return Search();
        }

        /// <summary>
        /// Sort the chests in the given location.
        /// </summary>
        /// <param name="location">The location where the chests to be sorted resides.</param>
        /// <returns>The number of chests operation.</returns>
        public int SortChests(GameLocation location)
        {
            int chestOperations = 0;
            if (!contentDict.ContainsKey(location))
                contentDict.Add(location, new Dictionary<string, ItemChest>());
            IDictionary<string, ItemChest> curContent = contentDict[location];
            IList<Item> toBeDeleted = new List<Item>();
            foreach (Chest sourceChest in GetChests(location))
            {
                monitor.Log($"CHEST", LogLevel.Debug);
                foreach (Item newItem in sourceChest.items)
                {
                    if (newItem.Stack == newItem.maximumStackSize())
                        continue;
                    if (!curContent.ContainsKey(newItem.Name))
                    {
                        curContent.Add(newItem.Name, new ItemChest(newItem, sourceChest));
                        continue;
                    }
                    Item oldItem = curContent[newItem.Name].Item;
                    Chest targetChest = curContent[newItem.Name].Chest;
                    if (targetChest == sourceChest)
                        continue;
                    chestOperations += 2;
                    if (oldItem.Stack + newItem.Stack > oldItem.maximumStackSize())
                    {
                        newItem.Stack -= oldItem.maximumStackSize() - oldItem.Stack;
                        oldItem.Stack = oldItem.maximumStackSize();
                        curContent[newItem.Name] = new ItemChest(newItem, sourceChest);
                    }
                    else
                    {
                        monitor.Log($"act", LogLevel.Debug);
                        oldItem.Stack += newItem.Stack;
                        toBeDeleted.Add(newItem);
                        monitor.Log($"act2", LogLevel.Debug);
                        if (oldItem.Stack == oldItem.maximumStackSize())
                            curContent.Remove(newItem.Name);
                    }
                }
                foreach (Item item in toBeDeleted)
                {
                    item.Stack = 0;
                    sourceChest.grabItemFromChest(item, Game1.MasterPlayer);
                }
                Game1.exitActiveMenu();
            }
            return chestOperations;
        }

        public int SortChestsInAllLocations()
        {
            int chestOperations = 0;
            foreach (GameLocation location in GetAccessibleLocations())
                chestOperations += SortChests(location);
            return chestOperations;
        }
    }
}
