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
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace ComicBrowser.DataModels
{
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

    [DataContractAttribute]
    public class ComicData
    {
        [DataMember]
        public String url { get; set; }
        [DataMember]
        public String name { get; set; }
        [DataMember]
        public String pubdate { get; set; }
        [DataMember]
        public String siteurl { get; set; }
    }
}
