using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSM.TabulationSystem.Web.Areas.Manage.ViewModels.Events;
using CSM.TabulationSystem.Web.Infrastructure.Data.Helpers;
using CSM.TabulationSystem.Web.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CSM.TabulationSystem.Web.Areas.Manage.Controllers
{
    [Authorize(Policy = "AuthorizeAdmin")]
    [Area("Manage")]
    public class EventsController : Controller
    {
        private readonly DefaultDbContext _context;
        public EventsController(DefaultDbContext context)
        {
            _context = context;
        }

        [HttpGet, Route("manage/events")]
        [HttpGet, Route("manage/events/index")]
        public IActionResult Index(int pageSize = 5, int pageIndex = 1, string keyword = "")
        {
            Page<Event> result = new Page<Event>();

            if (pageSize < 1)
            {
                pageSize = 1;
            }

            IQueryable<Event> eventQuery = (IQueryable<Event>)this._context.Events;

            if (string.IsNullOrEmpty(keyword) == false)
            {
                eventQuery = eventQuery.Where(u => u.Title.Contains(keyword)
                                            || u.Description.Contains(keyword));
            }

            long queryCount = eventQuery.Count();

            int pageCount = (int)Math.Ceiling((decimal)(queryCount / pageSize));
            long mod = (queryCount % pageSize);

            if (mod > 0)
            {
                pageCount = pageCount + 1;
            }

            int skip = (int)(pageSize * (pageIndex - 1));
            List<Event> events = eventQuery.ToList();

            result.Items = events.Skip(skip).Take((int)pageSize).ToList();
            result.PageCount = pageCount;
            result.PageSize = pageSize;
            result.QueryCount = queryCount;
            result.PageIndex = pageIndex;

            return View(new IndexViewModel()
            {
                Events = result
            });
        }

        [HttpGet, Route("manage/events/create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, Route("manage/events/create")]
        public IActionResult Create(CreateViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("index");

            var thisEvent = this._context.Events.FirstOrDefault(u => u.Title.ToLower() == model.Title.ToLower());

            if (thisEvent == null)
            {
                if (string.IsNullOrEmpty(model.Content))
                {
                    model.Content = model.Description;
                }

                thisEvent = new Event()
                {
                    Title = model.Title,
                    Description = model.Description,
                    Content = model.Content
                };
                this._context.Events.Add(thisEvent);
                this._context.SaveChanges();
            }

            return RedirectToAction("index");
        }

        [HttpGet, Route("manage/events/change-status/{status}/{eventId}")]
        public IActionResult ChangeStatus(string status, Guid? eventId)
        {
            var isLocked = status.ToLower() == "locked";
            var thisEvent = this._context.Events.FirstOrDefault(u => u.Id == eventId);

            if (thisEvent != null)
            {
                thisEvent.IsLocked = isLocked;
                this._context.Events.Update(thisEvent);
                this._context.SaveChanges();
            }

            return RedirectToAction("index");
        }

        [HttpGet, Route("manage/events/delete/{eventId}")]
        public IActionResult Delete(Guid? eventId)
        {
            var thisEvent = this._context.Events.FirstOrDefault(u => u.Id == eventId);

            if (thisEvent != null)
            {
                this._context.Events.Remove(thisEvent);
                this._context.SaveChanges();
            }

            return RedirectToAction("index");
        }

        [HttpGet, Route("manage/events/update/{eventId}")]
        public IActionResult Update(Guid? eventId)
        {
            var thisEvent = this._context.Events.FirstOrDefault(u => u.Id == eventId);

            if (thisEvent != null)
            {
                return View(
                    new UpdateViewModel()
                    {
                        EventId = eventId,
                        Title = thisEvent.Title,
                        Description = thisEvent.Description
                    }
                );
            }

            return RedirectToAction("create");
        }

        [HttpPost, Route("manage/events/update")]
        public IActionResult Update(UpdateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var thisEvent = this._context.Events.FirstOrDefault(u => u.Id == model.EventId);

            if (thisEvent != null)
            {
                thisEvent.Title = model.Title;
                thisEvent.Description = model.Description;
                this._context.Events.Update(thisEvent);
                this._context.SaveChanges();
            }

            return RedirectToAction("index");
        }
    }
}