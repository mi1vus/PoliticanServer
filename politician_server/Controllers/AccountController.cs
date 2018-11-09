using System.Web.Mvc;
using System.Web.Security;
using politician_server.Models;

namespace politician_server.Controllers
{
    public class AccountController : Controller
    {
        public ViewResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //if (model.UserName == "AutoAdmin")
                //{
                //    ModelState.AddModelError("", "Неправильный логин или пароль");
                //    return View();
                //}

                var authorize = DBHelper.Db.IsAdmin(model.UserName, model.Password);
                if (authorize)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false);
                    return Redirect(returnUrl ?? Url.Action("Index", "Home"));
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин или пароль");
                    return View();
                }
                //else
                //{
                //    ModelState.AddModelError("", "Вы забанены!");
                //    return View();
                //}
            }
            else
            {
                return View();
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            if (Request.Form["submitbutton"] != null && Request.Form["submitbutton"] == "Выйти")
            {
                FormsAuthentication.SignOut();
            }
            //else
            //if (Request.Form["submitbutton"] != null && Request.Form["submitbutton"] == "Сменить пароль")
            //{
            //    var userId = DbHelper.GetUserId(User?.Identity?.Name, User?.Identity?.Name);
            //    return Redirect(Url.Action("AddOrEdit", "User", new {id = userId }));
            //}


            var url = Request["ReturnUrl"];
            return Redirect(url ?? Url.Action("Home", "Index"));
        }

        //[Authorize]
        //public ActionResult ChangePassword()
        //{
        //    var userId = DbHelper.GetUserId(User?.Identity?.Name, User?.Identity?.Name);
        //    return Redirect(Url.Action("AddOrEdit", "User", new { id = userId }));
        //}

        [Authorize]
        public ActionResult SignOut()
        {
            //DbHelper.InitAuthorizeUserTables();
            //DbHelper.DeauthorizeUser(User?.Identity?.Name);
            FormsAuthentication.SignOut();
            return Redirect(Request["ReturnUrl"] ?? Url.Action("Index", "Home"));
        }
    }
}