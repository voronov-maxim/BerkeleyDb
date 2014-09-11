using BerkeleyDbClient;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace BerkeleyDbNet
{
    public sealed class BDbMethods
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport(BDbenvMethods.dllname, EntryPoint = "db_create", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int db_create(out IntPtr db, IntPtr dbenv, uint flags);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int close(IntPtr db, uint flags);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int cursor(IntPtr db, IntPtr txnid, out IntPtr cursorp, uint flags);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int del(IntPtr db, IntPtr txnid, ref db_dbt key, uint flags);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int get_pagesize(IntPtr db, out uint pagesizep);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int open(IntPtr db, IntPtr txnid, Byte[] file, Byte[] database, int type, uint flags, int mode);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int put(IntPtr db, IntPtr txnid, ref db_dbt key, ref db_dbt data, uint flags);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int set_flags(IntPtr db, uint flags);

        private static BDbMethods _btree;
        private static BDbMethods _hash;
        private static BDbMethods _recno;
        private static BDbMethods _queue;
        private static BDbMethods _unknown;
        private static BDbMethods _heap;

        private readonly BerkeleyDbType _dbType;
        private readonly BDbOffsetOf _offsetOf;

        private readonly close _close;
        private readonly cursor _cursor;
        private readonly del _del;
        private readonly get_pagesize _get_pagesize;
        private readonly open _open;
        private readonly put _put;
        private readonly set_flags _set_flags;

        private BDbMethods(IntPtr pdb, BDbOffsetOf offsetOf, BerkeleyDbType dbType)
        {
            _dbType = dbType;
            _offsetOf = offsetOf;

            foreach (BDbOffsetOfItem offsetOfItem in offsetOf.Db)
            {
                IntPtr funcptr = Marshal.ReadIntPtr(pdb + offsetOfItem.Offset);
                switch (offsetOfItem.Name)
                {
                    case "close":
                        _close = (close)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(close));
                        break;
                    case "cursor":
                        _cursor = (cursor)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(cursor));
                        break;
                    case "del":
                        _del = (del)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(del));
                        break;
                    case "get_pagesize":
                        _get_pagesize = (get_pagesize)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(get_pagesize));
                        break;
                    case "open":
                        _open = (open)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(open));
                        break;
                    case "put":
                        _put = (put)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(put));
                        break;
                    case "set_flags":
                        _set_flags = (set_flags)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(set_flags));
                        break;
                }
            }
        }

        private static BDbMethods CreateMethods(BerkeleyDbType dbType, IntPtr pdb, BDbOffsetOf offsetOf)
        {
            BDbMethods dbMethods = GetMethodsInternal(dbType);
            if (dbMethods == null)
            {
                lock (typeof(BDbMethods))
                {
                    dbMethods = GetMethodsInternal(dbType);
                    if (dbMethods == null)
                    {
                        dbMethods = new BDbMethods(pdb, offsetOf, dbType);
                        switch (dbType)
                        {
                            case BerkeleyDbType.DB_BTREE:
                                _btree = dbMethods;
                                break;
                            case BerkeleyDbType.DB_HASH:
                                _hash = dbMethods;
                                break;
                            case BerkeleyDbType.DB_RECNO:
                                _recno = dbMethods;
                                break;
                            case BerkeleyDbType.DB_QUEUE:
                                _queue = dbMethods;
                                break;
                            case BerkeleyDbType.DB_UNKNOWN:
                                _unknown = dbMethods;
                                break;
                            case BerkeleyDbType.DB_HEAP:
                                _heap = dbMethods;
                                break;
                        }
                    }
                }
            }
            return dbMethods;
        }
        public static BDbMethods GetMethods(BerkeleyDbType dbType)
        {
            BDbMethods methods = GetMethodsInternal(dbType);
            if (methods == null)
                throw new InvalidOperationException("must call DbMethods.Create");
            return methods;
        }
        private static BDbMethods GetMethodsInternal(BerkeleyDbType dbType)
        {
            switch (dbType)
            {
                case BerkeleyDbType.DB_BTREE:
                    return _btree;
                case BerkeleyDbType.DB_HASH:
                    return _hash;
                case BerkeleyDbType.DB_RECNO:
                    return _recno;
                case BerkeleyDbType.DB_QUEUE:
                    return _queue;
                case BerkeleyDbType.DB_UNKNOWN:
                    return _unknown;
                case BerkeleyDbType.DB_HEAP:
                    return _hash;
                default:
                    throw new ArgumentOutOfRangeException("dbType", dbType.ToString());
            }

        }

        public BerkeleyDbError Close(IntPtr pdb, BerkeleyDbClose flags)
        {
            return (BerkeleyDbError)_close(pdb, (uint)flags);
        }
        public static BerkeleyDbError Create(BDbOffsetOf offsetOf, IntPtr penv, BerkeleyDbType dbType, BerkeleyDbFlags flags, out IntPtr pdb)
        {
            var error = (BerkeleyDbError)db_create(out pdb, penv, 0);
            if (error != 0)
                return error;

            BDbMethods methods = CreateMethods(dbType, pdb, offsetOf);
            error = (BerkeleyDbError)methods.SetFlags(pdb, flags);
            if (error != 0 && pdb != IntPtr.Zero)
            {
                methods.Close(pdb, 0);
                pdb = IntPtr.Zero;
            }
            return error;
        }
        public BerkeleyDbError Cursor(IntPtr pdb, BerkeleyDbCursorFlags flags, out IntPtr pdbc)
        {
            return (BerkeleyDbError)_cursor(pdb, IntPtr.Zero, out pdbc, 0);
        }
        public BerkeleyDbError Del(IntPtr pdb, Byte[] key, BerkeleyDbDelete flag, BerkeleyDbMultiple multiple)
        {
            var keyDbt = new db_dbt();
            try
            {
                keyDbt.Init(key);
                keyDbt.flags |= db_dbt.DB_DBT_READONLY;
                if (multiple != 0)
                    keyDbt.flags |= db_dbt.DB_DBT_BULK;

                return (BerkeleyDbError)_del(pdb, IntPtr.Zero, ref keyDbt, (uint)flag | (uint)multiple);
            }
            finally
            {
                keyDbt.Dispose();
            }
        }
        public BerkeleyDbError GetPageSize(IntPtr pdb, out uint pageSize)
        {
            return (BerkeleyDbError)_get_pagesize(pdb, out pageSize);
        }
        public BerkeleyDbError Open(IntPtr pdb, String fileName, BerkeleyDbOpenFlags flags)
        {
            Byte[] fileNameUtf8 = Encoding.UTF8.GetBytes(fileName);
            return (BerkeleyDbError)_open(pdb, IntPtr.Zero, fileNameUtf8, null, (int)_dbType, (uint)flags, 0);
        }
        public BerkeleyDbError Put(IntPtr pdb, Byte[] key, Byte[] data, BerkeleyDbWriteMode flags)
        {
            var keyDbt = new db_dbt();
            var dataDbt = new db_dbt();
            try
            {
                keyDbt.Init(key);
                dataDbt.Init(data);
                return (BerkeleyDbError)_put(pdb, IntPtr.Zero, ref keyDbt, ref dataDbt, (uint)flags);
            }
            finally
            {
                keyDbt.Dispose();
                dataDbt.Dispose();
            }
        }
        public BerkeleyDbError PutMultipleDuplicate(IntPtr pdb, Byte[] key, Byte[] data, BerkeleyDbWriteMode flags)
        {
            var keyDbt = new db_dbt();
            var dataDbt = new db_dbt();
            try
            {
                keyDbt.Init(key);
                keyDbt.flags |= db_dbt.DB_DBT_BULK;

                dataDbt.Init(data);
                dataDbt.flags |= db_dbt.DB_DBT_BULK;

                return (BerkeleyDbError)_put(pdb, IntPtr.Zero, ref keyDbt, ref dataDbt, (uint)flags | (int)BerkeleyDbMultiple.DB_MULTIPLE);
            }
            finally
            {
                keyDbt.Dispose();
                dataDbt.Dispose();
            }
        }
        public BerkeleyDbError PutMultipleKey(IntPtr pdb, Byte[] data, BerkeleyDbWriteMode flags)
        {
            var keyDbt = new db_dbt();
            var dataDbt = new db_dbt();
            try
            {
                keyDbt.Init(data);
                keyDbt.flags |= db_dbt.DB_DBT_BULK;

                dataDbt.flags |= db_dbt.DB_DBT_READONLY;

                return (BerkeleyDbError)_put(pdb, IntPtr.Zero, ref keyDbt, ref dataDbt, (uint)flags | (int)BerkeleyDbMultiple.DB_MULTIPLE_KEY);
            }
            finally
            {
                keyDbt.Dispose();
                dataDbt.Dispose();
            }
        }
        public BerkeleyDbError SetFlags(IntPtr pdb, BerkeleyDbFlags flags)
        {
            return (BerkeleyDbError)_set_flags(pdb, (uint)flags);
        }

        public BerkeleyDbType DbType
        {
            get
            {
                return _dbType;
            }
        }
        public BDbOffsetOf OffsetOf
        {
            get
            {
                return _offsetOf;
            }
        }
    }
}
