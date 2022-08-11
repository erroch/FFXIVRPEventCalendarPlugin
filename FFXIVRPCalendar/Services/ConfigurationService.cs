namespace FFXIVRPCalendar.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Newtonsoft.Json;

    using FFXIVRPCalendar.Models;
    using FFXIVRPCalendar.Mock;


    public static class ConfigurationService
    {
        private static List<EventCategoryInfo> eventCategories;
        private static List<ESRBRatingInfo> ratings;

        public static async Task<List<EventCategoryInfo>> EventCategories(ConfigurationProperties configuration)
        {
            if (eventCategories == null)
            {
                string url = configuration.ApiAddress.Replace('\0', ' ').Trim() + "/Calendar/Categories";
                try
                {
                    using (HttpClient httpClient = new())
                    {
                        string response = await httpClient.GetStringAsync(url).ConfigureAwait(false);
                        List<EventCategoryInfo> results = JsonConvert.DeserializeObject<List<EventCategoryInfo>>(response);
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
                    throw new Exception($"URL: {url}: ex: {ex.Message}");
                }
            }

            return eventCategories;
        }

        public static async Task<List<ESRBRatingInfo>> ESRBRatings(ConfigurationProperties configuration)
        {
            if (ratings == null)
            {
                string url = configuration.ApiAddress.Replace('\0', ' ').Trim() + "/Calendar/Ratings";

                try
                {
                    using (HttpClient httpClient = new())
                    {
                        string response = await httpClient.GetStringAsync(url).ConfigureAwait(false);
                        List<ESRBRatingInfo> results = JsonConvert.DeserializeObject<List<ESRBRatingInfo>>(response);
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
                    throw new Exception($"URL: {url}: ex: {ex.Message}");
                }

            }

            return ratings;
        }
    }
}
