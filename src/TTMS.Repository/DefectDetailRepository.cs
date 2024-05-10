namespace TTMS.Repository
{
    public class DefectDetailRepository : DefaultRepository<DefectDetail, long>, IDefectDetailRepository
    {
        private readonly IFreeSql _fsql;
        private readonly IMapper _mapper;
        private readonly string? _accessUserId;

        public DefectDetailRepository(IFreeSql fsql, IMapper mapper, IHttpContextAccessor contextAccessor) : base(fsql)
        {
            _fsql = fsql;
            _mapper = mapper;
            _accessUserId = contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 根据缺陷id获取缺陷明细列表
        /// </summary>
        /// <param name="defectId"></param>
        /// <returns></returns>
        public async Task<List<DefectDetailResponse>> GetDefectDetailListByDefectIdAsync(int defectId)
        {
            return await _fsql.Select<DefectDetail>().Where(a => a.DefectId == defectId).ToListAsync<DefectDetailResponse>();
        }

        /// <summary>
        /// 新建缺陷明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<DefectDetailResponse> InsertDefectDetailAsync(CreateDefectDetailRequest request)
        {
            var model = _mapper.Map<CreateDefectDetailRequest, DefectDetail>(request);
            model.CreateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.CreateBy;
            try
            {
                await _fsql.Insert<DefectDetail>().AppendData(model).ExecuteAffrowsAsync();
                return _mapper.Map<DefectDetail, DefectDetailResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
