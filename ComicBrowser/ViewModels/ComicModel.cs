using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace ComicBrowser.ViewModels
{
    public class ComicListContext : DataContext
    {
        public ComicListContext(string connectionString)
            : base(connectionString)
        { }

        // Specify the SQL tablet for our comic data
        public Table<ComicItem> Items;
    }

    [Table]
    public class ComicItem : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public string siteUrl;
        public String imageUrl { get; set; }
        public int pivotIndex { get; set; }

        // Define ID: private field, public property, and database column.
        private int _dbId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int ComicDBId
        {
            get { return _dbId; }
            set
            {
                if (_dbId != value)
                {
                    NotifyPropertyChanging("ComicDBId");
                    _dbId = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ComicDBId"));
                }
            }
        }

        private string m_comicId;

        [Column]
        public string ComicId
        {
            get { return m_comicId; }
            set
            {
                if (m_comicId != value)
                {
                    NotifyPropertyChanging("ComicId");
                    m_comicId = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ComicId"));
                }
            }
        }

        private bool m_isShowing = true;

        [Column]
        public Boolean IsShowing
        {
            get { return m_isShowing; }
            set
            {
                if (m_isShowing != value)
                {
                    NotifyPropertyChanging("IsShowing");
                    m_isShowing = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsShowing"));
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;
        
        private String m_pubDate = "";
        public String PubDate
        {
            get
            {
                return m_pubDate;
            }

            set
            {
                if (value != m_pubDate)
                {
                    m_pubDate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("PubDate"));
                }
            }
        }

        private string m_comicName = "";
        [Column]
        public String ComicName
        {
            get
            {
                return m_comicName;
            }

            set
            {
                if (value != m_comicName)
                {
                    NotifyPropertyChanging("ComicName");
                    m_comicName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ComicName"));
                }
            }
        }

        private BitmapImage m_comicImage = null;
        public BitmapImage ComicImage
        {
            get
            {
                return m_comicImage;
            }

            set
            {
                if (value != m_comicImage)
                {
                    m_comicImage = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ComicImage"));
                }
            }
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }
        #endregion

        #region INotifyPropertyChanging Members
        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }
        #endregion
    }   
}
