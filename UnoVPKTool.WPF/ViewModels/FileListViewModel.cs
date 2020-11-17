using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UnoVPKTool.WPF.Commands;
using UnoVPKTool.WPF.Models;

namespace UnoVPKTool.WPF.ViewModels
{
    public class FileListViewModel : NotifyPropertyChanged
    {
        public ICommand FitColumnCommand { get; }
        public ICommand FitAllColumnsCommand { get; }
        public FileItemModel[] TestFiles { get; } = { new FileItemModel("path/to/file1", "File 1", 24), new FileItemModel("path/to/a/nother/file", "File 2", 1337) };

        public FileListViewModel()
        {
            FitColumnCommand = new RelayCommand<GridViewColumn>(FitColumn);
            FitAllColumnsCommand = new RelayCommand<GridViewColumnCollection>(FitAllColumns);
        }

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
