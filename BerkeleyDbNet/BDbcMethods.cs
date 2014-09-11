using BerkeleyDbClient;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BerkeleyDbNet
{
    public sealed class BDbcMethods
    {
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int close(IntPtr dbc);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int del(IntPtr dbc, uint flags);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int get(IntPtr dbc, ref db_dbt key, ref db_dbt data, uint flags);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int put(IntPtr dbc, ref db_dbt key, ref db_dbt data, uint flags);

        private static BDbcMethods _btree;
        private static BDbcMethods _hash;
        private static BDbcMethods _recno;
        private static BDbcMethods _queue;
        private static BDbcMethods _unknown;
        private static BDbcMethods _heap;

        private readonly BerkeleyDbType _dbType;
        private readonly BDbOffsetOf _offsetOf;

        private readonly close _close;
        private readonly del _del;
        private readonly get _get;
        private readonly put _put;

        private BDbcMethods(IntPtr pdbc, BDbOffsetOf offsetOf, BerkeleyDbType dbType)
        {
            _offsetOf = offsetOf;
            _dbType = dbType;

            foreach (BDbOffsetOfItem offsetOfItem in offsetOf.Dbc)
            {
                IntPtr funcptr = Marshal.ReadIntPtr(pdbc + offsetOfItem.Offset);
                switch (offsetOfItem.Name)
                {
                    case "close":
                        _close = (close)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(close));
                        break;
                    case "del":
                        _del = (del)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(del));
                        break;
                    case "get":
                        _get = (get)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(get));
                        break;
                    case "put":
                        _put = (put)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(put));
                        break;
                }
            }
        }

        private static BDbcMethods CreateMethods(BerkeleyDbType dbType, IntPtr pdbc, BDbOffsetOf offsetOf)
        {
            BDbcMethods dbcMethods = GetMethodsInternal(dbType);
            if (dbcMethods == null)
            {
                lock (typeof(BDbcMethods))
                {
                    dbcMethods = GetMethodsInternal(dbType);
                    if (dbcMethods == null)
                    {
                        dbcMethods = new BDbcMethods(pdbc, offsetOf, dbType);
                        switch (dbType)
                        {
                            case BerkeleyDbType.DB_BTREE:
                                _btree = dbcMethods;
                                break;
                            case BerkeleyDbType.DB_HASH:
                                _hash = dbcMethods;
                                break;
                            case BerkeleyDbType.DB_RECNO:
                                _recno = dbcMethods;
                                break;
                            case BerkeleyDbType.DB_QUEUE:
                                _queue = dbcMethods;
                                break;
                            case BerkeleyDbType.DB_UNKNOWN:
                                _unknown = dbcMethods;
                                break;
                            case BerkeleyDbType.DB_HEAP:
                                _heap = dbcMethods;
                                break;
                        }
                    }
                }
            }
            return dbcMethods;
        }
        public static BDbcMethods GetMethods(BerkeleyDbType dbType)
        {
            BDbcMethods methods = GetMethodsInternal(dbType);
            if (methods == null)
                throw new InvalidOperationException("must call DbcMethods.Create");
            return methods;
        }
        private static BDbcMethods GetMethodsInternal(BerkeleyDbType dbType)
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

        public BerkeleyDbError Close(IntPtr pdbc)
        {
            return (BerkeleyDbError)_close(pdbc);
        }
        public static BerkeleyDbError Create(BDbOffsetOf offsetOf, IntPtr pdb, BerkeleyDbType dbType, BerkeleyDbCursorFlags flags, out IntPtr pdbc)
        {
            BDbMethods dbMethods = BDbMethods.GetMethods(dbType);
            BerkeleyDbError error = dbMethods.Cursor(pdb, flags, out pdbc);
            if (error == 0)
                CreateMethods(dbType, pdbc, offsetOf);
            return error;
        }
        public BerkeleyDbError Del(IntPtr pdbc, BerkeleyDbDelete flags)
        {
            return (BerkeleyDbError)_del(pdbc, (uint)flags);
        }
        public BerkeleyDbError Get(IntPtr pdbc, Byte[] key, Byte[] data, BerkeleyDbOperation operation, BerkeleyDbMultiple multiple, out int keySize, out int dataSize)
        {
            var keyDbt = new db_dbt();
            var dataDbt = new db_dbt();
            try
            {
                keyDbt.Init(key);
                dataDbt.Init(data.Length);

                var error = (BerkeleyDbError)_get(pdbc, ref keyDbt, ref dataDbt, (uint)operation | (uint)multiple);
                if (error == 0)
                {
                    keyDbt.CopyToArray(key);
                    dataDbt.CopyToArray(data);
                }

                keySize = (int)keyDbt.size;
                dataSize = (int)dataDbt.size;
                return error;
            }
            finally
            {
                keyDbt.Dispose();
                dataDbt.Dispose();
            }
        }
        public BerkeleyDbError GetPartial(IntPtr pdbc, Byte[] key, Byte[] data, int dataOffset, out int dataSize)
        {
            var keyDbt = new db_dbt();
            var dataDbt = new db_dbt();

            try
            {
                BerkeleyDbOperation flags;
                if (dataOffset == 0)
                {
                    keyDbt.Init(key);
                    flags = BerkeleyDbOperation.DB_SET;
                }
                else
                {
                    keyDbt.flags = db_dbt.DB_DBT_READONLY;
                    flags = BerkeleyDbOperation.DB_CURRENT;
                }

                dataDbt.Init(data);
                dataDbt.dlen = (uint)data.Length;
                dataDbt.doff = (uint)dataOffset;
                dataDbt.flags |= db_dbt.DB_DBT_PARTIAL;

                var error = (BerkeleyDbError)_get(pdbc, ref keyDbt, ref dataDbt, (uint)flags);
                if (error == 0)
                    dataDbt.CopyToArray(data);
                dataSize = (int)dataDbt.size;
                return error;
            }
            finally
            {
                keyDbt.Dispose();
                dataDbt.Dispose();
            }
        }
        public BerkeleyDbError PutPartial(IntPtr pdbc, Byte[] key, Byte[] data, int dataOffset, int dataLength)
        {
            var keyDbt = new db_dbt();
            var dataDbt = new db_dbt();

            try
            {
                BerkeleyDbOperation flags;
                if (dataOffset == 0)
                {
                    keyDbt.Init(key);
                    flags = BerkeleyDbOperation.DB_KEYFIRST;
                }
                else
                {
                    keyDbt.flags = db_dbt.DB_DBT_READONLY;
                    flags = BerkeleyDbOperation.DB_CURRENT;
                }

                dataDbt.Init(data);
                dataDbt.dlen = (uint)dataLength;
                dataDbt.doff = (uint)dataOffset;
                dataDbt.flags |= db_dbt.DB_DBT_PARTIAL;

                return (BerkeleyDbError)_put(pdbc, ref keyDbt, ref dataDbt, (uint)flags);
            }
            finally
            {
                keyDbt.Dispose();
                dataDbt.Dispose();
            }
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
