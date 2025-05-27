using Fenzwork.Services.MMF;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Fenzwork.DebuggingTools
{
    public static class MessagingManager
    {
        public static string DirUniqueHash = PathHasher.HashFilename(AppDomain.CurrentDomain.BaseDirectory, 16);
        static FileSystemWatcher MessengerFilesWatcher;
        static int Counter;
        static bool ExitLoop;
        static DateTime StartTime;
        static DateTime PreviousUpdatedTime;
        static readonly ConsoleColor SystemPrintColor = ConsoleColor.DarkMagenta;
        static readonly int TickDelayMiliseconds = 5;
        static readonly long NotRespondingTicksToKill = 200;
        static object ConsoleLogLock = new ();


        public static ConcurrentDictionary<string, MessengerMMF> Messengers = new ();

        public static void Start(string[] args)
        {
            StartTime = new (long.Parse(args[0]));

            //Thread.Sleep(10000);

            var now = DateTime.Now;
            if (now.Date != PreviousUpdatedTime.Date)
                BriefDate();

            PreviousUpdatedTime = now;

            var dirPath = Path.Combine(Path.GetTempPath(), $"FWDT_{DirUniqueHash}_Messengers");

            var dirInfo = Directory.CreateDirectory(dirPath);
                
            var messengerFiles = dirInfo.GetFiles();
            
            foreach (var messengerFile in messengerFiles)
            {
                RegisterMessenger(messengerFile.FullName);
            }

            MessengerFilesWatcher = new FileSystemWatcher(dirInfo.FullName)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
            };

            MessengerFilesWatcher.Created += MessengerFilesWatcher_Created;

            TickLoop();
        }


        private static void MessengerFilesWatcher_Created(object sender, FileSystemEventArgs e)
        {
            RegisterMessenger(e.FullPath);
        }

        private static void BriefDate()
        {
            Print(SystemPrintColor, $"[ - {DateTime.Now:f} - ]");
        }

        public static void TickLoop()
        {
            while (!ExitLoop)
            {
                var now = DateTime.Now;
                if (now.Date != PreviousUpdatedTime.Date)
                    BriefDate();
                PreviousUpdatedTime = now;

                foreach (var messenger in Messengers)
                {
                    messenger.Value.Tick();

                    if (!MessengerResponseUpdate(messenger.Value))
                    {
                        UnregisterMessenger(messenger.Key);
                        continue;
                    }

                    while (messenger.Value.PendingMessages.TryDequeue(out var msg))
                    {
                        Print(messenger.Key, msg.Type, msg.Date, msg.Content);
                    }
                }

                Thread.Sleep(TickDelayMiliseconds);
            }
        }

        public static bool MessengerResponseUpdate(MessengerMMF messenger)
        {
            if (messenger.HeartbeatTime == -1)
                return false;

            if (messenger.OldTime == messenger.HeartbeatTime)
                messenger.TicksNotResponding += 1;
            else
                messenger.TicksNotResponding = 0;

            if (messenger.TicksNotResponding >= NotRespondingTicksToKill)
                return false;

            return true;
        }

        private static void Print(ConsoleColor color, string message)
        {
            lock (ConsoleLogLock)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ForegroundColor = oldColor;
            }
        }

        static void Print(ConsoleColor color, DateTime dateTime, string source, string header, string msg)
        {
            Print(color, $"[{dateTime:HH:mm:ss.ff}][{source}/{header}]{msg}");
        }

        static void Print(string source, sbyte type, DateTime dateTime, string content)
        {
            switch (type)
            {
                case 1:
                    Print(ConsoleColor.Red, dateTime, source, "error", content);
                    break;
                case 2:
                    Print(ConsoleColor.Yellow, dateTime, source, "warn", content);
                    break;
                case 3:
                    Print(ConsoleColor.White, dateTime, source, "info", content);
                    break;
                case 4:
                    Print(ConsoleColor.DarkGray, dateTime, source, "dbg", content);
                    break;
            }
        }
        
        public static void Dispose()
        {
            foreach (var messenger in Messengers)
            {
                messenger.Value.Dispose();
            }
        }

        public static void RegisterMessenger(string path)
        {
            var filename = Path.GetFileName(path).Split('+');
            var id = $"{filename[0]}{Counter++}";

            var mmf = new MessengerMMF(StartTime, path);

            mmf.Init();

            Print(SystemPrintColor, $"[ {id} ({filename[1]}) was connected ]");

            Messengers.TryAdd(id, mmf);
        }

        public static void UnregisterMessenger(string id)
        {
            Messengers.Remove(id, out var mmf);

            if (mmf == null)
                return;

            var file = mmf.FilePath;
            var longName = mmf.LongName;

            mmf.Dispose();

            try
            {
                File.Delete(file);
            }
            catch { }

            Print(SystemPrintColor, $"[ {id} ({longName}) was disconnected ]");
        }
    }

}