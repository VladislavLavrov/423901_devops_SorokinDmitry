using SortRollWebApp.Models.Entities;

namespace SortRollWebApp.Models
{
    /// <summary>
    /// Все параметры одного прохода
    /// </summary>
    public class PassContext
    {
        public Pass Pass { get; set; } = new Pass();
        public RollingStand RollingStand { get; set; } = new RollingStand();
        public PassRollingParameters PassRollingParameters { get; set; } = new PassRollingParameters();
    }
}