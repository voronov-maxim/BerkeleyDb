using System;
using System.Text;

namespace BerkeleyDbClient
{
    internal static class DbConst
    {
        public const int DB_CREATE = 0x00000001;
        public const int DB_THREAD = 0x00000020;
    }

    public enum BerkeleyDbClose
    {
        DB_NOSYNC = 0x00000001
    }

    [Flags]
    public enum BerkeleyDbCursorFlags
    {
        DB_CURSOR_BULK = 0x00000001,
        DB_READ_COMMITTED = 0x00000400,
        DB_READ_UNCOMMITTED = 0x00000200,
        DB_TXN_SNAPSHOT = 0x00000004,
        DB_WRITECURSOR = 0x00000010
    }

    public enum BerkeleyDbDelete
    {
        DB_CONSUME = 4
    }

    public enum BerkeleyDbEnvClose
    {
        DB_FORCESYNC = 0x00000001,
        DB_FORCESYNCENV = 0x00000002
    }

    [Flags]
    public enum BerkeleyDbEnvOpen
    {
        DB_INIT_CDB = 0x00000080,
        DB_INIT_LOCK = 0x00000100,
        DB_INIT_MPOOL = 0x00000400,
        DB_INIT_REP = 0x00001000,
        DB_INIT_TXN = 0x00002000,

        DB_RECOVER = 0x00000002,
        DB_RECOVER_FATAL = 0x00020000,

        DB_USE_ENVIRON = 0x00000004,
        DB_USE_ENVIRON_ROOT = 0x00000008,

        DB_CREATE = DbConst.DB_CREATE,
        DB_FAILCHK = 0x00000010,
        DB_LOCKDOWN = 0x00004000,
        DB_PRIVATE = 0x00010000,
        DB_REGISTER = 0x00040000,
        DB_SYSTEM_MEM = 0x00080000,
        DB_THREAD = DbConst.DB_THREAD
    }

    public enum BerkeleyDbError
    {
        DB_BUFFER_SMALL = -30999,
        DB_FOREIGN_CONFLICT = -30997,
        DB_KEYEMPTY = -30995,
        DB_KEYEXIST = -30994,
        DB_LOCK_DEADLOCK = -30993,
        DB_LOCK_NOTGRANTED = -30992,
        DB_NOTFOUND = -30988,
        DB_OLD_VERSION = -30987,
        DB_REP_HANDLE_DEAD = -30984,
        DB_REP_LEASE_EXPIRED = -30979,
        DB_REP_LOCKOUT = -30978,
        DB_SECONDARY_BAD = -30972,
        ENOENT = 2,
        EACCES = 13,
        EEXIST = 17,
        EINVAL = 22,
        ENOSPC = 28,
        EALREADY = 37,
        EADDRINUSE = 48
    }

    public enum BerkeleyDbFlags
    {
        DB_CHKSUM = 0x00000008,
        DB_DUP = 0x00000010,
        DB_DUPSORT = 0x00000002,
        DB_ENCRYPT = 0x00000001,
        DB_INORDER = 0x00000020,
        DB_RECNUM = 0x00000040,
        DB_RENUMBER = 0x00000080,
        DB_REVSPLITOFF = 0x00000100,
        DB_SNAPSHOT = 0x00000200,
        DB_TXN_NOT_DURABLE = 0x00000004
    }

    public enum BerkeleyDbMultiple
    {
        DB_MULTIPLE = 0x00000800,
        DB_MULTIPLE_KEY = 0x00004000
    }

    [Flags]
    public enum BerkeleyDbOpenFlags
    {
        DB_AUTO_COMMIT = 0x00000100,
        DB_CREATE = DbConst.DB_CREATE,
        DB_EXCL = 0x00000004,
        DB_MULTIVERSION = 0x00000008,
        DB_NOMMAP = 0x00000010,
        DB_RDONLY = 0x00000400,
        DB_READ_UNCOMMITTED = 0x00000200,
        DB_THREAD = DbConst.DB_THREAD,
        DB_TRUNCATE = 0x00040000
    }


    public enum BerkeleyDbOperation
    {
        DB_AFTER = 1,
        DB_CONSUME = 4,
        DB_CURRENT = 6,
        DB_FIRST = 7,
        DB_GET_BOTH = 8,
        DB_GET_BOTH_RANGE = 10,
        DB_GET_RECNO = 11,
        DB_JOIN_ITEM = 12,
        DB_KEYFIRST = 13,
        DB_KEYLAST = 14,
        DB_LAST = 15,
        DB_NEXT = 16,
        DB_NEXT_DUP = 17,
        DB_NEXT_NODUP = 18,
        DB_NODUPDATA = 19,
        DB_PREV = 23,
        DB_PREV_DUP = 24,
        DB_PREV_NODUP = 25,
        DB_SET = 26,
        DB_SET_RANGE = 27,
        DB_SET_RECNO = 28,
    }

    public enum BerkeleyDbType
    {
        DB_BTREE = 1,
        DB_HASH = 2,
        DB_RECNO = 3,
        DB_QUEUE = 4,
        DB_UNKNOWN = 5,
        DB_HEAP = 6
    }

    public enum BerkeleyDbWriteMode
    {
        DB_APPEND = 2,
        DB_NODUPDATA = 19,
        DB_NOOVERWRITE = 20,
        DB_OVERWRITE_DUP = 21,
    }

    public static class BerkeleyEnumExtension
    {
        public static String ToStringEx(this BerkeleyDbClose flags)
        {
            switch (flags)
            {
                case 0:
                    return "0";
                case BerkeleyDbClose.DB_NOSYNC:
                    return "db_nosync";
                default:
                    throw new ArgumentOutOfRangeException(flags.ToString());
            }
        }
        public static String ToStringEx(this BerkeleyDbCursorFlags flags)
        {
            if (flags == 0)
                return "0";

            var stringFlags = new StringBuilder();

            if ((flags & BerkeleyDbCursorFlags.DB_CURSOR_BULK) != 0)
                stringFlags.Append("db_cursor_bulk,");
            if ((flags & BerkeleyDbCursorFlags.DB_READ_COMMITTED) != 0)
                stringFlags.Append("db_read_committed,");
            if ((flags & BerkeleyDbCursorFlags.DB_READ_UNCOMMITTED) != 0)
                stringFlags.Append("db_read_uncommitted,");
            if ((flags & BerkeleyDbCursorFlags.DB_TXN_SNAPSHOT) != 0)
                stringFlags.Append("db_txn_snapshot,");
            if ((flags & BerkeleyDbCursorFlags.DB_WRITECURSOR) != 0)
                stringFlags.Append("db_writecursor,");

            stringFlags.Length--;
            return stringFlags.ToString();
        }
        public static String ToStringEx(this BerkeleyDbDelete flags)
        {
            switch (flags)
            {
                case 0:
                    return "0";
                case BerkeleyDbDelete.DB_CONSUME:
                    return "db_consume";
                default:
                    throw new ArgumentOutOfRangeException(flags.ToString());
            }
        }
        public static String ToStringEx(this BerkeleyDbEnvClose flags)
        {
            switch (flags)
            {
                case 0:
                    return "0";
                case BerkeleyDbEnvClose.DB_FORCESYNC:
                    return "db_forcesync";
                case BerkeleyDbEnvClose.DB_FORCESYNCENV:
                    return "db_forcesyncenv";
                default:
                    throw new ArgumentOutOfRangeException(flags.ToString());
            }
        }
        public static String ToStringEx(this BerkeleyDbEnvOpen flags)
        {
            if (flags == 0)
                return String.Empty;

            var stringFlags = new StringBuilder();

            if ((flags & BerkeleyDbEnvOpen.DB_INIT_CDB) != 0)
                stringFlags.Append("db_init_cdb,");
            if ((flags & BerkeleyDbEnvOpen.DB_INIT_LOCK) != 0)
                stringFlags.Append("db_init_lock,");
            if ((flags & BerkeleyDbEnvOpen.DB_INIT_MPOOL) != 0)
                stringFlags.Append("db_init_mpool,");
            if ((flags & BerkeleyDbEnvOpen.DB_INIT_REP) != 0)
                stringFlags.Append("db_init_rep,");
            if ((flags & BerkeleyDbEnvOpen.DB_INIT_TXN) != 0)
                stringFlags.Append("db_init_txn,");

            if ((flags & BerkeleyDbEnvOpen.DB_RECOVER) != 0)
                stringFlags.Append("db_recover,");
            if ((flags & BerkeleyDbEnvOpen.DB_RECOVER_FATAL) != 0)
                stringFlags.Append("db_recover_fatal,");

            if ((flags & BerkeleyDbEnvOpen.DB_USE_ENVIRON) != 0)
                stringFlags.Append("db_use_environ,");
            if ((flags & BerkeleyDbEnvOpen.DB_USE_ENVIRON_ROOT) != 0)
                stringFlags.Append("db_use_environ_root,");

            if ((flags & BerkeleyDbEnvOpen.DB_CREATE) != 0)
                stringFlags.Append("db_create,");
            if ((flags & BerkeleyDbEnvOpen.DB_FAILCHK) != 0)
                stringFlags.Append("db_failchk,");
            if ((flags & BerkeleyDbEnvOpen.DB_LOCKDOWN) != 0)
                stringFlags.Append("db_lockdown,");
            if ((flags & BerkeleyDbEnvOpen.DB_PRIVATE) != 0)
                stringFlags.Append("db_private,");
            if ((flags & BerkeleyDbEnvOpen.DB_REGISTER) != 0)
                stringFlags.Append("db_register,");
            if ((flags & BerkeleyDbEnvOpen.DB_SYSTEM_MEM) != 0)
                stringFlags.Append("db_system_mem,");
            if ((flags & BerkeleyDbEnvOpen.DB_THREAD) != 0)
                stringFlags.Append("db_thread,");

            stringFlags.Length--;
            return stringFlags.ToString();
        }
        public static String ToStringEx(this BerkeleyDbFlags flag)
        {
            switch (flag)
            {
                case 0:
                    return "0";
                case BerkeleyDbFlags.DB_CHKSUM:
                    return "db_chksum";
                case BerkeleyDbFlags.DB_DUP:
                    return "db_dup";
                case BerkeleyDbFlags.DB_DUPSORT:
                    return "db_dupsort";
                case BerkeleyDbFlags.DB_ENCRYPT:
                    return "db_encrypt";
                case BerkeleyDbFlags.DB_INORDER:
                    return "db_inorder";
                case BerkeleyDbFlags.DB_TXN_NOT_DURABLE:
                    return "db_txn_not_durable";
                case BerkeleyDbFlags.DB_RECNUM:
                    return "db_recnum";
                case BerkeleyDbFlags.DB_RENUMBER:
                    return "db_renumber";
                case BerkeleyDbFlags.DB_REVSPLITOFF:
                    return "db_revsplitoff";
                case BerkeleyDbFlags.DB_SNAPSHOT:
                    return "db_snapshot";
                default:
                    throw new ArgumentOutOfRangeException(flag.ToString());
            }
        }
        public static String ToStringEx(this BerkeleyDbMultiple flag)
        {
            switch (flag)
            {
                case 0:
                    return "0";
                case BerkeleyDbMultiple.DB_MULTIPLE:
                    return "db_multiple";
                case BerkeleyDbMultiple.DB_MULTIPLE_KEY:
                    return "db_multiple_key";
                default:
                    throw new ArgumentOutOfRangeException(flag.ToString());
            }
        }
        public static String ToStringEx(this BerkeleyDbOpenFlags flags)
        {
            var flagsBuilder = new StringBuilder();

            if ((flags & BerkeleyDbOpenFlags.DB_AUTO_COMMIT) != 0)
                flagsBuilder.Append("db_auto_commit,");
            if ((flags & BerkeleyDbOpenFlags.DB_CREATE) != 0)
                flagsBuilder.Append("db_create,");
            if ((flags & BerkeleyDbOpenFlags.DB_EXCL) != 0)
                flagsBuilder.Append("db_excl,");
            if ((flags & BerkeleyDbOpenFlags.DB_MULTIVERSION) != 0)
                flagsBuilder.Append("db_multiversion,");
            if ((flags & BerkeleyDbOpenFlags.DB_NOMMAP) != 0)
                flagsBuilder.Append("db_nommap,");
            if ((flags & BerkeleyDbOpenFlags.DB_RDONLY) != 0)
                flagsBuilder.Append("db_rdonly,");
            if ((flags & BerkeleyDbOpenFlags.DB_READ_UNCOMMITTED) != 0)
                flagsBuilder.Append("db_read_uncommitted,");
            if ((flags & BerkeleyDbOpenFlags.DB_THREAD) != 0)
                flagsBuilder.Append("db_thread,");
            if ((flags & BerkeleyDbOpenFlags.DB_TRUNCATE) != 0)
                flagsBuilder.Append("db_truncate,");

            flagsBuilder.Length--;
            return flagsBuilder.ToString();
        }
        public static String ToStringEx(this BerkeleyDbOperation flag)
        {
            switch (flag)
            {
                case 0:
                    return "0";
                case BerkeleyDbOperation.DB_AFTER:
                    return "db_after";
                case BerkeleyDbOperation.DB_CONSUME:
                    return "db_consume";
                case BerkeleyDbOperation.DB_CURRENT:
                    return "db_current";
                case BerkeleyDbOperation.DB_FIRST:
                    return "db_first";
                case BerkeleyDbOperation.DB_GET_BOTH:
                    return "db_get_both";
                case BerkeleyDbOperation.DB_GET_BOTH_RANGE:
                    return "db_get_both_range";
                case BerkeleyDbOperation.DB_GET_RECNO:
                    return "db_get_recno";
                case BerkeleyDbOperation.DB_JOIN_ITEM:
                    return "db_join_item";
                case BerkeleyDbOperation.DB_KEYFIRST:
                    return "db_keyfirst";
                case BerkeleyDbOperation.DB_KEYLAST:
                    return "db_keylast";
                case BerkeleyDbOperation.DB_LAST:
                    return "db_last";
                case BerkeleyDbOperation.DB_NEXT:
                    return "db_next";
                case BerkeleyDbOperation.DB_NEXT_DUP:
                    return "db_next_dup";
                case BerkeleyDbOperation.DB_NEXT_NODUP:
                    return "db_next_nodup";
                case BerkeleyDbOperation.DB_NODUPDATA:
                    return "db_nodupdata";
                case BerkeleyDbOperation.DB_PREV:
                    return "db_prev";
                case BerkeleyDbOperation.DB_PREV_DUP:
                    return "db_prev_dup";
                case BerkeleyDbOperation.DB_PREV_NODUP:
                    return "db_prev_nodup";
                case BerkeleyDbOperation.DB_SET:
                    return "db_set";
                case BerkeleyDbOperation.DB_SET_RANGE:
                    return "db_set_range";
                case BerkeleyDbOperation.DB_SET_RECNO:
                    return "db_set_recno";
                default:
                    throw new ArgumentOutOfRangeException(flag.ToString());
            }
        }
        public static String ToStringEx(this BerkeleyDbType flag)
        {
            switch (flag)
            {
                case BerkeleyDbType.DB_BTREE:
                    return "db_btree";
                case BerkeleyDbType.DB_HASH:
                    return "db_hash";
                case BerkeleyDbType.DB_RECNO:
                    return "db_recno";
                case BerkeleyDbType.DB_QUEUE:
                    return "db_queue";
                case BerkeleyDbType.DB_UNKNOWN:
                    return "db_unknown";
                default:
                    throw new ArgumentOutOfRangeException(flag.ToString());
            }
        }
        public static String ToStringEx(this BerkeleyDbWriteMode flag)
        {
            switch (flag)
            {
                case 0:
                    return "0";
                case BerkeleyDbWriteMode.DB_APPEND:
                    return "db_append";
                case BerkeleyDbWriteMode.DB_NODUPDATA:
                    return "db_nodupdata";
                case BerkeleyDbWriteMode.DB_NOOVERWRITE:
                    return "db_nooverwrite";
                case BerkeleyDbWriteMode.DB_OVERWRITE_DUP:
                    return "db_overwrite_dup";
                default:
                    throw new ArgumentOutOfRangeException(flag.ToString());
            }
        }
    }
}
