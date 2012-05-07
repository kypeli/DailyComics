using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Diagnostics;

namespace ComicBrowser
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
            this.DataContext = App.comicListModel;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            // Save changes to the database.
            App.comicListModel.SaveChangesToDB();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("Refreshing items.");
            App.comicListModel.refreshComicLists();
        }

    }
}