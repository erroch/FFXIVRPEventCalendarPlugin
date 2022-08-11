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

    public static class EventService
    {
        public const string API_PATH = "/Events/GetWeekTranslatableEvents";
        private static DateTime cacheAge;
        private readonly static List<RPEvent> rpEvents = new();
        public static string LastError = string.Empty;

        public static async Task<List<RPEvent>> GetToday(ConfigurationProperties configuration)
        {
            await MaybeUpdateCache(configuration.ApiAddress).ConfigureAwait(false);
            return rpEvents.Where(x => x.StartTimeUTC.Date >= DateTime.UtcNow.Date && x.StartTimeUTC.Date <= DateTime.UtcNow.Date.AddDays(1)).ToList();
        }

        private static async Task MaybeUpdateCache(string hostURL)
        {
            TimeSpan timeSpan = DateTime.UtcNow - cacheAge;
            if (timeSpan.TotalMinutes > 30)
            {
                await UpdateCache(hostURL).ConfigureAwait(false);
            }
        }

        private static  async Task UpdateCache(string hostURL)
        {
            try
            {
                rpEvents.Clear();
                cacheAge = DateTime.UtcNow;

                using (HttpClient httpClient = new())
                {
                    string response = await httpClient.GetStringAsync(hostURL.Replace('\0', ' ').Trim() + API_PATH).ConfigureAwait(false);
                    List<RPEvent> results = JsonConvert.DeserializeObject<List<RPEvent>>(response);
                    rpEvents.AddRange(results.OrderBy(x => x.StartTimeUTC));
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
        }
    }
}
