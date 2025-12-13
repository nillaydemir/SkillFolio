using System;
using System.Collections.Generic;

namespace SkillFolio.ViewModels
{
    public class CalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }

        // Key = gün (1–31), Value = o gündeki event başlıkları
        public Dictionary<int, List<string>> EventsByDay { get; set; }
            = new Dictionary<int, List<string>>();
    }
}
