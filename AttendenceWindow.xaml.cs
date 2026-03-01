using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
//using WpfApp1.Model;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for AttendenceWindow.xaml
    /// </summary>
    public partial class AttendenceWindow : UserControl
    {
        SqlConnection connection = new SqlConnection(DatabaseHelper.ConnectionString);
        private User _user;
        public AttendenceWindow(User user)
        {
            InitializeComponent();
            _user = user;
            LoadAvailableMonthsAndYears();
        }

        private void LoadAvailableMonthsAndYears()
        {
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand($@"
            SELECT DISTINCT 
                YEAR([date]) AS YearValue, 
                MONTH([date]) AS MonthValue 
            FROM attendance_{_user.Id}", connection);

                SqlDataReader reader = cmd.ExecuteReader();

                HashSet<int> yearSet = new();
                HashSet<int> monthSet = new();

                while (reader.Read())
                {
                    yearSet.Add(reader.GetInt32(0));   // YearValue
                    monthSet.Add(reader.GetInt32(1));  // MonthValue
                }
                reader.Close();

                // Fill YearComboBox
                YearComboBox.ItemsSource = yearSet.OrderByDescending(y => y).Select(y => y.ToString()).ToList();

                // Fill MonthComboBox with month names
                var monthNames = System.Globalization.DateTimeFormatInfo.InvariantInfo.MonthNames;

                MonthComboBox.ItemsSource = monthSet
                    .OrderBy(m => m)
                    .Select(m => monthNames[m - 1]) // m-1 because array is 0-indexed
                    .ToList();
            }
            finally
            {
                connection.Close();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MonthComboBox.SelectedItem != null && YearComboBox.SelectedItem != null)
            {
                GenerateButton.IsEnabled = true;
                SaveAsPdfButton.IsEnabled = true;
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            AttendanceContainer.Children.Clear();

            if (MonthComboBox.SelectedItem == null || YearComboBox.SelectedItem == null)
                return;

            string monthName = MonthComboBox.SelectedItem.ToString();
            int monthNumber = DateTime.ParseExact(monthName, "MMMM", null).Month;
            int year = int.Parse(YearComboBox.SelectedItem.ToString());

            string tableName = $"attendance_{_user.Id}";

            connection.Open();

            string query = $@"
        SELECT [date], [arrival_time], [exit_time], [ot_hours], [late]
        FROM {tableName}
        WHERE YEAR([date]) = @year AND MONTH([date]) = @month";

            using SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@month", monthNumber);

            using SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                AttendanceContainer.Children.Add(new TextBlock
                {
                    Text = "No attendance records found for the selected period.",
                    Foreground = Brushes.Red,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                return;
            }

            while (reader.Read())
            {
                DateTime date = reader.GetDateTime(0);
                string arrival = reader.IsDBNull(1) ? "N/A" : reader.GetTimeSpan(1).ToString(@"hh\:mm");
                string exit = reader.IsDBNull(2) ? "N/A" : reader.GetTimeSpan(2).ToString(@"hh\:mm");
                decimal ot = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                decimal late = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                string lateDisplay = late == 1 ? "Yes" : late.ToString();

                StackPanel entryPanel = new StackPanel { Margin = new Thickness(5), Orientation = Orientation.Vertical };

                entryPanel.Children.Add(new TextBlock { Text = $"Date: {date:yyyy-MM-dd}", FontWeight = FontWeights.Bold });
                entryPanel.Children.Add(new TextBlock { Text = $"Arrival Time: {arrival}" });
                entryPanel.Children.Add(new TextBlock { Text = $"Exit Time: {exit}" });
                entryPanel.Children.Add(new TextBlock { Text = $"Overtime Hours: {ot}" });
                entryPanel.Children.Add(new TextBlock { Text = $"Late: {lateDisplay}" });
                entryPanel.Children.Add(new Separator { Margin = new Thickness(0, 5, 0, 5) });

                AttendanceContainer.Children.Add(entryPanel);
            }
        }

        private void SaveAsPdf()
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)AttendanceContainer.ActualWidth, (int)AttendanceContainer.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            rtb.Render(AttendanceContainer);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using MemoryStream ms = new MemoryStream();
            encoder.Save(ms);
            ms.Position = 0;

            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XImage img = XImage.FromStream(ms);

            page.Width = img.PointWidth;
            page.Height = img.PointHeight;
            gfx.DrawImage(img, 0, 0);

            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Payslip",
                DefaultExt = ".pdf",
                Filter = "PDF Document (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                document.Save(dialog.FileName);
                MessageBox.Show("Payslip saved as PDF!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveAsPdfButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAsPdf();
        }
    }
}
