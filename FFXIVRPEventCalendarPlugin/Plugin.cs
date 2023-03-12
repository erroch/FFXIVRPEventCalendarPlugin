//-----------------------------------------------------------------------
// <copyright file="Plugin.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin
{
    using System.IO;
    using System.Reflection;

    using Dalamud.Data;
    using Dalamud.Game.ClientState;
    using Dalamud.Game.Command;
    using Dalamud.Game.Gui;
    using Dalamud.IoC;
    using Dalamud.Plugin;

    using FFXIVRPCalendarPlugin.Models;

    using FFXIVRPCalendarPlugin.UI;

    /// <summary>
    /// The FFXIV RP Event Calendar Dalamud Plugin.
    /// </summary>
    public sealed class Plugin : IDalamudPlugin
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="pluginInterface">The Dalamud plugin interface.</param>
        public Plugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            this.PluginUi = new PluginUI(this.Configuration);

            this.BuildCommands();

            this.PluginInterface.UiBuilder.Draw += this.DrawUI;
        }

        /// <summary>
        /// Gets the FFXIV client state.
        /// </summary>
        [PluginService]
        [RequiredVersion("1.0")]
        public static ClientState ClientState { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud Data Manger.
        /// </summary>
        [PluginService]
        [RequiredVersion("1.0")]
        public static DataManager DataManager { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud Command Manager.
        /// </summary>
        [PluginService]
        [RequiredVersion("1.0")]
        public static CommandManager CommandManager { get; private set; } = null!;

        /// <summary>
        /// Gets the FFXIV Chat Gui.
        /// </summary>
        [PluginService]
        [RequiredVersion("1.0")]
        public static ChatGui ChatGui { get; private set; } = null!;

        /// <summary>
        /// Gets the provided name of the plugin.
        /// </summary>
        public string Name => "FFXIV RP Event Calendar";

        private DalamudPluginInterface PluginInterface { get; init; }

        private Configuration Configuration { get; init; }

        private PluginUI PluginUi { get; init; }

        /// <summary>
        /// Dispose of the Plugin class.
        /// </summary>
        public void Dispose()
        {
            this.PluginUi.Dispose();
            Plugin.CommandManager.RemoveHandler("/eventsnow");
            Plugin.CommandManager.RemoveHandler("/eventsnext");
            Plugin.CommandManager.RemoveHandler("/events");
            Plugin.CommandManager.RemoveHandler("/resetcalendar");
            Plugin.CommandManager.RemoveHandler("/eventsdebug");
        }

        private void BuildCommands()
        {
            Plugin.CommandManager.AddHandler("/eventsnow", new CommandInfo(this.OnCommand) { HelpMessage = "Get RP events happening now." });
            Plugin.CommandManager.AddHandler("/eventsnext", new CommandInfo(this.OnCommand) { HelpMessage = "Get the next 5 events on my datacenter. (or /eventsnext x for x events up to 25.)" });
            Plugin.CommandManager.AddHandler("/events", new CommandInfo(this.OnCommand) { HelpMessage = "Show the RP Calendar UI." });
            Plugin.CommandManager.AddHandler("/resetcalendar", new CommandInfo(this.OnCommand) { ShowInHelp = false });
            Plugin.CommandManager.AddHandler("/eventsdebug", new CommandInfo(this.OnCommand) { ShowInHelp = false });
        }

        private void OnCommand(string command, string args)
        {
            switch (command)
            {
                case "/resetcalendar":
                    this.Configuration.Reset();
                    this.Configuration.Save();
                    break;
                case "/events":
                    this.PluginUi.Visible = true;
                    break;
                case "/eventsdebug":
                    this.PluginUi.DebugVisible = true;
                    break;
            }
        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        }
    }
}
