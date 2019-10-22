using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GraniteHouse.Models;
using GraniteHouse.Data;
using Microsoft.EntityFrameworkCore;
using GraniteHouse.Extensions;
using Microsoft.AspNetCore.Http;

namespace GraniteHouse.Controllers
{   
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var prodcutList = await _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTag).ToListAsync();
            return View(prodcutList);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTag).Where(m=>m.Id==id).FirstOrDefaultAsync();

            return View(product);
        }

        [HttpPost,ActionName("Details")]
        public async Task<IActionResult> DetailsPost(int id)
        {
            List<int> listShoppingCart = HttpContext.Session.Get<List<int>>("ShoppingCart");
            if (listShoppingCart==null)
            {
                listShoppingCart = new List<int>();
            }
            listShoppingCart.Add(id);
            HttpContext.Session.Set("ShoppingCart", listShoppingCart);
            return RedirectToAction("Index","Home",new {area= "Customer"});
        }

        public async Task<IActionResult> Remove(int id)
        {
            List<int> isList = HttpContext.Session.Get<List<int>>("ShoppingCart");
            if (isList.Count>0)
            {
                if (isList.Contains(id))
                {
                    isList.Remove(id);
                }
            }
            HttpContext.Session.Set("ShoppingCart",isList);
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
