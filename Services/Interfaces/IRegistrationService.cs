using LmsApi.Models.DTOs;
using LmsApi.Utils;

namespace LmsApi.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<Result<string>?> RegisterAsync(RegisterDto model, string baseUrl);
    }
}
