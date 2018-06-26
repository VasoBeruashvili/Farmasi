using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class ProductViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("groupId")]
        public int GroupId { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("image")]
        public byte[] Image { get; set; }

        [JsonProperty("imageBase64")]
        public string ImageBase64 { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("rest")]
        public double Rest { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("unitFullName")]
        public string UnitFullName { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("images")]
        public List<ProductImageViewModel> Images { get; set; }

        [JsonProperty("colors")]
        public List<ProductColorViewModel> Colors { get; set; }

        [JsonProperty("colorGroupNumber")]
        public string ColorGroupNumber { get; set; }
    }
}