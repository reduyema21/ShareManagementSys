using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SaccoShareManagementSys.Services;
using SaccoShareManagementSys.ViewModels;
using SaccoShareManagementSys.Data;
using Microsoft.EntityFrameworkCore;

namespace SaccoShareManagementSys.Controllers
{
    [Authorize]
    public class DividendsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDividendService _dividendService;
        private readonly ILogger<DividendsController> _logger;

        public DividendsController(ApplicationDbContext context,
            IDividendService dividendService,
            ILogger<DividendsController> logger)
        {
            _context = context;
            _dividendService = dividendService;
            _logger = logger;
        }

        // INDEX
        // INDEX - improved binding and fallback
        public async Task<IActionResult> Index(int? year, int? month, string? status, string? searchTerm)
        {
            try
            {
                // Build filter from query params (explicit)
                var filter = new DividendSearchFilter
                {
                    Year = year,
                    Month = month,
                    Status = string.IsNullOrWhiteSpace(status) ? "all" : status,
                    SearchTerm = searchTerm
                };

                // If year is not provided, try get latest year from DB
                if (!filter.Year.HasValue)
                {
                    var latestYear = await _dividendService.GetLatestDividendYearAsync();
                    filter.Year = latestYear ?? DateTime.Now.Year;
                }

                var viewModel = await _dividendService.GetAllDividendsAsync(filter);

                // Ensure all lists are initialized to avoid null references
                viewModel.Dividends ??= new List<DividendListItem>();
                viewModel.DividendTrendData ??= new List<MonthlyDividendTrend>();
                viewModel.ShareFlowData ??= new List<MonthlyShareFlow>();
                viewModel.Statistics ??= new DividendStatistics();
                viewModel.Filter ??= new DividendSearchFilter
                {
                    Year = filter.Year,
                    Month = filter.Month,
                    Status = filter.Status,
                    SearchTerm = filter.SearchTerm
                };

                // Debugging: log counts (helpful while testing)
                _logger.LogInformation("Index loaded: filter year={Year} status={Status} dividendsCount={Count}",
                    viewModel.Filter.Year, viewModel.Filter.Status, viewModel.Dividends.Count);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dividends");

                return View(new DividendIndexViewModel
                {
                    Dividends = new List<DividendListItem>(),
                    DividendTrendData = new List<MonthlyDividendTrend>(),
                    ShareFlowData = new List<MonthlyShareFlow>(),
                    Statistics = new DividendStatistics(),
                    Filter = new DividendSearchFilter { Year = DateTime.Now.Year, Status = "all" }
                });
            }
        }







        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var dividend = await _dividendService.GetDividendByIdAsync(id.Value);

                if (dividend == null)
                {
                    TempData["ErrorMessage"] = "Dividend not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(dividend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading details for ID: {DividendId}", id);
                TempData["ErrorMessage"] = "Error loading dividend details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // CREATE (GET)
        public IActionResult Create(int? year, int? month, decimal? totalProfit, decimal? dividendRate, decimal? totalShares)
        {
            var viewModel = new DividendViewModel
            {
                Year = year ?? DateTime.Now.Year,
                Month = month ?? DateTime.Now.Month,
                TotalProfit = totalProfit ?? 0,
                DividendRate = dividendRate ?? 0,
                TotalShares = totalShares ?? 0
            };

            PopulateDropdowns(viewModel);

            // auto compute if both provided
            if (totalProfit.HasValue && dividendRate.HasValue)
            {
                viewModel.TotalDividendPaid = totalProfit.Value * (dividendRate.Value / 100m);
            }

            return View(viewModel);
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DividendViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var (success, message) = await _dividendService.CreateDividendAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Index", new { year = model.Year, status = "all" });
                }


                ModelState.AddModelError("", message);
                PopulateDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dividend");
                ModelState.AddModelError("", "Unexpected error occurred. Try again.");
                PopulateDropdowns(model);
                return View(model);
            }
        }



        // EDIT (GET)

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var dividend = await _dividendService.GetDividendByIdAsync(id.Value);

                if (dividend == null)
                {
                    TempData["ErrorMessage"] = "Dividend not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new DividendViewModel
                {
                    DividendId = dividend.DividendId,
                    Year = dividend.Year,
                    Month = dividend.Month,
                    TotalProfit = dividend.TotalProfit,
                    DividendRate = dividend.DividendRate,
                    TotalShares = dividend.TotalShares,
                    TotalDividendPaid = dividend.TotalDividendPaid,
                    DistributionDate = dividend.DistributionDate,
                    Status = dividend.Status,
                    Notes = dividend.Notes
                };

                PopulateDropdowns(viewModel);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dividend for edit");
                TempData["ErrorMessage"] = "Error loading dividend.";
                return RedirectToAction(nameof(Index));
            }
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DividendViewModel model)
        {
            if (id != model.DividendId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var (success, message) = await _dividendService.UpdateDividendAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Details), new { id = model.DividendId });
                }

                ModelState.AddModelError("", message);
                PopulateDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dividend");
                ModelState.AddModelError("", "Unexpected error occurred.");
                PopulateDropdowns(model);
                return View(model);
            }
        }

        // DELETE (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var dividend = await _dividendService.GetDividendByIdAsync(id.Value);

                if (dividend == null)
                {
                    TempData["ErrorMessage"] = "Dividend not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(dividend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dividend for deletion");
                TempData["ErrorMessage"] = "Error loading dividend.";
                return RedirectToAction(nameof(Index));
            }
        }

        // DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var (success, message) = await _dividendService.DeleteDividendAsync(id);

                TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dividend");
                TempData["ErrorMessage"] = "Unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        // DISTRIBUTE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Distribute(int id)
        {
            try
            {
                var (success, message) = await _dividendService.DistributeDividendAsync(id);

                TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error distributing dividend");
                TempData["ErrorMessage"] = "Unexpected error occurred.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // ============================
        // CALCULATE (GET)
        // ============================
        public IActionResult Calculate()
        {
            var vm = new DividendCalculationViewModel
            {
                Year = DateTime.Now.Year,
                Month = DateTime.Now.Month,
                Status = "Pending"
            };

            return View(vm);
        }

        // CALCULATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(int year, int month, decimal totalProfit, decimal dividendRate)
        {
            try
            {
                var vm = await _dividendService.CalculateDividendAsync(year, month, totalProfit, dividendRate);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating dividend");
                TempData["ErrorMessage"] = "Error calculating dividend. Please try again.";
                return RedirectToAction(nameof(Calculate));
            }
        }

        // POPULATE DROPDOWNS
        private void PopulateDropdowns(DividendViewModel model)
        {
            model.Years = new SelectList(
                Enumerable.Range(DateTime.Now.Year - 5, 10).Reverse(),
                model.Year);

            model.Months = new SelectList(
                Enumerable.Range(1, 12).Select(m => new
                {
                    Value = m,
                    Text = new DateTime(model.Year, m, 1).ToString("MMMM")
                }),
                "Value",
                "Text",
                model.Month);

            model.StatusList = new SelectList(
                new[] { "Pending", "Approved", "Distributed", "Cancelled" },
                model.Status);
        }
    }
}