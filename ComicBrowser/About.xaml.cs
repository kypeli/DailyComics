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
using Microsoft.Phone.Tasks;

namespace ComicBrowser
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
			this.AnimatedTitleText.Begin();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // pop up the link to rate and review the app
            MarketplaceReviewTask review = new MarketplaceReviewTask();
    	    review.Show();
        }
    }
}