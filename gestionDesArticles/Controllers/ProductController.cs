using gestionDesArticles.Models;
using gestionDesArticles.Models.Repositories;
using gestionDesArticles.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gestionDesArticles.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ProductController : Controller
    {
        readonly IProductRepository Products;
        readonly ICategorieRepository Categories;
        readonly IWebHostEnvironment env;

        public ProductController(
            IProductRepository products,
            ICategorieRepository categories,
            IWebHostEnvironment env)
        {
            Products = products;
            Categories = categories;
            this.env = env;
        }

        // GET: Product
        [AllowAnonymous]
        public IActionResult Index(int? categoryId, int page = 1)
        {
            int pageSize = 4; // Nombre de produits par page

            var categories = Categories.GetAll();

            // Passer les catégories à la vue
            ViewData["Categories"] = categories;

            // Récupérer les produits en fonction de categoryId, s'il est spécifié
            IQueryable<Product> productsQuery = Products.GetAllProducts();

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
            }

            // Pagination
            var totalProducts = productsQuery.Count();
            var products = productsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.CategoryId = categoryId; // Passer categoryId à la vue

            return View(products);
        }

        // GET: Product/Search?val=xxx
        [AllowAnonymous]
        public IActionResult Search(string val)
        {
            ViewData["Categories"] = Categories.GetAll();
            return View("Index", Products.FindByName(val));
        }

        // GET: Product/Details/5
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            ViewData["Categories"] = Categories.GetAll();
            var product = Products.GetById(id);

            return product == null
                ? NotFound()
                : View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            ViewData["Categories"] = Categories.GetAll();
            CategoryList();
            return View(new CreateViewModel());
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateViewModel model)
        {
            if (model.ImagePath == null)
            {
                ModelState.AddModelError(
                    "ImagePath",
                    "L'image est obligatoire.");
            }

            if (!ModelState.IsValid)
            {
                CategoryList();
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                QteStock = model.QteStock,
                CategoryId = model.CategoryId,
                Image = SaveImage(model.ImagePath!)
            };

            Products.Add(product);

            return RedirectToAction(
                nameof(Details),
                new { id = product.ProductId });
        }

        // GET: Product/Edit/5
        public IActionResult Edit(int id)
        {
            ViewData["Categories"] = Categories.GetAll();
            var product = Products.GetById(id);

            if (product == null)
                return NotFound();

            CategoryList();

            var model = new EditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                QteStock = product.QteStock,
                CategoryId = product.CategoryId,
                ExistingImagePath = product.Image
            };

            return View(model);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditViewModel model)
        {
            var product = Products.GetById(model.ProductId);

            if (product == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                CategoryList();
                return View(model);
            }

            product.Name = model.Name;
            product.Price = model.Price;
            product.QteStock = model.QteStock;
            product.CategoryId = model.CategoryId;

            if (model.ImagePath != null)
            {
                DeleteImage(product.Image);
                product.Image = SaveImage(model.ImagePath);
            }

            Products.Update(product);

            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Delete/5
        public IActionResult Delete(int id)
        {
            ViewData["Categories"] = Categories.GetAll();
            var product = Products.GetById(id);

            return product == null
                ? NotFound()
                : View(product);
        }

        // POST: Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, Product product)
        {
            var p = Products.GetById(id);

            if (p != null)
            {
                DeleteImage(p.Image);
            }

            Products.Delete(id);

            return RedirectToAction(nameof(Index));
        }

        // Populate category dropdown
        private void CategoryList()
        {
            ViewBag.CategoryId = new SelectList(
                Categories.GetAll(),
                "CategoryId",
                "CategoryName");
        }

        // Save uploaded image
        private string SaveImage(IFormFile file)
        {
            string folder = Path.Combine(
                env.WebRootPath,
                "images");

            Directory.CreateDirectory(folder);

            string fileName =
                Guid.NewGuid() + "_" + file.FileName;

            string filePath =
                Path.Combine(folder, fileName);

            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return fileName;
        }

        // Delete uploaded image
        private void DeleteImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            string path = Path.Combine(
                env.WebRootPath,
                "images",
                fileName);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
 