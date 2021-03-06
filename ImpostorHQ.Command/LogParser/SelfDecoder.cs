﻿using System;
using System.IO;
using System.Collections.Generic;

namespace Impostor.Commands.Core.SELF
{

    public class SelfDecoder:IDisposable
    {
        public MemoryStream IOStream { get; private set; }
        public SelfDecoder(byte[] selfBytes)
        {
            IOStream = new MemoryStream(selfBytes);
        }

        public BinaryLog ReadLog()
        {
            var sizeBytes = new byte[2];
            IOStream.Read(sizeBytes, 0, 2);
            var size = BitConverter.ToUInt16(sizeBytes, 0);
            var data = new byte[size];
            IOStream.Read(data, 0, size);
            return BinaryLog.Deserialize(new MemoryStream(data), size);
        }

        public IEnumerable<BinaryLog> ReadAll()
        {
            while (IOStream.Position != IOStream.Length) yield return ReadLog();
        }
        public class BinaryLog
        {
            public ushort BaseLength { get; set; }
            public Shared.LogType Type { get; set; }
            public DateTime TimeStamp { get; set; }
            public byte[] LogData { get; set; }
            public static BinaryLog Deserialize(MemoryStream stream, ushort baseLength)
            {
                var type = stream.ReadByte();
                var buffer = new byte[8];
                stream.Read(buffer, 0, 8);
                var epoch = BitConverter.ToUInt64(buffer, 0);
                buffer = new byte[baseLength - 9];
                stream.Read(buffer, 0, buffer.Length);
                return new BinaryLog
                {
                    BaseLength = baseLength,
                    Type = (Shared.LogType)type,
                    TimeStamp = GetTime(epoch),
                    LogData = buffer
                };
            }

            private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            private static DateTime GetTime(ulong unixTime)
            {
                return Epoch.AddMilliseconds(unixTime);
            }
        }

        public void Dispose()
        {
            IOStream.Dispose();
        }
    }
}
