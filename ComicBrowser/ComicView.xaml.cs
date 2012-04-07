using System.Windows;
using System.Windows.Controls;
using ComicBrowser.ViewModels;
using Microsoft.Phone.Shell;
using System.Diagnostics;

namespace ComicBrowser
{
    public partial class ComicView : UserControl
    {

        public ComicView()
        {
            InitializeComponent();
        }

        private void ComicStrip_ImageOpened(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Image opened.");

//            ComicModel model = PhoneApplicationService.Current.State["Model"] as ComicModel;
        }
    }
}
