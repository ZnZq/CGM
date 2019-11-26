using CGM_by_ZnZ.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CGM_by_ZnZ.VM
{

    public class MainVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ObservableCollection<CharMaskModel> CharMaskList { get; set; } = new ObservableCollection<CharMaskModel>();
        public ObservableCollection<string> CombList { get; set; } = new ObservableCollection<string>() { };

        SynchronizationContext sync = null;

        public MainVM()
        {
            sync = SynchronizationContext.Current;

            CharMaskList.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    foreach (CharMaskModel item in e.NewItems)
                        item.VM = this;
            };

            CharMaskList.Add(new CharMaskModel()
            {
                Char = '$',
                Chars = "0123456789"
            });

            Mask = "$$$$";
        }

        List<int> indexs = new List<int>();
        Dictionary<char, string> chars_list = new Dictionary<char, string>();

        private string _Mask;
        public string Mask
        {
            get => _Mask;
            set
            {
                CombCount = 1;
                int a;
                _Mask = value;

                for (int i = 0; i < _Mask.Length; i++)
                {
                    if (chars_list.ContainsKey(_Mask[i]))
                        indexs.Add(i);

                    a = CharMaskList.Where(q => q.Char == _Mask[i]).Select(q => q.Chars.Length).Sum();
                    CombCount *= a > 0 ? a : 1;
                }
                OnPropertyChanged();
            }
        }

        SaveFileDialog saveDialog = new SaveFileDialog()
        {
            Filter = "Text file *.txt | *.txt",
            DefaultExt = "txt"
        };

        private bool _ToBlockSize;
        public bool ToBlockSize
        {
            get => _ToBlockSize;
            set
            {
                if (value)
                {
                    if (saveDialog.ShowDialog() == true)
                    {
                        PathToSave = saveDialog.FileName;
                        _ToBlockSize = true;
                        OnPropertyChanged();
                    }
                }
                else
                {
                    _ToBlockSize = false;
                    OnPropertyChanged();
                }
            }
        }

        private string _PathToSave;
        public string PathToSave
        {
            get => _PathToSave;
            set
            {
                _PathToSave = value;
                OnPropertyChanged();
            }
        }

        private int _BlockSize = 1000;
        public int BlockSize
        {
            get => _BlockSize;
            set
            {
                _BlockSize = value;
                OnPropertyChanged();
            }
        }

        private bool _IsGen;
        public bool IsGen
        {
            get => _IsGen;
            set
            {
                _IsGen = value;
                OnPropertyChanged();
            }
        }

        private int _CombCount;
        public int CombCount
        {
            get => _CombCount;
            set
            {
                _CombCount = value;
                OnPropertyChanged();
            }
        }

        private int _cCombCount;
        public int cCombCount
        {
            get => _cCombCount;
            set
            {
                _cCombCount = value;
                OnPropertyChanged();
            }
        }

        private ICommand _SaveCommand;
        public ICommand SaveCommand => _SaveCommand ?? (_SaveCommand = new RelayCommand((p) =>
        {
            if (saveDialog.ShowDialog() == true)
                save(saveDialog.FileName);
        }, o => CombList.Count != 0));

        private void save(string file, bool append = false)
        {
            if (CombList.Count == 0) return;
            FileMode mode = append ? FileMode.OpenOrCreate | FileMode.Append : FileMode.OpenOrCreate;
            using (FileStream fs = new FileStream(file, mode, FileAccess.Write))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, CombList) + (append ? Environment.NewLine : ""));
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        private ICommand _StartCommand;
        public ICommand StartCommand => _StartCommand ?? (_StartCommand = new RelayCommand(async (p) =>
       {
           IsGen = true;
           cCombCount = 0;
           CombList.Clear();

           chars_list.Clear();
           foreach (var c in CharMaskList)
               if (chars_list.ContainsKey(c.Char))
                   chars_list[c.Char] += c.Chars;
               else
                   chars_list[c.Char] = c.Chars;

           indexs.Clear();

           for (int i = 0; i < _Mask.Length; i++)
               if (chars_list.ContainsKey(_Mask[i]))
                   indexs.Add(i);

           await Task.Factory.StartNew(() =>
           {
               func(CombList, 0, Mask);
           });

           IsGen = false;

       }, o => !IsGen && (Mask.Length != 0) && (CharMaskList.Count != 0) && CombCount != 1));

        private void func(ObservableCollection<string> list, int lvl, string source)
        {
            int index = indexs[lvl];
            string m = source;
            string chars = chars_list[source[index]];
            for (var i = 0; i < chars.Length; i++)
            {
                if (!IsGen) return;
                m = ReplaceAt(m, index, chars[i]);
                if (lvl == indexs.Count - 1)
                {
                    sync.Send(q => { list.Add(m); }, null);
                    cCombCount++;
                    if (ToBlockSize && CombList.Count >= BlockSize)
                    {
                        save(PathToSave, true);
                        sync.Send(q => { CombList.Clear(); }, null);
                    }
                }
                else
                    func(list, lvl + 1, m);
            }
            if(lvl == 0 && ToBlockSize)
                save(PathToSave, true);
        }

        public string ReplaceAt(string value, int index, char newchar)
        {
            if (value.Length <= index)
                return value;
            else
                return string.Concat(value.Select((c, i) => i == index ? newchar : c));
        }

        private ICommand _StopCommand;
        public ICommand StopCommand => _StopCommand ?? (_StopCommand = new RelayCommand((p) =>
        {
            IsGen = false;
        }, o => IsGen));

        private ICommand _AddCharMaskCommand;
        public ICommand AddCharMaskCommand => _AddCharMaskCommand ?? (_AddCharMaskCommand = new RelayCommand(p =>
        {
            if (p is CharMaskModel model)
                CharMaskList.Add(model);
        }, o => !IsGen));
    }
}
