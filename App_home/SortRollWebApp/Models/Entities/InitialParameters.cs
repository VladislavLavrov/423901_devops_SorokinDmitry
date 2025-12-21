using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortRollWebApp.Models.Entities
{
    /// <summary>
    /// Модель данных таблицы
    /// </summary>
    public class InitialParameters
    {
        [Key]
        public int Id { get; set; }
        public double W0 { get; set; } // Площадь поперечного сечения заготовки
        public double P0 { get; set; } // Периметр заготовки
        public double L0 { get; set; } // Длина заготовки
        public double T0 { get; set; } // Температура выдачи из печи
        public double TAU { get; set; } // Время движения заготовки от печи к стану
        public double TAU0 { get; set; } // Пауза между прокатами полос в первом прокате
        public double LR { get; set; } // Запас частоты вращения (Переточка)
        public double VK { get; set; } // Конечная скорость прокатки
        public double T0min { get; set; } // Минимально возможная температура по технологии
        public double T0max { get; set; } // Максимально возможная температура по технологии
        
        [ForeignKey("SteelSection")]
        public int IdSteelSection { get; set; }

        // Навигационное свойство
        [ValidateNever]
        public virtual SteelSection SteelSection { get; set; }
    }
}
