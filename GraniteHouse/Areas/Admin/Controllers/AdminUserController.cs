using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteHouse.Areas.Admin.Controllers
{
    [Authorize(Roles=SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class AdminUserController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AdminUserController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View(_db.AuthenticationUsers.ToList());
        }

        public async Task<IActionResult> Edit(string id)
        {
            if(id==null || id.Trim().Length == 0)
            {
                return NotFound();
            }
            var userDb = await _db.AuthenticationUsers.FindAsync(id);
            if (userDb==null)
            {
                return NotFound();
            }
            return View(userDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (string id, AuthenticationUser authenticationUser)
        {
            if (id!=authenticationUser.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                AuthenticationUser UserDb = _db.AuthenticationUsers.Where(u => u.Id == id).FirstOrDefault();
                UserDb.Name = authenticationUser.Name;
                UserDb.PhoneNumber = authenticationUser.PhoneNumber;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(authenticationUser);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || id.Trim().Length == 0)
            {
                return NotFound();
            }
            var userDb = await _db.AuthenticationUsers.FindAsync(id);
            if (userDb == null)
            {
                return NotFound();
            }
            return View(userDb);
        }

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(string id)
        {
            if (id == null || id.Trim().Length==0)
            {
                return NotFound();
            }
            try { 
                AuthenticationUser UserDb = _db.AuthenticationUsers.Where(u => u.Id == id).FirstOrDefault();
                UserDb.LockoutEnd = DateTime.Now.AddYears(1000); 
               
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }

        }
    }
}