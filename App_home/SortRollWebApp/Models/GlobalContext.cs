using SortRollWebApp.Models.Entities;

namespace SortRollWebApp.Models
{
    /// <summary>
    /// Все параметры всего процесса проката
    /// </summary>
    public class GlobalContext
    {
        public Factory Factory { get; set; } = new Factory();
        public InitialParameters InitialParameters { get; set; } = new InitialParameters();
        public RollingMill RollingMill { get; set; } = new RollingMill();
        public Steel Steel { get; set; } = new Steel();
        public SteelSection SteelSection { get; set; } = new SteelSection();
        public GlobalRollingParameters GlobalRollingParameters { get; set; } = new GlobalRollingParameters();
        public List<PassContext> Passes { get; set; } = new List<PassContext>();
    }
}
