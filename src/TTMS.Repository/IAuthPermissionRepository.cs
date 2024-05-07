namespace TTMS.Repository
{
    public interface IAuthPermissionRepository : IBaseRepository<AuthPermission, long>
    {
        /// <summary>
        /// 获取根节点权限列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<List<AuthPermissionWithChildrenResponse>> GetAuthPermissionWithoutParentListAsync(AuthPermissionRequest request);

        /// <summary>
        /// 递归查询子权限
        /// </summary>
        /// <param name="parentCode"></param>
        /// <returns></returns>
        Task<List<AuthPermissionWithChildrenResponse>> GetChildrenAuthPermissionRecursiveAsync(string? parentCode);

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
