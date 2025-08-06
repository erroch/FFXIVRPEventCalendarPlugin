//-----------------------------------------------------------------------
// <copyright file="CalendarMockData.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Models.Mock
{
    /// <summary>
    /// Mock data for the FFXIV RP Event Calendar to simulate API results.
    /// </summary>
    public static class CalendarMockData
    {
        /// <summary>
        /// Get string containig test event information.
        /// </summary>
        /// <returns>A JSON formatted string contianing information about two roleplay events.</returns>
        public static string GetEventInfoMock()
        {
            return @"[
  {
    ""datacenter"": ""Crystal"",
    ""server"": ""Balmung"",
    ""serverId"": 91,
    ""eventName"": ""Phoenix Lounge Weekly Open Night"",
    ""location"": ""Mist Ward 19, Plot 2"",
    ""eventURL"": ""https://xiv.fyi/oZYi"",
    ""esrbRating"": ""Teen"",
    ""description"": ""https://xiv.fyi/oZYi\nMist Ward 19, Plot 2\nRating: Teen\n\nWeekly EU bar open night of the Phoenix Lounge adventuring bar.\n\nCategory: Bar/Tavern\nContacts: Akane Sakai"",
    ""eventCategory"": ""Bar/Tavern"",
    ""contacts"": ""Akane Sakai"",
    ""startTimeUTC"": ""2021-10-10T19:00:00Z"",
    ""endTimeUTC"": ""2021-10-10T22:00:00Z"",
    ""isRecurring"": true,
    ""lastValidated"": null
  },
  {
    ""datacenter"": ""Crystal"",
    ""server"": ""Balmung"",
    ""serverId"": 91,
    ""eventName"": ""Voss Café"",
    ""location"": ""Shirogane Ward 7, Plot 54"",
    ""eventURL"": ""https://xiv.fyi/GvvR"",
    ""esrbRating"": ""Teen"",
    ""description"": ""https://xiv.fyi/GvvR\nShirogane Ward 7, Plot 54\nRating: Teen\n\nA coffee shop with an upstairs café where anyone is welcome to relax, read, and socialize - well, until you get kicked out, that is.\n  \nLord and Lady Voss love their coffee, so why wouldn't they think it a great idea to open a coffee shop and café? With all sorts of flavors to offer and a warm, cozy atmosphere, this coffee shop offers a place for anyone to sit and relax. The café, located above the infamous Requiem Bar, offers two floors of seating, a small stage, a small library, cozy fires, and plenty of sitting space for you and your friends.\n\nKeep an eye out for special events such as poetry night, open mic night, and special one-time only events.\n\nVoss Coffee and Café is also the proud home of Mimic Muffins - Made with fresh Pixieberries, you'll never know which flavor you'll get!\n\n\""Voss Coffee and Café has allergy warnings. We use our equipment to make things with nuts, milk, and sometimes even poison, so cross-contamination is possible. Voss Coffee and Café and Voss Enterprises will not be held accountable for allergic reactions, death, the growth of extra limbs, or erectile dysfunctions. But worry not! All products are tested and tried by our team of Lalafel!\""\n\nCategory: Eatery\nContacts: Frederick Voss, Rena Jesal, Elise Voss, Linedari#1103"",
    ""eventCategory"": ""Eatery"",
    ""contacts"": ""Frederick Voss, Rena Jesal, Elise Voss, Linedari#1103"",
    ""startTimeUTC"": ""2021-10-10T21:00:00Z"",
    ""endTimeUTC"": ""2021-10-11T00:00:00Z"",
    ""isRecurring"": true,
    ""lastValidated"": null
  }
]";
        }
    }
}
