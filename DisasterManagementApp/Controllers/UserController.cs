using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using DisasterManagementApp.Data; // Adjust namespace as needed
using DisasterManagementApp.Models; // Adjust namespace as needed

public class UserController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SignIn (Registration Page)
    public IActionResult SignIn()
    {
        return View();
    }

    // POST: SignIn (Handles registration)
       [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult SignIn(User user)
{
    if (ModelState.IsValid)
    {
        // Check if username is already taken
        var isUsernameTaken = _context.Users.Any(u => u.Username == user.Username);
        
        if (isUsernameTaken)
        {
            ModelState.AddModelError("Username", "Username is already taken. Please choose a different one.");
            return View(user);
        }

        // Hash the password before storing
        user.Password = HashPassword(user.Password);

        // Set the default RoleId to "user" role only if RoleId is not provided in the request
        user.RoleId = 1; // Default to "user"

        // Save the user to the database (Username, Email, and Password)
        _context.Users.Add(user);
        _context.SaveChanges();
        return RedirectToAction("Login"); // Redirect to login after registration
    }
    return View(user);
}
    [HttpGet]
public JsonResult IsUsernameAvailable(string username)
{
    // Check if username is already taken
    var isAvailable = !_context.Users.Any(u => u.Username == username);
    
    return Json(isAvailable);
}

    // GET: Login (Login Page)
    public IActionResult Login()
    {
        return View();
    }

        // POST: Login (Handles login authentication)[HttpPost]
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(User user)
    {
        if (ModelState.IsValid)
        {
            // Retrieve user from the database
            var dbUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (dbUser != null && VerifyPassword(dbUser.Password, user.Password))
            {
                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, dbUser.Username),
                    new Claim(ClaimTypes.NameIdentifier, dbUser.UserId.ToString()) // Add UserId as a claim
                };

                // Create identity and principal
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign the user in with cookies
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                return RedirectToAction("Index", "Incident"); // Redirect to the incident page
            }

            ModelState.AddModelError("", "Invalid login attempt.");
        }
        else
        {
            // Output any validation errors to help debug
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
        }
        return View(user);
    }



    // GET: Logout (Handles logout)
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // Password hashing using PBKDF2
    private string HashPassword(string password)
    {
        // Generate a salt
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Derive a 256-bit subkey (32 bytes) using HMACSHA256 and PBKDF2
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        // Combine the salt and hashed password for storage
        return $"{Convert.ToBase64String(salt)}.{hashed}";
    }

    // Password verification (compare provided password with stored hash)
    private bool VerifyPassword(string storedPassword, string providedPassword)
    {
        // Split the stored password into salt and hash
        var parts = storedPassword.Split('.');
        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = parts[1];

        // Hash the provided password with the same salt
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: providedPassword,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        // Compare the hashes
        return hashed == storedHash;
    }
}
