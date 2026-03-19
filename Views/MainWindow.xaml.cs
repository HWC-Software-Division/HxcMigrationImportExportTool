using HxcMigrationImportExportTool.Models;
using HxcMigrationImportExportTool.Parsers;
using HxcMigrationImportExportTool.Services;
using HxcMigrationImportExportTool.Views;
using Microsoft.Win32;
using System.Diagnostics; 
using System.IO;
using System.Resources;
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
        private List<K13PageType> _pageTypes = new();
        private List<K13ResourceString> _resources = new();
        private List<K13CustomTable> _customTables = new();

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

        #region Load and Parse XML
        private void LoadPageTypes(string xmlFile)
        {
            var parser = new PageTypeParser();

            var pageTypes = parser.Parse(xmlFile);

            _pageTypes = pageTypes; 

            txtPageTypeCount.Text = pageTypes.Count.ToString();
            gridPageTypes.ItemsSource = pageTypes;
        }

        private void LoadCustomTables(string xmlFile)
        {
            var parser = new CustomTableParser();

            var tables = parser.Parse(xmlFile);

            _customTables = tables;

            txtCustomCount.Text = tables.Count.ToString();
            gridCustom.ItemsSource = tables;

            MessageBox.Show($"CustomTables detected : {tables.Count}");
        }

        private void LoadResourceStrings(string xmlFile)
        {
            var parser = new ResourceStringParser();

            var resources = parser.Parse(xmlFile);

            _resources = resources;

            txtResourceCount.Text = resources.Count.ToString();
            gridResource.ItemsSource = resources;

            MessageBox.Show($"ResourceStrings detected : {resources.Count}");
        }

        #endregion

        #region Tabs List actions
        private void GridPageTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            if (gridPageTypes.SelectedItem is K13PageType pt)
            {
                gridDetail.ItemsSource = pt.Fields;
            }
        }

        private void GridResource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridResource.SelectedItem is K13ResourceString rs)
            {
                gridDetail.ItemsSource = new List<object>
                {
                    new { rs.Key, rs.Value }
                };
            }
        }

        private void GridCustom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridCustom.SelectedItem is K13CustomTable ct)
            {
                gridDetail.ItemsSource = ct.Fields;
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        #endregion

        #region Action Click
        private void BtnDbSetting_Click(object sender, RoutedEventArgs e)
        {
            var win = new DbSettingWindow();
            win.ShowDialog();
        }

        private async void BtnMigrate_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Start Migrate 🚀");

            var win = new MigrateSelectionWindow(
                _pageTypes,
                _resources,
                _customTables
            );

            //if (win.ShowDialog() == true)
            //{
            //    // ✅ ดึงค่าที่เลือก
            //    var selectedPageTypes = win.SelectedPageTypes;
            //    var selectedResources = win.SelectedResources;
            //    var selectedCustomTables = win.SelectedCustomTables;

            //    MessageBox.Show($"Selected PageTypes: {selectedPageTypes.Count}");
            //}

            if (win.ShowDialog() == true)
            {
                var api = new XbykApiService("http://localhost:34486/", "dev-key");

                var service = new MigrateService(api);

                //var (success, fail) = await service.MigratePageTypesAsync(win.SelectedPageTypes);
                //MessageBox.Show($"✅ Success: {success}\n❌ Fail: {fail}");

                var (success, fail, skip) = await service.MigratePageTypesAsync(win.SelectedPageTypes);

                MessageBox.Show($"✅ Success: {success}\n" +
                                $"⏭ Skip: {skip}\n" +
                                $"❌ Fail: {fail}"
                );
            }
        }

        private void BtnExportReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export Report 📄");
        }

        private void BtnClearScreen_Click(object sender, RoutedEventArgs e)
        {
            ResetUI();
        }

        private void ResetUI()
        {
            // 1. Clear path
            txtZipFile.Text = "";

            // 2. Clear count
            txtPageTypeCount.Text = "0";
            txtResourceCount.Text = "0";
            txtCustomCount.Text = "0";

            // 3. Clear grids
            gridPageTypes.ItemsSource = null;
            gridResource.ItemsSource = null;
            gridCustom.ItemsSource = null;

            // 4. Clear detail
            gridDetail.ItemsSource = null;

            // 5. (optional) clear selection
            gridPageTypes.SelectedItem = null;
            gridResource.SelectedItem = null;
            gridCustom.SelectedItem = null;
        }

        #endregion

    }
}