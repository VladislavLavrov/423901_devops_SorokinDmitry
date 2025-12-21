using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using System.ComponentModel.DataAnnotations;

namespace SortRollWebApp.Models.Entities
{
    /// <summary>
    /// Модель данных таблицы
    /// </summary>
    public class Factory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        // Навигационное свойство
        [ValidateNever]
        public virtual ICollection<RollingMill> RollingMills { get; set; }
    }
}
