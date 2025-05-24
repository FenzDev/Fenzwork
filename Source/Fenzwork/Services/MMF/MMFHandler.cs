using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Fenzwork.Services.MMF
{
    public abstract class MMFHandler : IDisposable
    {
        protected MemoryMappedFile MMF;
        protected MemoryMappedViewAccessor SignalView;
        protected MemoryMappedViewStream DataView;
        public virtual string FilePath { get; protected set; }
        public virtual long FileCapacity { get; protected set; }

        protected static string DirUniqueHash => PathHasher.HashFilename(AppDomain.CurrentDomain.BaseDirectory, 16);

        public virtual void Init()
        {
            if (FileCapacity <= 0)
                MMF = MemoryMappedFile.CreateFromFile(FilePath, FileMode.Open);
            else
                MMF = MemoryMappedFile.CreateFromFile(FilePath, FileMode.OpenOrCreate, null, FileCapacity);
        }
        public abstract void Tick();
        public virtual void Dispose()
        {
            if (SignalView!=null) SignalView.Dispose();
            if (DataView!=null) DataView.Dispose();
            if (MMF!=null) MMF.Dispose();
        }

    }
}
