namespace TTMS.DTO.Request
{
    /// <summary>
    /// 请求参数;查询权限
    /// </summary>
    public class AuthPermissionRequest
    {
        /// <summary>
        /// 权限id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 权限名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 是否禁用;t禁用;f启用
        /// </summary>
        public bool? IsDisable { get; set; }
    }

    /// <summary>
    /// 请求参数;新建权限
    /// </summary>
    public class CreateAuthPermissionRequest
    {
        /// <summary>
        /// 权限名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 权限码
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 权限描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 父级权限码
        /// </summary>
        public string? ParentCode { get; set; }

        /// <summary>
        /// url
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否禁用;t禁用;f启用
        /// </summary>
        public bool IsDisable { get; set; }
    }

    /// <summary>
    /// 请求参数;编辑权限
    /// </summary>
    public class UpdateAuthPermissionRequest : CreateAuthPermissionRequest
    {
        /// <summary>
        /// 权限id
        /// </summary>
        [Required(ErrorMessage = "Id is required.")]
        public int Id { get; set; }
    }

    /// <summary>
    /// 请求参数;批量删除权限
    /// </summary>
    public class DeleteAuthPermissionRequest
    {
        /// <summary>
        /// 权限id
        /// </summary>
        [Required(ErrorMessage = "AuthPermissionIds is required.")]
        public List<int> AuthPermissionIds { get; set; } = new List<int>();
    }
}
