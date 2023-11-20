//-----------------------------------------------------------------------
// <copyright file="DebugUI.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    using Dalamud.Logging;

    using FFXIVRPCalendarPlugin;
    using FFXIVRPCalendarPlugin.Models;
    using FFXIVRPCalendarPlugin.Services;

    using ImGuiNET;

    using Lumina.Excel;
    using Lumina.Excel.GeneratedSheets;

    /// <summary>
    /// The RP Calendar debugging UI.
    /// </summary>
    public class DebugUI : IDisposable
    {
        private bool disposedValue;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugUI"/> class.
        /// </summary>
        public DebugUI()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Debug UI is visible.
        /// </summary>
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        /// <summary>
        /// Draw the Debug UI.
        /// </summary>
        public void Draw()
        {
            if (!this.visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(1200, 540), ImGuiCond.Always);
            try
            {
                if (ImGui.Begin(
                    "FFXIV RP Event Calendar Settings",
                    ref this.visible,
                    ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
                {
                    ImGui.Text("Debug Info:");
                    if (Plugin.ClientState.LocalPlayer != null)
                    {
                        ImGui.Text($"Player Found: {Plugin.ClientState.LocalPlayer.Name.TextValue}");
                        var gameWorld = Plugin.ClientState.LocalPlayer.CurrentWorld?.GameData;
                        if (gameWorld != null)
                        {
                            ImGui.Text($"Current Server: {gameWorld.Name.RawString}");
                            ImGui.Text($"Current Server Id: {gameWorld.RowId}");
                            ImGui.Text($"Current Datacenter: {gameWorld.DataCenter.Value}");

                            ExcelSheet<World>? worldSheet = Plugin.DataManager.GetExcelSheet<World>();
                            ExcelSheet<WorldDCGroupType>? dataCentgerSheet = Plugin.DataManager.GetExcelSheet<WorldDCGroupType>();

                            if (worldSheet != null && dataCentgerSheet != null)
                            {
                                IDictionary<uint, World> worldDicitonary = worldSheet.ToDictionary(row => row.RowId, row => row);
                                IDictionary<uint, WorldDCGroupType> datacenterDictionary = dataCentgerSheet.ToDictionary(row => row.RowId, row => row);
                                World world = worldDicitonary[gameWorld.RowId];
                                WorldDCGroupType datacenter = datacenterDictionary[world.DataCenter.Row];
                                uint serverId = worldDicitonary[gameWorld.RowId].RowId;
                                string serverName = worldDicitonary[gameWorld.RowId].InternalName;
                                string datacenterName = datacenter.Name;
                                uint datacenterId = datacenter.RowId;
                                byte regionId = datacenter.Region;

                                if (worldDicitonary.ContainsKey(gameWorld.RowId))
                                {
                                    ImGui.Text($"Server Id: {serverId}");
                                    ImGui.Text($"Server Name: {serverName}");
                                    ImGui.Text($"Datacenter Name: {datacenterName}");
                                }

                                ImGui.Text("Server List");
                                ImGui.Spacing();
                                if (ImGui.BeginTable(
                                    "##ServerList",
                                    4,
                                    ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg))
                                {
                                    ImGui.TableSetupColumn("##Region");
                                    ImGui.TableSetupColumn("##DataCenter");
                                    ImGui.TableSetupColumn("##Server");
                                    ImGui.TableSetupColumn("##ServerId");

                                    if (WorldService.Worlds != null)
                                    {
                                        List<World> validWorlds = WorldService.WorldList.OrderBy(x => x.InternalName.RawString).ToList();

                                        foreach (World currentWorld in validWorlds)
                                        {
                                            WorldDCGroupType dc = datacenterDictionary[currentWorld.DataCenter.Row];
                                            string regionName = ((WorldDCRegion)dc.Region).GetDescription();
                                            ImGui.TableNextRow();
                                            ImGui.TableSetColumnIndex(0);
                                            ImGui.Text($"{regionName}");
                                            ImGui.TableSetColumnIndex(1);
                                            ImGui.Text($"{dc.Name}");
                                            ImGui.TableSetColumnIndex(2);
                                            ImGui.Text($"{currentWorld.InternalName}");
                                            ImGui.TableSetColumnIndex(3);
                                            ImGui.Text($"{currentWorld.RowId}");
                                        }
                                    }

                                    ImGui.EndTable();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error(ex, ex.Message);
                Plugin.ChatGui.PrintError($"RPEventCalendar: DebugIU: {ex.Message}");
            }

            ImGui.End();
        }

        /// <summary>
        /// Dispose of the <see cref="DebugUI"/> class.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the <see cref="DebugUI"/> class.
        /// </summary>
        /// <param name="disposing">A value indicating whether the Dispose call has been called directly instead of from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                this.disposedValue = true;
            }
        }
    }
}
