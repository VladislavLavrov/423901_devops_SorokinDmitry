using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortRollWebApp.Models.Entities
{
    /// <summary>
    /// Модель данных таблицы
    /// </summary>
    public class RollingMill
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [ForeignKey("Factory")]
        public int IdFactory { get; set; }

        // Навигационные свойства
        [ValidateNever]
        public virtual Factory Factory { get; set; }

        [ValidateNever]
        public virtual ICollection<SteelSection> SteelSections { get; set; }

        [ValidateNever]
        public virtual ICollection<RollingStand> RollingStands { get; set; }
    }
}
