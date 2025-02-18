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
using Newtonsoft.Json;

namespace WpfApp1.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<FileSystemItem> _items;
        private ObservableCollection<FileSystemItem> _selectedFiles;
        private ObservableCollection<string> _saveNames;
        private string _selectedSave;

        private const string SaveFilePath = "selected_files.json";

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

        public ObservableCollection<string> SaveNames
        {
            get => _saveNames;
            set
            {
                _saveNames = value;
                OnPropertyChanged(nameof(SaveNames));
            }
        }

        public string SelectedSave
        {
            get => _selectedSave;
            set
            {
                _selectedSave = value;
                OnPropertyChanged(nameof(SelectedSave));
                LoadSelection(value);
            }
        }

        public MainViewModel()
        {
            SelectedFiles = new ObservableCollection<FileSystemItem>();
            SaveNames = new ObservableCollection<string>();
            LoadFileSystem(@"D:\Contents\競馬");
            LoadSavedSelections();
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

        /// <summary>
        /// 選択状態を JSON に保存 (Newtonsoft.Json 使用)
        /// </summary>
        public void SaveSelection()
        {
            var selectedFilePaths = SelectedFiles.Select(f => f.FullPath).ToList();

            Dictionary<string, List<string>> saveData = new Dictionary<string, List<string>>();
            if (File.Exists(SaveFilePath))
            {
                var json = File.ReadAllText(SaveFilePath);
                saveData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json) ?? new Dictionary<string, List<string>>();
            }

            string saveName = "Selection " + (saveData.Count + 1);
            saveData[saveName] = selectedFilePaths;

            File.WriteAllText(SaveFilePath, JsonConvert.SerializeObject(saveData, Formatting.Indented));
            SaveNames.Add(saveName);
            SelectedSave = saveName;
        }

        /// <summary>
        /// 保存済みの選択状態のリストをロード
        /// </summary>
        private void LoadSavedSelections()
        {
            SaveNames.Clear();

            if (File.Exists(SaveFilePath))
            {
                var json = File.ReadAllText(SaveFilePath);
                var saveData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);

                if (saveData != null)
                {
                    foreach (var key in saveData.Keys)
                    {
                        SaveNames.Add(key);
                    }
                }
            }
        }

        /// <summary>
        /// 選択状態を復元
        /// </summary>
        private void LoadSelection(string saveName)
        {
            if (string.IsNullOrEmpty(saveName) || !File.Exists(SaveFilePath)) return;

            var json = File.ReadAllText(SaveFilePath);
            var saveData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            if (saveData == null || !saveData.ContainsKey(saveName)) return;

            var selectedPaths = new HashSet<string>(saveData[saveName]);
            SetSelectionRecursive(Items, selectedPaths);
        }

        private void SetSelectionRecursive(IEnumerable<FileSystemItem> items, HashSet<string> selectedPaths)
        {
            foreach (var item in items)
            {
                item.IsChecked = selectedPaths.Contains(item.FullPath);
                if (item.Children.Any())
                {
                    SetSelectionRecursive(item.Children, selectedPaths);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
