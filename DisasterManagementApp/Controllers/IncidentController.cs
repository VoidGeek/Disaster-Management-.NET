using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System;
// using System.Linq;
using DisasterManagementApp.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Firebase.Auth;
using Firebase.Storage;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;



[Authorize] // <-- This restricts all actions in this controller to logged-in users
public class IncidentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public IncidentController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // GET: Incidents


    public IActionResult Index(string searchTerm)
    {
        // Get the User ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
        {
            // Retrieve the user's role from the database based on their UserId
            var roleId = _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.RoleId)
                .FirstOrDefault();

            // Get the base query for incidents based on user role
            IQueryable<Incident> query = roleId == 2
                ? _context.Incidents
                : _context.Incidents.Where(i => i.UserId == userId);

            // Apply search if searchTerm is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Use regular expression to match any part of the title (case-insensitive)
                query = query.Where(i => Regex.IsMatch(i.Title, searchTerm, RegexOptions.IgnoreCase));
            }

            // Execute the query and return results to the view
            var incidents = query.ToList();
             // Pass the RoleId to the view using ViewBag or ViewData
            ViewBag.RoleId = roleId;
            return View(incidents);
        }

        return View(new List<Incident>());
    }





    // GET: Incident/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Incident/Create
    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Incident incident, IFormFile fileUpload)
{
    if (ModelState.IsValid)
    {
        // Get the User ID from claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            // Assign the user ID to the incident
            incident.UserId = userId;
        }
        else
        {
            // If parsing fails, add an error to ModelState and return the view
            ModelState.AddModelError("", "Unable to find a valid User ID.");
            return View(incident);
        }

        // Set the DateReported to the current UTC date and time if not provided
        if (incident.DateReported == default(DateTime))
        {
            incident.DateReported = DateTime.UtcNow;
        }

        // Check if there is a file to upload
        if (fileUpload != null && fileUpload.Length > 0)
        {
            // Firebase storage configuration
            var apiKey = _configuration["Firebase:ApiKey"];
            var authEmail = _configuration["Firebase:AuthEmail"];
            var authPassword = _configuration["Firebase:AuthPassword"];
            var bucket = _configuration["Firebase:Bucket"];

            // Authenticate to Firebase using email and password
            var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            var firebaseAuth = await auth.SignInWithEmailAndPasswordAsync(authEmail, authPassword);

            // Create a unique filename for the file to be uploaded
            var stream = fileUpload.OpenReadStream();
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileUpload.FileName)}";

            // Upload the file to Firebase Storage
            var task = new FirebaseStorage(
                bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(firebaseAuth.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("uploads") // Define the folder name in your Firebase storage
                .Child(fileName)
                .PutAsync(stream);

            try
            {
                // Get the download URL after the file is uploaded
                incident.FileUrl = await task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                ModelState.AddModelError("", "File upload failed. Please try again.");
                return View(incident);
            }
        }

        // Store the incident details in PostgreSQL
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

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

    // GET: Incident/Edit/5
}


