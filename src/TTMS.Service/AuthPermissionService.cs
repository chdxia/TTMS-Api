namespace TTMS.Service
{
    public class AuthPermissionService : IAuthPermissionService
    {
        private readonly IAuthPermissionRepository _authPermissionRepository;
        public AuthPermissionService(IAuthPermissionRepository authPermissionRepository)
        {
            _authPermissionRepository = authPermissionRepository;
        }

        /// <summary>
        /// 递归获取权限列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<AuthPermissionWithChildrenResponse>> GetAuthPermissionAsync(AuthPermissionRequest request)
        {
            var authPermissionWithoutParentList = await _authPermissionRepository.GetAuthPermissionWithoutParentListAsync(request);
            var listResponse = new List<AuthPermissionWithChildrenResponse>();
            foreach (var permissionResponse in authPermissionWithoutParentList)
            {
                permissionResponse.Children = await _authPermissionRepository.GetChildrenAuthPermissionRecursiveAsync(permissionResponse.Code);
                listResponse.Add(permissionResponse);
            }
            return listResponse;
        }

        public async Task<bool> HasPermissionAsync(string interfaceName, string userId)
        {
            // 在数据库中查询用户的权限信息

            // 判断用户是否有访问该接口的权限
            await Task.CompletedTask;
            return true;
        }
    }
}
