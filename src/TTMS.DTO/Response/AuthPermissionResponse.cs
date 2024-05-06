﻿namespace TTMS.DTO.Response
{
    /// <summary>
    /// 返回参数;权限
    /// </summary>
    public class AuthPermissionResponse
    {
        /// <summary>
        /// 权限id
        /// </summary>
        public int Id { get; set; }

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

        /// <summary>
        /// 创建人
        /// </summary>
        public int CreateBy { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string? CreateByName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后更新人
        /// </summary>
        public int UpdateBy { get; set; }

        /// <summary>
        /// 最后更新人
        /// </summary>
        public string? UpdateByName { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }

    /// <summary>
    /// 返回分页数据;权限
    /// </summary>
    public class PageListAuthPermissionResponse : BasePageListResponse
    {
        /// <summary>
        /// Items
        /// </summary>
        public new List<AuthPermissionResponse> Items { get; set; } = new List<AuthPermissionResponse>();
    }
}
