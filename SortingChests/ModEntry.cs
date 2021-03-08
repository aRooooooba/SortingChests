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
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private ChestFactory chestFactory;
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            chestFactory = new ChestFactory(helper.Multiplayer, Monitor);
            helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
        }


        /*********
        ** Private methods
        *********/
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            Monitor.Log("called", LogLevel.Debug);
            chestFactory.sortChests(e.Player.currentLocation);
        }
    }
}