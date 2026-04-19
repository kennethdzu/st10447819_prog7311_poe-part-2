using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using TechMove.Glms.Web.Services;
using Xunit;

namespace TechMove.Glms.Tests;

public class CurrencyCalculationTests
{
    // Builds a LiveApiConversionStrategy with a mocked HTTP handler and a given JSON response.
    private static LiveApiConversionStrategy BuildStrategy(string jsonResponse)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonResponse)
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);

        // Build an in-memory configuration matching appsettings.json structure.
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExchangeRateApi:BaseUrl"] = "https://v6.exchangerate-api.com/v6/",
                ["ExchangeRateApi:ApiKey"]  = "test-key"
            })
            .Build();

        return new LiveApiConversionStrategy(httpClient, config);
    }

    [Fact]
    public async Task ConvertUsdToZarAsync_ValidMathCalculation_ReturnsCorrectZarAmount()
    {
        // Arrange
        var usdAmount = 100m;
        var expectedTotal = 1850m; // 100 * 18.50
        var strategy = BuildStrategy("{\"conversion_rates\":{\"ZAR\":18.50}}");

        // Act
        var result = await strategy.ConvertUsdToZarAsync(usdAmount);

        // Assert
        Assert.Equal(expectedTotal, result);
    }

    [Fact]
    public async Task ConvertUsdToZarAsync_ZeroUsd_ReturnsZeroZar()
    {
        // Arrange — edge case: 0 USD should produce 0 ZAR regardless of rate.
        var strategy = BuildStrategy("{\"conversion_rates\":{\"ZAR\":18.50}}");

        // Act
        var result = await strategy.ConvertUsdToZarAsync(0m);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task ConvertUsdToZarAsync_FractionalUsd_RoundsToTwoDecimalPlaces()
    {
        // Arrange — verifies Math.Round(x, 2) is applied correctly.
        // 1.99 * 18.50 = 36.815 → rounds to 36.82
        var strategy = BuildStrategy("{\"conversion_rates\":{\"ZAR\":18.50}}");

        // Act
        var result = await strategy.ConvertUsdToZarAsync(1.99m);

        // Assert
        Assert.Equal(36.82m, result);
    }

    [Fact]
    public async Task ConvertUsdToZarAsync_ApiDown_ThrowsMeaningfulException()
    {
        // Arrange — simulate the external API being unreachable.
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(handlerMock.Object);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExchangeRateApi:BaseUrl"] = "https://v6.exchangerate-api.com/v6/",
                ["ExchangeRateApi:ApiKey"]  = "test-key"
            })
            .Build();

        var strategy = new LiveApiConversionStrategy(httpClient, config);

        // Act & Assert — should throw with a user-friendly message, not a raw socket error.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => strategy.ConvertUsdToZarAsync(50m));

        Assert.Contains("temporarily unavailable", ex.Message);
    }
}
