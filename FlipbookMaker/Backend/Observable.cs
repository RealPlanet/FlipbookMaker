using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlipbookMaker.Frontend
{
    public class Observable
        : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void NotifyPropertyChanged<T>(ref T prop, T newVal, [CallerMemberName] string propertyName = "")
        {
            prop = newVal;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
