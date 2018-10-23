using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.TabulationSystem.Web.ViewModels.Home
{
    public class ScoreBreakdownViewModel
    {
        public Guid? JudgeId { get; set; }

        public Guid? ScoreId { get; set; }

        public decimal Points { get; set; }

        public string JudgeTotem { get; set; }
    }
}
