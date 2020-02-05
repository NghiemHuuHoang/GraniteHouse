using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Extensions;
using GraniteHouse.Models;
using GraniteHouse.Models.ModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteHouse.Areas.Customer.Controllers
{   
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public SSShoppingCartViewModel SSShoppingCartVM { get; set; }
        public ShoppingCartController(ApplicationDbContext db)
        {
            _db = db;
            SSShoppingCartVM = new SSShoppingCartViewModel()
            {
                Products = new List<Models.Products>()
            };
        }
        public async Task<IActionResult> Index()
        {
            List<int> listShoppingCart = HttpContext.Session.Get<List<int>>("ShoppingCart");
            if (listShoppingCart == null)
            {
                return View(SSShoppingCartVM);
            }
            if (listShoppingCart.Count > 0)
            {
                foreach (var cartItem in listShoppingCart)
                {
                    Products products = _db.Products.Include(p => p.ProductTypes).Include(p => p.SpecialTag).Where(p => p.Id == cartItem).FirstOrDefault();
                    SSShoppingCartVM.Products.Add(products);
                }
            }
           
            return View(SSShoppingCartVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            if (ModelState.IsValid)
            {
                List<int> listCartItem = HttpContext.Session.Get<List<int>>("ShoppingCart");
                SSShoppingCartVM.Order.OrderDate = SSShoppingCartVM.Order.OrderDate
                                                 .AddHours(SSShoppingCartVM.Order.OrderTime.Hour)
                                                 .AddMinutes(SSShoppingCartVM.Order.OrderTime.Minute);
                Order order = SSShoppingCartVM.Order;
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                int orderId = order.Id;
               

                foreach (int productId in listCartItem)
                {
                    try
                    {
                        
                        
                            


                            //var total = SSShoppingCartVM.Products.Where(a=>a.Id==productId).Sum(c => c.Price);
                            DetailsOrder detailsOrder = new DetailsOrder()
                            {

                                OrderId = orderId,
                                ProductId = productId,
                                //Total = SSShoppingCartVM.Products.Sum(a => a.Price)

                            };
                            _db.DetailsOrders.Add(detailsOrder);
                        }
                        
                    
                    catch (Exception ex)
                    {

                    }

                }
                await _db.SaveChangesAsync();
                listCartItem = new List<int>();
                HttpContext.Session.Set("ShoppingCart", listCartItem);
                return RedirectToAction("OrderConfirmation", "ShoppingCart", new { Id = orderId });
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id)
        {
            List<int> listCartItem = HttpContext.Session.Get<List<int>>("ShoppingCart");
            if (listCartItem.Count>0)
            {
                if (listCartItem.Contains(id))
                {
                    listCartItem.Remove(id);
                }
            }
            HttpContext.Session.Set("ShoppingCart",listCartItem);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            SSShoppingCartVM.Order =await _db.Orders.Where(a => a.Id == id).FirstOrDefaultAsync();
            List<DetailsOrder> objectProductList = _db.DetailsOrders.Where(p => p.OrderId == id).ToList();
            foreach (DetailsOrder productOrder in objectProductList)
            {
                SSShoppingCartVM.Products.Add(await _db.Products.Include(p=>p.ProductTypes).Include(p=>p.SpecialTag).Where(p=>p.Id==productOrder.ProductId).FirstOrDefaultAsync());
            }
            return View(SSShoppingCartVM);
        }
    }
}