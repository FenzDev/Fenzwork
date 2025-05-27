using Fenzwork.Services.MMF;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Fenzwork.DebuggingTools
{
    public static class MessagingManager
    {
        static string DirUniqueHash = PathHasher.HashFilename(AppDomain.CurrentDomain.BaseDirectory, 16);
        static FileSystemWatcher MessengerFilesWatcher;
        static int Counter;
        static DateTime StartTime;
        static DateTime PreviousUpdatedTime;
        static readonly ConsoleColor SystemPrintColor = ConsoleColor.DarkMagenta;
        static readonly int TickDelayMiliseconds = 5;
        static readonly long NotRespondingTicksToKill = 200;
        static object ConsoleLogLock = new ();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        static Thread InputThread;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        static volatile bool ExitLoop;

        public static ConcurrentDictionary<string, MessengerMMF> Messengers = new ();

        public static void Start(string[] args)
        {
            BriefTitle(false);

            StartTime = new (long.Parse(args[0]));

            InputThread = new(InputLoop)
            {
                IsBackground = true
            };
            InputThread.Start();

            //Thread.Sleep(10000);

            var now = DateTime.Now;
            if (now.Date != PreviousUpdatedTime.Date)
                BriefDate();

            BriefIntroToHelp();

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

        static void MessengerFilesWatcher_Created(object sender, FileSystemEventArgs e)
        {
            RegisterMessenger(e.FullPath);
        }

        static void InputLoop()
        {
            var key = Console.ReadKey(true);

            while (key.Modifiers != ConsoleModifiers.Control || key.Key != ConsoleKey.C)
            {
                if (key.Modifiers == ConsoleModifiers.Shift && key.Key == ConsoleKey.C)
                    Clear();
                else if (key.Modifiers == ConsoleModifiers.None && key.Key == ConsoleKey.H)
                    BriefHelp();

                key = Console.ReadKey(true);
            }
        }

        static void TickLoop()
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

        static void Clear()
        {
            lock (ConsoleLogLock)
            {
                Console.Clear();
                
                BriefDate(false);
                
                BriefIntroToHelp(false);

                // TODO : Maybe Brief connections
            }
        }

        static void BriefTitle(bool doLock = true)
        {
            Print(SystemPrintColor, "----------------------------------", doLock);
            Print(SystemPrintColor, " ~  FENZWORK . DEBUGGING TOOLS  ~ ", doLock);
            Print(SystemPrintColor, "----------------------------------", doLock);
        }

        static void BriefIntroToHelp(bool doLock = true)
        {
            Print(SystemPrintColor, "  Press h to get help.", doLock);
        }

        static void BriefHelp(bool doLock = true)
        {
            Print(SystemPrintColor,
                "  Help\n    H         -> to display this message.\n    Shift + C -> to clear the log.\n    Ctrl + C  -> to exit from the program.",
                doLock);
        }

        static void BriefDate(bool doLock = true)
        {
            Print(SystemPrintColor, $"[ {DateTime.Now:f} ]", doLock);
        }

        static bool MessengerResponseUpdate(MessengerMMF messenger)
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

        static void Print(ConsoleColor color, string message, bool doLock = true)
        {
            if (doLock)
            {
                lock (ConsoleLogLock)
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    Console.WriteLine(message);
                    Console.ForegroundColor = oldColor;
                }
            }
            else
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

        static void RegisterMessenger(string path)
        {
            var filename = Path.GetFileName(path).Split('+');
            var id = $"{filename[0]}{Counter++}";

            var mmf = new MessengerMMF(StartTime, path);

            mmf.Init();

            Print(SystemPrintColor, $"  - {id} ({filename[1]}) was connected");

            Messengers.TryAdd(id, mmf);
        }

        static void UnregisterMessenger(string id)
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

            Print(SystemPrintColor, $"  - {id} ({longName}) was disconnected");
        }
    }

}