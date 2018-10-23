using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CSM.TabulationSystem.Web.Models;
using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using CSM.TabulationSystem.Web.ViewModels.Home;
using CSM.TabulationSystem.Web.Infrastructure.Data.Models;

namespace CSM.TabulationSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly DefaultDbContext _context;

        public HomeController(DefaultDbContext context)
        {
            _context = context;
        }

        [HttpGet, Route("home")]
        [HttpGet, Route("home/index")]
        public IActionResult Index(Guid? eventId)
        {
            var allMyScores = this._context.Scores.Where(s => s.EventId == eventId);

            IndexViewModel model = new IndexViewModel();
            List<ContestantScores> contestantScores = new List<ContestantScores>();
            var criteria = this._context.Criteria.Where(c => c.EventId == eventId).ToList();


            //all events
            var myEvents = this._context.Events.ToList();
            List<TextValuePair> events = new List<TextValuePair>();
            foreach(Event myEvent in myEvents)
            {
                events.Add(new TextValuePair()
                {
                    Id = myEvent.Id,
                    Name = myEvent.Title
                });
            }
            model.Events = events;

            if (allMyScores != null)
            {

                var contestants = this._context.Contestants.Where(c => c.EventId == eventId).ToList();
                var aveScore = decimal.Parse("0.00");
                foreach (var contestant in contestants)
                {
                    aveScore = decimal.Parse("0.00");
                    Dictionary<string, decimal> scores = new Dictionary<string, decimal>();
                    foreach (var criterion in criteria)
                    {
                        var criterionScores = allMyScores.Where(s =>
                                                                    s.EventId == eventId
                                                                && s.ContestantId == contestant.Id
                                                                && s.CriterionId == criterion.Id).ToList();


                        var totalScore = decimal.Parse("0.00");
                        foreach(var criterionScore in criterionScores)
                        {
                            totalScore = totalScore + (criterionScore != null ? criterionScore.Points : 0);
                        }

                        var score = totalScore / (decimal)criterionScores.Count;

                        scores.Add(criterion.Name.ToLower(), score);
                        aveScore = aveScore + score;
                    }

                    contestantScores.Add(new ContestantScores()
                    {
                        ContestantId = contestant.Id,
                        ContestantName = contestant.Name,
                        Scores = scores,
                        AverageScore = aveScore
                    });
                }

                var eventName = this._context.Events.FirstOrDefault(e => e.Id == eventId);

                model.ContestantScores = contestantScores.OrderByDescending(s => s.AverageScore).ToList();
                model.Criteria = criteria;
                model.EventName = (eventName != null ? eventName.Title : "");
            }

            return View(model);
        }

        [HttpGet,Route("home/score-breakdown")]
        public IActionResult ScoreBreakdown(Guid? contestantId, Guid? criterionId)
        {
            var scores = this._context.Scores.Where(s => s.ContestantId == contestantId && s.CriterionId == criterionId)
                .Select(s => new ScoreBreakdownViewModel()
                {
                    JudgeId = s.JudgeId,
                    Points = s.Points,
                    ScoreId = s.Id
                })
                .ToList();

            foreach(ScoreBreakdownViewModel score in scores)
            {
                score.JudgeTotem = GetJudgeTotem(score.JudgeId);
            }

            return Ok(scores);
        }

        private string GetJudgeTotem(Guid? judgeId)
        {
            var judge = this._context.Judges.FirstOrDefault(j => j.Id == judgeId);

            if(judge != null)
            {
                return judge.Totem;
            }

            return "bear";
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
