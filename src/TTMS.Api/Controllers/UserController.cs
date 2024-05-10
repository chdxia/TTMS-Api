namespace TTMS.Api.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    [ApiExplorerSettings(GroupName = "用户")]
    public class UserController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="userRepository"></param>
        /// <param name="userService"></param>
        public UserController(IUserRepository userRepository, IUserService userService)
        {
            _userRepository = userRepository;
            _userService = userService;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel<UserLoginResponse>))]
        public async Task<IActionResult> UserLogin([FromBody] UserLoginRequest request)
        {
            var result = await _userService.UserLoginAsync(request);
            return ToSuccessResult(result);
        }

        /// <summary>
        /// 根据id获取用户信息
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel<UserResponse>))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _userRepository.GetUserByIdAsync(id);
            return ToSuccessResult(result);
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost("GetList")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel<List<UserResponse>>))]
        public async Task<IActionResult> GetListAsync([FromBody] UserRequest request)
        {
            var result = await _userRepository.GetUserListAsync(request);
            return ToSuccessResult(result);
        }

        /// <summary>
        /// 分页获取用户列表
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost("GetPageList")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel<PageListUserResponse>))]
        public async Task<IActionResult> GetPageListAsync([FromBody] UserRequest request)
        {
            var result = await _userRepository.GetUserPageListAsync(request);
            return ToSuccessResult(result);
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("CreateUser")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel<UserResponse>))]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequest request)
        {
            var result = await _userRepository.InsertUserAsync(request);
            return ToSuccessResult(result);
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateUser")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel<UserResponse>))]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserRequest request)
        {
            var result = await _userRepository.UpdateUserAsync(request);
            return ToSuccessResult(result);
        }

        /// <summary>
        /// 批量删除用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("DeleteUser")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel))]
        public async Task<IActionResult> DeleteUserAsync([FromBody] DeleteUserRequest request)
        {
            await _userRepository.DeleteUserAsync(request);
            return ToSuccessResult();
        }

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/GetUserPermissionList")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel<List<AuthUserPermissionResponse>>))]
        public async Task<IActionResult> GetUserPermissionListAsync(int id)
        {
            var result = await _userRepository.GetUserPermissionListAsync(id);
            return ToSuccessResult(result);
        }

        /// <summary>
        /// 编辑用户权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("UpdateUserPermission")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel))]
        public async Task<IActionResult> UpdateUserPermissionAsync([FromBody] UpdateAuthUserPermissionRequest request)
        {
            await _userRepository.UpdateUserPermissionAsync(request);
            return ToSuccessResult();
        }
    }
}
