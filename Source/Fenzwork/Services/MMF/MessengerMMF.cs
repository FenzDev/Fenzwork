using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Services.MMF
{
    public class MessengerMMF : MMFHandler
    {
        public MessengerMMF(bool isReader, string path)
        {
            var splittedFileName = Path.GetFileName(path).Split('+');

            ShortName = splittedFileName[0];
            LongName = splittedFileName[1];
            SessionUniqueStartTime = new DateTime(long.Parse(splittedFileName[2]));

            FilePath = Path.Combine(path);
            FileCapacity = 4096;
            IsReader = isReader;
        }
        public MessengerMMF(bool isReader, string longName, string shortName, long sessionStartTick)
        {
            SessionUniqueStartTime = new DateTime(sessionStartTick);
            LongName = longName;
            ShortName = shortName;

            FilePath = Path.Combine(Path.GetTempFileName(), $"FWD_{DirUniqueHash}_Messengers", $"{shortName}+{longName}+{longName}");
            FileCapacity = 4096;
            IsReader = isReader;
        }


        public DateTime SessionUniqueStartTime { get; private init; }
        public string LongName { get; private init; }
        public string ShortName { get; private init; }
        public bool IsReader { get; private init; }


        public ConcurrentQueue<(byte Type, string Message)> PendingMessages;

        public override void Init()
        {
            base.Init();

            SignalView.Write(0, DateTime.Now.Ticks);

            if (IsReader)
            {
                // TODO: Reading
            }
            else
            {
                // -2 means this is the end of messages
                SignalView.Write(8, -2);
            }

            DataView.Position = 8;
        }

        public override void Tick()
        {
            if (IsReader)
            {

            }
            else
            {

            }
        }
    }
}
