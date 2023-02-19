using System.IO;
using System.Reflection;
using Microsoft.Win32.SafeHandles;
using Relativity.DataExchange.Io;
using ZetaLongPaths;
using ZetaLongPaths.Native;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
    public class LongPathFileHelper : IFile
    {
        public FileStream Create(string filePath)
        {
            var fileInfo = new ZlpFileInfo(filePath);
            return fileInfo.OpenCreate();
        }

        public FileStream Create(string filePath, bool append)
        {
            CreationDisposition disposition = append
                ? CreationDisposition.OpenExisting
                : CreationDisposition.CreateAlways;
            ZetaLongPaths.Native.FileAccess fileAccess
                = ZetaLongPaths.Native.FileAccess.GenericRead | ZetaLongPaths.Native.FileAccess.GenericWrite;

            SafeFileHandle fileHandle = ZlpIOHelper.CreateFileHandle(
                filePath,
                disposition,
                fileAccess,
                ZetaLongPaths.Native.FileShare.None);

            FileStream fileStream = new FileStream(fileHandle, System.IO.FileAccess.ReadWrite);
            SetFileName(fileStream, filePath);

            if (append)
            {
                fileStream.Seek(0L, SeekOrigin.End);
            }

            return fileStream;
        }

        public string ReadAllText(string path)
        {
            return ZlpIOHelper.ReadAllText(path);
        }

        public FileStream ReopenAndTruncate(string filePath, long position)
        {
            CreationDisposition disposition = CreationDisposition.OpenExisting;
            ZetaLongPaths.Native.FileAccess fileAccess = ZetaLongPaths.Native.FileAccess.GenericRead | ZetaLongPaths.Native.FileAccess.GenericWrite;

            SafeFileHandle fileHandle = ZlpIOHelper.CreateFileHandle(filePath, disposition, fileAccess, ZetaLongPaths.Native.FileShare.None);
            FileStream fileStream = new FileStream(
                fileHandle,
                System.IO.FileAccess.ReadWrite);
            SetFileName(fileStream, filePath);

            fileStream.SetLength(position);
            fileStream.Seek(0L, SeekOrigin.End);

            return fileStream;
        }

        public void Delete(string filePath)
        {
            if (ZlpIOHelper.FileExists(filePath))
            {
                ZlpIOHelper.DeleteFile(filePath);
            }
        }

        public void Copy(string sourceFilePath, string destinationFilePath)
        {
            Copy(sourceFilePath, destinationFilePath, false);
        }

        public void Copy(string sourceFilePath, string destinationFilePath, bool overwrite)
        {
            ZlpIOHelper.CopyFile(sourceFilePath, destinationFilePath, overwrite);
        }

        public bool Exists(string filePath)
        {
            return ZlpIOHelper.FileExists(filePath);
        }

        public long GetFileSize(string filePath)
        {
            var fileInfo = new ZlpFileInfo(filePath);
            return fileInfo.Length;
        }

        public void Move(string sourceFilePath, string destinationFilePath)
        {
            ZlpIOHelper.MoveFile(sourceFilePath, destinationFilePath);
        }

        private void SetFileName(FileStream stream, string filePath)
        {
            FieldInfo nameField = stream.GetType().GetField("_fileName", BindingFlags.NonPublic | BindingFlags.Instance);
            nameField.SetValue(stream, filePath);
        }

        public int CountLinesInFile(string path)
        {
            // Required by interface but not used by this API.
            throw new System.NotImplementedException();
        }

        public FileStream Create(string path, FileMode mode, System.IO.FileAccess access, System.IO.FileShare share)
        {
            // Required by interface but not used by this API.
            throw new System.NotImplementedException();
        }

        public StreamWriter CreateText(string path)
        {
            // Required by interface but not used by this API.
            throw new System.NotImplementedException();
        }
    }
}
