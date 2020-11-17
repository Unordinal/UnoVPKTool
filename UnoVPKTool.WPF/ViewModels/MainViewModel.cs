using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoVPKTool.WPF.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private readonly StringWriterExt _sw = new StringWriterExt(true);
        private string _consoleOut = string.Empty;

        public string ConsoleOut
        {
            get => _consoleOut;
            set => SetProperty(ref _consoleOut, value);
        }

        public MainViewModel()
        {
            Console.SetOut(_sw);
            Console.SetError(_sw);
            _sw.Flushed += (s, e) => ConsoleOut = _sw.ToString();
        }
    }
}
