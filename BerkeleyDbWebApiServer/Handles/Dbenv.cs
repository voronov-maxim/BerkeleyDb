using BerkeleyDbNet;
using BerkeleyDbClient;
using System;
using System.IO;

namespace BerkeleyDbWebApiServer
{
    public struct Dbenv
    {
        public readonly IntPtr Handle;
        public readonly BDbenvMethods Methods;

        public Dbenv(IntPtr pdbenv, BDbenvMethods methods)
        {
            Handle = pdbenv;
            Methods = methods;
        }
    }

    public static class DbenvInstance
    {
        private static Dbenv _instance;

        private static Dbenv CreateEnv(EnvConfig envConfig)
        {
            BDbOffsetOf offsetOf = BDbOffsetOfReader.ReadXmlFile();
            BDbOffsetOfInstance.SetInstance(offsetOf);

            IntPtr pdbenv;
            BerkeleyDbError error = BDbenvMethods.Create(offsetOf, out pdbenv);
            if (error != 0)
                throw new InvalidOperationException("db_env_create error " + BDbenvMethods.StrError(error));

            BDbenvMethods methods = BDbenvMethods.GetMethods();
            error = methods.Open(pdbenv, envConfig.DbHome, envConfig.OpenFlags);
            if (error == BerkeleyDbError.ENOENT && envConfig.UseTempIfFault)
            {
                String dbHome = Path.Combine(Path.GetTempPath(), "berkeley_db_home");
                Directory.CreateDirectory(dbHome);
                error = methods.Open(pdbenv, dbHome, envConfig.OpenFlags);
            }

            if (error != 0)
            {
                methods.Close(pdbenv, 0);
                throw new InvalidOperationException("DB_ENV->open error " + BDbenvMethods.StrError(error));
            }

            return new Dbenv(pdbenv, methods);
        }

        public static Dbenv Instance
        {
            get
            {
                if (_instance.Handle != IntPtr.Zero)
                    return _instance;

                lock (typeof(DbenvInstance))
                {
                    if (_instance.Handle != IntPtr.Zero)
                        return _instance;

                    _instance = CreateEnv(EnvConfigReader.ReadXmlFile());
                }

                return _instance;
            }
        }
    }
}
