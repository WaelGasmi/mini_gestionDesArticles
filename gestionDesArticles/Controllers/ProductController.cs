using gestionDesArticles.Models;
using gestionDesArticles.Models.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gestionDesArticles.Controllers
{
    public class ProductController : Controller
    {
        readonly IProductRepository Products;
        readonly ICategorieRepository Categories;
        readonly IWebHostEnvironment env;

        public ProductController(IProductRepository products,
                                 ICategorieRepository categories,
                                 IWebHostEnvironment env)
        {
            Products = products;
            Categories = categories;
            this.env = env;
        }

        // GET: Product
        public IActionResult Index() => View(Products.GetAll());

        // GET: Product/Search?val=xxx
        public IActionResult Search(string val) => View("Index", Products.FindByName(val));

        // GET: Product/Details/5
        public IActionResult Details(int id)
        {
            var product = Products.GetById(id);
            return product == null ? NotFound() : View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            CategoryList();
            return View(new CreateViewModel());
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateViewModel model)
        {
            if (model.ImagePath == null)
                ModelState.AddModelError("ImagePath", "L'image est obligatoire.");

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
            return RedirectToAction(nameof(Details), new { id = product.ProductId });
        }

        // GET: Product/Edit/5
        public IActionResult Edit(int id)
        {
            var product = Products.GetById(id);
            if (product == null) return NotFound();

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
            if (product == null) return NotFound();

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
            var product = Products.GetById(id);
            return product == null ? NotFound() : View(product);
        }

        // POST: Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, Product product)
        {
            var p = Products.GetById(id);
            if (p != null) DeleteImage(p.Image);
            Products.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        void CategoryList()
        {
            ViewBag.CategoryId = new SelectList(Categories.GetAll(), "CategoryId", "CategoryName");
        }

        string SaveImage(IFormFile file)
        {
            string folder = Path.Combine(env.WebRootPath, "images");
            Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid() + "_" + file.FileName;
            using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                file.CopyTo(stream);
            return fileName;
        }

        void DeleteImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;
            string path = Path.Combine(env.WebRootPath, "images", fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
    }
}
