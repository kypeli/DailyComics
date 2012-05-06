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

namespace ComicBrowser.ViewModels
{
    public class ComicListModel : INotifyPropertyChanged
    {
        private ObservableCollection<ComicModel> _comicsListModel = new ObservableCollection<ComicModel>();
        public ObservableCollection<ComicModel> ComicsListModel
        {
            get
            {
                return _comicsListModel;
            }
        }

        public void addComic(ComicModel comicModel)
        {
            _comicsListModel.Add(comicModel);
        }

        public ComicModel getComicModel(int pivotIndex)
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
                && (_comicsListModel[pivotIndex] as ComicModel).ComicImage != null)
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
