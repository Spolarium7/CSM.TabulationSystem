using CSM.TabulationSystem.Web.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.ViewModels.Home
{
    public class IndexViewModel
    {
        public Guid? EventId { get; set; }

        public string EventName { get; set; }

        public List<TextValuePair> Events { get; set; }

        public List<Criterion> Criteria { get; set; }

        public List<ContestantScores> ContestantScores { get; set; }
    }

    public class ContestantScores
    {
        public Guid? ContestantId { get; set; }

        public string ContestantName { get; set; }

        public IDictionary<string, decimal> Scores { get; set; }

        public decimal AverageScore { get; set; }
    }

    public class TextValuePair
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }
    }
}
