using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBHelper.Models;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;

namespace DBHelper
{
    public static class Constants
    {
        public const int IdRoleAdmin = 1;
        public const string RightReadName = "Read";
        public const string RightWriteName = "Write";
        public const string RightBannedName = "None";
    }

    public enum AddUserError
    {
        AddSucess = 0,
        NickExist,
        MailExist,
        AnyDataEmpty,
        SQLError,
        Exception
    }

    public static class Db
    {
        // строка подключения к БД
        private static readonly string ConnStr /*= "server=localhost;user=MYSQL;database=terminal_archive;password=tt2QeYy2pcjNyBm6AENp;"*/;
        private static readonly string DB /*= "terminal_archive"*/;
        //private static string _connStrTest = "server=localhost;user=MYSQL;database=products;password=tt2QeYy2pcjNyBm6AENp;";

        //public static Dictionary<int, Terminal> Terminals = new Dictionary<int, Terminal>();
        //public static List<Group> Groups = new ListUser<Group>();
        //public static Dictionary<int, User> Users = new Dictionary<int, User>();
        //public static Dictionary<int, Role> Roles = new Dictionary<int, Role>();

        static Db()
        {
            var rootWebConfig =
            System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/MyWebSiteRoot");
            if (rootWebConfig.AppSettings.Settings.Count <= 0) return;
            var connectionSetting =
                rootWebConfig.AppSettings.Settings["connStr"];
            if (connectionSetting != null)
                ConnStr = connectionSetting.Value;
            var DBSetting =
                rootWebConfig.AppSettings.Settings["DB"];
            if (DBSetting != null)
                DB = DBSetting.Value;
        }

        /// <returns>true - authorize, false - banned, null - no user or pass </returns>
        public static bool IsAuthorizeUser(string Nick, string password)
        {
            if (string.IsNullOrWhiteSpace(Nick))
                return false;

            bool users = false;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                var hash = GetMD5(password);

                var sql = string.Format(
@" SELECT  COUNT(`u`.`id`)
 FROM `{0}`.`users` AS `u`
 WHERE `u`.`nickname` = '{1}' AND `u`.`pass` = '{2}' ; ", DB, Nick, hash);

                conn.Open();

                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read() && !dataReader.IsDBNull(0))
                {
                    users = dataReader.GetInt32(0) > 0;
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return users;
        }

        public static bool IsAdmin(string Nick, string password)
        {
            if (string.IsNullOrWhiteSpace(Nick))
                return false;

            bool users = false;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                var hash = GetMD5(password);

                var sql = string.Format(
@" SELECT  COUNT(`u`.`id`) 
 FROM `{0}`.`admins` AS `u` 
 WHERE `u`.`nickname` = '{1}' AND `u`.`pass` = '{2}' ; ", DB, Nick, hash);

                conn.Open();

                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read() && !dataReader.IsDBNull(0))
                {
                    users = dataReader.GetInt32(0) > 0;
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return users;
        }

        public static bool NickExist(string Nick)
        {
            int result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                string selectSql = string.Format(
@" SELECT count(u.id)
  FROM `{0}`.`users` AS `u`
  WHERE `u`.`nickname` = '{1}'  ; ", DB, Nick);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                reader.Read();
                result = reader.GetInt32(0);
                reader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result > 0;
        }

        public static bool MailExist(string mail)
        {
            int result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                string selectSql = string.Format(
@" SELECT count(u.id)
  FROM `{0}`.`users` AS `u`
  WHERE `u`.`mail` = '{1}'  ; ", DB, mail);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                reader.Read();
                result = reader.GetInt32(0);
                reader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result > 0;
        }

        public static string ChangePassword(string Nick, string ExamPass)
        {
            string result = null;
            var conn = new MySqlConnection(ConnStr);

            conn.Open();
            try
            {
                string selectSql = string.Format(
@" SELECT count(u.id), `u`.`id`
  FROM `{0}`.`users` AS `u`
  WHERE `u`.`nickname` = '{1}' AND `u`.`exam_pass` = '{2}'; ", DB, Nick, ExamPass);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                reader.Read();
                var users = reader.GetInt32(0);

                int user_id = -1;
                if (!reader.IsDBNull(1))
                    user_id = reader.GetInt32(1);

                reader.Close();

                if (users <= 0)
                {
                    throw new Exception();
                }

                string newPassword = GenerateCode(8);

                var addSql = string.Format(
@" UPDATE `{0}`.`users`  AS `u` SET
 `pass`= '{3}'
 WHERE 
 `u`.`nickname` = '{1}' AND `u`.`exam_pass` = '{2}'; ", DB, Nick, ExamPass, GetMD5(newPassword));

                var addCommand = new MySqlCommand(addSql, conn);
                int res = addCommand.ExecuteNonQuery();
                if (res > 0)
                    result = newPassword;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        public static bool AddFile(string Nick, int stage, string DataName, string DataPath, bool isFile)
        {
            if (string.IsNullOrWhiteSpace(Nick) || stage < 1
                || string.IsNullOrWhiteSpace(DataName) || string.IsNullOrWhiteSpace(DataPath))
                return false;

            int result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                //if (user != "AutoAdmin"/*!UserIsAdmin(user, conn)*/)
                //    throw new Exception("Unauthorize operation!");

                //var numberFormatInfo = new System.Globalization.CultureInfo("en-Us", false).NumberFormat;
                //numberFormatInfo.NumberGroupSeparator = "";
                //numberFormatInfo.NumberDecimalSeparator = ".";
                string sqlFile = isFile ? "true" : "false";
                string selectSql = string.Format(
@" SELECT count(f.id), `f`.`id_user`
  FROM `{0}`.`users_files` AS `f`
  LEFT JOIN  `{0}`.`users` AS `u` ON `u`.`id` = `f`.`id_user`
  WHERE `u`.`nickname` = '{1}' AND `f`.`stage` = {2} AND `f`.`name` = '{3}' AND `f`.`is_file` = {4} ; ", DB, Nick, stage, DataName, sqlFile);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                reader.Read();
                var files = reader.GetInt32(0);
                var id_user = -1;
                if (!reader.IsDBNull(1))
                    id_user = reader.GetInt32(1);
                reader.Close();

                string addSql;

                DataPath = DataPath.Replace("\\", "\\\\");

                if (files > 0)
                {
                    addSql = string.Format(
@" UPDATE `{0}`.`users_files`  AS `f` SET
 `path`= '{5}'
 WHERE 
 `f`.`id_user` = '{1}' AND `f`.`stage` = {2} AND `f`.`name` = '{3}' AND `f`.`is_file` = {4} ; ", DB, id_user, stage, DataName, sqlFile, DataPath);
                }
                else
                {
                    selectSql = string.Format(
    @" SELECT `u`.`id`
  FROM `{0}`.`users` AS `u`
  WHERE `u`.`nickname` = '{1}'; ", DB, Nick);

                    //              o.id_terminal = {terminal} AND o.RNN = '';";

                    selectCommand = new MySqlCommand(selectSql, conn);
                    reader = selectCommand.ExecuteReader();
                    reader.Read();
                    id_user = reader.GetInt32(0);
                    reader.Close();

                    addSql = string.Format(
@" INSERT INTO
 `{0}`.`users_files`
 (`id_user`,`stage`,`path`,`name`,`is_file`)
 VALUES
 ({1},{2},'{3}','{4}',{5}) ", DB, id_user, stage, DataPath, DataName, sqlFile);
                }
                var addCommand = new MySqlCommand(addSql, conn);

                result = addCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result > 0;

        }

        public static List<File> GetFiles(string Nick, int stage, MySqlConnection contextConn = null)
        {
            List<File> result = new List<File>();
            var conn = contextConn ?? new MySqlConnection(ConnStr);
            try
            {
                string sql = string.Format(
@" SELECT 
 `f`.`id`, `f`.`id_user`, `f`.`path`, `f`.`name`, `f`.`is_file`
 FROM `{0}`.`users_files` AS f 
  LEFT JOIN  `{0}`.`users` AS `u` ON `u`.`id` = `f`.`id_user`
  WHERE `u`.`nickname` = '{1}' AND `f`.`stage` = {2} ; ", DB, Nick, stage);

                if (contextConn == null)
                    conn.Open();

                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read() && !dataReader.IsDBNull(0))
                {
                    var f = new File();
                    f.Id = dataReader.GetInt32(0);
                    f.IdUser = dataReader.GetInt32(1);
                    f.Stage = stage;
                    f.Path = dataReader.GetString(2);
                    f.Name = dataReader.GetString(3);
                    f.IsFile = dataReader.GetBoolean(4);
                    result.Add(f);
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
                //result = 0;
            }
            finally
            {
                if (contextConn == null)
                    conn.Close();
            }
            return result;
        }

        public static File GetFile(int id, MySqlConnection contextConn = null)
        {
            File result = new File();
            var conn = contextConn ?? new MySqlConnection(ConnStr);
            try
            {
                string sql = string.Format(
@" SELECT 
 `f`.`id`, `f`.`id_user`, `f`.`stage`, `f`.`path`, `f`.`name`, `f`.`is_file`
 FROM `{0}`.`users_files` AS f 
  WHERE  `f`.`id` = {1} ; ", DB, id);

                if (contextConn == null)
                    conn.Open();

                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read() && !dataReader.IsDBNull(0))
                {
                    result = new File();
                    result.Id = dataReader.GetInt32(0);
                    result.IdUser = dataReader.GetInt32(1);
                    result.Stage = dataReader.GetInt32(2);
                    result.Path = dataReader.GetString(3);
                    result.Name = dataReader.GetString(4);
                    result.IsFile = dataReader.GetBoolean(5);
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
                //result = 0;
            }
            finally
            {
                if (contextConn == null)
                    conn.Close();
            }
            return result;
        }

        public static bool DeleteFile(int fid)
        {
            int result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                string selectSql = string.Format(
@" DELETE f FROM `{0}`.`users_files` AS f 
   WHERE `f`.`id` = {1}  ; ", DB, fid);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                var selectCommand = new MySqlCommand(selectSql, conn);
                result = selectCommand.ExecuteNonQuery();
                //reader.Read();
                //result = reader.GetInt32(0);
                //reader.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nick"></param>
        /// <param name="Fname"></param>
        /// <param name="Lname"></param>
        /// <param name="Midname"></param>
        /// <param name="City"></param>
        /// <param name="Telephone"></param>
        /// <param name="Mail"></param>
        /// <param name="Work"></param>
        /// <param name="Status"></param>
        /// <param name="Deviz"></param>
        /// <param name="Parties"></param>
        /// <param name="PolitExp"></param>
        /// <param name="Alcohol"></param>
        /// <param name="Smoking"></param>
        /// <param name="Religions"></param>
        /// <returns>null - nick exist, false - error, true - added</returns>
        public static AddUserError AddUser(
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
                    string Religions
            )
        {
            if (string.IsNullOrWhiteSpace(Nick)
                || string.IsNullOrWhiteSpace(Pass)
                || string.IsNullOrWhiteSpace(Fname)
                || string.IsNullOrWhiteSpace(Lname)
                || string.IsNullOrWhiteSpace(Midname)
                || string.IsNullOrWhiteSpace(City)
                || string.IsNullOrWhiteSpace(Telephone)
                || string.IsNullOrWhiteSpace(Mail)
                || string.IsNullOrWhiteSpace(Work)
                || string.IsNullOrWhiteSpace(Status)
                || string.IsNullOrWhiteSpace(Deviz)
                || string.IsNullOrWhiteSpace(Parties)
                || string.IsNullOrWhiteSpace(PolitExp)
                || string.IsNullOrWhiteSpace(Religions)
                )
                return AddUserError.AnyDataEmpty;

            int result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                //if (user != "AutoAdmin"/*!UserIsAdmin(user, conn)*/)
                //    throw new Exception("Unauthorize operation!");

                //var numberFormatInfo = new System.Globalization.CultureInfo("en-Us", false).NumberFormat;
                //numberFormatInfo.NumberGroupSeparator = "";
                //numberFormatInfo.NumberDecimalSeparator = ".";

                string sqlFemale = Female ? "true" : "false";
                string sqlAlcohol = Alcohol ? "true" : "false";
                string sqlSmoking = Smoking ? "true" : "false";
                string selectSql = string.Format(
@" SELECT count(u.id)
  FROM `{0}`.`users` AS `u`
  WHERE `u`.`nickname` = '{1}'  ; ", DB, Nick);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                reader.Read();
                var users = reader.GetInt32(0);
                reader.Close();

                if (users > 0)
                {
                    conn.Close();
                    return AddUserError.NickExist;
                }
                users = 0;
                selectSql = string.Format(
@" SELECT count(u.id)
  FROM `{0}`.`users` AS `u`
  WHERE `u`.`mail` = '{1}'  ; ", DB, Mail);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                selectCommand = new MySqlCommand(selectSql, conn);
                reader = selectCommand.ExecuteReader();
                reader.Read();
                users = reader.GetInt32(0);
                reader.Close();

                if (users > 0)
                {
                    conn.Close();
                    return AddUserError.MailExist;
                }

                string password = GetMD5(Pass);

                string examPassword = GenerateCode(32);

                string addSql = string.Format(
@" INSERT INTO
 `{0}`.`users`
(`nickname`, `fname`, `lname`, `midname`, `city`,
  `telephone`, `mail`, `work`, `status`, `deviz`, `parties`, `polit_exp`, `female`, `alcohol`, `smoking`, `religions`,
   `exam_pass`,  `pass`) VALUES 
 ('{1}','{2}','{3}','{4}','{5}',
  '{6}','{7}','{8}','{9}','{10}','{11}','{12}',{13},{14},{15},'{16}',
  '{17}','{18}') ", DB,
  Nick, Fname, Lname, Midname, City,
  Telephone, Mail, Work, Status, Deviz, Parties, PolitExp, sqlFemale, sqlAlcohol, sqlSmoking, Religions,
  examPassword, password);
                var addCommand = new MySqlCommand(addSql, conn);

                result = addCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result > 0 ? AddUserError.AddSucess : AddUserError.SQLError;

        }

        public static bool SetToExamStage(string Nick, int ExamStage)
        {
            int result = 0;
            var conn = new MySqlConnection(ConnStr);

            conn.Open();
            try
            {
                string selectSql = string.Format(
@" SELECT count(u.id), `u`.`stage`
  FROM `{0}`.`users` AS `u`
  WHERE `u`.`nickname` = '{1}' AND `u`.`stage` = {2} ; ", DB, Nick, ExamStage);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                reader.Read();
                var users = reader.GetInt32(0);

                //int stage = -1;
                //if (!reader.IsDBNull(1))
                //    stage = reader.GetInt32(1);

                reader.Close();

                if (users <= 0 || ExamStage <= 0)
                {
                    throw new Exception();
                }

                string examPassword = GenerateCode(32);

                string addSql = string.Format(
@" UPDATE `{0}`.`users`  AS `u` SET
 `state`= 1, `u`.`exam_pass` = '{3}'
 WHERE 
 `u`.`nickname` = '{1}' AND `u`.`stage` = {2}  ; ", DB, Nick, ExamStage, examPassword);

                var addCommand = new MySqlCommand(addSql, conn);
                result = addCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result > 0;

        }

        public static bool SetExamError(string Nick, int ExamStage, string ExamPass, string msg)
        {
            int result = 0;
            var conn = new MySqlConnection(ConnStr);

            conn.Open();
            try
            {
                string selectSql = string.Format(
@" SELECT count(u.id), `u`.`id`
  FROM `{0}`.`users` AS `u`
  WHERE `u`.`nickname` = '{1}' AND  `u`.`state` = 1 AND `u`.`stage` = {2} AND `u`.`exam_pass` = '{3}'; ", DB, Nick, ExamStage, ExamPass);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                reader.Read();
                var users = reader.GetInt32(0);

                int user_id = -1;
                if (!reader.IsDBNull(1))
                    user_id = reader.GetInt32(1);

                reader.Close();

                if (users <= 0 || ExamStage <= 0)
                {
                    throw new Exception();
                }


                selectSql = string.Format(
@" SELECT count(h.id)
  FROM `{0}`.`users_history` AS h
  WHERE `h`.`stage` = '{1}' AND  `h`.`id_user` = {2} ; ", DB, ExamStage, user_id);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                selectCommand = new MySqlCommand(selectSql, conn);
                reader = selectCommand.ExecuteReader();
                reader.Read();
                var histories = reader.GetInt32(0);
                reader.Close();

                if (histories > 0)
                {
                    throw new Exception();
                }

                var addSql = string.Format(
 @" INSERT INTO `{0}`.`users_history`
   (`stage`, `score`, `id_user`, `msg`) VALUES
   ( {1}, 0, {2}, '{3}') ; ", DB, ExamStage, user_id, msg);

                var addCommand = new MySqlCommand(addSql, conn);
                var result1 = addCommand.ExecuteNonQuery();

                if (result1 <= 0)
                {
                    throw new Exception();
                }

                string newExamPassword = GenerateCode(32);

                addSql = string.Format(
@" UPDATE `{0}`.`users`  AS `u` SET
 `state`=2, exam_pass = '{4}'
 WHERE 
 `u`.`nickname` = '{1}' AND  `u`.`state` = 1 AND `u`.`stage` = {2} AND `u`.`exam_pass` = '{3}'; ", DB, Nick, ExamStage, ExamPass, newExamPassword);

                addCommand = new MySqlCommand(addSql, conn);
                result = addCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result > 0;

        }

        public static bool EndExamStage(string Nick, int ExamStage, int Score, string ExamPass, string msg)
        {
            int result = 0;
            var conn = new MySqlConnection(ConnStr);

            conn.Open();
            try
            {
                string selectSql = string.Format(
@" SELECT count(u.id), `u`.`id`
  FROM `{0}`.`users` AS `u`
  WHERE `u`.`nickname` = '{1}' AND  `u`.`state` = 1 AND `u`.`stage` = {2} AND `u`.`exam_pass` = '{3}'; ", DB, Nick, ExamStage, ExamPass);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                reader.Read();
                var users = reader.GetInt32(0);

                int user_id = -1;
                if (!reader.IsDBNull(1))
                    user_id = reader.GetInt32(1);

                reader.Close();

                if (users <= 0 || ExamStage < 0 || Score < 0)
                {
                    throw new Exception();
                }


                selectSql = string.Format(
@" SELECT count(h.id)
  FROM `{0}`.`users_history` AS h
  WHERE `h`.`stage` = '{1}' AND  `h`.`id_user` = {2} ; ", DB, ExamStage, user_id);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                selectCommand = new MySqlCommand(selectSql, conn);
                reader = selectCommand.ExecuteReader();
                reader.Read();
                var histories = reader.GetInt32(0);
                reader.Close();

                if (histories > 0)
                {
                    throw new Exception();
                }


                var addSql = string.Format(
 @" INSERT INTO `{0}`.`users_history`
   (`stage`, `score`, `id_user`, `msg`) VALUES
   ( {1}, {2}, {3}, '{4}') ", DB, ExamStage, Score, user_id, msg);

                var addCommand = new MySqlCommand(addSql, conn);
                var result1 = addCommand.ExecuteNonQuery();

                if (result1 <= 0)
                {
                    throw new Exception();
                }

                //string newExamPassword = GenerateCode(32);
                /*, exam_pass = '{5}'*/
                addSql = string.Format(
@" UPDATE `{0}`.`users`  AS `u` SET
 `stage`= {4}, `state`=0
 WHERE 
 `u`.`nickname` = '{1}' AND  `u`.`state` = 1 AND `u`.`stage` = {2} AND `u`.`exam_pass` = '{3}'; ", DB, Nick, ExamStage, ExamPass, ExamStage + 1/*, newExamPassword*/);

                addCommand = new MySqlCommand(addSql, conn);
                result = addCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result > 0;
        }

        public static List<User> UsersRating(MySqlConnection contextConn = null)
        {
            List<User> result = new List<User>();
            var conn = contextConn ?? new MySqlConnection(ConnStr);
            try
            {
                string sql = string.Format(
@" SELECT 
 `u`.`id`, `u`.`nickname`, `u`.`female`, `u`.`fname`, `u`.`lname`, `u`.`midname`, `u`.`city`, `u`.`mail`, `u`.`telephone`, `u`.`stage`, `u`.`state`, `u`.`reward`
 FROM `{0}`.`users_rating` AS u;  ", DB);

                if (contextConn == null)
                    conn.Open();

                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read() && !dataReader.IsDBNull(0))
                {
                    var us = new User();
                    us.Id = dataReader.GetInt32(0);
                    us.Nick = dataReader.GetString(1);
                    us.Female = dataReader.GetBoolean(2);
                    us.Fname = dataReader.GetString(3);
                    us.Lname = dataReader.GetString(4);
                    us.Midname = dataReader.GetString(5);
                    us.City = dataReader.GetString(6);
                    us.Mail = dataReader.GetString(7);
                    us.Telephone = dataReader.GetString(8);
                    us.Stage = dataReader.GetInt32(9);
                    us.State = (TaskStates)dataReader.GetInt32(10);
                    if (!dataReader.IsDBNull(11))
                        us.Score = dataReader.GetInt32(11);
                    result.Add(us);
                }
                dataReader.Close();

            }
            catch (Exception ex)
            {
                //result = 0;
            }
            finally
            {
                if (contextConn == null)
                    conn.Close();
            }
            return result;
        }

        public static User GetUser(string name, MySqlConnection contextConn = null)
        {
            User result = null;
            var conn = contextConn ?? new MySqlConnection(ConnStr);
            try
            {
                string sql = string.Format(
@" SELECT 
 `u`.`id`, `u`.`nickname`, `u`.`fname`, `u`.`lname`, `u`.`midname`, `u`.`city`, 
 `u`.`telephone`, `u`.`mail`, `u`.`work`, `u`.`status`, `u`.`deviz`, `u`.`parties`, `u`.`polit_exp`, `u`.`female`, `u`.`alcohol`, `u`.`smoking`, `u`.`religions`,
 `u`.`stage`, `u`.`state`, `u`.`exam_pass`, `ur`.`reward`
 FROM `{0}`.`users` AS u
 LEFT JOIN `{0}`.`users_rating` AS `ur` ON u.id = ur.id
 WHERE `u`.`nickname` = '{1}' ;  ", DB, name);

                if (contextConn == null)
                    conn.Open();

                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read() && !dataReader.IsDBNull(0))
                {
                    result = new User();

                    result.Id = dataReader.GetInt32(0);
                    result.Nick = dataReader.GetString(1);
                    result.Fname = dataReader.GetString(2);
                    result.Lname = dataReader.GetString(3);
                    result.Midname = dataReader.GetString(4);
                    result.City = dataReader.GetString(5);
                    result.Telephone = dataReader.GetString(6);
                    result.Mail = dataReader.GetString(7);
                    result.Work = dataReader.GetString(8);
                    result.Status = dataReader.GetString(9);
                    result.Deviz = dataReader.GetString(10);
                    result.Parties = dataReader.GetString(11);
                    result.PolitExp = dataReader.GetString(12);
                    result.Female = dataReader.GetBoolean(13);
                    result.Alcohol = dataReader.GetBoolean(14);
                    result.Smoking = dataReader.GetBoolean(15);
                    result.Religions = dataReader.GetString(16);
                    result.Stage = dataReader.GetInt32(17);
                    result.State = (TaskStates)dataReader.GetInt32(18);
                    if (!dataReader.IsDBNull(19))
                        result.ExamPass = dataReader.GetString(19);
                    result.Score = dataReader.GetInt32(20);
                }
                dataReader.Close();
            }
            catch (Exception ex)
            {
                //result = 0;
            }
            finally
            {
                if (contextConn == null)
                    conn.Close();
            }
            return result;
        }

        public static List<LevelScore> GetLevels(string Nick, MySqlConnection contextConn = null)
        {
            List<LevelScore> result = new List<LevelScore>();
            var conn = contextConn ?? new MySqlConnection(ConnStr);
            try
            {
                string sql = string.Format(
@" SELECT  `h`.`stage`  , SUM(`h`.`score`) 
 FROM `{0}`.`users_history` AS `h`
 LEFT JOIN  `{0}`.`users` As `u` ON `u`.`id` = `h`.`id_user`
 WHERE `u`.`nickname` = '{1}'
   GROUP BY `h`.`stage` ; ", DB, Nick);

                if (contextConn == null)
                    conn.Open();

                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read() && !dataReader.IsDBNull(0))
                {
                    var us = new LevelScore();
                    us.Number = dataReader.GetInt32(0);
                    us.Score = dataReader.GetInt32(1);
                    result.Add(us);
                }
                dataReader.Close();

            }
            catch (Exception ex)
            {
                //result = 0;
            }
            finally
            {
                if (contextConn == null)
                    conn.Close();
            }
            return result;
        }

        public static bool AddOpponent(int UserId, int OpponentId, int Stage)
        {
            if (UserId < 1 || OpponentId < 1 || Stage < 1)
                return false;

            int result = 0;
            var conn = new MySqlConnection(ConnStr);
            try
            {
                conn.Open();

                string selectSql = string.Format(
@" SELECT count(`o`.`id_user`)
  FROM `{0}`.`users_opponents` AS `o`
  WHERE `o`.`id_user` = '{1}' AND `o`.`stage` = {2} AND `o`.`id_opponent` = {3}  ; ", DB, UserId, Stage, OpponentId);

                //              o.id_terminal = {terminal} AND o.RNN = '';";

                var selectCommand = new MySqlCommand(selectSql, conn);
                var reader = selectCommand.ExecuteReader();
                reader.Read();
                var files = reader.GetInt32(0);
                reader.Close();

                string addSql;

                if (files > 0)
                    return false;

                {
                    addSql = string.Format(
@" INSERT INTO
 `{0}`.`users_opponents`
 (`id_user`, `stage`, `id_opponent`)
 VALUES
 ({1},{2},{3}) ", DB, UserId, Stage, OpponentId);
                }
                var addCommand = new MySqlCommand(addSql, conn);

                result = addCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                conn.Close();
            }
            return result > 0;
        }

        public static List<UserOpponent> GetOpponentls(string Nick, int Stage, MySqlConnection contextConn = null)
        {
            List<UserOpponent> result = new List<UserOpponent>();
            var conn = contextConn ?? new MySqlConnection(ConnStr);
            try
            {
                string sql = string.Format(
@" SELECT  `u1`.`nickname` , `o`.`stage`, `u2`.`nickname`, `u2`.`stage`, `u2`.`mail`, `u2`.`reward`   
 FROM `{0}`.`users_opponents` AS `o`
 LEFT JOIN  `{0}`.`users` As `u1` ON `u1`.`id` = `o`.`id_user`
 LEFT JOIN  `{0}`.`users_rating` As `u2` ON `u2`.`id` = `o`.`id_opponent`
WHERE `u1`.`nickname` = '{1}' ; ", DB, Nick);

                if (contextConn == null)
                    conn.Open();

                var countCommand = new MySqlCommand(sql, conn);

                var dataReader = countCommand.ExecuteReader();
                while (dataReader.HasRows && dataReader.Read() && !dataReader.IsDBNull(0))
                {
                    var us = new UserOpponent();
                    us.UserNick = dataReader.GetString(0);
                    us.Stage = dataReader.GetInt32(1);
                    us.OpponentNick = dataReader.GetString(2);
                    us.OpponentStage = dataReader.GetInt32(3);
                    us.OpponentMail = dataReader.GetString(4);
                    if (!dataReader.IsDBNull(5))
                        us.OpponentRating = dataReader.GetInt32(5);

                    result.Add(us);
                }
                dataReader.Close();

            }
            catch (Exception ex)
            {
                //result = 0;
            }
            finally
            {
                if (contextConn == null)
                    conn.Close();
            }
            return result;
        }

        private static string GenerateCode(int length)
        {
            string result;
            {
                string s1, st;
                result = "";
                Random rnd = new Random();
                int n;
                st = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                for (int j = 0; j < length; j++)
                {
                    n = rnd.Next(0, 61);
                    s1 = st.Substring(n, 1);
                    result += s1;
                }
            }
            return result;
        }

        private static string GetMD5(string src)
        {
            string result;
            using (var md5Hash = MD5.Create())
            {
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(src));
                var sBuilder = new StringBuilder();
                foreach (var t in data)
                {
                    sBuilder.Append(t.ToString("x2"));
                }
                result = sBuilder.ToString();
            }
            return result;
        }
    }
}
