using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class ContragentRelationViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("parentId")]
        public int ParentId { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tel")]
        public string Tel { get; set; }

        [JsonProperty("sales")]
        public decimal Sales { get; set; }

        [JsonProperty("childrenSales")]
        public decimal ChildrenSales { get; set; }

        [JsonProperty("groupSales")]
        public decimal GroupSales { get; set; }

        [JsonProperty("groupBonus")]
        public decimal GroupBonus { get; set; }


        [JsonProperty("groupBonusWithoutVat")]
        public decimal GroupBonusWithoutVat { get; set; }

        [JsonProperty("bonus")]
        public decimal Bonus { get; set; }

        [JsonProperty("groupPayBonus")]
        public decimal GroupPayBonus { get; set; }

        [JsonProperty("percent")]
        public decimal Percent { get; set; }
    }
}