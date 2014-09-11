using BerkeleyDbClient;
using BerkeleyDbClient.Dto;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Web.Http;

namespace BerkeleyDbWebApiServer.Controllers
{
    public sealed class CursorController : ApiController
    {
        [HttpGet]
        public BerkeleyDbError Close([FromUri] ulong handle)
        {
            DbcHandle dbc = DbcInstance.RemoveDbc(handle);
            if (dbc.Handle == IntPtr.Zero)
                throw new HttpResponseException((HttpStatusCode)422);

            return dbc.Methods.Close(dbc.Handle);
        }
        [HttpGet]
        public BerkeleyDbError DeleteCurrent([FromUri] ulong handle, [FromUri] BerkeleyDbDelete flag)
        {
            DbcHandle dbc = GetDbc(handle);
            return dbc.Methods.Del(dbc.Handle, flag);
        }
        [HttpPost]
        public JToken Read([FromUri] ulong handle, [FromUri] BerkeleyDbOperation operation, [FromUri] BerkeleyDbMultiple multiple, [FromUri] int size, [FromBody] Byte[] key)
        {
            DbcHandle dbc = GetDbc(handle);

            if (key == null)
                key = new Byte[size];
            Byte[] value = new Byte[size];
            int keySize, valueSize;

            BerkeleyDbError error = dbc.Methods.Get(dbc.Handle, key, value, operation, multiple, out keySize, out valueSize);
            if (error == BerkeleyDbError.DB_BUFFER_SMALL)
            {
                if (key.Length < keySize)
                    key = new Byte[keySize];
                if (value.Length < valueSize)
                    value = new Byte[valueSize];

                error = dbc.Methods.Get(dbc.Handle, key, value, operation, multiple, out keySize, out valueSize);
            }

            return ControllersHelper.CreateJTokenObject(error, key, value, keySize, valueSize);
        }
        [HttpPost]
        public JToken ReadPartial([FromUri] ulong handle, [FromUri] int valueOffset, [FromUri] int size, [FromBody] Byte[] key)
        {
            DbcHandle dbc = GetDbc(handle);

            Byte[] value = new Byte[size];
            int valueSize;
            var error = dbc.Methods.GetPartial(dbc.Handle, key, value, valueOffset, out valueSize);
            return ControllersHelper.CreateJTokenObject(error, null, value, 0, valueSize);
        }
        [HttpPost]
        public BerkeleyDbError WritePartial([FromUri] ulong handle, BerkeleyDtoPartialPut data)
        {
            DbcHandle dbc = GetDbc(handle);
            return dbc.Methods.PutPartial(dbc.Handle, data.Key, data.Value.Data, data.Value.Offset, data.Value.Length);
        }

        private static DbcHandle GetDbc(ulong handle)
        {
            DbcHandle dbc = DbcInstance.GetDbc(handle);
            if (dbc.Handle == IntPtr.Zero)
                throw new HttpResponseException((HttpStatusCode)422);

            return dbc;
        }
    }
}
