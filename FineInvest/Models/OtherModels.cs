using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FineInvest.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Display(Name = "Категория")]
        public string Name { get; set; }
        public int Type  { get; set; }

    }
    public class ArtType
    {
        public int Id { get; set; }

        [Display(Name = "Тип")]
        public string Name { get; set; }

    }
    public class ArtSort
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

}