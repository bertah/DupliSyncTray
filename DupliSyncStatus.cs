using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DupliSyncTray
{
    public enum DupliSyncStatus
    {
        OK = 0,
        DossierInatteignable = 1,
        UploadCopyFailed = 2,
        DownloadCopyFailed = 3
    }
}
