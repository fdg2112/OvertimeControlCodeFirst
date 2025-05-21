using OvertimeControlCodeFirst.Data;
using OvertimeControlCodeFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OvertimeControlCodeFirst.Controllers
{
    public class OvertimeController : Controller
    {
        private readonly OvertimeDbContext _context;

        public OvertimeController(OvertimeDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            var areaIdClaim = User.FindFirst("AreaId");
            var secretariatIdClaim = User.FindFirst("SecretariatId");

            int? areaId = null;
            if (areaIdClaim != null && !string.IsNullOrEmpty(areaIdClaim.Value))
            {
                areaId = int.Parse(areaIdClaim.Value);
            }

            if (secretariatIdClaim == null || string.IsNullOrEmpty(secretariatIdClaim.Value))
            {
                return Json(new { error = "No se encontró el claim de SecretariatId." });
            }
            int secretariatId = int.Parse(secretariatIdClaim.Value);

            var employees = _context.Employees
                .Include(e => e.Area)
                .Include(e => e.Secretariat)
                .Where(e =>
                    (areaId.HasValue && e.AreaId == areaId) ||  
                    (!areaId.HasValue && e.SecretariatId == secretariatId)) 
                .Select(e => new
                {
                    EmployeeId = e.EmployeeId,
                    RecordNumber = e.RecordNumber,
                    Name = e.Name,
                    LastName = e.LastName,
                    AreaId = e.AreaId,
                    AreaNombre = e.Area != null ? e.Area.Name : "Sin Área",
                    SecretariatId = e.SecretariatId,
                    SecretariaNombre = e.Secretariat.Name
                })
                .ToList();

            var secretariats = _context.Secretariats
                .Select(s => new { id = s.SecretariatId, nombre = s.Name })
                .ToList();

            var areas = _context.Areas
                .Where(a => a.SecretariatId == secretariatId)
                .Select(a => new { id = a.AreaId, nombre = a.Name })
                .ToList();

            return Json(new
            {
                employees,
                secretariats,
                areas
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Overtime overtime)
        {
            ModelState.Remove("Area");
            ModelState.Remove("Secretariat");
            ModelState.Remove("Empleado");

            if (overtime.DateStart >= overtime.DateEnd)
            {
                return Json(new
                {
                    success = false,
                    message = "La fecha y hora de inicio deben ser anteriores a la fecha y hora de fin."
                });
            }

            if (ModelState.IsValid)
            {
                var employee = await _context.Employees
                    .Where(e => e.EmployeeId == overtime.EmployeeId)
                    .Select(e => new { e.AreaId, e.SecretariatId })
                    .FirstOrDefaultAsync();

                if (employee == null)
                {
                    return Json(new { success = false, message = "El employee seleccionado no existe." });
                }

                overtime.AreaId = employee.AreaId ?? 0; 
                overtime.SecretariatId = employee.SecretariatId;

                var hasOverlap = await _context.Overtimes
                    .AnyAsync(h => h.EmployeeId == overtime.EmployeeId &&
                                   h.DateStart.Date == overtime.DateStart.Date &&
                                   !(h.DateEnd <= overtime.DateStart || h.DateStart >= overtime.DateEnd));

                if (hasOverlap)
                {
                    return Json(new { success = false, message = "Las horas extras se solapan con otras ya registradas." });
                }

                _context.Add(overtime);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Horas extras guardadas exitosamente." });
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors)) Console.WriteLine(error.ErrorMessage);
                return Json(new { success = false, message = "Error al guardar las horas extras. Por favor, revise los datos ingresados." });
            }
        }


    }
}


