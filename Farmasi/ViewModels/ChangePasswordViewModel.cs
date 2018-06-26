using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "CurrentPassword")]
        [JsonProperty("currentPassword")]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword")]
        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "RepeatPassword")]
        [JsonProperty("repeatPassword")]
        public string RepeatPassword { get; set; }
    }
}