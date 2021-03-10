using System;
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
            foreach (Chest sourceChest in GetChests(location))
            {
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
                    monitor.Log($"target chest: {targetChest.items[0].DisplayName}", LogLevel.Debug);
                    monitor.Log($"old item: {oldItem.Stack}", LogLevel.Debug);
                    monitor.Log($"new item: {newItem.Stack}", LogLevel.Debug);
                    monitor.Log($"max: {oldItem.maximumStackSize()}", LogLevel.Debug);
                    if (oldItem.Stack + newItem.Stack > oldItem.maximumStackSize())
                    {
                        newItem.Stack -= oldItem.maximumStackSize() - oldItem.Stack;
                        oldItem.Stack = oldItem.maximumStackSize();
                        chestOperations += 2;
                        curContent[newItem.Name] = new ItemChest(newItem, sourceChest);
                    }
                    else
                    {
                        oldItem.Stack += newItem.Stack;
                        newItem.Stack = 0;
                        sourceChest.grabItemFromChest(newItem, Game1.MasterPlayer);
                        chestOperations += 2;
                    }
                    monitor.Log($"target chest: {targetChest.items[0].DisplayName}", LogLevel.Debug);
                    monitor.Log($"old item: {oldItem.Stack}", LogLevel.Debug);
                    monitor.Log($"new item: {newItem.Stack}", LogLevel.Debug);
                }
            }
            return chestOperations;
        }
    }
}
