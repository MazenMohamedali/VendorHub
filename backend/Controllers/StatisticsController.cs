using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorHub.DTOs.sharedDto;
using VendorHub.DTOs.StatisticsDto;
using VendorHub.Extensions;
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

        [HttpGet("vendor/{vendorId:int}")]
        [Authorize(Roles = "Vendor,Admin")]
        public async Task<ActionResult<GeneralResponse<VendorStatisticsDto>>> GetVendorStatistics(int vendorId, CancellationToken cancellationToken)
        {
            if (!User.IsInRole("Admin") && this.GetUserId() != vendorId)
                return Forbid();

            var result = await _statisticsService.GetVendorStatisticsAsync(vendorId, cancellationToken);
            return this.HandleResult(result);
        }
    }
}
