using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Net.Mail;
using System.IO;
using DBHelper.Models;
using RestSharp;
using RestSharp.Authenticators;

namespace politician_server.Controllers
{
    public class UsersController : Controller
    {
        public static string webServerUrl = "http://90.156.139.94:777";

        //public UsersController()
        //{
        //}

        // DELETE api/values/5
        public string Authorize(string name, string pass)
        {
            try
            {
                var res = DBHelper.Db.IsAuthorizeUser(name, pass);
                return res.ToString();
            }
            catch (Exception ex)
            {
                LogError("Authorize = " + ex.ToString(), ex.StackTrace);
                return "False";
            }
        }

        public string NickExist(string name)
        {
            try
            {
                var res = DBHelper.Db.NickExist(name);
                return res.ToString();
            }
            catch (Exception ex)
            {
                LogError("NickExist = " + ex.ToString(), ex.StackTrace);
                return "False";
            }
        }

        public string MailExist(string mail)
        {
            try
            {
                var res = DBHelper.Db.MailExist(mail);
                return res.ToString();
            }
            catch (Exception ex)
            {
                LogError("MailExist = " + ex.ToString(), ex.StackTrace);
                return "False";
            }
        }

        //public static IRestResponse SendSimpleMessage()
        //{

        //}
        //public string MailConfirm(string name) {
        //    SendSimpleMessage();
        //}
        public string MailConfirm(string name)
        {
            bool result = false;

            try
            {
                var user = DBHelper.Db.GetUser(name);
                if (user == null)
                    throw new Exception("user is null");

                var url = webServerUrl + $"/Home/MailConfirm?nick={user.Nick}&examPass={user.ExamPass}";
                var urlMailChange = webServerUrl + $"/Home/MailChange?nick={user.Nick}&examPass={user.ExamPass}";
                var urlUnsubscribe = webServerUrl + $"/Home/UserDelete?nick={user.Nick}&examPass={user.ExamPass}";

                // текст письма
                var body =
"<h2>Подтверждение почты в игре 'Я политик'.</h2><br><br><br>" +
$"Вы зарегистрировались в игре 'Я политик' под псевдонимом '{user.Nick}'. Для подтверждения регистрации пройдите по ссылке:" +
$"<a href={url} target=\"_blank\"> {url}</a><br><br><br>" +
"Если вы забыли ваш пароль, то можете сменить его по ссылке:" +
$"<a href={urlMailChange} target=\"_blank\"> {urlMailChange}</a><br><br><br>" +
$"Если вы хотите отказаться от регистрации, то пройдите по ссылке" +
$"<a href={urlUnsubscribe} target=\"_blank\"> {urlUnsubscribe}</a><br>" +
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
                m.Subject = "IamaPolitician - подтверждение почты";

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
                result = true;//r.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                LogError("MailConfirm = " + ex.ToString(), ex.StackTrace);
            }
            return result.ToString();
        }

        public static bool MailNotifyNewStage(string name, string pass)
        {
            bool result = false;

            try
            {
                var user = DBHelper.Db.GetUser(name);
                var levels = DBHelper.Db.GetLevels(name);

                if (user == null || user.ExamPass != pass)
                    throw new Exception("user is null");

                //var url = webServerUrl + $"/Home/MailConfirm?nick={user.Nick}&examPass={user.ExamPass}";
                var urlMailChange = webServerUrl + $"/Home/MailChange?nick={user.Nick}&examPass={user.ExamPass}";
                var urlUnsubscribe = webServerUrl + $"/Home/UserDelete?nick={user.Nick}&examPass={user.ExamPass}";

                // текст письма
                var body =
"<h2>Ваше задание проверено, вы перешли на новый уровень в игре 'Я политик'.</h2><br><br><br>" +
$"Вы зарегистрировались в игре 'Я политик' под псевдонимом '{user.Nick}' и отправили материалы по заданию №{user.Stage - 1} на проверку. " +
$"Задание было проверено! Вы получили за {user.Stage - 1} задание - {levels.FirstOrDefault(t => t.Number == user.Stage - 1)?.Score} баллов. " +
$"Если вы хотите отказаться от регистрации, то пройдите по ссылке" +
$"<a href={urlUnsubscribe} target=\"_blank\"> {urlUnsubscribe}</a><br>" +
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
                m.Subject = "IamaPolitician - проверка задания окончена";

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
                result = true;//r.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                //LogError("MailConfirm = " + ex.ToString(), ex.StackTrace);
                return false;
            }
            return result;
        }

        public string Ping() {
            return "ОК";
        }

        public string LogError(string msg, string path)
        {
            bool res = false;
            try
            {
                if (!System.IO.Directory.Exists(Server.MapPath("~/Logs/")))
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Logs/"));

                var fpath = Server.MapPath("~/Logs/" + DateTime.Today.ToString("dd_MM_yyyy") + ".txt");
                var text = string.Format(
    @"[{0}] - {1} +++ [{2}]" + Environment.NewLine, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") , msg, path);
                System.IO.File.AppendAllText(fpath, text);
                res = true;
            }
            catch {}

            return res.ToString();
        }

        // GET api/users
        public JsonResult GetRatings()
        {
            try
            {
                var users = DBHelper.Db.UsersRating();
                return Json(users);
            }
            catch (Exception ex)
            {
                LogError("Ratings = " + ex.ToString(), ex.StackTrace);
                return Json(new List<User>());
            }
        }

        public JsonResult GetLevels(string usName)
        {
            try
            {
                var levels = DBHelper.Db.GetLevels(usName);
                return Json(levels);
            }
            catch (Exception ex)
            {
                LogError("GetLevels = " + ex.ToString(), ex.StackTrace);
                return Json(new List<LevelScore>());
            }
        }

        public JsonResult GetFiles(string usName, int stage)
        {
            try
            {
                var files = DBHelper.Db.GetFiles(usName, stage);
                return Json(files);
            }
            catch (Exception ex)
            {
                LogError("GetFiles = " + ex.ToString(), ex.StackTrace);
                return Json(new List<User>());
            }
        }

        public string DeleteFile(int fielId, string usName, int stage)
        {
            bool res = false;
            try
            {
                var files = DBHelper.Db.GetFiles(usName, stage);
                var file = files.Single(t => t.Id == fielId);
                if (file != null)
                    res = DBHelper.Db.DeleteFile(fielId);
                if (res)
                {
                    System.IO.File.Delete(Server.MapPath("~/Files/" + usName + "/" + stage + "/" + file.Name));
                    System.IO.File.Delete(Server.MapPath("~/Backup/" + usName + "/" + stage + "/" + file.Name));
                }
            }
            catch (Exception ex)
            {
                LogError("DeleteFile = " + ex.ToString(), ex.StackTrace);
            }
            return res.ToString();
        }
        // GET api/users/3
        public JsonResult GetUser(string name)
        {
            try
            {
                var user = DBHelper.Db.GetUser(name);
                if (user.Stage != 23)
                user.ExamPass = null;
                return Json(user);
            }
            catch (Exception ex)
            {
                LogError("User = " + ex.ToString(), ex.StackTrace);
                return Json(null);
            }
        }

        public string AddUser(
                    string Nick,
                    string Pass,
                    string Fname,
                    string Lname,
                    string Midname,
                    string City,
                    string Telephone,
                    string Mail,
                    string Work,
                    string Status,
                    string Deviz,
                    string Parties,
                    string PolitExp,
                    bool Female,
                    bool Alcohol,
                    bool Smoking,
                    string Religions)
        {
            try
            {
                var res = DBHelper.Db.AddUser(                    
                    Nick,
                    Pass,
                    Fname,
                    Lname,
                    Midname,
                    City,
                    Telephone,
                    Mail,
                    Work,
                    Status,
                    Deviz,
                    Parties,
                    PolitExp,
                    Female,
                    Alcohol,
                    Smoking,
                    Religions);
                return ((int)res).ToString();
            }
            catch (Exception ex)
            {
                LogError("AddUser = " + ex.ToString(), ex.StackTrace);
                return "False"; 
            }
        }

        // DELETE api/values/5
        public JsonResult Upload(string user, int stage, string fileName, string surl = null)
        {
            var result = false;
            try
            {
                if (string.IsNullOrWhiteSpace(surl))
                {
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Files/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/"));
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Backup/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Backup/"));

                    if (!System.IO.Directory.Exists(Server.MapPath("~/Files/" + user + "/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/" + user + "/"));
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Files/" + user + "/" + stage + "/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/" + user + "/" + stage + "/"));

                    if (!System.IO.Directory.Exists(Server.MapPath("~/Backup/" + user + "/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Backup/" + user + "/"));
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Backup/" + user + "/" + stage + "/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Backup/" + user + "/" + stage + "/"));

                    if (Request.Files.Count > 0)
                        for (int i = 0; i < Request.Files.Count; ++i)
                        {
                            //foreach (string f in Request.Files.AllKeys)
                            //{
                            //    HttpPostedFile file = Request.Files[f];
                            //    file.SaveAs("D:\\UploadFile\\UploadedFiles\\" + file.FileName);
                            //}


                            var f = Request.Files[i];
                            System.IO.Stream fileContent = f.InputStream;
                            System.IO.FileStream fileStream = System.IO.File.Create(Server.MapPath("~/Files/" + user + "/" + stage + "/") + fileName);
                            fileContent.Seek(0, System.IO.SeekOrigin.Begin);
                            fileContent.CopyTo(fileStream);
                            fileStream.Dispose();
                            fileStream = System.IO.File.Create(Server.MapPath("~/Backup/" + user + "/" + stage + "/") + fileName);
                            fileContent.Seek(0, System.IO.SeekOrigin.Begin);
                            fileContent.CopyTo(fileStream);
                            fileStream.Dispose();
                        }
                    else if (Request.InputStream.Length != 0)
                    {
                        System.IO.Stream fileContent = Request.InputStream;
                        System.IO.FileStream fileStream = System.IO.File.Create(Server.MapPath("~/Files/" + user + "/" + stage + "/") + fileName);
                        fileContent.Seek(0, System.IO.SeekOrigin.Begin);
                        fileContent.CopyTo(fileStream);
                        fileStream.Dispose();
                        fileStream = System.IO.File.Create(Server.MapPath("~/Backup/" + user + "/" + stage + "/") + fileName);
                        fileContent.Seek(0, System.IO.SeekOrigin.Begin);
                        fileContent.CopyTo(fileStream);
                        fileStream.Dispose();
                    }

                    if (System.IO.File.Exists(Server.MapPath("~/Files/" + user + "/" + stage + "/" + fileName)) &&
                        System.IO.File.Exists(Server.MapPath("~/Backup/" + user + "/" + stage + "/" + fileName)))
                    {
                        result = DBHelper.Db.AddFile(user, stage, fileName, Server.MapPath("~/Files/" + user + "/" + stage + "/" + fileName), true);
                    }
                }
                else
                {
                    result = DBHelper.Db.AddFile(user, stage, fileName, surl, false);
                }
            }
            catch (Exception ex)
            {
                LogError("Upload = " + ex.ToString(), ex.StackTrace);
            }

            return Json(result);
        }

        // DELETE api/values/5
        public string AddTextFile(string user, int stage, string fileName, string text)
        {
            var result = false;
            try
            {
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Files/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/"));
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Backup/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Backup/"));

                    if (!System.IO.Directory.Exists(Server.MapPath("~/Files/" + user + "/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/" + user + "/"));
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Files/" + user + "/" + stage + "/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Files/" + user + "/" + stage + "/"));

                    if (!System.IO.Directory.Exists(Server.MapPath("~/Backup/" + user + "/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Backup/" + user + "/"));
                    if (!System.IO.Directory.Exists(Server.MapPath("~/Backup/" + user + "/" + stage + "/")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Backup/" + user + "/" + stage + "/"));

                    // Create a file to write to.
                    using (StreamWriter sw = System.IO.File.CreateText(Server.MapPath("~/Files/" + user + "/" + stage + "/" + fileName + ".txt")))
                    {
                        sw.Write(text);
                    }

                    using (StreamWriter sw = System.IO.File.CreateText(Server.MapPath("~/Backup/" + user + "/" + stage + "/" + fileName + ".txt")))
                    {
                        sw.Write(text);
                    }

                    if (System.IO.File.Exists(Server.MapPath("~/Files/" + user + "/" + stage + "/" + fileName + ".txt")) &&
                        System.IO.File.Exists(Server.MapPath("~/Backup/" + user + "/" + stage + "/" + fileName + ".txt")))
                    {
                        result = DBHelper.Db.AddFile(user, stage, fileName + ".txt", Server.MapPath("~/Files/" + user + "/" + stage + "/" + fileName + ".txt"), true);
                    }
            }
            catch (Exception ex)
            {
                LogError("Upload = " + ex.ToString(), ex.StackTrace);
            }

            return result.ToString();
        }

        public string SetExamStage(string nick, int stage)
        {
            try
            {
                var res = DBHelper.Db.SetToExamStage(nick, stage);
                return res.ToString();
            }
            catch (Exception ex)
            {
                LogError("SetExamStage = " + ex.ToString(), ex.StackTrace);
                return "False";
            }
        }

        public string EndExamStage(string name, int stage, int score, string examPass, string comment)
        {
            try
            {
                var res = DBHelper.Db.EndExamStage(name, stage, score, examPass, comment);
                return res.ToString();
            }
            catch (Exception ex)
            {
                LogError("SetExamStage = " + ex.ToString(), ex.StackTrace);
                return "False";
            }
        }

        public string EndExamTestStage(string name, int stage, int score, string comment)
        {
            try
            {
                bool res = false;
                var user = DBHelper.Db.GetUser(name);
                if (user != null && (user.Stage == 3 || user.Stage == 6) && stage == user.Stage && 
                    ((user.Stage == 3 && score <= 30) ||
                     (user.Stage == 6 && score <= 86)))
                    res = DBHelper.Db.EndExamStage(name, stage, score, user.ExamPass, comment);

                return res.ToString();
            }
            catch (Exception ex)
            {
                LogError("SetExamStage = " + ex.ToString(), ex.StackTrace);
                return "False";
            }
        }

        public string AddOpponents(string name, int stage, int opponents)
        {
            bool res = false;
            try
            {
                var user = DBHelper.Db.GetUser(name);
                var ratings = DBHelper.Db.UsersRating();
                var opponentsDB = DBHelper.Db.GetOpponentls(name, stage);

                if (ratings == null || user == null || user.Stage < stage || opponentsDB.Count > 0)
                    throw new Exception("ratings null or user null or low stage or opponents added");

                int time = DateTime.Now.Millisecond;
                Random rnd = new Random(time);
                var opposites = ratings.Where(t => t.Id != user.Id && t.Stage >= stage).Select(t=>t.Id).ToList();

                if (opposites.Count < opponents)
                    throw new Exception($"no {opponents} Подходящих игроков");
                res = true;
                var count = opposites.Count;

                for (int i = 0; res &&  i < opponents && i < count; ++i)
                {                    
                    int r = rnd.Next(opposites.Count);
                    res = DBHelper.Db.AddOpponent(user.Id, opposites[r], stage);
                    opposites.RemoveAt(r);
                }
            }
            catch (Exception ex)
            {
                LogError("AddOpponents = " + ex.ToString(), ex.StackTrace);
                res = false;
            }
            return res.ToString();
        }

        public JsonResult GetOpponents(string name, int stage)
        {
            {
                try
                {
                    var opponents = DBHelper.Db.GetOpponentls(name, stage);
                    return Json(opponents);
                }
                catch (Exception ex)
                {
                    LogError("GetOpponents = " + ex.ToString(), ex.StackTrace);
                    return Json(new List<LevelScore>());
                }
            }
        }
    }
}