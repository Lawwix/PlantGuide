using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantGuide.Data;
using PlantGuide.Models;

namespace PlantGuide.Controllers
{
    public class PlantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PlantsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Plants
        // support search and simple filter by care keyword
        public async Task<IActionResult> Index(string search, string careFilter)
        {
            var query = _context.Plants.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    (p.ScientificName ?? "").ToLower().Contains(search) ||
                    (p.Description ?? "").ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(careFilter))
            {
                var f = careFilter.Trim().ToLower();
                query = query.Where(p => (p.CareInstructions ?? "").ToLower().Contains(f));
            }

            var list = await query.OrderBy(p => p.Name).ToListAsync();
            ViewData["Search"] = search;
            ViewData["CareFilter"] = careFilter;
            return View(list);
        }

        // GET: Plants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var plant = await _context.Plants.FirstOrDefaultAsync(p => p.Id == id);
            if (plant == null) return NotFound();
            return View(plant);
        }

        // GET: Plants/Create
        public IActionResult Create() => View();

        // POST: Plants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plant plant, IFormFile? photo)
        {
            // Сначала присваиваем путь к фото (если есть файл или дефолт)
            if (photo != null && photo.Length > 0)
            {
                plant.PhotoPath = await SavePhotoFile(photo);
            }
            else
            {
                plant.PhotoPath = "/images/default.jpg"; // убедись, что файл существует
            }

            // Удаляем старую валидацию поля PhotoPath (если был [Required])
            ModelState.Remove(nameof(Plant.PhotoPath));
            // Перепроверим модель (необязательно)
            TryValidateModel(plant);

            if (ModelState.IsValid)
            {
                _context.Add(plant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // если ModelState невалиден — вернёмся в форму и покажем ошибки
            return View(plant);
        }

        // GET: Plants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var plant = await _context.Plants.FindAsync(id);
            if (plant == null) return NotFound();
            return View(plant);
        }

        // POST: Plants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Plant plant, IFormFile? photo)
        {
            if (id != plant.Id) return NotFound();

            if (photo != null && photo.Length > 0)
            {
                plant.PhotoPath = await SavePhotoFile(photo);
            }

            // Если PhotoPath был Required — убираем старую ошибку валидации
            ModelState.Remove(nameof(Plant.PhotoPath));
            TryValidateModel(plant);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plant);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlantExists(plant.Id)) return NotFound();
                    throw;
                }
            }

            return View(plant);
        }

        // GET: Plants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var plant = await _context.Plants.FirstOrDefaultAsync(p => p.Id == id);
            if (plant == null) return NotFound();
            return View(plant);
        }

        // POST: Plants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plant = await _context.Plants.FindAsync(id);
            if (plant != null)
            {
                _context.Plants.Remove(plant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PlantExists(int id) => _context.Plants.Any(e => e.Id == id);

        // helper to save uploaded images to wwwroot/images and return relative path
        private async Task<string> SavePhotoFile(IFormFile photo)
        {
            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "images");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            // create unique file name
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // return relative path for use in <img src="...">
            return Path.Combine("images", fileName).Replace("\\", "/");
        }
    }
}


//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using PlantGuide.Data;
//using PlantGuide.Models;

//namespace PlantGuide.Controllers
//{
//    public class PlantsController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public PlantsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IActionResult> Index()
//        {
//            return View(await _context.Plants.ToListAsync());
//        }

//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null) return NotFound();
//            var plant = await _context.Plants.FirstOrDefaultAsync(p => p.Id == id);
//            if (plant == null) return NotFound();
//            return View(plant);
//        }

//        public IActionResult Create() => View();

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Plant plant)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Add(plant);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(plant);
//        }

//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null) return NotFound();
//            var plant = await _context.Plants.FindAsync(id);
//            if (plant == null) return NotFound();
//            return View(plant);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, Plant plant)
//        {
//            if (id != plant.Id) return NotFound();
//            if (ModelState.IsValid)
//            {
//                _context.Update(plant);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(plant);
//        }

//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null) return NotFound();
//            var plant = await _context.Plants.FirstOrDefaultAsync(p => p.Id == id);
//            if (plant == null) return NotFound();
//            return View(plant);
//        }

//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var plant = await _context.Plants.FindAsync(id);
//            _context.Plants.Remove(plant);
//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
