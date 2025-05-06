using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace OvertimeControlCodeFirst.Services
{
    public class CustomCircuitHandler : CircuitHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CustomCircuitHandler> _logger;

        public CustomCircuitHandler(IHttpContextAccessor httpContextAccessor, ILogger<CustomCircuitHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                _logger.LogInformation("Usuario desconectado, saliendo...");
                await httpContext.SignOutAsync();
            }

            await base.OnCircuitClosedAsync(circuit, cancellationToken);
        }
    }
}
