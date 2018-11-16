using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using politician_server.Models;
using System.Net.Mail;
using System.Net;

namespace politician_server.Controllers
{
    public class HomeController : Controller
    {
        public int PageSize = 20;

        public ActionResult MailConfirm(string nick, string examPass)
        {
            var user = DBHelper.Db.GetUser(nick);

            if (user == null)
                return View("Error");

            int score = 10;
            if (!user.Smoking)
                score += 2;
            if (!user.Alcohol)
                score += 2;

            if (!DBHelper.Db.EndExamStage(user.Nick, 0, score, examPass, "подтверждение почты"))
                return View("Error");

            return View("MailConfirmed");
        }

        public ActionResult MailChange(string nick, string examPass)
        {
            var user = DBHelper.Db.GetUser(nick);

            if (user == null)
                return View("Error");
            string newPass = null;
            if ((newPass = DBHelper.Db.ChangePassword(user.Nick, examPass)) == null)
                return View("Error");

            try
            {
                //var url = UsersController.webServerUrl + $"/Home/MailConfirm?nick={user.Nick}&examPass={user.ExamPass}";
                //var urlMailChange = UsersController.webServerUrl + $"/Home/MailChange?nick={user.Nick}&examPass={user.ExamPass}";
                var urlUnsubscribe = UsersController.webServerUrl + $"/Home/UserDelete?nick={user.Nick}&examPass={user.ExamPass}";

                // текст письма
                var body =
"<h2>Смена пароля в игре 'Я политик'.</h2><br><br><br>" +
$"Вы зарегистрировались в игре 'Я политик' под псевдонимом '{user.Nick}'. По вашему запросу пароль был изменен на:'{newPass}'" +
$"Если вы хотите отказаться от регистрации, то пройдите по ссылке" +
$"<a href={urlUnsubscribe} target=\"_blank\"> {urlUnsubscribe}</a>" +
"Внимание. Не направляйте сообщения на адрес отправки данного уведомления –<br>" +
"любые сообщения на этот адрес будут без рассмотрения автоматически удалены.<br>";

                /*                RestClient client = new RestClient();
                                client.BaseUrl = new Uri("https://api.mailgun.net/v3");
                                client.Authenticator =
                                    new HttpBasicAuthenticator("api", "697f4571bedacca9e7e965de6c11efd1-c9270c97-bcb2f4f5");
                                RestRequest request = new RestRequest();
                                request.AddParameter("domain", "sandbox6c86115741e54bf6b203eec3defeee47.mailgun.org", ParameterType.UrlSegment);
                                request.Resource = "{domain}/messages";
                                request.AddParameter("from", "IamaPolitician <postmaster@sandbox6c86115741e54bf6b203eec3defeee47.mailgun.org>");
                                request.AddParameter("to", $@"<{user.Mail}>");
                                request.AddParameter("subject", "IamaPolitician - подтверждение почты");
                                request.AddParameter("text", body);
                                request.Method = Method.POST;
                                var r = client.Execute(request); 
                    */


                // отправитель - устанавливаем адрес и отображаемое в письме имя
                MailAddress from = new MailAddress("IamaPolitician@yandex.ru", "IamaPolitician");
                // кому отправляем
                MailAddress to = new MailAddress(user.Mail);
                //MailAddress to = new MailAddress("test @allaboutspam.com");

                // создаем объект сообщения
                MailMessage m = new MailMessage(from, to);

                m.Headers.Add("Precedence", "bulk");
                m.Headers.Add("List-Unsubscribe", $"<{urlUnsubscribe}>");

                // тема письма
                m.Subject = "IamaPolitician - смена пароля";

                m.Body = body;

                // письмо представляет код html
                m.IsBodyHtml = true;
                // адрес smtp-сервера и порт, с которого будем отправлять письмо
                SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 25);// 465);
                                                                        // логин и пароль
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential("IamaPolitician", "ae05d656fdcdc7e1c94870da2599d5ea");
                smtp.EnableSsl = true;
                smtp.Send(m);
            }
            catch (Exception ex)
            {
                return View("Error");
            }

            return View("MailChanged");
        }

        public ActionResult UserDelete(string nick, string examPass)
        {
            //var user = DBHelper.Db.GetUser(nick);

            //if (user == null)
            //    return View("Error");

            //int score = 10;
            //if (!user.Smoking)
            //    score += 2;
            //if (!user.Alcohol)
            //    score += 2;

            //if (!DBHelper.Db.EndExamStage(user.Nick, 0, score, examPass, "подтверждение почты"))
            //    return View("Error");

            return View("MailConfirmed");
        }

        [Authorize]
        public ActionResult Index(int page = 1, int FilterUserStage = -1, int FilterUserState = -1, string FilterUserNick = "")
        {
            var StageAll = new List<Stage>();
            StageAll.Add(new Stage { Id = -1, Name = "Все" });
            for (int i = 1; i < 29; ++i)
            {
                StageAll.Add(new Stage { Id = i, Name = i.ToString() });
            }
            ViewBag.StageAll = StageAll;

            var users = DBHelper.Db.UsersRating();

            var TaskStatesAll = new List<Stage>();
            TaskStatesAll.Add(new Stage { Id = -1, Name = "Все" });
            int id = 0;
            foreach (string mkey in Enum.GetNames(typeof(DBHelper.Models.TaskStates)))
            {
                TaskStatesAll.Add(new Stage { Id = id, Name = mkey });
                ++id;
            }
            ViewBag.TaskStatesAll = TaskStatesAll;

            var usersModel = new Models.UsersListViewModel
            {
                Users = new List<politician_server.Models.ViewUser>(),
                PagingInfo = new Models.PagingInfo
                {
                    CurrentPage = 1,
                    ItemsPerPage = PageSize,
                    TotalItems = 0
                }
            };

            int maxPages;
            bool filter = FilterUserStage > 0 || FilterUserState >= 0 || !string.IsNullOrWhiteSpace(FilterUserNick);

            Func<DBHelper.Models.User, bool> PredicatStage = t => true;
            if (FilterUserStage > 0)
                PredicatStage = t => t.Stage == FilterUserStage;

            Func<DBHelper.Models.User, bool> PredicatState = t => true;
            if (FilterUserState >= 0)
                PredicatState = t => t.State == (DBHelper.Models.TaskStates)FilterUserState;

            Func<DBHelper.Models.User, bool> PredicatNick = t => true;
            if (!string.IsNullOrWhiteSpace(FilterUserNick))
                PredicatNick = t => t.Nick == FilterUserNick;

            var totalItems = users.Count(t => PredicatStage(t) && PredicatState(t) && PredicatNick(t));
            if (totalItems <= 0)
                maxPages = 1;
            else
                maxPages = (int)Math.Ceiling((decimal)totalItems / PageSize);
            page = page > maxPages ? maxPages : page;
            usersModel.Users =
            (from user in users.Where(t => PredicatStage(t) && PredicatState(t) && PredicatNick(t)).OrderBy(t => t.Stage).Skip((page - 1) * PageSize).Take(PageSize)
             select new Models.ViewUser(user)).ToList();
            usersModel.PagingInfo = new Models.PagingInfo
            {
                CurrentPage = page > maxPages ? maxPages : page,
                ItemsPerPage = PageSize,
                TotalItems = totalItems
            };

            ViewBag.CurrentController = GetType().ToString();
            return View(usersModel);
        }

        [Authorize]
        public ViewResult Exam(string nick, int page = 1)
        {
            var user = DBHelper.Db.GetUser(nick);
            var files = DBHelper.Db.GetFiles(nick, user?.Stage ?? 0);
            if (user == null || files == null || string.IsNullOrWhiteSpace(nick))
                return View("Error");

            var maxPages = 0;
            int totalItems = files.Count();
            if (totalItems <= 0)
                maxPages = 1;
            else
                maxPages = (int)Math.Ceiling((decimal)totalItems / PageSize);

            page = page > maxPages ? maxPages : page;

            var usersModel = new UserDetailViewModel
            {
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page > maxPages ? maxPages : page,
                    ItemsPerPage = PageSize,
                    TotalItems = totalItems
                },
                User = new ViewUser(user),
                Files = files.OrderBy(t => t.Name).Skip((page - 1) * PageSize).Take(PageSize)
            };

            return View(usersModel);
        }

        [Authorize]
        [HttpPost]
        public ViewResult Exam(string nick, int page = 1, int score = -1, string comment = "")
        {
             var user = DBHelper.Db.GetUser(nick);
            var files = DBHelper.Db.GetFiles(nick, user?.Stage ?? 0);
            if (user == null || string.IsNullOrWhiteSpace(nick))
                return View("Error");

            var r = Request;
            if (Request.Form["submitbutton"] != null && string.Compare(Request.Form["submitbutton"], "Сообщить об ошибке материалов", true) == 0)
            {
                if (!DBHelper.Db.SetExamError(nick, user.Stage,  user.ExamPass, comment))
                    return View("Error");

                return View("Saved");
            }


            var maxPages = 0;
            int totalItems = files.Count();
            if (totalItems <= 0)
                maxPages = 1;
            else
                maxPages = (int)Math.Ceiling((decimal)totalItems / PageSize);

            page = page > maxPages ? maxPages : page;

            var usersModel = new UserDetailViewModel
            {
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page > maxPages ? maxPages : page,
                    ItemsPerPage = PageSize,
                    TotalItems = totalItems
                },
                User = new ViewUser(user),
                Files = files.OrderBy(t => t.Name).Skip((page - 1) * PageSize).Take(PageSize)
            };

            if (score < 0)
            {
                ModelState.AddModelError("", "Вы забыли оценить задание!");
                return View(usersModel);
            }

            if (!DBHelper.Db.EndExamStage(nick, user.Stage, score, user.ExamPass, comment))
                return View("Error");

            return View("Saved");
        }

        [Authorize]
        public ActionResult Download(string nick, int stage, int fid)
        {
            var file = DBHelper.Db.GetFile(fid);

            if (file == null || !file.IsFile || string.IsNullOrWhiteSpace(file.Path) ||
                !System.IO.File.Exists(file.Path))
                return View("Error");

            byte[] fileBytes = System.IO.File.ReadAllBytes(file.Path);
            string fileName = file.Name;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult DownloadWin_32_64_Client()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Files/Client/I_am_a_politician.rar"));
            string fileName = "I_am_a_politician.rar";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}
