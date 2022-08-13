namespace FFXIVRPCalendarPlugin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    using ImGuiNET;
    using Dalamud.Game.Gui;
    using Dalamud.Game.ClientState;
    using Dalamud.IoC;

    using FFXIVRPCalendarPlugin;
    using FFXIVRPCalendarPlugin.Models;
    using FFXIVRPCalendarPlugin.Services;
    using Lumina.Excel.GeneratedSheets;

    public class EventsService : IDisposable
    {
        private uint? lastServerId;
        private DateTime? lastRefresh;
        public DateTime? LastRefreshLocalTime;

        private const uint REFRESH_INTERVAL = 15 * 60;

        public EventsService(Configuration configuration)
        {
            this.configuration = configuration;
            Plugin.ClientState.TerritoryChanged += OnTerritoryChanged;
        }

        private void OnTerritoryChanged(object? sender, ushort args)
        {
            if (Plugin.ClientState.LocalPlayer?.CurrentWorld?.Id != this.lastServerId)
            {
                this.FilteredEvents = null;
                this.FilterEvents();
                this.lastServerId = Plugin.ClientState.LocalPlayer?.CurrentWorld?.Id;
            }
        }

        public List<RPEvent>? RoleplayEvents { get; set; }
        private List<RPEvent>? FilteredEvents { get; set; }

        public List<RPEvent>? ServerEvents { get; set; }

        public List<RPEvent>? DatacenterEvents { get; set;  }

        public List<RPEvent>? RegionEvents { get; set; }

        private bool eventsLoaded = false;
        private bool disposedValue;
        private readonly Configuration configuration;

        public void RefreshEvents(bool forceRefresh = false)
        {
            if (lastRefresh.HasValue)
            {
                TimeSpan sinceLastRefresh = DateTime.UtcNow - lastRefresh.Value;
                if (sinceLastRefresh.TotalSeconds >= REFRESH_INTERVAL)
                {
                    forceRefresh = true;
                }
            }

            if (!this.eventsLoaded || forceRefresh)
            {
                this.eventsLoaded = true;
                this.lastRefresh = DateTime.UtcNow;
                this.LastRefreshLocalTime = TimeZoneInfo.ConvertTimeFromUtc(this.lastRefresh.Value, configuration.ConfigurationProperties.TimeZoneInfo);

                try
                {
                    Task<List<RPEvent>?>.Run(async () => await EventService.GetToday(this.configuration.ConfigurationProperties)
                        .ContinueWith(
                            t =>
                            {
                                if (t.IsFaulted)
                                {
                                    AggregateException? ex = t.Exception;
                                    if (ex != null)
                                    {
                                        Plugin.ChatGui.PrintError(ex.Message);
                                    }
                                    else
                                    {
                                        Plugin.ChatGui.PrintError("error fetching events.");
                                    }
                                    this.RoleplayEvents = null;
                                }
                                else
                                {
                                    this.RoleplayEvents = t.Result;
                                    this.FilterEvents();
                                }
                            }
                        )
                    );
                }
                catch (Exception ex)
                {
                    Plugin.ChatGui.PrintError($"error fetching events: {ex.Message}");
                }
            }
            else
            {
                if (CheckForServerChange())
                {
                    this.FilterEvents();
                }
            }
        }

        public void FilterEvents()
        {
            if (this.RoleplayEvents != null)
            {
                DateTime nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, configuration.ConfigurationProperties.TimeZoneInfo);
                DateTime startUTC = TimeZoneInfo.ConvertTimeToUtc(nowLocal.Date, configuration.ConfigurationProperties.TimeZoneInfo);
                DateTime endUTC = TimeZoneInfo.ConvertTimeToUtc(nowLocal.Date.AddDays(1), configuration.ConfigurationProperties.TimeZoneInfo);

                this.FilteredEvents = this.RoleplayEvents
                    .Where(x =>
                        (configuration.ConfigurationProperties.Categories is null || configuration.ConfigurationProperties.Categories.Contains(x.EventCategory)) &&
                        (configuration.ConfigurationProperties.Ratings is null || configuration.ConfigurationProperties.Ratings.Contains(x.ESRBRating)) &&
                        x.StartTimeUTC >= startUTC &&
                        x.StartTimeUTC <= endUTC
                    )
                    .Select(x =>
                    {
                        RPEvent result = x;
                        result.LocalStartTime = TimeZoneInfo.ConvertTimeFromUtc(result.StartTimeUTC, configuration.ConfigurationProperties.TimeZoneInfo);
                        result.LocalEndTime = TimeZoneInfo.ConvertTimeFromUtc(result.EndTimeUTC, configuration.ConfigurationProperties.TimeZoneInfo);
                        return x;
                    })
                .ToList();

                World ? gameWorld = Plugin.ClientState?.LocalPlayer?.CurrentWorld?.GameData;
                if (gameWorld == null)
                {
                    this.ServerEvents = null;
                    this.DatacenterEvents = null;
                    this.RegionEvents = null;
                }
                else
                {
                    this.ServerEvents = this.FilteredEvents?.Where(x => x.ServerId == gameWorld.RowId).ToList();
                    uint[] worldIds = WorldService.GetDatacenterWorldIds(gameWorld.DataCenter.Row);

                    this.DatacenterEvents = this.FilteredEvents?
                        .Where(x => worldIds.Contains(x.ServerId))
                        .ToList();

                    WorldDCGroupType? datacenter = WorldService.Datacenters?[gameWorld.DataCenter.Row];
                    if (datacenter == null)
                    {
                        this.RegionEvents = null;
                    }
                    else
                    {
                        uint[] regionWorldIds = WorldService.GetRegionWorldIds(datacenter.Region);
                        this.RegionEvents = this.FilteredEvents?
                            .Where(x => regionWorldIds.Contains(x.ServerId))
                            .ToList();
                    }
                }
            }
        }

        private bool CheckForServerChange()
        {
            if (Plugin.ClientState.LocalPlayer?.CurrentWorld?.Id != this.lastServerId)
            {
                this.lastServerId = Plugin.ClientState.LocalPlayer?.CurrentWorld?.Id;
                return true;
            }
            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Plugin.ClientState.TerritoryChanged -= OnTerritoryChanged;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~EventsService()
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
    }
}
