using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.Services;

namespace VendorHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Vendor,Admin")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("vendor/{vendorId}")]
        public async Task<IActionResult> GetVendorStatistics(int vendorId)
        {
            var result = await _statisticsService.GetVendorStatisticsAsync(vendorId);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
