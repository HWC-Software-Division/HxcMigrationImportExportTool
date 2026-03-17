using HxcMigrationImportExportTool.Models;
using HxcMigrationImportExportTool.Parsers;
using HxcMigrationImportExportTool.Services;
using HxcMigrationImportExportTool.Views;
using Microsoft.Win32;
using System.Diagnostics; 
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace HxcMigrationImportExportTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSelectZip_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Zip files (*.zip)|*.zip";

            if (dialog.ShowDialog() == true)
            {
                txtZipFile.Text = dialog.FileName;

                AnalyzeZip(dialog.FileName);
            }
        }

        private void AnalyzeZip(string zipPath)
        {
            Logger.Log("Start Analyze ZIP");

            var folder = ZipService.Extract(zipPath);  
            var xmlFiles = Directory.GetFiles(folder, "*.xml.export", SearchOption.AllDirectories);
 
            var pageTypeFile = xmlFiles.FirstOrDefault(x => x.Contains("cms_documenttype"));
            var customTableFile = xmlFiles.FirstOrDefault(x => x.Contains("cms_customtable"));
            var resourceFile = xmlFiles.FirstOrDefault(x => x.Contains("cms_resourcestring"));

            Logger.Log($"Extract folder : {folder}");
            foreach (var file in xmlFiles)
            {
                Logger.Log($"XML Found : {file}");
            }

            if (pageTypeFile != null)
            {
                Logger.Log("PageType export detected");

                LoadPageTypes(pageTypeFile);
            }

            if (customTableFile != null)
            {
                Logger.Log("CustomTable export detected");

                LoadCustomTables(customTableFile);
            }

            if (resourceFile != null)
            {
                Logger.Log("ResourceString export detected");

                LoadResourceStrings(resourceFile);
            }
            Logger.Log("Analyze ZIP finished");
        }

        private void LoadPageTypes(string xmlFile)
        {
            var parser = new PageTypeParser();

            var pageTypes = parser.Parse(xmlFile);

            txtPageTypeCount.Text = pageTypes.Count.ToString();

            gridPageTypes.ItemsSource = pageTypes;
        }

        private void LoadCustomTables(string xmlFile)
        {
            var parser = new CustomTableParser();

            var tables = parser.Parse(xmlFile);

            MessageBox.Show($"CustomTables detected : {tables.Count}");
        }

        private void LoadResourceStrings(string xmlFile)
        {
            var parser = new ResourceStringParser();

            var resources = parser.Parse(xmlFile);

            MessageBox.Show($"ResourceStrings detected : {resources.Count}");
        }

        private void gridPageTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (gridPageTypes.SelectedItem is K13PageType pageType)
            //{
            //    if (pageType.Fields == null || pageType.Fields.Count == 0)
            //    {
            //        MessageBox.Show("No fields found.", pageType.ClassName);
            //        return;
            //    }

            //    var fields = string.Join("\n",
            //        pageType.Fields.Select(f => $"{f.Column} ({f.DataType})"));

            //    MessageBox.Show(fields, $"Fields of {pageType.ClassName}");
            //}

            if (gridPageTypes.SelectedItem is K13PageType pageType)
            {
                gridFields.ItemsSource = pageType.Fields;
            }
        }

        #region Action Click
        private void BtnDbSetting_Click(object sender, RoutedEventArgs e)
        {
            var win = new DbSettingWindow();
            win.ShowDialog();
        }

        private void BtnMigrate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Start Migrate 🚀");
        }

        private void BtnExportReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export Report 📄");
        }

        #endregion

    }
}