using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
    internal abstract class FileBase : IFile
    {
        private bool _wasCheckedForMalware;

        public int DocumentArtifactId { get; set; }

        public string Location { get; set; }

        public string Filename { get; set; }

        public long Size { get; set; }

        public bool IsMalwareDetected { get; private set; }

        public async Task ValidateMalwareAsync(IAntiMalwareHandler malwareHandler)
        {
            if (_wasCheckedForMalware || string.IsNullOrEmpty(Location))
            {
                _wasCheckedForMalware = true;
                return;
            }

            IsMalwareDetected = await malwareHandler.ContainsMalwareAsync(this).ConfigureAwait(false);

            _wasCheckedForMalware = true;
        }
    }
}
