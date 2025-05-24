using Fenzwork.Services.MMF;
using System.Collections.Concurrent;

namespace Fenzwork.DebuggingTools
{
    public static class MessagingManager
    {
        public static string DirUniqueHash = PathHasher.HashFilename(AppDomain.CurrentDomain.BaseDirectory, 16);
        static FileSystemWatcher MessengerFilesWatcher;
        static bool ExitLoop;


        public static ConcurrentDictionary<string, MessengerMMF> Messengers;

        public static void Start()
        {
            var dirPath = $"FWD_{DirUniqueHash}_Messengers";

            var messengerFiles = Directory.CreateDirectory(dirPath).GetFiles();
            
            foreach (var messengerFile in messengerFiles)
            {
                RegisterMessenger(messengerFile.FullName);
            }

            MessengerFilesWatcher = new FileSystemWatcher(Path.Combine(Path.GetTempPath(), DirUniqueHash));

            MessengerFilesWatcher.Created += MessengerFilesWatcher_Created;
        }

        private static void MessengerFilesWatcher_Created(object sender, FileSystemEventArgs e)
        {
            RegisterMessenger(e.FullPath);
        }

        public static void TickLoop()
        {
            while (!ExitLoop)
            {
                foreach (var messenger in Messengers)
                {
                    messenger.Value.Tick();
                }

                Thread.Sleep(15);
            }
        }


        public static void RegisterMessenger(string path)
        {
            var shortName = Path.GetFileName(path).Split('+')[0];

            var mmf = new MessengerMMF(path);

            Messengers.TryAdd(shortName, mmf);

            mmf.Init();

        }


        //public static void UnregisterMessenger(string id)
    }

}