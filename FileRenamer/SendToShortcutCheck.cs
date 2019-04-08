using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IWshRuntimeLibrary;

namespace FileRenamer
{
    public class SendToShortcutCheck
    {
        private string _sendToDir;
        private string _thisAppPath;
        private string _shortcutName;

        public SendToShortcutCheck()
        {
            _sendToDir = Environment.GetFolderPath(Environment.SpecialFolder.SendTo, Environment.SpecialFolderOption.None);
            _thisAppPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            _shortcutName = @"\Batch File Renamer.lnk";
        }


        public bool CreateOrUpdateLink()
        {
            bool success = false;

            WshShell shell = new WshShell();

            string shortcutPath = _sendToDir + _shortcutName;

            try
            {
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.Description = "Shortcut for a Batch File Renamer";

                if (string.IsNullOrEmpty(shortcut.TargetPath)
                    || shortcut.TargetPath != _thisAppPath)
                {
                    shortcut.TargetPath = _thisAppPath;
                    shortcut.Save();
                }

                success = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }



            return success;
        }

    }
}
