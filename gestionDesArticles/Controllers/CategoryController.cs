using gestionDesArticles.Models;
using gestionDesArticles.Models.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace gestionDesArticles.Controllers
{
    public class CategoryController : Controller
    {
        readonly ICategorieRepository Categories;

        public CategoryController(ICategorieRepository categories)
        {
            Categories = categories;
        }

        // GET: Category
        public IActionResult Index() => View(Categories.GetAll());

        // GET: Category/Details/5
        public IActionResult Details(int id)
        {
            var category = Categories.GetById(id);
            return category == null ? NotFound() : View(category);
        }

        // GET: Category/Create
        public IActionResult Create() => View();

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid) return View(category);
            Categories.Add(category);
            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Edit/5
        public IActionResult Edit(int id)
        {
            var category = Categories.GetById(id);
            return category == null ? NotFound() : View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (!ModelState.IsValid) return View(category);
            Categories.Update(category);
            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Delete/5
        public IActionResult Delete(int id)
        {
            var category = Categories.GetById(id);
            return category == null ? NotFound() : View(category);
        }

        // POST: Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, Category category)
        {
            Categories.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
