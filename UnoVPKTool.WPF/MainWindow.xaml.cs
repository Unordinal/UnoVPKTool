using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Microsoft.Win32;
using UnoVPKTool.WPF.ViewModels;

namespace UnoVPKTool.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string MainTitle = "Uno VPK Tool";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            FileListView.DataContext = new FileListViewModel();

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
                Title = MainTitle + " - " + Path.GetFileName(openFileDialog.FileName);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (e.Source as TextBox)?.ScrollToEnd();
        }
    }
}
