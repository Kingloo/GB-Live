using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GB_Live
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnNotifyPropertyChanged([CallerMemberName] string propertyName = default(string))
        {
            PropertyChangedEventHandler pceh = PropertyChanged;

            if (pceh != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);

                pceh(this, args);
            }
        }
    }
}
