using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class UserViewModel
    {
        [Required]
        [Display(Name = "UserName")]
        [JsonProperty("userName")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [JsonProperty("password")]
        public string Password { get; set; }
        
        [Display(Name = "MailTo")]
        [JsonProperty("mailTo")]
        public string MailTo { get; set; }



        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("tel")]
        public string Tel { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("rfLink")]
        public string RfLink { get; set; }

        [JsonProperty("createDate")]
        public DateTime CreateDate { get; set; }
    }
}