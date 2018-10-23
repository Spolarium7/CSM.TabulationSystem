using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Areas.Manage.ViewModels.Judges
{
    public class IndexViewModel
    {
        public Page<JudgeViewModel> Judges { get; set; }

        public Guid? EventId { get; set; }
    }
}
