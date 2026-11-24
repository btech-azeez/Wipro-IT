using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EmployeeManagement.Services
{
    [Authorize]
    public class EmployeeNotificationHub : Hub
    {
    }
}