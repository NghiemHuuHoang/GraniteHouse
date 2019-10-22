using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models;
using GraniteHouse.Models.ModelViews;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteHouse.Areas.Admin.Controllers
{   
    [Authorize(Roles=SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class ProductsController : Controller
    {   

        private readonly ApplicationDbContext _db;
        public readonly HostingEnvironment _hostingEnvironment;

        [BindProperty]
        public ProductViewModel ProductViewModel { get; set; }

        public ProductsController(ApplicationDbContext db,HostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            ProductViewModel = new ProductViewModel()
            {
                ProductTypes = _db.ProductTypes.ToList(),
                SpecialTags = _db.SpecialTags.ToList(),
                Products = new Products()
            };
            
        }
        public async Task<IActionResult> Index()
        {
            var product = _db.Products.Include(m => m.ProductTypes).Include(m=>m.SpecialTag);
            return View(await product.ToListAsync());
        }

        public IActionResult Create()
        {
            return View(ProductViewModel);
        }

        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            if (!ModelState.IsValid)
            {
                return View(ProductViewModel);
             
            }
           _db.Products.Add(ProductViewModel.Products);
            await _db.SaveChangesAsync();

            //image being saved
            string Webrootpath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var productFromDb = _db.Products.Find(ProductViewModel.Products.Id);

            if (files.Count!=0)
            {
                //image has been uploaded
                var uploads = Path.Combine(Webrootpath, SD.ImageFolder);
                var extension = Path.GetExtension(files[0].FileName);
                using (var filestream=new FileStream(Path.Combine(uploads,ProductViewModel.Products.Id+extension),FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                    
                }
                productFromDb.Image = @"\" + SD.ImageFolder + @"\" + ProductViewModel.Products.Id + extension;
            }
            else
            {
                var uploads = Path.Combine(Webrootpath,SD.ImageFolder+ @"\" +SD.DefaultProductImage);
                System.IO.File.Copy(uploads,Webrootpath+@"\"+SD.ImageFolder+@"\"+ProductViewModel.Products.Id+".jpg");
                productFromDb.Image = @"\" + SD.ImageFolder + @"\" + ProductViewModel.Products.Id + ".jpg";
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ProductViewModel.Products =await _db.Products.Include(m => m.SpecialTag).Include(m => m.ProductTypes).SingleOrDefaultAsync(m => m.Id == id);
            if (ProductViewModel.Products==null)
            {
                return NotFound();
            }
            return View(ProductViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                var productFormDb = _db.Products.Where(m => m.Id == ProductViewModel.Products.Id).FirstOrDefault();

                if (files.Count!=0 && files[0]!=null)
                {
                    var uploads = Path.Combine(webRootPath,SD.ImageFolder);
                    var extensions_new = Path.GetExtension(files[0].FileName);
                    var extensions_old = Path.GetExtension(productFormDb.Image);

                    if (System.IO.File.Exists(Path.Combine(uploads, ProductViewModel.Products.Id + extensions_old)))
                    {
                        System.IO.File.Delete(Path.Combine(uploads, ProductViewModel.Products.Id + extensions_old));
                    }
                    using (var filestream = new FileStream(Path.Combine(uploads, ProductViewModel.Products.Id + extensions_new), FileMode.Create))
                    {
                        files[0].CopyTo(filestream);

                    }
                    ProductViewModel.Products.Image = @"\" + SD.ImageFolder + @"\" + ProductViewModel.Products.Id + extensions_new;
                }
                if (ProductViewModel.Products.Image != null)
                {
                    productFormDb.Image = ProductViewModel.Products.Image;
                }
                productFormDb.Name = ProductViewModel.Products.Name;
                productFormDb.Price = ProductViewModel.Products.Price;
                productFormDb.Available = ProductViewModel.Products.Available;
                productFormDb.ProductTypeId = ProductViewModel.Products.ProductTypeId;
                productFormDb.SpecialTagId = ProductViewModel.Products.SpecialTagId;
                productFormDb.ShadeColor = ProductViewModel.Products.ShadeColor;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ProductViewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ProductViewModel.Products = await _db.Products.Include(m => m.SpecialTag).Include(m => m.ProductTypes).SingleOrDefaultAsync(m => m.Id == id);
            if (ProductViewModel.Products == null)
            {
                return NotFound();
            }
            return View(ProductViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ProductViewModel.Products = await _db.Products.Include(m => m.SpecialTag).Include(m => m.ProductTypes).SingleOrDefaultAsync(m => m.Id == id);
            if (ProductViewModel.Products == null)
            {
                return NotFound();
            }
            return View(ProductViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            Products products = _db.Products.Find(id);

            if (products==null)
            {
                return NotFound();
            }
            else
            {
                var uploads = Path.Combine(webRootPath,SD.ImageFolder);
                var extensions = Path.GetExtension(products.Image);

                if (System.IO.File.Exists(Path.Combine(uploads,products.Id+extensions)))
                {
                    System.IO.File.Delete(Path.Combine(uploads,products.Id+extensions));
                }
                _db.Products.Remove(products);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}