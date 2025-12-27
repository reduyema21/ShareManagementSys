using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using SaccoShareManagementSys.Services;
using SaccoShareManagementSys.ViewModels;
using System.Security.Claims;

namespace SaccoShareManagementSys.Controllers
{
    [Authorize]
    public class ShareholdersController : Controller
    {
        private readonly IShareholderService _shareholderService;
        private readonly ILogger<ShareholdersController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public ShareholdersController(IShareholderService shareholderService, ILogger<ShareholdersController> logger, UserManager<IdentityUser> userManager)  //inject dep here
        {
            _shareholderService = shareholderService;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Shareholders
        public async Task<IActionResult> Index(ShareholderSearchFilter filter)
        {
            try
            {
                var viewModel = await _shareholderService.GetAllShareholdersAsync(filter);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shareholders");
                TempData["ErrorMessage"] = "Error loading shareholders. Please try again.";
                return View(new ShareholderIndexViewModel());
            }
        }

        // GET: Shareholders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var shareholder = await _shareholderService.GetShareholderByIdAsync(id.Value);

                if (shareholder == null)
                {
                    TempData["ErrorMessage"] = "Shareholder not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(shareholder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shareholder details for ID: {ShareholderId}", id);
                TempData["ErrorMessage"] = "Error loading shareholder details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Shareholders/Create
        public IActionResult Create()
        {
            var viewModel = new ShareholderViewModel
            {
                StatusList = new SelectList(new[] { "Active", "Inactive" }),
                MemberTypeList = new SelectList(new[] { "New", "Active", "Premium" }),
                GenderList = new SelectList(new[] { "Male", "Female", "Other" })
            };

            return View(viewModel);
        }

        // POST: Shareholders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShareholderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.StatusList = new SelectList(new[] { "Active", "Inactive" });
                model.MemberTypeList = new SelectList(new[] { "New", "Active", "Premium" });
                model.GenderList = new SelectList(new[] { "Male", "Female", "Other" });
                return View(model);
            }

            try
            {
                var (success, message) = await _shareholderService.CreateShareholderAsync(model); //this shows create uses the service or controller posts this service

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    // ✅ Add this block here
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        var createdShareholder = await _shareholderService.GetByUserIdAsync(user.Id);
                        if (createdShareholder != null)
                        {
                            await _userManager.AddClaimAsync(user,
                                new Claim("ShareholderId", createdShareholder.ShareholderId.ToString()));
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    model.StatusList = new SelectList(new[] { "Active", "Inactive" });
                    model.MemberTypeList = new SelectList(new[] { "New", "Active", "Premium" });
                    model.GenderList = new SelectList(new[] { "Male", "Female", "Other" });
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shareholder");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                model.StatusList = new SelectList(new[] { "Active", "Inactive" });
                model.MemberTypeList = new SelectList(new[] { "New", "Active", "Premium" });
                model.GenderList = new SelectList(new[] { "Male", "Female", "Other" });
                return View(model);
            }
        }

        // GET: Shareholders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var shareholder = await _shareholderService.GetShareholderByIdAsync(id.Value);

                if (shareholder == null)
                {
                    TempData["ErrorMessage"] = "Shareholder not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ShareholderViewModel
                {
                    ShareholderId = shareholder.ShareholderId,
                    FullName = shareholder.FullName,
                    Email = shareholder.Email,
                    Phone = shareholder.Phone ?? "",
                    JoinDate = shareholder.JoinDate,
                    TotalShares = shareholder.TotalShares,
                    CurrentBalance = shareholder.CurrentBalance,
                    NumberOfCertificates = shareholder.NumberOfCertificates,
                    Status = shareholder.Status,
                    MemberType = shareholder.MemberType,
                    Address = shareholder.Address,
                    City = shareholder.City,
                    IdNumber = shareholder.IdNumber,
                    DateOfBirth = shareholder.DateOfBirth,
                    Gender = shareholder.Gender,
                    StatusList = new SelectList(new[] { "Active", "Inactive" }, shareholder.Status),
                    MemberTypeList = new SelectList(new[] { "New", "Active", "Premium" }, shareholder.MemberType),
                    GenderList = new SelectList(new[] { "Male", "Female", "Other" }, shareholder.Gender)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shareholder for edit");
                TempData["ErrorMessage"] = "Error loading shareholder.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Shareholders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShareholderViewModel model)
        {
            if (id != model.ShareholderId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.StatusList = new SelectList(new[] { "Active", "Inactive" }, model.Status);
                model.MemberTypeList = new SelectList(new[] { "New", "Active", "Premium" }, model.MemberType);
                model.GenderList = new SelectList(new[] { "Male", "Female", "Other" }, model.Gender);
                return View(model);
            }

            try
            {
                var (success, message) = await _shareholderService.UpdateShareholderAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Details), new { id = model.ShareholderId });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    model.StatusList = new SelectList(new[] { "Active", "Inactive" }, model.Status);
                    model.MemberTypeList = new SelectList(new[] { "New", "Active", "Premium" }, model.MemberType);
                    model.GenderList = new SelectList(new[] { "Male", "Female", "Other" }, model.Gender);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shareholder");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                model.StatusList = new SelectList(new[] { "Active", "Inactive" }, model.Status);
                model.MemberTypeList = new SelectList(new[] { "New", "Active", "Premium" }, model.MemberType);
                model.GenderList = new SelectList(new[] { "Male", "Female", "Other" }, model.Gender);
                return View(model);
            }
        }

        // GET: Shareholders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var shareholder = await _shareholderService.GetShareholderByIdAsync(id.Value);

                if (shareholder == null)
                {
                    TempData["ErrorMessage"] = "Shareholder not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(shareholder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shareholder for deletion");
                TempData["ErrorMessage"] = "Error loading shareholder.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Shareholders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var (success, message) = await _shareholderService.DeleteShareholderAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shareholder");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}