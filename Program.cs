using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace ConsoleAppForTheTestTask
{
    [Table(Name = "Employees")]
    public class Employee
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int EmployeeID { get; set; }
        [Column(Name = "EmployeeName")]
        public string EmployeeName { get; set; }
    }

    [Table(Name = "DisabledPerson")]
    public class DisabledPerson
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int DisabledPersonID { get; set; }
        [Column(Name = "EmployeeID")]
        public int EmployeeID { get; set; }
    }

    [Table(Name = "Phones")]
    public class Phone
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int PhoneID { get; set; }
        [Column(Name = "PhoneNumber")]
        public string EmployeeName { get; set; }
        [Column(Name = "EmployeeID")]
        public int EmployeeID { get; set; }
    }

    class Program
    {
        static string connectionString = @"";
        static void Main(string[] args)
        {
            DataContext db = new DataContext(connectionString);

            // Получаем таблицу пользователей
            Table<Employee> Employees = db.GetTable<Employee>();

            foreach (var Employee in Employees)
            {
                Console.WriteLine("{0} \t{1}", Employee.EmployeeID, Employee.EmployeeName);
            }

            Console.Read();
        }
    }
}
