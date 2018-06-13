using System.Collections.Generic;
using System.Linq;

namespace VMRCPACK.UOW.Dapper.support
{
    public class DMapper<TEntity> where TEntity : class
    {
        public DMapper(string[] idNames)
        {
            global::Slapper.AutoMapper.Configuration.AddIdentifiers(typeof(TEntity), idNames.ToList());
        }

        public virtual void AddIdentifiers(System.Type type, string[] idNames)
        {
            global::Slapper.AutoMapper.Configuration.AddIdentifiers(type, idNames.ToList());
        }

        public virtual TEntity Map(dynamic queryesult)
        {
            global::Slapper.AutoMapper.Cache.ClearAllCaches();
            return (global::Slapper.AutoMapper.MapDynamic<TEntity>(queryesult) as TEntity);
        }

        public virtual IEnumerable<TEntity> MapList(dynamic queryResult)
        {
            global::Slapper.AutoMapper.Cache.ClearAllCaches();
            return (global::Slapper.AutoMapper.MapDynamic<TEntity>(queryResult) as IEnumerable<TEntity>).ToList();
        }
    }
}
