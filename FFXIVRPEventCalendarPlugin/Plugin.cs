namespace FFXIVRPCalendarPlugin
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Reflection;

    using Dalamud.Data;
    using Dalamud.Game.ClientState;
    using Dalamud.Game.Command;
    using Dalamud.Game.Gui;
    using Dalamud.IoC;
    using Dalamud.Plugin;

    using FFXIVRPCalendarPlugin.Services;
    using FFXIVRPCalendarPlugin.Models;

    using FFXIVRPCalendarPlugin.UI;
    using Lumina.Excel;
    using Lumina.Excel.GeneratedSheets;

    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "FFXIV RP Event Calendar";

        [PluginService]
        [RequiredVersion("1.0")]
        public static ClientState ClientState { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static DataManager Data { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static CommandManager CommandManager { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static ChatGui ChatGui { get; private set; } = null!;

        private DalamudPluginInterface PluginInterface { get; init; }
        // private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        public Plugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
            
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var imagePath = Path.Combine(Path.GetDirectoryName(assemblyLocation)!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            this.PluginUi = new PluginUI(this.Configuration);


            this.BuildCommands();

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

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
            Plugin.CommandManager.AddHandler("/eventsnow", new CommandInfo(OnCommand) { HelpMessage = "Get RP events happening now." });
            Plugin.CommandManager.AddHandler("/eventsnext", new CommandInfo(OnCommand) { HelpMessage = "Get the next 5 events on my datacenter. (or /eventsnext x for x events up to 25.)" });
            Plugin.CommandManager.AddHandler("/events", new CommandInfo(OnCommand) { HelpMessage = "Show the RP Calendar UI." });
            Plugin.CommandManager.AddHandler("/resetcalendar", new CommandInfo(OnCommand) { ShowInHelp = false });
            Plugin.CommandManager.AddHandler("/eventsdebug", new CommandInfo(OnCommand) { ShowInHelp = false });
        }

        private void OnCommand(string command, string args)
        {
            switch (command)
            {
                case "/resetcalendar":
                    this.Configuration.ConfigurationProperties = new ConfigurationProperties();
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

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }
    }
}
