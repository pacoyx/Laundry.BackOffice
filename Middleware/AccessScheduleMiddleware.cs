using System.Text.Json;

public class AccessScheduleMiddleware
{
    private readonly RequestDelegate _next;

    public AccessScheduleMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var windows = httpContext.User.FindAll(AccessScheduleWindow.ClaimType)
                .Select(c => AccessScheduleWindow.TryParse(c.Value))
                .Where(w => w != null)
                .Select(w => w!)
                .ToList();

            if (windows.Count > 0 && !AccessScheduleEvaluator.IsWithinSchedule(windows, PeruClock.Now))
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                httpContext.Response.ContentType = "application/json";

                var response = new ApiResponse<string>
                {
                    Success = false,
                    Message = "Fuera de su horario de acceso permitido.",
                    StatusCode = StatusCodes.Status403Forbidden,
                    Data = ""
                };

                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }
        }

        await _next(httpContext);
    }
}
