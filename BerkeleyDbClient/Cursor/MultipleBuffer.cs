using System;

namespace BerkeleyDbClient
{
    internal struct MultipleBuffer
    {
        private Byte[] _buffer;
        private readonly int _bufferSize;
        private int _posLeft;
        private int _posRight;

        public MultipleBuffer(int bufferSize)
        {
            _bufferSize = bufferSize;

            _buffer = null;
            _posLeft = 0;
            _posRight = 0;
        }
        public MultipleBuffer(Byte[] buffer)
        {
            _buffer = buffer;

            _bufferSize = 0;
            _posLeft = 0;
            _posRight = buffer.Length;
        }

        public bool AddRecord(Byte[] data)
        {
            if (_buffer == null)
            {
                _buffer = new Byte[_bufferSize];
                _posLeft = 0;
                _posRight = _bufferSize;
            }

            if (_posLeft + data.Length + sizeof(uint) * 3 > _posRight)
                return false;

            _posRight -= sizeof(uint);
            WriteBuffer(_posLeft, _posRight);
            _posRight -= sizeof(uint);
            WriteBuffer(data.Length, _posRight);

            Buffer.BlockCopy(data, 0, _buffer, _posLeft, data.Length);
            _posLeft += data.Length;
            return true;
        }
        public Byte[] Close()
        {
            WriteBuffer(-1, _posRight - 4);

            Byte[] result = _buffer;
            _buffer = null;
            return result;
        }
        public int GetBufferSize(int length)
        {
            length += sizeof(uint) * 3;
            if (length <= BufferSize)
                return BufferSize;

            return ((length + 1023) / 1024) * 1024;
        }
        public bool GetNextRecord(out ArraySegment<Byte> record)
        {
            _posRight -= sizeof(uint);
            int offset = BitConverter.ToInt32(_buffer, _posRight);
            if (offset >= 0)
            {
                _posRight -= sizeof(uint);
                int size = BitConverter.ToInt32(_buffer, _posRight);
                record = new ArraySegment<Byte>(_buffer, offset, size);
                return true;
            }

            record = new ArraySegment<Byte>();
            return false;
        }
        public void RemoveRecord(int recordLength)
        {
            _posLeft -= recordLength;
            _posRight += 2 * sizeof(uint);
        }
        private void WriteBuffer(int value, int offset)
        {
            _buffer[offset + 0] = (Byte)value;
            _buffer[offset + 1] = (Byte)(value >> 8);
            _buffer[offset + 2] = (Byte)(value >> 16);
            _buffer[offset + 3] = (Byte)(value >> 24);
        }

        public int BufferSize
        {
            get
            {
                return _bufferSize;
            }
        }
        public int Position
        {
            get
            {
                return _posLeft;
            }
        }
    }

}
