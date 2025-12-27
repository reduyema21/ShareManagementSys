using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using SaccoShareManagementSys.Services;
using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SharesController : Controller
    {
        private readonly IShareService _shareService;
        private readonly IShareholderService _shareholderService;
        private readonly ILogger<SharesController> _logger;

        public SharesController(
            IShareService shareService,
            IShareholderService shareholderService,
            ILogger<SharesController> logger)
        {
            _shareService = shareService;
            _shareholderService = shareholderService;
            _logger = logger;
        }

        // GET: Shares
        public async Task<IActionResult> Index(ShareSearchFilter filter)
        {
            try
            {
                var viewModel = await _shareService.GetAllSharesAsync(filter);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shares");
                TempData["ErrorMessage"] = "Error loading shares. Please try again.";
                return View(new ShareIndexViewModel());
            }
        }

        // GET: Shares/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var share = await _shareService.GetShareByIdAsync(id.Value);

                if (share == null)
                {
                    TempData["ErrorMessage"] = "Share certificate not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(share);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading share details for ID: {ShareId}", id);
                TempData["ErrorMessage"] = "Error loading share details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Shares/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var shareholders = await _shareholderService.GetActiveShareholdersAsync();
                var nextCertNumber = await _shareService.GenerateNextCertificateNumberAsync();

                var viewModel = new ShareViewModel
                {
                    CertificateNumber = nextCertNumber,
                    Shareholders = new SelectList(
                        shareholders.Select(s => new
                        {
                            Value = s.ShareholderId,
                            Text = $"#{s.ShareholderId:D4} - {s.FullName}"
                        }),
                        "Value",
                        "Text"),
                    ShareTypes = new SelectList(new[]
                    {
                        "Ordinary Shares",
                        "Preference Shares",
                        "Cumulative Shares",
                        "Non-Cumulative Shares",
                        "Redeemable Shares"
                    }),
                    StatusList = new SelectList(new[] { "Active", "Matured", "Cancelled" })
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create share page");
                TempData["ErrorMessage"] = "Error loading page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Shares/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShareViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var (success, message) = await _shareService.CreateShareAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    await PopulateDropdowns(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating share");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        // GET: Shares/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var share = await _shareService.GetShareByIdAsync(id.Value);

                if (share == null)
                {
                    TempData["ErrorMessage"] = "Share certificate not found.";
                    return RedirectToAction(nameof(Index));
                }

                var shareholders = await _shareholderService.GetActiveShareholdersAsync();

                var viewModel = new ShareViewModel
                {
                    ShareId = share.ShareId,
                    ShareholderId = share.ShareholderId,
                    CertificateNumber = share.CertificateNumber,
                    ShareType = share.ShareType,
                    NumberOfShares = share.NumberOfShares,
                    ShareValue = share.ShareValue,
                    PurchaseDate = share.PurchaseDate,
                    Status = share.Status,
                    Notes = share.Notes,
                    MaturityDate = share.MaturityDate,
                    ShareholderName = share.Shareholder?.FullName,
                    Shareholders = new SelectList(
                        shareholders.Select(s => new
                        {
                            Value = s.ShareholderId,
                            Text = $"#{s.ShareholderId:D4} - {s.FullName}"
                        }),
                        "Value",
                        "Text",
                        share.ShareholderId),
                    ShareTypes = new SelectList(new[]
                    {
                        "Ordinary Shares",
                        "Preference Shares",
                        "Cumulative Shares",
                        "Non-Cumulative Shares",
                        "Redeemable Shares"
                    }, share.ShareType),
                    StatusList = new SelectList(new[] { "Active", "Matured", "Cancelled" }, share.Status)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading share for edit");
                TempData["ErrorMessage"] = "Error loading share.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Shares/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShareViewModel model)
        {
            if (id != model.ShareId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var (success, message) = await _shareService.UpdateShareAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Details), new { id = model.ShareId });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    await PopulateDropdowns(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating share");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        // GET: Shares/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var share = await _shareService.GetShareByIdAsync(id.Value);

                if (share == null)
                {
                    TempData["ErrorMessage"] = "Share certificate not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(share);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading share for deletion");
                TempData["ErrorMessage"] = "Error loading share.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Shares/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var (success, message) = await _shareService.DeleteShareAsync(id);

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
                _logger.LogError(ex, "Error deleting share");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX: Generate next certificate number
        [HttpGet]
        public async Task<JsonResult> GenerateNextCertificateNumber()
        {
            try
            {
                var nextNumber = await _shareService.GenerateNextCertificateNumberAsync();
                return Json(new { success = true, certificateNumber = nextNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating certificate number");
                return Json(new { success = false, message = "Error generating certificate number" });
            }
        }

        private async Task PopulateDropdowns(ShareViewModel model)
        {
            var shareholders = await _shareholderService.GetActiveShareholdersAsync();

            model.Shareholders = new SelectList(
                shareholders.Select(s => new
                {
                    Value = s.ShareholderId,
                    Text = $"#{s.ShareholderId:D4} - {s.FullName}"
                }),
                "Value",
                "Text",
                model.ShareholderId);

            model.ShareTypes = new SelectList(new[]
            {
                "Ordinary Shares",
                "Preference Shares",
                "Cumulative Shares",
                "Non-Cumulative Shares",
                "Redeemable Shares"
            }, model.ShareType);

            model.StatusList = new SelectList(new[] { "Active", "Matured", "Cancelled" }, model.Status);
        }
    }
}