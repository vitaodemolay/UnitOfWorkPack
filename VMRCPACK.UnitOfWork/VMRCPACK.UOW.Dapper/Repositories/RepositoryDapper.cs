using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VMRCPACK.UnitOfWork.Interfaces.Repository;
using VMRCPACK.UnitOfWork.Interfaces.support;
using VMRCPACK.UnitOfWork.Interfaces.UnitOfWork;
using VMRCPACK.UOW.Dapper.support;
using VMRCPACK.UOW.Dapper.Uow;


namespace VMRCPACK.UOW.Dapper.Repositories
{
    public abstract class RepositoryDapper<TEntity, TContext> : IRepositoryReadOnly<TEntity>,
                                                                IRepositoryReadOnlyAsync<TEntity>,
                                                                IRepositoryReadOnlyTSQL<TEntity>,
                                                                IRepositoryReadOnlyTSQLAsync<TEntity>,
                                                                IRepositoryReadOnlyExtended<TEntity>,
                                                                IRepositoryReadOnlyExtendedTSQL<TEntity>,
                                                                IRepositoryReadOnlyPagination<TEntity>,
                                                                IRepositoryReadOnlyPaginationAsync<TEntity>,
                                                                IRepositoryWriteTSQL<TEntity>,
                                                                IRepositoryWriteTSQLAsync<TEntity>
            

        where TEntity : class
        where TContext : ConnectionContext
    {

        #region Private Objects
        private string tableName { get; set; }
        private Dictionary<string, Type> fieldIdName { get; set; }
        private IUnitOfWork<TContext> unitOfWorkDapper { get; set; }
        public DMapper<TEntity> MapperObject { get; protected set; }
        private IDbConnection Connection => this.unitOfWorkDapper.dbContext.Connection;
        private IDbTransaction transaction => (this.unitOfWorkDapper as UnitOfWorkDapper<TContext>).transaction;
        private string selectDefault => $"SELECT * FROM {this.tableName}";
        #endregion

        #region Private Methods
        private static string CompileExceptionObjectOnOneString(Exception ex, string previousLog)
        {
            string _log = String.Empty;
            if (String.IsNullOrEmpty(previousLog)) _log = ex.Message;
            else _log = String.Format("{0}; {1}", previousLog, ex.Message);

            if (ex.InnerException != null) _log = CompileExceptionObjectOnOneString(ex.InnerException, _log);

            return _log;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWork">UnitOfWork Instace object</param>
        public RepositoryDapper(IUnitOfWork<TContext> unitOfWork, string tablename, Dictionary<string, Type> fieldIdName)
        {
            if (unitOfWork == null) throw new NullReferenceException("Unit of Work object is Null.");
            this.unitOfWorkDapper = unitOfWork;

            if (string.IsNullOrEmpty(tablename)) throw new Exception("TableName is required for this working");
            this.tableName = tablename;

            if (fieldIdName == null) throw new Exception("FieldIdName is required for this working");
            this.fieldIdName = fieldIdName;

            MapperObject = new DMapper<TEntity>((from ids in fieldIdName select (ids.Key)).ToArray<string>());
        }
        #endregion

        #region ReadOnly
        public virtual TEntity Find(Expression<Func<TEntity, bool>> filter = null)
        {
            if (filter == null)
                return this.Find(this.selectDefault);

            var filterSql = DynamicQuery.GetDynamicQuery<TEntity>(this.tableName, this.fieldIdName.First().Key, filter);
            IDictionary<string, Object> expando = new ExpandoObject();
            expando = filterSql.Param;

            return this.Find(filterSql.Sql, expando);
        }


        public virtual IList<TEntity> FindAll(Expression<Func<TEntity, bool>> filter = null)
        {
            if (filter == null)
                return this.FindAll(this.selectDefault);

            var filterSql = DynamicQuery.GetDynamicQuery<TEntity>(this.tableName, this.fieldIdName.First().Key, filter);
            IDictionary<string, Object> expando = new ExpandoObject();
            expando = filterSql.Param;

            return this.FindAll(filterSql.Sql, expando);
        }
        #endregion

        #region ReadOnly Async
        public virtual async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            if (filter == null)
                return await this.FindAsync(this.selectDefault);

            var filterSql = DynamicQuery.GetDynamicQuery<TEntity>(this.tableName, this.fieldIdName.First().Key, filter);
            IDictionary<string, Object> expando = new ExpandoObject();
            expando = filterSql.Param;

            return await this.FindAsync(filterSql.Sql, expando);
        }

        public virtual async Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            if (filter == null)
                return (await this.FindAllAsync(this.selectDefault)).ToList();

            var filterSql = DynamicQuery.GetDynamicQuery<TEntity>(this.tableName, this.fieldIdName.First().Key, filter);
            IDictionary<string, Object> expando = new ExpandoObject();
            expando = filterSql.Param;

            return (await this.FindAllAsync(filterSql.Sql, expando)).ToList();
        }
        #endregion

        #region ReadOnly TSQL
        public virtual TEntity Find(string sql, IDictionary<string, object> param = null)
        {
            var result = this.Connection.Query<dynamic>(sql, param, this.transaction).ToList();
            return this.MapperObject.MapList(result).FirstOrDefault();
        }

        public virtual IList<TEntity> FindAll(string sql, IDictionary<string, object> param = null)
        {
            var listResult = this.Connection.Query<dynamic>(sql, param, this.transaction).ToList();
            return this.MapperObject.MapList(listResult).ToList();
        }

        public virtual IPagination<TEntity> FindAll(int pageIndex, int pageSize, string fieldOrderBy, string sql, bool desc = false, IDictionary<string, object> param = null)
        {
            IPagination<TEntity> _result;

            string part = sql.ToUpper().Split(new string[] { "FROM" }, StringSplitOptions.None).ToList().Last();
            string label = string.Empty;

            if (part.Contains("JOIN"))
            {
                var tks = part.Split(new string[] { " " }, StringSplitOptions.None).ToList();
                int pos = tks.FindIndex(f => f.Contains(this.tableName.ToUpper()));
                for (int i = pos + 1; i < tks.Count; i++)
                {
                    if (tks[i] != "AS" && !string.IsNullOrEmpty(tks[i]))
                    {
                        label = tks[i].Trim() + ".";
                        break;
                    }
                }
            }

            string secSql = $"SELECT COUNT(DISTINCT {label}{this.fieldIdName.First().Key}) FROM {part}";

            if (param == null)
                param = new ExpandoObject();

            string _desc = desc ? "DESC" : string.Empty;
            sql += string.Format(" ORDER BY {0} {2} OFFSET @PageSize * (@PageNumber - 1) ROWS FETCH NEXT @PageSize ROWS ONLY; {1};", fieldOrderBy, secSql, _desc);
            param.Add("@PageSize", pageSize);
            param.Add("@PageNumber", pageIndex);

            using (var _multQuery = this.Connection.QueryMultiple(sql, param, transaction))
            {
                var listResult = _multQuery.Read<dynamic>().ToList();
                var totalRegisters = _multQuery.Read<int>().FirstOrDefault();

                _result = new Pagination<TEntity>(this.MapperObject.MapList(listResult).ToList(), totalRegisters);
            }

            return _result;
        }
        #endregion

        #region ReadOnly TSQL Async
        public virtual async Task<TEntity> FindAsync(string sql, IDictionary<string, object> param = null)
        {
            var result = (await this.Connection.QueryAsync<dynamic>(sql, param, this.transaction)).ToList();
            return this.MapperObject.MapList(result).FirstOrDefault();
        }

        public virtual async Task<IList<TEntity>> FindAllAsync(string sql, IDictionary<string, object> param = null)
        {
            var listResult = (await this.Connection.QueryAsync<dynamic>(sql, param, this.transaction)).ToList();
            return this.MapperObject.MapList(listResult).ToList();
        }

        public virtual async Task<IPagination<TEntity>> FindAllAsync(int pageIndex, int pageSize, string fieldOrderBy, string sql, bool desc = false, IDictionary<string, object> param = null)
        {
            IPagination<TEntity> _result;

            string part = sql.ToUpper().Split(new string[] { "FROM" }, StringSplitOptions.None).ToList().Last();
            string label = string.Empty;

            if (part.Contains("JOIN"))
            {
                var tks = part.Split(new string[] { " " }, StringSplitOptions.None).ToList();
                int pos = tks.FindIndex(f => f.Contains(this.tableName.ToUpper()));
                for (int i = pos + 1; i < tks.Count; i++)
                {
                    if (tks[i] != "AS" && !string.IsNullOrEmpty(tks[i]))
                    {
                        label = tks[i].Trim() + ".";
                        break;
                    }
                }
            }

            string secSql = $"SELECT COUNT(DISTINCT {label}{this.fieldIdName.First().Key}) FROM {part}";

            if (param == null)
                param = new ExpandoObject();

            string _desc = desc ? "DESC" : string.Empty;
            sql += string.Format(" ORDER BY {0} {2} OFFSET @PageSize * (@PageNumber - 1) ROWS FETCH NEXT @PageSize ROWS ONLY; {1};", fieldOrderBy, secSql, _desc);
            param.Add("@PageSize", pageSize);
            param.Add("@PageNumber", pageIndex);

            using (var _multQuery = this.Connection.QueryMultipleAsync(sql, param, transaction))
            {
                var listResult =  (await _multQuery).Read<dynamic>().ToList();
                var totalRegisters = (await _multQuery).Read<int>().FirstOrDefault();

                _result = new Pagination<TEntity>(this.MapperObject.MapList(listResult).ToList(), totalRegisters);
            }

            return _result;
        }
        #endregion

        #region ReadOnly Extended
        public virtual void Reload(ref TEntity entity)
        {
            IDictionary<string, object> IdValue = new ExpandoObject();
            ParameterExpression pe = Expression.Parameter(entity.GetType(), "entity");

            Expression left = null;
            foreach (var item in this.fieldIdName)
            {
                var prop = Expression.PropertyOrField(pe, item.Key);
                Expression right = Expression.Equal(prop, Expression.Constant(entity.GetType().GetProperty(item.Key).GetValue(entity, null)));
                left = left == null ? right : Expression.AndAlso(left, right);
            }

            var filter = Expression.Lambda<Func<TEntity, bool>>(left, pe);

            entity = this.Find(filter);
        }

        public virtual int Count(Expression<Func<TEntity, bool>> filter = null)
        {
            QueryResult filterSql = null;
            IDictionary<string, Object> expando = new ExpandoObject();
            if (filter == null)
                filterSql = new QueryResult(this.selectDefault, expando);
            else
                filterSql = DynamicQuery.GetDynamicQuery<TEntity>(this.tableName, this.fieldIdName.First().Key, filter);

            expando = filterSql.Param;

            return this.Count(filterSql.Sql, expando);
        }

        public virtual bool Exists(Expression<Func<TEntity, bool>> filter)
        {
            QueryResult filterSql = null;
            IDictionary<string, Object> expando = new ExpandoObject();
            if (filter == null)
                filterSql = new QueryResult(this.selectDefault, expando);
            else
                filterSql = DynamicQuery.GetDynamicQuery<TEntity>(this.tableName, this.fieldIdName.First().Key, filter);

            expando = filterSql.Param;

            return this.Exists(filterSql.Sql, expando);
        }
        #endregion

        #region ReadOnly Extended TSQL
        public virtual int Count(string sql, IDictionary<string, object> param = null)
        {
            var listResult = this.Connection.Query<dynamic>(sql, param, this.transaction).ToList();
            return listResult.Count;
        }

        public virtual bool Exists(string sql, IDictionary<string, object> param = null)
        {
            var listResult = this.Connection.Query<dynamic>(sql, param, this.transaction).ToList();
            return listResult.Count >= 1;
        }
        #endregion

        #region ReadOnly Pagination
        public virtual IPagination<TEntity> FindAll(int pageIndex, int pageSize, string fieldOrderBy, bool desc = false, Expression<Func<TEntity, bool>> filter = null)
        {
            QueryResult filterSql = null;
            IDictionary<string, Object> expando = new ExpandoObject();
            if (filter == null)
                filterSql = new QueryResult(this.selectDefault, expando);
            else
                filterSql = DynamicQuery.GetDynamicQuery<TEntity>(this.tableName, this.fieldIdName.First().Key, filter);

            string sql = filterSql.Sql;
            expando = filterSql.Param;

            return FindAll(pageIndex, pageSize, fieldOrderBy, sql, desc, expando);
        }
        #endregion

        #region ReadOnly Pagination Async
        public virtual async Task<IPagination<TEntity>> FindAllAsync(int pageIndex, int pageSize, string fieldOrderBy, bool desc = false, Expression<Func<TEntity, bool>> filter = null)
        {
            QueryResult filterSql = null;
            IDictionary<string, Object> expando = new ExpandoObject();
            if (filter == null)
                filterSql = new QueryResult(this.selectDefault, expando);
            else
                filterSql = DynamicQuery.GetDynamicQuery<TEntity>(this.tableName, this.fieldIdName.First().Key, filter);

            string sql = filterSql.Sql;
            expando = filterSql.Param;

            return await FindAllAsync(pageIndex, pageSize, fieldOrderBy, sql, desc, expando);
        }
        #endregion

        #region Write TSQL
        public virtual void execute(string sql, IDictionary<string, object> param = null, int numberOfRowsChanges = 1)
        {
            try
            {
                int result = 0;
                result = Connection.Execute(sql, param, transaction);
                if (result != numberOfRowsChanges)
                    throw new Exception($"Has a problem with this execution. The number of changed rows is not equals the parameter (the number of affected rows is {result}). Execution aborted");

            }
            catch (Exception ex)
            {
                Exception auxex = null;
                try
                {
                    (this.unitOfWorkDapper as UnitOfWorkDapper<TContext>).RollbackTransaction();
                }
                catch (Exception _ex)
                {
                    auxex = _ex;
                }

                throw auxex == null ? ex : new Exception(CompileExceptionObjectOnOneString(ex, string.Empty), auxex);
            }
        }
        #endregion

        #region Write TSQL Async
        public virtual async Task executeAsync(string sql, IDictionary<string, object> param = null, int numberOfRowsChanges = 1)
        {
            try
            {
                int result = 0;
                result =  await Connection.ExecuteAsync(sql, param, transaction);
                if (result != numberOfRowsChanges)
                    throw new Exception($"Has a problem with this execution. The number of changed rows is not equals the parameter (the number of affected rows is {result}). Execution aborted");

            }
            catch (Exception ex)
            {
                Exception auxex = null;
                try
                {
                    (this.unitOfWorkDapper as UnitOfWorkDapper<TContext>).RollbackTransaction();
                }
                catch (Exception _ex)
                {
                    auxex = _ex;
                }

                throw auxex == null ? ex : new Exception(CompileExceptionObjectOnOneString(ex, string.Empty), auxex);
            }
        }
        #endregion
    }
}
