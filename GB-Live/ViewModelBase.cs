using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GB_Live
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler pceh = this.PropertyChanged;
            if (pceh != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);

                pceh(this, args);
            }
        }

        //private System.Windows.Threading.Dispatcher _dispatcher = System.Windows.Application.Current.Dispatcher;
        //public System.Windows.Threading.Dispatcher AppDisp { get { return this._dispatcher; } }
    }
}
