using BerkeleyDbClient;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace BerkeleyDbNet
{
    public sealed class BDbenvMethods
    {
        public const string dllname = "libdb61";

        [SuppressUnmanagedCodeSecurity]
        [DllImport(dllname, EntryPoint = "__os_ufree", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern void os_ufree(IntPtr dbenv, IntPtr ptr);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(dllname, EntryPoint = "db_env_create", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int env_create(out IntPtr dbenvp, uint flags);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(dllname, EntryPoint = "db_strerror", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Ansi)]
        private static extern String db_strerror(int error);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int close(IntPtr dbenv, uint flags);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int open(IntPtr dbenv, Byte[] db_home, uint flags, int mode);

        private static BDbenvMethods _methods;

        private readonly close _close;
        private readonly open _open;

        private BDbenvMethods(IntPtr pdbenv, BDbOffsetOfCollection offsetOfs)
        {
            foreach (BDbOffsetOfItem offsetOfItem in offsetOfs)
            {
                IntPtr funcptr = Marshal.ReadIntPtr(pdbenv + offsetOfItem.Offset);
                switch (offsetOfItem.Name)
                {
                    case "close":
                        _close = (close)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(close));
                        break;
                    case "open":
                        _open = (open)Marshal.GetDelegateForFunctionPointer(funcptr, typeof(open));
                        break;
                }
            }
        }

        private static BDbenvMethods CreateMethods(IntPtr pdbenv, BDbOffsetOf offsetOf)
        {
            if (_methods == null)
            {
                lock (typeof(BDbenvMethods))
                {
                    if (_methods == null)
                        _methods = new BDbenvMethods(pdbenv, offsetOf.Dbenv);
                }
            }
            return _methods;
        }
        public static BDbenvMethods GetMethods()
        {
            if (_methods == null)
                throw new InvalidOperationException("must call DbenvMethods.Create");
            return _methods;

        }

        public BerkeleyDbError Close(IntPtr pdbenv, BerkeleyDbEnvClose flag)
        {
            return (BerkeleyDbError)_close(pdbenv, (uint)flag);
        }
        public static BerkeleyDbError Create(BDbOffsetOf offsetOf, out IntPtr pdbenv)
        {
            var error = (BerkeleyDbError)env_create(out pdbenv, 0);
            if (error == 0)
                CreateMethods(pdbenv, offsetOf);
            return error;
        }
        public BerkeleyDbError Open(IntPtr pdbenv, String dbHome, BerkeleyDbEnvOpen flags)
        {
            Byte[] dbHomeUtf8 = Encoding.UTF8.GetBytes(dbHome);
            return (BerkeleyDbError)_open(pdbenv, dbHomeUtf8, (uint)flags, 0);
        }
        public static String StrError(BerkeleyDbError error)
        {
            return db_strerror((int)error);
        }
    }
}
