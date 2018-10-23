using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Infrastructure.Data.Models
{
    public class Score : BaseModel
    {
        public Guid? EventId { get; set; }

        public Guid? UserId { get; set; }

        public Guid? JudgeId { get; set; }

        public Guid? CriterionId { get; set; }

        public Guid? ContestantId { get; set; }

        public decimal Points { get; set; } 
    }
}
