using HxcMigrationImportExportTool.Models;
using HxcMigrationImportExportTool.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Data.SqlClient;

namespace HxcMigrationImportExportTool.Views
{
    /// <summary>
    /// Interaction logic for DbSettingWindow.xaml
    /// </summary>
    public partial class DbSettingWindow : Window
    {
        public DbSettingWindow()
        {
            InitializeComponent();

            LoadConfig();
        }

        private void LoadConfig()
        {
            var config = ConfigService.Load();

            if (config != null)
            {
                txtServer.Text = config.Server;
                txtDatabase.Text = config.Database;
                txtUsername.Text = config.Username;
                txtPassword.Password = config.Password;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var config = new DbConfig
            {
                Server = txtServer.Text,
                Database = txtDatabase.Text,
                Username = txtUsername.Text,
                Password = txtPassword.Password
            };

            ConfigService.Save(config);

            MessageBox.Show("Saved successfully ✅");
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var connStr = $"Server={txtServer.Text};Database={txtDatabase.Text};User Id={txtUsername.Text};Password={txtPassword.Password};TrustServerCertificate=True;";

                using var conn = new SqlConnection(connStr);
                conn.Open();

                MessageBox.Show("Connection Success ✅");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Failed ❌\n" + ex.Message);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
