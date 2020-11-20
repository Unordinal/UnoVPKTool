using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using UnoVPKTool.Exceptions;
using UnoVPKTool.VPK;
using UnoVPKTool.WPF.Commands;
using UnoVPKTool.WPF.Logging;

namespace UnoVPKTool.WPF.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private readonly StringWriterExt _sw = new StringWriterExt(true);
        private string _consoleOut = string.Empty;

        public Logger Logger { get; }

        public ICommand OpenVPKCommand { get; }

        public DirectoryFile? OpenedVPK { get; private set; }

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
            Logger = new Logger();

            OpenVPKCommand = new RelayCommand<string>(OpenVPK);

            Logger.LogDebug("Test debug.");
            Logger.LogError("Test error.");
        }

        private void OpenVPK(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            string fileName = Path.GetFileName(filePath);
            // If the file isn't a VPK directory, it might be a VPK archive. Let's try and find the directory file from here.
            if (!DirectoryFile.DirectoryFileNameRegex.IsMatch(fileName))
            {
                if (DirectoryFile.ArchiveFileNameRegex.IsMatch(fileName))
                {
                    Console.WriteLine($"'{fileName}' seems to be a VPK archive file, not a VPK directory. Looking for the matching directory file...");
                    // We found the directory!... maybe. If not, try with the original path.
                    string? possibleArchivePath = Utils.ArchivePathToDirectoryPath(filePath);
                    if (possibleArchivePath is not null)
                    {
                        filePath = possibleArchivePath;
                        Console.Write("Matching directory file found!" + " ");
                    }
                }
                // It's not an archive file either. Just try and read it as a directory file below.
            }

            try
            {
                OpenedVPK = new DirectoryFile(filePath);
                Console.WriteLine($"Loaded VPK: '{filePath}'");
            }
            catch (InvalidVPKFileException ex)
            {
                string message = $"Couldn't open '{filePath}' as it was not a valid VPK directory file.";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(message);
                Console.WriteLine(ex);
            }
            catch (UnsupportedVPKFileException ex)
            {
                string message = $"Couldn't open '{filePath}' as the VPK is of an unsupported version.";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(message);
                Console.WriteLine(ex);
            }
        }
    }
}