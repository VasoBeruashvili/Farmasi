using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class ProductImageViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("productId")]
        public int? ProductId { get; set; }

        [JsonProperty("image")]
        public byte[] Image { get; set; }
    }
}