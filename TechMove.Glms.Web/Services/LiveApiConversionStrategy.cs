using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TechMove.Glms.Web.Services
{
    public class LiveApiConversionStrategy : ICurrencyConversionStrategy
    {
        HttpClient httpClient;
        string apiUrl;

        public LiveApiConversionStrategy(HttpClient client, IConfiguration configuration)
        {
            httpClient = client;

            string baseUrl = configuration["ExchangeRateApi:BaseUrl"];
            if (baseUrl == null)
            {
                throw new InvalidOperationException("ExchangeRateApi:BaseUrl is not configured.");
            }

            string apiKey = configuration["ExchangeRateApi:ApiKey"];
            if (apiKey == null)
            {
                throw new InvalidOperationException("ExchangeRateApi:ApiKey is not configured.");
            }

            apiUrl = baseUrl.TrimEnd('/') + "/" + apiKey + "/latest/USD";
        }

        public async Task<decimal> ConvertUsdToZarAsync(decimal usdAmount)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                
                using (JsonDocument document = JsonDocument.Parse(content))
                {
                    JsonElement rates = document.RootElement.GetProperty("conversion_rates");
                    
                    if (rates.TryGetProperty("ZAR", out JsonElement zarRateElement))
                    {
                        decimal zarRate = zarRateElement.GetDecimal();
                        return Math.Round(usdAmount * zarRate, 2);
                    }
                }

                throw new InvalidOperationException("ZAR exchange rate was not present in the API response.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Currency conversion is temporarily unavailable. Please try again shortly.", ex);
            }
        }
    }
}
