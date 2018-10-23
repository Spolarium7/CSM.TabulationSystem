using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Infrastructure.Data.Models
{
    public class Judge : BaseModel
    {
        public Guid? UserId { get; set; }

        public Guid? EventId { get; set; }

        public string Totem { get; set; }

        public string EventKey { get; set; }
    }
}
