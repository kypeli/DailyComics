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
using ComicBrowser.ViewModels;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace ComicBrowser
{
    public partial class Fullscreen : PhoneApplicationPage
    {
        public Fullscreen()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            if (this.NavigationContext.QueryString.ContainsKey("comicIndex"))
            {
                int comicIndex = Convert.ToInt16(this.NavigationContext.QueryString["comicIndex"]);
                if (comicIndex >= 0)
                {
                    BitmapImage bmImage = App.comicListModel.ShowingComicsListModel.ElementAt(comicIndex).ComicImage;
                    FullscreenComic.Source = bmImage;
                }
            }

            base.OnNavigatedTo(e);
        }
    }
}    