using NetBlaze.Domain.Common;

namespace NetBlaze.Domain.Entities
{
    public class CompanyPolicyHolidays:BaseEntity<int>
    {
        public DateTime HolidayDate { get; set; }

        public string? Description { get; set; }

        public string HolidayName { get; set; }

        public bool Mandatory { get; set; }
        
        public bool? IsRecurring { get; set; }

        public int DurationInDays { get; set; }

        public bool IsActive { get; set; }
    }
}
