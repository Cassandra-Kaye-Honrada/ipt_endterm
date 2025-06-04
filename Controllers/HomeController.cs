using Endterm_IPT.DataAccess;
using Endterm_IPT.Models;
using Microsoft.AspNetCore.Mvc;
using Quibol_ASS2_3D.Models;
using System.Data;
using System.Diagnostics;

namespace Quibol_ASS2_3D.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataHelper _databaseHelper;

        public HomeController()
        {
            string connString = "Server=localhost;Database=onlinestore;User Id=root;Password='';";
            _databaseHelper = new DataHelper(connString);
        }
        public IActionResult Index()
        {
            string query = @"
                SELECT p.ProductId, p.ProductName, p.Description, p.BasePrice, 
                p.Stock, p.ImageUrl, p.Status, c.CategoryName
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.Status = 'Active'";

            DataTable dt = _databaseHelper.selectQuery(query);

            List<Product> products = new List<Product>();
            foreach (DataRow row in dt.Rows)
            {
                products.Add(new Product
                {
                    ProductId = Convert.ToInt32(row["ProductId"]),
                    ProductName = row["ProductName"].ToString(),
                    Description = row["Description"].ToString(),
                    BasePrice = Convert.ToDecimal(row["BasePrice"]),
                    Stock = Convert.ToInt32(row["Stock"]),
                    ImageUrl = row["ImageUrl"].ToString(),
                    Status = row["Status"].ToString(),
                    CategoryName = row["CategoryName"].ToString()
                });
            }

            return View(products);
        }

        

        [HttpGet]
        public JsonResult SearchJson(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<Product>());
            query = query.Replace("'", "''");
            string sql = $@"
            SELECT p.ProductId, p.ProductName, p.Description, p.BasePrice, p.Stock, p.ImageUrl, p.Status, c.CategoryName
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
            WHERE p.ProductName LIKE '%{query}%' AND p.Status = 'Active'
            LIMIT 10";

            DataTable dt = _databaseHelper.selectQuery(sql);
            var products = new List<Product>();

            foreach (DataRow row in dt.Rows)
            {
                products.Add(new Product
                {
                    ProductId = Convert.ToInt32(row["ProductId"]),
                    ProductName = row["ProductName"].ToString(),
                    Description = row["Description"].ToString(),
                    BasePrice = Convert.ToDecimal(row["BasePrice"]),
                    Stock = Convert.ToInt32(row["Stock"]),
                    ImageUrl = row["ImageUrl"].ToString(),
                    Status = row["Status"].ToString(),
                    CategoryName = row["CategoryName"]?.ToString() ?? ""
                });
            }

            return Json(products);
        }



        public IActionResult ProductDetails(int id)
        {
            string query = $@"
            SELECT p.*, c.CategoryName 
            FROM products p 
            JOIN categories c ON p.CategoryId = c.CategoryId 
            WHERE p.ProductId = {id} 
            LIMIT 1";

            DataTable dt = _databaseHelper.selectQuery(query);

            if (dt.Rows.Count == 0)
            {
                return NotFound();
            }

            var row = dt.Rows[0];
            var product = new Product
            {
                ProductId = Convert.ToInt32(row["ProductId"]),
                ProductName = row["ProductName"].ToString(),
                Description = row["Description"].ToString(),
                BasePrice = Convert.ToDecimal(row["BasePrice"]),
                Stock = Convert.ToInt32(row["Stock"]),
                ImageUrl = row["ImageUrl"].ToString(),
                Status = row["Status"].ToString(),
                CategoryName = row["CategoryName"].ToString()
            };

            return View(product);
        }

    }
}
