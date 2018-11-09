using System.Collections.Generic;
using DBHelper.Models;

namespace politician_server.Models
{
    public class Stage
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ViewUser : User
    {
        public string GroupsIdsString { get; set; }
        public string GroupsNamesString { get; set; }

        public ViewUser(User user)
        {
            if (user == null)
                return;

            Id = user.Id;
            Nick = user.Nick;
            Fname = user.Fname;
            Lname = user.Lname;
            Midname = user.Midname;
            City = user.City;
            Telephone = user.Telephone;
            Mail = user.Mail;
            Work = user.Work;
            Status = user.Status;
            Deviz = user.Deviz;
            Parties = user.Parties;
            PolitExp = user.PolitExp;
            Alcohol = user.Alcohol;
            Smoking = user.Smoking;
            Religions = user.Religions;
            ExamPass = user.ExamPass;
            Stage = user.Stage;
            State = user.State;
            Score = user.Score;
        }
    }

    public class UsersListViewModel
    {
        public IEnumerable<ViewUser> Users { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }

    public class UserDetailViewModel
    {
        public ViewUser User { get; set; }
        public IEnumerable<File> Files { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }

    //public class TerminalParametersViewModel
    //{
    //    public ViewTerminal Terminal { get; set; }
    //    public IEnumerable<Parameter> Parameters { get; set; }
    //}
}