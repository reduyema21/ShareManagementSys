using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaccoShareManagementSys.Data;
using SaccoShareManagementSys.Models;
using SaccoShareManagementSys.ViewModels;



namespace SaccoShareManagementSys.Services
{
    public class ShareholderService : IShareholderService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ShareholderService> _logger;

        public ShareholderService(ApplicationDbContext context, ILogger<ShareholderService> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;  // dependency is injected from outside class (don't need to create
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<ShareholderIndexViewModel> GetAllShareholdersAsync(ShareholderSearchFilter? filter = null)
        {
            var query = _context.Shareholders.AsQueryable();

            // Apply filters
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    query = query.Where(s =>
                        s.FullName.Contains(filter.SearchTerm) ||
                        s.Email.Contains(filter.SearchTerm) ||
                        s.Phone!.Contains(filter.SearchTerm) ||
                        s.ShareholderId.ToString().Contains(filter.SearchTerm));
                }

                if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status != "all")
                {
                    query = query.Where(s => s.Status == filter.Status);
                }

                if (!string.IsNullOrWhiteSpace(filter.MemberType) && filter.MemberType != "all")
                {
                    query = query.Where(s => s.MemberType == filter.MemberType);
                }
            }

            var shareholders = await query
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ShareholderListItem
                {
                    ShareholderId = s.ShareholderId,
                    FullName = s.FullName,
                    Email = s.Email,
                    Phone = s.Phone ?? "",
                    JoinDate = s.JoinDate,
                    TotalShares = s.TotalShares,
                    CurrentBalance = s.CurrentBalance,
                    NumberOfCertificates = s.NumberOfCertificates,
                    Status = s.Status,
                    MemberType = s.MemberType
                })
                .ToListAsync();

            var statistics = await GetStatisticsAsync();

            return new ShareholderIndexViewModel
            {
                Shareholders = shareholders,
                Statistics = statistics,
                Filter = filter ?? new ShareholderSearchFilter()
            };
        }

        public async Task<Shareholder?> GetShareholderByIdAsync(int id)
        {
            return await _context.Shareholders
                .Include(s => s.Shares)
                .FirstOrDefaultAsync(s => s.ShareholderId == id);
        }
        public async Task<List<Shareholder>> GetActiveShareholdersAsync()
        {
            return await _context.Shareholders
                .Where(s => s.Status == "Active")
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }


        public async Task<(bool Success, string Message)> CreateShareholderAsync(ShareholderViewModel model)
        {
            try
            {
                // Check if email already exists
                if (await EmailExistsAsync(model.Email))
                {
                    return (false, "Email address already exists");
                }

                // Check if phone already exists
                if (await PhoneExistsAsync(model.Phone))
                {
                    return (false, "Phone number already exists");
                }

                var shareholder = new Shareholder
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    JoinDate = model.JoinDate,
                    TotalShares = model.TotalShares,
                    CurrentBalance = model.CurrentBalance,
                    NumberOfCertificates = model.NumberOfCertificates,
                    Status = model.Status,
                    MemberType = model.MemberType,
                    Address = model.Address,
                    City = model.City,
                    IdNumber = model.IdNumber,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    CreatedAt = DateTime.Now,
                    IsApproved = true //mark approved immediately when admin creates

                };
                // link shareholder to identity user
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    shareholder.UserId = user.Id; // <-- this is crucial
                }

                _context.Shareholders.Add(shareholder);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Shareholder created: {ShareholderId} - {Name}",
                    shareholder.ShareholderId, shareholder.FullName);

                return (true, "Shareholder created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shareholder");
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateShareholderAsync(ShareholderViewModel model)
        {
            try
            {
                var shareholder = await _context.Shareholders
                    .FirstOrDefaultAsync(s => s.ShareholderId == model.ShareholderId);

                if (shareholder == null)
                {
                    return (false, "Shareholder not found");
                }

                // Check if email already exists (excluding current shareholder)
                if (await EmailExistsAsync(model.Email, model.ShareholderId))
                {
                    return (false, "Email address already exists");
                }

                // Check if phone already exists (excluding current shareholder)
                if (await PhoneExistsAsync(model.Phone, model.ShareholderId))
                {
                    return (false, "Phone number already exists");
                }

                shareholder.FullName = model.FullName;
                shareholder.Email = model.Email;
                shareholder.Phone = model.Phone;
                shareholder.JoinDate = model.JoinDate;
                shareholder.Status = model.Status;
                shareholder.MemberType = model.MemberType;
                shareholder.Address = model.Address;
                shareholder.City = model.City;
                shareholder.IdNumber = model.IdNumber;
                shareholder.DateOfBirth = model.DateOfBirth;
                shareholder.Gender = model.Gender;
                shareholder.UpdatedAt = DateTime.Now;

                // to ensure Identity user is linked and mark approved
                if (string.IsNullOrEmpty(shareholder.UserId))
                {
                    var user = await _userManager.FindByEmailAsync(shareholder.Email);
                    if (user != null)
                    {
                        shareholder.UserId = user.Id;
                        shareholder.IsApproved = true; // admin edit approves if linked to user
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Shareholder updated: {ShareholderId} - {Name}",
                    shareholder.ShareholderId, shareholder.FullName);

                return (true, "Shareholder updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shareholder");
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteShareholderAsync(int id)
        {
            try
            {
                var shareholder = await _context.Shareholders
                    .Include(s => s.Shares)
                    .FirstOrDefaultAsync(s => s.ShareholderId == id);

                if (shareholder == null)
                {
                    return (false, "Shareholder not found");
                }

                // Check if shareholder has shares
                if (shareholder.Shares != null && shareholder.Shares.Any())
                {
                    return (false, "Cannot delete shareholder with existing shares");
                }

                // Check if shareholder has balance
                if (shareholder.CurrentBalance > 0)
                {
                    return (false, "Cannot delete shareholder with outstanding balance");
                }

                _context.Shareholders.Remove(shareholder);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Shareholder deleted: {ShareholderId} - {Name}",
                    shareholder.ShareholderId, shareholder.FullName);

                return (true, "Shareholder deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shareholder");
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<ShareholderStatistics> GetStatisticsAsync()
        {
            var totalShareholders = await _context.Shareholders.CountAsync();
            var activeShareholders = await _context.Shareholders.CountAsync(s => s.Status == "Active");
            var totalValue = await _context.Shareholders.SumAsync(s => (decimal?)s.CurrentBalance) ?? 0;
            var avgBalance = totalShareholders > 0 ? totalValue / totalShareholders : 0;

            return new ShareholderStatistics
            {
                TotalShareholders = totalShareholders,
                ActiveShareholders = activeShareholders,
                TotalPortfolioValue = totalValue,
                AverageBalance = avgBalance
            };
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _context.Shareholders.Where(s => s.Email == email);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.ShareholderId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null)
        {
            var query = _context.Shareholders.Where(s => s.Phone == phone);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.ShareholderId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
        public async Task<(bool Success, string Message, ShareTransfer? Transfer)> CreateTransferAsync(ShareTransferViewModel model)
        {
            var fromShareholder = await _context.Shareholders.FindAsync(model.FromShareholderId);
            var toShareholder = await _context.Shareholders.FindAsync(model.ToShareholderId);

            if (fromShareholder == null || toShareholder == null)
                return (false, "Sender or receiver not found.", null);

            // Balance / shares check
            if (fromShareholder.CurrentBalance < model.ShareAmount || fromShareholder.TotalShares < model.ShareAmount)
                return (false, "Insufficient balance or shares.", null);

            // Deduct and add balances
            fromShareholder.CurrentBalance -= model.ShareAmount;
            fromShareholder.TotalShares -= model.ShareAmount;
            toShareholder.CurrentBalance += model.ShareAmount;
            toShareholder.TotalShares += model.ShareAmount;

            // Create transfer record
            var transfer = new ShareTransfer
            {
                FromShareholderId = model.FromShareholderId,
                ToShareholderId = model.ToShareholderId,
                ShareAmount = model.ShareAmount,
                TransferDate = DateTime.Now,
                Status = "Completed",
                Notes = model.Notes
            };

            _context.ShareTransfers.Add(transfer);
            await _context.SaveChangesAsync();

            return (true, "Transfer completed successfully.", transfer);
        }

        public async Task<int> GetTotalMembersAsync()
        {
            return await _context.Shareholders.CountAsync();
        }
        //to show the profile
        public async Task<Shareholder?> GetByIdAsync(int shareholderId)
        {
            return await _context.Shareholders
                .FirstOrDefaultAsync(s => s.ShareholderId == shareholderId);
        }

        public async Task<Shareholder?> GetByUserIdAsync(string userId)
        {
            return await _context.Shareholders
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }


    }

}