//-----------------------------------------------------------------------
// <copyright file="SettingsUI.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    using Dalamud.Game.Gui;

    using FFXIVRPCalendarPlugin.Models;
    using FFXIVRPCalendarPlugin.Services;

    using ImGuiNET;
    using Lumina.Excel.GeneratedSheets;

    /// <summary>
    /// The Settings UI.
    /// </summary>
    public class SettingsUI : IDisposable
    {
        private const string TimeZoneSelectionTitle = "Timezone: ";
        private readonly Configuration configuration;
        private bool disposedValue;
        private bool visible = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsUI"/> class.
        /// </summary>
        /// <param name="configuration">The Dalamud configuration.</param>
        public SettingsUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Settings UI is visible.
        /// </summary>
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DebugUI()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Dispose of the <see cref="SettingsUI"/> class.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Draw the Settings UI.
        /// </summary>
        public void Draw()
        {
            if (!this.visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(500, 240), ImGuiCond.Always);
            if (ImGui.Begin(
                "FFXIV RP Event Calendar Settings",
                ref this.visible,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                this.BuildTimezone();

                byte[] urlBuffer = System.Text.Encoding.ASCII.GetBytes(this.configuration.ApiAddress.PadRight(255, ' '));
                if (ImGui.InputText("URL", urlBuffer, 256))
                {
                    string apiAddress = System.Text.Encoding.Default.GetString(urlBuffer).Replace("\0", string.Empty).Trim();
                    this.configuration.ApiAddress = apiAddress.Trim();
                    this.configuration.Save();
                }
            }

            ImGui.End();
        }

        private void BuildTimezone()
        {
            IEnumerable<TimeZoneInfo> timeZoneInfos = TimeZoneInfo.GetSystemTimeZones();
            ImGui.Text(TimeZoneSelectionTitle);
            ImGui.SameLine();

            if (ImGui.BeginCombo(" ", this.configuration.TimeZoneInfo.DisplayName))
            {
                foreach (TimeZoneInfo timeZoneInfo in timeZoneInfos)
                {
                    if (ImGui.Selectable(
                        timeZoneInfo.DisplayName,
                        this.configuration.TimeZoneInfo == timeZoneInfo))
                    {
                        this.configuration.TimeZoneInfo = timeZoneInfo;
                        this.configuration.Save();
                    }
                }

                ImGui.EndCombo();
            }
        }

        /// <summary>
        /// Dispose of the <see cref="SettingsUI"/> class.
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
