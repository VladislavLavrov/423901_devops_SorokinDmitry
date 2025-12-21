using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortRollWebApp.Models.Entities
{
    /// <summary>
    /// Модель данных таблицы
    /// </summary>
    public class SteelSection
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }
        //public double Area { get; set; } // Площадь поперечного сечения

        [ForeignKey("RollingMill")]
        public int IdRollingMill { get; set; }

        // Навигационные свойства
        [ValidateNever]
        public virtual RollingMill RollingMill { get; set; }

        [ValidateNever]
        public virtual ICollection<Pass> Passes { get; set; }

        [ValidateNever]
        public virtual ICollection<InitialParameters> InitialParameters { get; set; }
    }
}
