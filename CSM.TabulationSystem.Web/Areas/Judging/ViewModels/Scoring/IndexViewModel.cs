using CSM.TabulationSystem.Web.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Areas.Judging.ViewModels.Scoring
{
    public class IndexViewModel
    {
        public Guid? EventId { get; set; }

        public string EventName { get; set; }

        public List<Criterion> Criteria { get; set; }

        public List<ContestantScores> ContestantScores { get; set; } 
    }

    public class ContestantScores
    {
        public Guid? ContestantId { get; set; }

        public string ContestantName { get; set; }

        public IDictionary<string,decimal> Scores { get; set; }
    }

}
