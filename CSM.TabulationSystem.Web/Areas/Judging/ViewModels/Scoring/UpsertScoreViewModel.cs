using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Areas.Judging.ViewModels.Scoring
{
    public class UpsertScoreViewModel
    {
        [Required]
        public Guid? EventId { get; set; }

        [Required]
        public Guid? CriterionId { get; set; }

        [Required]
        public Guid? ContestantId { get; set; }

        [Required]
        public decimal Points { get; set; }
    }
}
