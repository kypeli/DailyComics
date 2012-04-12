using System;
using System.Windows;
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
using Microsoft.Phone.Info;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ComicBrowser
{
    public partial class MainPage : PhoneApplicationPage
    {

        WebClient wc = new WebClient();
        ObservableCollection<ComicView> pivotComicContent = new ObservableCollection<ComicView>();

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            createPivotContent();

            ImageTools.IO.Decoders.AddDecoder<GifDecoder>();
            ImageTools.IO.Encoders.AddEncoder<PngEncoder>();
        }

        private void createPivotContent()
        {
            wc.OpenReadCompleted += ComicsFetchCompleted;
            wc.OpenReadAsync(new Uri("http://lakka.kapsi.fi:61950/rest/comic/list"));
        }

        private void ComicsFetchCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            wc.OpenReadCompleted -= ComicsFetchCompleted;

            StreamReader s = null;
            MemoryStream ms;
            ComicModel model = (ComicModel)e.UserState;

            try
            {
                s = new StreamReader((Stream)e.Result);
                ms = new MemoryStream(Encoding.Unicode.GetBytes(s.ReadToEnd()));
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            // Process JSON to get interesting data.
            DataContractJsonSerializer jsonparser = new DataContractJsonSerializer(typeof(PivotComicsData));
            PivotComicsData comics = null;
            try
            {
                comics = (PivotComicsData)jsonparser.ReadObject(ms);
            }
            catch (SerializationException)
            {
                Debug.WriteLine("Cannot serialize the JSON. Giving up! Json: " + new StreamReader(ms).ReadToEnd());
                model = null;
                this.DataContext = null;
                return;
            }

            IEnumerator<ComicInfo> enumerator = comics.comics.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ComicInfo comic = enumerator.Current;

                PivotItem comicPivotItem = new PivotItem();
                comicPivotItem.Header = comic.name;

                ComicView comicView = new ComicView();
                comicPivotItem.Content = comicView;
                comicView.id = comic.comicid;
                TopPivot.Items.Add(comicPivotItem);
            }

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
        


        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int currentPivot = ((Pivot)sender).SelectedIndex;
            Debug.WriteLine("Pivot changed. Current pivot: " + currentPivot);
            updatePivotPage(currentPivot);
        }

        private void updatePivotPage(int currentPivot)
        {
            wc.OpenReadCompleted -= HTTPOpenReadCompleted;
            TopPivot.SelectedItem = TopPivot.Items[currentPivot];
            TopPivot.SelectedIndex = currentPivot;

            this.DataContext = null;
            if (PhoneApplicationService.Current.State.ContainsKey("model_" + currentPivot))
            {
                this.DataContext = (ComicModel)PhoneApplicationService.Current.State["model_" + currentPivot];
            }

            if (this.DataContext != null)
            {
                Debug.WriteLine("Data model found in cache.");
            }
            else
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
                ComicModel model = new ComicModel();
                model.pivotIndex = forPivotIndex;
                model.ComicLoading = true;
                try
                {
                    Debug.WriteLine("URL: " + comicDataUri.ToString());
                    wc.OpenReadCompleted -= FetchComicReadCompleted;
                    wc.OpenReadCompleted += HTTPOpenReadCompleted;
                    wc.OpenReadAsync(comicDataUri, model);

                    this.DataContext = model;
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
            ComicView currentView = ((TopPivot.Items[TopPivot.SelectedIndex] as PivotItem).Content as ComicView);
            Uri comicUri = new Uri("http://lakka.kapsi.fi:61950/rest/comic/get?id=" + currentView.id); 
            return comicUri;
        }

        void HTTPOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            StreamReader s = null;
            MemoryStream ms;
            ComicModel model = (ComicModel)e.UserState;

            try
            {
                s = new StreamReader((Stream)e.Result);
                ms = new MemoryStream(Encoding.Unicode.GetBytes(s.ReadToEnd()));
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            // Process JSON to get interesting data.
            DataContractJsonSerializer jsonparser = new DataContractJsonSerializer(typeof(ComicData));
            ComicData data = null;
            try
            {
                data = (ComicData)jsonparser.ReadObject(ms);
            }
            catch (SerializationException)
            {
                Debug.WriteLine("Cannot serialize the JSON. Giving up! Json: " + new StreamReader(ms).ReadToEnd());
                model = null;
                this.DataContext = null;
                return;
            }

            Debug.WriteLine("Parsing JSON done. Fetching comic from URL: " + data.url);

            int requestedPivotIndex = model.pivotIndex;
            model.imageUrl = data.url;
            model.PubDate = data.pubdate;

            wc.OpenReadCompleted -= HTTPOpenReadCompleted;
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
            ComicModel currentComicModel = (ComicModel)e.UserState;
            Debug.WriteLine("Fetched comic strip image: " + currentComicModel.imageUrl);

            currentComicModel.ComicLoading = false;

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

        }

        private void showNewComic(ComicModel currentComicModel, MemoryStream comicBytes)
        {
            ComicView pivotItem = ((TopPivot.SelectedItem as PivotItem).Content as ComicView);
            BitmapImage comicImage = new BitmapImage();
            comicImage.SetSource(comicBytes);
            pivotItem.ComicStrip.Source = comicImage;

            PhoneApplicationService.Current.State["model_" + currentComicModel.pivotIndex] = currentComicModel;
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

        private void TopPivot_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}