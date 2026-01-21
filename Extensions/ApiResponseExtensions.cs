using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos.Common;
using System.Collections.Generic;
using System.Linq;

namespace MonitoringDokumenGS.Extensions
{
    public static class ApiResponseExtensions
    {
        public static IActionResult OkResponse<T>(this ControllerBase controller, T? data, string message = "")
            => controller.Ok(new ApiResponse<T> { Success = true, Message = message, Data = data, StatusCode = 200 });

        public static IActionResult CreatedResponse<T>(this ControllerBase controller, T? data, string message = "")
            => controller.StatusCode(201, new ApiResponse<T> { Success = true, Message = message, Data = data, StatusCode = 201 });

        public static IActionResult ErrorResponse(this ControllerBase controller, string message, int statusCode = 500, IEnumerable<string>? errors = null)
            => controller.StatusCode(statusCode, new ApiResponse<object> { Success = false, Message = message, Errors = errors, StatusCode = statusCode });

        public static IActionResult ValidationErrorResponse(this ControllerBase controller)
        {
            var errors = controller.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m));
            return controller.BadRequest(new ApiResponse<object> { Success = false, Message = "Validation failed", Errors = errors, StatusCode = 400 });
        }
    }
}
