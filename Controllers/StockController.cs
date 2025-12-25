using Microsoft.AspNetCore.Mvc;

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
            var content = response.Content.ReadAsStringAsync();
            
            return Ok(content);
        }
    }
}