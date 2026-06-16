using WpfApp1.Services;
using WpfApp1.Models;
using WpfApp1.Views.Shared;
using WpfApp1.Views.Admin;
using WpfApp1.Views.Manager;
using WpfApp1.Views.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class Employee
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public decimal Salary { get; set; } = 2000;
        public DateTime DateOfBirth { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
    }
}
