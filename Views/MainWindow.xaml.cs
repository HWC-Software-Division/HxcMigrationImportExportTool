using HxcMigrationImportExportTool.Parsers;
using HxcMigrationImportExportTool.Services;
using Microsoft.Win32;
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
            var folder = ZipService.Extract(zipPath);

            var xmlFiles = Directory.GetFiles(folder, "*.xml.export", SearchOption.AllDirectories);

            var pageTypeFile = xmlFiles.FirstOrDefault(x => x.Contains("cms_documenttype"));
            var customTableFile = xmlFiles.FirstOrDefault(x => x.Contains("cms_customtable"));
            var resourceFile = xmlFiles.FirstOrDefault(x => x.Contains("cms_resourcestring"));
             
            if (pageTypeFile != null)
            {
                LoadPageTypes(pageTypeFile);
            }

            if (customTableFile != null)
            {
                LoadCustomTables(customTableFile);
            }

            if (resourceFile != null)
            {
                LoadResourceStrings(resourceFile);
            }            
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

        }
    } 
}