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
    public partial class MainWindow : Window
    {
        private List<K13PageType> _pageTypes = new();
        private List<K13CustomTable> _customTables = new();

        private List<K13ResourceString> _resourceStrings = new();
        private List<ResourceStringGridRow> _resourceGridRows = new();

        public MainWindow()
        {
            InitializeComponent();
            ConfigureDetailGridForEmpty();
        }

        private void BtnSelectZip_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Zip files (*.zip)|*.zip"
            };

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
            else
            {
                txtPageTypeCount.Text = "0";
                gridPageTypes.ItemsSource = null;
            }

            if (customTableFile != null)
            {
                Logger.Log("CustomTable export detected");
                LoadCustomTables(customTableFile);
            }
            else
            {
                txtCustomCount.Text = "0";
                gridCustom.ItemsSource = null;
            }

            if (resourceFile != null)
            {
                Logger.Log("ResourceString export detected");
                LoadResourceStrings(resourceFile);
            }
            else
            {
                txtResourceCount.Text = "0";
                gridResource.ItemsSource = null;
            }

            ClearDetail();

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
            _resourceStrings = parser.Parse(xmlFile);

            _resourceGridRows = _resourceStrings
                .Select(x => new ResourceStringGridRow
                {
                    Id = x.Id,
                    Key = x.Key,
                    Description = x.Description,
                    LanguageCount = x.Values?.Count ?? 0,
                    LanguagesDisplay = x.Values == null || x.Values.Count == 0
                        ? string.Empty
                        : string.Join(", ", x.Values.Keys.OrderBy(k => k)),
                    Resource = x
                })
                .OrderBy(x => x.Key)
                .ToList();

            txtResourceCount.Text = _resourceGridRows.Count.ToString();
            gridResource.ItemsSource = _resourceGridRows;

            ClearDetail();
        }

        #endregion


        #region Tabs List actions
        private void GridPageTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridPageTypes.SelectedItem is K13PageType pageType)
            {
                ConfigureDetailGridForPageType();
                gridDetail.ItemsSource = pageType.Fields;
            }
            else
            {
                ClearDetail();
            } 
        }

        private void GridResource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridResource.SelectedItem is ResourceStringGridRow selected && selected.Resource != null)
            {
                var valueRows = (selected.Resource.Values ?? new Dictionary<string, string>())
                    .OrderBy(x => x.Key)
                    .Select(x => new ResourceValueRow
                    {
                        Language = x.Key,
                        Value = x.Value
                    })
                    .ToList();

                ConfigureDetailGridForResourceString();
                gridDetail.ItemsSource = valueRows;
            }
            else
            {
                ClearDetail();
            }
        }

        private void GridCustom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ตอนนี้ยังไม่ทำ detail ของ custom table
            // ภายหลังค่อยเพิ่ม ConfigureDetailGridForCustomTable() ได้
            ClearDetail();
        } 

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            // ให้ตอบสนองเฉพาะตอน TabControl เปลี่ยน tab จริง
            if (e.OriginalSource != tabMain)
            {
                return;
            }
            ClearDetail();
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
                _resourceStrings,
                _customTables
            );

            if (win.ShowDialog() == true)
            {
                var api = new XbykApiService("http://localhost:34486/", "dev-key");

                var service = new MigrateService(api);

                var (success, fail, skip) = await service.MigratePageTypesAsync(win.SelectedPageTypes);

                MessageBox.Show($"✅ Success: {success}\n" +
                                $"⏭ Skip: {skip}\n" +
                                $"❌ Fail: {fail}"
                );
            }
        }

        private void BtnExportReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export Report clicked");
        } 

        private async void BtnClearScreen_Click(object sender, RoutedEventArgs e)
        {
            txtZipFile.Text = string.Empty;
            txtPageTypeCount.Text = "0";
            txtResourceCount.Text = "0";
            txtCustomCount.Text = "0";

            gridPageTypes.ItemsSource = null;
            gridResource.ItemsSource = null;
            gridCustom.ItemsSource = null;

            _resourceStrings.Clear();
            _resourceGridRows.Clear();

            ClearDetail();
        }

        private void ClearDetail()
        {
            gridDetail.ItemsSource = null;
            ConfigureDetailGridForEmpty();
        }
         
        private void ConfigureDetailGridForEmpty()
        {
            gridDetail.AutoGenerateColumns = false;
            gridDetail.Columns.Clear();
        }

        private void ConfigureDetailGridForPageType()
        {
            gridDetail.AutoGenerateColumns = false;
            gridDetail.Columns.Clear();

            gridDetail.Columns.Add(new DataGridTextColumn
            {
                Header = "Column",
                Binding = new System.Windows.Data.Binding("Column"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });

            gridDetail.Columns.Add(new DataGridTextColumn
            {
                Header = "Data Type",
                Binding = new System.Windows.Data.Binding("DataType"),
                Width = new DataGridLength(220)
            });
        }

        private void ConfigureDetailGridForResourceString()
        {
            gridDetail.AutoGenerateColumns = false;
            gridDetail.Columns.Clear();

            gridDetail.Columns.Add(new DataGridTextColumn
            {
                Header = "Language",
                Binding = new System.Windows.Data.Binding("Language"),
                Width = new DataGridLength(180)
            });

            gridDetail.Columns.Add(new DataGridTextColumn
            {
                Header = "Value",
                Binding = new System.Windows.Data.Binding("Value"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
        }

        //เผื่ออนาคต
        private void ConfigureDetailGridForCustomTable()
        {
            gridDetail.AutoGenerateColumns = false;
            gridDetail.Columns.Clear();

            gridDetail.Columns.Add(new DataGridTextColumn
            {
                Header = "Column",
                Binding = new System.Windows.Data.Binding("Column"),
                Width = new DataGridLength(220)
            });

            gridDetail.Columns.Add(new DataGridTextColumn
            {
                Header = "Data Type",
                Binding = new System.Windows.Data.Binding("DataType"),
                Width = new DataGridLength(180)
            });
        }

        #endregion
    }

    public class ResourceStringGridRow
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int LanguageCount { get; set; }
        public string LanguagesDisplay { get; set; } = string.Empty;

        public K13ResourceString? Resource { get; set; }
    }

    public class ResourceValueRow
    {
        public string Language { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}