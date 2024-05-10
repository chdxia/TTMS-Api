namespace TTMS.Repository
{
    public class DefectFileRepository : DefaultRepository<DefectFile, long>, IDefectFileRepository
    {
        private readonly IFreeSql _fsql;
        private readonly IMapper _mapper;
        private readonly string? _accessUserId;

        public DefectFileRepository(IFreeSql fsql, IMapper mapper, IHttpContextAccessor contextAccessor) : base(fsql)
        {
            _fsql = fsql;
            _mapper = mapper;
            _accessUserId = contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 根据缺陷id获取缺陷文件列表
        /// </summary>
        /// <param name="defectId"></param>
        /// <returns></returns>
        public async Task<List<DefectFileResponse>> GetDefectFileListByDefectIdAsync(int defectId)
        {
            return await _fsql.Select<DefectFile>().Where(a => !a.IsDelete).Where(a => a.DefectId == defectId).ToListAsync<DefectFileResponse>();
        }

        /// <summary>
        /// 批量新增缺陷文件
        /// </summary>
        /// <param name="defectId"></param>
        /// <param name="listUrl"></param>
        /// <returns></returns>
        public async Task<List<DefectFileResponse>> InsertDefectFileAsync(int defectId, List<string> listUrl)
        {
            var models = new List<DefectFile>();
            foreach (var url in listUrl)
            {
                var model = new DefectFile();
                model.DefectId = defectId;
                model.Url = url;
                model.CreateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.CreateBy;
                model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
                models.Add(model);
            }
            try
            {
                await InsertAsync(models);
                return _mapper.Map<List<DefectFile>, List<DefectFileResponse>>(models);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 删除缺陷文件
        /// </summary>
        /// <param name="defectFileId"></param>
        /// <returns></returns>
        public async Task DeleteDefectFileAsync(int defectFileId)
        {
            var update = _fsql.Update<DefectFile>()
                .Set(a => a.IsDelete, true)
                .Set(a => a.UpdateTime, DateTime.Now)
                .Where(a => a.Id == defectFileId);
            update = _accessUserId != null ? update.Set(a => a.UpdateBy, int.Parse(_accessUserId)) : update;
            var affectedRows = await update.ExecuteAffrowsAsync();
            if (affectedRows <= 0)
            {
                throw new Exception("文件不存在或已被删除.");
            }
        }
    }
}
