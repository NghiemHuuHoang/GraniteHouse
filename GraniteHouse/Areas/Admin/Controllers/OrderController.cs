using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models;
using GraniteHouse.Models.ModelViews;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteHouse.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.AdminEndUser + "," + SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private int PageSize = 3;

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index(int productPage = 1, string searchName = null, string searchPhone = null, string searchDate = null, string searchEmail = null)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            OrderViewModel orderViewModel = new OrderViewModel()
            {
                Orders = new List<Models.Order>()
            };

            StringBuilder param=new StringBuilder();
            param.Append("/Admin/Order?productPage=:");
            param.Append("&searchName=");
            if (searchName!=null)
            {
                param.Append(searchName);
            }

            param.Append("&searchEmail=");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }

            param.Append("&searchPhone=");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }

            param.Append("&searchDate=");
            if (searchDate != null)
            {
                param.Append(searchDate);
            }

            orderViewModel.Orders = _db.Orders.Include(a => a.User).ToList();
            if (User.IsInRole(SD.AdminEndUser))
            {
                orderViewModel.Orders = orderViewModel.Orders.Where(p => p.UserNameId == claim.Value).ToList();
            }
            if (searchName != null)
            {
                orderViewModel.Orders = orderViewModel.Orders.Where(a => a.CustomerName.ToLower().Contains(searchName.ToLower())).ToList();
            }
            if (searchPhone != null)
            {
                orderViewModel.Orders = orderViewModel.Orders.Where(a => a.CustomerPhone.ToLower().Contains(searchPhone.ToLower())).ToList();
            }

            if (searchDate != null)
            {
                try
                {
                    DateTime date = Convert.ToDateTime(searchDate);
                    orderViewModel.Orders = orderViewModel.Orders.Where(a => a.OrderDate.ToShortDateString().Equals(date.ToShortDateString())).ToList();

                }
                catch (Exception ex)
                {
                    Console.Write("Not found"+ex);
                }
            }

            var count = orderViewModel.Orders.Count;
            orderViewModel.Orders = orderViewModel.Orders.OrderBy(p => p.OrderDate)
                                    .Skip((productPage - 1) * PageSize)
                                    .Take(PageSize).ToList();
            orderViewModel.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemPerPage = PageSize,
                TotalItems = count,
               
                urlParameter = param.ToString()
            };

            if (searchEmail != null)
            {
                orderViewModel.Orders = orderViewModel.Orders.Where(a => a.Customermail.ToLower().Contains(searchEmail.ToLower())).ToList();
            }
            return View(orderViewModel);
        }

        //Get Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var products = (IEnumerable<Products>)(from p in _db.Products
                                                   join a in _db.DetailsOrders
                                                   on p.Id equals a.ProductId
                                                   where a.OrderId == id
                                                   select p).Include("ProductTypes").Include("SpecialTag");

            DetailsOrderViewModel objDetailsOrderViewModel = new DetailsOrderViewModel()
            {
                Order = _db.Orders.Include(m => m.User).Where(a => a.Id == id).FirstOrDefault(),
                Users = _db.AuthenticationUsers.ToList(),
                Products = products.ToList()
            };
            return View(objDetailsOrderViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DetailsOrderViewModel detailsOrderViewModel)
        {

            if (ModelState.IsValid)
            {
                detailsOrderViewModel.Order.OrderDate = detailsOrderViewModel.Order.OrderDate
                                                      .AddHours(detailsOrderViewModel.Order.OrderTime.Hour)
                                                      .AddMinutes(detailsOrderViewModel.Order.OrderTime.Minute);
                var orderDb = _db.Orders.Where(m => m.Id == detailsOrderViewModel.Order.Id).FirstOrDefault();
                orderDb.CustomerName = detailsOrderViewModel.Order.CustomerName;
                if (User.IsInRole(SD.SuperAdminEndUser))
                {
                    orderDb.UserNameId = detailsOrderViewModel.Order.UserNameId;
                }
                orderDb.CustomerPhone = detailsOrderViewModel.Order.CustomerPhone;
                orderDb.Customermail = detailsOrderViewModel.Order.Customermail;
                orderDb.OrderDate = detailsOrderViewModel.Order.OrderDate;
                orderDb.IsConfirmed = detailsOrderViewModel.Order.IsConfirmed;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(detailsOrderViewModel);
        }

        //Get Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var products = (IEnumerable<Products>)(from p in _db.Products
                                                   join a in _db.DetailsOrders
                                                   on p.Id equals a.ProductId
                                                   where a.OrderId == id
                                                   select p).Include("ProductTypes").Include("SpecialTag");

            DetailsOrderViewModel objDetailsOrderViewModel = new DetailsOrderViewModel()
            {
                Order = _db.Orders.Include(m => m.User).Where(a => a.Id == id).FirstOrDefault(),
                Users = _db.AuthenticationUsers.ToList(),
                Products = products.ToList()
            };
            return View(objDetailsOrderViewModel);
        }

        //GET Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var products = (IEnumerable<Products>)(from p in _db.Products
                                                   join a in _db.DetailsOrders
                                                   on p.Id equals a.ProductId
                                                   where a.OrderId == id
                                                   select p).Include("ProductTypes").Include("SpecialTag");

            DetailsOrderViewModel objDetailsOrderViewModel = new DetailsOrderViewModel()
            {
                Order = _db.Orders.Include(m => m.User).Where(a => a.Id == id).FirstOrDefault(),
                Users = _db.AuthenticationUsers.ToList(),
                Products = products.ToList()
            };
            return View(objDetailsOrderViewModel);
        }

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            var order = await _db.Orders.FindAsync(id);
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}