using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CSM.TabulationSystem.Web.Infrastructure.Data.Enums;

namespace CSM.TabulationSystem.Web.Areas.Manage.ViewModels.Criteria
{
    public class UpdateViewModel
    {
        [Required]
        public Guid? CriterionId { get; set; }

        [Required]
        public Guid? EventId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Percentage { get; set; }
    }
}
