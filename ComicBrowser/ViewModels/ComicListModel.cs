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

            /**
             * If the collection is not empty (i.e. we want to update the view), we need to do
             * a trick here. It seems the ObservableCollection is not updated if the second LINQ query
             * is just being associated with it. We need to get the result of the query to a temp variable,
             * then clean the model and then populate the UI model with the new values.
             * 
             * Not sure why this is not working. 
             * See: http://stackoverflow.com/questions/10481734/observablecollection-not-updated-when-doing-a-second-linq-query
             */
            ObservableCollection<ComicItem> showingItems = new ObservableCollection<ComicItem>(comicsSelectedInDB);
            if (m_showingComicsListModel != null)
            {
                // List is being updated.
                m_showingComicsListModel.Clear();
                foreach (ComicItem item in showingItems)
                {
                    m_showingComicsListModel.Add(item);
                }
            }                        
            else
            {
                // Initial run of the query, so we can use association.
                m_showingComicsListModel = showingItems;
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
