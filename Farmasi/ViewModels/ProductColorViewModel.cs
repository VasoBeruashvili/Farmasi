using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class ProductColorViewModel
    {
        [JsonProperty("productId")]
        public int ProductId { get; set; }

        [JsonProperty("colorGroupNumber")]
        public string ColorGroupNumber { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}