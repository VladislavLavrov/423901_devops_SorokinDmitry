using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortRollWebApp.Models.Entities
{
    /// <summary>
    /// Модель данных таблицы
    /// </summary>
    public class Pass
    {
        [Key]
        public int Id { get; set; }
        public int N { get; set; } // Номер прохода
        public int SchemaInput {  get; set; } // Код формы подката
        public int SchemaCaliber { get; set; } // Код формы выходного калибра
        //public int Schema => SchemaInput * 10 + SchemaCaliber; // Вычисляемый код схемы проката
        
        [NotMapped]
        public int Schema
        {
            get
            {
                if (SchemaCaliber > 9) return SchemaInput * 100 + SchemaCaliber;
                else return SchemaInput * 10 + SchemaCaliber; }
        } // Вычисляемый код схемы проката
        public double H0 {  get; set; } // Высота полосы до
        public double B0 { get; set; } // Ширина полосы до
        public double H1 { get; set; } // Высота полосы после
        public double B1 { get; set; } // Ширина полосы после
        public double W { get; set; } // Площадь поперечного сечения после
        public double S { get; set; } // Зазор между валками
        public double BVR { get; set; } // Ширина калибра по врезу
        public double BD { get; set; } // Ширина калибра по дну
        public double R { get; set; } // Радиус калибра
        public double ROV { get; set; } // Радиус овального калибра
        public double R8 { get; set; } // Вогнутость дна калибра
        public double SUMX { get; set; } // Расположение калибров по бочке
        public double PSI { get; set; } // Коэффициент плеча приложения усилия прокатки
        public double Z { get; set; } // Число ниток
        public double SP { get; set; } // Способ пересчета соответственных полос
        public double TOP { get; set; } // Опытная температура на прокатном стане

        [ForeignKey("SteelSection")]
        public int IdSteelSection { get; set; }

        // Навигационное свойство
        [ValidateNever]
        public virtual SteelSection SteelSection { get; set; }
    }
}
