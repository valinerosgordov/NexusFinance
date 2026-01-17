using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FinancialPlanner.Models;
using Newtonsoft.Json;

namespace FinancialPlanner.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, decimal> _cachedRates = new();
        private DateTime _lastUpdate = DateTime.MinValue;
        private const int CacheHours = 1;

        public CurrencyService()
        {
            _httpClient = new HttpClient();
        }

        public List<Currency> GetAvailableCurrencies()
        {
            return new List<Currency>
            {
                new Currency { Code = "RUB", Name = "Российский рубль", Symbol = "₽" },
                new Currency { Code = "USD", Name = "Доллар США", Symbol = "$" },
                new Currency { Code = "EUR", Name = "Евро", Symbol = "€" },
                new Currency { Code = "GBP", Name = "Фунт стерлингов", Symbol = "£" },
                new Currency { Code = "JPY", Name = "Японская йена", Symbol = "¥" },
                new Currency { Code = "CNY", Name = "Китайский юань", Symbol = "¥" },
                new Currency { Code = "KRW", Name = "Южнокорейская вона", Symbol = "₩" }
            };
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency) return 1m;

            var cacheKey = $"{fromCurrency}_{toCurrency}";
            
            // Проверка кэша
            if (_cachedRates.ContainsKey(cacheKey) && 
                DateTime.Now - _lastUpdate < TimeSpan.FromHours(CacheHours))
            {
                return _cachedRates[cacheKey];
            }

            try
            {
                // Используем бесплатный API exchangerate-api.com
                var response = await _httpClient.GetStringAsync(
                    $"https://api.exchangerate-api.com/v4/latest/{fromCurrency}");
                
                var data = JsonConvert.DeserializeObject<ExchangeRateResponse>(response);
                
                if (data?.Rates != null && data.Rates.ContainsKey(toCurrency))
                {
                    var rate = data.Rates[toCurrency];
                    _cachedRates[cacheKey] = rate;
                    _lastUpdate = DateTime.Now;
                    return rate;
                }
            }
            catch
            {
                // Fallback на статические курсы при ошибке API
                return GetFallbackRate(fromCurrency, toCurrency);
            }

            return GetFallbackRate(fromCurrency, toCurrency);
        }

        private decimal GetFallbackRate(string fromCurrency, string toCurrency)
        {
            // Базовые курсы (примерные, для оффлайн работы)
            var baseRates = new Dictionary<string, decimal>
            {
                { "USD_RUB", 90m },
                { "EUR_RUB", 98m },
                { "GBP_RUB", 115m },
                { "JPY_RUB", 0.6m },
                { "CNY_RUB", 12.5m },
                { "KRW_RUB", 0.07m }
            };

            var key = $"{fromCurrency}_{toCurrency}";
            if (baseRates.ContainsKey(key))
                return baseRates[key];

            var reverseKey = $"{toCurrency}_{fromCurrency}";
            if (baseRates.ContainsKey(reverseKey))
                return 1m / baseRates[reverseKey];

            return 1m;
        }

        public decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency) return amount;
            
            var rate = GetExchangeRateAsync(fromCurrency, toCurrency).Result;
            return amount * rate;
        }

        private class ExchangeRateResponse
        {
            [JsonProperty("rates")]
            public Dictionary<string, decimal>? Rates { get; set; }
        }
    }
}
