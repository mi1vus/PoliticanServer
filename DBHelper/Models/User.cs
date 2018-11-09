using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHelper.Models
{
    public enum TaskStates 
    {
        Выполнение = 0,
        Проверка = 1,
        Ошибка = 2,
        Undefined
    }

    public class User
    {
        public int Id { get; set; }
        public string Nick { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Midname { get; set; }
        public string City { get; set; }
        public string Telephone { get; set; }
        public string Mail { get; set; }
        public string Work { get; set; }
        public string Status { get; set; }
        public string Deviz { get; set; }
        public string Parties { get; set; }
        public string PolitExp { get; set; }
        public bool Female { get; set; }
        public bool Alcohol { get; set; }
        public bool Smoking { get; set; }
        public string Religions { get; set; }
        public string ExamPass { get; set; }
        public int Stage { get; set; }
        public TaskStates State { get; set; }
        public int Score { get; set; }
    }

    public class UserOpponent
    {
        public string UserNick { get; set; }
        public int Stage { get; set; }
        public string OpponentNick { get; set; }
        public string OpponentMail { get; set; }
        public int OpponentStage { get; set; }
        public int OpponentRating { get; set; }
    }

}
