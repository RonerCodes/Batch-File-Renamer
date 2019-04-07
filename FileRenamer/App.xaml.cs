using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FileRenamer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        private void AppStartup(object sender, StartupEventArgs e)
        {
            MainWindow main = new MainWindow();

            if(e.Args.Length > 0)
            {
                main.FileListStartupArgs = e.Args;
            }
            else
            {
                main.FileListStartupArgs = new string[0];
            }

            main.Show();
        }
    }
}
