using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CSM.TabulationSystem.Web.Infrastructure.Data.Enums;

namespace CSM.TabulationSystem.Web.Areas.Manage.ViewModels.Users
{
    public class UpdateProfileViewModel
    {
        [Required]
        public Guid? UserId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public SystemRole Role { get; set; }
    }
}
