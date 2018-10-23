using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Infrastructure.Data.Models
{
    public class Criterion : BaseModel
    {
        public Guid? EventId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Percentage { get; set; }

    }
}
