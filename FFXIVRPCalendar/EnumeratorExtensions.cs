namespace FFXIVRPCalendar
{
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
            if (!source.GetType().IsEnum)
            {
                throw new InvalidEnumArgumentException($"The type of {nameof(source)} is not enumerator.");
            }

            var description =
                source
                    .GetType()
                    .GetMember(source.ToString())
                    .FirstOrDefault(m => m.DeclaringType == source.GetType())
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description;

            if (description == null)
            {
                return source.ToString();
            }

            return description;
        }
    }
}
