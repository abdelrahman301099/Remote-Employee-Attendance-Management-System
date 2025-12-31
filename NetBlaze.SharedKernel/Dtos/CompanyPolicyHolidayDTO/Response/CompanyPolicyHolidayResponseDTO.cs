
namespace NetBlaze.SharedKernel.Dtos.CompanyPolicyHolidayDTO.Response
{
    public class CompanyPolicyHolidayResponseDTO
    {
        public string Name { get; set; }

        public DateTime Date { get; set; }

        public string? Description { get; set; }

        public int Duration { get; set; }

        public bool IsActive { get; set; }
    }
}
