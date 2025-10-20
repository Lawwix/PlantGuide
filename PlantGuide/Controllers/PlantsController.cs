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
        public async Task<IActionResult> Index(string search, string careFilter)
        {
            // Нормализуем входные параметры
            search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            careFilter = string.IsNullOrWhiteSpace(careFilter) ? null : careFilter.Trim();

            // Берём IQueryable — запрос будет выполнен в БД после всех Where
            var query = _context.Plants.AsQueryable();

            // Поиск: проверяем несколько полей
            if (!string.IsNullOrEmpty(search))
            {
                // Используем EF.Functions.Like для корректной работы в SQLite/SQL
                var pattern = $"%{search}%";
                query = query.Where(p =>
                    EF.Functions.Like(p.Name ?? "", pattern) ||
                    EF.Functions.Like(p.ScientificName ?? "", pattern) ||
                    EF.Functions.Like(p.Description ?? "", pattern));
            }

            // Фильтрация по инструкциям по уходу (ищем подстроку)
            if (!string.IsNullOrEmpty(careFilter))
            {
                var pattern = $"%{careFilter}%";
                query = query.Where(p => EF.Functions.Like(p.CareInstructions ?? "", pattern));
            }

            // Сортируем и выполняем запрос
            var list = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plant plant, IFormFile? photo)
        {
            // сохраняем фото (если есть) в wwwroot/images и возвращаем путь вида "/images/xxx.jpg"
            if (photo != null && photo.Length > 0)
            {
                plant.PhotoPath = await SavePhotoFile(photo);
            }
            else
            {
                plant.PhotoPath = "/images/default.jpg";
            }

            // если в модели PhotoPath помечено [Required], снимем старую ошибку валидации
            ModelState.Remove(nameof(Plant.PhotoPath));
            TryValidateModel(plant);

            if (!ModelState.IsValid)
            {
                // для отладки можно вывести ModelState ошибки в Output
                var errors = ModelState.Where(kv => kv.Value.Errors.Any())
                    .Select(kv => new { kv.Key, Errors = kv.Value.Errors.Select(e => e.ErrorMessage) });
                foreach (var e in errors) Console.WriteLine($"Model error: {e.Key} => {string.Join(",", e.Errors)}");

                return View(plant);
            }

            _context.Add(plant); _context.Add(plant); 
            await _context.SaveChangesAsync(); 
            Console.WriteLine("Saved plant id=" + plant.Id); 
            return RedirectToAction(nameof(Index));
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

            // Загружаем текущую запись из БД
            var existing = await _context.Plants.FindAsync(id);
            if (existing == null) return NotFound();

            // Обновляем поля (которые редактирует пользователь)
            existing.Name = plant.Name;
            existing.ScientificName = plant.ScientificName;
            existing.Description = plant.Description;
            existing.CareInstructions = plant.CareInstructions;

            // Если пользователь загрузил новое фото — сохраняем и подставляем путь,
            // иначе оставляем существующий existing.PhotoPath
            if (photo != null && photo.Length > 0)
            {
                existing.PhotoPath = await SavePhotoFile(photo); // см. ниже
            }

            // Если PhotoPath у тебя помечен [Required], удалить старую валидацию:
            ModelState.Remove(nameof(Plant.PhotoPath));
            // Перепроверяем модель на валидность
            TryValidateModel(existing);

            if (!ModelState.IsValid)
            {
                return View(existing); // возвращаем форму с ошибками
            }

            try
            {
                _context.Update(existing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Plants.Any(e => e.Id == id)) return NotFound();
                throw;
            }
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

        // helper to save uploaded images to wwwroot/images and return path like "~/images/xxxx.jpg"
        private async Task<string> SavePhotoFile(IFormFile photo)
        {
            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "images");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            // return path with tilde so Url.Content can resolve it
            return $"~/images/{fileName}";
        }
    }
}
