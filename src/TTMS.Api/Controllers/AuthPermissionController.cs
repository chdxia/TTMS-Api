namespace TTMS.Api.Controllers
{
    /// <summary>
    /// 分组
    /// </summary>
    [Authorize]
    [ApiExplorerSettings(GroupName = "权限")]
    public class AuthPermissionController : BaseApiController
    {
        private readonly IAuthPermissionRepository _authPermissionRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="authPermissionRepository"></param>
        public AuthPermissionController(IAuthPermissionRepository authPermissionRepository)
        {
            _authPermissionRepository = authPermissionRepository;
        }

        /// <summary>
        /// 获取权限列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetList")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel<List<AuthPermissionResponse>>))]
        public async Task<IActionResult> GetListAsync([FromBody] AuthPermissionRequest request)
        {
            var result = await _authPermissionRepository.GetAuthPermissionListAsync(request);
            return ToSuccessResult(result);
        }

        /// <summary>
        /// 分页获取权限列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetPageList")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel<PageListAuthPermissionResponse>))]
        public async Task<IActionResult> GetPageListAsync([FromBody] AuthPermissionRequest request)
        {
            var result = await _authPermissionRepository.GetAuthPermissionPageListAsync(request);
            return ToSuccessResult(result);
        }

        /// <summary>
        /// 新增权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("CreateAuthPermission")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel<AuthPermissionResponse>))]
        public async Task<IActionResult> CreateAuthPermissionAsync([FromBody] CreateAuthPermissionRequest request)
        {
            var result = await _authPermissionRepository.InsertAuthPermissionAsync(request); 
            return ToSuccessResult(result);
        }

        /// <summary>
        /// 编辑权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("UpdateAuthPermission")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel<AuthPermissionResponse>))]
        public async Task<IActionResult> UpdateAuthPermissionAsync([FromBody] UpdateAuthPermissionRequest request)
        {
            var result = await _authPermissionRepository.UpdateAuthPermissionAsync(request);
            return ToSuccessResult(result);
        }

        /// <summary>
        /// 批量删除权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("DeleteAuthPermission")]
        [ProducesResponseType(200, Type = typeof(ApiResultModel))]
        public async Task<IActionResult> DeleteAuthPermissionAsync([FromBody] DeleteAuthPermissionRequest request)
        {
            await _authPermissionRepository.DeleteAuthPermissionAsync(request);
            return ToSuccessResult();
        }
    }
}
