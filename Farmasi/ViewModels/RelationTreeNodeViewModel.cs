using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class RelationTreeNodeViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("cid")]
        public int Cid { get; set; }

        [JsonProperty("nodes")]
        public List<RelationTreeNodeViewModel> Nodes { get; set; } = new List<RelationTreeNodeViewModel>();

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        public int SubNodeCountSum
        {
            get { return 1 + Nodes.Sum(x => x.SubNodeCountSum); }
        }
    }
}