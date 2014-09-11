using BerkeleyDbNet;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace BerkeleyDbWebApiServer
{
    public struct DbHandle
    {
        public readonly IntPtr Handle;
        public readonly BDbMethods Methods;

        public DbHandle(IntPtr pdb, BDbMethods methods)
        {
            Handle = pdb;
            Methods = methods;
        }
    }

    public static class DbInstance
    {
        private static ConcurrentDictionary<ulong, DbHandle> _dbs = new ConcurrentDictionary<ulong, DbHandle>();
        private static long _handle;

        public static ulong AddDb(DbHandle db)
        {
            ulong handle = (ulong)Interlocked.Increment(ref _handle);
            return _dbs.TryAdd(handle, db) ? handle : 0;
        }
        public static DbHandle GetDb(ulong handle)
        {
            DbHandle db;
            _dbs.TryGetValue(handle, out db);
            return db;
        }
        public static DbHandle RemoveDb(ulong handle)
        {
            DbHandle db;
            _dbs.TryRemove(handle, out db);
            return db;
        }
    }
}
