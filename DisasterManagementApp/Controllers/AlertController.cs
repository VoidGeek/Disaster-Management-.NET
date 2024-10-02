using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DisasterManagementApp.Data;
using System.Linq;
using System.Security.Claims;


    public class AlertController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlertController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Alert
        public async Task<IActionResult> Index()
        {
            // Load all alerts including their related incident information (if any)
            var alerts = await _context.Alerts.Include(a => a.Incident).ToListAsync();
            return View(alerts);
        }

        // GET: Alert/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alerts
                .Include(a => a.Incident)
                .FirstOrDefaultAsync(m => m.AlertId == id);

            if (alert == null)
            {
                return NotFound();
            }

            return View(alert);
        }

        // GET: Alert/Create
        public IActionResult Create(int incidentId)
    {
        // Initialize a new Alert with the provided IncidentId and current date/time
        var alert = new Alert
        {
            IncidentId = incidentId, // Set the foreign key automatically
            AlertDate = DateTime.Now.ToUniversalTime(),
            Message = "" // Initialize Message to avoid the error
        };

        return View(alert);
    }

    // POST: Alert/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Alert alert)
    {
        if (ModelState.IsValid)
        {
            _context.Add(alert);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Alert created successfully!";
            return RedirectToAction("Index", "Incident"); // Redirect to Incident index or any other page
        }

        return View(alert);
    }

        // GET: Alert/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null)
            {
                return NotFound();
            }
            return View(alert);
        }

        // POST: Alert/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlertId,Message,AlertDate,IncidentId")] Alert alert)
        {
            if (id != alert.AlertId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alert);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlertExists(alert.AlertId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(alert);
        }

        // GET: Alert/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alerts
                .Include(a => a.Incident)
                .FirstOrDefaultAsync(m => m.AlertId == id);

            if (alert == null)
            {
                return NotFound();
            }

            return View(alert);
        }

        // POST: Alert/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            
            if (alert == null)
            {
                // If alert is null, return NotFound or handle the case accordingly
                return NotFound();
            }
            
            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private bool AlertExists(int id)
        {
            return _context.Alerts.Any(e => e.AlertId == id);
        }
        [HttpGet]
    public async Task<IActionResult> GetRecentAlerts()
    {
        // Get the current user's UserId from the cookie or claims and convert to int
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return Unauthorized(); // Handle case where user ID could not be parsed
        }

        // Use raw SQL query to fetch alerts for the current user
        var recentAlerts = await _context.Alerts
            .FromSqlRaw(@"
                 SELECT a.""AlertId"", a.""IncidentId"", a.""Message"", a.""AlertDate""
                    FROM public.""Alerts"" a
                    JOIN public.""Incidents"" i ON a.""IncidentId"" = i.""IncidentId""
                    WHERE i.""UserId"" = {0}", currentUserId)
            .ToListAsync();

        return Json(recentAlerts);
    }
    }

