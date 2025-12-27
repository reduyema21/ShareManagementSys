using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SaccoShareManagementSys.Data;
using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.Services;
using SaccoShareManagementSys.ViewModels;


namespace SaccoShareManagementSys.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITransactionService _transactionService;
        private readonly IShareholderService _shareholderService;
        private readonly IShareService _shareService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            ApplicationDbContext context,
            ITransactionService transactionService,
            IShareholderService shareholderService,
            IShareService shareService,
            ILogger<TransactionsController> logger)
        {
            _context = context;
            _transactionService = transactionService;
            _shareholderService = shareholderService;
            _shareService = shareService;
            _logger = logger;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(TransactionSearchFilter filter)
        {
            try
            {
                var viewModel = await _transactionService.GetAllTransactionsAsync(filter);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transactions");
                TempData["ErrorMessage"] = "Error loading transactions. Please try again.";
                return View(new TransactionIndexViewModel());
            }
        }


        // GET: Transactions/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new TransactionViewModel();
                await PopulateDropdowns(model); // populate all dropdowns
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create transaction page");
                TempData["ErrorMessage"] = "Error loading page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }


        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionViewModel model)
        {
            // **Updated ModelState logging**
            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        _logger.LogWarning("ModelState Error: {Field} => {Error}", kvp.Key, error.ErrorMessage);
                    }
                }

                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var (success, message) = await _transactionService.CreateTransactionAsync(model);

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
                _logger.LogError(ex, "Error creating transaction");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id.Value);

                if (transaction == null)
                {
                    TempData["ErrorMessage"] = "Transaction not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new TransactionViewModel
                {
                    TransactionId = transaction.TransactionId,
                    ShareId = transaction.ShareId,
                    ShareholderId = transaction.Share!.ShareholderId,
                    TransactionType = transaction.TransactionType,
                    Amount = transaction.Amount,
                    TransactionDate = transaction.TransactionDate,
                    Description = transaction.Description ?? "",
                    PaymentMethod = transaction.PaymentMethod,
                    Status = transaction.Status ?? "",
                    Notes = transaction.Notes,
                    ShareholderName = transaction.Share?.Shareholder?.FullName,
                    ShareCertificateNumber = transaction.Share?.CertificateNumber,
                    PaymentMethods = new SelectList(
                        new[] { "Cash", "Bank Transfer", "M-Pesa", "Cheque", "Transfer" },
                        transaction.PaymentMethod),
                    StatusList = new SelectList(
                        new[] { "Completed", "Pending", "Failed", "Cancelled" },
                        transaction.Status)
                };

                await PopulateDropdowns(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transaction for edit");
                TempData["ErrorMessage"] = "Error loading transaction.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransactionViewModel model)
        {
            if (id != model.TransactionId)
            {
                _logger.LogWarning("Edit called with mismatched ID. URL ID: {UrlId}, Model ID: {ModelId}", id, model.TransactionId);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.PaymentMethods = new SelectList(
                    new[] { "Cash", "Bank Transfer", "M-Pesa", "Cheque", "Transfer" },
                    model.PaymentMethod);
                model.StatusList = new SelectList(
                    new[] { "Completed", "Pending", "Failed", "Cancelled" },
                    model.Status);
                _logger.LogWarning("ModelState invalid. Returning Edit view.");
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var (success, message) = await _transactionService.UpdateTransactionAsync(model);

                _logger.LogInformation("Update success: {Success}", success);

                if (success)
                {
                    var updatedShare = await _shareService.GetShareByIdAsync(model.ShareId);

                    // Update the view model properties for display
                    model.ShareholderName = updatedShare?.Shareholder?.FullName;
                    model.ShareCertificateNumber = updatedShare?.CertificateNumber;


                    TempData["SuccessMessage"] = message;
                    _logger.LogInformation("Redirecting to Index after successful update.");
                    //return RedirectToAction(nameof(Details), new { id = model.TransactionId });
                    return RedirectToAction(nameof(Index));

                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    model.PaymentMethods = new SelectList(
                        new[] { "Cash", "Bank Transfer", "M-Pesa", "Cheque", "Transfer" },
                        model.PaymentMethod);
                    model.StatusList = new SelectList(
                        new[] { "Completed", "Pending", "Failed", "Cancelled" },
                        model.Status);
                    _logger.LogWarning("Update failed: {Message}", message);
                    await PopulateDropdowns(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                model.PaymentMethods = new SelectList(
                    new[] { "Cash", "Bank Transfer", "M-Pesa", "Cheque", "Transfer" },
                    model.PaymentMethod);
                model.StatusList = new SelectList(
                    new[] { "Completed", "Pending", "Failed", "Cancelled" },
                    model.Status);
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id.Value);

                if (transaction == null)
                {
                    TempData["ErrorMessage"] = "Transaction not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transaction details for ID: {TransactionId}", id);
                TempData["ErrorMessage"] = "Error loading transaction details.";
                return RedirectToAction(nameof(Index));
            }
        }


        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id.Value);

                if (transaction == null)
                {
                    TempData["ErrorMessage"] = "Transaction not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transaction for deletion");
                TempData["ErrorMessage"] = "Error loading transaction.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var (success, message) = await _transactionService.DeleteTransactionAsync(id);

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
                _logger.LogError(ex, "Error deleting transaction");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX: Get shares by shareholder
        [HttpGet]
        public async Task<JsonResult> GetSharesByShareholderId(int shareholderId)
        {
            try
            {
                var shares = await _transactionService.GetSharesByShareholderIdAsync(shareholderId);
                var sharesList = shares.Select(s => new
                {
                    value = s.ShareId,
                    text = "{s.CertificateNumber} - {s.ShareType} (ETB {s.ShareValue:N2})"
                });

                return Json(sharesList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shares for shareholder");
                return Json(new List<object>());
            }
        }
        //api for transaction flow barchart which is used to exchange the data from the database
        //[HttpGet]
        //public async Task<IActionResult> GetTransactionFlow()
        //{
        //    var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        //    var credits = await _context.ShareTransactions
        //        .Where(t =>
        //            (t.TransactionType == "Credit" ||
        //             t.TransactionType == "Deposit" ||
        //             t.TransactionType == "Dividend" ||
        //             t.TransactionType == "Purchase") &&
        //            t.Status == "Completed" &&
        //            t.TransactionDate >= firstDayOfMonth)
        //        .SumAsync(t => (decimal?)t.Amount) ?? 0;

        //    var debits = await _context.ShareTransactions
        //        .Where(t =>
        //            (t.TransactionType == "Debit" ||
        //             t.TransactionType == "Withdrawal" ||
        //             t.TransactionType == "Transfer") &&
        //            t.Status == "Completed" &&
        //            t.TransactionDate >= firstDayOfMonth)
        //        .SumAsync(t => (decimal?)t.Amount) ?? 0;

        //    return Json(new
        //    {
        //        months = new[] { DateTime.Now.ToString("MMM yyyy") },
        //        credits = new[] { credits },
        //        debits = new[] { debits }
        //    });
        //}
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> GetTransactionFlow()
        {
            var transactions = await _context.ShareTransactions
                .Where(t => t.Status == "Completed")
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();

            // Get distinct transaction dates
            var transactionDates = transactions
                .Select(t => t.TransactionDate.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var labels = transactionDates.Select(d => d.ToString("MMM dd")).ToArray();

            var credits = transactionDates.Select(date =>
                transactions
                    .Where(t => t.TransactionDate.Date == date &&
                                (t.TransactionType == "Credit" || t.TransactionType == "Deposit" || t.TransactionType == "Dividend" || t.TransactionType == "Purchase"))
                    .Sum(t => t.Amount)
            ).ToArray();

            var debits = transactionDates.Select(date =>
                transactions
                    .Where(t => t.TransactionDate.Date == date &&
                                (t.TransactionType == "Debit" || t.TransactionType == "Withdrawal" || t.TransactionType == "Transfer"))
                    .Sum(t => Math.Abs(t.Amount))
            ).ToArray();

            return Json(new
            {
                months = labels,   // only recorded transaction dates
                credits,
                debits
            });
        }


        private async Task PopulateDropdowns(TransactionViewModel model)
        {
            var shareholders = await _shareholderService.GetActiveShareholdersAsync();

            model.Shareholders = new SelectList(
                shareholders.Select(s => new {
                    Id = s.ShareholderId,            // Make sure your property name is correct
                    FullName = s.FullName // Must match your model property
                }),
                "Id",
                "FullName"
            );
            // to Load all active shares 
            var result = await _shareService.GetAllSharesAsync();

            model.Shares = new SelectList(
                result.Shares.Select(s => new
                {
                    Value = s.ShareId,
                    Text = $"{s.CertificateNumber} - {s.ShareholderName}"
                }),
                "Value",
                "Text",
                model.ShareId
            );

            // 2️⃣ Transaction Types
            model.TransactionTypes = new SelectList(
                new[] { "Credit", "Debit", "Deposit", "Withdrawal", "Dividend", "Transfer" },
                model.TransactionType
            );

            //  Payment Methods
            model.PaymentMethods = new SelectList(
                new[] { "Cash", "Bank Transfer", "M-Pesa", "Cheque", "Transfer" },
                model.PaymentMethod
            );

            //  Status List
            model.StatusList = new SelectList(
                new[] { "Completed", "Pending", "Failed", "Cancelled" },
                model.Status
            );
        }

    }
}