using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Model;
using WpfApp1.View;

namespace WpfApp1.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<FileSystemItem> _items;
        private ObservableCollection<FileSystemItem> _selectedFiles;

        public ObservableCollection<FileSystemItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public ObservableCollection<FileSystemItem> SelectedFiles
        {
            get => _selectedFiles;
            private set
            {
                _selectedFiles = value;
                OnPropertyChanged(nameof(SelectedFiles));
            }
        }

        public MainViewModel()
        {
            SelectedFiles = new ObservableCollection<FileSystemItem>();
            LoadFileSystem(@"D:\Contents\競馬");
        }

        private void LoadFileSystem(string rootPath)
        {
            if (Directory.Exists(rootPath))
            {
                Items = new ObservableCollection<FileSystemItem>();
                var root = new DirectoryInfo(rootPath);
                Items.Add(CreateFileSystemItem(root));
            }
        }

        private FileSystemItem CreateFileSystemItem(DirectoryInfo directoryInfo, FileSystemItem parent = null)
        {
            var item = new FileSystemItem
            {
                Name = directoryInfo.Name,
                FullPath = directoryInfo.FullName,
                IsDirectory = true,
                Parent = parent // 親ノードを設定
            };

            // フォルダのサブフォルダを追加
            foreach (var dir in directoryInfo.GetDirectories())
            {
                item.Children.Add(CreateFileSystemItem(dir, item)); // 子ノードに親を設定
            }

            // フォルダ内のファイルを追加
            foreach (var file in directoryInfo.GetFiles())
            {
                var fileItem = new FileSystemItem
                {
                    Name = file.Name,
                    FullPath = file.FullName,
                    IsDirectory = false,
                    Parent = item // 親ノードを設定
                };

                fileItem.PropertyChanged += FileSelectionChanged;
                item.Children.Add(fileItem);
            }

            return item;
        }

        /// <summary>
        /// ファイルの選択状態が変更されたときに `SelectedFiles` を更新
        /// </summary>
        private void FileSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileSystemItem.IsChecked))
            {
                UpdateSelectedFiles();
            }
        }

        /// <summary>
        /// `SelectedFiles` を最新の状態に更新
        /// </summary>
        private void UpdateSelectedFiles()
        {
            SelectedFiles.Clear();
            AddSelectedFilesRecursive(Items, SelectedFiles);
        }

        private void AddSelectedFilesRecursive(IEnumerable<FileSystemItem> items, ObservableCollection<FileSystemItem> selectedFiles)
        {
            foreach (var item in items)
            {
                if (!item.IsDirectory && item.IsChecked == true)
                {
                    selectedFiles.Add(item);
                }
                if (item.Children.Any())
                {
                    AddSelectedFilesRecursive(item.Children, selectedFiles);
                }
            }
        }

        /// <summary>
        /// 選択されたファイルのリストを別ウィンドウで表示
        /// </summary>
        public void ShowSelectedFilesWindow()
        {
            var window = new SelectedFilesWindow(this);
            window.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
