namespace TTMS.Repository
{
    public class UserRepository : DefaultRepository<User, long>, IUserRepository
    {
        private readonly IFreeSql _fsql;
        private readonly IMapper _mapper;
        private readonly string? _accessUserId;

        public UserRepository(IFreeSql fsql, IMapper mapper, IHttpContextAccessor contextAccessor) : base(fsql)
        {
            _fsql = fsql;
            _mapper = mapper;
            _accessUserId = contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<UserLoginResponse> UserLoginAsync(UserLoginRequest request)
        {
            var model = await _fsql.Select<User>().Where(a => a.Account == request.Account).ToOneAsync();
            if (model == null || model.PassWord != SecurityUtility.HashWithSalt(request.PassWord, model.Salt))
            {
                throw new Exception("Incorrect Account or PassWord."); // 账户或密码错误
            }
            return _mapper.Map<User, UserLoginResponse>(model);
        }

        /// <summary>
        /// 根据id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UserResponse> GetUserByIdAsync(int id)
        {
            return await _fsql.Select<User>().Where(a => a.Id == id).ToOneAsync<UserResponse>();
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<UserResponse>> GetUserListAsync(UserRequest request)
        {
            var query = _fsql.Select<User>()
                .Where(a => !a.IsDelete)
                .WhereIf(!string.IsNullOrEmpty(request.Account), a => a.Account.Contains(request.Account))
                .WhereIf(!string.IsNullOrEmpty(request.UserName), a => a.UserName.Contains(request.UserName))
                .WhereIf(!string.IsNullOrEmpty(request.AccountOrUserName), a => a.Account.Contains(request.AccountOrUserName) || a.UserName.Contains(request.AccountOrUserName))
                .WhereIf(!string.IsNullOrEmpty(request.Email), a => a.Email.Contains(request.Email))
                .WhereIf(request.GroupId.HasValue, a => a.GroupId == request.GroupId)
                .WhereIf(request.RoleId.HasValue, a => a.RoleId == request.RoleId)
                .WhereIf(request.IsDisable.HasValue, a => request.IsDisable == a.IsDisable)
                .WhereIf(request.CreateTimeStart.HasValue, a => a.CreateTime >= request.CreateTimeStart)
                .WhereIf(request.CreateTimeEnd.HasValue, a => a.CreateTime <= request.CreateTimeEnd)
                .WhereIf(request.CreateBy.HasValue, a => a.CreateBy == request.CreateBy)
                .WhereIf(request.UpdateTimeStart.HasValue, a => a.UpdateTime >= request.UpdateTimeStart)
                .WhereIf(request.UpdateTimeEnd.HasValue, a => a.UpdateTime <= request.UpdateTimeEnd)
                .WhereIf(request.UpdateBy.HasValue, a => a.UpdateBy == request.UpdateBy)
                .OrderByDescending(a => a.CreateTime);
            var listResponse = await query.ToListAsync<UserResponse>();
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
        /// 分页获取用户列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PageListUserResponse> GetUserPageListAsync(UserRequest request)
        {
            var query = _fsql.Select<User>()
                .Where(a => !a.IsDelete)
                .WhereIf(!string.IsNullOrEmpty(request.Account), a => a.Account.Contains(request.Account))
                .WhereIf(!string.IsNullOrEmpty(request.UserName), a => a.UserName.Contains(request.UserName))
                .WhereIf(!string.IsNullOrEmpty(request.AccountOrUserName), a => a.Account.Contains(request.AccountOrUserName) || a.UserName.Contains(request.AccountOrUserName))
                .WhereIf(!string.IsNullOrEmpty(request.Email), a => a.Email.Contains(request.Email))
                .WhereIf(request.GroupId.HasValue, a => a.GroupId == request.GroupId)
                .WhereIf(request.RoleId.HasValue, a => a.RoleId == request.RoleId)
                .WhereIf(request.IsDisable.HasValue, a => request.IsDisable == a.IsDisable)
                .WhereIf(request.CreateTimeStart.HasValue, a => a.CreateTime >= request.CreateTimeStart)
                .WhereIf(request.CreateTimeEnd.HasValue, a => a.CreateTime <= request.CreateTimeEnd)
                .WhereIf(request.CreateBy.HasValue, a => a.CreateBy == request.CreateBy)
                .WhereIf(request.UpdateTimeStart.HasValue, a => a.UpdateTime >= request.UpdateTimeStart)
                .WhereIf(request.UpdateTimeEnd.HasValue, a => a.UpdateTime <= request.UpdateTimeEnd)
                .WhereIf(request.UpdateBy.HasValue, a => a.UpdateBy == request.UpdateBy)
                .OrderByDescending(a => a.CreateTime);
            var totalCount = await query.CountAsync();
            var userItems = await query.Page(request.PageIndex, request.PageSize).ToListAsync<UserResponse>();
            var createByAndUpdateByIds = userItems.Select(item => item.CreateBy).Union(userItems.Select(item => item.UpdateBy)).Distinct();
            var createByAndUpdateByUsers = await _fsql.Select<User>().Where(a => createByAndUpdateByIds.Contains(a.Id)).ToListAsync();
            foreach (var item in userItems)
            {
                item.CreateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item.CreateBy)?.UserName;
                item.UpdateByName = createByAndUpdateByUsers.FirstOrDefault(a => a.Id == item.UpdateBy)?.UserName;
            }
            var pageListResponse = new PageListUserResponse
            {
                Items = userItems,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
            return pageListResponse;
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<UserResponse> InsertUserAsync(CreateUserRequest request)
        {
            if (_fsql.Select<User>().Where(a => a.Account == request.Account || a.Email == request.Email).Where(a => a.IsDelete == false).ToList().Any())
            {
                throw new Exception("Account or email already exists."); // 新增失败，账户或邮箱已存在
            }
            if (!_fsql.Select<Group>().Where(a => a.IsDelete == false).WhereIf(request.GroupId.HasValue, a => a.Id == request.GroupId).ToList().Any())
            {
                throw new Exception("Group does not exist."); // 新增失败，分组不存在
            }
            var model = _mapper.Map<CreateUserRequest, User>(request);
            var hashWithNewSalt = SecurityUtility.HashWithNewSalt(request.PassWord);
            model.PassWord = hashWithNewSalt.hashedValue;
            model.Salt = hashWithNewSalt.salt;
            model.CreateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.CreateBy;
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            try
            {
                await InsertAsync(model);
                return _mapper.Map<User, UserResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<UserResponse> UpdateUserAsync(UpdateUserRequest request)
        {
            var model = await _fsql.Select<User>().Where(a => a.Id == request.Id).FirstAsync();
            if(model == null) // 如果用户表没有这个用户Id
            {
                throw new Exception("User does not exist."); // 修改失败，用户不存在
            }
            else if (_fsql.Select<User>() // 如果Account或Email和其它未删除用户一样
                .Where(a => a.Id != request.Id)
                .Where(a => a.Account == request.Account || a.Email == request.Email)
                .Where(a => a.IsDelete == false)
                .ToList().Any())
            {
                throw new Exception("Account or email already exists."); // 修改失败，账户或邮箱已存在
            }
            if (!_fsql.Select<Group>().Where(a => a.IsDelete == false).WhereIf(request.GroupId.HasValue, a => a.Id == request.GroupId).ToList().Any())
            {
                throw new Exception("Group does not exist."); // 修改失败，分组不存在
            }
            _mapper.Map(request, model);
            if (!string.IsNullOrEmpty(request.PassWord))
            {
                var hashWithNewSalt = SecurityUtility.HashWithNewSalt(request.PassWord);
                model.PassWord = hashWithNewSalt.hashedValue;
                model.Salt = hashWithNewSalt.salt;
            }
            model.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : model.UpdateBy;
            model.UpdateTime = DateTime.Now;
            try
            {
                await UpdateAsync(model);
                return _mapper.Map<User, UserResponse>(model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task DeleteUserAsync(DeleteUserRequest request)
        {
            if (!request.UserIds.Any())
            {
                throw new Exception("用户Id为空，请填写有效用户Id.");
            }
            var existingUserIds = await _fsql.Select<User>()
                .Where(a => request.UserIds.Contains(a.Id))
                .ToListAsync();
            var nonExistingUserIds = request.UserIds.Except(existingUserIds.Select(a => a.Id));
            if (nonExistingUserIds.Any())
            {
                throw new Exception($"删除失败，以下用户ID不存在: {string.Join(", ", nonExistingUserIds)}.");
            }
            var update = _fsql.Update<User>()
                .Set(a => a.IsDelete, true)
                .Set(a => a.UpdateTime, DateTime.Now)
                .Where(a => request.UserIds.Contains(a.Id));
            update = _accessUserId != null ? update.Set(a => a.UpdateBy, int.Parse(_accessUserId)) : update;
            var affectedRows = await update.ExecuteAffrowsAsync();
            if (affectedRows <= 0)
            {
                throw new Exception("删除失败.");
            }
        }

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<AuthUserPermissionResponse>> GetUserPermissionListAsync(int userId)
        {
            var listResponse = await _fsql.Select<AuthPermission>().Where(a => !a.IsDelete && !a.IsDisable).ToListAsync<AuthUserPermissionResponse>();
            var userPermissions = await _fsql.Select<AuthUserPermission>().Where(a => a.UserId == userId && !a.IsDelete).ToListAsync();
            var permissionIds = new HashSet<int>(userPermissions.Select(a => a.PermissionId));
            foreach (var item in listResponse)
            {
                item.HasPermission = permissionIds.Contains(item.Id);
            }
            return listResponse;
        }

        /// <summary>
        /// 编辑用户权限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task UpdateUserPermissionAsync(UpdateAuthUserPermissionRequest request)
        {
            // 获取这个用户已有的权限
            var existingUserPermissions = await _fsql.Select<AuthUserPermission>()
                .Where(a => a.UserId == request.UserId)
                .Where(a => request.AuthPermissionIds.Contains(a.PermissionId))
                .ToListAsync();
            foreach(var oneExistingUserPermission in existingUserPermissions)
            {
                if (request.AuthPermissionIds.Contains(oneExistingUserPermission.PermissionId))
                {
                    oneExistingUserPermission.IsDelete = false; // 请求数据中有的权限，将IsDelete设置为false
                }
                else
                {
                    oneExistingUserPermission.IsDelete = true; // 请求数据中没有的权限，将IsDelete设置为true
                }
                oneExistingUserPermission.UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : 0;
            }
            // 请求数据中有，但数据库中没有，直接新增
            var newAuthUserPermissionList = new List<AuthUserPermission>();
            var newAuthUserPermissions = request.AuthPermissionIds.Where(permissionId => !existingUserPermissions.Any(a => a.PermissionId == permissionId))
                .Select(permissionId => new AuthUserPermission {
                    UserId = request.UserId,
                    PermissionId = permissionId,
                    IsDelete = false,
                    CreateBy = _accessUserId != null ? int.Parse(_accessUserId) : 0,
                    UpdateBy = _accessUserId != null ? int.Parse(_accessUserId) : 0,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                })
                .ToList();
            newAuthUserPermissionList.AddRange(newAuthUserPermissions);
            await _fsql.Update<AuthUserPermission>().SetSource(existingUserPermissions).ExecuteAffrowsAsync();
            await _fsql.Insert<AuthUserPermission>().AppendData(newAuthUserPermissionList).ExecuteAffrowsAsync();
        }
    }
}
