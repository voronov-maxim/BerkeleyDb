using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BerkeleyDbClient
{
    public sealed class BerkeleyBulkDuplicateCursor : BerkeleyBulkCursorBase
    {
        private MultipleBuffer _deleteBuffer;
        private readonly List<Byte[]> _deleteData;
        private Byte[] _readKey;
        private MultipleBuffer _readValueBuffer;
        private readonly List<Dto.BerkeleyDtoPut> _writeData;
        private MultipleBuffer _writeKeyBuffer;
        private MultipleBuffer _writeValueBuffer;

        public BerkeleyBulkDuplicateCursor(BerkeleyDb berkeleyDb, int bufferSize) :
            base(berkeleyDb, bufferSize, BerkeleyDbMultiple.DB_MULTIPLE)
        {
            _deleteBuffer = new MultipleBuffer(bufferSize);
            _deleteData = new List<Byte[]>();
            _writeData = new List<Dto.BerkeleyDtoPut>();
            _writeKeyBuffer = new MultipleBuffer(bufferSize);
            _writeValueBuffer = new MultipleBuffer(bufferSize);
        }

        public void AddDelete(Byte[] key, Byte[] value)
        {
            if (_deleteBuffer.AddRecord(key))
            {
                if (_deleteBuffer.AddRecord(value))
                    return;

                _deleteBuffer.RemoveRecord(key.Length);
            }

            if (_deleteBuffer.Position == 0)
            {
                int bufferSize = _deleteBuffer.GetBufferSize(key.Length + value.Length);
                var buffer = new MultipleBuffer(bufferSize);
                buffer.AddRecord(key);
                buffer.AddRecord(value);
                _deleteData.Add(buffer.Close());
            }
            else
            {
                CloseDeleteBuffer();
                AddDelete(key, value);
            }
        }
        public void AddWrite(Byte[] key, Byte[] value)
        {
            if (_writeKeyBuffer.AddRecord(key))
            {
                if (_writeValueBuffer.AddRecord(value))
                    return;

                _writeKeyBuffer.RemoveRecord(key.Length);
            }

            if (_writeValueBuffer.Position == 0)
                _writeData.Add(new Dto.BerkeleyDtoPut(key, value));
            else
            {
                CloseWriteBuffer();
                AddWrite(key, value);
            }
        }
        public void AddWrite(Byte[] key, IEnumerable<Byte[]> values)
        {
            foreach (Byte[] value in values)
                AddWrite(key, value);
        }
        private void CloseDeleteBuffer()
        {
            if (_deleteBuffer.Position > 0)
                _deleteData.Add(_deleteBuffer.Close());
        }
        private void CloseWriteBuffer()
        {
            if (_writeValueBuffer.Position > 0)
            {
                Byte[] keyBuffer = _writeKeyBuffer.Close();
                Byte[] valueBuffer = _writeValueBuffer.Close();
                _writeData.Add(new Dto.BerkeleyDtoPut(keyBuffer, valueBuffer));
            }
        }
        public async Task<BerkeleyError> DeleteAsync()
        {
            CloseDeleteBuffer();

            foreach (Byte[] data in _deleteData)
            {
                BerkeleyError error = await base.BerkeleyDb.DeleteAsync(data, 0, BerkeleyDbMultiple.DB_MULTIPLE_KEY).ConfigureAwait(false);
                if (error.HasError)
                    return error;
            }

            _deleteData.Clear();
            return BerkeleyError.NoError;
        }
        internal override bool GetNext(out BerkeleyKeyValueBulk keyValue)
        {
            ArraySegment<Byte> value;
            if (_readValueBuffer.GetNextRecord(out value))
            {
                keyValue = new BerkeleyKeyValueBulk(_readKey, value);
                return true;
            }
            else
            {
                keyValue = new BerkeleyKeyValueBulk();
                return false;
            }
        }
        protected override void SetDtoGet(ref Dto.BerkeleyDtoGet dataGet)
        {
            _readKey = dataGet.Key;
            _readValueBuffer = new MultipleBuffer(dataGet.Value);
        }
        public async Task<BerkeleyError> WriteAsync(BerkeleyDbWriteMode writeMode)
        {
            CloseWriteBuffer();
            foreach (Dto.BerkeleyDtoPut data in _writeData)
            {
                BerkeleyError error = await base.BerkeleyDb.Methods.WriteDuplicateAsync(base.BerkeleyDb, data, _writeKeyBuffer.BufferSize, writeMode).ConfigureAwait(false);
                if (error.HasError)
                    return error;
            }
            _writeData.Clear();
            return BerkeleyError.NoError;
        }
    }
}
