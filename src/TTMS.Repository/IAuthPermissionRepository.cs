namespace TTMS.Repository
{
    public interface IAuthPermissionRepository : IBaseRepository<AuthPermission, long>
    {
        /// <summary>
        /// 获取权限嵌套列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<List<AuthPermissionWithChildrenResponse>> GetAuthPermissionListAsync(AuthPermissionRequest request);

        /// <summary>
        /// 新增权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AuthPermissionResponse> InsertAuthPermissionAsync(CreateAuthPermissionRequest request);

        /// <summary>
        /// 编辑权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AuthPermissionResponse> UpdateAuthPermissionAsync(UpdateAuthPermissionRequest request);

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task DeleteAuthPermissionAsync(DeleteAuthPermissionRequest request);
    }
}
