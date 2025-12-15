using Calculator.Data;
using Calculator.Models;

using Microsoft.AspNetCore.Mvc;
namespace Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        public CalculatorController()
        {
        }
        /// <summary>
        /// Отображение страницы Index.
        /// </summary>
        public IActionResult Index() => View();
        /// <summary>
        /// Обработка запроса на вычисление.
        /// </summary>
        /// <param name="num1">Первый операнд.</param>
        /// <param name="num2">Второй операнд.</param>
        /// <param name="operation">Тип операции (сложение, вычитание, умножение, деление).</param>
[       HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(double num1, double num2, Operation operation)
        {
            // Подготовка объекта для расчета
            var dataInputVariant = new DataInputVariants
            {
                Operand_1 = num1,
                Operand_2 = num2,
                Type_operation = operation,
            };
            ViewBag.Result = CalculatorLibrary.CalculateOperation(num1, num2, operation).ToString();
            // Перенаправление на страницу Index
            return View();
        }
    }
}