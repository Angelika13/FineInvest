using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using FineInvest.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;

namespace FineInvest.Controllers
{
    public class ArticlesController : Controller
    {
        EFDbContext db = new EFDbContext();

        public int pageSize = 20;
        private ICSUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ICSUserManager>();
            }
        }
        private ICSRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ICSRoleManager>();
            }
        }
        public ActionResult Index(int page = 1)
        {
            if (db.CatCurrency.Count() != 0)
            {
                if (DateTime.Now.Subtract(db.CatCurrency.First().DateLoad) > new TimeSpan(1, 0, 0, 0))
                    CurrencyUpdate();
            }
            else
                CurrencyUpdate();

            ViewArticle model = new ViewArticle();
            model.Articles = db.Articles.Include(o => o.Category).Include(o => o.Type).Where(o => o.CategoryId < 8).OrderByDescending(o => o.DateOrd);
            model.AllCurrency = db.CatCurrency.Where(c => c.curVisible == true);
            model.PolezArt = db.Articles.Where(a => a.CategoryId == 13);
            model.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = pageSize,
                TotalItems = model.Articles.Count()
            };
            model.Articles = model.Articles.Skip((page - 1) * pageSize).Take(pageSize);
            return View(model);
        }
        public void CurrencyUpdate()
        {
            WebRequest request = WebRequest.Create("http://www.nbrb.by/api/exrates/rates?periodicity=0");
            WebResponse response = request.GetResponse();
            IEnumerable<Rate> rates = new List<Rate>() { };
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                string line;
                if ((line = stream.ReadLine()) != null)
                {
                    rates = JsonConvert.DeserializeObject<List<Rate>>(line);
                }
            }
            DateTime dateLast = rates.First().Date.AddDays(-1);
            WebRequest requestLast = WebRequest.Create("http://www.nbrb.by/api/exrates/rates?ondate=" + dateLast.Year + "-" + dateLast.Month + "-" + dateLast.Day + "&periodicity=0");
            WebResponse responseLast = requestLast.GetResponse();
            IEnumerable<Rate> ratesLast = new List<Rate>() { };
            using (StreamReader stream = new StreamReader(responseLast.GetResponseStream()))
            {
                string line;
                if ((line = stream.ReadLine()) != null)
                {
                    ratesLast = JsonConvert.DeserializeObject<List<Rate>>(line);
                }
            }
            IEnumerable<Currency> AllCurrency = db.CatCurrency;
            Currency currency = new Currency();
            foreach (Rate item in rates)
            {
                if (AllCurrency.Where(c => c.NumCode == item.Cur_ID).Count() != 0)
                    currency = AllCurrency.First(c => c.NumCode == item.Cur_ID);

                currency.Changes = item.Cur_OfficialRate - ratesLast.First(r => r.Cur_ID == item.Cur_ID).Cur_OfficialRate;
                currency.Value = item.Cur_OfficialRate;
                currency.DateLoad = DateTime.Now;

                if (AllCurrency.Where(c => c.NumCode == item.Cur_ID).Count() == 0)
                {
                    currency.NumCode = item.Cur_ID;
                    currency.CharCode = item.Cur_Abbreviation;
                    currency.Nominal = item.Cur_Scale;
                    currency.Name = item.Cur_Name;

                    db.Entry(currency).State = EntityState.Added;
                }
                else
                    db.Entry(currency).State = EntityState.Modified;

                db.SaveChanges();
            }
        }
        public ActionResult MoveMenu(int? categ, int? type, int page = 1)
        {
            ViewArticle model = new ViewArticle();
            model.Articles = db.Articles.Include(o => o.Category).Include(o => o.Type).Where(a => a.CategoryId == categ).Where(a => a.TypeId == type).OrderByDescending(o => o.DateOrd);
            model.AllCurrency = db.CatCurrency.Where(c => c.curVisible == true);
            model.PolezArt = db.Articles.Where(a => a.CategoryId == 13);
            model.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = pageSize,
                TotalItems = model.Articles.Count()
            };
            model.Articles = model.Articles.Skip((page - 1) * pageSize).Take(pageSize);

            if (model.Articles.Count() == 0)
                return RedirectToAction("Index");
            else
                return View(model);
        }

        public ActionResult List(int SelectCateg = 0, int SelectSort = 0, int page = 1)
        {
            List<Category> categories = db.Categories.ToList();
            List<ArtSort> sort = new List<ArtSort>();
            categories.Insert(0, new Category { Name = "Все", Id = 0 });
            sort.Insert(0, new ArtSort { Name = "по умолчанию", Id = 0 });
            sort.Insert(1, new ArtSort { Name = "дате добавления", Id = 1 });
            sort.Insert(2, new ArtSort { Name = "заголовку", Id = 2 });

            ListArticle model = new ListArticle();

            model.SelectCateg = SelectCateg;
            model.SelectSort = SelectSort;

            model.AllCategory = new SelectList(categories, "Id", "Name", SelectCateg);
            model.Sortirovka = new SelectList(sort, "Id", "Name", SelectSort);

            if (SelectCateg == 0)
                model.Articles = db.Articles.Include(a => a.Category).Include(a => a.Type).OrderByDescending(o => o.Id);
            else
                model.Articles = db.Articles.Include(a => a.Category).Include(a => a.Type).Where(a => a.CategoryId == model.SelectCateg).OrderByDescending(o => o.Id);

            if (SelectSort == 1)
                model.Articles = model.Articles.OrderByDescending(a => a.DateOrd);
            if (SelectSort == 2)
                model.Articles = model.Articles.OrderBy(a => a.Caption);

            model.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = pageSize,
                TotalItems = model.Articles.Count()
            };
            model.Articles = model.Articles.Skip((page - 1) * pageSize).Take(pageSize);

            return View(model);
        }
        public ActionResult Show(int id)
        {
            Article article = db.Articles.Include(a => a.Category).Include(a => a.Type).First(o => o.Id == id);

            IEnumerable<Article> Articles = new List<Article> { };
            if (article.TypeId == 6)
                Articles = db.Articles.Include(a => a.Category).Include(a => a.Type).Take(5); 
            else
                Articles = db.Articles.Include(a => a.Category).Include(a => a.Type).Where(a => a.CategoryId == article.CategoryId).Except(db.Articles.Where(a => a.Id == id)).Take(5); 

            if (article != null)
            {
                return View(new ShowArticle
                {
                    Id = article.Id,
                    Caption = article.Caption,
                    DateOrd = article.DateOrd,
                    ArtTxt = article.ArtTxt,
                    CategoryId = article.CategoryId,
                    TypeId = article.TypeId,
                    CategoryName = article.Category.Name,
                    TypeName = article.Type.Name,
                    Picture = article.Picture,
                    Articles = Articles
                });
            }
            return RedirectToAction("Index");
        }
        public ActionResult Edit(int id)
        {
            List<Category> categories = db.Categories.ToList();
            List<ArtType> type = db.ArtTypes.ToList();

            Article article = db.Articles.Include(o => o.Category).First(o => o.Id == id);

            if (article != null)
            {
                return View(new EditArticle
                {
                    Id = article.Id,
                    Picture = article.Picture,
                    Caption = article.Caption,
                    DateOrd = article.DateOrd,
                    ArtTxt = article.ArtTxt,
                    CategoryId = article.CategoryId,
                    TypeId = article.TypeId,
                    AllType = new SelectList(type, "Id", "Name", article.TypeId),
                    AllCategory = new SelectList(categories, "Id", "Name", article.CategoryId)
                });
            }
            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult Edit(EditArticle model, HttpPostedFileBase uploadImage)
        {
            Article article = db.Articles.First(o => o.Id == model.Id);

            if (article != null)
            {
                byte[] imageData = null;

                if (uploadImage != null)
                {
                    // считываем переданный файл в массив байтов
                    using (var binaryReader = new BinaryReader(uploadImage.InputStream))
                    {
                        imageData = binaryReader.ReadBytes(uploadImage.ContentLength);
                    }
                    article.Picture = imageData;
                }
                article.Caption = model.Caption;
                article.ArtTxt = model.ArtTxt;
                article.CategoryId = model.CategoryId;
                article.TypeId = model.TypeId;
            }
            db.Entry(article).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("List");

        }
        public ActionResult Create()
        {
            List<Category> categories = db.Categories.ToList();
            List<ArtType> type = db.ArtTypes.ToList();

            return View(new EditArticle
            {
                AllType = new SelectList(type, "Id", "Name", 0),
                AllCategory = new SelectList(categories, "Id", "Name", 0),

            });
        }
        [HttpPost]
        public ActionResult Create(EditArticle model, HttpPostedFileBase uploadImage)
        {
            Article article = new Article();

            if (ModelState.IsValid)
            {
                byte[] imageData = null;

                if (uploadImage != null)
                {
                    // считываем переданный файл в массив байтов
                    using (var binaryReader = new BinaryReader(uploadImage.InputStream))
                    {
                        imageData = binaryReader.ReadBytes(uploadImage.ContentLength);
                    }
                }
                if (article != null)
                {
                    article.Picture = imageData;
                    article.Caption = model.Caption;
                    article.DateOrd = DateTime.Now;
                    article.ArtTxt = model.ArtTxt;
                    article.CategoryId = model.CategoryId;
                    article.TypeId = model.TypeId;
                }
                db.Entry(article).State = EntityState.Added;
                db.SaveChanges();
                return RedirectToAction("List");
            }
            return View(model);
        }
        public ActionResult PortfelControl(string userId)
        {
            if (Request.IsAuthenticated)
            {
                var curUser = UserManager.FindById(User.Identity.GetUserId());

                if (User.IsInRole("admin"))
                {
                    ViewPortfel model = new ViewPortfel();

                    model.Articles = db.Articles.Include(a => a.Category).Include(a => a.Type).Where(a => a.TypeId == 6);
                    model.PortAccess = db.Portfels.Include(a => a.User).Include(a => a.Article).Include(a => a.Article.Category).Include(a => a.Article.Type).Where(a => a.OpenAccess == false && a.Otkaz == false);
                    //model.Portfels = db.Portfels.Include(a => a.User).Include(a => a.Article).Include(a => a.Article.Category).Include(a => a.Article.Type);
                    model.Portfels = db.Portfels.Except(model.PortAccess).Include(a => a.User).Include(a => a.Article).Include(a => a.Article.Category).Include(a => a.Article.Type);

                    return View(model);
                }
                else
                {
                    ViewPortfel model = new ViewPortfel();

                    model.Articles = db.Articles.Include(a => a.Category).Include(a => a.Type).Where(a => a.TypeId == 6);
                    model.Portfels = db.Portfels.Include(a => a.Article).Include(a => a.Article.Category).Include(a => a.Article.Type).Where(a => a.UserId == curUser.Id);

                    foreach (Portfel portfel in model.Portfels)
                    {
                        model.Articles = model.Articles.Where(a => a.Id != portfel.ArticleId);
                    }

                    return View(model);
                }
            }
            return RedirectToAction("Login", "Users");
        }
        public ActionResult accessOk(int Id)
        {
            ViewPortfel model = new ViewPortfel();
            Portfel portfel = db.Portfels.First(p => p.Id == Id);
            portfel.OpenAccess = true;

            db.Entry(portfel).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("PortfelControl");

        }
        public ActionResult accessRem(int Id)
        {
            ViewPortfel model = new ViewPortfel();
            Portfel portfel = db.Portfels.First(p => p.Id == Id);

            db.Entry(portfel).State = EntityState.Deleted;
            db.SaveChanges();
            return RedirectToAction("PortfelControl");

        }
        public ActionResult accessDel(int Id)
        {
            ViewPortfel model = new ViewPortfel();
            Portfel portfel = db.Portfels.First(p => p.Id == Id);

            db.Entry(portfel).State = EntityState.Deleted;
            db.SaveChanges();
            return RedirectToAction("PortfelControl");

        }
        public ActionResult OpenArt(int id)
        {
            ViewPortfel model = new ViewPortfel();
            Portfel portfel = new Portfel()
            {
                ArticleId = id,
                OpenAccess = false,
                UserId = User.Identity.GetUserId(),
                Otkaz = false,
                DateQuest = DateTime.Now
            };
            db.Entry(portfel).State = EntityState.Added;
            db.SaveChanges();
            return RedirectToAction("PortfelControl");
        }
    }
}