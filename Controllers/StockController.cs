using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace PG_TechnicalAssessment.Controllers
{
    [ApiController]
    [Route("api/stock")]
    public class StockController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StockController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetStockData(string symbol)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // Yahoo Finance requires User Agent header
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?interval=15m&range=1mo";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to fetch data from Yahoo Finance");
                }
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize JSON into c# objects
                var yahooData = JsonSerializer.Deserialize<YahooFinanceResponse>(content);
                if (yahooData?.chart?.result == null || yahooData.chart.result.Count == 0)
                {
                    return NotFound("No data found for the specified symbol");
                }

                var timestamps = yahooData.chart.result[0].timestamp;
                var lows = yahooData.chart.result[0].indicators.quote[0].low;
                var highs = yahooData.chart.result[0].indicators.quote[0].high;
                var volumes = yahooData.chart.result[0].indicators.quote[0].volume;

                // Convert timestamps to dates
                var intervals = new List<StockInterval>();

                for (int i = 0; i < timestamps.Count; i++)
                {
                    var dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamps[i]).DateTime;
                    var dateString = dateTime.ToString("yyyy-MM-dd");
                    intervals.Add(new StockInterval
                    {
                        Date = dateString,
                        Low = lows[i],
                        High = highs[i],
                        Volume = volumes[i]
                    });
                }

                // Group intervals by date
                var groupedByDay = intervals
                    .GroupBy(interval => interval.Date)
                    .Select(group => new DailyStock
                    {
                        day = group.Key,
                        lowAverage = Math.Round(group.Average(x => x.Low), 4),
                        highAverage = Math.Round(group.Average(x => x.High), 4),
                        volume = group.Sum(x => x.Volume)
                    })
                    .ToList();

                return Ok(groupedByDay);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Unable to connect to Yahoo Finance API");
            }
            catch (JsonException)
            {
                return StatusCode(500, "Failed to parse response from Yahoo Finance");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
            
        }
    }

    public class YahooFinanceResponse
    {
        public required Chart chart { get; set; }
    }

    public class Chart
    {
        public required List<Result> result { get; set; }
    }

    public class Result
    {
        public required List<long> timestamp { get; set; }
        public required Indicators indicators { get; set; }
    }

    public class Indicators
    {
        public required List<Quote> quote { get; set; }
    }

    public class Quote
    {
        public required List<double> low { get; set; }
        public required List<double> high { get; set; }
        public required List<long> volume { get; set; }
    }

    public class StockInterval
    {
        public required string Date {get; set;}
        public double Low {get; set;}
        public double High {get; set;}
        public long Volume {get; set;}
    }
    
    public class DailyStock
    {
        public required string day {get; set;}
        public double lowAverage {get; set;}
        public double highAverage {get; set;}
        public long volume {get; set;}
    }
}