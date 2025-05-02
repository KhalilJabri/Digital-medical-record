using medical.Models;

namespace medical.Services
{
    public interface IAppointmentService
    {
        Task<(string Token, object User)> CreateAppointment(User user);
        Task<string> Login(string username, string password);
    }
}
