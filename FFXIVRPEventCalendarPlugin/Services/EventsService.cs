//-----------------------------------------------------------------------
// <copyright file="EventsService.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    using FFXIVRPCalendarPlugin;
    using FFXIVRPCalendarPlugin.Models;
    using Lumina.Excel.GeneratedSheets;

    /// <summary>
    /// A service handing event list building and filtering.
    /// </summary>
    public class EventsService : IDisposable
    {
        private const uint REFRESHINTERVAL = 15 * 60;

        private readonly Configuration configuration;

        private uint? lastServerId;
        private DateTime? lastRefresh;
        private bool eventsLoaded = false;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsService"/> class.
        /// </summary>
        /// <param name="configuration">The plugin configuration.</param>
        public EventsService(Configuration configuration)
        {
            this.configuration = configuration;
            Plugin.ClientState.TerritoryChanged += this.OnTerritoryChanged;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="EventsService"/> class.
        /// </summary>
        ~EventsService()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// Gets the user local time for the last time the calendar cache was updated.
        /// </summary>
        public DateTime? LastRefreshLocalTime { get; internal set; }

        /// <summary>
        /// Gets the filtered list of roleplay events for the current server.
        /// </summary>
        public List<RPEvent>? ServerEvents { get; internal set; }

        /// <summary>
        /// Gets the filtered list of roleplay events for the current datacenter.
        /// </summary>
        public List<RPEvent>? DatacenterEvents { get; internal set; }

        /// <summary>
        /// Gets the filtered list of roleplay events for the current physical region.
        /// </summary>
        public List<RPEvent>? RegionEvents { get; internal set; }

        private List<RPEvent>? RoleplayEvents { get; set; }

        private List<RPEvent>? FilteredEvents { get; set; }

        /// <summary>
        /// Refresh the event list.
        /// </summary>
        /// <param name="forceRefresh">A flag indicating if the cache values should be ignored and a refresh done even if within the normal timeout interfval.</param>
        public void RefreshEvents(bool forceRefresh = false)
        {
            if (this.lastRefresh.HasValue)
            {
                TimeSpan sinceLastRefresh = DateTime.UtcNow - this.lastRefresh.Value;
                if (sinceLastRefresh.TotalSeconds >= REFRESHINTERVAL)
                {
                    forceRefresh = true;
                }
            }

            if (!this.eventsLoaded || forceRefresh)
            {
                this.eventsLoaded = true;
                this.lastRefresh = DateTime.UtcNow;
                this.LastRefreshLocalTime = TimeZoneInfo.ConvertTimeFromUtc(this.lastRefresh.Value, this.configuration.ConfigurationProperties.TimeZoneInfo);

                try
                {
                    Task<List<RPEvent>?>.Run(async () => await CalendarService.GetToday(this.configuration.ConfigurationProperties)
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
                            }));
                }
                catch (Exception ex)
                {
                    Plugin.ChatGui.PrintError($"error fetching events: {ex.Message}");
                }
            }
            else
            {
                if (this.CheckForServerChange())
                {
                    this.FilterEvents();
                }
            }
        }

        /// <summary>
        /// Updates the filtered events lists based on configuration options.
        /// </summary>
        public void FilterEvents()
        {
            if (this.RoleplayEvents != null)
            {
                DateRange dateRange = this.GetDatesForTimeframe(this.configuration.ConfigurationProperties.EventTimeframe);

                this.FilteredEvents = this.RoleplayEvents
                    .Where(x =>
                        (this.configuration.ConfigurationProperties.Categories is null || this.configuration.ConfigurationProperties.Categories.Contains(x.EventCategory)) &&
                        (this.configuration.ConfigurationProperties.Ratings is null || this.configuration.ConfigurationProperties.Ratings.Contains(x.ESRBRating)) &&
                        (
                            (this.configuration.ConfigurationProperties.EventTimeframe == EventTimeframe.Now && x.StartTimeUTC <= DateTime.UtcNow && x.EndTimeUTC >= DateTime.UtcNow) ||
                            (x.StartTimeUTC >= dateRange.StartDateUTC && x.StartTimeUTC <= dateRange.EndDateUTC)))
                    .Select(x =>
                    {
                        RPEvent result = x;
                        result.LocalStartTime = TimeZoneInfo.ConvertTimeFromUtc(result.StartTimeUTC, this.configuration.ConfigurationProperties.TimeZoneInfo);
                        result.LocalEndTime = TimeZoneInfo.ConvertTimeFromUtc(result.EndTimeUTC, this.configuration.ConfigurationProperties.TimeZoneInfo);
                        return x;
                    })
                .ToList();

                World? gameWorld = Plugin.ClientState?.LocalPlayer?.CurrentWorld?.GameData;
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

        /// <summary>
        /// Dispose of the <see cref="EventsService"/> class.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the <see cref="EventsService"/> class.
        /// </summary>
        /// <param name="disposing">A value indicating whether the Dispose call has been called directly instead of from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    Plugin.ClientState.TerritoryChanged -= this.OnTerritoryChanged;
                }

                this.disposedValue = true;
            }
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

        private bool CheckForServerChange()
        {
            if (Plugin.ClientState.LocalPlayer?.CurrentWorld?.Id != this.lastServerId)
            {
                this.lastServerId = Plugin.ClientState.LocalPlayer?.CurrentWorld?.Id;
                return true;
            }

            return false;
        }

        private DateRange GetDatesForTimeframe(EventTimeframe timeframe)
        {
            return timeframe switch
            {
                EventTimeframe.Now => new DateRange(DateTime.UtcNow, DateTime.UtcNow),
                EventTimeframe.NextHours => new DateRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1)),
                EventTimeframe.Today => this.GetTodayRange(),
                EventTimeframe.ThisWeek => this.GetThisWeekRange(),
                _ => this.GetTodayRange(),
            };
        }

        private DateRange GetTodayRange()
        {
            DateTime nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.configuration.ConfigurationProperties.TimeZoneInfo);
            DateTime startUTC = TimeZoneInfo.ConvertTimeToUtc(nowLocal.Date, this.configuration.ConfigurationProperties.TimeZoneInfo);
            DateTime endUTC = TimeZoneInfo.ConvertTimeToUtc(nowLocal.Date.AddDays(1), this.configuration.ConfigurationProperties.TimeZoneInfo);
            return new DateRange(startUTC, endUTC);
        }

        private DateRange GetThisWeekRange()
        {
            DateTime nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.configuration.ConfigurationProperties.TimeZoneInfo);
            DateTime startLocal = nowLocal.AddDays((-1 * (int)nowLocal.DayOfWeek) + 1).Date;

            // Our week starts on Monday.
            if (nowLocal.DayOfWeek == DayOfWeek.Sunday)
            {
                startLocal = startLocal.AddDays(-7);
            }

            DateTime endLocal = startLocal.AddDays(6);

            DateTime startUTC = TimeZoneInfo.ConvertTimeToUtc(startLocal, this.configuration.ConfigurationProperties.TimeZoneInfo);
            DateTime endUTC = TimeZoneInfo.ConvertTimeToUtc(endLocal.AddDays(1), this.configuration.ConfigurationProperties.TimeZoneInfo);
            return new DateRange(startUTC, endUTC);
        }

        private class DateRange
        {
            public DateRange(DateTime startDateUTC, DateTime endDateUTC)
            {
                this.StartDateUTC = startDateUTC;
                this.EndDateUTC = endDateUTC;
            }

            public DateTime StartDateUTC { get; set; }

            public DateTime EndDateUTC { get; set; }
        }
    }
}
