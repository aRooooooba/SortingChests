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
        private int skipTriggers;
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            skipTriggers = 0;
            chestFactory = new ChestFactory(helper.Multiplayer, Monitor);
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.World.ChestInventoryChanged += OnChestInventoryChanged;
        }


        /*********
        ** Private methods
        *********/
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            skipTriggers += chestFactory.SortChestsInAllLocations();
        }

        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnChestInventoryChanged(object sender, ChestInventoryChangedEventArgs e)
        {
            Monitor.Log($"skip {skipTriggers}", LogLevel.Debug);
            if (skipTriggers > 0)
            {
                skipTriggers--;
                return;
            }
            Monitor.Log("real called", LogLevel.Debug);
        }
    }
}