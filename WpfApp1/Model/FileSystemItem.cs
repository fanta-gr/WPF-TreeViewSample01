using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Model
{
    public class FileSystemItem : INotifyPropertyChanged
    {
        private bool _isChecked;
        private ObservableCollection<FileSystemItem> _children;

        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));

                    // 子要素のチェック状態を更新
                    foreach (var child in Children)
                    {
                        child.IsChecked = value;
                    }
                }
            }
        }

        public ObservableCollection<FileSystemItem> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<FileSystemItem>();
                }
                return _children;
            }
            set
            {
                _children = value;
                OnPropertyChanged(nameof(Children));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
