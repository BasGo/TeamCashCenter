using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamCashCenter.Data;
using TeamCashCenter.Models;

namespace TeamCashCenter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly CashCenterContext _db;

        public TransactionsController(CashCenterContext db)
        {
            _db = db;
        }
        
            // GET: api/transactions/export/excel
            [HttpGet("export/excel")]
            public async Task<IActionResult> ExportToExcel(
                DateTime? from = null,
                DateTime? to = null,
                Guid? accountId = null,
                Guid? userId = null)
            {
                var query = _db.Transactions
                    .AsNoTracking()
                    .Include(t => t.Account)
                    .Include(t => t.User)
                    .AsQueryable();

                if (from.HasValue)
                    query = query.Where(t => t.BookingDate >= from.Value);
                if (to.HasValue)
                    query = query.Where(t => t.BookingDate <= to.Value);
                if (accountId.HasValue)
                    query = query.Where(t => t.AccountId == accountId.Value);
                if (userId.HasValue)
                    query = query.Where(t => t.UserId == userId.Value);

                var transactions = await query
                    .OrderByDescending(t => t.BookingDate)
                    .ThenByDescending(t => t.Id)
                    .ToListAsync();

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var ws = workbook.Worksheets.Add("Transaktionen");
                // Header
                ws.Cell(1, 1).Value = "Id";
                ws.Cell(1, 2).Value = "ParentId";
                ws.Cell(1, 3).Value = "Erfassungsdatum";
                ws.Cell(1, 4).Value = "Buchungsdatum";
                ws.Cell(1, 5).Value = "Betrag";
                ws.Cell(1, 6).Value = "Konto";
                ws.Cell(1, 7).Value = "Spieler";
                ws.Cell(1, 8).Value = "Beschreibung";

                for (int i = 0; i < transactions.Count; i++)
                {
                    var t = transactions[i];
                    ws.Cell(i + 2, 1).Value = t.Id.ToString();
                    ws.Cell(i + 2, 2).Value = t.ParentId?.ToString() ?? "";
                    ws.Cell(i + 2, 3).Value = t.Date.ToString("yyyy-MM-dd HH:mm:ss");
                    ws.Cell(i + 2, 4).Value = t.BookingDate.ToString("yyyy-MM-dd");
                    ws.Cell(i + 2, 5).Value = t.Amount;
                    ws.Cell(i + 2, 6).Value = t.Account?.Name ?? "";
                    ws.Cell(i + 2, 7).Value = t.User != null ? (t.User.FirstName + " " + t.User.LastName) : "";
                    ws.Cell(i + 2, 8).Value = t.Description;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);
                var fileName = $"Transaktionen_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

        // GET: api/transactions?page=1&pageSize=20&from=2025-01-01&to=2025-12-31&accountId=1&playerId=2&sortBy=bookingDate&sortDir=desc
        [HttpGet]
        public async Task<IActionResult> Get(
            int page = 1,
            int pageSize = 20,
            DateTime? from = null,
            DateTime? to = null,
            Guid? accountId = null,
            Guid? userId = null,
            string? sortBy = null,
            string? sortDir = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _db.Transactions
                .AsNoTracking()
                .Include(t => t.Account)
                .Include(t => t.User)
                .AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(t => t.BookingDate >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(t => t.BookingDate <= to.Value);
            }
            if (accountId.HasValue)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }
            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId.Value);
            }

            // total before paging
            var totalCount = await query.CountAsync();

            // sorting
            var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(sortBy, "amount", StringComparison.OrdinalIgnoreCase))
            {
                query = descending ? query.OrderByDescending(t => t.Amount).ThenByDescending(t => t.Id) : query.OrderBy(t => t.Amount).ThenBy(t => t.Id);
            }
            else
            {
                // default: booking date
                query = descending ? query.OrderByDescending(t => t.BookingDate).ThenByDescending(t => t.Id) : query.OrderBy(t => t.BookingDate).ThenBy(t => t.Id);
            }
            
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    ParentId = t.ParentId,
                    Date = t.Date,
                    BookingDate = t.BookingDate,
                    Description = t.Description,
                    Amount = t.Amount,
                    AccountId = t.AccountId,
                    AccountName = t.Account != null ? t.Account.Name : null,
                    UserId = t.UserId,
                    UserName = t.User != null ? (t.User.FirstName + " " + t.User.LastName) : null
                })
                .ToListAsync();

            var resp = new PagedResponse<TransactionDto>
            {
                Items = items,
                TotalCount = totalCount
            };

            return Ok(resp);
        }
    }
}
