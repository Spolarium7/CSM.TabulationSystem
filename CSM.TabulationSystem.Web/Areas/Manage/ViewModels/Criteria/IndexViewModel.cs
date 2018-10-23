using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using CSM.TabulationSystem.Web.Infrastructure.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Areas.Manage.ViewModels.Criteria
{
    public class IndexViewModel
    {
        public Page<Criterion> Criteria { get; set; }

        public Guid? EventId { get; set; }
    }
}
