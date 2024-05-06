namespace TTMS.Repository
{
    public interface IAuthPermissionRepository : IBaseRepository<AuthPermission, long>
    {
        /// <summary>
        /// 获取分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<List<AuthPermissionResponse>> GetAuthPermissionListAsync(AuthPermissionRequest request);

        /// <summary>
        /// 分页获取分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PageListAuthPermissionResponse> GetAuthPermissionPageListAsync(AuthPermissionRequest request);

        /// <summary>
        /// 新增分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AuthPermissionResponse> InsertAuthPermissionAsync(CreateAuthPermissionRequest request);

        /// <summary>
        /// 编辑分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AuthPermissionResponse> UpdateAuthPermissionAsync(UpdateAuthPermissionRequest request);

        /// <summary>
        /// 删除分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task DeleteAuthPermissionAsync(DeleteAuthPermissionRequest request);
    }
}
