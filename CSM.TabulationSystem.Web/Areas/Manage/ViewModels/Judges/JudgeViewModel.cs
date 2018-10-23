using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Areas.Manage.ViewModels.Judges
{
    public class JudgeViewModel
    {
        public Guid? EventId { get; set; }

        public Guid? JudgeId { get; set; }

        public Guid? UserId { get; set; }

        public string FullName { get; set; }

        public string Totem { get; set; }
    }
}
