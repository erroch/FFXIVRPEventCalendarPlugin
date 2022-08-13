//-----------------------------------------------------------------------
// <copyright file="EnumeratorExtensions.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Enumerator Extensions.
    /// </summary>
    public static class EnumeratorExtensions
    {
        /// <summary>
        /// Gets the description of a given enumeration value.
        /// </summary>
        /// <typeparam name="T">The enumerator type.</typeparam>
        /// <param name="source">The source enumeration value.</param>
        /// <returns>The enumeration description.</returns>
        public static string GetDescription<T>(this T source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Source must not be null.");
            }

            if (!source.GetType().IsEnum)
            {
                throw new InvalidEnumArgumentException($"The type of {nameof(source)} is not enumerator.");
            }

            var description =
                source
                    .GetType()?
                    .GetMember(source.ToString() ?? string.Empty)
                    .FirstOrDefault(m => m.DeclaringType == source.GetType())
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description;

            if (description == null)
            {
                return source.ToString() ?? string.Empty;
            }

            return description;
        }
    }
}
