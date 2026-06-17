namespace SUP_Project_s32557.Api.Services;

public interface ICurrencyService
{
    decimal ConvertFromPln(decimal amount, string currency);
}

public class StaticCurrencyService : ICurrencyService
{
    private static readonly Dictionary<string, decimal> Rates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PLN"] = 1m,
        ["EUR"] = 4.30m,
        ["USD"] = 4.00m,
        ["GBP"] = 5.05m
    };

    public decimal ConvertFromPln(decimal amount, string currency)
    {
        if (!Rates.TryGetValue(currency, out var rate))
            throw new BusinessException("Unsupported currency. Allowed: PLN, EUR, USD, GBP.");
        return Math.Round(amount / rate, 2);
    }
}
