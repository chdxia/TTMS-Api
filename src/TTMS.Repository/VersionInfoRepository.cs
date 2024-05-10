namespace TTMS.Repository
{
    public class VersionInfoRepository : DefaultRepository<VersionInfo, long>, IVersionInfoRepository
    {
        private readonly IFreeSql _fsql;
        private readonly IMapper _mapper;
        private readonly string? _accessUserId;

        public VersionInfoRepository(IFreeSql fsql, IMapper mapper, IHttpContextAccessor contextAccessor) : base(fsql)
        {
            _fsql = fsql;
            _mapper = mapper;
            _accessUserId = contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 分页获取版本列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PageListVersionInfoResponse> GetVersionInfoPageListAsync(VersionInfoRequest request)
        {
            var query = _fsql.Select<VersionInfo>()
                .Where(a => !a.IsDelete)
                .WhereIf(request.Id.HasValue, a => a.Id == request.Id)
                .WhereIf(request.VertionTime.HasValue, a => a.VersionTimeStart <= request.VertionTime)
                .WhereIf(request.VertionTime.HasValue, a => request.VertionTime <= a.VersionTimeEnd)
                .WhereIf(!string.IsNullOrEmpty(request.VersionNo), a => a.VersionNo.Contains(request.VersionNo))
                .WhereIf(request.CreateTimeStart.HasValue, a => a.CreateTime >= request.CreateTimeStart)
                .WhereIf(request.CreateTimeEnd.HasValue, a => a.CreateTime <= request.CreateTimeEnd)
                .WhereIf(request.CreateBy.HasValue, a => a.CreateBy == request.CreateBy)
                .WhereIf(request.UpdateTimeStart.HasValue, a => a.UpdateTime >= request.UpdateTimeStart)
                .WhereIf(request.UpdateTimeEnd.HasValue, a => a.UpdateTime <= request.UpdateTimeEnd)
                .WhereIf(request.UpdateBy.HasValue, a => a.UpdateBy == request.UpdateBy);
            var totalCount = await query.CountAsync();
            var versionInfoItems = await query.Page(request.PageIndex, request.PageSize).ToListAsync<VersionInfoResponse>();
            var createByAndUpdateByIds = versionInfoItems.Select(item => item.CreateBy).Union(versionInfoItems.Select(item => item.UpdateBy)).Distinct();
            var createByAndUpdateByUsers = await _fsql.Select<User>().Where(a => createByAndUpdateByIds.Contains(a.Id)).ToListAsync();
            var versionIds = versionInfoItems.Select(item => item.Id).Distinct();
            var demandVersionInfos = await _fsql.Select<DemandVersionInfo>().Where(a => !a.IsDelete).Where(a => versionIds.Contains(a.VersionInfoId)).ToListAsync();
            var demandVersionInfoIds = demandVersionInfos.Select(a => a.DemandId).Distinct();
            var demands = await _fsql.Select<Demand>()
                .Where(a => !a.IsDelete)
                .Where(a => demandVersionInfoIds.Contains(a.Id))
                .ToListAsync<DemandResponse>();
            foreach (var item in versionInfoItems)
            {
                item.CreateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item.CreateBy)?.UserName;
                item.UpdateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item.UpdateBy)?.UserName;
                var demandItems = demands
                    .Where(a => demandVersionInfos.Where(demandVersionInfo => demandVersionInfo.VersionInfoId == item.Id).Select(a => a.DemandId).Distinct().Contains(a.Id))
                    .Distinct();
                if (demandItems != null && !demandItems.All(demand => demand.DemandState >= DemandState.已上线)) // 如果该版本下存在未上线需求，则表示版本任务未完成
                {
                    item.VersionState = "任务未完成";
                }
            }
            var pageListResponse = new PageListVersionInfoResponse
            {
                Items = versionInfoItems,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
            return pageListResponse;
        }

        /// <summary>
        /// 新增版本
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<VersionInfoResponse> InsertVersionInfoAsync(CreateVersionInfoRequest request)
        {
            var model = _mapper.Map<CreateVersionInfoRequest, VersionInfo>(request);
            model.CreateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.CreateBy;
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            try
            {
                await InsertAsync(model);
                return _mapper.Map<VersionInfo, VersionInfoResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 编辑版本
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<VersionInfoResponse> UpdateVersionInfoAsync(UpdateVersionInfoRequest request)
        {
            var model = await _fsql.Select<VersionInfo>().Where(a => a.Id == request.Id).FirstAsync();
            if (model == null)
            {
                throw new Exception("Group does not exist.");
            }
            _mapper.Map(request, model);
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            model.UpdateTime = DateTime.Now;
            try
            {
                await UpdateAsync(model);
                return _mapper.Map<VersionInfo, VersionInfoResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 删除版本
        /// </summary>
        /// <returns></returns>
        public async Task DeleteVersionInfoAsync(DeleteVersionInfoRequest request)
        {
            if (!request.VersionIds.Any())
            {
                throw new Exception("版本Id为空，请填写有效版本Id.");
            }
            var existingGroupIds = await _fsql.Select<VersionInfo>()
                .Where(a => request.VersionIds.Contains(a.Id))
                .ToListAsync();
            var nonExistingGroupIds = request.VersionIds.Except(existingGroupIds.Select(a => a.Id));
            if (nonExistingGroupIds.Any())
            {
                throw new Exception($"删除失败，以下版本ID不存在: {string.Join(", ", nonExistingGroupIds)}.");
            }
            var update = _fsql.Update<VersionInfo>()
                .Set(a => a.IsDelete, true)
                .Set(a => a.UpdateTime, DateTime.Now)
                .Where(a => request.VersionIds.Contains(a.Id));
            update = _accessUserId != null ? update.Set(a => a.UpdateBy, int.Parse(_accessUserId)) : update;
            var affectedRows = await update.ExecuteAffrowsAsync();
            if (affectedRows <= 0)
            {
                throw new Exception("删除失败.");
            }
        }
    }
}
