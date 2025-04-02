using WasherService.Data;
using WasherService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Nodes;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;



namespace WasherService.Controllers
{
    [Route("api")]
    [ApiController]
    public class OtherController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public OtherController(ApplicationDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient(); // ✅ Use HttpClientFactory
        }

        [HttpGet("order/list")]
        public async Task<IActionResult> GetWasherOrders()
        {
            // return Ok("ok");
            // Extract User ID from JWT Token
            string? userId = null;
             Console.WriteLine("ok2");
            if (HttpContext.Items["User"] is JwtSecurityToken jwtToken)
            {
                Console.WriteLine("ok");

                userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            }

            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Unauthorized: Invalid or missing token..." });
            }

            // Fetch orders from Order Service
            string orderServiceUrl = $"http://localhost:5000/order/api/order/washer/list/{userId}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(orderServiceUrl);
                response.EnsureSuccessStatusCode(); // Throws an exception if not successful

                string responseData = await response.Content.ReadAsStringAsync();
                JsonNode? json = JsonNode.Parse(responseData);

                return Ok(json);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { message = "Failed to fetch customer orders", error = ex.Message });
            }
        }


        [HttpGet("order/details/{order_id}")]
        public async Task<IActionResult> GetOrderDetails(string order_id)
        {
            // Extract User ID from JWT Token
            string? userId = null;
            if (HttpContext.Items["User"] is JwtSecurityToken jwtToken)
            {
                userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            }

            if (string.IsNullOrEmpty(order_id))
            {
                return Unauthorized(new { message = "Unauthorized: Invalid or missing token." });
            }

            // Fetch orders from Order Service
            string orderServiceUrl = $"http://localhost:5000/order/api/order/orderId/{order_id}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(orderServiceUrl);
                response.EnsureSuccessStatusCode(); // Throws exception if not successful

                string responseData = await response.Content.ReadAsStringAsync();
                // var orders = JsonSerializer.Deserialize<IEnumerable<OrderStr>>(responseData, new JsonSerializerOptions
                // {
                //     PropertyNameCaseInsensitive = true
                // });
           
// string responseData = await response.Content.ReadAsStringAsync();
            JsonNode? json = JsonNode.Parse(responseData);
            Console.WriteLine(json);
                return Ok(json);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { message = "Failed to fetch order details", error = ex.Message });
            }
        }


 [HttpPost("update/workStatus/{id}")]
public async Task<IActionResult> WorkAcceptAction(int id, WasherUpdateStcr data)
{
     string jsonDatas = JsonSerializer.Serialize(data);
     Console.WriteLine(jsonDatas);
    string orderServiceUrl = $"http://localhost:5000/order/api/update/workStatus/{id}";
// return Ok("ok");
    try
    {
        // Serialize data
        string jsonData = JsonSerializer.Serialize(data);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        // Send HTTP POST with data
        HttpResponseMessage response = await _httpClient.PostAsync(orderServiceUrl, content);
        response.EnsureSuccessStatusCode(); // Throws if status code is not 2xx

        // Read response
        string responseData = await response.Content.ReadAsStringAsync();
        JsonNode? json = JsonNode.Parse(responseData);

        return Ok(json);
    }
    catch (HttpRequestException ex)
    {
        return StatusCode(500, new { message = "Failed to fetch customer orders", error = ex.Message });
    }
}
    }
}



 public class WasherUpdateStcr
    {
        public bool isWasherAccepted { get; set; }
    }