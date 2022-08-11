namespace FFXIVRPCalendar.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class ConfigurationProperties
    {
        public string ApiAddress { get; set; } = "https://api.ffxiv-rp.org/api";

        public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;

        public List<string> DataCenters { get; set; } = new();

        public List<string> Ratings { get; set; } = new List<string>(new string[] { "Teen" });

        public List<string> Categories { get; set; } = null;

        public bool Initalized { get; set; } = false;

        public EventDisplayRange EventDisplayRange { get; set; } = EventDisplayRange.Everywhere;
    }
}
