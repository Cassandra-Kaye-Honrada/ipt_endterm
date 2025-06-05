using System.Data;
using System.Security.Cryptography;
using System.Text;
using Endterm_IPT.Models;
using Endterm_IPT.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace Endterm_IPT.Controllers
{
    public class AuthController : Controller
    {
        private readonly DataHelper _databaseHelper;

        public AuthController()
        {
            string connString = "Server=localhost;Database=onlinestore;User Id=root;Password='';";
            _databaseHelper = new DataHelper(connString);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User model)
        {
            if (ModelState.IsValid)
            {
                string checkQuery = $"SELECT * FROM Users WHERE Email='{model.Email}'";
                DataTable dt = _databaseHelper.selectQuery(checkQuery);

                if (dt.Rows.Count > 0)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(model);
                }

                string hashedPassword = HashPassword(model.PasswordHash);
                string insertQuery = $"INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Address, City, State, ZipCode, Phone, IsAdmin, RegistrationDate) " +
                                     $"VALUES ('{model.Email}', '{hashedPassword}', '{model.FirstName}', '{model.LastName}', '{model.Address}', '{model.City}', '{model.State}', '{model.ZipCode}', '{model.Phone}', {model.IsAdmin}, '{model.RegistrationDate:yyyy-MM-dd HH:mm:ss}')";

                int result = _databaseHelper.executeQuery(insertQuery);
                if (result > 0) return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(Login model)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = HashPassword(model.Password);
                string query = $"SELECT * FROM Users WHERE Email = '{model.Email}' AND PasswordHash = '{hashedPassword}'";

                DataTable dt = _databaseHelper.selectQuery(query);

                if (dt.Rows.Count > 0)
                {
                    var user = dt.Rows[0];
                    bool isAdmin = Convert.ToBoolean(user["IsAdmin"]);

                    HttpContext.Session.SetString("Email", user["Email"].ToString());
                    HttpContext.Session.SetString("FirstName", user["FirstName"].ToString());
                    HttpContext.Session.SetString("IsAdmin", isAdmin.ToString());
                    HttpContext.Session.SetString("UserId", user["UserId"].ToString());

                    return isAdmin
                        ? RedirectToAction("AdminDashboard", "AdminDashboard")
                        : RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        public IActionResult AdminLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        private string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // Helper method to check if user is authenticated
        public static bool IsUserAuthenticated(HttpContext httpContext)
        {
            return !string.IsNullOrEmpty(httpContext.Session.GetString("UserId"));
        }
    }
}