using Microsoft.AspNetCore.Mvc;
using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.Data;

namespace SaccoShareManagementSys.Controllers
{
    public class ProfileImgController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileImgController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(ProfileImg model)
        {
            if (model.UploadImage != null && model.UploadImage.Length > 0)
            {
                using var ms = new MemoryStream();
                await model.UploadImage.CopyToAsync(ms);
                model.ImageData = ms.ToArray();
                model.FileName = model.UploadImage.FileName;

                _context.ProfileImg.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("ViewImage", new { id = model.Id });
            }

            return View(model);
        }

        public IActionResult ViewImage(int id)
        {
            var image = _context.ProfileImg.FirstOrDefault(p => p.Id == id);
            if (image == null)
                return NotFound();

            return View(image);
        }

        public IActionResult ShowImage(int id)
        {
            var image = _context.ProfileImg.Find(id);
            if (image?.ImageData == null)
                return NotFound();

            return File(image.ImageData, "image/jpeg");
        }
    }
}