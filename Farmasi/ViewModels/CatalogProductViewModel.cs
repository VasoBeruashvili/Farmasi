using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class CatalogProductViewModel
    {
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("GroupId")]
        public int GroupId { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Comment")]
        public string Comment { get; set; }

        [JsonProperty("Path")]
        public string Path { get; set; }

        [JsonProperty("Image")]
        public byte[] Image { get; set; }

        [JsonProperty("ImageBase64")]
        public string ImageBase64 { get; set; }

        [JsonProperty("Price")]
        public decimal Price { get; set; }

        [JsonProperty("Rest")]
        public double Rest { get; set; }

        [JsonProperty("Currency")]
        public string Currency { get; set; }

        [JsonProperty("UnitFullName")]
        public string UnitFullName { get; set; }

        [JsonProperty("Quantity")]
        public int Quantity { get; set; }

        [JsonProperty("Images")]
        public List<ProductImageViewModel> Images { get; set; }

        [JsonProperty("Colors")]
        public List<ProductColorViewModel> Colors { get; set; }

        [JsonProperty("ColorGroupNumber")]
        public string ColorGroupNumber { get; set; }
    }
}