﻿/**
 * Copyright (c) 2012, Johan Paul <johan.paul@gmail.com>
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the <organization> nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.using System;
 */

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
using Microsoft.Phone.Tasks;

namespace ComicBrowser
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

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
            this.ComicLoading = false;

            createPivotContent();

            ImageTools.IO.Decoders.AddDecoder<GifDecoder>();
            ImageTools.IO.Encoders.AddEncoder<PngEncoder>();
        }

        private void createPivotContent()
        {

            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += ComicListFetchCompleted;
            wc.DownloadStringAsync(new Uri("http://lakka.kapsi.fi:61950/rest/comic/list"));
        }

        private void ComicListFetchCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            WebClient wc = sender as WebClient;
            if (wc != null)
            {
                wc = null;
            }

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

                    WebClient comicDataClient = new WebClient();
                    comicDataClient.DownloadStringCompleted += ComicJSONFetchCompleted;
                    comicDataClient.DownloadStringAsync(comicDataUri, model);
                }
                catch (WebException e)
                {
                    Debug.WriteLine("Web access already in progress. Cannot start a new one... cancelling. Error: " + e.ToString());
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

            // Clean the webclient that was created when this request was created.
            WebClient webClient = sender as WebClient;
            if (webClient != null)
            {
                webClient = null;
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
            model.siteUrl = data.siteurl;

            WebClient wc = new WebClient();
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
            [DataMember]
            public String siteurl { get; set; }
        }

        void FetchComicReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            // Clean the webclient that was created when this request was created.
            WebClient webClient = sender as WebClient;
            if (webClient != null)
            {
                webClient = null;
            }
            
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

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void ComicStrip_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (comicLoading) {
                return;
            }

            ComicModel model = PhoneApplicationService.Current.State["model_" + TopPivot.SelectedIndex.ToString()] as ComicModel;
            if (model == null)
            {
                Debug.WriteLine("Model null!");
                return;
            }

            if (model.siteUrl != null && 
                model.siteUrl.Length == 0) 
            {
                Debug.WriteLine("Site URL empty!");
                return;
            }

            WebBrowserTask wbTask = new WebBrowserTask();
            wbTask.Uri = new Uri(model.siteUrl, UriKind.RelativeOrAbsolute);
            wbTask.Show();
        }
    }
}