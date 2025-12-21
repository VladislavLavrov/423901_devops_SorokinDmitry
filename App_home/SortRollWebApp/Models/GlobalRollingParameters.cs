namespace SortRollWebApp.Models
{
    /// <summary>
    /// Глобальные параметры процесса проката
    /// </summary>
    public class GlobalRollingParameters
    {
        // Управляющие переменные
        public double K10 { get; set; } // Количество составляющих теплового баланса
        public double K11 { get; set; } // Метод расчета сигма С
        public int K12 { get; set; } // Заданы ли фактические обороты валков
        public double K13 { get; set; } // Температурный режим задан
        public double K14 { get; set; } // Ввести расчет площадей
        public double K15 { get; set; } // Коэффициент вытяжки заданы
        public double K16 { get; set; } // Заполнение калибров
        public double K17 { get; set; } // Расчет размеров полос
        public double K18 { get; set; }
        public double K19 { get; set; } // Форма чистового
        public double K20 { get; set; }
        public int TS { get; set; }
        public int NPR { get; set; }
    }
}
