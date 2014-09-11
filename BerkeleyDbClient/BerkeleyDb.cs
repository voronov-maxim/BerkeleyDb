using System;
using System.Threading.Tasks;

namespace BerkeleyDbClient
{
    public sealed class BerkeleyDb : IDisposable
    {
        private const long InvalidHandle = 0;

        private readonly BerkeleyDbMethodsAsync _methods;
        private readonly BerkeleyDbType _dbType;
        private readonly BerkeleyDbFlags _flags;
        private long _handle;
        private int _pageSize;

        public BerkeleyDb(BerkeleyDbMethodsAsync methods, BerkeleyDbType dbType, BerkeleyDbFlags flags)
        {
            _methods = methods;
            _dbType = dbType;
            _flags = flags;
        }

        public async Task<BerkeleyError> CloseAsync(BerkeleyDbClose flags)
        {
            if (_handle == InvalidHandle)
                return BerkeleyError.NoError;

            BerkeleyError error = await _methods.CloseDbAsync(this, flags).ConfigureAwait(false);
            if (!error.HasError)
                _handle = InvalidHandle;
            return error;
        }
        public void Dispose()
        {
            CloseAsync(0).Wait();
        }
        public async Task<BerkeleyError> DeleteAsync(Byte[] key, BerkeleyDbDelete flag, BerkeleyDbMultiple multiple)
        {
            return await _methods.DeleteAsync(this, key, flag, multiple).ConfigureAwait(false);
        }
        public async Task<BerkeleyResult<int>> GetPageSizeAsync()
        {
            if (_pageSize == 0)
            {
                BerkeleyResult<int> result = await _methods.GetPageSizeAsync(this);
                if (result.Error.HasError)
                    return result;

                _pageSize = result.Result;
            }

            return new BerkeleyResult<int>(_pageSize);
        }
        public async Task<BerkeleyError> OpenAsync(String name, BerkeleyDbOpenFlags flags)
        {
            BerkeleyResult<long> result = await _methods.CreateDb(this, _flags).ConfigureAwait(false);
            if (result.HasError)
                return result.Error;

            _handle = result.Result;
            return await _methods.OpenDbAsync(this, name, flags).ConfigureAwait(false);
        }
        public async Task<BerkeleyError> WriteAsync(Byte[] key, Byte[] value, BerkeleyDbWriteMode writeMode)
        {
            return await _methods.WriteAsync(this, key, value, writeMode).ConfigureAwait(false);
        }

        public BerkeleyDbType DbType
        {
            get
            {
                return _dbType;
            }
        }
        public long Handle
        {
            get
            {
                return _handle;
            }
        }
        public BerkeleyDbMethodsAsync Methods
        {
            get
            {
                return _methods;
            }
        }
    }
}
