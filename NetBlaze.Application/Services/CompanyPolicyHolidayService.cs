
using Microsoft.EntityFrameworkCore;
using NetBlaze.Application.Interfaces.General;
using NetBlaze.Application.Interfaces.ServicesInterfaces;
using NetBlaze.Application.Mappings;
using NetBlaze.Domain.Entities;
using NetBlaze.SharedKernel.Dtos.CompanyPolicyHolidayDTO.Response;
using NetBlaze.SharedKernel.Dtos.CompanyPolicyHolidayDTO.Rquest;
using NetBlaze.SharedKernel.Dtos.General;
using NetBlaze.SharedKernel.HelperUtilities.General;
using NetBlaze.SharedKernel.SharedResources;
using System.Net;
using System.Threading;

namespace NetBlaze.Application.Services
{
    public class CompanyPolicyHolidayService : ICompanyPolicyHolidaysService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;

        public CompanyPolicyHolidayService(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
        }



        public async Task<ApiResponse<PaginatedResponse<CompanyPolicyHolidayResponseDTO>>> GetHolidaysAsync(int pageNumber, int pageSize)
        {
            var query = _unitOfWork.Repository
                .GetQueryable<CompanyPolicyHolidays>().Where(h => h.IsActive);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(h => h.HolidayDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new CompanyPolicyHolidayResponseDTO
                {
                    Name = v.HolidayName,
                    Date = v.HolidayDate,
                    Duration = v.DurationInDays,
                    Description = v.Description
                })
                .ToListAsync();

            var result = new PaginatedResponse<CompanyPolicyHolidayResponseDTO>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return ApiResponse<PaginatedResponse<CompanyPolicyHolidayResponseDTO>>
                .ReturnSuccessResponse(
                    result,
                    "Returned Successfully",
                    HttpStatusCode.OK);
        }

        public async Task<ApiResponse<int>> ImportHolidaysFromIcsAsync(string icsUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(icsUrl))
            {
                return ApiResponse<int>.ReturnFailureResponse("InvalidRequest", HttpStatusCode.BadRequest); //TODO: Add message to resources
            }


            var client = _httpClientFactory.CreateClient();
            using var response = await client.GetAsync(icsUrl, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return ApiResponse<int>.ReturnFailureResponse(Messages.ErrorOccurredInServer, HttpStatusCode.BadGateway);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            var events = ParseIcs(content);

            var added = 0;

            foreach (var e in events)
            {
                var exists = await _unitOfWork.Repository.AnyAsync<CompanyPolicyHolidays>(
                    x => x.HolidayDate.Date == e.start.Date && x.HolidayName == e.summary,
                    cancellationToken).ConfigureAwait(false);

                if (exists)
                {
                    continue;
                }

                var entity = new CompanyPolicyHolidays
                {
                    HolidayName = e.summary,
                    HolidayDate = e.start.Date,
                    DurationInDays = Math.Max(1, (int)Math.Ceiling((e.end.Date - e.start.Date).TotalDays)),
                    Description = e.summary
                };

                await _unitOfWork.Repository.AddAsync<CompanyPolicyHolidays, int>(entity, cancellationToken).ConfigureAwait(false);
                added++;
            }

            if (added == 0)
            {
                return ApiResponse<int>.ReturnSuccessResponse(0, Messages.SampleNotModified);
            }

            var rows = await _unitOfWork.Repository.CompleteAsync(cancellationToken).ConfigureAwait(false);

            return ApiResponse<int>.ReturnSuccessResponse(rows, Messages.SampleAddedSuccessfully);
        }

        public async Task<ApiResponse<object>> UpdateHolidayAsync( CompanyPolicyHolidayRequestDTO request, CancellationToken cancellationToken = default)
        {
           
            var current = await _unitOfWork.Repository
                .GetSingleAsync<CompanyPolicyHolidays>(
                     false,
                     x => x.Id == request.Id && x.IsActive,
                    cancellationToken);

            if (current == null)
            {
                return ApiResponse<object>
                    .ReturnFailureResponse("NotFound", HttpStatusCode.NotFound);
            }

           
            current.IsActive = false;

            
            var newEntity = new CompanyPolicyHolidays
            {
                HolidayName = request.Name,
                HolidayDate = request.Date,
                DurationInDays = request.Duration,
                Description = request.Description,
                IsActive = true
            };

            await _unitOfWork.Repository.AddAsync(newEntity, cancellationToken);

          
            var rows = await _unitOfWork.Repository
                .CompleteAsync(cancellationToken)
                .ConfigureAwait(false);

            if (rows > 0)
            {
                return ApiResponse<object>
                    .ReturnSuccessResponse(null, "UpdatedSuccessfully");
            }

            return ApiResponse<object>
                .ReturnFailureResponse("UpdateFailed", HttpStatusCode.InternalServerError);
        }



        #region Helper

        private static List<(string summary, DateTime start, DateTime end)> ParseIcs(string ics)
        {
            var list = new List<(string summary, DateTime start, DateTime end)>();
            string? summary = null;
            DateTime? dtStart = null;
            DateTime? dtEnd = null;

            using var reader = new StringReader(ics);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("BEGIN:VEVENT", StringComparison.OrdinalIgnoreCase))
                {
                    summary = null;
                    dtStart = null;
                    dtEnd = null;
                    continue;
                }

                if (line.StartsWith("END:VEVENT", StringComparison.OrdinalIgnoreCase))
                {
                    if (summary != null && dtStart.HasValue)
                    {
                        var start = dtStart.Value;
                        var end = dtEnd ?? start.AddDays(1);
                        list.Add((summary, start, end));
                    }
                    summary = null;
                    dtStart = null;
                    dtEnd = null;
                    continue;
                }

                if (line.StartsWith("SUMMARY:", StringComparison.OrdinalIgnoreCase))
                {
                    summary = line.Substring("SUMMARY:".Length).Trim();
                    continue;
                }

                if (line.StartsWith("DTSTART", StringComparison.OrdinalIgnoreCase))
                {
                    var value = ExtractDateValue(line);
                    dtStart = value;
                    continue;
                }

                if (line.StartsWith("DTEND", StringComparison.OrdinalIgnoreCase))
                {
                    var value = ExtractDateValue(line);
                    dtEnd = value;
                    continue;
                }
            }

            return list;
        }

        private static DateTime ExtractDateValue(string line)
        {
            var idx = line.IndexOf(":", StringComparison.Ordinal);
            var raw = idx >= 0 ? line[(idx + 1)..].Trim() : string.Empty;

            if (DateTime.TryParseExact(raw, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var dateOnly))
            {
                return dateOnly;
            }

            if (DateTime.TryParseExact(raw, "yyyyMMdd'T'HHmmss'Z'", null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var dateTimeUtc))
            {
                return dateTimeUtc;
            }

            if (DateTime.TryParse(raw, out var general))
            {
                return general;
            }

            return DateTime.UtcNow.Date;
        }


        public async Task<ApiResponse<bool>> CheckIfTodayIsHolidayAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.UtcNow.Date;

            var any = await _unitOfWork.Repository
                .AnyAsync<CompanyPolicyHolidays>(
                x => x.HolidayDate.Date <= today &&
                today < x.HolidayDate.AddDays(x.DurationInDays).Date,
                cancellationToken
            );

            return ApiResponse<bool>.ReturnSuccessResponse(any);
        }
        #endregion
    }
}
