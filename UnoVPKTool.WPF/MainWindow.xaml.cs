using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using UnoVPKTool.WPF.Commands;
using UnoVPKTool.WPF.Models;
using UnoVPKTool.WPF.ViewModels;

namespace UnoVPKTool.WPF
{
    // Instance Members
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ICommand FitColumnCommand { get; }

        public ICommand FitAllColumnsCommand { get; }

        public FileItemModel[] TestFiles { get; } = { new FileItemModel("path/to/file1", 24), new FileItemModel("path/to/a/nother/file", 1337) };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            FitColumnCommand = new RelayCommand<GridViewColumn>(FitColumn);
            FitAllColumnsCommand = new RelayCommand<GridViewColumnCollection>(FitAllColumns);

            var assembly = Assembly.GetExecutingAssembly().GetName();
            Console.WriteLine($"{assembly.Name}, version {assembly.Version}");
            Console.WriteLine();
        }

        private void OpenVPK_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel vm = (MainViewModel)DataContext;
            var openFileDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                AddExtension = true,
                Filter = "VPK Directory Files|*_dir.vpk|All files (*.*)|*.*",
                Multiselect = false,
                Title = "Open VPK..."
            };

            if (openFileDialog.ShowDialog() == true)
            {
                vm.OpenVPKCommand.Execute(openFileDialog.FileName);
                if (vm.OpenedVPK is not null)
                {
                    Title = MainTitle + " - " + Path.GetFileName(openFileDialog.FileName);
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (e.Source as TextBox)?.ScrollToEnd();
        }
    }

    // Static Members
    public partial class MainWindow
    {
        private const string MainTitle = "Uno VPK Tool";

        protected static void FitColumn(GridViewColumn column)
        {
            if (column is null) return;
            if (double.IsNaN(column.Width))
                column.Width = 1;

            column.Width = double.NaN;
        }

        protected static void FitAllColumns(GridViewColumnCollection columns)
        {
            if (columns is null) return;
            foreach (var column in columns)
            {
                FitColumn(column);
            }
        }
    }
}