using System;

namespace GBLive.WPF.Common
{
    public class BasicEventArgs<T> : EventArgs
    {
        private readonly T _t = default(T);
        public T Object => _t;

        public BasicEventArgs(T basic)
        {
            _t = basic;
        }
    }
}
