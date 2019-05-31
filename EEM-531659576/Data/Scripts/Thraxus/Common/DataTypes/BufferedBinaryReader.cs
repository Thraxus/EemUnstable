using System;
using System.IO;

namespace Eem.Thraxus.Common.DataTypes
{
	public class BufferedBinaryReader : IDisposable
	{
		private readonly Stream _stream;
		private readonly byte[] _buffer;
		private readonly int _bufferSize;
		private int _bufferOffset;
		private int _numBufferedBytes;

		public BufferedBinaryReader(Stream stream, int bufferSize)
		{
			_stream = stream;
			_bufferSize = bufferSize;
			_buffer = new byte[bufferSize];
			_bufferOffset = bufferSize;
		}

		public int NumBytesAvailable => Math.Max(0, _numBufferedBytes - _bufferOffset);

		public bool FillBuffer()
		{
			int numBytesUnread = _bufferSize - _bufferOffset;
			int numBytesToRead = _bufferSize - numBytesUnread;
			_bufferOffset = 0;
			_numBufferedBytes = numBytesUnread;
			if (numBytesUnread > 0)
			{
				Buffer.BlockCopy(_buffer, numBytesToRead, _buffer, 0, numBytesUnread);
			}
			while (numBytesToRead > 0)
			{
				int numBytesRead = _stream.Read(_buffer, numBytesUnread, numBytesToRead);
				if (numBytesRead == 0)
				{
					return false;
				}
				_numBufferedBytes += numBytesRead;
				numBytesToRead -= numBytesRead;
				numBytesUnread += numBytesRead;
			}
			return true;
		}

		public ushort ReadUInt16()
		{
			ushort val = (ushort)(_buffer[_bufferOffset] | _buffer[_bufferOffset + 1] << 8);
			_bufferOffset += 2;
			return val;
		}

		public void Dispose()
		{
			_stream.Close();
		}
	}
}
