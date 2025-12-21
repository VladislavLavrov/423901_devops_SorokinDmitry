namespace SortRollWebApp.Models
{
    /// <summary>
    /// Параметры каждого прохода в прокате
    /// </summary>
    public class PassRollingParameters
    {
        // Расчетные величины блок скорости
        public double V1 { get; set; } // Скорость прокатки
        public double DK { get; set; } // Катающий диаметр
        public double NR { get; set; } // Расчетная частота вращения
        public double NN { get; set; } // Номинальная частота вращения валков
        public double NMIN { get; set; } // Минимальная частота вращения валков
        public double NMAX { get; set; } // Максимальная частота вращения валков
        public double VMIN { get; set; } // Минимальная скорость валков
        public double VMAX { get; set; } // Максимальная скорость валков
        public double KSI { get; set; } // Скорость деформации

        // скорость2
        public int KODP { get; set; } // Определение признака привода

        // tempsig температура и сопротивление деформации
        public double LP { get; set; } // Длина полосы
        public double SIGSP { get; set; } // Сопротивление деформации на переднем конце полосы
        public double SIGSZ { get; set; } // То же но на заднем конце
        public double SIGSSR { get; set; } // Среднее сопротивление деформации
        public double TP { get; set; } // Время движения переднего конца
        public double TZ { get; set; } // То же но заднего конца
        public double TM { get; set; } // Машинное время
        public double DTLP { get; set; } // Потери лучеиспускания на переднем конце
        public double DTLZ { get; set; } // Также на заднем конце
        public double DTDP { get; set; } // Прирост от деформации на переднем конце
        public double DTDZ { get; set; } // Также на заднем
        public double DTDSR { get; set; } // Средний прирост от деформации
        public double DTP { get; set; } // Общие потери на переднем конце
        public double DTZ { get; set; } // Также на заднем
        public double T1P { get; set; } // Температура переднего конца раската
        public double T1Z { get; set; } // Также но заднего конца
        public double TSR { get; set; } // Средняя темпеература полосы
        public double PSITR { get; set; } // Показатель трения

        // Вспомогательные параметры расчитываются в функции int razmer(int sxema,int i)  
        public double BK { get; set; } // Ширина калибра  
        public double HVR { get; set; } // Глубина вреза ручья  
        public double HG { get; set; } // Геометрическая высота калибра  
        public double PER { get; set; } // Периметр полосы  
        public int KOD0 { get; set; } // Код формы заготовки  
        public int KOD1 { get; set; } // Код формы калибра  
        public double DD { get; set; } // Диаметр валков по дну калибра  

        // Параметры , расчитываемые в функции int Parameter(int schema,int i)  
        public double A0 { get; set; } // Исходное отношение осей полосы  
        public double A1 { get; set; } // Отношение осей полосы в проходе  
        public double AK { get; set; } // Отношение осей калибра  
        public double AZ { get; set; } // Степень защемления полосы  
        public double DEL0 { get; set; } // Исходная степень заполнения калибра  
        public double DEL1 { get; set; } // Степень заполнения калибра  
        public double TANFI { get; set; } // Выпуск калибра  
        public double A { get; set; } // Приведенный диаметр валков  
        public double KOBJ { get; set; } // Коэфф. обжатия  
        public double KVIT { get; set; } // Коэф. вытяжки валков  
        public double E { get; set; } // Относительное обжатие  
        public double DELV { get; set; } // Степень заполнения выпуска калибра  
        public double KUSH { get; set; } // Коэфф. уширения  

        // Расчет площади поперечного сечения  
        public double WR { get; set; } // Расчетная площадь поперечного сечения полосы  
        public double KVITR { get; set; } // Расчетный коэфф. вытяжки  

        // Расчет уширения с учетом марки стали  
        public double BETAR { get; set; } // Расчетный коэф. уширения  
        public double KBETAP { get; set; } // Поправка на коэф. уширения по переднему концу полосы  
        public double KBETAZ { get; set; } // Поправка на коэф. уширения по заднему концу полосы  
        public double BETASTP { get; set; } // Коэф. уширения по переднему концу полосы  
        public double BETASTZ { get; set; } // Коэф. уширения по заднему концу полосы  
        public double SIGSBP { get; set; } // Сопротивление деформации стали Ст3 по переднему концу полосы  
        public double SIGSBZ { get; set; } // Сопротивление деформации стали Ст3 по заднему концу полосы  
        public double B1RP { get; set; } // Расчетная ширина по переднему концу полосы  
        public double B1RZ { get; set; } // Расчетная ширина по заднему концу полосы  
        public double DELOP { get; set; } // Опытная степень заполнения калибра по врезу  
        public double DELRP { get; set; } // Расчетная степень заполнения калибра по врезу для переднего конца  
        public double DELRZ { get; set; } // Расчетная степень заполнения калибра по врезу для заднего конца  

        // Расчет абсолютных параметров очага деформации  
        public double DELH { get; set; } // Абсолютное обжатие  
        public double DELB { get; set; } // Абсолютное уширение  
        public double UGZAX { get; set; } // Угол захвата  
        public double LOD { get; set; } // Длина очага деформации  

        // Расчет допустимых углов захвата и отношений осей полосы  
        public double ALPHA { get; set; } // Допустимый угол захвата  
        public double ADOP { get; set; } // Допустимое отношение осей  

        // Расчет энергосиловых параметров  
        public double FKON { get; set; } // Площадь контактной поверхности  
        public double NSIG { get; set; } // Коэфф. напряженного состояния  
        public double LH { get; set; } // Показатель формы очага деформации  
        public double NVAL { get; set; } // Коэфф. мощности деформации  
        public double PSRP { get; set; } // Среднее давление на переднем конце  
        public double PSRZ { get; set; } // Среднее давление на заднем конце  
        public double P1P { get; set; } // Усилие на переднем конце  
        public double P1Z { get; set; } // Усилие на заднем конце  
        public double KPP { get; set; } // Коэффициент загрузки по усилию на переднем конце  
        public double KPZ { get; set; } // Коэффициент загрузки по усилию на заднем конце  
        public double MDP { get; set; } // Момент деформации на переднем конце  
        public double MDZ { get; set; } // Момент деформации на заднем конце  
        public double MTRP { get; set; } // Момент трения на переднем конце  
        public double MTRZ { get; set; } // Момент трения на заднем конце  
        public double MPRP { get; set; } // Момент прокатки на переднем конце  
        public double MPRZ { get; set; } // Момент прокатки на заднем конце  
        public double NPRP { get; set; } // Мощность на переднем конце  
        public double KMP { get; set; } // Коэфф. загрузки по моменту на переднем конце  
        public double KMZ { get; set; } // Коэфф. загрузки по моменту на заднем конце  
        public double NPRZ { get; set; } // Мощность на заднем конце  
        public double RP { get; set; } // Максимальная реакция на переднем конце  
        public double RZ { get; set; } // Максимальная реакция на заднем конце  
        public double MIP { get; set; } // Момент приведенный к двигателю на переднем конце  
        public double MIZ { get; set; } // Момент приведенный к двигателю на заднем конце  
        public double MISUM { get; set; } // Суммарный момент на двигателе группового привода  
        public double MISUMP { get; set; } // Суммарный момент на двигателе группового привода передний конец  
        public double MISUMZ { get; set; } // Суммарный момент на двигателе группового привода задний  
        public double KDVP { get; set; } // Коэф. загрузки двигателя на переднем конце  
        public double KDVZ { get; set; } // Коэф. загрузки двигателя на заднем конце  
        public double MDV { get; set; } // Момент, развиваемый двигателем при данных оборотах  
        public double JJ { get; set; } // Номер электродвигателя  
        public double NDVR { get; set; } // Рабочая частота вращения двигателя  
        public double WWP { get; set; } // Затраты энергии по переднему концу  
        public double WWZ { get; set; } // Затраты энергии по заднему концу  
        public double PLECHO { get; set; } // Плечо усилия прокатки  

        // Расчет энергосиловых параметров по методу соответственной полосы sp_polosa  
        public double B1SP { get; set; } // Ширина соответственной полосы после прокатки  
        public double H1SP { get; set; } // Высота соответственной полосы после прокатки  
        public double B0SP { get; set; } // Ширина соответственной полосы до прокатки  
        public double H0SP { get; set; } // Высота соответственной полосы до прокатки  
        public double ESP { get; set; } // Относительное обжатие соответственной полосы  
        public double LSP { get; set; } // Длина очага деформации  
        public double NRSP { get; set; } // Частота вращения валков  
        public double DKSP { get; set; } // Катающий диаметр  
        public double KSISP { get; set; } // Скорость деформации  
        public double SIGSSPP { get; set; } // Сопротивление деформации передний конец  
        public double SIGSSPZ { get; set; } // Сопротивление деформации задний конец  
        public double DELSP { get; set; }  // Параметр формы очага деформации  
        public double MUSP { get; set; }   // Коэфф. трения по Гету  
        public double NSIGSP { get; set; } // Коэф. напряженного состояния  
        public double HSRSP { get; set; }  // Средняя высота полосы  
        public double LHSR { get; set; }   // Фактор формы  
        public double PSRSPP { get; set; } // Среднее давление передний конец  
        public double PSRSPZ { get; set; } // Среднее давление задний конец  
        public double FKONSP { get; set; } // Контактная поверхность  
        public double P1SPP { get; set; }  // Усилие передний конец  
        public double P1SPZ { get; set; }  // Усилие задний конец  
        public double RSPP { get; set; }   // Реакция передний конец  
        public double RSPZ { get; set; }   // Реакция задний конец  
        public double KPSPP { get; set; }  // Коэфф. загрузки по усилию передний конец  
        public double KPSPZ { get; set; }  // Коэфф. загрузки по усилию задний конец  
        public double MTRSPP { get; set; } // Момент трения передний конец  
        public double MTRSPZ { get; set; } // Момент трения задний конец  
        public double MDSPP { get; set; }  // Момент деформации с учетом числа ниток передний конец  
        public double MDSPZ { get; set; }  // Момент деформации с учетом числа ниток задний конец  
        public double MPRSPP { get; set; } // Крутящий момент на валках передний конец  
        public double MPRSPZ { get; set; } // Крутящий момент на валках задний конец  
        public double KMSPP { get; set; }  // Коэф. загрузки по моменту передний конец  
        public double KMSPZ { get; set; }  // Коэф. загрузки по моменту задний конец  
        public double NPSPP { get; set; }  // Мощность прокатки передний конец  
        public double NPSPZ { get; set; }  // Мощность прокатки задний конец  
        public double WWSPP { get; set; }  // Энергия прокатки передний конец  
        public double WWSPZ { get; set; }  // Энергия прокатки задний конец  
        public double MISPP { get; set; }  // Момент на двигателе передний конец  
        public double MISPZ { get; set; }  // Момент на двигателе задний конец
        public double MISUMSPP { get; set; } // Суммарный момент на двигателе передний конец  
        public double MISUMSPZ { get; set; } // Суммарный момент на двигателе задний конец  
        public double KDVSPP { get; set; }   // Коэфф. загрузки двигателя передний конец  
        public double KDVSPZ { get; set; }   // Коэфф. загрузки двигателя задний конец  

        // Вспомогательные массивы, необходимые для подготовки отчетов  
        public double KP_ { get; set; } // Доп. - коэфф. загрузки по усилию  

        // Определение установочного зазора  
        public double SU { get; set; } // Установочный зазор по середине полосы  
        public double SUP { get; set; } // Установочный зазор по переднему концу полосы  
        public double SUZ { get; set; } // Установочный зазор по заднему концу полосы  
        public double FKLP { get; set; } // Упругая деформация клети по переднему концу полосы  
        public double FKLZ { get; set; } // Упругая деформация клети по заднему концу полосы  
        public double FKL { get; set; } // Среднее значение упругой деформации  
        public double NGSP { get; set; } // Коэфф. жестких концов  

        // Оптимизация по быстродействию  
        public double VRMAX { get; set; } // Максимальная расчетная скорость в проходе
    }
}
