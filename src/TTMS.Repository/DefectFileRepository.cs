﻿namespace TTMS.Repository
{
    public class DefectFileRepository : DefaultRepository<DefectFile, long>, IDefectFileRepository
    {
        private readonly IFreeSql _fsql;
        private readonly IMapper _mapper;

        public DefectFileRepository(IFreeSql fsql, IMapper mapper) : base(fsql)
        {
            _fsql = fsql;
            _mapper = mapper;
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
        public async Task<(bool, string, List<DefectFileResponse>?)> InsertDefectFileAsync(int defectId, List<string> listUrl)
        {
            var models = new List<DefectFile>();
            foreach (var url in listUrl)
            {
                var model = new DefectFile();
                model.DefectId = defectId;
                model.Url = url;
                model.CreateTime = model.UpdateTime = DateTime.Now;
                models.Add(model);
            }
            try
            {
                await InsertAsync(models);
                var result = _mapper.Map<List<DefectFile>, List<DefectFileResponse>>(models);
                return (true, "", result);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        /// <summary>
        /// 删除缺陷文件
        /// </summary>
        /// <param name="defectFileId"></param>
        /// <returns></returns>
        public async Task<(bool, string)> DeleteDefectFileAsync(int defectFileId)
        {
            var affectedRows = await _fsql.Update<DefectFile>()
                .Set(a => a.IsDelete, true)
                .Set(a => a.UpdateTime, DateTime.Now)
                .Where(a => a.Id == defectFileId)
                .ExecuteAffrowsAsync();
            return affectedRows > 0 ? (true, "") : (false, "文件不存在或已被删除.");
        }
    }
}
