using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Farmasi.ViewModels
{
    public class RecoveryViewModel
    {
        public int Id { get; set; }
        
        public string Email { get; set; }
    }
}