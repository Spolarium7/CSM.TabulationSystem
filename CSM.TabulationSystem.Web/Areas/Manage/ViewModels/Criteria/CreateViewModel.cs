using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Areas.Manage.ViewModels.Criteria
{
    public class CreateViewModel
    {
        public Guid? EventId { get; set; }

        public string Name { get; set; }

        public decimal Percentage { get; set; }

        public string Description { get; set; }
    }
}
