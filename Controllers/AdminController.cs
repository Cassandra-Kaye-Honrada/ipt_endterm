using Microsoft.AspNetCore.Mvc;
using Endterm_IPT.Models;
using Endterm_IPT.DataAccess;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Linq;
using System;

namespace Endterm_IPT.Controllers
{
    public class AdminController : Controller
    {
        private readonly DatabaseHelper _databaseHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _xmlFilePath;

        public AdminController(IWebHostEnvironment webHostEnvironment)
        {
            string connString = "Server=localhost;Database=onlinestore;User Id=root;Password='';";
            _databaseHelper = new DatabaseHelper(connString);

            _webHostEnvironment = webHostEnvironment;
            string xmlFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Schemas");

            if (!Directory.Exists(xmlFolder))
            {
                Directory.CreateDirectory(xmlFolder);
            }

            _xmlFilePath = Path.Combine(xmlFolder, "products.xml");
        }

        // Index
        public IActionResult ProductIndex()
        {
            string query = "SELECT * FROM products";
            DataTable dt = _databaseHelper.SelectQuery(query);
            List<Product> sqlProducts = new List<Product>();

            foreach (DataRow row in dt.Rows)
            {
                Product product = new Product
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
                sqlProducts.Add(product);
            }

            ViewBag.XmlProducts = ReadProductsFromXML();
            return View(sqlProducts);
        }

        // Create Product (SQL)
        public IActionResult ProductCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProductCreate(Product prod)
        {
            if (!ModelState.IsValid)
                return View(prod);

            string query = $"INSERT INTO products (ProductName, Description, BasePrice, Stock, ImageUrl, Status, CategoryName) VALUES ('{prod.ProductName}', '{prod.Description}', {prod.BasePrice}, {prod.Stock}, '{prod.ImageUrl}', '{prod.Status}', '{prod.CategoryName}')";
            _databaseHelper.ExecuteQuery(query);

            return RedirectToAction("ProductIndex");
        }

        // Edit Product
        public IActionResult ProductEdit(int id)
        {
            string query = $"SELECT * FROM products WHERE ProductId = {id}";
            DataTable dt = _databaseHelper.SelectQuery(query);

            if (dt.Rows.Count == 0) return NotFound();

            DataRow row = dt.Rows[0];
            Product product = new Product
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProductEdit(Product product)
        {
            if (!ModelState.IsValid) return View(product);

            string query = $"UPDATE products SET ProductName = '{product.ProductName}', Description = '{product.Description}', BasePrice = {product.BasePrice}, Stock = {product.Stock}, ImageUrl = '{product.ImageUrl}', Status = '{product.Status}', CategoryName = '{product.CategoryName}' WHERE ProductId = {product.ProductId}";
            _databaseHelper.ExecuteQuery(query);

            return RedirectToAction("ProductIndex");
        }

        // Delete Product
        public IActionResult ProductDelete(int id)
        {
            string query = $"SELECT * FROM products WHERE ProductId = {id}";
            DataTable dt = _databaseHelper.SelectQuery(query);
            if (dt.Rows.Count == 0) return NotFound();

            DataRow row = dt.Rows[0];
            Product product = new Product
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProductDelete(Product product)
        {
            string query = $"DELETE FROM products WHERE ProductId = {product.ProductId}";
            _databaseHelper.ExecuteQuery(query);

            return RedirectToAction("ProductIndex");
        }

        // Export products 
        public IActionResult ExportProductsToXML()
        {
            string query = "SELECT * FROM products";
            DataTable dt = _databaseHelper.SelectQuery(query);
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

            WriteProductsToXML(products);
            TempData["Message"] = "Products exported to XML successfully.";

            return RedirectToAction("ProductIndex");
        }

        // Import products 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ImportProductsFromXML(IFormFile xmlFile)
        {
            if (xmlFile == null || xmlFile.Length == 0)
            {
                TempData["Error"] = "Please select a valid XML file to upload.";
                return RedirectToAction("ProductIndex");
            }

            var tempFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Schemas", "temp_products.xml");

            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                xmlFile.CopyTo(stream);
            }

            string dtdPath = Path.Combine(_webHostEnvironment.WebRootPath, "Schemas", "products.dtd");
            if (!System.IO.File.Exists(dtdPath))
            {
                TempData["Error"] = "DTD file not found.";
                return RedirectToAction("ProductIndex");
            }

            string validationErrors = "";

            XmlReaderSettings settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse,
                ValidationType = ValidationType.DTD,
                XmlResolver = new XmlUrlResolver()
            };

            settings.ValidationEventHandler += (sender, e) =>
            {
                validationErrors += e.Message + Environment.NewLine;
            };

            List<Product> products = new List<Product>();

            using (XmlReader reader = XmlReader.Create(tempFilePath, settings))
            {
                XDocument xmlDoc = XDocument.Load(reader);

                if (!string.IsNullOrWhiteSpace(validationErrors))
                {
                    TempData["Error"] = "XML Validation errors:\n" + validationErrors;
                    return RedirectToAction("ProductIndex");
                }

                products = xmlDoc.Descendants("Product").Select(p => new Product
                {
                    ProductId = int.Parse(p.Element("ProductId")?.Value ?? "0"),
                    ProductName = p.Element("ProductName")?.Value,
                    Description = p.Element("Description")?.Value,
                    BasePrice = decimal.Parse(p.Element("BasePrice")?.Value ?? "0"),
                    Stock = int.Parse(p.Element("Stock")?.Value ?? "0"),
                    ImageUrl = p.Element("ImageUrl")?.Value,
                    Status = p.Element("Status")?.Value,
                    CategoryName = p.Element("CategoryName")?.Value
                }).ToList();
            }

            ViewBag.XmlProducts = products;
            TempData["Message"] = "XML file imported and validated successfully.";

            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }

            return View("ProductIndex", products);
        }

        // Read products from XML file
        private List<Product> ReadProductsFromXML()
        {
            if (!System.IO.File.Exists(_xmlFilePath))
                return new List<Product>();

            XDocument xmlDoc = XDocument.Load(_xmlFilePath);
            return xmlDoc.Descendants("Product").Select(p => new Product
            {
                ProductId = int.Parse(p.Element("ProductId")?.Value ?? "0"),
                ProductName = p.Element("ProductName")?.Value,
                Description = p.Element("Description")?.Value,
                BasePrice = decimal.Parse(p.Element("BasePrice")?.Value ?? "0"),
                Stock = int.Parse(p.Element("Stock")?.Value ?? "0"),
                ImageUrl = p.Element("ImageUrl")?.Value,
                Status = p.Element("Status")?.Value,
                CategoryName = p.Element("CategoryName")?.Value
            }).ToList();
        }

        // Write products 
        private void WriteProductsToXML(List<Product> products)
        {
            XDocument xmlDoc = new XDocument(
                new XDocumentType("Products", null, "products.dtd", null),
                new XElement("Products",
                    products.Select(p => new XElement("Product",
                        new XElement("ProductId", p.ProductId),
                        new XElement("ProductName", p.ProductName),
                        new XElement("Description", p.Description),
                        new XElement("BasePrice", p.BasePrice),
                        new XElement("Stock", p.Stock),
                        new XElement("ImageUrl", p.ImageUrl),
                        new XElement("Status", p.Status),
                        new XElement("CategoryName", p.CategoryName)
                    ))
                )
            );

            xmlDoc.Save(_xmlFilePath);
        }
    }
}