using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

namespace Fenzwork.Services.MMF
{
    public class MessengerMMF : MMFHandler
    {
        public MessengerMMF(string path)
        {
            var splittedFileName = Path.GetFileName(path).Split('+');

            ShortName = splittedFileName[0];
            LongName = splittedFileName[1];
            SessionUniqueStartTime = new DateTime(long.Parse(splittedFileName[2]));

            FilePath = Path.Combine(path);
            FileCapacity = 4096;
            IsReader = true;
        }
        public MessengerMMF(string longName, string shortName, long sessionStartTick)
        {
            SessionUniqueStartTime = new DateTime(sessionStartTick);
            LongName = longName;
            ShortName = shortName;

            FilePath = Path.Combine(Path.GetTempPath(), $"FWDT_{DirUniqueHash}_Messengers", $"{shortName}+{longName}+{sessionStartTick}");
            FileCapacity = 4096;
            IsReader = false;
        }


        public DateTime SessionUniqueStartTime { get; private init; }
        public string LongName { get; private init; }
        public string ShortName { get; private init; }
        public bool IsReader { get; private init; }
        public long HeartbeatTime;
        public long Cursor;


        public ConcurrentQueue<(sbyte Type, string Content)> PendingMessages = new();


        public override void Init()
        {
            base.Init();

            Cursor = sizeof(long);

            if (IsReader)
            {
                HeartbeatTime = Accessor.ReadInt64(0);
            }
            else
            {
                Accessor.Write(0, HeartbeatTime);
                Accessor.Write(Cursor, (sbyte)0);
            }

        }

        string ReadString(long position)
        {
            int length = Accessor.ReadInt32(position); // Read string length
            position += sizeof(int);

            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = Accessor.ReadChar(position + i * sizeof(char));
            }

            return new string(chars);
        }

        void WriteString(long position, string value)
        {
            Accessor.Write(position, value.Length); // Write string length first
            position += sizeof(int);

            for (int i = 0; i < value.Length; i++)
            {
                Accessor.Write(position + i * sizeof(char), value[i]);
            }
        }

        public override void Tick()
        {

            if (IsReader)
                ReaderTick();
            else
                WriterTick();
        }

        void ReaderTick()
        {
            HeartbeatTime = Accessor.ReadInt64(0);

            var type = Accessor.ReadSByte(Cursor);

            if (type == 0)
                return;

            if (type == -1)
            {
                Cursor = sizeof(long);
                return;
            }

            var content = ReadString(Cursor + 1);

            PendingMessages.Enqueue((type, content));

            Cursor += sizeof(sbyte) + sizeof(int) + sizeof(char) * content.Length;
        }

        void WriterTick()
        {
            Accessor.Write(0, HeartbeatTime);

            if (PendingMessages.TryDequeue(out var msg))
            {
                if (Cursor + sizeof(sbyte) + sizeof(int) + sizeof(char) * msg.Content.Length + 1 >= FileCapacity)
                {
                    Accessor.Write(Cursor, (sbyte)-1);
                    Cursor = sizeof(long);
                }

                var cursorStart = Cursor;

                Cursor += sizeof(sbyte);

                WriteString(Cursor, msg.Content);

                Cursor += sizeof(int) + sizeof(char) * msg.Content.Length;

                Accessor.Write(Cursor, (sbyte)0);

                Accessor.Write(cursorStart, msg.Type);
            }
        }
    }
}
