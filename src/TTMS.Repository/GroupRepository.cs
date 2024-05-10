namespace TTMS.Repository
{
    public class GroupRepository : DefaultRepository<Group, long>, IGroupRepository
    {
        private readonly IFreeSql _fsql;
        private readonly IMapper _mapper;
        private readonly string? _accessUserId;

        public GroupRepository(IFreeSql fsql, IMapper mapper, IHttpContextAccessor contextAccessor) : base(fsql)
        {
            _fsql = fsql;
            _mapper = mapper;
            _accessUserId = contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 获取分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<GroupResponse>> GetGroupListAsync(GroupRequest request)
        {
            var query = _fsql.Select<Group>()
                .Where(a => !a.IsDelete)
                .WhereIf(!string.IsNullOrEmpty(request.GroupName), a => a.GroupName.Contains(request.GroupName))
                .WhereIf(request.CreateTimeStart.HasValue, a => a.CreateTime >= request.CreateTimeStart)
                .WhereIf(request.CreateTimeEnd.HasValue, a => a.CreateTime <= request.CreateTimeEnd)
                .WhereIf(request.CreateBy.HasValue, a => a.CreateBy == request.CreateBy)
                .WhereIf(request.UpdateTimeStart.HasValue, a => a.UpdateTime >= request.UpdateTimeStart)
                .WhereIf(request.UpdateTimeEnd.HasValue, a => a.UpdateTime <= request.UpdateTimeEnd)
                .WhereIf(request.UpdateBy.HasValue, a => a.UpdateBy == request.UpdateBy)
                .OrderByDescending(a => a.CreateTime);
            var listResponse = await query.ToListAsync<GroupResponse>();
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
        /// 分页获取分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PageListGroupResponse> GetGroupPageListAsync(GroupRequest request)
        {
            var query = _fsql.Select<Group>()
                .Where(a => !a.IsDelete)
                .WhereIf(!string.IsNullOrEmpty(request.GroupName), a => a.GroupName.Contains(request.GroupName))
                .WhereIf(request.CreateTimeStart.HasValue, a => a.CreateTime >= request.CreateTimeStart)
                .WhereIf(request.CreateTimeEnd.HasValue, a=> a.CreateTime <= request.CreateTimeEnd)
                .WhereIf(request.CreateBy.HasValue, a=> a.CreateBy == request.CreateBy)
                .WhereIf(request.UpdateTimeStart.HasValue, a=> a.UpdateTime >= request.UpdateTimeStart)
                .WhereIf(request.UpdateTimeEnd.HasValue, a=> a.UpdateTime <= request.UpdateTimeEnd)
                .WhereIf(request.UpdateBy.HasValue, a=> a.UpdateBy == request.UpdateBy)
                .OrderByDescending(a => a.CreateTime);
            var totalCount = await query.CountAsync();
            var groupItems = await query.Page(request.PageIndex, request.PageSize).ToListAsync<GroupResponse>();
            var createByAndUpdateByIds = groupItems.Select(item => item.CreateBy).Union(groupItems.Select(item => item.UpdateBy)).Distinct();
            var createByAndUpdateByUsers = await _fsql.Select<User>().Where(a => createByAndUpdateByIds.Contains(a.Id)).ToListAsync();
            foreach (var item in groupItems)
            {
                item.CreateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item.CreateBy)?.UserName;
                item.UpdateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item.UpdateBy)?.UserName;
            }
            var pageListResponse = new PageListGroupResponse
            {
                Items = groupItems,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
            return pageListResponse;
        }

        /// <summary>
        /// 新增分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GroupResponse> InsertGroupAsync(CreateGroupRequest request)
        {
            var model = _mapper.Map<CreateGroupRequest, Group>(request);
            model.CreateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.CreateBy;
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            try
            {
                await InsertAsync(model);
                return _mapper.Map<Group, GroupResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 编辑分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GroupResponse> UpdateGroupAsync(UpdateGroupRequest request)
        {
            var model = await _fsql.Select<Group>().Where(a => a.Id == request.Id).FirstAsync();
            if(model == null)
            {
                throw new Exception("Group does not exist.");
            }
            _mapper.Map(request, model);
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            model.UpdateTime = DateTime.Now;
            try
            {
                await UpdateAsync(model);
                return _mapper.Map<Group, GroupResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 删除分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task DeleteGroupAsync(DeleteGroupRequest request)
        {
            if (!request.GroupIds.Any())
            {
                throw new Exception("分组Id为空，请填写有效分组Id.");
            }
            var existingGroupIds = await _fsql.Select<Group>()
                .Where(a => request.GroupIds.Contains(a.Id))
                .ToListAsync();
            var nonExistingGroupIds = request.GroupIds.Except(existingGroupIds.Select(a => a.Id));
            if (nonExistingGroupIds.Any())
            {
                throw new Exception($"删除失败，以下分组ID不存在: {string.Join(", ", nonExistingGroupIds)}.");
            }
            var update = _fsql.Update<Group>()
                .Set(a => a.IsDelete, true)
                .Set(a => a.UpdateTime, DateTime.Now)
                .Where(a => request.GroupIds.Contains(a.Id));
            update = _accessUserId != null ? update.Set(a => a.UpdateBy, int.Parse(_accessUserId)) : update;
            var affectedRows = await update.ExecuteAffrowsAsync();
            if (affectedRows <= 0)
            {
                throw new Exception("删除失败.");
            }
        }
    }
}
