using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Infrastructure.Data.Models
{
    public class EventResult: BaseModel
    {
        public Guid? EventId { get; set; }

        public Guid? ContestantId { get; set; }

        public decimal Points { get; set; }

        public int Rank { get; set; }

        public string Ordinal { get; set; }
    }
}
