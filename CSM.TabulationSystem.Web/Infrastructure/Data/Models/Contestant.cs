using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using CSM.TabulationSystem.Web.Infrastructure.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Infrastructure.Data.Models
{
    public class Contestant : BaseModel
    {
        public Guid? EventId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ContestantStatus Status { get; set; }
    }
}
