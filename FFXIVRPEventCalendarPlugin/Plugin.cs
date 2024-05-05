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
    using Dalamud.Interface;
    using Dalamud.IoC;
    using Dalamud.Plugin;
    using Dalamud.Plugin.Services;
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
            this.PluginInterface.UiBuilder.OpenMainUi += this.OpenMainUi;
        }

        /// <summary>
        /// Gets the FFXIV client state.
        /// </summary>
        [PluginService]
        [RequiredVersion("1.0")]
        public static IClientState ClientState { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud Data Manger.
        /// </summary>
        [PluginService]
        [RequiredVersion("1.0")]
        public static IDataManager DataManager { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud Command Manager.
        /// </summary>
        [PluginService]
        [RequiredVersion("1.0")]
        public static ICommandManager CommandManager { get; private set; } = null!;

        /// <summary>
        /// Gets the FFXIV Chat Gui.
        /// </summary>
        [PluginService]
        [RequiredVersion("1.0")]
        public static IChatGui ChatGui { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud Plugin Log.
        /// </summary>
        [PluginService]
        [RequiredVersion("1.0")]
        public static IPluginLog PluginLog { get; private set; } = null!;

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
            Plugin.CommandManager.RemoveHandler("/events");
            Plugin.CommandManager.RemoveHandler("/resetcalendar");
            Plugin.CommandManager.RemoveHandler("/eventsdebug");
        }

        private void BuildCommands()
        {
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

        private void OpenMainUi() => this.PluginUi.Visible = true;
    }
}
