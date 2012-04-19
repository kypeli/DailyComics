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

        private Boolean comicLoading = false;
        private String pubDate = "";
        private string comicName;
        public string ComicId;

        public event PropertyChangedEventHandler PropertyChanged;

        [DataMember]
        public String imageUrl { get; set; }

        [DataMember]
        public int pivotIndex { get; set; }

        [DataMember]
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

        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }
    }   
}
