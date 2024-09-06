using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GenshinJPTextSpeaker
{

    public class Bindable : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            backingStore = value;
            OnNotifyPropertyChanged(propertyName);
        }

        public virtual void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            App.Current.Dispatcher.Invoke(() => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
        #endregion
    }

    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action _callbackNoArgument;
        private readonly Action<object> _callback;

        public Command(Action<object> callback)
        {
            _callback = callback;
        }

        public Command(Action callback)
        {
            _callbackNoArgument = callback;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _callback?.Invoke(parameter);
            _callbackNoArgument?.Invoke();
        }
    }
}
