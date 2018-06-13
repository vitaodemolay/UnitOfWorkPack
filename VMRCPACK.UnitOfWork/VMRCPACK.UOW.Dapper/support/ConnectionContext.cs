using System;
using System.Data;

namespace VMRCPACK.UOW.Dapper.support
{
    public class ConnectionContext : IDisposable
    {
        private readonly bool _ContextOwnsConnection;
        public readonly IDbConnection Connection;

        public ConnectionContext(IDbConnection connection, bool contextOwnsConnection)
        {
            this.Connection = connection;
            this._ContextOwnsConnection = contextOwnsConnection;
        }

        public virtual void Dispose()
        {
            if (_ContextOwnsConnection)
                Connection.Dispose();
        }

    }
}
