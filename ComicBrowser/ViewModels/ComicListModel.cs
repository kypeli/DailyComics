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
                m_showingComicsListModel = value;
                OnPropertyChanged("ShowingComicsListModel");
            }
        }

        public void refreshComicLists()
        {

            // Fetching all comics for the Settings view
            var comicsInDB         = from ComicItem item in comicListDb.Items      
                                     select item;

            AllComicsListModel     = new ObservableCollection<ComicItem>(comicsInDB);

            // Fetching all comics that we want to show based on user's settings
            // to show the comics in the UI. 
            var comicsSelectedInDB = from ComicItem item in comicListDb.Items
                                     where item.IsShowing == true
                                     select item;

            ShowingComicsListModel = new ObservableCollection<ComicItem>(comicsSelectedInDB);
        }

        public void addComic(ComicItem comicItem)
        {
            var comicAlreadyInDB = (from ComicItem item in comicListDb.Items
                                    where item.ComicId == comicItem.ComicId
                                    select new { item }).SingleOrDefault();


            if (comicAlreadyInDB == null)
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
