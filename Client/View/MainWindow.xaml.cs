using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;

namespace Client.View
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => ((INotifyCollectionChanged)MessagesItems.ItemsSource).CollectionChanged += (q, z) => Scroller.ScrollToEnd();
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow != null) Application.Current.MainWindow.DragMove();
        }
    }
}