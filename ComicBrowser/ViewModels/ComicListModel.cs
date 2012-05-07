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
            refreshComicList();
        }

        private ObservableCollection<ComicItem> _comicsListModel;
        public ObservableCollection<ComicItem> ComicsListModel
        {
            get
            {
                return _comicsListModel;
            }
        }

        public void refreshComicList()
        {
            var comicsSelectedInDB = from ComicItem item in comicListDb.Items
                                     where item.IsShowing == true
                                     select item;

            _comicsListModel = new ObservableCollection<ComicItem>(comicsSelectedInDB);

            // Execute the query.
            foreach (ComicItem item in _comicsListModel)
            {
                Debug.WriteLine("Comic to show from DB cache: " + item.ComicName);
            }
        }

        public void addComic(ComicItem comicItem)
        {
            bool itemFound = false;
            foreach (ComicItem item in _comicsListModel) 
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
                _comicsListModel.Add(comicItem);
            }

        }

        public ComicItem getComicModel(int pivotIndex)
        {
            if ((_comicsListModel.Count - 1) >= pivotIndex)
            {
                return _comicsListModel[pivotIndex];
            }

            return null;
        }

        public bool modelAlreadyFetched(int pivotIndex)
        {
            if (_comicsListModel.Count > pivotIndex
                && _comicsListModel[pivotIndex] != null
                && (_comicsListModel[pivotIndex] as ComicItem).ComicImage != null)
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
    }
}
