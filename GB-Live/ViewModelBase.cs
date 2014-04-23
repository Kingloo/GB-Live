using System;
using System.ComponentModel;

namespace GB_Live
{
    class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler pceh = this.PropertyChanged;
            if (pceh != null)
            {
                pceh(this, new PropertyChangedEventArgs(name));
            }
        }

        private System.Windows.Threading.Dispatcher _dispatcher = System.Windows.Application.Current.Dispatcher;
        public System.Windows.Threading.Dispatcher AppDisp { get { return this._dispatcher; } }
    }
}
