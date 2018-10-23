using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Infrastructure.Data.Models
{
    public class Event : BaseModel
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public string Description { get; set; }

        public bool IsLocked { get; set; }

    }
}
