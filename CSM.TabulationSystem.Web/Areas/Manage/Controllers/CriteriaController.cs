using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSM.TabulationSystem.Web.Areas.Manage.ViewModels.Criteria;
using CSM.TabulationSystem.Web.Infrastructure.Data.Enums;
using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using CSM.TabulationSystem.Web.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CSM.TabulationSystem.Web.Areas.Manage.Controllers
{
    [Authorize(Policy = "AuthorizeAdmin")]
    [Area("Manage")]
    public class CriteriaController : Controller
    {
        private readonly DefaultDbContext _context;
        public CriteriaController(DefaultDbContext context)
        {
            _context = context;
        }

        [HttpGet, Route("manage/events/{eventId}/criteria")]
        [HttpGet, Route("manage/events/{eventId}/criteria/index")]
        public IActionResult Index(Guid? eventId, int pageSize = 5, int pageIndex = 1, string keyword = "")
        {
            Page<Criterion> result = new Page<Criterion>();

            if (pageSize < 1)
            {
                pageSize = 1;
            }

            IQueryable<Criterion> criterionQuery = (IQueryable<Criterion>)this._context.Criteria.Where(e => e.EventId == eventId);

            if (string.IsNullOrEmpty(keyword) == false)
            {
                criterionQuery = criterionQuery.Where(u => (u.Name.Contains(keyword)
                                            || u.Description.Contains(keyword))
                                            && u.EventId == eventId);
            }

            long queryCount = criterionQuery.Count();

            int pageCount = (int)Math.Ceiling((decimal)(queryCount / pageSize));
            long mod = (queryCount % pageSize);

            if (mod > 0)
            {
                pageCount = pageCount + 1;
            }

            int skip = (int)(pageSize * (pageIndex - 1));
            List<Criterion> criteria = criterionQuery.ToList();

            result.Items = criteria.Skip(skip).Take((int)pageSize).ToList();
            result.PageCount = pageCount;
            result.PageSize = pageSize;
            result.QueryCount = queryCount;
            result.PageIndex = pageIndex;

            return View(new IndexViewModel()
            {
                Criteria = result,
                EventId = eventId
            });
        }

        [HttpGet, Route("manage/events/{eventId}/criteria/create")]
        public IActionResult Create(Guid? eventId)
        {
            return View(new CreateViewModel()
            {
                EventId = eventId
            });
        }

        [HttpPost, Route("manage/events/criteria/create")]
        public IActionResult Create(CreateViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("index");

            var criterion = this._context.Criteria.FirstOrDefault(u => u.Name.ToLower() == model.Name.ToLower() && u.EventId == model.EventId);

            if (criterion == null)
            {
                criterion = new Criterion()
                {
                    EventId = model.EventId,
                    Name = model.Name,
                    Description = model.Description,
                    Percentage = model.Percentage
                };
                this._context.Criteria.Add(criterion);
                this._context.SaveChanges();
            }

            return RedirectToAction("index", new { eventId = model.EventId });
        }


        [HttpGet, Route("manage/events/criteria/delete/{criterionId}")]
        public IActionResult Delete(Guid? criterionId)
        {
            var criterion = this._context.Criteria.FirstOrDefault(u => u.Id == criterionId);

            if (criterion != null)
            {

                this._context.Criteria.Remove(criterion);
                this._context.SaveChanges();
            }

            return RedirectToAction("index", new { eventId = criterion.EventId });
        }

        [HttpGet, Route("manage/events/criteria/update/{criterionId}")]
        public IActionResult Update(Guid? criterionId)
        {
            var criterion = this._context.Criteria.FirstOrDefault(u => u.Id == criterionId);

            if (criterion != null)
            {
                return View(
                    new UpdateViewModel()
                    {
                        EventId = criterion.EventId,
                        CriterionId = criterionId,
                        Name = criterion.Name,
                        Description = criterion.Description,
                        Percentage = criterion.Percentage
                    }
                );
            }

            return RedirectToAction("index", new { eventId = criterion.EventId });
        }

        [HttpPost, Route("manage/events/criteria/update")]
        public IActionResult Update(UpdateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var criterion = this._context.Criteria.FirstOrDefault(u => u.Id == model.CriterionId);

            if (criterion != null)
            {
                criterion.Name = model.Name;
                criterion.Description = model.Description;
                criterion.Percentage = model.Percentage;
                this._context.Criteria.Update(criterion);
                this._context.SaveChanges();
            };

            return RedirectToAction("index", new { eventId = criterion.EventId });
        }
    }
}