using CSM.TabulationSystem.Web.Infrastructure.Data.Enums;
using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.Infrastructure.Data.Models
{
    public class User : BaseModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName
        {
            get
            {
                return this.FirstName + " " + this.LastName;
            }
        }

        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        public SystemRole Role { get; set; }

        public LoginStatus LoginStatus { get; set; }

        public string RegistrationCode { get; set; }

        public int LoginTrials { get; set; }
    }
}
