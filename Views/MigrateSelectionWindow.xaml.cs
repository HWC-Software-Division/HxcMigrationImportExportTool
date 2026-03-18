using HxcMigrationImportExportTool.Models;
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

namespace HxcMigrationImportExportTool.Views
{
    /// <summary>
    /// Interaction logic for MigrateSelectionWindow.xaml
    /// </summary>
    public partial class MigrateSelectionWindow : Window
    {
        //รายการทั้งหมด + checkbox
        public List<SelectItem<K13PageType>> PageTypes { get; set; }
        public List<SelectItem<K13ResourceString>> Resources { get; set; }
        public List<SelectItem<K13CustomTable>> CustomTables { get; set; }

        //รายการเฉพาะที่ user เลือก
        public List<K13PageType> SelectedPageTypes { get; set; } = new();
        public List<K13ResourceString> SelectedResources { get; set; } = new();
        public List<K13CustomTable> SelectedCustomTables { get; set; } = new();

        public MigrateSelectionWindow(
            List<K13PageType> pageTypes,
            List<K13ResourceString> resources,
            List<K13CustomTable> customTables
        )
        {
            InitializeComponent();

            PageTypes = pageTypes.Select(x => new SelectItem<K13PageType> { Data = x }).ToList();
            Resources = resources.Select(x => new SelectItem<K13ResourceString> { Data = x }).ToList();
            CustomTables = customTables.Select(x => new SelectItem<K13CustomTable> { Data = x }).ToList();

            BindUI();
        }
        private void BindUI()
        {
            listPageTypes.ItemsSource = PageTypes;
            listResources.ItemsSource = Resources;
            listCustomTable.ItemsSource = CustomTables;
        }

        private void BtnConfirmMigrate_Click(object sender, RoutedEventArgs e)
        {
            SelectedPageTypes = PageTypes
                .Where(x => x.IsSelected)
                .Select(x => x.Data)
                .ToList();

            SelectedResources = Resources
                .Where(x => x.IsSelected)
                .Select(x => x.Data)
                .ToList();

            SelectedCustomTables = CustomTables
                .Where(x => x.IsSelected)
                .Select(x => x.Data)
                .ToList();

            MessageBox.Show($"Selected PageTypes: {SelectedPageTypes.Count}");

            DialogResult = true;
            Close();
        }

        #region Select Actions
        private void PageTypeSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = ((CheckBox)sender).IsChecked == true;

            foreach (var item in PageTypes)
            {
                item.IsSelected = isChecked;
            }
             
            listPageTypes.Items.Refresh();
        }



        #endregion
    }
}
