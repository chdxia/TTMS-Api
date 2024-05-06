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
        /// 获取权限列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<AuthPermissionResponse>> GetAuthPermissionListAsync(AuthPermissionRequest request)
        {
            var query = _fsql.Select<AuthPermission>()
                .Where(a => !a.IsDelete)
                .WhereIf(!string.IsNullOrEmpty(request.Name), a => a.Name.Contains(request.Name))
                .WhereIf(request.IsDisable.HasValue, a => a.IsDisable == request.IsDisable)
                .WhereIf(request.CreateTimeStart.HasValue, a => a.CreateTime >= request.CreateTimeStart)
                .WhereIf(request.CreateTimeEnd.HasValue, a => a.CreateTime <= request.CreateTimeEnd)
                .WhereIf(request.CreateBy.HasValue, a => a.CreateBy == request.CreateBy)
                .WhereIf(request.UpdateTimeStart.HasValue, a => a.UpdateTime >= request.UpdateTimeStart)
                .WhereIf(request.UpdateTimeEnd.HasValue, a => a.UpdateTime <= request.UpdateTimeEnd)
                .WhereIf(request.UpdateBy.HasValue, a => a.UpdateBy == request.UpdateBy)
                .OrderByDescending(a => a.CreateTime);
            var listResponse = await query.ToListAsync<AuthPermissionResponse>();
            var createByAndUpdateByIds = listResponse.Select(item => item.CreateBy).Union(listResponse.Select(item => item.UpdateBy)).Distinct();
            var createByAndUpdateByUsers = await _fsql.Select<User>().Where(a => createByAndUpdateByIds.Contains(a.Id)).ToListAsync();
            foreach (var item in listResponse)
            {
                item.CreateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item.CreateBy)?.UserName;
                item.UpdateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item.UpdateBy)?.UserName;
            }
            return listResponse;
        }

        /// <summary>
        /// 分页获取权限列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PageListAuthPermissionResponse> GetAuthPermissionPageListAsync(AuthPermissionRequest request)
        {
            var query = _fsql.Select<AuthPermission>()
                .Where(a => !a.IsDelete)
                .WhereIf(!string.IsNullOrEmpty(request.Name), a => a.Name.Contains(request.Name))
                .WhereIf(request.IsDisable.HasValue, a => a.IsDisable == request.IsDisable)
                .WhereIf(request.CreateTimeStart.HasValue, a => a.CreateTime >= request.CreateTimeStart)
                .WhereIf(request.CreateTimeEnd.HasValue, a=> a.CreateTime <= request.CreateTimeEnd)
                .WhereIf(request.CreateBy.HasValue, a=> a.CreateBy == request.CreateBy)
                .WhereIf(request.UpdateTimeStart.HasValue, a=> a.UpdateTime >= request.UpdateTimeStart)
                .WhereIf(request.UpdateTimeEnd.HasValue, a=> a.UpdateTime <= request.UpdateTimeEnd)
                .WhereIf(request.UpdateBy.HasValue, a=> a.UpdateBy == request.UpdateBy)
                .OrderByDescending(a => a.CreateTime);
            var totalCount = await query.CountAsync();
            var authPermissionItems = await query.Page(request.PageIndex, request.PageSize).ToListAsync<AuthPermissionResponse>();
            var createByAndUpdateByIds = authPermissionItems.Select(item => item.CreateBy).Union(authPermissionItems.Select(item => item.UpdateBy)).Distinct();
            var createByAndUpdateByUsers = await _fsql.Select<User>().Where(a => createByAndUpdateByIds.Contains(a.Id)).ToListAsync();
            foreach (var item in authPermissionItems)
            {
                item.CreateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item.CreateBy)?.UserName;
                item.UpdateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item.UpdateBy)?.UserName;
            }
            var pageListResponse = new PageListAuthPermissionResponse
            {
                Items = authPermissionItems,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
            return pageListResponse;
        }

        /// <summary>
        /// 新增权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AuthPermissionResponse> InsertAuthPermissionAsync(CreateAuthPermissionRequest request)
        {
            var model = _mapper.Map<CreateAuthPermissionRequest, AuthPermission>(request);
            if (_accessUserId != null)
            {
                model.CreateBy = model.UpdateBy = int.Parse(_accessUserId);
            }
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
            if (_accessUserId != null)
            {
                model.UpdateBy = int.Parse(_accessUserId);
            }
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
                throw new Exception($"删除失败，以下分组ID不存在: {string.Join(", ", nonExistingAuthPermissionIds)}.");
            }
            var update = _fsql.Update<AuthPermission>()
                .Set(a => a.IsDelete, true)
                .Set(a => a.UpdateTime, DateTime.Now)
                .Where(a => request.AuthPermissionIds.Contains(a.Id));
            if (_accessUserId != null)
            {
                update = update.Set(a => a.UpdateBy, int.Parse(_accessUserId));
            }
            var affectedRows = await update.ExecuteAffrowsAsync();
            if (affectedRows <= 0)
            {
                throw new Exception("删除失败.");
            }
        }
    }
}
