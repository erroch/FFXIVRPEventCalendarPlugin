namespace FFXIVRPCalendarPlugin
{
    using System.ComponentModel;

    /// <summary>
    /// Represents a World DC DataCenter
    /// </summary>
    public enum WorldDCRegion : byte
    {
        [Description("Non-Public Data Center")]
        NonPublic = 0,
        [Description("Japanese Date Center")]
        Japanese = 1,
        [Description("North American Data Center")]
        NorthAmerica = 2,
        [Description("European Data Center")]
        European = 3,
        [Description("Oceanian Data Center")]
        Oceanian = 4
    }
}
