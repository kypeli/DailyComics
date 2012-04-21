using System;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using ComicBrowser.ViewModels;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using ImageTools.IO.Gif;
using ImageTools;
using ImageTools.IO.Png;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace ComicBrowser
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private WebClient wc = new WebClient();
        private bool comicLoading = false;

        private ObservableCollection<ComicModel> _comicsListModel = new ObservableCollection<ComicModel>();
        public ObservableCollection<ComicModel> ComicsListModel {
            get {
                return _comicsListModel;
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

        public MainPage()
        {
            InitializeComponent();
            TopPivot.DataContext = this;
            this.DataContext = this;

            createPivotContent();

            ImageTools.IO.Decoders.AddDecoder<GifDecoder>();
            ImageTools.IO.Encoders.AddEncoder<PngEncoder>();
        }

        private void createPivotContent()
        {

            wc.DownloadStringCompleted += ComicListFetchCompleted;
            wc.DownloadStringAsync(new Uri("http://lakka.kapsi.fi:61950/rest/comic/list"));
        }

        private void ComicListFetchCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            wc.DownloadStringCompleted -= ComicListFetchCompleted;

            if (RESTError(e)) {
                Debug.WriteLine("Error fetching comic list! Error: " + e.Error.ToString());
                return;
            }

            // Process JSON to get interesting data.
            DataContractJsonSerializer jsonparser = new DataContractJsonSerializer(typeof(PivotComicsData));
            PivotComicsData comics = null;
            try
            {
                byte[] jsonArray = Encoding.UTF8.GetBytes(e.Result);
                MemoryStream jsonStream = new MemoryStream(jsonArray);
                comics = (PivotComicsData)jsonparser.ReadObject(jsonStream);
            }
            catch (SerializationException)
            {
                Debug.WriteLine("Cannot serialize the JSON. Giving up! Json: " + e.Result);
                return;
            }

            // Populate the model with comic data. 
            IEnumerator<ComicInfo> enumerator = comics.comics.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ComicInfo comic = enumerator.Current;
                ComicModel model = new ComicModel();
                
                model.ComicName = comic.name;
                model.ComicId = comic.comicid;
                _comicsListModel.Add(model);

                Debug.WriteLine("Got new comic to show. Name: " + comic.name + ", id: " + comic.comicid);
            }

            // Activate the first comic after the pivots have been populated.
            if (TopPivot.Items.Count > 0)
            {
                TopPivot.SelectedItem = TopPivot.Items[0];
            }
        }

        [DataContract]
        public class PivotComicsData
        {
            [DataMember]
            public IEnumerable<ComicInfo> comics { get; set; }
        }

        [DataContract]
        public class ComicInfo 
        {
            [DataMember]
            public String name { get; set; }
            [DataMember]
            public String comicid { get; set; }
        }

        private void TopPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int currentPivot = ((Pivot)sender).SelectedIndex;
            Debug.WriteLine("Pivot changed. Current pivot: " + currentPivot.ToString());
            updatePivotPage(currentPivot);
        }

        private void updatePivotPage(int currentPivot)
        {
            wc.DownloadStringCompleted -= ComicJSONFetchCompleted;

            TopPivot.SelectedItem = TopPivot.Items[currentPivot];
            TopPivot.SelectedIndex = currentPivot;

            if (PhoneApplicationService.Current.State.ContainsKey("model_" + currentPivot.ToString()) == false)
            {
                Debug.WriteLine("No cached model found. Fetching new data from the web.");
                fetchComicDataFromWeb(currentPivot);
            }
        }

        private void fetchComicDataFromWeb(int forPivotIndex)
        {
            Uri comicDataUri = getComicDataUri(forPivotIndex);
            if (comicDataUri != null)
            {
                ComicModel model = _comicsListModel[forPivotIndex];
                model.pivotIndex = forPivotIndex;
                this.ComicLoading = true;

                try
                {
                    Debug.WriteLine("Fetching comic strip: " + comicDataUri.ToString());
                    wc.CancelAsync();
                    wc.OpenReadCompleted -= FetchComicReadCompleted;
                    wc.DownloadStringCompleted += ComicJSONFetchCompleted;
                    wc.DownloadStringAsync(comicDataUri, model);
                }
                catch (NotSupportedException)
                {
                    Debug.WriteLine("Web access already in progress. Cannot start a new one... cancelling.");
                    model = null;
                }

            }
        }

        private Uri getComicDataUri(int pivotIndex)
        {
            ComicModel model = _comicsListModel[pivotIndex];
            Uri comicUri = new Uri("http://lakka.kapsi.fi:61950/rest/comic/get?id=" + model.ComicId); 
            return comicUri;
        }

        void ComicJSONFetchCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (RESTError(e))
            {
                Debug.WriteLine("Error fetching JSON! Error: " + e.Error.ToString());
                return;
            }

            ComicModel model = (ComicModel)e.UserState;

            // Process JSON to get interesting data.
            DataContractJsonSerializer jsonparser = new DataContractJsonSerializer(typeof(ComicData));
            ComicData data = null;
            try
            {
                byte[] jsonBytes = Encoding.UTF8.GetBytes(e.Result);
                MemoryStream jsonStream = new MemoryStream(jsonBytes);
                data = (ComicData)jsonparser.ReadObject(jsonStream);
            }
            catch (SerializationException)
            {
                Debug.WriteLine("Cannot serialize the JSON. Giving up! Json: " + e.Result);
                model = null;
                return;
            }

            Debug.WriteLine("Parsing JSON done. Fetching comic from URL: " + data.url);

            int requestedPivotIndex = model.pivotIndex;
            model.imageUrl = data.url;
            model.PubDate = data.pubdate;

            wc.DownloadStringCompleted -= ComicJSONFetchCompleted;
            wc.OpenReadCompleted += FetchComicReadCompleted;
            wc.OpenReadAsync(new Uri(data.url,
                                     UriKind.Absolute),
                             model
                             );
        }

        [DataContractAttribute]
        public class ComicData
        {
            [DataMember]
            public String url  { get; set; }
            [DataMember]
            public String name { get; set; }
            [DataMember]
            public String pubdate { get; set; }
        }

        void FetchComicReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (RESTError(e))
            {
                Debug.WriteLine("Error fetching comic image! Error: " + e.Error.ToString());
                return;
            }

            ComicModel currentComicModel = (ComicModel)e.UserState;
            Debug.WriteLine("Fetched comic strip image.");

            Stream reply = null;
            try
            {
                reply = (Stream)e.Result;
            }
            catch (WebException webEx)
            {
                if (webEx.Status != WebExceptionStatus.Success)
                {
                    Debug.WriteLine("Web error occured. Cannot load image!");
                    return;
                }
            }

            MemoryStream comicStripBytes = new MemoryStream();
            reply.CopyTo(comicStripBytes);
            byte[] imgBytes = comicStripBytes.ToArray();
            if (isGifImage(imgBytes))
            {
                Debug.WriteLine("Image is a GIF");

                ExtendedImage gifStrip = new ExtendedImage();
                gifStrip.LoadingCompleted += 
                    (s, args) => {
                        Debug.WriteLine("GIF loaded. Encoding GIF image to PNG image...");

                        ExtendedImage gifImage = (ExtendedImage)s;
                        MemoryStream pngBytes = new MemoryStream();

                        ImageTools.IO.Png.PngEncoder enc = new PngEncoder();
                        enc.Encode(gifImage, pngBytes);

                        this.Dispatcher.BeginInvoke(() => {
                            Debug.WriteLine("Encoding done. Setting PNG bytes to BitmapImage and showing.");
                            showNewComic(currentComicModel, pngBytes);
                        });
                    };

                gifStrip.UriSource = new Uri(currentComicModel.imageUrl,
                                             UriKind.Absolute);
            }
            else
            {
                Debug.WriteLine("Image is not a GIF. Putting image bytes directly to BitmapImage.");
                showNewComic(currentComicModel, comicStripBytes);
            }

            PhoneApplicationService.Current.State["model_" + currentComicModel.pivotIndex.ToString()] = currentComicModel;
            this.ComicLoading = false;
        }

        private void showNewComic(ComicModel currentComicModel, MemoryStream comicBytes)
        {
            int forPivotIndex = currentComicModel.pivotIndex;
            BitmapImage comicImage = new BitmapImage();
            comicImage.SetSource(comicBytes);

            currentComicModel.ComicImage = comicImage;
        }

        private bool RESTError(AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return true;
            }

            return false;
        }

        private bool isGifImage(byte[] imgBytes)
        {
            if (imgBytes.Length  < 3) {
                return false;
            }

            if (imgBytes[0] == 'G'
                && imgBytes[1] == 'I'
                && imgBytes[2] == 'F')
            {
                return true;
            }

            return false;                
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