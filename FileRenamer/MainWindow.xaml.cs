using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using F = System.IO;

namespace FileRenamer
{
    public class FileEntity
    {
        public FileInfo FileInfo { get; set; }
        public int ID { get; set; }
        public string OriginalName { get; set; }
        public string NewName { get; set; }
        public Exception Exception { get; set; }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string[] FileListStartupArgs { get; set; }

        private int _textChangeCount = 0;
        private List<FileEntity> _fileEntities = new List<FileEntity>();
        private DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private bool _isProcessing = false;
        private int _exceptionThrownCount = 0;
        private bool _firstTimeRan = false;

        // statics
        private static MainWindow _mainWindow;

        public MainWindow()
        {
            InitializeComponent();

            if (_mainWindow == null)
                _mainWindow = this;

            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1500);
        }


        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_isProcessing)
                return;

            if (_mainWindow._textChangeCount > 0)
            {
                _isProcessing = true;

                _mainWindow._textChangeCount = 0;

                _mainWindow.RenameForPreview();

                _isProcessing = false;
            }
        }

        private void FillListIn()
        {
            listBoxFilesIn.ItemsSource = _fileEntities.OrderBy(x => x.FileInfo.Name)
                .Select(x => x.OriginalName);
        }


        private void CheckBoxExtensionRename_Checked(object sender, RoutedEventArgs e)
        {
            textBoxExtension.IsEnabled = true;
        }

        private void CheckBoxExtensionRename_Unchecked(object sender, RoutedEventArgs e)
        {
            textBoxExtension.IsEnabled = false;
        }

        private void buttonApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_dispatcherTimer.IsEnabled)
                _dispatcherTimer.Stop();

            buttonApplyChanges.IsEnabled = false;

            List<FileEntity> fileEntities;

            if (sender is ListBoxItem)
            {
                fileEntities = new List<FileEntity>();
                fileEntities.Add((sender as ListBoxItem).DataContext as FileEntity);
                fileEntities[0].Exception = null;
            }
            else
            {
                listBoxFilesOut.ItemsSource = null;
                fileEntities = _fileEntities.OrderBy(x => x.FileInfo.Name).ToList();
            }


            foreach (var f in fileEntities)
            {
                bool isSame = false;


                // create Item item
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = f.NewName;


                try
                {
                    // skip if exactly the same
                    if (f.OriginalName == f.NewName)
                        isSame = true;

                    //check if exists
                    if (!f.FileInfo.Exists)
                        throw new IOException("Original file does not exist or has been moved.");


                    // Permanently rename files
                    if (!isSame)
                        f.FileInfo.MoveTo(f.FileInfo.DirectoryName + @"\" + f.NewName);

                    listBoxItem.Background = Brushes.LightGreen;



                    // finally add the item to out list.

                    if (_firstTimeRan)
                    {
                        // iterate until matching list item found then update it instead
                        foreach (var obj in listBoxFilesOut.Items)
                        {
                            var li = obj as ListBoxItem;
                            if ((li.DataContext as FileEntity).ID == f.ID)
                            {
                                li.Selected -= ErroredListItem_Click;
                                li.Background = Brushes.LightGreen;
                            }
                        }
                    }
                    else
                        listBoxFilesOut.Items.Add(listBoxItem);

                }
                catch (Exception ex)
                {
                    f.Exception = ex;

                    _exceptionThrownCount++;

                    listBoxItem.Background = Brushes.OrangeRed;
                    listBoxItem.DataContext = f;
                    listBoxItem.Selected += ErroredListItem_Click;


                    if (_firstTimeRan)
                    {
                        // iterate until matching list item found then update it instead
                        foreach (var obj in listBoxFilesOut.Items)
                        {
                            var li = obj as ListBoxItem;
                            if ((li.DataContext as FileEntity).ID == f.ID)
                            {
                                (li.DataContext as FileEntity).Exception = ex;
                            }
                        }
                    }
                    else
                        listBoxFilesOut.Items.Add(listBoxItem);
                }
            }

            // if exceptions were thrown, show dialog
            if (_exceptionThrownCount > 0)
                MessageBox.Show($"{_exceptionThrownCount} error(s) was encountered.  Click on the red item(s) for more details.", "Errors After Processing", MessageBoxButton.OK);

            _exceptionThrownCount = 0;

            _firstTimeRan = true;
        }


        // error buttons from out list.
        private void ErroredListItem_Click(object sender, RoutedEventArgs e)
        {
            var listObject = sender as ListBoxItem;
            var context = listObject.DataContext as FileEntity;




            MessageBoxResult mResult = MessageBox.Show(context.Exception.Message
                + "\n\rTry again?", "Processing Error", MessageBoxButton.YesNo);

            if (mResult == MessageBoxResult.Yes)
                buttonApplyChanges_Click(sender, null);

        }

        private void RenameForPreview()
        {
            // validate chars
            var invalidChars = F.Path.GetInvalidFileNameChars();



            var newExt = !string.IsNullOrEmpty(textBoxExtension.Text) ? '.' + textBoxExtension.Text : "";

            foreach (var f in _fileEntities)
            {
                var nameOnly = textBoxPrepend.Text + f.FileInfo.Name.TrimEnd(f.FileInfo.Extension.ToCharArray()) + textBoxAppend.Text;


                f.NewName = nameOnly + (checkBoxExtensionRename.IsChecked.Value ? newExt : f.FileInfo.Extension);
            }

            listBoxFilesOut.ItemsSource = _fileEntities.OrderBy(x => x.FileInfo.Name).Select(x => x.NewName);
        }


        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var path in FileListStartupArgs)
            {
                var fileInfo = new FileInfo(path);

                _fileEntities.Add(new FileEntity
                {
                    FileInfo = fileInfo,
                    OriginalName = fileInfo.Name,
                    ID = fileInfo.GetHashCode()
                });
            }

            FillListIn();

            RenameForPreview();
        }


        private void RecordPresses(object sender, TextChangedEventArgs e)
        {
            if (!_dispatcherTimer.IsEnabled)
                _dispatcherTimer.Start();

            _textChangeCount++;
        }
    }
}
