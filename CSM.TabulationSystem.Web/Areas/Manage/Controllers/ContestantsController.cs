using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSM.TabulationSystem.Web.Areas.Manage.ViewModels.Contestants;
using CSM.TabulationSystem.Web.Infrastructure.Data.Enums;
using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using CSM.TabulationSystem.Web.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CSM.TabulationSystem.Web.Areas.Manage.Controllers
{
    [Authorize(Policy = "AuthorizeAdmin")]
    [Area("Manage")]
    public class ContestantsController : Controller
    {
        private readonly DefaultDbContext _context;
        public ContestantsController(DefaultDbContext context)
        {
            _context = context;
        }

        [HttpGet, Route("manage/events/{eventId}/contestants")]
        [HttpGet, Route("manage/events/{eventId}/contestants/index")]
        public IActionResult Index(Guid? eventId, int pageSize = 5, int pageIndex = 1, string keyword = "")
        {
            Page<Contestant> result = new Page<Contestant>();

            if (pageSize < 1)
            {
                pageSize = 1;
            }

            IQueryable<Contestant> contentantQuery = (IQueryable<Contestant>)this._context.Contestants.Where(e => e.EventId == eventId);

            if (string.IsNullOrEmpty(keyword) == false)
            {
                contentantQuery = contentantQuery.Where(u => (u.Name.Contains(keyword)
                                            || u.Description.Contains(keyword))
                                            && u.EventId == eventId);
            }

            long queryCount = contentantQuery.Count();

            int pageCount = (int)Math.Ceiling((decimal)(queryCount / pageSize));
            long mod = (queryCount % pageSize);

            if (mod > 0)
            {
                pageCount = pageCount + 1;
            }

            int skip = (int)(pageSize * (pageIndex - 1));
            List<Contestant> contestants = contentantQuery.ToList();

            result.Items = contestants.Skip(skip).Take((int)pageSize).ToList();
            result.PageCount = pageCount;
            result.PageSize = pageSize;
            result.QueryCount = queryCount;
            result.PageIndex = pageIndex;

            return View(new IndexViewModel()
            {
                Contestants = result,
                EventId = eventId
            });
        }


        [HttpGet, Route("manage/events/{eventId}/contestants/create")]
        public IActionResult Create(Guid? eventId)
        {
            return View(new CreateViewModel()
            {
                EventId = eventId
            });
        }

        [HttpPost, Route("manage/events/contestants/create")]
        public IActionResult Create(CreateViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("index");

            var contestant = this._context.Contestants.FirstOrDefault(u => u.Name.ToLower() == model.Name.ToLower() && u.EventId == model.EventId);

            if (contestant == null)
            {
                contestant = new Contestant()
                {
                    EventId = model.EventId,
                    Name = model.Name,
                    Description = model.Description,
                };
                this._context.Contestants.Add(contestant);
                this._context.SaveChanges();
            }

            return RedirectToAction("index", new { eventId = model.EventId });
        }

        [HttpGet, Route("manage/events/contestants/change-status/{status}/{contestantId}")]
        public IActionResult ChangeStatus(string status, Guid? contestantId)
        {
            var stat = (ContestantStatus)Enum.Parse(typeof(ContestantStatus), status);
            var contestant = this._context.Contestants.FirstOrDefault(u => u.Id == contestantId);

            if (contestant != null)
            {
                contestant.Status = stat;
                this._context.Contestants.Update(contestant);
                this._context.SaveChanges();
            }

            return RedirectToAction("index", new { eventId = contestant.EventId });
        }

        [HttpGet, Route("manage/events/contestants/delete/{contestantId}")]
        public IActionResult Delete(Guid? contestantId)
        {
            var contestant = this._context.Contestants.FirstOrDefault(u => u.Id == contestantId);

            if (contestant != null)
            {
                this._context.Contestants.Remove(contestant);
                this._context.SaveChanges();
            }

            return RedirectToAction("index", new { eventId = contestant.EventId });
        }

        [HttpGet, Route("manage/events/contestants/update/{contestantId}")]
        public IActionResult Update(Guid? contestantId)
        {
            var contestant = this._context.Contestants.FirstOrDefault(u => u.Id == contestantId);

            if (contestant != null)
            {
                return View(
                    new UpdateViewModel()
                    {
                        EventId = contestant.EventId,
                        ContestantId = contestantId,
                        Name = contestant.Name,
                        Description = contestant.Description
                    }
                );
            }

            return RedirectToAction("index", new { eventId = contestant.EventId });
        }

        [HttpPost, Route("manage/events/contestants/update")]
        public IActionResult Update(UpdateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var contestant = this._context.Contestants.FirstOrDefault(u => u.Id == model.ContestantId);

            if (contestant != null)
            {
                contestant.Name = model.Name;
                contestant.Description = model.Description;
                this._context.Contestants.Update(contestant);
                this._context.SaveChanges();
            }

            return RedirectToAction("index", new { eventId = contestant.EventId });
        }
    }
}