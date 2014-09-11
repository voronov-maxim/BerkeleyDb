using System;
using System.Threading.Tasks;

namespace BerkeleyDbClient
{
    public abstract class BerkeleyBulkCursorBase : BerkeleyCursor
    {
        private readonly BerkeleyDbMultiple _multiple;

        protected BerkeleyBulkCursorBase(BerkeleyDb berkeleyDb, int bufferSize, BerkeleyDbMultiple multiple)
            : base(berkeleyDb, bufferSize, BerkeleyDbCursorFlags.DB_CURSOR_BULK)
        {
            _multiple = multiple;
        }

        internal abstract bool GetNext(out BerkeleyKeyValueBulk keyValue);
        public async Task<BerkeleyBulkEnumerator> ReadAsync(Byte[] key, BerkeleyDbOperation operation)
        {
            BerkeleyError error = await SetBuffer(key, operation).ConfigureAwait(false);
            return error.HasError ? new BerkeleyBulkEnumerator(error) : new BerkeleyBulkEnumerator(this);
        }
        protected async Task<BerkeleyError> SetBuffer(Byte[] key, BerkeleyDbOperation operation)
        {
            BerkeleyError error = await base.OpenAsync().ConfigureAwait(false);
            if (error.HasError)
                return error;

            BerkeleyResult<Dto.BerkeleyDtoGet> resultDtoGet = await base.Methods.GetDtoGet(this, key, operation, _multiple, base.BufferSize).ConfigureAwait(false);
            if (!resultDtoGet.HasError)
            {
                Dto.BerkeleyDtoGet dtoGet = resultDtoGet.Result;
                SetDtoGet(ref dtoGet);
            }

            return resultDtoGet.Error;
        }
        protected abstract void SetDtoGet(ref Dto.BerkeleyDtoGet dataGet);
    }
}
