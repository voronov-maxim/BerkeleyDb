using System;
using System.Threading.Tasks;

namespace BerkeleyDbClient
{
    public sealed class BerkeleyKeyValueCursor : BerkeleyCursor
    {
        public BerkeleyKeyValueCursor(BerkeleyDb berkeleyDb)
            : base(berkeleyDb, 0, 0)
        {
        }

        public async Task<BerkeleyError> DeleteAsync(Byte[] key, BerkeleyDbDelete flag = 0)
        {
            return await base.BerkeleyDb.DeleteAsync(key, flag, 0).ConfigureAwait(false);
        }
        public async Task<BerkeleyResult<BerkeleyKeyValue>> ReadAsync(Byte[] key, BerkeleyDbOperation operation)
        {
            BerkeleyError error = await base.OpenAsync().ConfigureAwait(false);
            if (error.HasError)
                return new BerkeleyResult<BerkeleyKeyValue>(error);

            BerkeleyResult<Dto.BerkeleyDtoGet> resultDtoGet = await base.Methods.GetDtoGet(this, key, operation, 0, 0).ConfigureAwait(false);
            if (resultDtoGet.HasError)
                return new BerkeleyResult<BerkeleyKeyValue>(resultDtoGet.Error);

            var keyValue = new BerkeleyKeyValue(resultDtoGet.Result.Key, resultDtoGet.Result.Value);
            return new BerkeleyResult<BerkeleyKeyValue>(keyValue);
        }
    }
}
