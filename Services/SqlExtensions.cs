using WpfApp1.Services;
using WpfApp1.Models;
using WpfApp1.Views.Shared;
using WpfApp1.Views.Admin;
using WpfApp1.Views.Manager;
using WpfApp1.Views.Employee;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
    public static class SqlExtensions
    {
        public static SqlCommand WithParams(this SqlCommand cmd, params (string name, object val)[] ps)
        {
            foreach (var (n, v) in ps) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
            return cmd;
        }
    }

}
