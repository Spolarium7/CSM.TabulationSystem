using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSM.TabulationSystem.Web.Areas.Judging.ViewModels.Scoring;
using CSM.TabulationSystem.Web.Infrastructure.Authentication;
using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CSM.TabulationSystem.Web.Areas.Judging.Controllers
{
    [Area("Judging")]
    public class ScoringController : Controller
    {
        private readonly DefaultDbContext _context;
        public ScoringController(DefaultDbContext context)
        {
            _context = context;
        }       

        public JudgeIdentifierViewModel GetEventByKey()
        {
            if(WebUser.IsAuthenticated == false)
            {
                return null;
            }

            if (string.IsNullOrEmpty(WebUser.EventKey))
            {
                return null;
            }
            else
            {
                var judge = this._context.Judges.FirstOrDefault(j => j.EventKey == WebUser.EventKey && j.UserId == WebUser.UserId);

                if(judge != null)
                {
                    var myEvent = this._context.Events.FirstOrDefault(e => e.Id == judge.EventId);

                    return new JudgeIdentifierViewModel()
                    {
                        EventId = judge.EventId,
                        JudgeId = judge.Id,
                        UserId = WebUser.UserId,
                        EventIsLocked = myEvent.IsLocked,
                        EventName = myEvent.Title
                    };
                }
                else
                {
                    return null;
                }
            }
        }


        [HttpGet, Route("judging/scoring")]
        [HttpGet, Route("judging/scoring/index")]
        public IActionResult Index()
        {
            JudgeIdentifierViewModel judge = GetEventByKey();
            if(judge == null)
            {
                return RedirectToAction("EnterEventKey", new { redirectUrl = "index" });
            }

            if (judge.EventIsLocked == true)
            {
                return RedirectPermanent("~/judging/scoring/accessdenied");
            }

            var allMyScores = this._context.Scores.Where(s => s.EventId == judge.EventId && s.JudgeId == judge.JudgeId && s.UserId == WebUser.UserId);

            IndexViewModel model = new IndexViewModel();
            List<ContestantScores> contestantScores = new List<ContestantScores>();
            var criteria = this._context.Criteria.Where(c => c.EventId == judge.EventId).ToList();

            if (allMyScores != null)
            {
                
                var contestants = this._context.Contestants.Where(c => c.EventId == judge.EventId).ToList();

                foreach(var contestant in contestants)
                {
                    Dictionary<string, decimal> scores = new Dictionary<string, decimal>();

                    foreach(var criterion in criteria)
                    {
                        var score = allMyScores.FirstOrDefault(s => 
                                                                    s.EventId == judge.EventId
                                                                && s.JudgeId == judge.JudgeId
                                                                && s.UserId == WebUser.UserId
                                                                && s.ContestantId == contestant.Id
                                                                && s.CriterionId == criterion.Id);
                        scores.Add(criterion.Name.ToLower(), (score != null ? score.Points : decimal.Parse("0.00")));
                    }

                    contestantScores.Add(new ContestantScores()
                    {
                        ContestantId = contestant.Id,
                        ContestantName = contestant.Name,
                        Scores = scores
                    });
                }

                var eventName = this._context.Events.FirstOrDefault(e => e.Id == judge.EventId);

                model.ContestantScores = contestantScores;
                model.Criteria = criteria;
                model.EventName = (eventName != null ? eventName.Title : "");
            }

            return View(model);
        }

        [HttpGet, Route("judging/scoring/accessdenied")]
        public IActionResult AccessDenied(string returnUrl)
        {
            
            return View();
        }


        [HttpGet, Route("judging/scoring/enter-event-key")]
        public IActionResult EnterEventKey(string redirectUrl)
        {
            WebUser.EventKey = string.Empty;
            return View(new EnterEventKeyViewModel()
            {
                ReturnUrl = redirectUrl
            });
        }

        [HttpPost, Route("judging/scoring/enter-event-key")]
        public IActionResult EnterEventKey(EnterEventKeyViewModel model)
        {
            WebUser.EventKey = model.EventKey;

            if (string.IsNullOrEmpty(model.ReturnUrl))
            {
                model.ReturnUrl = "index";
            }

            return RedirectToAction(model.ReturnUrl);
        }

        [HttpPost, Route("judging/scoring/upsert-score")]
        public IActionResult UpsertScore(UpsertScoreViewModel model)
        {
            JudgeIdentifierViewModel judge = GetEventByKey();
            if (judge == null)
            {
                return BadRequest();
            }

            if(judge.EventIsLocked == true)
            {
                return BadRequest();
            }

            var score = this._context.Scores.FirstOrDefault(s =>
                                                                 s.ContestantId == model.ContestantId
                                                              && s.CriterionId == model.CriterionId
                                                              && s.EventId == judge.EventId
                                                              && s.JudgeId == judge.JudgeId
                                                              && s.UserId == judge.UserId);

            if (score == null)
            {
                score = new Infrastructure.Data.Models.Score()
                {
                    ContestantId = model.ContestantId,
                    CriterionId = model.CriterionId,
                    EventId = judge.EventId,
                    JudgeId = judge.JudgeId,
                    UserId = judge.UserId,
                    Points = model.Points
                };

                this._context.Scores.Add(score);
                this._context.SaveChanges();
                return Ok(score);
            }
            else
            {
                score.Points = model.Points;
                this._context.Scores.Update(score);
                this._context.SaveChanges();
                return Ok(score);
            };
        }
    }
}