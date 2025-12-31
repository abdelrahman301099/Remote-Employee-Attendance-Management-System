using Microsoft.AspNetCore.Mvc;
using NetBlaze.Application.Interfaces.ServicesInterfaces;
using NetBlaze.SharedKernel.Dtos.CompanyPolicyHolidayDTO.Response;
using NetBlaze.SharedKernel.Dtos.CompanyPolicyHolidayDTO.Rquest;
using NetBlaze.SharedKernel.Dtos.General;
using NetBlaze.SharedKernel.HelperUtilities.General;

namespace NetBlaze.Api.Controllers
{
    [ApiController]
    [Route("api/company-policy/holidays")]
    public class CompanyPolicyHolidaysController : ControllerBase, ICompanyPolicyHolidaysService
    {
        private readonly ICompanyPolicyHolidaysService _service;

        public CompanyPolicyHolidaysController(
            ICompanyPolicyHolidaysService service)
        {
            _service = service;
        }

       
        [HttpGet("getHolidays")]
        public async Task<ApiResponse<PaginatedResponse<CompanyPolicyHolidayResponseDTO>>> GetHolidaysAsync([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10)
        {
            return await _service.GetHolidaysAsync(pageNumber, pageSize);
        }

        
        [HttpPost("importHolidays")]
        public async Task<ApiResponse<int>> ImportHolidaysFromIcsAsync( [FromQuery] string icsUrl,CancellationToken cancellationToken = default)
        {
            return await _service.ImportHolidaysFromIcsAsync(icsUrl, cancellationToken);
        }

        
        [HttpPut("updateHoliday")]
        public async Task<ApiResponse<object>> UpdateHolidayAsync( [FromBody] CompanyPolicyHolidayRequestDTO request, CancellationToken cancellationToken = default)
        {
            
            return await _service.UpdateHolidayAsync(request, cancellationToken);
        }
    }
}
