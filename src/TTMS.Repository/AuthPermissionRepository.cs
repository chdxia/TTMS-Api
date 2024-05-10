namespace TTMS.Repository
{
    public class AuthPermissionRepository : DefaultRepository<AuthPermission, long>, IAuthPermissionRepository
    {
        private readonly IFreeSql _fsql;
        private readonly IMapper _mapper;
        private readonly string? _accessUserId;

        public AuthPermissionRepository(IFreeSql fsql, IMapper mapper, IHttpContextAccessor contextAccessor) : base(fsql)
        {
            _fsql = fsql;
            _mapper = mapper;
            _accessUserId = contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 获取权限嵌套列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<AuthPermissionWithChildrenResponse>> GetAuthPermissionListAsync(AuthPermissionRequest request)
        {
            // 获取所有权限列表
            var allAuthPermission = await _fsql.Select<AuthPermission>().Where(a => !a.IsDelete).ToListAsync<AuthPermissionWithChildrenResponse>();
            var createByAndUpdateByIds = allAuthPermission.Select(item => item.CreateBy).Union(allAuthPermission.Select(item => item.UpdateBy)).Distinct();
            var createByAndUpdateByUsers = await _fsql.Select<User>().Where(a => createByAndUpdateByIds.Contains(a.Id)).ToListAsync();
            foreach (var item in allAuthPermission)
            {
                item.CreateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item.CreateBy)?.UserName;
                item.UpdateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item?.UpdateBy)?.UserName;
            }

            // 获取根节点权限列表
            var query = allAuthPermission.Where(a => a.ParentId == null && a.Level == 0);
            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(a => a.Name.Contains(request.Name));
            }
            if (request.IsDisable.HasValue)
            {
                query = query.Where(a => a.IsDisable == request.IsDisable);
            }
            var listResponse = query.OrderByDescending(a => a.Sort).ToList();

            // 递归获取子权限
            async Task<List<AuthPermissionWithChildrenResponse>> GetChildrenAuthPermissionRecursiveAsync(int parentId)
            {
                var children = allAuthPermission.Where(a => a.ParentId == parentId).OrderByDescending(a => a.Sort).ToList();
                foreach (var child in children)
                {
                    child.Children = await GetChildrenAuthPermissionRecursiveAsync(child.Id);
                }
                return children;
            }

            // 为每个权限获取子权限
            foreach (var permissionResponse in listResponse)
            {
                permissionResponse.Children = await GetChildrenAuthPermissionRecursiveAsync(permissionResponse.Id);
            }
            return listResponse;
        }

        /// <summary>
        /// 新增权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthPermissionResponse> InsertAuthPermissionAsync(CreateAuthPermissionRequest request)
        {
            var model = _mapper.Map<CreateAuthPermissionRequest, AuthPermission>(request);
            model.CreateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.CreateBy;
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            try
            {
                await InsertAsync(model);
                return _mapper.Map<AuthPermission, AuthPermissionResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 编辑权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthPermissionResponse> UpdateAuthPermissionAsync(UpdateAuthPermissionRequest request)
        {
            var model = await _fsql.Select<AuthPermission>().Where(a => a.Id == request.Id).FirstAsync();
            if(model == null)
            {
                throw new Exception("AuthPermission does not exist.");
            }
            _mapper.Map(request, model);
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            model.UpdateTime = DateTime.Now;
            try
            {
                await UpdateAsync(model);
                return _mapper.Map<AuthPermission, AuthPermissionResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task DeleteAuthPermissionAsync(DeleteAuthPermissionRequest request)
        {
            if (!request.AuthPermissionIds.Any())
            {
                throw new Exception("权限Id为空，请填写有效权限Id.");
            }
            var existingAuthPermissionIds = await _fsql.Select<AuthPermission>()
                .Where(a => request.AuthPermissionIds.Contains(a.Id))
                .ToListAsync();
            var nonExistingAuthPermissionIds = request.AuthPermissionIds.Except(existingAuthPermissionIds.Select(a => a.Id));
            if (nonExistingAuthPermissionIds.Any())
            {
                throw new Exception($"删除失败，以下权限ID不存在: {string.Join(", ", nonExistingAuthPermissionIds)}.");
            }
            var update = _fsql.Update<AuthPermission>()
                .Set(a => a.IsDelete, true)
                .Set(a => a.UpdateTime, DateTime.Now)
                .Where(a => request.AuthPermissionIds.Contains(a.Id));
            update = _accessUserId != null ? update.Set(a => a.UpdateBy, int.Parse(_accessUserId)) : update;
            var affectedRows = await update.ExecuteAffrowsAsync();
            if (affectedRows <= 0)
            {
                throw new Exception("删除失败.");
            }
        }
    }
}
