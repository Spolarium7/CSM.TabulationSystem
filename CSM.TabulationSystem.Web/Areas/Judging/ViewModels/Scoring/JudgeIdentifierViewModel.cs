using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Areas.Judging.ViewModels.Scoring
{
    public class JudgeIdentifierViewModel
    {
        public Guid? EventId { get; set; }

        public string EventName { get; set; }

        public bool EventIsLocked { get; set; }

        public Guid? JudgeId { get; set; }

        public Guid? UserId { get; set; }

    }
}
