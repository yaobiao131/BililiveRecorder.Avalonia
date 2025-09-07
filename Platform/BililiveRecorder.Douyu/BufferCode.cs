using System.Buffers.Binary;

namespace BililiveRecorder.Douyu;

public class BufferCode
{
    private byte[] _buffer = [];
    private int _readLength;
    private bool _littleEndian = true;

    public List<string> Decode(byte[] newBuffer, bool? littleEndian = null)
    {
        var messages = new List<string>();
        var useLittleEndian = littleEndian ?? _littleEndian;
        _buffer = Concat(_buffer, newBuffer);
        while (_buffer.Length > 0)
        {
            if (_readLength == 0)
            {
                if (_buffer.Length < 4) break;

                // Read the length (4 bytes)
                _readLength = Convert.ToInt32(useLittleEndian
                    ? BinaryPrimitives.ReadUInt32LittleEndian(_buffer.AsSpan(0, 4))
                    : BinaryPrimitives.ReadUInt32BigEndian(_buffer.AsSpan(0, 4)));

                // Remove the length bytes from buffer
                _buffer = _buffer[4..];
            }

            if (_buffer.Length < _readLength) break;

            // Decode the message (skip first 8 bytes and go until readLength - 1)
            // Note: Original code slices from 8 to readLength-1, but we need to verify this logic
            const int messageStart = 8;
            var messageEnd = _readLength - 1;
            var messageLength = messageEnd - messageStart;

            if (messageLength > 0 && messageStart + messageLength <= _buffer.Length)
            {
                var message = System.Text.Encoding.UTF8.GetString(_buffer, messageStart, messageLength);

                // Remove processed bytes from buffer
                _buffer = _buffer[_readLength..];
                _readLength = 0;

                messages.Add(message);
            }
            else
            {
                // Handle error case where indices are invalid
                _buffer = [];
                _readLength = 0;
                break;
            }
        }

        return messages;
    }

    private static byte[] Concat(byte[] buffer1, byte[] buffer2)
    {
        var result = new byte[buffer1.Length + buffer2.Length];
        Buffer.BlockCopy(buffer1, 0, result, 0, buffer1.Length);
        Buffer.BlockCopy(buffer2, 0, result, buffer1.Length, buffer2.Length);
        return result;
    }
}
