using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Model;

namespace WpfApp1.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<FileSystemItem> _items;

        public ObservableCollection<FileSystemItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public MainViewModel()
        {
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

        private FileSystemItem CreateFileSystemItem(DirectoryInfo directoryInfo)
        {
            var item = new FileSystemItem
            {
                Name = directoryInfo.Name,
                FullPath = directoryInfo.FullName,
                IsDirectory = true
            };

            // フォルダのサブフォルダを追加
            foreach (var dir in directoryInfo.GetDirectories())
            {
                item.Children.Add(CreateFileSystemItem(dir));
            }

            // フォルダ内のファイルを追加
            foreach (var file in directoryInfo.GetFiles())
            {
                item.Children.Add(new FileSystemItem
                {
                    Name = file.Name,
                    FullPath = file.FullName,
                    IsDirectory = false
                });
            }

            return item;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
