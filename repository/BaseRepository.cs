using SqlSugar;
using System.Linq.Expressions;

namespace dy.net.repository
{
    /// <summary>
    /// 通用Repository基类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public abstract class BaseRepository<T> where T : class, new()
    {
        /// <summary>
        /// SQLSugar客户端
        /// </summary>
        protected readonly ISqlSugarClient Db;

        /// <summary>
        /// 构造函数注入SQLSugar客户端
        /// </summary>
        /// <param name="db">SQLSugar客户端实例</param>
        protected BaseRepository(ISqlSugarClient db)
        {
            Db = db;
        }

        #region 新增操作

        /// <summary>
        /// 新增单个实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>是否新增成功</returns>
        public virtual bool Insert(T entity)
        {
            return Db.Insertable(entity).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 新增单个实体（异步）
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>是否新增成功</returns>
        public virtual async Task<bool> InsertAsync(T entity)
        {
            return await Db.Insertable(entity).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// 批量新增实体
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns>新增的数量</returns>
        public virtual int InsertRange(IEnumerable<T> entities)
        {
            return Db.Insertable(entities.ToArray()).ExecuteCommand();
        }

        /// <summary>
        /// 批量新增实体（异步）
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns>新增的数量</returns>
        public virtual async Task<int> InsertRangeAsync(IEnumerable<T> entities)
        {
            return await Db.Insertable(entities.ToArray()).ExecuteCommandAsync();
        }

        /// <summary>
        /// 新增实体并返回自增ID
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>自增ID</returns>
        public virtual int InsertReturnIdentity(T entity)
        {
            return Db.Insertable(entity).ExecuteReturnIdentity();
        }

        /// <summary>
        /// 新增实体并返回自增ID（异步）
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>自增ID</returns>
        public virtual async Task<int> InsertReturnIdentityAsync(T entity)
        {
            return await Db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        #endregion

        #region 删除操作

        /// <summary>
        /// 根据主键删除
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns>是否删除成功</returns>
        public virtual bool DeleteById(object id)
        {
            return Db.Deleteable<T>().In(id).ExecuteCommand() > 0;
        }
        /// <summary>
        /// 事务执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="errorCallBack"></param>
        /// <returns></returns>
        public async Task<bool> UseTranAsync(Func<Task> action, Action<Exception> errorCallBack)
        {
            var res = Db.Ado.UseTranAsync(async () =>
              {
                  await action();
              }, errorCallBack: errorCallBack);

            return res.IsCompletedSuccessfully;
        }

        /// <summary>
        /// 根据主键删除（异步）
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns>是否删除成功</returns>
        public virtual async Task<bool> DeleteByIdAsync(object id)
        {
            return await Db.Deleteable<T>().In(id).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// 根据主键集合删除
        /// </summary>
        /// <param name="ids">主键集合</param>
        /// <returns>删除的数量</returns>
        public virtual int DeleteByIds(IEnumerable<object> ids)
        {
            return Db.Deleteable<T>().In(ids).ExecuteCommand();
        }

        /// <summary>
        /// 根据主键集合删除（异步）
        /// </summary>
        /// <param name="ids">主键集合</param>
        /// <returns>删除的数量</returns>
        public virtual async Task<int> DeleteByIdsAsync(IEnumerable<object> ids)
        {
            return await Db.Deleteable<T>().In(ids).ExecuteCommandAsync();
        }

        /// <summary>
        /// 根据条件删除
        /// </summary>
        /// <param name="whereExpression">删除条件</param>
        /// <returns>删除的数量</returns>
        public virtual int Delete(Expression<Func<T, bool>> whereExpression)
        {
            return Db.Deleteable<T>().Where(whereExpression).ExecuteCommand();
        }

        /// <summary>
        /// 根据条件删除（异步）
        /// </summary>
        /// <param name="whereExpression">删除条件</param>
        /// <returns>删除的数量</returns>
        public virtual async Task<int> DeleteAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await Db.Deleteable<T>().Where(whereExpression).ExecuteCommandAsync();
        }

        #endregion

        #region 修改操作

        /// <summary>
        /// 更新单个实体
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>是否更新成功</returns>
        public virtual bool Update(T entity)
        {
            return Db.Updateable(entity).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 更新单个实体（异步）
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>是否更新成功</returns>
        public virtual async Task<bool> UpdateAsync(T entity)
        {
            return await Db.Updateable(entity).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// 批量更新实体
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns>更新的数量</returns>
        public virtual int UpdateRange(IEnumerable<T> entities)
        {
            return Db.Updateable(entities.ToArray()).ExecuteCommand();
        }

        /// <summary>
        /// 批量更新实体（异步）
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <returns>更新的数量</returns>
        public virtual async Task<int> UpdateRangeAsync(IEnumerable<T> entities)
        {
            return await Db.Updateable(entities.ToArray()).ExecuteCommandAsync();
        }

        /// <summary>
        /// 根据条件更新
        /// </summary>
        /// <param name="updateExpression">更新表达式</param>
        /// <param name="whereExpression">更新条件</param>
        /// <returns>更新的数量</returns>
        public virtual int Update(Expression<Func<T, T>> updateExpression, Expression<Func<T, bool>> whereExpression)
        {
            return Db.Updateable<T>().SetColumns(updateExpression).Where(whereExpression).ExecuteCommand();
        }

        /// <summary>
        /// 根据条件更新（异步）
        /// </summary>
        /// <param name="updateExpression">更新表达式</param>
        /// <param name="whereExpression">更新条件</param>
        /// <returns>更新的数量</returns>
        public virtual async Task<int> UpdateAsync(Expression<Func<T, T>> updateExpression, Expression<Func<T, bool>> whereExpression)
        {
            return await Db.Updateable<T>().SetColumns(updateExpression).Where(whereExpression).ExecuteCommandAsync();
        }

        #endregion

        #region 查询操作

        /// <summary>
        /// 根据主键查询
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns>实体对象</returns>
        public virtual T GetById(object id)
        {
            return Db.Queryable<T>().InSingle(id);
        }

        /// <summary>
        /// 根据主键查询（异步）
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns>实体对象</returns>
        public virtual async Task<T> GetByIdAsync(object id)
        {
            return await Db.Queryable<T>().InSingleAsync(id);
        }

        /// <summary>
        /// 根据条件查询单个实体
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <returns>实体对象</returns>
        public virtual T GetFirst(Expression<Func<T, bool>> whereExpression)
        {
            return Db.Queryable<T>().Where(whereExpression).First();
        }

        /// <summary>
        /// 根据条件查询单个实体（异步）
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <returns>实体对象</returns>
        public virtual async Task<T> GetFirstAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await Db.Queryable<T>().Where(whereExpression).FirstAsync();
        }

        /// <summary>
        /// 查询所有实体
        /// </summary>
        /// <returns>实体集合</returns>
        public virtual List<T> GetAll()
        {
            return Db.Queryable<T>().ToList();
        }

        /// <summary>
        /// 查询所有实体（异步）
        /// </summary>
        /// <returns>实体集合</returns>
        public virtual async Task<List<T>> GetAllAsync()
        {
            return await Db.Queryable<T>().ToListAsync();
        }

        /// <summary>
        /// 根据条件查询实体集合
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual List<T> GetList(Expression<Func<T, bool>> whereExpression)
        {
            return Db.Queryable<T>().Where(whereExpression).ToList();
        }

        /// <summary>
        /// 根据条件查询实体集合（异步）
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await Db.Queryable<T>().Where(whereExpression).ToListAsync();
        }
      
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="totalCount">总记录数</param>
        /// <returns>分页实体集合</returns>
        public virtual List<T> GetPageList(Expression<Func<T, bool>> whereExpression, int pageIndex, int pageSize, out int totalCount)
        {
            var queryable = Db.Queryable<T>().Where(whereExpression);
            totalCount = queryable.Count();
            return queryable.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// 分页查询（带排序）
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="orderByExpression">排序表达式</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="totalCount">总记录数</param>
        /// <returns>分页实体集合</returns>
        public virtual List<T> GetPageList(Expression<Func<T, bool>> whereExpression, Expression<Func<T, object>> orderByExpression, bool isAsc, int pageIndex, int pageSize, out int totalCount)
        {
            var queryable = Db.Queryable<T>().Where(whereExpression);
            if (isAsc)
            {
                queryable = queryable.OrderBy(orderByExpression);
            }
            else
            {
                queryable = queryable.OrderByDescending(orderByExpression);
            }
            totalCount = queryable.Count();
            return queryable.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// 分页查询（异步）
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>分页结果（实体集合和总记录数）</returns>
        public virtual async Task<(List<T> list, int totalCount)> GetPageListAsync(Expression<Func<T, bool>> whereExpression, int pageIndex, int pageSize)
        {
            var queryable = Db.Queryable<T>().Where(whereExpression);
            var totalCount = await queryable.CountAsync();
            var list = await queryable.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return (list, totalCount);
        }

        /// <summary>
        /// 判断是否存在符合条件的记录
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <returns>是否存在</returns>
        public virtual bool Exists(Expression<Func<T, bool>> whereExpression)
        {
            return Db.Queryable<T>().Where(whereExpression).Any();
        }

        /// <summary>
        /// 判断是否存在符合条件的记录（异步）
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <returns>是否存在</returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await Db.Queryable<T>().Where(whereExpression).AnyAsync();
        }

        /// <summary>
        /// 获取符合条件的记录数量
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <returns>记录数量</returns>
        public virtual int Count(Expression<Func<T, bool>> whereExpression = null)
        {
            return whereExpression == null ? Db.Queryable<T>().Count() : Db.Queryable<T>().Where(whereExpression).Count();
        }

        /// <summary>
        /// 获取符合条件的记录数量（异步）
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <returns>记录数量</returns>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> whereExpression = null)
        {
            return whereExpression == null ? await Db.Queryable<T>().CountAsync() : await Db.Queryable<T>().Where(whereExpression).CountAsync();
        }

        #endregion

        #region 高级查询

        /// <summary>
        /// 自定义查询（返回IQueryable，可继续拼接查询条件）
        /// </summary>
        /// <param name="whereExpression">初始查询条件</param>
        /// <returns>IQueryable对象</returns>
        public virtual ISugarQueryable<T> Query(Expression<Func<T, bool>> whereExpression = null)
        {
            var query = Db.Queryable<T>();
            return whereExpression != null ? query.Where(whereExpression) : query;
        }

        #endregion
    }
}
