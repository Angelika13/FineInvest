using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FineInvest.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using Calabonga.Xml.Exports;

namespace FineInvest.Controllers
{
    public class UsersController : Controller
    {

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
        [Authorize(Roles = "admin")]
        public ActionResult Index()
        {
            return View(UserManager.Users.Include(u => u.Role).Where(u => u.UserName != "MainAdmin")); //учетную запись главного администратора нельзя редактировать - роль по умолчанию, чтобы не потерять доступ к управлению сайтом
        }

        [Authorize]
        public async Task<ActionResult> Edit(string id)
        {
            var curUser = UserManager.FindById(User.Identity.GetUserId());

            List<ICSRole> roles = RoleManager.Roles.ToList();
            ICSUser user = await UserManager.Users.Include(u => u.Role).FirstAsync(u => u.Id == curUser.Id);
            if (user != null)
            {
                return View(new EditUserModel
                {
                    Email = user.Email,
                    FIO = user.FIO,
                    Year = user.Year,
                    Id = user.Id,
                    RoleId=user.RoleId,
                    PhoneNumber = user.PhoneNumber,
                    Role  = user.Role.Description,
                    AllRoles = new SelectList(roles, "Id", "Description", user.RoleId)
                });
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<ActionResult> Edit(EditUserModel model)
        {
            if (ModelState.IsValid)
            {
                ICSUser user = await UserManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.FIO = model.FIO;
                    user.Year = model.Year;
                    user.PhoneNumber = model.PhoneNumber;
                    IdentityResult result = await UserManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Что-то пошло не так");
                    }
                }
            }
            return View(model);
        }

        [Authorize(Roles = "admin")]        
        public async Task<ActionResult> Change(string id)
        {
            List<ICSRole> roles = RoleManager.Roles.ToList();
            ICSUser user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                return View(new EditUserModel
                {
                    Email = user.Email,
                    FIO = user.FIO,
                    Year = user.Year,
                    Id = user.Id,
                    RoleId = user.RoleId,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role.Description,
                    AllRoles = new SelectList(roles, "Id", "Description", user.RoleId)
                });
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<ActionResult> Change(EditUserModel model)
        {
            List<ICSRole> roles = RoleManager.Roles.ToList();
            model.AllRoles = new SelectList(roles, "Id", "Description", model.RoleId);

            if (ModelState.IsValid)
            {
                ICSUser user = await UserManager.FindByIdAsync(model.Id);
                ICSRole role = await RoleManager.FindByIdAsync(user.RoleId);
                if (user != null)
                {
                    await UserManager.RemoveFromRoleAsync(user.Id, role.Name);
                    role = await RoleManager.FindByIdAsync(model.RoleId);
                    await UserManager.AddToRoleAsync(user.Id, role.Name);

                    user.RoleId = model.RoleId;
                    IdentityResult result = await UserManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Что-то пошло не так");
                    }
                }
            }
            return View(model);
        }
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                ICSRole role = await RoleManager.FindByNameAsync("user");
                ICSUser user = new ICSUser {
                    FIO = model.FIO,
                    UserName =  model.Email,
                    Email = model.Email,
                    Year = DateTime.Now.Year,
                    RoleId = role.Id,
                    PhoneNumber = model.PhoneNumber
                };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.UpdateAsync(user);
                    await UserManager.AddToRoleAsync(user.Id, "user");
                    return RedirectToAction("Login", "Users");
                }
                else
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }
            return View(model);
        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public ActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                bool UserExist = false;

                ICSUser user = await UserManager.FindAsync(model.FioOrEmail, model.Password); //ищем пользователя по имени
                if (user == null)
                {
                    ICSUser userEmail = await UserManager.FindByEmailAsync(model.FioOrEmail); //ищем  пользователя по Email (если был введен Email)
                    if (userEmail != null)
                    {
                        user = await UserManager.FindAsync(userEmail.UserName, model.Password);
                        if (user == null)
                        {
                            ModelState.AddModelError("", "Неверный логин или пароль.");
                        }
                        else
                        {
                            UserExist = true;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Неверный логин или пароль.");
                    }
                }
                else
                {
                    UserExist = true;
                }
                if (UserExist == true)
                {
                    ClaimsIdentity claim = await UserManager.CreateIdentityAsync(user,
                    DefaultAuthenticationTypes.ApplicationCookie);
                    AuthenticationManager.SignOut();
                    AuthenticationManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = true
                    }, claim);
                    if (String.IsNullOrEmpty(returnUrl))
                        return RedirectToAction("Index", "Home");
                    return Redirect(returnUrl);
                }
            }
            ViewBag.returnUrl = returnUrl;
            return View(model);
        }
        public ActionResult LogOut()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string id)
        {
            ICSUser user = await UserManager.FindByIdAsync(id);

            return View(new ResetPasswordViewModel { Id = id, Email = user.Email});
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                // Не показывать, что пользователь не существует
                return RedirectToAction("ResetPasswordConfirmation", "Users");
            }
            var result = await UserManager.RemovePasswordAsync(user.Id);
            result = await UserManager.AddPasswordAsync(user.Id, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Users");
            }
            return View();
        }
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}