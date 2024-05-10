namespace TTMS.Repository
{
    public class DefectRepository : DefaultRepository<Defect, long>, IDefectRepository
    {
        private readonly IFreeSql _fsql;
        private readonly IMapper _mapper;
        private readonly string? _accessUserId;

        public DefectRepository(IFreeSql fsql, IMapper mapper, IHttpContextAccessor contextAccessor) : base(fsql)
        {
            _fsql = fsql;
            _mapper = mapper;
            _accessUserId = contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 根据id获取缺陷信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DefectResponse> GetDefectByIdAsync(int id)
        {
            return await _fsql.Select<Defect>().Where(a => a.Id == id).ToOneAsync<DefectResponse>();
        }

        /// <summary>
        /// 获取缺陷列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<DefectResponse>> GetDefectListAsync(DefectRequest request)
        {
            var query = _fsql.Select<Defect, Demand, DemandUser>()
                .LeftJoin(a => a.t1.DemandId == a.t2.Id)
                .LeftJoin(a => a.t2.Id == a.t3.DemandId)
                .Where(a => !a.t2.IsDelete)
                .WhereIf(request.Id.HasValue, a => a.t1.Id == request.Id)
                .WhereIf(request.GroupId.HasValue, a => a.t2.GroupId == request.GroupId)
                .WhereIf(!string.IsNullOrEmpty(request.ModuleName), a => a.t2.ModuleName == request.ModuleName)
                .WhereIf(!string.IsNullOrEmpty(request.DemandName), a => a.t2.DemandName == request.DemandName)
                .WhereIf(request.DeveloperId.HasValue, a => a.t3.UserId == request.DeveloperId && !a.t3.IsDelete)
                .WhereIf(request.TesterId.HasValue, a => a.t3.UserId == request.TesterId && !a.t3.IsDelete)
                .WhereIf(!string.IsNullOrEmpty(request.Title), a => a.t1.Title == request.Title)
                .WhereIf(request.DefectType.HasValue, a => a.t1.DefectType == request.DefectType)
                .WhereIf(request.DefectState.HasValue, a => a.t1.DefectState == request.DefectState)
                .WhereIf(request.CreateTimeStart.HasValue, a => a.t1.CreateTime >= request.CreateTimeStart)
                .WhereIf(request.CreateTimeEnd.HasValue, a => a.t1.CreateTime <= request.CreateTimeEnd)
                .WhereIf(request.CreateBy.HasValue, a => a.t1.CreateBy == request.CreateBy)
                .WhereIf(request.UpdateTimeStart.HasValue, a => a.t1.UpdateTime >= request.UpdateTimeStart)
                .WhereIf(request.UpdateTimeEnd.HasValue, a => a.t1.UpdateTime <= request.UpdateTimeEnd)
                .WhereIf(request.UpdateBy.HasValue, a => a.t1.UpdateBy == request.UpdateBy)
                .Distinct();
            var listResponse = await query.ToListAsync<DefectResponse>(); // 返回的数据
            var demandIds = listResponse.Select(item => item.DemandId).Distinct(); // 查询返回数据的demandId
            var createByAndUpdateByIds = listResponse.Select(item => item.CreateBy).Union(listResponse.Select(item => item.UpdateBy)).Distinct(); // 查询返回数据的创建人和修改人id
            var demandUsers = await _fsql.Select<DemandUser>().Where(a => !a.IsDelete).Where(a => demandIds.Contains(a.DemandId)).ToListAsync(); // 查询demandId对应的DemandUser表的数据
            var demandUserIds = demandUsers.Select(a => a.UserId).Distinct(); // 查询demandUsers中所有的UserId
            var users = await _fsql.Select<User>()
                .Where(a => demandUserIds.Contains(a.Id) || createByAndUpdateByIds.Contains(a.Id))
                .ToListAsync<UserResponse>(); // 查询所有创建人、修改人、DemandUser表所关联的用户信息
            foreach (var item in listResponse)
            {
                // demandUsers中的DemandId与当前item中的DemandId匹配的用户，且用户角色为开发
                item.Developer = users
                    .Where(a => demandUsers.Where(demandUser => demandUser.DemandId == item.DemandId).Select(a => a.UserId).Distinct().Contains(a.Id) && a.RoleId == RoleType.开发)
                    .ToList();
                // demandUsers中的DemandId与当前item中的DemandId匹配的用户，且用户角色为测试
                item.Tester = users
                    .Where(a => demandUsers.Where(demandUser => demandUser.DemandId == item.DemandId).Select(a => a.UserId).Distinct().Contains(a.Id) && a.RoleId == RoleType.测试)
                    .ToList();
                item.CreateByName = users.FirstOrDefault(a => a.Id == item.CreateBy)?.UserName;
                item.UpdateByName = users.FirstOrDefault(a => a.Id == item.UpdateBy)?.UserName;
            }
            return listResponse;
        }

        /// <summary>
        /// 分页获取缺陷列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PageListDefectResponse> GetDefectPageListAsync(DefectRequest request)
        {
            var query = _fsql.Select<Defect, Demand, DemandUser>()
                .LeftJoin(a => a.t1.DemandId == a.t2.Id)
                .LeftJoin(a => a.t2.Id == a.t3.DemandId)
                .Where(a => !a.t2.IsDelete)
                .WhereIf(request.Id.HasValue, a => a.t1.Id == request.Id)
                .WhereIf(request.GroupId.HasValue, a => a.t2.GroupId == request.GroupId)
                .WhereIf(!string.IsNullOrEmpty(request.ModuleName), a => a.t2.ModuleName == request.ModuleName)
                .WhereIf(!string.IsNullOrEmpty(request.DemandName), a => a.t2.DemandName == request.DemandName)
                .WhereIf(request.DeveloperId.HasValue, a => a.t3.UserId == request.DeveloperId && !a.t3.IsDelete)
                .WhereIf(request.TesterId.HasValue, a => a.t3.UserId == request.TesterId && !a.t3.IsDelete)
                .WhereIf(!string.IsNullOrEmpty(request.Title), a => a.t1.Title == request.Title)
                .WhereIf(request.DefectType.HasValue, a => a.t1.DefectType == request.DefectType)
                .WhereIf(request.DefectState.HasValue, a => a.t1.DefectState == request.DefectState)
                .WhereIf(request.CreateTimeStart.HasValue, a => a.t1.CreateTime >= request.CreateTimeStart)
                .WhereIf(request.CreateTimeEnd.HasValue, a => a.t1.CreateTime <= request.CreateTimeEnd)
                .WhereIf(request.CreateBy.HasValue, a => a.t1.CreateBy == request.CreateBy)
                .WhereIf(request.UpdateTimeStart.HasValue, a => a.t1.UpdateTime >= request.UpdateTimeStart)
                .WhereIf(request.UpdateTimeEnd.HasValue, a => a.t1.UpdateTime <= request.UpdateTimeEnd)
                .WhereIf(request.UpdateBy.HasValue, a => a.t1.UpdateBy == request.UpdateBy)
                .Distinct();
            var totalCount = await query.CountAsync(); // 分页前的总数据条数
            var defectItems = await query.Page(request.PageIndex, request.PageSize).ToListAsync<DefectResponse>(); // 分页返回的数据
            var demandIds = defectItems.Select(item => item.DemandId).Distinct(); // 查询返回数据的demandId
            var createByAndUpdateByIds = defectItems.Select(item => item.CreateBy).Union(defectItems.Select(item => item.UpdateBy)).Distinct(); // 查询返回数据的创建人和修改人id
            var demandUsers = await _fsql.Select<DemandUser>().Where(a => !a.IsDelete).Where(a => demandIds.Contains(a.DemandId)).ToListAsync(); // 查询demandId对应的DemandUser表的数据
            var demandUserIds = demandUsers.Select(a => a.UserId).Distinct(); // 查询demandUsers中所有的UserId
            var users = await _fsql.Select<User>()
                .Where(a => demandUserIds.Contains(a.Id) || createByAndUpdateByIds.Contains(a.Id))
                .ToListAsync<UserResponse>(); // 查询所有创建人、修改人、DemandUser表所关联的用户信息
            foreach (var item in defectItems)
            {
                // demandUsers中的DemandId与当前item中的DemandId匹配的用户，且用户角色为开发
                item.Developer = users
                    .Where(a => demandUsers.Where(demandUser => demandUser.DemandId == item.DemandId).Select(a => a.UserId).Distinct().Contains(a.Id) && a.RoleId == RoleType.开发)
                    .ToList();
                // demandUsers中的DemandId与当前item中的DemandId匹配的用户，且用户角色为测试
                item.Tester = users
                    .Where(a => demandUsers.Where(demandUser => demandUser.DemandId == item.DemandId).Select(a => a.UserId).Distinct().Contains(a.Id) && a.RoleId == RoleType.测试)
                    .ToList();
                item.CreateByName = users.FirstOrDefault(a => a.Id == item.CreateBy)?.UserName;
                item.UpdateByName = users.FirstOrDefault(a => a.Id == item.UpdateBy)?.UserName;
            }
            var pageListResponse = new PageListDefectResponse
            {
                Items = defectItems,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
            return pageListResponse;
        }

        /// <summary>
        /// 新增缺陷
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<DefectResponse> InsertDefectAsync(CreateDefectRequest request)
        {
            var model = _mapper.Map<CreateDefectRequest, Defect>(request);
            model.CreateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.CreateBy;
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            try
            {
                await InsertAsync(model);
                return _mapper.Map<Defect, DefectResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 编辑缺陷
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<DefectResponse> UpdateDefectAsync(UpdateDefectRequest request)
        {
            var model = await _fsql.Select<Defect>().Where(a => a.Id == request.Id).FirstAsync();
            if (model == null)
            {
                throw new Exception("Defect does not exist.");
            }
            _mapper.Map(request, model);
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            model.UpdateTime = DateTime.Now;
            try
            {
                await UpdateAsync(model);
                return _mapper.Map<Defect, DefectResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
