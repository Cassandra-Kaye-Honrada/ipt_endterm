
using Endterm_IPT.DataAccess;
using Endterm_IPT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Endterm_IPT.Controllers.Admin
{
    public class AdminUsersController : Controller
    {
        private readonly DataHelper _databaseHelper;
        public AdminUsersController()
        {
            string connString = "Server=localhost;Database=onlinestore;User Id=root;Password='';";
            _databaseHelper = new DataHelper(connString);
        }

        [Route("UserList", Name = "ViewUsers")]
        public IActionResult UserList()
        {
            string query = "select * from users where IsAdmin = 0;";
            DataTable dt = _databaseHelper.selectQuery(query);
            List<User> sqlUsers = new List<User>();
            foreach (DataRow row in dt.Rows)
            {
                User user = new User
                {
                    UserId = Convert.ToInt32(row["UserId"]),
                    Email = row["Email"].ToString(),
                    FirstName = row["FirstName"].ToString(),
                    LastName = row["LastName"].ToString(),
                    Address = row["Address"].ToString(),
                    City = row["City"].ToString(),
                    State = row["State"].ToString(),
                    ZipCode = row["ZipCode"].ToString(),
                    Phone = row["Phone"].ToString(),
                    IsAdmin = Convert.ToBoolean(row["IsAdmin"]),
                    RegistrationDate = Convert.ToDateTime(row["RegistrationDate"])
                };
                sqlUsers.Add(user);
            }
            return View(sqlUsers);
        }

        public IActionResult Delete(int id)
        {
            string query = $"delete from users where UserId = {id}";
            _databaseHelper.executeQuery(query);
            return RedirectToRoute("ViewUsers");
        }


        public IActionResult Edit(int id)
        {
            string query = $"Select * from users where UserId = {id};";
            DataTable dt = _databaseHelper.selectQuery(query);
            var row = dt.Rows[0];
            User user = new User
            {
                UserId = Convert.ToInt32(row["UserId"]),
                Email = row["Email"].ToString(),
                FirstName = row["FirstName"].ToString(),
                LastName = row["LastName"].ToString(),
                Address = row["Address"].ToString(),
                City = row["City"].ToString(),
                State = row["State"].ToString(),
                ZipCode = row["ZipCode"].ToString(),
                Phone = row["Phone"].ToString(),
                IsAdmin = Convert.ToBoolean(row["IsAdmin"]),
                RegistrationDate = Convert.ToDateTime(row["RegistrationDate"])
            };
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit( User user)
        {
            string query = "";
            if (!ModelState.IsValid)
            {
                query = @$"update users set 
                    Email = '{user.Email}',FirstName = '{user.FirstName}',
                    LastName = '{user.LastName}',Address = '{user.Address}',City = '{user.City}',
                    State = '{user.State}',ZipCode = '{user.ZipCode}',Phone = '{user.Phone}' where UserId = {user.UserId}";
                _databaseHelper.executeQuery(query);
                return RedirectToRoute("ViewUsers");
            }
            return View(user);
        }
    }
}
