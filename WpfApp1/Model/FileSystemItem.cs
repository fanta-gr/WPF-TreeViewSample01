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
        private bool? _isChecked = false; // true: チェック, false: 未チェック, null: 部分チェック
        private ObservableCollection<FileSystemItem> _children;
        private FileSystemItem _parent;

        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }

        public bool? IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    if (_isChecked == true && value == null)
                    {
                        // 現在選択状態のノードをクリックした場合は未選択 (false) にする
                        value = false;
                    }
                    else if (_isChecked == null && value == true)
                    {
                        // 部分選択のノードをクリックした場合は全選択 (true) にする
                        value = true;
                    }

                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));

                    // 子ノードのチェック状態を変更
                    foreach (var child in Children)
                    {
                        child.IsChecked = value;
                    }

                    // 親ノードのチェック状態を更新
                    UpdateParentCheckState();
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

        public FileSystemItem Parent
        {
            get => _parent;
            set => _parent = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void UpdateParentCheckState()
        {
            if (Parent == null) return;

            bool allChecked = Parent.Children.All(c => c.IsChecked == true);
            bool allUnchecked = Parent.Children.All(c => c.IsChecked == false);

            Parent._isChecked = allChecked ? true : allUnchecked ? false : (bool?)null;
            Parent.OnPropertyChanged(nameof(IsChecked));

            // さらに上の親ノードも更新
            Parent.UpdateParentCheckState();
        }
    }

}
