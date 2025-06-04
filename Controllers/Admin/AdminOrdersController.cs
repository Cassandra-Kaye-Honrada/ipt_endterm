using Endterm_IPT.DataAccess;
using Endterm_IPT.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Endterm_IPT.Controllers.Admin
{
    public class AdminOrdersController : Controller
    {
        private readonly DatabaseHelper _databaseHelper;

        public AdminOrdersController()
        {
            string connString = "Server=localhost;Database=onlinestore;User Id=root;Password='';";
            _databaseHelper = new DatabaseHelper(connString);
        }
        [Route("orderView/{id?}", Name ="ViewOrders")]
        public ActionResult Index(int? id)
        {
            string query = "";
            if(id != null)
            {
                 query = @$"
                SELECT 
                    users.FirstName, 
                    users.LastName, 
                    checkout.OrderId,
                    checkout.OrderDate, 
                    checkout.Status, 
                    checkout.ShippingAddress, 
                    checkout.ShippingCity, 
                    checkout.ShippingState, 
                    checkout.ShippingZipCode, 
                    checkout.PaymentMethod
                FROM 
                    checkout
                INNER JOIN 
                    users ON checkout.UserId = users.UserId 
                where 
                    checkout.UserId = {id}";
            }
            else
            {
                 query = @"
                SELECT 
                    users.FirstName, 
                    users.LastName, 
                    checkout.OrderId,
                    checkout.OrderDate, 
                    checkout.Status, 
                    checkout.ShippingAddress, 
                    checkout.ShippingCity, 
                    checkout.ShippingState, 
                    checkout.ShippingZipCode, 
                    checkout.PaymentMethod
                FROM 
                    checkout
                INNER JOIN 
                    users ON checkout.UserId = users.UserId";
            }
            
            DataTable dt = _databaseHelper.SelectQuery(query);
            List<Order>sqlOrders = new List<Order>();
            foreach (DataRow row in dt.Rows)
            {
                Order order = new Order
                {
                    OrderId = Convert.ToInt32(row["OrderId"]),
                    UserName = row["FirstName"].ToString() + " " + row["LastName"].ToString(),
                    OrderDate = Convert.ToDateTime(row["OrderDate"]),
                    Status = row["Status"].ToString(),
                    ShippingAddress = row["ShippingAddress"].ToString(),
                    ShippingCity = row["ShippingCity"].ToString(),
                    ShippingState = row["ShippingState"].ToString(),
                    ShippingZipCode = row["ShippingZipCode"].ToString(),
                    PaymentMethod = row["PaymentMethod"].ToString()
                };
                sqlOrders.Add(order);
            }
            return View(sqlOrders);
        }
        [Route("updateOrder/{id}", Name ="OrderUpdate")]
        public IActionResult updateOrderStatus(int id)
        {
            string query = $"Update checkout set Status = 'Accepted' where OrderId = {id}";
            _databaseHelper.ExecuteQuery(query);
            return RedirectToRoute("ViewOrders");
        }

        // GET: AdminOrdersController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AdminOrdersController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AdminOrdersController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AdminOrdersController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AdminOrdersController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AdminOrdersController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AdminOrdersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
