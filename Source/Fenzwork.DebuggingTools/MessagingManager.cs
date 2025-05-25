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

        public static ConcurrentDictionary<string, MessengerMMF> Messengers = new ();

        public static void Start()
        {
            Console.WriteLine("Starting");
            var dirPath = Path.Combine(Path.GetTempPath(), $"FWDT_{DirUniqueHash}_Messengers");

            var dirInfo = Directory.CreateDirectory(dirPath);
                
            var messengerFiles = dirInfo.GetFiles();
            
            foreach (var messengerFile in messengerFiles)
            {

                Console.WriteLine(" Messenger found");
                RegisterMessenger(messengerFile.FullName);
            }

            MessengerFilesWatcher = new FileSystemWatcher(dirInfo.FullName)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
            };

            MessengerFilesWatcher.Created += MessengerFilesWatcher_Created;

            Console.WriteLine(" Done setting up");

            TickLoop();
        }

        private static void MessengerFilesWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(" Messenger found");
            RegisterMessenger(e.FullPath);
        }

        public static void TickLoop()
        {
            while (!ExitLoop)
            {
                foreach (var messenger in Messengers)
                {
                    //if (messenger.Key.EndsWith("1")) Debugger.Break();

                    messenger.Value.Tick();
                    
                    while (messenger.Value.PendingMessages.TryDequeue(out var msg))
                    {
                        Print(messenger.Key, msg.Type, msg.Content);
                    }
                }

                Thread.Sleep(15);
            }
        }

        static void Print(string source, sbyte type, string content)
        {
            switch (type)
            {
                case 1:
                    PrintTyped(ConsoleColor.Red, source, "error", content);
                    break;
                case 2:
                    PrintTyped(ConsoleColor.Yellow, source, "warn", content);
                    break;
                case 3:
                    PrintTyped(ConsoleColor.White, source, "info", content);
                    break;
                case 4:
                    PrintTyped(ConsoleColor.DarkGray, source, "dbg", content);
                    break;
            }
        }
        public static void PrintTyped(ConsoleColor color, string source, string header, string msg)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss:ff}][{source}/{header}]{msg}");
            Console.ForegroundColor = old;
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
            var id = $"{Path.GetFileName(path).Split('+')[0]}{Counter++}";

            var mmf = new MessengerMMF(path);

            mmf.Init();

            Messengers.TryAdd(id, mmf);
        }


        //public static void UnregisterMessenger(string id)
    }

}