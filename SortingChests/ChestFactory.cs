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

        public void SortChests(GameLocation location)
        {
            if (!contentDict.ContainsKey(location))
                contentDict.Add(location, new Dictionary<string, ItemChest>());
            IDictionary<string, ItemChest> curContent = contentDict[location];
            foreach (Chest chest in GetChests(location))
            {
                foreach (Item item in chest.items)
                {
                    if (!curContent.ContainsKey(item.Name))
                        curContent.Add(item.Name, new ItemChest(item, chest));
                    else if (curContent[item.Name].chest == chest)
                        continue;
                    else
                    {
                        curContent[item.Name].item.addToStack(item);
                        item.Stack = 0;
                        chest.grabItemFromChest(item, Game1.MasterPlayer);
                    }
                }
            }
        }
    }
}
