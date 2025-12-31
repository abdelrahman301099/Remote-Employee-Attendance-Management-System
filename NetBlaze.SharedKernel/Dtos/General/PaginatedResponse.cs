using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetBlaze.SharedKernel.Dtos.General
{
    public class PaginatedResponse<T>
    {
        
        
            public List<T> Items { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
       
    }
}
