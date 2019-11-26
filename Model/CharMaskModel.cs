using CGM_by_ZnZ.VM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CGM_by_ZnZ.Model
{
    public class CharMaskModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MainVM VM = null;

        public CharMaskModel()
        {
            THIS = this;
        }
        private char _Char;
        public char Char
        {
            get => _Char;
            set
            {
                _Char = value;
                OnPropertyChanged();
                if (VM != null)
                    VM.Mask = VM.Mask;
            }
        }

        private string _Chars;
        public string Chars
        {
            get => _Chars;
            set
            {
                _Chars = value;
                OnPropertyChanged();
                if(VM != null)
                    VM.Mask = VM.Mask;
            }
        }

        public CharMaskModel THIS { get; set; }
    }
}
