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
            var client = _httpClientFactory.CreateClient();

            // Yahoo Finance requires User Agent header
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?interval=15m";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            // Deserialize JSON into c# objects
            var yahooData = JsonSerializer.Deserialize<YahooFinanceResponse>(content);

            var timestamps = yahooData.chart.result[0].timestamp;
            var lows = yahooData.chart.result[0].indicators.quote[0].low;
            var highs = yahooData.chart.result[0].indicators.quote[0].high;
            var volumes = yahooData.chart.result[0].indicators.quote[0].volume;
            
            // Confirms that the data was parsed
            return Ok(new { 
                message = "Parsed successfully", 
                dataPoints = timestamps.Count 
            });
        }
    }
    
    public class YahooFinanceResponse
    {
        public Chart chart {get; set;}
    }

    public class Chart
    {
        public List<Result> result {get; set;}
    }

    public class Result
    {
        public List<long> timestamp {get; set;}
        public Indicators indicators {get; set;}
    }

    public class Indicators
    {
        public List<Quote> quote {get; set;}
    }

    public class Quote
    {
        public List<double> low {get; set;}
        public List<double> high {get; set;}
        public List<long> volume {get; set;}
    }
}