using System;

namespace WPFExcelView
{
    public class UBI
    {
        private string violationConf;
        private string violationInteg;
        private string violationAcc;
        public string Id { get; set; }
        public string Update { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SourceOfThreat { get; set; }
        public string ObjectOfInfluence { get; set; }
        public string ViolationConf
        {
            get
            {
                return violationConf;
            }
            set
            {
                violationConf = Validation(value);
            }
        }
        public string ViolationInteg
        {
            get
            {
                return violationInteg;
            }
            set
            {
                violationInteg = Validation(value);
            }
        }
        public string ViolationAcc
        {
            get
            {
                return violationAcc;
            }
            set
            {
                violationAcc = Validation(value);
            }
        }

        string Validation(string s) // Нужно чтобы не городить огромное присвоение в распарсинге таблицы
        {
            if (s == "0")
            {
                return "Нет";
            }
            if (s == "1")
            {
                return "Да";
            }
            if (s == "Нет") // Следущие 2 проверки сделаны для окна update, иначе там пропуски будут
            {
                return "Нет";
            }
            if (s == "Да")
            {
                return "Да";
            }
            return "";
        }
    }
}
