using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using codecampster.Models;
using Microsoft.AspNet.Identity;
using codecampster.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace codecampster.Controllers
{
    public class SessionsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private ApplicationDbContext _context;

        public SessionsController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<SpeakersController>();
            _context = context;
        }

        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
        public IActionResult Agenda()
        {
            ViewBag.Timeslots = _context.Timeslots.OrderBy(t => t.Rank);
            ViewBag.TrackCount = _context.Tracks.Count();
            ViewBag.Tracks = _context.Tracks;
            IQueryable<Session> sessions = _context.Sessions.Include(s => s.Speaker).Include(s => s.Track).Include(s => s.Timeslot).OrderBy(x => Guid.NewGuid());
            return View(sessions.ToList());
        }


        [ResponseCache(Duration = 300,Location=ResponseCacheLocation.Client)]
        public IActionResult Index(string track, string timeslot)
        {
            ViewBag.Timeslots = _context.Timeslots.Where(s=> (!(s.Special == true))).OrderBy(t => t.Rank);
            ViewBag.Tracks = _context.Tracks.OrderBy(x => x.Name);
            IQueryable<Session> sessions = _context.Sessions.Where(s => (!(s.Special == true))).Include(s => s.Speaker).Include(s => s.Track).Include(s => s.Timeslot).OrderBy(x => Guid.NewGuid());
            ViewData["Title"] = string.Format("All {0} Sessions",sessions.Count());
            if (!string.IsNullOrEmpty(track))
            {
                int trackID = 0;
                if (int.TryParse(track, out trackID))
                {
                    var tr = _context.Tracks.Single(t => t.ID == trackID);
                    ViewData["Title"] = string.Format("{0} Track Sessions {1}", tr.Name, tr.RoomNumber);
                    return View(sessions.Where(s => (s.TrackID == trackID) && (!(s.Special == true))).ToList());
                }
            }
            if (!string.IsNullOrEmpty(timeslot))
            {
                int timeslotId = 0;
                if (int.TryParse(timeslot, out timeslotId))
                {
                    var ts = _context.Timeslots.Single(t => t.ID == timeslotId);
                    ViewData["Title"] = string.Format("{0} - {1} Sessions", ts.StartTime, ts.EndTime);
                    return View(sessions.Where(s => s.TimeslotID == timeslotId).ToList());
                }
            }
            return View(sessions.ToList());
        }

        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Session session = _context.Sessions.Include(s => s.Speaker).Include(s => s.Track).Include(s => s.Timeslot).Single(m => m.SessionID == id);
            if (session == null)
            {
                return HttpNotFound();
            }

            return View(session);
        }

        // GET: Sessions/Create
        public IActionResult Create()
        {
            ViewData["SpeakerID"] = new SelectList(_context.Speakers, "ID", "Speaker");
            return View();
        }

        // POST: Sessions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Session session)
        {
            if (ModelState.IsValid)
            {
                _context.Sessions.Add(session);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewData["SpeakerID"] = new SelectList(_context.Speakers, "ID", "Speaker", session.SpeakerID);
            return View(session);
        }

        // GET: Sessions/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Session session = _context.Sessions.Single(m => m.SessionID == id);
            if (session == null)
            {
                return HttpNotFound();
            }
            ViewData["SpeakerID"] = new SelectList(_context.Speakers, "ID", "Speaker", session.SpeakerID);
            return View(session);
        }

        // POST: Sessions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Session session)
        {
            if (ModelState.IsValid)
            {
                _context.Update(session);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewData["SpeakerID"] = new SelectList(_context.Speakers, "ID", "Speaker", session.SpeakerID);
            return View(session);
        }

        // GET: Sessions/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Session session = _context.Sessions.Single(m => m.SessionID == id);
            if (session == null)
            {
                return HttpNotFound();
            }

            return View(session);
        }

        // POST: Sessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Session session = _context.Sessions.Single(m => m.SessionID == id);
            _context.Sessions.Remove(session);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
