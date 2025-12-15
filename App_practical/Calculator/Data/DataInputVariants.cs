using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Calculator.Models;
namespace Calculator.Data
{
    public class DataInputVariants
    {
        [Key]
        public int ID_DataInputVariant { get; set; } // Уникальный идентификатор для объекта DataInputVariant
        public double Operand_1 { get; set; } // Первый операнд для операции
        public double Operand_2 { get; set; } // Второй операнд для операции
        public Operation Type_operation { get; set; } // Тип операции(например, сложение, вычитание и т.д.)
        [Column(TypeName = "varchar(128)")]
        public string? Result { get; set; } // Результат выполнения операции
    }
}