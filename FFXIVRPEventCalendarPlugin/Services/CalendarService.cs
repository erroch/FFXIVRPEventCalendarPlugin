//-----------------------------------------------------------------------
// <copyright file="CalendarService.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Dalamud.Configuration;
    using Dalamud.Logging;

    using FFXIVRPCalendarPlugin.Models;
    using FFXIVRPCalendarPlugin.Models.Mock;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides access to varioud configuration functions and data.
    /// </summary>
    public static class CalendarService
    {
        private static readonly List<RPEvent> RPEvents = new ();
        private static string lastError = string.Empty;
        private static List<EventCategoryInfo>? eventCategories;
        private static List<ESRBRatingInfo>? ratings;
        private static DateTime cacheAge;

        /// <summary>
        /// Gets the last error encountered when trying to recieve information from the event calendar API.
        /// </summary>
        /// <returns>A string containing the last error enountered.</returns>
        public static string LastError()
        {
            return lastError;
        }

        /// <summary>
        /// Gets a list of Event Categories from the API indicated in the system configuration.
        /// </summary>
        /// <param name="configuration">The <see cref="Configuration"/> containing API information.</param>
        /// <returns>A list of <see cref="EventCategoryInfo"/> for filtering purposes.</returns>
        /// <exception cref="Exception">Will throw an exception if the httpClient call fails.</exception>
        public static async Task<List<EventCategoryInfo>> EventCategories(Configuration configuration)
        {
            if (eventCategories == null)
            {
                string url = configuration.ApiAddress.Replace('\0', ' ').Trim() + "/Calendar/Categories";
                try
                {
                    using (HttpClient httpClient = new ())
                    {
                        string response = await httpClient.GetStringAsync(url).ConfigureAwait(false);
                        List<EventCategoryInfo>? results = JsonConvert.DeserializeObject<List<EventCategoryInfo>>(response);
                        if (results == null)
                        {
                            eventCategories = new List<EventCategoryInfo>();
                        }
                        else
                        {
                            eventCategories = results;
                        }
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.LogError(ex, ex.Message);
                    throw new Exception($"URL: {url}: ex: {ex.Message}");
                }
            }

            return eventCategories;
        }

        /// <summary>
        /// Gets a list of ESRB ratings from the API indicated in the system configuration.
        /// </summary>
        /// <param name="configuration">The <see cref="Configuration"/> containing API information.</param>
        /// <returns>A list of <see cref="ESRBRatingInfo"/> for filtering purposes.</returns>
        /// <exception cref="Exception">Will throw an exception if the httpClient call fails.</exception>
        public static async Task<List<ESRBRatingInfo>> ESRBRatings(Configuration configuration)
        {
            if (ratings == null)
            {
                string url = configuration.ApiAddress.Replace('\0', ' ').Trim() + "/Calendar/Ratings";

                try
                {
                    using (HttpClient httpClient = new ())
                    {
                        string response = await httpClient.GetStringAsync(url).ConfigureAwait(false);
                        List<ESRBRatingInfo>? results = JsonConvert.DeserializeObject<List<ESRBRatingInfo>>(response);
                        if (results == null)
                        {
                            ratings = new List<ESRBRatingInfo>();
                        }
                        else
                        {
                            ratings = results;
                        }
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.LogError(ex, ex.Message);
                    throw new Exception($"URL: {url}: ex: {ex.Message}");
                }
            }

            return ratings;
        }

        /// <summary>
        /// Gets a listing of RP events for the current week with padding for filtering.
        /// </summary>
        /// <param name="configuration">The <see cref="Configuration"/> containing API information.</param>
        /// <returns>A list of <see cref="RPEvent"/>. for the current day.</returns>
        public static async Task<List<RPEvent>> GetToday(Configuration configuration)
        {
            await MaybeUpdateCache(configuration.ApiAddress).ConfigureAwait(false);
            return RPEvents;
        }

        private static async Task MaybeUpdateCache(string hostURL)
        {
            TimeSpan timeSpan = DateTime.UtcNow - cacheAge;
            if (timeSpan.TotalMinutes > 30)
            {
                await UpdateCache(hostURL).ConfigureAwait(false);
            }
        }

        private static async Task UpdateCache(string hostURL)
        {
            try
            {
                RPEvents.Clear();
                cacheAge = DateTime.UtcNow;

                using (HttpClient httpClient = new ())
                {
                    string response = await httpClient.GetStringAsync(hostURL.Replace('\0', ' ').Trim() + "/Events/GetWeekTranslatableEvents").ConfigureAwait(false);
                    List<RPEvent>? results = JsonConvert.DeserializeObject<List<RPEvent>>(response);
                    if (results != null)
                    {
                        RPEvents.AddRange(results.OrderBy(x => x.StartTimeUTC));
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }
        }
    }
}
