using Endterm_IPT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Org.BouncyCastle.Ocsp;
using Endterm_IPT.DataAccess;
using System.Data;
using System.Xml.Linq;

namespace Endterm_IPT.Controllers.Admin
{
    public class AdminCategoriesController : Controller
    {
        private readonly DataHelper _dataHelper;
        public AdminCategoriesController()
        {
            string connString = "Server=localhost;Database=onlinestore;User Id=root;Password='';";
            _dataHelper = new DataHelper(connString);
        }

        [Route("category", Name ="CategoriesAdmin")]
        public IActionResult Index()
        {
            string query = "Select * from categories";
            List<Categories> categories = new List<Categories>();
            DataTable dt = _dataHelper.selectQuery(query);
            foreach (DataRow dr in dt.Rows)
            {
                categories.Add(new Categories
                {
                    categoryId = Convert.ToInt32(dr["CategoryId"].ToString()),
                    categoryName = dr["CategoryName"].ToString(),
                    categoryDescription = dr["Description"].ToString(),
                });
            }
            return View(categories);
        }

        [HttpGet]
        [Route("categoryInsert", Name ="showInsertForm")]
        public IActionResult showInsert()
        {
            return View();
        }

        [Route("categoryInsert", Name = "InsertingCategories")]
        [HttpPost]
        public IActionResult showInsert(Categories category)
        {
            if (ModelState.IsValid)
            {
                string query = $"insert into categories (CategoryName, Description) values ('{category.categoryName}','{category.categoryDescription}')";
                int result = _dataHelper.executeQuery(query);
                if (result > 0)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Registration Failed");
                }
            }
            return View(category);
        }


        [HttpGet]
        [Route("CategoryEdit", Name = "EditCategory")]
        public IActionResult showEditForm(int id ) 
        {
            string query = $"Select * from categories where CategoryId = {id}";
            DataTable dt = _dataHelper.selectQuery(query);
            if (dt.Rows.Count == 0)
            {
                return NotFound();
            }
            Categories category = new Categories
            {
                categoryName = dt.Rows[0]["CategoryName"].ToString(),
                categoryDescription = dt.Rows[0]["Description"].ToString() ?? "",
                categoryId = Convert.ToInt32(dt.Rows[0]["CategoryId"])
            };
            return View(category);
        }

        [Route("CategoryEdit", Name = "UpdateCategory")]
        [HttpPost]
        public IActionResult showEditForm(Categories category)
        {
            if (ModelState.IsValid)
            {
                    string query = $"Update categories set CategoryName = '{category.categoryName}'," +
                        $"Description = '{category.categoryDescription}' where CategoryId = '{category.categoryId}'";
                    int result = _dataHelper.executeQuery(query);
                    if (result > 0)
                    {
                        return RedirectToRoute("CategoriesAdmin");
                    }
            }
            return View(category);
        }

        [Route("CategoryDelete", Name ="DeleteCategory")]
        public IActionResult deleteCategory(int id)
        {
            string query = $"delete from categories where CategoryId = {id}";
            int result =	_dataHelper.executeQuery(query);
                return RedirectToRoute("CategoriesAdmin");

            
        }


        [Route("viewProducts", Name ="ProductsView")]
        public IActionResult viewProducts(int id, string? name) {
            string query = $"Select * from products where CategoryId = {id}";
            List<Product> products = new List<Product>();
            DataTable dt = _dataHelper.selectQuery(query);
            if (dt.Rows.Count == 0)
            {
                ViewBag.Message = "This category is empty";
            }
            foreach (DataRow dr in dt.Rows)
            {
                products.Add(new Product
                {
                    ProductId = Convert.ToInt32(dr["ProductId"].ToString()),
                    ProductName = dr["ProductName"].ToString(),
                    Description = dr["Description"].ToString(),
                    BasePrice = Convert.ToDecimal(dr["BasePrice"].ToString()),
                    Stock = Convert.ToInt32(dr["Stock"].ToString()),
                
                    Status = dr["Status"].ToString(),
                    CategoryName = name,
                });
            }
            return View(products);
        }


        [Route("updateStatus", Name ="UpdateStatusToActive")]
        public IActionResult updateProduct(int id)
        {
            string selectQuery = $"select CategoryId from products where ProductId = '{id}'";
            List<int> products = new List<int>();
            DataTable dt = _dataHelper.selectQuery(selectQuery);
            foreach (DataRow dr in dt.Rows)
            {
                products.Add(Convert.ToInt32(dr["CategoryId"]));
            }
            int CategoryId = products[0];

            string query = @$"
                UPDATE Products 
                SET Status = CASE 
                    WHEN Status = 'Active' THEN 'Inactive' 
                    ELSE 'Active' 
                END 
                WHERE ProductId = {id}";
            int result = _dataHelper.executeQuery(query);
            return RedirectToAction("viewProducts", new { id = CategoryId });
        }
    }
}
