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

namespace WpfApp1.ViewModels
{
    class DashboardViewModel
    {
        public User CurrentUser { get; private set; }

        public DashboardViewModel(User user)
        {
            // Assign the logged-in user to a property
            CurrentUser = user;

            // You can now use CurrentUser.Role, CurrentUser.Username, etc.
        }
    }
}
