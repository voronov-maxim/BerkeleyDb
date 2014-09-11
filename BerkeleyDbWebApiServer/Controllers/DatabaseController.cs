using BerkeleyDbClient;
using BerkeleyDbClient.Dto;
using BerkeleyDbNet;
using System;
using System.Net;
using System.Web.Http;

namespace BerkeleyDbWebApiServer.Controllers
{
    public sealed class DatabaseController : ApiController
    {
        [HttpGet]
        public BerkeleyDbError Close([FromUri] ulong handle, BerkeleyDbClose flags)
        {
            DbHandle db = DbInstance.RemoveDb(handle);
            if (db.Handle == IntPtr.Zero)
                throw new HttpResponseException((HttpStatusCode)422);

            return db.Methods.Close(db.Handle, flags);
        }
        [HttpGet]
        public BerkeleyDtoResult Create([FromUri] BerkeleyDbType type, [FromUri] String flags)
        {
            ulong handle = 0;
            BerkeleyDbFlags bdbFlags = BerkeleyEnumParser.Flags(flags);

            IntPtr pdb;
            IntPtr penv = DbenvInstance.Instance.Handle;
            BerkeleyDbError error = BDbMethods.Create(BDbOffsetOfInstance.Instance, penv, type, bdbFlags, out pdb);
            if (error == 0)
            {
                BDbMethods dbMethods = BDbMethods.GetMethods(type);
                var db = new DbHandle(pdb, dbMethods);
                handle = DbInstance.AddDb(db);
                if (handle == 0)
                {
                    dbMethods.Close(pdb, 0);
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }
            }
                
            return new BerkeleyDtoResult(error, handle.ToString());
        }
        [HttpPost]
        public BerkeleyDbError Delete([FromUri] ulong handle, [FromUri] BerkeleyDbDelete flag, [FromUri] BerkeleyDbMultiple multiple, [FromBody] Byte[] key)
        {
            DbHandle db = GetDb(handle);
            return db.Methods.Del(db.Handle, key, flag, multiple);
        }
        [HttpGet]
        public BerkeleyDbError Open([FromUri] ulong handle, [FromUri] String name, [FromUri] String flags)
        {
            DbHandle db = GetDb(handle);
            BerkeleyDbOpenFlags openFlags = BerkeleyEnumParser.OpenFlags(flags);
            return db.Methods.Open(db.Handle, name, openFlags);
        }
        [HttpGet]
        public BerkeleyDtoResult OpenCursor([FromUri] ulong handle, [FromUri] String flags)
        {
            DbHandle db = GetDb(handle);
            BerkeleyDbCursorFlags cursorFlags = flags == null ? 0 : BerkeleyEnumParser.CursorFlags(flags);

            ulong cursorHandle = 0;
            IntPtr pdbc;
            BerkeleyDbError error = BDbcMethods.Create(BDbOffsetOfInstance.Instance, db.Handle, db.Methods.DbType, cursorFlags, out pdbc);
            if (error == 0)
            {
                DbcHandle dbc = new DbcHandle(pdbc, BDbcMethods.GetMethods(db.Methods.DbType));
                cursorHandle = DbcInstance.AddDbc(dbc);
                if (cursorHandle == 0)
                {
                    dbc.Methods.Close(dbc.Handle);
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }
            }

            return new BerkeleyDtoResult(error, cursorHandle.ToString());
        }
        [HttpGet]
        public BerkeleyDtoResult PageSize([FromUri] ulong handle)
        {
            uint pageSize;
            DbHandle db = GetDb(handle);
            BerkeleyDbError error = db.Methods.GetPageSize(db.Handle, out pageSize);
            return new BerkeleyDtoResult(error, pageSize.ToString());
        }
        public BerkeleyDbError Write([FromUri] ulong handle, BerkeleyDbWriteMode writeMode, [FromBody] BerkeleyDtoPut data)
        {
            DbHandle db = GetDb(handle);
            return db.Methods.Put(db.Handle, data.Key, data.Value, writeMode);
        }
        public BerkeleyDbError WriteMultipleDuplicate([FromUri] ulong handle, BerkeleyDbWriteMode writeMode, [FromBody] BerkeleyDtoPut data)
        {
            DbHandle db = GetDb(handle);
            return db.Methods.PutMultipleDuplicate(db.Handle, data.Key, data.Value, writeMode);
        }
        public BerkeleyDbError WriteMultipleKey([FromUri] ulong handle, BerkeleyDbWriteMode writeMode, [FromBody] Byte[] data)
        {
            DbHandle db = GetDb(handle);
            return db.Methods.PutMultipleKey(db.Handle, data, writeMode);
        }

        private static DbHandle GetDb(ulong handle)
        {
            DbHandle db = DbInstance.GetDb(handle);
            if (db.Handle == IntPtr.Zero)
                throw new HttpResponseException((HttpStatusCode)422);

            return db;
        }
    }
}
