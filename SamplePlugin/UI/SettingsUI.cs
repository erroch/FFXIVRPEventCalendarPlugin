namespace FFXIVRPCalendarPlugin.UI
{
    using ImGuiNET;
    using Dalamud.Game.Gui;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;
    using FFXIVRPCalendar;
    using FFXIVRPCalendar.Services;
    using FFXIVRPCalendar.Models;

    public class SettingsUI : IDisposable
    {
        private Configuration configuration;
        private bool disposedValue;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        // passing in the image here just for simplicity
        public SettingsUI(Configuration configuration)
        {
            this.configuration = configuration;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DebugUI()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Draw()
        {
            if (!visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(500, 240), ImGuiCond.Always);
            if (ImGui.Begin("FFXIV RP Event Calendar Settings", ref visible,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.BeginCombo("Range", this.configuration.ConfigurationProperties.EventDisplayRange.GetDescription(), ImGuiComboFlags.None))
                {
                    if (ImGui.Selectable("Everywhere"))
                    {
                        configuration.ConfigurationProperties.EventDisplayRange = EventDisplayRange.Everywhere;
                        configuration.Save();
                    }

                    if (ImGui.Selectable("Physical Datacenter"))
                    {
                        configuration.ConfigurationProperties.EventDisplayRange = EventDisplayRange.PhysicalDatacenter;
                        configuration.Save();
                    }

                    if (ImGui.Selectable("Local Datacenter"))
                    {
                        configuration.ConfigurationProperties.EventDisplayRange = EventDisplayRange.LocalDatacenter;
                        configuration.Save();
                    }

                    if (ImGui.Selectable("Current Server"))
                    {
                        configuration.ConfigurationProperties.EventDisplayRange = EventDisplayRange.LocalServer;
                        configuration.Save();
                    }
                    ImGui.EndCombo();
                }

                // can't ref a property, so use a local copy
                bool useLocalTime = configuration.UseLocalTimeZone;
                string altTimeZoneName = configuration.ConfigurationProperties.TimeZoneInfo.DisplayName;

                if (ImGui.Checkbox("Use Local Time Zone", ref useLocalTime))
                {
                    configuration.UseLocalTimeZone = useLocalTime;
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    configuration.Save();
                }

                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(configuration.ConfigurationProperties.TimeZoneInfo.DisplayName.PadRight(255, ' '));
                if (ImGui.InputText("Alternate Timzeone", buffer, 256))
                {
                    string alternateTimezone = System.Text.Encoding.Default.GetString(buffer);
                    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(alternateTimezone);
                    if (timeZoneInfo != null)
                    {
                        configuration.ConfigurationProperties.TimeZoneInfo = timeZoneInfo;
                    }

                    configuration.Save();
                }

                byte[] urlBuffer = System.Text.Encoding.ASCII.GetBytes(configuration.ConfigurationProperties.ApiAddress.PadRight(255, ' '));
                if (ImGui.InputText("URL", urlBuffer, 256))
                {
                    string apiAddress = System.Text.Encoding.Default.GetString(urlBuffer).Replace("\0", string.Empty).Trim();
                    configuration.ConfigurationProperties.ApiAddress = apiAddress.Trim();
                    configuration.Save();
                }
            }
            ImGui.End();
        }

    }
}
