namespace PaymentGateway.Api.Utility
{
    /// <summary>
    /// Provides a collection of ISO 4217 currency codes.
    /// </summary>
    /// <remarks>This class contains a predefined set of ISO 4217 currency codes, such as "USD", "EUR", and
    /// "GBP". The codes are stored in a <see cref="HashSet{T}"/> to ensure uniqueness and allow efficient
    /// lookups.</remarks>
    public static class IsoCurrencyCodes
    {
        /// <summary>
        /// Represents a collection of supported currency codes.
        /// </summary>
        /// <remarks>The collection includes commonly used currency codes such as "USD", "EUR", and "GBP".
        /// This set is read-only and can be used to validate or reference supported currencies.</remarks>
        public static readonly HashSet<string> Codes = new(StringComparer.OrdinalIgnoreCase)
        {
            "USD", "EUR", "GBP", 
        };
    }
}
