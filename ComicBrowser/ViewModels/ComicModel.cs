using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.Serialization;

namespace ComicBrowser.ViewModels
{
    [DataContract]
    public class ComicModel : INotifyPropertyChanged
    {

        private String pubDate = "";
        private string comicName;
        private BitmapImage comicImage;

        public string ComicId;

        public event PropertyChangedEventHandler PropertyChanged;
        public string siteUrl;

        [DataMember]
        public String imageUrl { get; set; }

        [DataMember]
        public int pivotIndex { get; set; }

        [DataMember]
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

        [DataMember]
        public String ComicName
        {
            get
            {
                return comicName;
            }

            set
            {
                if (value != comicName)
                {
                    comicName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ComicName"));
                }
            }
        }

        public BitmapImage ComicImage
        {
            get
            {
                return comicImage;
            }

            set
            {
                if (value != comicImage)
                {
                    comicImage = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ComicImage"));
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
