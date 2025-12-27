using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SaccoShareManagementSys.Services;


using SaccoShareManagementSys.ViewModels;

namespace SaccoShareManagementSys.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShareTransfersController : Controller
    {
        private readonly IShareTransferService _transferService;
        private readonly ILogger<ShareTransfersController> _logger;

        public ShareTransfersController(
            IShareTransferService transferService,
            ILogger<ShareTransfersController> logger)
        {
            _transferService = transferService;
            _logger = logger;
        }

        // GET: ShareTransfers
        public async Task<IActionResult> Index(ShareTransferFilter filter)
        {
            try
            {
                var viewModel = await _transferService.GetAllTransfersAsync(filter);
                viewModel.Filter = filter;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading share transfers");
                TempData["ErrorMessage"] = "Error loading transfers. Please try again.";
                return View(new ShareTransferIndexViewModel());
            }
        }


        // GET: ShareTransfers/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var shareholders = await _transferService.GetActiveShareholdersAsync();

                var viewModel = new ShareTransferViewModel
                {
                    Shareholders = new SelectList(
                        shareholders.Select(s => new
                        {
                            Value = s.ShareholderId,
                            Text = $"#{s.ShareholderId:D4} - {s.FullName} (Balance: ETB {s.CurrentBalance:N2})"
                        }),
                        "Value",
                        "Text")
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create transfer page");
                TempData["ErrorMessage"] = "Error loading page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ShareTransfers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShareTransferViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var shareholders = await _transferService.GetActiveShareholdersAsync();
                model.Shareholders = new SelectList(
                    shareholders.Select(s => new
                    {
                        Value = s.ShareholderId,
                        Text = $"#{s.ShareholderId:D4} - {s.FullName} (Balance: {s.CurrentBalance:N2} Birr)"
                    }),
                    "Value",
                    "Text");

                return View(model);
            }

            try
            {
                var (success, message, transfer) = await _transferService.CreateTransferAsync(model);

                if (success && transfer != null)
                {
                    TempData["SuccessMessage"] = $"Transfer completed successfully! {model.ShareAmount:N2} Birr transferred.";
                    return RedirectToAction(nameof(Details), new { id = transfer.TransferId });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    var shareholders = await _transferService.GetActiveShareholdersAsync();
                    model.Shareholders = new SelectList(
                        shareholders.Select(s => new
                        {
                            Value = s.ShareholderId,
                            Text = $"#{s.ShareholderId:D4} - {s.FullName} (Balance: {s.CurrentBalance:N2} Birr )"
                        }),
                        "Value",
                        "Text");

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating share transfer");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");

                var shareholders = await _transferService.GetActiveShareholdersAsync();
                model.Shareholders = new SelectList(
                    shareholders.Select(s => new
                    {
                        Value = s.ShareholderId,
                        Text = $"#{s.ShareholderId:D4} - {s.FullName} (Balance: ETB {s.CurrentBalance:N2})"
                    }),
                    "Value",
                    "Text");

                return View(model);
            }
        }

        // GET: ShareTransfers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var transfer = await _transferService.GetTransferByIdAsync(id.Value);
                if (transfer == null)
                {
                    TempData["ErrorMessage"] = "Transfer not found.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new ShareTransferViewModel
                {
                    TransferId = transfer.TransferId,
                    FromShareholderId = transfer.FromShareholderId,
                    ToShareholderId = transfer.ToShareholderId,
                    FromShareholderName = transfer.FromShareholder?.FullName,
                    ToShareholderName = transfer.ToShareholder?.FullName,
                    ShareAmount = transfer.ShareAmount,
                    TransferDate = transfer.TransferDate,
                    Notes = transfer.Notes,
                    Status = transfer.Status,
                    StatusList = new SelectList(new[]
                    {
                new { Value = "Pending", Text = "Pending" },
                new { Value = "Completed", Text = "Completed" },
                new { Value = "Cancelled", Text = "Cancelled" }
            }, "Value", "Text")
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit transfer page");
                TempData["ErrorMessage"] = "Error loading page.";
                return RedirectToAction(nameof(Index));
            }
        }
        // POST: ShareTransfers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShareTransferViewModel model)
        {
            if (id != model.TransferId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.StatusList = new SelectList(new[]
                {
            new { Value = "Pending", Text = "Pending" },
            new { Value = "Completed", Text = "Completed" },
            new { Value = "Cancelled", Text = "Cancelled" }
        }, "Value", "Text");

                return View(model);
            }

            try
            {
                var result = await _transferService.UpdateTransferAsync(model);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    model.StatusList = new SelectList(new[]
                    {
                new { Value = "Pending", Text = "Pending" },
                new { Value = "Completed", Text = "Completed" },
                new { Value = "Cancelled", Text = "Cancelled" }
            }, "Value", "Text");

                    return View(model);
                }

                TempData["SuccessMessage"] = "Transfer updated successfully!";
                return RedirectToAction(nameof(Details), new { id = model.TransferId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating share transfer");
                ModelState.AddModelError(string.Empty, "Unexpected error occurred.");

                model.StatusList = new SelectList(new[]
                {
            new { Value = "Pending", Text = "Pending" },
            new { Value = "Completed", Text = "Completed" },
            new { Value = "Cancelled", Text = "Cancelled" }
        }, "Value", "Text");

                return View(model);
            }
        }



        // GET: ShareTransfers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var transfer = await _transferService.GetTransferByIdAsync(id.Value);

                if (transfer == null)
                {
                    TempData["ErrorMessage"] = "Transfer not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(transfer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transfer details for ID: {TransferId}", id);
                TempData["ErrorMessage"] = "Error loading transfer details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: ShareTransfers/GetShareholderBalance/5
        [HttpGet]
        public async Task<IActionResult> GetShareholderBalance(int shareholderId)
        {
            try
            {
                var shareholders = await _transferService.GetActiveShareholdersAsync();
                var shareholder = shareholders.FirstOrDefault(s => s.ShareholderId == shareholderId);

                if (shareholder == null)
                {
                    return Json(new { success = false, message = "Shareholder not found" });
                }

                return Json(new
                {
                    success = true,
                    shareholderId = shareholder.ShareholderId,
                    fullName = shareholder.FullName,
                    currentBalance = shareholder.CurrentBalance
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shareholder balance");
                return Json(new { success = false, message = "Error retrieving balance" });
            }
        }

        // GET: ShareTransfers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var transfer = await _transferService.GetTransferByIdAsync(id.Value);
                if (transfer == null)
                {
                    TempData["ErrorMessage"] = "Transfer not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(transfer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete page");
                TempData["ErrorMessage"] = "Error loading delete page.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ShareTransfers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _transferService.DeleteTransferAsync(id);

                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction(nameof(Delete), new { id });
                }

                TempData["SuccessMessage"] = "Transfer deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting share transfer");
                TempData["ErrorMessage"] = "Unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }


    }

}