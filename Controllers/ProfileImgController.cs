using Microsoft.AspNetCore.Mvc;
using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace SaccoShareManagementSys.Controllers
{
    [Authorize]
    public class ProfileImgController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public ProfileImgController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]            // temporary for testing
        [HttpGet]
        public IActionResult Test()
        {
            return Content("ProfileImg controller reachable");
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> Upload(ProfileImg model)
        //{
        //    if (model.UploadImage != null && model.UploadImage.Length > 0)
        //    {
        //        var user = await _userManager.GetUserAsync(User);

        //        using var ms = new MemoryStream();
        //        await model.UploadImage.CopyToAsync(ms);

        //        model.ImageData = ms.ToArray();
        //        model.FileName = model.UploadImage.FileName;
        //        model.UserId = user!.Id;   // IMPORTANT LINE

        //        _context.ProfileImg.Add(model);
        //        await _context.SaveChangesAsync();

        //        return RedirectToAction("Index", "Home");
        //    }

        //    return View(model);
        //}
        [HttpPost]
        public async Task<IActionResult> Upload(ProfileImg model)
        {
            if (model.UploadImage == null)
            {
                return Content("UploadImage is NULL");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Content("User NOT logged in");
            }

            using var ms = new MemoryStream();
            await model.UploadImage.CopyToAsync(ms);

            model.ImageData = ms.ToArray();
            model.FileName = model.UploadImage.FileName;
            model.UserId = user.Id;

            // Check if the user already has an image
            var existing = _context.ProfileImg.FirstOrDefault(p => p.UserId == user.Id);
            if (existing != null)
            {
                // Update existing image
                existing.ImageData = model.ImageData;
                existing.FileName = model.FileName;
                _context.Update(existing);
            }
            else
            {
                // Add new image
                _context.ProfileImg.Add(model);
            }

            await _context.SaveChangesAsync();

            // Redirect to dashboard so sidebar reloads
            return RedirectToAction("Index", "Home");
        }



        // GET: ProfileImg/ViewImage
        public async Task<IActionResult> ViewImage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var img = _context.ProfileImg.FirstOrDefault(p => p.UserId == user.Id);
            if (img == null)
                return Content("No image found");

            // You can return a view and pass the model
            return View(img);
        }

        // Optionally: Show the image as file
        public IActionResult ShowImage(int id)
        {
            var img = _context.ProfileImg.FirstOrDefault(p => p.Id == id);
            if (img == null) return NotFound();

            return File(img.ImageData!, "image/jpeg");
        }
    }
}