using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class ContragentViewModel
    {
        [Required]
        [MinLength(11)]
        [MaxLength(11)]
        [Display(Name = "UserName")]
        [JsonProperty("userName")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [JsonProperty("password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "RepeatPassword")]
        [JsonProperty("repeatPassword")]
        public string RepeatPassword { get; set; }

        [Required]
        [Display(Name = "ContragentFullName")]
        [JsonProperty("contragentFullName")]
        public string ContragentFullName { get; set; }

        [Required]
        [MinLength(9)]
        [MaxLength(9)]
        [Display(Name = "ContragentPhone")]
        [JsonProperty("contragentPhone")]
        public string ContragentPhone { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Address")]
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}