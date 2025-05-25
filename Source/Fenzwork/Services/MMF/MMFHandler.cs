using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Fenzwork.Services.MMF
{
    public abstract class MMFHandler : IDisposable
    {
        protected MemoryMappedFile MMF;
        protected MemoryMappedViewAccessor Accessor;
        public virtual string FilePath { get; protected set; }
        public virtual long FileCapacity { get; protected set; }

        protected static string DirUniqueHash => PathHasher.HashFilename(AppDomain.CurrentDomain.BaseDirectory, 16);

        public virtual void Init()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

            if (FileCapacity <= 0)
                MMF = MemoryMappedFile.CreateFromFile(FilePath, FileMode.Open);
            else
            {
                var stream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                MMF = MemoryMappedFile.CreateFromFile(stream, null, FileCapacity, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);

            }


            Accessor = MMF.CreateViewAccessor();
        }
        public abstract void Tick();
        public virtual void Dispose()
        {
            if (Accessor!=null) Accessor.Dispose();
            if (MMF!=null) MMF.Dispose();
        }

    }
}
