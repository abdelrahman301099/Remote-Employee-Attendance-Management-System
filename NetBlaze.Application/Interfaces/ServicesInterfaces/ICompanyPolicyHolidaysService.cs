

using NetBlaze.SharedKernel.Dtos.CompanyPolicyHolidayDTO.Response;
using NetBlaze.SharedKernel.Dtos.CompanyPolicyHolidayDTO.Rquest;
using NetBlaze.SharedKernel.Dtos.General;
using NetBlaze.SharedKernel.HelperUtilities.General;

namespace NetBlaze.Application.Interfaces.ServicesInterfaces
{
    public interface ICompanyPolicyHolidaysService
    {
        Task<ApiResponse<int>> ImportHolidaysFromIcsAsync(string icsUrl, CancellationToken cancellationToken = default);

        Task<ApiResponse<object>> UpdateHolidayAsync(CompanyPolicyHolidayRequestDTO request, CancellationToken cancellationToken = default);

        Task<ApiResponse<PaginatedResponse<CompanyPolicyHolidayResponseDTO>>> GetHolidaysAsync(int pageNumber, int pageSize)
    }
}
