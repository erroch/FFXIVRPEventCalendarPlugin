//-----------------------------------------------------------------------
// <copyright file="ESRBRatingInfo.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendar.Models
{
    /// <summary>
    /// Represents an ESRBRatingInfo.
    /// </summary>
    public class ESRBRatingInfo
    {
        /// <summary>
        /// Gets or sets the rating name.
        /// </summary>
        public string RatingName { get; set; }

        /// <summary>
        /// Gets or sets the rating description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the string prefix used when listing events of mixed ratings.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this event requires the user to be age verified.
        /// </summary>
        public bool RequiresAgeValidation { get; set; }
    }
}