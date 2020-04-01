using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FineInvest.Models
{
    public class Article
    {

        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Display(Name = "Категория")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Display(Name = "Тип")]
        public int TypeId { get; set; }
        public ArtType Type { get; set; }
        
        [Display(Name = "Заголовок")]
        public string Caption { get; set; }

        [Display(Name = "Картинка")]
        public byte[] Picture { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата")]
        public DateTime DateOrd { get; set; }

        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        public string ArtTxt { get; set; }

    }
    public class ViewArticle
    {
        public IEnumerable<Article> PolezArt { get; set; }
        public IEnumerable<Article> Articles { get; set; }
        public IEnumerable<Currency> AllCurrency { get; set; }
        public IEnumerable<Rate> Rates { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
    public class PagingInfo
    {
        public int TotalItems { get; set; }

        public int ItemsPerPage { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage); }
        }
    }
    public class ListArticle
    {
        public IEnumerable<Article> Articles { get; set; }
        public int SelectCateg { get; set; }
        public SelectList AllCategory { get; set; }
        public int SelectSort { get; set; }
        public SelectList Sortirovka { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
    public class EditArticle
    {

        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Display(Name = "Категория")]
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public SelectList AllCategory { get; set; }
        
        [Display(Name = "Тип")]
        public string TypeName { get; set; }
        public int TypeId { get; set; }
        public SelectList AllType { get; set; }
        
        [Display(Name = "Заголовок")]
        public string Caption { get; set; }

        [Display(Name = "Картинка")]
        public byte[] Picture { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата")]
        public DateTime DateOrd { get; set; }

        [Display(Name = "Описание")]
        [DataType(DataType.Html)]
        public string ArtTxt { get; set; }        
    }
    public class ShowArticle
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Display(Name = "Категория")]
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public SelectList AllCategory { get; set; }

        [Display(Name = "Тип")]
        public string TypeName { get; set; }
        public int TypeId { get; set; }
        public SelectList AllType { get; set; }

        [Display(Name = "Заголовок")]
        public string Caption { get; set; }

        [Display(Name = "Картинка")]
        public byte[] Picture { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата")]
        public DateTime DateOrd { get; set; }

        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        public string ArtTxt { get; set; }
        public IEnumerable<Article> Articles { get; set; }
    }
    public class Portfel
    {

        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        public int ArticleId { get; set; }
        public Article Article { get; set; }

        public string UserId { get; set; }
        public ICSUser User { get; set; }

        [Display(Name = "Доступ")]
        public bool OpenAccess { get; set; }
        public DateTime DateQuest { get; set; }
        public bool Otkaz { get; set; }

    }
    public class ViewPortfel
    {
        public IEnumerable<Portfel> PortAccess { get; set; }
        public IEnumerable<Portfel> Portfels { get; set; }
        public IEnumerable<Article> Articles { get; set; }
        
    }
}