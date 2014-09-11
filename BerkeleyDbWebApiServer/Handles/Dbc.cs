using BerkeleyDbNet;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace BerkeleyDbWebApiServer
{
    public struct DbcHandle
    {
        public readonly IntPtr Handle;
        public readonly BDbcMethods Methods;

        public DbcHandle(IntPtr pdbc, BDbcMethods methods)
        {
            Handle = pdbc;
            Methods = methods;
        }
    }

    public static class DbcInstance
    {
        private static ConcurrentDictionary<ulong, DbcHandle> _dbs = new ConcurrentDictionary<ulong, DbcHandle>();
        private static long _handle;

        public static ulong AddDbc(DbcHandle dbc)
        {
            ulong handle = (ulong)Interlocked.Increment(ref _handle);
            return _dbs.TryAdd(handle, dbc) ? handle : 0;
        }
        public static DbcHandle GetDbc(ulong handle)
        {
            DbcHandle dbc;
            _dbs.TryGetValue(handle, out dbc);
            return dbc;
        }
        public static DbcHandle RemoveDbc(ulong handle)
        {
            DbcHandle dbc;
            _dbs.TryRemove(handle, out dbc);
            return dbc;
        }
    }
}
