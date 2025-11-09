namespace PaymentGateway.Api.Validation
{
    public static class CardLast4Extractor
    {
        /// <summary>
        /// Extracts the last four digits from a card number string.
        /// </summary>
        /// <param name="cardNum">The card number</param>
        /// <returns>The last 4 characters of the card number</returns>
        /// <exception cref="ArgumentException"></exception>
        public static string Extract(string cardNum)
        {
            if (string.IsNullOrWhiteSpace(cardNum)) throw new ArgumentException("Card number required.", nameof(cardNum));
            var digits = new string(cardNum.Where(char.IsDigit).ToArray());
            if (digits.Length < 4) throw new ArgumentException("Card number has fewer than 4 digits.", nameof(cardNum));
            return digits[^4..];
        }
    }
}
