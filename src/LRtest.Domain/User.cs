﻿namespace LRtest.Domain
{
    /// <summary>
    /// 用户表
    /// </summary>
    [Table(Name = "user")]
    public class User
    {
        /// <summary>
        /// 主键id;用户id
        /// </summary>
        [Column(Name = "id", DbType = "int8", IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 账号;唯一
        /// </summary>
        [Column(Name = "account", DbType = "varchar")]
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        [Column(Name = "user_name", DbType = "varchar")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 用户邮箱;唯一
        /// </summary>
        [Column(Name = "email", DbType = "varchar")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 用户密码
        /// </summary>
        [Column(Name = "password", DbType = "varchar", IsNullable = true)]
        public string? PassWord { get; set; }

        /// <summary>
        /// 账号token
        /// </summary>
        [Column(Name = "access_token", DbType = "varchar", IsNullable = true)]
        public string? AccessToken { get; set; }

        /// <summary>
        /// 账号状态;t启用,f停用
        /// </summary>
        [Column(Name = "state", DbType = "bool")]
        public bool State { get; set; }

        /// <summary>
        /// 是否删除;t已删除,f未删除
        /// </summary>
        [Column(Name = "is_delete", DbType = "bool")]
        public bool IsDelete { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "create_time", DbType = "timestamp")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Column(Name = "create_by", DbType = "int8")]
        public long CreateBy { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column(Name = "update_time", DbType = "timestamp")]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [Column(Name = "update_by", DbType = "int8")]
        public long UpdateBy { get; set; }
    }
}