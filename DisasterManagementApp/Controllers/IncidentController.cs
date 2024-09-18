using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System;
// using System.Linq;
using DisasterManagementApp.Data;
using Microsoft.AspNetCore.Authorization;

[Authorize] // <-- This restricts all actions in this controller to logged-in users
public class IncidentController : Controller
{
    private readonly ApplicationDbContext _context;

    public IncidentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Incidents
    public IActionResult Index()
    {
        // Retrieve incidents without converting the DateTime values
        var incidents = _context.Incidents.ToList();
        return View(incidents);
    }

    // GET: Incident/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Incident/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Incident incident)
    {
        if (ModelState.IsValid)
        {
            // Ensure that DateReported is in UTC (or default to current UTC time if not set)
            if (incident.DateReported == default(DateTime))
            {
                incident.DateReported = DateTime.UtcNow; // Set to UTC now if no date is provided
            }
            else
            {
                incident.DateReported = incident.DateReported.ToUniversalTime(); // Convert to UTC if needed
            }

            // Store the incident with UTC DateReported
            _context.Incidents.Add(incident);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        return View(incident);
    }

    // GET: Incident/Delete/5
    public IActionResult Delete(int? incidentId)
    {
        if (incidentId == null)
        {
            return NotFound();
        }

        var incident = _context.Incidents
            .FirstOrDefault(m => m.IncidentId == incidentId);
        if (incident == null)
        {
            return NotFound();
        }

        // No need to convert to IST, the value remains in UTC for display
        return View(incident);
    }

    // POST: Incident/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int incidentId)
    {
        var incident = _context.Incidents.Find(incidentId);
        if (incident != null)
        {
            _context.Incidents.Remove(incident);
            _context.SaveChanges();
        }

        return RedirectToAction(nameof(Index));
    }
}
