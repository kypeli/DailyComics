using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ComicBrowser.ViewModels
{
    public class ComicModel : INotifyPropertyChanged
    {

        BitmapImage imageSource = null;
        Boolean comicLoading = false;
        String pubDate = "";

        public String imageUrl { get; set; }
        public int pivotIndex { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ComicModel() {
        }

        public BitmapImage ImageSource
        {
            get
            {
                return imageSource;
            }

            set
            {
                if (value != imageSource)
                {
                    imageSource = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ImageSource"));
                }
            }
        }

        public Boolean ComicLoading
        {
            get
            {
                return comicLoading;
            }

            set
            {
                if (value != comicLoading)
                {
                    comicLoading = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ComicLoading"));
                }
            }
        }

        public String PubDate
        {
            get
            {
                return pubDate;
            }

            set
            {
                if (value != pubDate)
                {
                    pubDate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("PubDate"));
                }
            }
        }

        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }


    }   

}
