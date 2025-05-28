using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OvertimeControlCodeFirst.Data;
using Microsoft.AspNetCore.Authorization;
using OvertimeControlCodeFirst.Models;
using System.Globalization;
using OvertimeControlCodeFirst.Helpers;
using OvertimeControlCodeFirst.Enums;

namespace OvertimeControlCodeFirst.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly OvertimeDbContext _context;

        public DashboardController(OvertimeDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst("UserId");
            var roleClaim = User.FindFirst("Role");
            var areaIdClaim = User.FindFirst("AreaId");
            var secretariatIdClaim = User.FindFirst("SecretariatId");

            if (userIdClaim == null || roleClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            var role = roleClaim.Value;
            int? areaId = string.IsNullOrEmpty(areaIdClaim?.Value) ? null : int.Parse(areaIdClaim.Value);
            int? secretariatId = string.IsNullOrEmpty(secretariatIdClaim?.Value) ? null : int.Parse(secretariatIdClaim.Value);

            IQueryable<Overtime> query = _context.Overtimes
                .Include(h => h.Area)
                .Include(h => h.Employee)
                .Include(h => h.Secretariat)
                .AsQueryable();

            if (role == "Jefe de Área" && areaId.HasValue)
            {
                query = query.Where(h => h.AreaId == areaId.Value);
            }
            else if (role == "Secretario")
            {
                query = query.Where(h => h.SecretariatId == secretariatId);
            }
            else if (role == "Intendente" || role == "Secretario Hacienda")
            {
                var secretariats = await _context.Secretariats.ToListAsync();
                ViewData["Secretariats"] = secretariats;
            }

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var startDate = DateTime.Now.AddMonths(-11);

            var monthlyHours = await query
                .Where(h => h.DateStart.Month == currentMonth && h.DateStart.Year == currentYear)
                .GroupBy(h => h.HourType)
                .Select(g => new
                {
                    HourType = g.Key,
                    TotalHours = g.Sum(h => h.HoursQuantity)
                })
                .ToListAsync();
            var overtimes50 = monthlyHours.FirstOrDefault(h => h.HourType == Enums.HourType.FiftyPercent)?.TotalHours ?? 0;
            var overtimes100 = monthlyHours.FirstOrDefault(h => h.HourType == Enums.HourType.OneHundredPercent)?.TotalHours ?? 0;
            
            var monthlyExpense = await _context.OvertimeExpenseView.ToListAsync();
            var expense50 = monthlyExpense.FirstOrDefault(h => h.HourType == Enums.HourType.FiftyPercent)?.TotalExpense ?? 0;
            var expense100 = monthlyExpense.FirstOrDefault(h => h.HourType == Enums.HourType.OneHundredPercent)?.TotalExpense ?? 0;
            
            var historicalHours = await query
                .Where(h => h.DateStart >= startDate)
                .GroupBy(h => new { h.DateStart.Year, h.DateStart.Month, h.HourType })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    HourType = g.Key.HourType,
                    TotalHours = g.Sum(h => h.HoursQuantity)
                })
                .OrderBy(h => h.Year)
                .ThenBy(h => h.Month)
                .ToListAsync();

            var months = historicalHours
                .Select(h => $"{h.Month:00}/{h.Year}")
                .Distinct()
                .ToList();

            var historicalOvertimes50 = months
                .Select(m => historicalHours
                    .Where(h => $"{h.Month:00}/{h.Year}" == m && h.HourType == Enums.HourType.FiftyPercent)
                    .Sum(h => h.TotalHours))
                .ToList();

            var historicalOvertimes100 = months
                .Select(m => historicalHours
                    .Where(h => $"{h.Month:00}/{h.Year}" == m && h.HourType == Enums.HourType.OneHundredPercent)
                    .Sum(h => h.TotalHours))
                .ToList();

            var areas = await _context.Areas
                .Where(a => role == "Secretario" ? a.SecretariatId == secretariatId : true)
                .ToListAsync();

            var activities = await _context.WorkActivities.ToListAsync();

            var employees = await _context.Employees
                .Include(e => e.Area)
                .Include(e => e.Secretariat)
                .Where(e =>
                    (areaId.HasValue && e.AreaId == areaId) ||
                    (!areaId.HasValue && e.SecretariatId == secretariatId))
                .ToListAsync();

            ViewData["WorkActivities"] = activities;
            ViewData["Employees"] = employees;
            ViewData["Areas"] = areas;
            ViewData["Overtimes50"] = overtimes50;
            ViewData["Overtimes100"] = overtimes100;
            ViewData["Expense50"] = Math.Round(expense50, 2);
            ViewData["Expense100"] = Math.Round(expense100, 2);
            ViewData["Months"] = months;
            ViewData["HistoricalOvertimes50"] = historicalOvertimes50;
            ViewData["HistoricalOvertimes100"] = historicalOvertimes100;
            ViewData["TotalExpenseFormatted"] = (expense50 + expense100).ToString("C", new CultureInfo("es-AR"));



            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(int? areaId = null, int? secretariatId = null)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var startDate = new DateTime(currentYear, currentMonth, 1).AddMonths(-11);

            var query = _context.Overtimes
                .Include(h => h.Employee)
                    .ThenInclude(e => e.SalaryCategory)
                        .ThenInclude(sc => sc.SalaryCategoryValues)
                .Include(h => h.Area)
                .Include(h => h.Secretariat)
                .AsQueryable();

            if (areaId.HasValue)
            {
                query = query.Where(h => h.AreaId == areaId.Value);
            }
            else if (secretariatId.HasValue)
            {
                query = query.Where(h => h.SecretariatId == secretariatId.Value);
            }
            query = query.Where(h => h.DateStart.Month == currentMonth && h.DateStart.Year == currentYear);

            var hours = await query.GroupBy(h => h.HourType).Select(g => new
            {
                HourType = g.Key,
                TotalHours = g.Sum(h => h.HoursQuantity)
            }).ToListAsync();

            var expenses = await query
                .Select(h => new
                {
                    h.HourType,
                    h.HoursQuantity,
                    BaseSalary = SalaryHelper.GetValidBaseSalary(h.Employee.SalaryCategory.SalaryCategoryValues, currentMonth, currentYear)
                })
                .GroupBy(h => h.HourType)
                .Select(g => new
                {
                    HourType = g.Key,
                    TotalExpense = g.Sum(h => h.HoursQuantity * ((h.HourType == Enums.HourType.FiftyPercent
                        ? (h.BaseSalary / 132) * 1.5m
                        : (h.BaseSalary / 132) * 2m)))
                }).ToListAsync();


            var historicalQuery = _context.Overtimes
                .Include(h => h.Employee)
                .Include(h => h.Area)
                .AsQueryable();

            if (areaId.HasValue)
            {
                historicalQuery = historicalQuery.Where(h => h.AreaId == areaId.Value);
            }
            else if (secretariatId.HasValue)
            {
                historicalQuery = historicalQuery.Where(h => h.SecretariatId == secretariatId.Value);
            }

            var historical = await historicalQuery
                .Where(h => h.DateStart >= startDate)
                .GroupBy(h => new { h.DateStart.Year, h.DateStart.Month, h.HourType })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    HourType = g.Key.HourType,
                    TotalHours = g.Sum(h => h.HoursQuantity)
                }).OrderBy(h => h.Year).ThenBy(h => h.Month)
                .ToListAsync();

            var historical50 = historical
                .Where(h => h.HourType == Enums.HourType.FiftyPercent)
                .Select(h => h.TotalHours)
                .ToList();

            var historical100 = historical
                .Where(h => h.HourType == Enums.HourType.OneHundredPercent)
                .Select(h => h.TotalHours)
                .ToList();

            return Json(new
            {
                Overtimes50 = hours.FirstOrDefault(h => h.HourType == Enums.HourType.FiftyPercent)?.TotalHours ?? 0,
                Overtimes100 = hours.FirstOrDefault(h => h.HourType == Enums.HourType.OneHundredPercent)?.TotalHours ?? 0,
                Expense50 = expenses.FirstOrDefault(g => g.HourType == Enums.HourType.FiftyPercent)?.TotalExpense ?? 0,
                Expense100 = expenses.FirstOrDefault(g => g.HourType == Enums.HourType.OneHundredPercent)?.TotalExpense ?? 0,
                Historical50 = historical50,
                Historical100 = historical100
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetDonutChartData()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var overtimes = await _context.Overtimes
                .Include(h => h.Employee)
                    .ThenInclude(e => e.SalaryCategory)
                        .ThenInclude(c => c.SalaryCategoryValues)
                .Include(h => h.Area)
                .Include(h => h.Secretariat)
                .Where(h => h.DateStart.Month == currentMonth && h.DateStart.Year == currentYear)
                .ToListAsync();

            var processed = overtimes.Select(h =>
            {
                var baseSalary = SalaryHelper.GetValidBaseSalary(h.Employee.SalaryCategory.SalaryCategoryValues, currentMonth, currentYear);
                var hourlyRate = (baseSalary / 132) * (h.HourType == Enums.HourType.FiftyPercent ? 1.5m : 2m);
                return new
                {
                    h.HoursQuantity,
                    HourlyRate = hourlyRate,
                    Area = h.Area?.Name ?? "Sin área",
                    Secretariat = h.Secretariat?.Name ?? "Sin secretaría"
                };
            });

            var expenseBySecretariat = processed
                .GroupBy(h => h.Secretariat)
                .Select(g => new
                {
                    Secretariat = g.Key,
                    TotalExpense = g.Sum(x => x.HoursQuantity * x.HourlyRate)
                })
                .ToList();

            var expenseByArea = processed
                .GroupBy(h => h.Area)
                .Select(g => new
                {
                    Area = g.Key,
                    TotalExpense = g.Sum(x => x.HoursQuantity * x.HourlyRate)
                })
                .ToList();

            return Json(new
            {
                ExpenseBySecretariat = expenseBySecretariat,
                ExpenseByArea = expenseByArea
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAreasBySecretariat(int? secretariatId)
        {
            var areas = await _context.Areas
                .Where(a => !secretariatId.HasValue || a.SecretariatId == secretariatId.Value)
                .Select(a => new { a.AreaId, a.Name })
                .ToListAsync();

            return Json(areas);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeesByArea(int? areaId = null)
        {
            var role = User.FindFirst("Role")?.Value;
            var areaIdClaim = User.FindFirst("AreaId")?.Value;
            var secretariatIdClaim = User.FindFirst("SecretariatId")?.Value;

            int? areaIdUser = string.IsNullOrEmpty(areaIdClaim) ? null : int.Parse(areaIdClaim);
            int? secretariatIdUser = string.IsNullOrEmpty(secretariatIdClaim) ? null : int.Parse(secretariatIdClaim);

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var previousYearMonth = currentMonth == 1 ? currentYear - 1 : currentYear;
            var employeesQuery = _context.Employees.AsQueryable();

            if (role == "Jefe de Área" && areaIdUser.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.AreaId == areaIdUser.Value);
            }
            else if (role == "Secretario" && secretariatIdUser.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.Area.SecretariatId == secretariatIdUser.Value);
                if (areaId.HasValue)
                {
                    employeesQuery = employeesQuery.Where(e => e.AreaId == areaId.Value);
                }
            }
            else if (role == "Intendente" || role == "Secretario Hacienda")
            {
                // Intendentes y Secretarios de Hacienda no tienen filtro inicial por ahora
            }
            else if (areaId.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.AreaId == areaId.Value);
            }

            var employees = await employeesQuery
                .Select(e => new
                {
                    e.RecordNumber,
                    e.LastName,
                    e.Name,
                    CurrentOvertimes50 = e.Overtimes
                        .Where(h => h.DateStart.Month == currentMonth &&
                                    h.DateStart.Year == currentYear &&
                                    h.HourType == Enums.HourType.FiftyPercent)
                        .Sum(h => h.HoursQuantity),
                    CurrentOvertimes100 = e.Overtimes
                        .Where(h => h.DateStart.Month == currentMonth &&
                                    h.DateStart.Year == currentYear &&
                                    h.HourType == Enums.HourType.OneHundredPercent)
                        .Sum(h => h.HoursQuantity),
                    PreviousOvertimes50 = e.Overtimes
                        .Where(h => h.DateStart.Month == previousMonth &&
                                    h.DateStart.Year == previousYearMonth &&
                                    h.HourType == Enums.HourType.FiftyPercent)
                        .Sum(h => h.HoursQuantity),
                    PreviousOvertimes100 = e.Overtimes
                        .Where(h => h.DateStart.Month == previousMonth &&
                                    h.DateStart.Year == previousYearMonth &&
                                    h.HourType == Enums.HourType.OneHundredPercent)
                        .Sum(h => h.HoursQuantity)
                })
                .ToListAsync();

            return Json(employees);
        }

        [HttpGet]
        public async Task<IActionResult> GetComparativeExpenses()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var query = _context.Overtimes
                .Include(h => h.Employee)
                    .ThenInclude(e => e.SalaryCategory)
                        .ThenInclude(sc => sc.SalaryCategoryValues)
                .Include(h => h.Area)
                .Include(h => h.Secretariat)
                .AsQueryable();

            var expenseBySecretariat = await query
                .Select(h => new
                {
                    h.Secretariat.Name,
                    h.HourType,
                    h.HoursQuantity,
                    BaseSalary = SalaryHelper.GetValidBaseSalary(h.Employee.SalaryCategory.SalaryCategoryValues, currentMonth, currentYear)
                })
                .GroupBy(h => h.Name)
                .Select(g => new
                {
                    Secretariat = g.Key,
                    TotalExpense = g.Sum(h => h.HoursQuantity * ((h.HourType == Enums.HourType.FiftyPercent
                        ? (h.BaseSalary / 132) * 1.5m
                        : (h.BaseSalary / 132) * 2m)))
                })
                .ToListAsync();

            var expenseByArea = await query
                .Select(h => new
                {
                    h.Area.Name,
                    h.HourType,
                    h.HoursQuantity,
                    BaseSalary = SalaryHelper.GetValidBaseSalary(h.Employee.SalaryCategory.SalaryCategoryValues, currentMonth, currentYear)
                })
                .GroupBy(h => h.Name)
                .Select(g => new
                {
                    Area = g.Key,
                    TotalExpense = g.Sum(h => h.HoursQuantity * ((h.HourType == Enums.HourType.FiftyPercent
                        ? (h.BaseSalary / 132) * 1.5m
                        : (h.BaseSalary / 132) * 2m)))
                })
                .ToListAsync();


            return Json(new
            {
                ExpenseBySecretariat = expenseBySecretariat,
                ExpenseByArea = expenseByArea
            });
        }
    }
}