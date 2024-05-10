namespace TTMS.Repository
{
    public class DefectDetailFileRepository : DefaultRepository<DefectDetailFile, long>, IDefectDetailFileRepository
    {
        private readonly IFreeSql _fsql;
        private readonly IMapper _mapper;
        private readonly string? _accessUserId;

        public DefectDetailFileRepository(IFreeSql fsql, IMapper mapper, IHttpContextAccessor contextAccessor) : base(fsql)
        {
            _fsql = fsql;
            _mapper = mapper;
            _accessUserId = contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 根据缺陷明细id获取缺陷明细文件列表
        /// </summary>
        /// <param name="defectDetailId"></param>
        /// <returns></returns>
        public async Task<List<DefectDetailFileResponse>> GetDefectDetailFileListByDefectDetailIdAsync(int defectDetailId)
        {
            return await _fsql.Select<DefectDetailFile>().Where(a => !a.IsDelete).Where(a => a.DefectDetailId == defectDetailId).ToListAsync<DefectDetailFileResponse>();
        }

        /// <summary>
        /// 批量新增缺陷明细文件
        /// </summary>
        /// <param name="defectDetailId"></param>
        /// <param name="listUrl"></param>
        /// <returns></returns>
        public async Task<List<DefectDetailFileResponse>> InsertDefectDetailFileAsync(int defectDetailId, List<string> listUrl)
        {
            var models = new List<DefectDetailFile>();
            foreach (var url in listUrl)
            {
                var model = new DefectDetailFile();
                model.DefectDetailId = defectDetailId;
                model.Url = url;
                model.CreateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.CreateBy;
                model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
                models.Add(model);
            }
            try
            {
                await InsertAsync(models);
                return _mapper.Map<List<DefectDetailFile>, List<DefectDetailFileResponse>>(models);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 删除缺陷明细文件
        /// </summary>
        /// <param name="defectDetailFileId"></param>
        /// <returns></returns>
        public async Task DeleteDefectDetailFileAsync(int defectDetailFileId)
        {
            var update = _fsql.Update<DefectDetailFile>()
                .Set(a => a.IsDelete, true)
                .Set(a => a.UpdateTime, DateTime.Now)
                .Where(a => a.Id == defectDetailFileId);
            update = _accessUserId != null ? update.Set(a => a.UpdateBy, int.Parse(_accessUserId)) : update;
            var affectedRows = await update.ExecuteAffrowsAsync();
            if (affectedRows <= 0)
            {
                throw new Exception("文件不存在或已被删除.");
            }
        }
    }
}
