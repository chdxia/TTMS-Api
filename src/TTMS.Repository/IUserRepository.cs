namespace TTMS.Repository
{
    public interface IUserRepository : IBaseRepository<User, long>
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserLoginResponse> UserLoginAsync(UserLoginRequest request);

        /// <summary>
        /// 根据id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<UserResponse> GetUserByIdAsync(int id);

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<List<UserResponse>> GetUserListAsync(UserRequest request);

        /// <summary>
        /// 分页获取用户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PageListUserResponse> GetUserPageListAsync(UserRequest request);

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserResponse> InsertUserAsync(CreateUserRequest request);

        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserResponse> UpdateUserAsync(UpdateUserRequest request);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task DeleteUserAsync(DeleteUserRequest request);

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<AuthUserPermissionResponse>> GetUserPermissionListAsync(int userId);

        /// <summary>
        /// 编辑用户权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task UpdateUserPermissionAsync(UpdateAuthUserPermissionRequest request);
    }
}
