using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using D2MC;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace D2MC_Config
{
    public partial class MainWindow : Window
    {
        private static DbContext db;

        public MainWindow()
        {
            InitializeComponent();
            db = new DbContext();

            path.Text = db.FetchPath();

            List<string> parameters = db.FetchParameters();
            foreach(string param in parameters)
            {
                AddParameter(param);
            }
        }

        private void Click_FilePath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".exe";
            dlg.Filter = "Diablo II executable (*.exe)|*.exe";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                path.Text = filename;
            }
        }
        
        private void  Click_RemoveParameters(object sender, RoutedEventArgs e)
        {
            parameters.Children.Clear();
        }

        private void Click_AddParameter(object sender, RoutedEventArgs e)
        {
            AddParameter();
        }

        public void AddParameter(string name = "-new_param")
        {
            TextBox newParameter = new TextBox();
            newParameter.Text = name;
            parameters.Children.Add(newParameter);
        }

        private void Click_Save(object sender, RoutedEventArgs e)
        {
            db.Query("DELETE FROM Parameters");

            string query = "INSERT INTO Parameters (name) VALUES ";
            foreach(TextBox child in parameters.Children)
            {
                query += "('"+ child.Text+"'),";
            }

            query = query.Substring(0, query.Length-1);
            db.Query(query);


            db.Query("DELETE FROM Path");
            db.Query("INSERT INTO Path VALUES ('"+ path.Text +"')");

            content.Children.Clear();
            TextBox saved = new TextBox();
            saved.Text = "Saved!";
            saved.Foreground = System.Windows.Media.Brushes.Green;
            content.Children.Add(saved);
            Thread.Sleep(1500); // too quick to change UI ?
            Process.GetCurrentProcess().Kill();
        }
    }
}
