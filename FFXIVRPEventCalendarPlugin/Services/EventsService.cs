﻿//-----------------------------------------------------------------------
// <copyright file="EventsService.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using FFXIVRPCalendarPlugin;
    using FFXIVRPCalendarPlugin.Models;
    using Lumina.Excel.Sheets;

    /// <summary>
    /// A service handing event list building and filtering.
    /// </summary>
    public class EventsService : IDisposable
    {
        private const uint RefreshInterval = 15 * 60;

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

        /// <summary>
        /// Gets a list of all roleplay events filtered by date range and filter options.
        /// </summary>
        public List<RPEvent>? FilteredEvents { get; internal set; }

        private List<RPEvent>? RoleplayEvents { get; set; }

        /// <summary>
        /// Refresh the event list.
        /// </summary>
        /// <param name="forceRefresh">A flag indicating if the cache values should be ignored and a refresh done even if within the normal timeout interfval.</param>
        public void RefreshEvents(bool forceRefresh = false)
        {
            if (this.lastRefresh.HasValue)
            {
                TimeSpan sinceLastRefresh = DateTime.UtcNow - this.lastRefresh.Value;
                if (sinceLastRefresh.TotalSeconds >= RefreshInterval)
                {
                    forceRefresh = true;
                }
            }

            if (!this.eventsLoaded || forceRefresh)
            {
                this.eventsLoaded = true;
                this.lastRefresh = DateTime.UtcNow;
                this.LastRefreshLocalTime = TimeZoneInfo.ConvertTimeFromUtc(this.lastRefresh.Value, this.configuration.TimeZoneInfo);

                try
                {
                    Task<List<RPEvent>?>.Run(async () => await CalendarService.GetEvents(this.configuration)
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
                    Plugin.PluginLog.Error(ex, ex.Message);
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
                DateRange dateRange = this.GetDatesForTimeframe(this.configuration.EventTimeframe);

                // TODO: Re-add show real gil filter when the API actually supports it.
                // (this.configuration.ShowRealGilEvents || !x.IsRealGilEvent) &&
                this.FilteredEvents = this.RoleplayEvents
                    .Where(x =>
                        (this.configuration.Categories is null || this.configuration.Categories.Contains(x.EventCategory)) &&
                        (this.configuration.Ratings is null || this.configuration.Ratings.Contains(x.ESRBRating)) &&
                        (!this.configuration.OneTimeEventsOnly || !x.IsRecurring) &&
                        (
                            (this.configuration.EventTimeframe == EventTimeframe.Now && x.StartTimeUTC <= DateTime.UtcNow && x.EndTimeUTC >= DateTime.UtcNow) ||
                            (x.StartTimeUTC >= dateRange.StartDateUTC && x.StartTimeUTC <= dateRange.EndDateUTC)))
                    .Select(x =>
                    {
                        RPEvent result = x;

                        // TODO: Re-add show real gil filter when the API actually supports it.
                        // if (x.IsRealGilEvent)
                        // {
                        //     if (x.EventName != null && x.EventName.Contains("[$]") == false)
                        //     {
                        //         x.EventName = "[$] " + x.EventName;
                        //     }
                        // }
                        result.LocalStartTime = TimeZoneInfo.ConvertTimeFromUtc(result.StartTimeUTC, this.configuration.TimeZoneInfo);
                        result.LocalEndTime = TimeZoneInfo.ConvertTimeFromUtc(result.EndTimeUTC, this.configuration.TimeZoneInfo);
                        return x;
                    })
                .ToList();

                World? gameWorld = Plugin.ClientState?.LocalPlayer?.CurrentWorld.ValueNullable;
                if (gameWorld == null)
                {
                    this.ServerEvents = null;
                    this.DatacenterEvents = null;
                    this.RegionEvents = null;
                }
                else
                {
                    this.ServerEvents = this.FilteredEvents?
                        .Where(x => x.ServerId == gameWorld.Value.RowId)
                        .ToList();

                    uint[] worldIds = WorldService.GetDatacenterWorldIds(gameWorld.Value.DataCenter.Value.RowId);

                    this.DatacenterEvents = this.FilteredEvents?
                        .Where(x => worldIds.Contains(x.ServerId))
                        .ToList();

                    WorldDCGroupType datacenter = gameWorld.Value.DataCenter.Value; // WorldService.Datacenters?[];

                    uint[] regionWorldIds = WorldService.GetRegionWorldIds(datacenter.Region);
                    this.RegionEvents = this.FilteredEvents?
                        .Where(x => regionWorldIds.Contains(x.ServerId))
                        .ToList();
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
                    // TODO: unregister events, etc. here.
                }

                this.disposedValue = true;
            }
        }

        private bool CheckForServerChange()
        {
            if (Plugin.ClientState.LocalPlayer?.CurrentWorld.RowId != this.lastServerId)
            {
                this.lastServerId = Plugin.ClientState.LocalPlayer?.CurrentWorld.RowId;
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
                EventTimeframe.NextWeek => this.GetNextWeekRange(),
                _ => this.GetTodayRange(),
            };
        }

        private DateRange GetTodayRange()
        {
            DateTime nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.configuration.TimeZoneInfo);
            DateTime startUTC = TimeZoneInfo.ConvertTimeToUtc(nowLocal.Date, this.configuration.TimeZoneInfo);
            DateTime endUTC = TimeZoneInfo.ConvertTimeToUtc(nowLocal.Date.AddDays(1), this.configuration.TimeZoneInfo);
            return new DateRange(startUTC, endUTC);
        }

        private DateRange GetThisWeekRange()
        {
            DateTime nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.configuration.TimeZoneInfo);
            DateTime startLocal = nowLocal.AddDays((-1 * (int)nowLocal.DayOfWeek) + 1).Date;

            // Our week starts on Monday.
            if (nowLocal.DayOfWeek == DayOfWeek.Sunday)
            {
                startLocal = startLocal.AddDays(-7);
            }

            DateTime endLocal = startLocal.AddDays(6);

            DateTime startUTC = TimeZoneInfo.ConvertTimeToUtc(startLocal, this.configuration.TimeZoneInfo);
            DateTime endUTC = TimeZoneInfo.ConvertTimeToUtc(endLocal.AddDays(1), this.configuration.TimeZoneInfo);
            return new DateRange(startUTC, endUTC);
        }

        private DateRange GetNextWeekRange()
        {
            DateTime nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.configuration.TimeZoneInfo).AddDays(7);
            DateTime startLocal = nowLocal.AddDays((-1 * (int)nowLocal.DayOfWeek) + 1).Date;

            // Our week starts on Monday.
            if (nowLocal.DayOfWeek == DayOfWeek.Sunday)
            {
                startLocal = startLocal.AddDays(-7);
            }

            DateTime endLocal = startLocal.AddDays(6);

            DateTime startUTC = TimeZoneInfo.ConvertTimeToUtc(startLocal, this.configuration.TimeZoneInfo);
            DateTime endUTC = TimeZoneInfo.ConvertTimeToUtc(endLocal.AddDays(1), this.configuration.TimeZoneInfo);
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
