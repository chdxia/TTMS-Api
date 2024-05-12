namespace TTMS.Service
{
    public interface IAuthPermissionService
    {
        Task<bool> HasPermissionAsync(string interfaceName, string userId);
    }
}
