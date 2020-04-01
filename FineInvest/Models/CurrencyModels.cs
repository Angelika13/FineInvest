using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FineInvest.Models
{
    public class Currency
    {

        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public int NumCode { get; set; }

        [HiddenInput(DisplayValue = true)]
        public string CharCode { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int Nominal { get; set; }

        [Display(Name = "Валюта")]
        public string Name { get; set; }

        [Display(Name = "Курс")]
        public decimal? Value { get; set; }
        
        [Display(Name = "Изменение")]
        public decimal? Changes { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата")]
        public DateTime DateLoad { get; set; }
        public bool curVisible  { get; set; }

    }
    public class Rate
    {
        [Key]
        public int Cur_ID { get; set; }
        public DateTime Date { get; set; }
        public string Cur_Abbreviation { get; set; }
        public int Cur_Scale { get; set; }
        public string Cur_Name { get; set; }
        public decimal? Cur_OfficialRate { get; set; }
        [Display(Name = "Изменение")]
        public decimal? Changes { get; set; }
        public bool curVisible { get; set; }
    }
}