namespace TTMS.Domain
{
    /// <summary>
    /// 权限表
    /// </summary>
    [Table(Name = "auth_permission")]
    public class AuthPermission
    {
        /// <summary>
        /// 主键id;权限id
        /// </summary>
        [Column(Name = "id", DbType = "int8", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 权限名称
        /// </summary>
        [Column(Name = "name", DbType = "varchar", IsNullable = true)]
        public string? Name { get; set; }

        /// <summary>
        /// 权限码
        /// </summary>
        [Column(Name = "code", DbType = "varchar")]
        public string? Code { get; set; }

        /// <summary>
        /// 权限描述
        /// </summary>
        [Column(Name = "description", DbType = "varchar", IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 父级权限id
        /// </summary>
        [Column(Name = "parent_id", DbType = "int8", IsNullable = true)]
        public int? ParentId { get; set; }

        /// <summary>
        /// 权限层级
        /// </summary>
        [Column(Name = "level", DbType = "int8")]
        public int Level { get; set; }

        /// <summary>
        /// url
        /// </summary>
        [Column(Name = "url", DbType = "varchar", IsNullable = true)]
        public string? Url { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Column(Name = "sort", DbType = "int8")]
        public int Sort { get; set; }

        /// <summary>
        /// 是否禁用;t禁用;f启用
        /// </summary>
        [Column(Name = "is_disable", DbType = "bool")]
        public bool IsDisable { get; set; }

        /// <summary>
        /// 是否删除;t已删除:f未删除;默认未删除
        /// </summary>
        [Column(Name = "is_delete", DbType = "bool")]
        public bool IsDelete { get; set; } = false;

        /// <summary>
        /// 创建人
        /// </summary>
        [Column(Name = "create_by", DbType = "int8")]
        public int CreateBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "create_time", DbType = "timestamp")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新人
        /// </summary>
        [Column(Name = "update_by", DbType = "int8")]
        public int UpdateBy { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Column(Name = "update_time", DbType = "timestamp")]
        public DateTime? UpdateTime { get; set; } = DateTime.Now;
    }
}
