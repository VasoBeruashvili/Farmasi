using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class ProductOutViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("tdate")]
        public DateTime Tdate { get; set; }

        [JsonProperty("num")]
        public long Num { get; set; }

        [JsonProperty("purpose")]
        public string Purpose { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}