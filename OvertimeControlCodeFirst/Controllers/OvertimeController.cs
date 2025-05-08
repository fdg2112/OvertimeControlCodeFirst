using ControlHorasExtras.Data;
using ControlHorasExtras.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlHorasExtras.Controllers
{
    public class OvertimeController : Controller
    {
        private readonly OvertimeControlContext _context;

        public OvertimeController(OvertimeControlContext context)
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
            var secretariaIdClaim = User.FindFirst("SecretariaId");

            int? areaId = null;
            if (areaIdClaim != null && !string.IsNullOrEmpty(areaIdClaim.Value))
            {
                areaId = int.Parse(areaIdClaim.Value);
            }

            if (secretariaIdClaim == null || string.IsNullOrEmpty(secretariaIdClaim.Value))
            {
                return Json(new { error = "No se encontró el claim de SecretariaId." });
            }
            int secretariaId = int.Parse(secretariaIdClaim.Value);

            var employees = _context.Employees
                .Include(e => e.Area)
                .Include(e => e.Secretaria)
                .Where(e =>
                    (areaId.HasValue && e.AreaId == areaId) ||  
                    (!areaId.HasValue && e.SecretariaId == secretariaId)) 
                .Select(e => new
                {
                    EmpleadoId = e.EmpleadoId,
                    Legajo = e.Legajo,
                    Nombre = e.Nombre,
                    Apellido = e.Apellido,
                    AreaId = e.AreaId,
                    AreaNombre = e.Area != null ? e.Area.NombreArea : "Sin Área",
                    SecretariaId = e.SecretariaId,
                    SecretariaNombre = e.Secretaria.NombreSecretaria
                })
                .ToList();

            var secretarias = _context.Secretariats
                .Select(s => new { id = s.SecretariaId, nombre = s.NombreSecretaria })
                .ToList();

            var areas = _context.Areas
                .Where(a => a.SecretariaId == secretariaId)
                .Select(a => new { id = a.AreaId, nombre = a.NombreArea })
                .ToList();

            return Json(new
            {
                employees,
                secretarias,
                areas
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OvertimeHour horasExtra)
        {
            ModelState.Remove("Area");
            ModelState.Remove("Secretaria");
            ModelState.Remove("Empleado");

            if (horasExtra.FechaHoraInicio >= horasExtra.FechaHoraFin)
            {
                return Json(new
                {
                    success = false,
                    message = "La fecha y hora de inicio deben ser anteriores a la fecha y hora de fin."
                });
            }

            if (ModelState.IsValid)
            {
                var empleado = await _context.Employees
                    .Where(e => e.EmpleadoId == horasExtra.EmpleadoId)
                    .Select(e => new { e.AreaId, e.SecretariaId })
                    .FirstOrDefaultAsync();

                if (empleado == null)
                {
                    return Json(new { success = false, message = "El empleado seleccionado no existe." });
                }

                horasExtra.AreaId = empleado.AreaId ?? 0; 
                horasExtra.SecretariaId = empleado.SecretariaId;

                var solapamiento = await _context.OvertimeHours
                    .AnyAsync(h => h.EmpleadoId == horasExtra.EmpleadoId &&
                                   h.FechaHoraInicio.Date == horasExtra.FechaHoraInicio.Date &&
                                   !(h.FechaHoraFin <= horasExtra.FechaHoraInicio || h.FechaHoraInicio >= horasExtra.FechaHoraFin));

                if (solapamiento)
                {
                    return Json(new { success = false, message = "Las horas extras se solapan con otras ya registradas." });
                }

                _context.Add(horasExtra);
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


