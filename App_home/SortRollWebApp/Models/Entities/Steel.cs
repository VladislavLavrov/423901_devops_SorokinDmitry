using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using System.ComponentModel.DataAnnotations;

namespace SortRollWebApp.Models.Entities
{
    /// <summary>
    /// Модель данных таблицы
    /// </summary>
    public class Steel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public double SteelCode { get; set; } // Код стали для расчетов

        // Коэффициенты для расчета сопротивления деформации
        public double K1 { get; set; }
        public double K2 { get; set; }
        public double K3 { get; set; }
        public double K4 { get; set; }
        public double K5 { get; set; }
        public double K6 { get; set; }
        public double K7 { get; set; }
        public double K8 { get; set; }
        public double K9 { get; set; }
    }
}