namespace TTMS.Repository
{
    public class DemandRepository : DefaultRepository<Demand, long>, IDemandRepository
    {
        private readonly IFreeSql _fsql;
        private readonly IMapper _mapper;
        private readonly string? _accessUserId;

        public DemandRepository(IFreeSql fsql, IMapper mapper, IHttpContextAccessor contextAccessor) : base(fsql)
        {
            _fsql = fsql;
            _mapper = mapper;
            _accessUserId = contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 根据id获取需求信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DemandResponse> GetDemandByIdAsync(int id)
        {
            return await _fsql.Select<Demand>().Where(a => a.Id == id).ToOneAsync<DemandResponse>();
        }

        /// <summary>
        /// 分页获取需求列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PageListDemandResponse> GetDemandPageListAsync(DemandRequest request)
        {
            var query = _fsql.Select<Demand, DemandUser, DemandVersionInfo, VersionInfo>()
                .LeftJoin(a => a.t1.Id == a.t2.DemandId)
                .LeftJoin(a => a.t1.Id == a.t3.DemandId)
                .LeftJoin(a => a.t4.Id == a.t3.VersionInfoId)
                .Where(a => !a.t1.IsDelete)
                .WhereIf(request.Id.HasValue, a => a.t1.Id == request.Id)
                .WhereIf(request.GroupId.HasValue, a => a.t1.GroupId == request.GroupId)
                .WhereIf(!string.IsNullOrEmpty(request.ModuleName), a => a.t1.ModuleName.Contains(request.ModuleName))
                .WhereIf(request.DemandType.HasValue, a => a.t1.DemandType == request.DemandType)
                .WhereIf(!string.IsNullOrEmpty(request.DemandName), a => a.t1.DemandName.Contains(request.DemandName))
                .WhereIf(!string.IsNullOrEmpty(request.ProposerName), a => a.t1.ProposerName.Contains(request.ProposerName))
                .WhereIf(request.ProposeTimeStart.HasValue, a => a.t1.ProposeTime >= request.ProposeTimeStart)
                .WhereIf(request.ProposeTimeEnd.HasValue, a => a.t1.ProposeTime <= request.ProposeTimeEnd)
                .WhereIf(request.DemandPriority.HasValue, a => a.t1.DemandPriority == request.DemandPriority)
                .WhereIf(request.DeveloperId.HasValue, a => a.t2.UserId == request.DeveloperId && !a.t2.IsDelete)
                .WhereIf(request.TesterId.HasValue, a => a.t2.UserId == request.TesterId && !a.t2.IsDelete)
                .WhereIf(request.DemandState.HasValue, a => a.t1.DemandState == request.DemandState)
                .WhereIf(request.PlanOnlineTimeStart.HasValue, a => a.t1.PlanOnlineTime >= request.PlanOnlineTimeStart)
                .WhereIf(request.PlanOnlineTimeEnd.HasValue, a => a.t1.PlanOnlineTime <= request.PlanOnlineTimeEnd)
                .WhereIf(request.ActualOnlineTimeStart.HasValue, a => a.t1.ActualOnlineTime >= request.ActualOnlineTimeStart)
                .WhereIf(request.ActualOnlineTimeEnd.HasValue, a => a.t1.ActualOnlineTime <= request.ActualOnlineTimeEnd)
                .WhereIf(!string.IsNullOrEmpty(request.VersionNo), a => a.t4.VersionNo == request.VersionNo)
                .WhereIf(request.CreateTimeStart.HasValue, a => a.t1.CreateTime >= request.CreateTimeStart)
                .WhereIf(request.CreateTimeEnd.HasValue, a => a.t1.CreateTime <= request.CreateTimeEnd)
                .WhereIf(request.CreateBy.HasValue, a => a.t1.CreateBy == request.CreateBy)
                .WhereIf(request.UpdateTimeStart.HasValue, a => a.t1.UpdateTime >= request.UpdateTimeStart)
                .WhereIf(request.UpdateTimeEnd.HasValue, a => a.t1.UpdateTime <= request.UpdateTimeEnd)
                .WhereIf(request.UpdateBy.HasValue, a => a.t1.UpdateBy == request.UpdateBy)
                .Distinct();
            var totalCount = await query.CountAsync();
            var demandItems = await query.Page(request.PageIndex, request.PageSize).ToListAsync<DemandResponse>();
            var demandIds = demandItems.Select(item => item.Id).Distinct();
            var createByAndUpdateByIds = demandItems.Select(item => item.CreateBy).Union(demandItems.Select(item => item.UpdateBy)).Distinct();
            var demandUsers = await _fsql.Select<DemandUser>().Where(a => !a.IsDelete).Where(a => demandIds.Contains(a.DemandId)).ToListAsync();
            var demandUserIds = demandUsers.Select(a => a.UserId).Distinct();
            var demandVersionInfos = await _fsql.Select<DemandVersionInfo>().Where(a => !a.IsDelete).Where(a => demandIds.Contains(a.DemandId)).ToListAsync();
            var demandVersionInfoIds = demandVersionInfos.Select(a => a.VersionInfoId).Distinct();
            var users = await _fsql.Select<User>()
                .Where(a => demandUserIds.Contains(a.Id) || createByAndUpdateByIds.Contains(a.Id))
                .ToListAsync<UserResponse>();
            var versionInfos = await _fsql.Select<VersionInfo>()
                .Where (a => !a.IsDelete)
                .Where(a => demandVersionInfoIds.Contains(a.Id))
                .ToListAsync<VersionInfoResponse>();
            foreach (var item in demandItems)
            {
                item.Developer = users
                    .Where(a => demandUsers.Where(demandUser => demandUser.DemandId == item.Id).Select(a => a.UserId).Distinct().Contains(a.Id) && a.RoleId == RoleType.开发)
                    .ToList();
                item.Tester = users
                    .Where(a => demandUsers.Where(demandUser => demandUser.DemandId == item.Id).Select(a => a.UserId).Distinct().Contains(a.Id) && a.RoleId == RoleType.测试)
                    .ToList();
                item.CreateByName = users.FirstOrDefault(a => a.Id == item.CreateBy)?.UserName;
                item.UpdateByName = users.FirstOrDefault(a => a.Id == item.UpdateBy)?.UserName;
                item.VersionInfos = versionInfos
                    .Where(a => demandVersionInfos.Where(demandVersionInfo => demandVersionInfo.DemandId == item.Id).Select(a => a.VersionInfoId).Distinct().Contains(a.Id))
                    .ToList();
            }
            var pageListResponse = new PageListDemandResponse
            {
                Items = demandItems,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
            return pageListResponse;
        }

        /// <summary>
        /// 新增需求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<DemandResponse> InsertDemandAsync(CreateDemandRequest request)
        {
            var model = _mapper.Map<CreateDemandRequest, Demand>(request);
            model.CreateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.CreateBy;
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            try
            {
                await InsertAsync(model);
                return _mapper.Map<Demand, DemandResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 编辑需求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<DemandResponse> UpdateDemandAsync(UpdateDemandRequest request)
        {
            var model = await _fsql.Select<Demand>().Where(a => a.Id == request.Id).FirstAsync();
            if (model == null)
            {
                throw new Exception("Demand does not exist.");
            }
            _mapper.Map(request, model);
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            model.UpdateTime = DateTime.Now;
            try
            {
                await UpdateAsync(model);
                return _mapper.Map<Demand, DemandResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 修改DemandUser关联表
        /// </summary>
        /// <param name="demandId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public async Task UpdateDemandUserAsync(int demandId, List<int> userIds)
        {
            // 获取数据库中与demandId相关的所有UserDemand记录
            var existingDemandUsers = await _fsql.Select<DemandUser>().Where(a => a.DemandId == demandId).ToListAsync();

            foreach (var existingUserDemand in existingDemandUsers)
            {
                if (userIds.Contains(existingUserDemand.UserId))
                {
                    existingUserDemand.IsDelete = false; // request中存在的关联记录，将IsDelete设置为false
                }
                else
                {
                    existingUserDemand.IsDelete = true; // request中不存在的关联记录，将IsDelete设置为true
                }
            }
            // request中存在，但数据库没有，直接新增关联记录
            var newDemandUsers = userIds.Where(userId => !existingDemandUsers.Any(a => a.UserId == userId))
                .Select(userId => new DemandUser { DemandId = demandId, UserId = userId, IsDelete = false })
                .ToList();
            await _fsql.Update<DemandUser>().SetSource(existingDemandUsers).ExecuteAffrowsAsync();
            await _fsql.Insert<DemandUser>().AppendData(newDemandUsers).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 根据需求批量关联版本;修改DemandVersionInfo关联表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task UpdateDemandVersionInfoAsync(UpdateDemandVersionInfoRequest request)
        {
            // 检查请求数据中是否包含非项目需求
            var otherDemands = await _fsql.Select<Demand>()
                .Where(a => request.DemandIds.Contains(a.Id))
                .Where(a => a.DemandType == Enums.DemandType.非项目需求)
                .ToListAsync();
            if (otherDemands.Any())
            {
                throw new Exception("非项目需求不允许关联版本.");
            }
            // 获取数据库中与demandIds相关的所有DemandVersionInfo记录
            var existingDemandVersionInfos = await _fsql.Select<DemandVersionInfo>()
                .Where(a => request.DemandIds.Contains(a.DemandId))
                .ToListAsync();
            var newDemandVersionInfoList = new List<DemandVersionInfo>();
            foreach (var demandId in request.DemandIds)
            {
                var oneExistingDemandVersionInfos = existingDemandVersionInfos.Where(a => a.DemandId == demandId);
                foreach (var existingDemandVersionInfo in oneExistingDemandVersionInfos)
                {
                    if (request.VersionInfoIds.Contains(existingDemandVersionInfo.VersionInfoId))
                    {
                        existingDemandVersionInfo.IsDelete = false; // 请求数据中已存在的关联记录，将IsDelete设置为false
                    }
                    else
                    {
                        existingDemandVersionInfo.IsDelete = true; // 请求数据中不存在的关联记录，将IsDelete设置为true
                    }
                }
                // 请求数据中存在，但数据库没有，直接新增关联记录
                var newDemandVersionInfos = request.VersionInfoIds.Where(versionInfoId => !oneExistingDemandVersionInfos.Any(a => a.VersionInfoId == versionInfoId))
                    .Select(versionInfoId => new DemandVersionInfo { DemandId = demandId, VersionInfoId = versionInfoId, IsDelete = false})
                    .ToList();
                newDemandVersionInfoList.AddRange(newDemandVersionInfos);
            }
            await _fsql.Update<DemandVersionInfo>().SetSource(existingDemandVersionInfos).ExecuteAffrowsAsync();
            await _fsql.Insert<DemandVersionInfo>().AppendData(newDemandVersionInfoList).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 批量删除需求
        /// </summary>
        /// <returns></returns>
        public async Task DeleteDemandAsync(DeleteDemandRequest request)
        {
            if (!request.DemandIds.Any())
            {
                throw new Exception("需求Id为空，请填写有效需求Id.");
            }
            var existingDemandIds = await _fsql.Select<Demand>()
                .Where(a => request.DemandIds.Contains(a.Id))
                .ToListAsync();
            if (existingDemandIds.Where(a => a.DemandState != Enums.DemandState.待规划).Any())
            {
                throw new Exception("删除失败，只能删除待规划的需求");
            }
            var nonExistingDemandIds = request.DemandIds.Except(existingDemandIds.Select(a => a.Id));
            if (nonExistingDemandIds.Any())
            {
                throw new Exception($"删除失败，以下需求ID不存在: {string.Join(", ", nonExistingDemandIds)}.");
            }
            var update = _fsql.Update<Demand>()
                .Set(a => a.IsDelete, true)
                .Set(a => a.UpdateTime, DateTime.Now)
                .Where(a => request.DemandIds.Contains(a.Id));
            update = _accessUserId != null ? update.Set(a => a.UpdateBy, int.Parse(_accessUserId)) : update;
            var affectedRows = await update.ExecuteAffrowsAsync();
            if (affectedRows <= 0)
            {
                throw new Exception("删除失败.");
            }
        }
    }
}
