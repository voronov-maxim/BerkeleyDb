using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BerkeleyDbClient
{
    public sealed class BerkeleyBlobCursor : BerkeleyCursor
    {
        public BerkeleyBlobCursor(BerkeleyDb berkeleyDb, int bufferSize)
            : base(berkeleyDb, bufferSize, BerkeleyDbCursorFlags.DB_CURSOR_BULK)
        {
        }

        private static Byte[] Join(List<Byte[]> readData)
        {
            int lenght = 0;
            for (int i = 0; i < readData.Count; i++)
                lenght += readData[i].Length;

            var join = new Byte[lenght];
            int offset = 0;
            for (int i = 0; i < readData.Count; i++)
            {
                Byte[] data = readData[i];
                Buffer.BlockCopy(data, 0, join, offset, data.Length);
                offset += data.Length;
            }

            return join;
        }
        public async Task<BerkeleyResult<Byte[]>> ReadAsync(Byte[] key)
        {
            BerkeleyError error = await base.OpenAsync().ConfigureAwait(false);
            if (error.HasError)
                return new BerkeleyResult<Byte[]>(error);

            var readData = new List<Byte[]>();
            int offset = 0;
            for (; ; )
            {
                BerkeleyResult<Byte[]> result = await base.Methods.ReadPartialAsync(this, key, offset, base.BufferSize).ConfigureAwait(false);
                if (result.HasError)
                    return result;

                Byte[] value = result.Result;
                readData.Add(value);
                offset += value.Length;

                if (value.Length < base.BufferSize)
                    break;
            }

            return new BerkeleyResult<Byte[]>(Join(readData));
        }
        public async Task<BerkeleyError> WriteAsync(Byte[] key, Byte[] value)
        {
            BerkeleyError error = await base.OpenAsync().ConfigureAwait(false);
            if (error.HasError)
                return error;

            int offset = 0;
            do
            {
                int bufferSize = Math.Min(value.Length - offset, base.BufferSize);
                error = await base.Methods.WritePartialAsync(this, key, value, offset, bufferSize).ConfigureAwait(false);
                if (error.HasError)
                    return error;

                offset += bufferSize;
            }
            while (offset < value.Length);

            return BerkeleyError.NoError;
        }
    }
}
