using Endterm_IPT.DataAccess;
using Endterm_IPT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Endterm_IPT.Controllers
{
    public class ProductController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public ProductController()
        {
            string connString = "Server=localhost;Database=onlinestore;User Id=root;Password='';";
            _dbHelper = new DatabaseHelper(connString);
        }

        public IActionResult ViewDetails(int id)
        {
            var productItem = FetchProductInfo(id);
            if (productItem == null)
            {
                return NotFound();
            }
            return View(productItem);
        }

        private Product FetchProductInfo(int productId)
        {
            var sqlQuery = @"SELECT p.*, c.CategoryName 
                           FROM Products p 
                           LEFT JOIN Categories c ON p.CategoryId = c.CategoryId 
                           WHERE p.ProductId = " + productId + " AND p.Status = 'Active'";

            var dataTable = _dbHelper.SelectQuery(sqlQuery);

            if (dataTable.Rows.Count == 0) return null;

            var dataRow = dataTable.Rows[0];
            return new Product
            {
                ProductId = Convert.ToInt32(dataRow["ProductId"]),
                ProductName = dataRow["ProductName"].ToString(),
                Description = dataRow["Description"].ToString(),
                BasePrice = Convert.ToDecimal(dataRow["BasePrice"]),
                Stock = Convert.ToInt32(dataRow["Stock"]),
                CategoryId = dataRow["CategoryId"] == DBNull.Value ? (int?)null : Convert.ToInt32(dataRow["CategoryId"]),
                ImageUrl = dataRow["ImageUrl"].ToString(),
                Status = dataRow["Status"].ToString(),
                CategoryName = dataRow["CategoryName"]?.ToString()
            };
        }
    }
}