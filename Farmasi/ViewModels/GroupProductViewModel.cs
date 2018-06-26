using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class GroupProductViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("parentId")]
        public int ParentId { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("productQuantity")]
        public string ProductQuantity { get; set; }

        [JsonProperty("children")]
        public List<GroupProductViewModel> Children { get; set; }
    }
}