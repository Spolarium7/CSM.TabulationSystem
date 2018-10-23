using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Areas.Manage.ViewModels.Contestants
{
    public class CreateViewModel
    {
        public Guid? EventId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
