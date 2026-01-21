using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.Services;
using SaccoShareManagementSys.Services.SaccoManagement.Services;
using SaccoShareManagementSys.ViewModels;
using System.Security.Claims;

[Authorize(Roles = "Member")]
public class MemberController : Controller
{
    private readonly IShareholderService _shareholderService;
    private readonly IShareService _shareService;
    private readonly IDividendService _dividendService;
    private readonly ITransactionService _transactionService;
    private readonly IShareTransferService _shareTransferService;

    public MemberController(
        IShareholderService shareholderService,
        IShareService shareService,
        IDividendService dividendService,
        ITransactionService transactionService,
        IShareTransferService shareTransferService)
    {
        _shareholderService = shareholderService;
        _shareService = shareService;
        _dividendService = dividendService;
        _transactionService = transactionService;
        _shareTransferService = shareTransferService;
    }
    // helper methods
    private async Task<Shareholder?> GetApprovedShareholderAsync()
    {
        // Get current logged-in user ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return null;

        // Fetch shareholder from database
        var shareholder = await _shareholderService.GetByUserIdAsync(userId);

        // Return only if approved
        if (shareholder == null || !shareholder.IsApproved)
            return null;

        return shareholder;
    }

   
    private async Task<int> GetCurrentShareholderIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return -1;

        var shareholder = await _shareholderService.GetByUserIdAsync(userId);
        if (shareholder == null || !shareholder.IsApproved)
            return -1;

        return shareholder.ShareholderId;
    }




    //[Authorize(Roles = "Member")]
    //public async Task<IActionResult> Index()
    //{
    //    int shareholderId = await GetCurrentShareholderIdAsync();

    //    if (shareholderId < 0)
    //        return Forbid();//means to redirect pendingapproval 

    //    var (totalShares, totalValue) = await _shareService.GetUserSharesSummaryAsync(shareholderId);
    //    var totalDividends = await _dividendService.GetUserTotalDividendsAsync(shareholderId);
    //    //var lastTransaction = await _transactionService.GetUserLastTransactionAsync(shareholderId);
    //    //var recentTransactions = await _transactionService.GetUserTransactionsAsync(shareholderId);
    //    var sentTransfers = await _shareTransferService.GetSentRequestsAsync(shareholderId);
    //    var receivedTransfers = await _shareTransferService.GetReceivedRequestsAsync(shareholderId);
    //    var allTransfers = await _shareTransferService.GetRequestsForUserAsync(shareholderId);

    //    var model = new MemberDashboardViewModel
    //    {
    //        TotalShares = totalShares,
    //        TotalShareValue = totalValue,
    //        TotalDividends = totalDividends,
    //        //LastTransaction = lastTransaction,
    //        //RecentTransactions = recentTransactions,
    //        SentTransfers = sentTransfers,
    //        ReceivedTransfers = receivedTransfers,
    //        AllTransfers = allTransfers
    //    };

    //    return View(model);
    //}
    public async Task<IActionResult> Index()
    {
        var shareholder = await GetApprovedShareholderAsync();
        if (shareholder == null)
            return RedirectToPage("/Account/PendingApproval", new { area = "Identity" });

        int shareholderId = shareholder.ShareholderId;

        // Get shares and dividends
        var (totalShares, totalValue) = await _shareService.GetUserSharesSummaryAsync(shareholderId);
        var totalDividends = await _dividendService.GetUserTotalDividendsAsync(shareholderId);

        // Get transfers
        var sentTransfers = await _shareTransferService.GetSentRequestsAsync(shareholderId);
        var receivedTransfers = await _shareTransferService.GetReceivedRequestsAsync(shareholderId);
        var allTransfers = await _shareTransferService.GetRequestsForUserAsync(shareholderId);

        var lastTransaction = await _transactionService.GetUserLastTransactionAsync(shareholderId);
        var recentTransactions = (await _transactionService.GetUserTransactionsAsync(shareholderId)).ToList();

        // 4 Build view model
        var model = new MemberDashboardViewModel
        {
            TotalShares = totalShares,
            TotalShareValue = totalValue,
            TotalDividends = totalDividends,
            SentTransfers = sentTransfers,
            ReceivedTransfers = receivedTransfers,
            AllTransfers = allTransfers,
            LastTransaction = lastTransaction,
            RecentTransactions = recentTransactions.Take(8).ToList()

            // only show latest 8
        };

        return View(model);
    }



    //public async Task<IActionResult> MyDividends()
    //{
    //    int shareholderId = await GetCurrentShareholderIdAsync(); // Use synchronous helper
    //    if (shareholderId < 0)
    //        return Forbid();

    //    // Replace placeholder with actual call if implemented
    //    var dividends = await _dividendService.GetUserDividendsAsync(shareholderId);
    //    return View(dividends);
    //}
    public async Task<IActionResult> MyDividends()
    {
        var shareholder = await GetApprovedShareholderAsync();
        if (shareholder == null)
            return RedirectToPage("/Account/PendingApproval", new { area = "Identity" });

        var dividends = await _dividendService.GetUserDividendsAsync(shareholder.ShareholderId);
        return View(dividends);
    }


    //public async Task<IActionResult> MyShares()
    //{
    //    int shareholderId = await GetCurrentShareholderIdAsync();
    //    if (shareholderId < 0)
    //        return Forbid();

    //    var shares = await _shareService.GetSharesByShareholderIdAsync(shareholderId);
    //    return View(shares);
    //}
    public async Task<IActionResult> MyShares()
    {
        var shareholder = await GetApprovedShareholderAsync();
        if (shareholder == null)
            return RedirectToPage("/Account/PendingApproval", new { area = "Identity" });

        var shares = await _shareService.GetSharesByShareholderIdAsync(shareholder.ShareholderId);
        return View(shares);
    }

    //public async Task<IActionResult> Profile()
    //{
    //    int shareholderId = await GetCurrentShareholderIdAsync();
    //    if (shareholderId < 0)
    //        return Forbid();

    //    var shareholder = await _shareholderService.GetByIdAsync(shareholderId);
    //    return View(shareholder);
    //}
    public async Task<IActionResult> Profile()
    {
        var shareholder = await GetApprovedShareholderAsync();
        if (shareholder == null)
            return RedirectToPage("/Account/PendingApproval", new { area = "Identity" });

        return View(shareholder);
    }

    //public async Task<IActionResult> MyTransactions()
    //{
    //    int shareholderId = await GetCurrentShareholderIdAsync(); // Use synchronous helper
    //    if (shareholderId < 0)
    //        return Forbid();

    //    // Fetch transactions for the current member
    //    var txs = await _transactionService.GetUserTransactionsAsync(shareholderId);
    //    return View(txs);
    //}
    public async Task<IActionResult> MyTransactions()
    {
        var shareholder = await GetApprovedShareholderAsync();
        if (shareholder == null)
            return RedirectToPage("/Account/PendingApproval", new { area = "Identity" });

        var txs = await _transactionService.GetUserTransactionsAsync(shareholder.ShareholderId);
        return View(txs);
    }


}