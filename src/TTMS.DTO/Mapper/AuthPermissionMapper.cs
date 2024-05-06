namespace TTMS.DTO.Mapper
{
    /// <summary>
    /// 权限表mapper映射
    /// </summary>
    public class AuthPermissionMapper : Profile
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AuthPermissionMapper()
        {
            CreateMap<CreateAuthPermissionRequest, AuthPermission>().ReverseMap();
            CreateMap<UpdateAuthPermissionRequest, AuthPermission>().ReverseMap();
            CreateMap<AuthPermission, AuthPermissionResponse>().ReverseMap();
        }
    }
}
