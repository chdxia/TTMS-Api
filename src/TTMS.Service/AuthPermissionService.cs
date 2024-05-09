namespace TTMS.Service
{
    public class AuthPermissionService
    {
        public async Task<bool> HasPermissionAsync(string interfaceName, string userId)
        {
            // 在数据库中查询用户的权限信息

            // 判断用户是否有访问该接口的权限
            await Task.CompletedTask;
            return true;
        }
    }
}
