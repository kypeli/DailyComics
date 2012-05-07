using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;

namespace ComicBrowser.ViewModels
{
    public class ComicListModel : INotifyPropertyChanged
    {
        private ComicListContext comicListDb;

        public ComicListModel(string dbString)
        {
            comicListDb = new ComicListContext(dbString);
            refreshComicLists();
        }

        private ObservableCollection<ComicItem> m_allComicsListModel;
        public ObservableCollection<ComicItem> AllComicsListModel
        {
            get
            {
                return m_allComicsListModel;
            }

            set
            {
                if (m_allComicsListModel != value)
                {
                    m_allComicsListModel = value;
                    OnPropertyChanged("AllComicsListModel");
                }
            }
        }

        private ObservableCollection<ComicItem> m_showingComicsListModel;
        public ObservableCollection<ComicItem> ShowingComicsListModel
        {
            get
            {
                return m_showingComicsListModel;
            }

            set
            {
                if (m_showingComicsListModel != value)
                {
                    m_showingComicsListModel = value;
                    OnPropertyChanged("ShowingComicsListModel");
                }
            }
        }

        public void refreshComicLists()
        {

            var comicsInDB          = from ComicItem item in comicListDb.Items      
                                      select item;

            m_allComicsListModel    = new ObservableCollection<ComicItem>(comicsInDB);

            var comicsSelectedInDB = from ComicItem item in comicListDb.Items
                                      where item.IsShowing == true
                                      select item;

            bool refreshModel = false;
            if (m_showingComicsListModel != null)
            {
                refreshModel = true;
            }
            
            m_showingComicsListModel = new ObservableCollection<ComicItem>(comicsSelectedInDB);
            if (refreshModel)
            {
                OnPropertyChanged("ShowingComicsListModel");
            }
        }

        public void addComic(ComicItem comicItem)
        {
            bool itemFound = false;
            foreach (ComicItem item in m_allComicsListModel) 
            {
                if (item.ComicId == comicItem.ComicId)
                {
                    itemFound = true;
                    break;
                }
            }

            if (itemFound == false)
            {
                Debug.WriteLine("Item not found in DB, adding to DB and view model.");

                // Adding to DB.
                comicListDb.Items.InsertOnSubmit(comicItem);
                comicListDb.SubmitChanges();

                // And also immediately to the screen.
                m_showingComicsListModel.Add(comicItem);
                m_allComicsListModel.Add(comicItem);
            }

        }

        public ComicItem getComicModel(int pivotIndex)
        {
            if ((m_showingComicsListModel.Count - 1) >= pivotIndex)
            {
                return m_showingComicsListModel[pivotIndex];
            }

            return null;
        }

        public bool modelAlreadyFetched(int pivotIndex)
        {
            if (m_showingComicsListModel.Count > pivotIndex
                && m_showingComicsListModel[pivotIndex] != null
                && (m_showingComicsListModel[pivotIndex] as ComicItem).ComicImage != null)
            {
                return true;
            }

            return false;
        }

        private bool comicLoading = false;
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
                    OnPropertyChanged("ComicLoading");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String argname)
        {

            PropertyChangedEventArgs args = new PropertyChangedEventArgs(argname);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }

        internal void SaveChangesToDB()
        {
            comicListDb.SubmitChanges();
        }
    }
}
