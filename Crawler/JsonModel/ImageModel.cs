using System.Text.Json;
using System.Net;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Crawler.JsonModel
{
    internal class ImageModel
    {
        [JsonProperty("[data-gallery-role=gallery-placeholder]")]
        public SecondObj Gallery { get; set; }
    }

    internal class SecondObj
    {
        [JsonProperty("mage/gallery/gallery")]
        public ThirdObj Mage { get; set; }
    }

    internal class ThirdObj
    {
        [JsonProperty("data")]
        public List<FourdObj> Data { get; set; }
    }
    internal class FourdObj
    {
        [JsonProperty("full")]
        public string ImageLink { get; set; }
    }

}
