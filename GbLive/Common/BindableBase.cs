using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace GbLive.Common
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, string propertyName, [CallerFilePath]string filePath = "", [CallerLineNumber]int sourceLineNumber = 0)
        {
            if (String.IsNullOrWhiteSpace(propertyName))
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "BindableBase.SetProperty<{0}>(storage: {1}, value: {2})", typeof(T), storage.ToString(), value.ToString()));
                sb.AppendLine("propertyName was null or whitespace");
                sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "file: {0}, line number: {1}", filePath, sourceLineNumber));

                throw new ArgumentNullException(nameof(propertyName), sb.ToString());
            }

            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;

            RaisePropertyChanged(propertyName);

            return true;
        }

        protected virtual void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
