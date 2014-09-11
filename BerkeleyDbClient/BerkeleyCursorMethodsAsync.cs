using System;
using System.Threading.Tasks;

namespace BerkeleyDbClient
{
    public abstract class BerkeleyCursorMethodsAsync
    {
        public abstract Task<BerkeleyError> CloseCursorAsync(BerkeleyCursor cursor);
        public abstract Task<BerkeleyError> DeleteCurrentAsync(BerkeleyCursor cursor, BerkeleyDbDelete flag);
        public abstract Task<BerkeleyResult<Dto.BerkeleyDtoGet>> GetDtoGet(BerkeleyCursor cursor, Byte[] key, BerkeleyDbOperation operation, BerkeleyDbMultiple multiple, int bufferSize);
        public abstract Task<BerkeleyResult<Byte[]>> ReadPartialAsync(BerkeleyCursor cursor, Byte[] key, int offset, int bufferSize);
        public abstract Task<BerkeleyError> WritePartialAsync(BerkeleyCursor cursor, Byte[] key, Byte[] value, int offset, int bufferSize);
    }
}
