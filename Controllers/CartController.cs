using System.Data;
using Endterm_IPT.DataAccess;
using Endterm_IPT.Models;
using Microsoft.AspNetCore.Mvc;

namespace Endterm_IPT.Controllers
{
    public class CartController : Controller
    {
        private readonly DatabaseHelper _databaseHelper;

        public CartController()
        {
            string connString = "Server=localhost;Database=onlinestore;User Id=root;Password='';";
            _databaseHelper = new DatabaseHelper(connString);
        }

        [HttpPost]
        public IActionResult Add([FromBody] AddToCartRequest request)
        {
            if (!AuthController.IsUserAuthenticated(HttpContext))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, redirectToLogin = true, redirectUrl = "/Auth/Login" });
                }
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                string userId = HttpContext.Session.GetString("UserId");

                string productQuery = $"SELECT Stock FROM Products WHERE ProductId = {request.ProductId}";
                DataTable productData = _databaseHelper.SelectQuery(productQuery);

                if (productData.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                int availableStock = Convert.ToInt32(productData.Rows[0]["Stock"]);
                if (availableStock < request.Quantity)
                {
                    return Json(new { success = false, message = "Insufficient stock available" });
                }

                string priceQuery = $"SELECT BasePrice FROM Products WHERE ProductId = {request.ProductId}";
                DataTable priceData = _databaseHelper.SelectQuery(priceQuery);
                decimal productPrice = Convert.ToDecimal(priceData.Rows[0]["BasePrice"]);

                string pendingOrderQuery = $"SELECT OrderID FROM checkout WHERE UserID = {userId} AND Status = 'pending'";
                DataTable pendingOrderData = _databaseHelper.SelectQuery(pendingOrderQuery);

                int orderId;
                if (pendingOrderData.Rows.Count > 0)
                {
                    orderId = Convert.ToInt32(pendingOrderData.Rows[0]["OrderID"]);
                }
                else
                {
                    string createOrderQuery = $"INSERT INTO checkout (UserID, OrderDate, Status, TotalAmount) VALUES ({userId}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 'pending', 0)";
                    _databaseHelper.ExecuteQuery(createOrderQuery);

                    string getOrderIdQuery = $"SELECT LAST_INSERT_ID() as OrderID";
                    DataTable orderIdData = _databaseHelper.SelectQuery(getOrderIdQuery);
                    orderId = Convert.ToInt32(orderIdData.Rows[0]["OrderID"]);
                }

                string checkCartQuery = $"SELECT Quantity FROM Cart WHERE OrderID = {orderId} AND ProductID = {request.ProductId}";
                DataTable cartData = _databaseHelper.SelectQuery(checkCartQuery);

                if (cartData.Rows.Count > 0)
                {
                    int currentQuantity = Convert.ToInt32(cartData.Rows[0]["Quantity"]);
                    int newQuantity = currentQuantity + request.Quantity;

                    if (newQuantity > availableStock)
                    {
                        return Json(new { success = false, message = "Cannot add more items. Exceeds available stock." });
                    }

                    string updateQuery = $"UPDATE Cart SET Quantity = {newQuantity}, PriceAtPurchase = {productPrice} WHERE OrderID = {orderId} AND ProductID = {request.ProductId}";
                    _databaseHelper.ExecuteQuery(updateQuery);
                }
                else
                {
                    string insertQuery = $"INSERT INTO Cart (OrderID, ProductID, Quantity, PriceAtPurchase) VALUES ({orderId}, {request.ProductId}, {request.Quantity}, {productPrice})";
                    _databaseHelper.ExecuteQuery(insertQuery);
                }

                string updateStockQuery = $"UPDATE Products SET Stock = Stock - {request.Quantity} WHERE ProductId = {request.ProductId}";
                _databaseHelper.ExecuteQuery(updateStockQuery);

                string updateTotalQuery = $@"
            UPDATE checkout 
            SET TotalAmount = (
                SELECT SUM(Quantity * PriceAtPurchase) 
                FROM Cart 
                WHERE OrderID = {orderId}
            ) 
            WHERE OrderID = {orderId}";
                _databaseHelper.ExecuteQuery(updateTotalQuery);

                string cartCountQuery = $"SELECT SUM(Quantity) as TotalItems FROM Cart WHERE OrderID = {orderId}";
                DataTable cartCountData = _databaseHelper.SelectQuery(cartCountQuery);
                int cartCount = cartCountData.Rows.Count > 0 && cartCountData.Rows[0]["TotalItems"] != DBNull.Value
                    ? Convert.ToInt32(cartCountData.Rows[0]["TotalItems"]) : 0;

                return Json(new { success = true, message = "Item added to cart successfully", cartCount = cartCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to cart: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"An error occurred while adding item to cart: {ex.Message}" });
            }
        }
        [HttpGet]
        public IActionResult Index()
        {
            if (!AuthController.IsUserAuthenticated(HttpContext))
            {
                return RedirectToAction("Login", "Auth");
            }

            string userId = HttpContext.Session.GetString("UserId");

            string pendingOrderQuery = $"SELECT OrderID FROM checkout WHERE UserID = {userId} AND Status = 'pending'";
            DataTable pendingOrderData = _databaseHelper.SelectQuery(pendingOrderQuery);

            if (pendingOrderData.Rows.Count == 0)
            {
                ViewBag.CartTotal = 0;
                return View(new List<CartItem>());
            }

            int orderId = Convert.ToInt32(pendingOrderData.Rows[0]["OrderID"]);

            string query = $@"
                SELECT c.OrderID, c.ProductID, c.Quantity, c.PriceAtPurchase, 
                       p.ProductName, p.ImageUrl, p.ImageUrl, p.Stock
                FROM Cart c
                INNER JOIN Products p ON c.ProductID = p.ProductId
                WHERE c.OrderID = {orderId}";

            DataTable cartData = _databaseHelper.SelectQuery(query);
            var cartItems = new List<CartItem>();
            decimal cartTotal = 0;

            foreach (DataRow row in cartData.Rows)
            {
                var cartItem = new CartItem
                {
                    CartID = Convert.ToInt32(row["OrderID"]),
                    ProductId = Convert.ToInt32(row["ProductID"]),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    ProductName = row["ProductName"].ToString(),
                    BasePrice = Convert.ToDecimal(row["PriceAtPurchase"]),
                    ImageUrl = row["ImageUrl"].ToString(),
                    ImageFile = "",
                    Stock = Convert.ToInt32(row["Stock"])
                };
                cartItems.Add(cartItem);
                cartTotal += cartItem.BasePrice * cartItem.Quantity;
            }

            ViewBag.CartTotal = cartTotal;
            return View(cartItems);
        }

        public IActionResult Remove(int productId)
        {
            if (!AuthController.IsUserAuthenticated(HttpContext))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            try
            {
                string userId = HttpContext.Session.GetString("UserId");

                string pendingOrderQuery = $"SELECT OrderID FROM checkout WHERE UserID = {userId} AND Status = 'pending'";
                DataTable pendingOrderData = _databaseHelper.SelectQuery(pendingOrderQuery);

                if (pendingOrderData.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "No cart found" });
                }

                int orderId = Convert.ToInt32(pendingOrderData.Rows[0]["OrderID"]);

                string getQuantityQuery = $"SELECT Quantity FROM Cart WHERE OrderID = {orderId} AND ProductID = {productId}";
                DataTable quantityData = _databaseHelper.SelectQuery(getQuantityQuery);

                if (quantityData.Rows.Count > 0)
                {
                    int removedQuantity = Convert.ToInt32(quantityData.Rows[0]["Quantity"]);

                    string restoreStockQuery = $"UPDATE Products SET Stock = Stock + {removedQuantity} WHERE ProductId = {productId}";
                    _databaseHelper.ExecuteQuery(restoreStockQuery);
                }

                string removeQuery = $"DELETE FROM Cart WHERE OrderID = {orderId} AND ProductID = {productId}";
                _databaseHelper.ExecuteQuery(removeQuery);

                string updateTotalQuery = $@"
            UPDATE checkout 
            SET TotalAmount = COALESCE((
                SELECT SUM(Quantity * PriceAtPurchase) 
                FROM Cart 
                WHERE OrderID = {orderId}
            ), 0) 
            WHERE OrderID = {orderId}";
                _databaseHelper.ExecuteQuery(updateTotalQuery);

                // Get updated cart count
                string cartCountQuery = $"SELECT SUM(Quantity) as TotalItems FROM Cart WHERE OrderID = {orderId}";
                DataTable cartCountData = _databaseHelper.SelectQuery(cartCountQuery);
                int cartCount = cartCountData.Rows.Count > 0 && cartCountData.Rows[0]["TotalItems"] != DBNull.Value
                    ? Convert.ToInt32(cartCountData.Rows[0]["TotalItems"]) : 0;

                string totalQuery = $"SELECT TotalAmount FROM checkout WHERE OrderID = {orderId}";
                DataTable totalData = _databaseHelper.SelectQuery(totalQuery);
                decimal cartTotal = totalData.Rows.Count > 0 ? Convert.ToDecimal(totalData.Rows[0]["TotalAmount"]) : 0;

                return Json(new { success = true, message = "Item removed from cart", cartCount = cartCount, cartTotal = cartTotal });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from cart: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while removing item from cart" });
            }
        }


        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (!AuthController.IsUserAuthenticated(HttpContext))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            try
            {
                string userId = HttpContext.Session.GetString("UserId");

                string pendingOrderQuery = $"SELECT OrderID FROM checkout WHERE UserID = {userId} AND Status = 'pending'";
                DataTable pendingOrderData = _databaseHelper.SelectQuery(pendingOrderQuery);

                if (pendingOrderData.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "No cart found" });
                }

                int orderId = Convert.ToInt32(pendingOrderData.Rows[0]["OrderID"]);

                string currentQuantityQuery = $"SELECT Quantity FROM Cart WHERE OrderID = {orderId} AND ProductID = {productId}";
                DataTable currentQuantityData = _databaseHelper.SelectQuery(currentQuantityQuery);

                if (currentQuantityData.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "Item not found in cart" });
                }

                int currentCartQuantity = Convert.ToInt32(currentQuantityData.Rows[0]["Quantity"]);

                string stockQuery = $"SELECT Stock FROM Products WHERE ProductId = {productId}";
                DataTable stockData = _databaseHelper.SelectQuery(stockQuery);

                if (stockData.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                int currentStock = Convert.ToInt32(stockData.Rows[0]["Stock"]);
                int totalAvailable = currentStock + currentCartQuantity; 
                if (quantity > totalAvailable)
                {
                    return Json(new { success = false, message = "Insufficient stock available" });
                }

                int stockAdjustment = currentCartQuantity - quantity; 
                string updateStockQuery = $"UPDATE Products SET Stock = Stock + {stockAdjustment} WHERE ProductId = {productId}";
                _databaseHelper.ExecuteQuery(updateStockQuery);

                string updateQuery = $"UPDATE Cart SET Quantity = {quantity} WHERE OrderID = {orderId} AND ProductID = {productId}";
                _databaseHelper.ExecuteQuery(updateQuery);

                string updateTotalQuery = $@"
            UPDATE checkout 
            SET TotalAmount = (
                SELECT SUM(Quantity * PriceAtPurchase) 
                FROM Cart 
                WHERE OrderID = {orderId}
            ) 
            WHERE OrderID = {orderId}";
                _databaseHelper.ExecuteQuery(updateTotalQuery);

                string cartCountQuery = $"SELECT SUM(Quantity) as TotalItems FROM Cart WHERE OrderID = {orderId}";
                DataTable cartCountData = _databaseHelper.SelectQuery(cartCountQuery);
                int cartCount = cartCountData.Rows.Count > 0 && cartCountData.Rows[0]["TotalItems"] != DBNull.Value
                    ? Convert.ToInt32(cartCountData.Rows[0]["TotalItems"]) : 0;

                string totalQuery = $"SELECT TotalAmount FROM checkout WHERE OrderID = {orderId}";
                DataTable totalData = _databaseHelper.SelectQuery(totalQuery);
                decimal cartTotal = totalData.Rows.Count > 0 ? Convert.ToDecimal(totalData.Rows[0]["TotalAmount"]) : 0;

                return Json(new { success = true, message = "Quantity updated successfully", cartCount = cartCount, cartTotal = cartTotal });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating quantity: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while updating quantity" });
            }
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            if (!AuthController.IsUserAuthenticated(HttpContext))
            {
                return Json(new { cartCount = 0 });
            }

            try
            {
                string userId = HttpContext.Session.GetString("UserId");

                string pendingOrderQuery = $"SELECT OrderID FROM checkout WHERE UserID = {userId} AND Status = 'pending'";
                DataTable pendingOrderData = _databaseHelper.SelectQuery(pendingOrderQuery);

                if (pendingOrderData.Rows.Count == 0)
                {
                    return Json(new { cartCount = 0 });
                }

                int orderId = Convert.ToInt32(pendingOrderData.Rows[0]["OrderID"]);

                string cartCountQuery = $"SELECT SUM(Quantity) as TotalItems FROM Cart WHERE OrderID = {orderId}";
                DataTable cartCountData = _databaseHelper.SelectQuery(cartCountQuery);
                int cartCount = cartCountData.Rows.Count > 0 && cartCountData.Rows[0]["TotalItems"] != DBNull.Value
                    ? Convert.ToInt32(cartCountData.Rows[0]["TotalItems"]) : 0;

                return Json(new { cartCount = cartCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cart count: {ex.Message}");
                return Json(new { cartCount = 0 });
            }
        }
    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartItem
    {
        public int CartID { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public decimal BasePrice { get; set; }
        public string ImageUrl { get; set; }
        public string ImageFile { get; set; }
        public int Stock { get; set; }
    }
}