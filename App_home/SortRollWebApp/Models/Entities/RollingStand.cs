using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortRollWebApp.Models.Entities
{
    /// <summary>
    /// Модель данных таблицы
    /// </summary>
    public class RollingStand
    {
        [Key]
        public int Id { get; set; }
        public int Number { get; set; } // Номер клети
        public double DB { get; set; } // Диаметр бочки валка
        public double DSH { get; set; } // Диаметр шейки валка
        public double MU { get; set; } // Состояние поверхности валков
        public double FPOD { get; set; } // Коэффициент трения в подшипниках
        public double LKL { get; set; } // Расстояние м-ду клетями
        public double AKL { get; set; } // Расстояние между станинами
        public double IR { get; set; } // Передаточное отношение редуктора
        public double ETA { get; set; } // КПД линии привода
        public double NZ { get; set; } // Опытная частота вращения валков
        public double NNOM { get; set; } // Номинальная мощность двигателя
        public double NDVN { get; set; } // Номинальная частота двигателя
        public double NDVMIN { get; set; } // Минимальная частота двигателя
        public double NDVMAX { get; set; } // Максимальная частота двигателя
        public double PP { get; set; } // Признак привода
        public double PDOP { get; set; } // Допустимое усилие прокатки
        public double MDOP { get; set; } // Допустимый крутящий момент
        public double C { get; set; } // Коэффициент жесткости

        [ForeignKey("RollingMill")]
        public int IdRollingMill { get; set; }

        // Навигационное свойство
        [ValidateNever]
        public virtual RollingMill RollingMill { get; set; }
    }
}
