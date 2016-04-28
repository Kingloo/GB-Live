using System.Windows;
using System.Windows.Controls;

namespace GB_Live
{
    internal class GBEventTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Everyone { get; set; }
        public DataTemplate Premium { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            GBUpcomingEvent gbEvent = (GBUpcomingEvent)item;

            if (gbEvent.Premium)
            {
                return Premium;
            }
            else
            {
                return Everyone;
            }
        }
    }
}
