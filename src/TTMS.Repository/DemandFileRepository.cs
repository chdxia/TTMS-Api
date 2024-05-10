namespace TTMS.Repository
{
    public class DemandFileRepository : DefaultRepository<DemandFile, long>, IDemandFileRepository
    {
        private readonly IFreeSql _fsql;
        private readonly IMapper _mapper;
        private readonly string? _accessUserId;

        public DemandFileRepository(IFreeSql fsql, IMapper mapper, IHttpContextAccessor contextAccessor) : base(fsql)
        {
            _fsql = fsql;
            _mapper = mapper;
            _accessUserId = contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 根据需求id获取需求文件列表
        /// </summary>
        /// <param name="demandId"></param>
        /// <returns></returns>
        public async Task<List<DemandFileResponse>> GetDemandFileListByDemandIdAsync(int demandId)
        {
            return await _fsql.Select<DemandFile>().Where(a => !a.IsDelete).Where(a => a.DemandId == demandId).ToListAsync<DemandFileResponse>();
        }

        /// <summary>
        /// 批量新增需求文件
        /// </summary>
        /// <param name="demandId"></param>
        /// <param name="listUrl"></param>
        /// <returns></returns>
        public async Task<List<DemandFileResponse>> InsertDemandFileAsync(int demandId, List<string> listUrl)
        {
            var models = new List<DemandFile>();
            foreach (var url in listUrl)
            {
                var model = new DemandFile();
                model.DemandId = demandId;
                model.Url = url;
                model.CreateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.CreateBy;
                model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
                models.Add(model);
            }
            try
            {
                await InsertAsync(models);
                return _mapper.Map<List<DemandFile>, List<DemandFileResponse>>(models);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 删除需求文件
        /// </summary>
        /// <param name="demandFileId"></param>
        /// <returns></returns>
        public async Task DeleteDemandFileAsync(int demandFileId)
        {
            var update = _fsql.Update<DemandFile>()
                .Set(a => a.IsDelete, true)
                .Set(a => a.UpdateTime, DateTime.Now)
                .Where(a => a.Id == demandFileId);
            update = _accessUserId != null ? update.Set(a => a.UpdateBy, int.Parse(_accessUserId)) : update;
            var affectedRows = await update.ExecuteAffrowsAsync();
            if (affectedRows <= 0)
            {
                throw new Exception("文件不存在或已被删除.");
            }
        }
    }
}
