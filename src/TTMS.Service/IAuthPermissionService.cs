namespace TTMS.Service
{
    public interface IAuthPermissionService
    {
        /// <summary>
        /// 递归获取权限列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<List<AuthPermissionWithChildrenResponse>> GetAuthPermissionAsync(AuthPermissionRequest request);

        Task<bool> HasPermissionAsync(string interfaceName, string userId);
    }
}
