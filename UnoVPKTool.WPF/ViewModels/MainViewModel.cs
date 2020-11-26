using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using UnoVPKTool.Exceptions;
using UnoVPKTool.VPK;
using UnoVPKTool.WPF.Commands;
using UnoVPKTool.WPF.Logging;
using UnoVPKTool.WPF.Models;

namespace UnoVPKTool.WPF.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private readonly StringWriterExt _sw = new StringWriterExt(true);
        private ObservableCollection<IItemModel> _blockFileModels = new ObservableCollection<IItemModel>();
        private DirectoryItemModel _treeViewRoot = new("root");
        private string _consoleOut = string.Empty;

        public Logger Logger { get; }

        public ICommand OpenVPKCommand { get; }

        public DirectoryFile? OpenedVPK { get; private set; }

        public ObservableCollection<IItemModel> BlockFileModels
        {
            get => _blockFileModels;
            set => SetProperty(ref _blockFileModels, value);
        }

        public DirectoryItemModel TreeViewRoot
        {
            get => _treeViewRoot;
            set => SetProperty(ref _treeViewRoot, value);
        }

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

        private void BuildTreeView()
        {
            if (OpenedVPK is null) return;

            BlockFileModels.Clear();
            //DirectoryItemModel rootDir = CreateDirectories(OpenedVPK.EntryBlocks);
            //BlockFileModels.Add(rootDir);
        }

        /*/// <summary>
        /// Builds a directory that contains directories and/or items from a list of blocks.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        private static DirectoryItemModel CreateDirectories(IEnumerable<DirectoryEntryBlock> blocks)
        {
            if (blocks is null) throw new ArgumentNullException(nameof(blocks));

            DirectoryItemModel root = new("root");
            foreach (var block in blocks)
            {
                DirectoryItemModel currDirectory = root;
                var pathParts = block.FilePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                string currPath = "";
                foreach (var part in pathParts)
                {
                    bool isFile = Path.HasExtension(part);
                    if (isFile)
                        currPath += part;
                    else
                        currPath += part + Path.DirectorySeparatorChar;

                    IItemModel? childItem = currDirectory.Items.SingleOrDefault(b => b.Path == currPath);
                    if (childItem is null)
                    {
                        if (isFile)
                            childItem = new BlockFileItemModel(currPath, block);
                        else
                            childItem = new DirectoryItemModel(currPath);
                            
                        currDirectory.Items.Add(childItem);
                    }

                    if (childItem is not DirectoryItemModel childDir) break;
                    currDirectory = childDir;
                }
            }

            return root;
        }

        /// <summary>
        /// Creates directory models in the specified base directory model that lead up to the given item model, returning the last directory created, or null if none were.
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        private static DirectoryItemModel? CreateDirectoryModels(DirectoryItemModel baseDirectory, IItemModel? itemModel)
        {
            if (itemModel is null) return null;

            var directoryPathOnly = Path.GetDirectoryName(itemModel.Path); // Possibly needs to be changed.
            if (directoryPathOnly is null) return null;

            string[] directories = directoryPathOnly.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            // This all doesn't seem ideal.
            DirectoryItemModel prevDirModel = baseDirectory;
            foreach (var dir in directories)
            {
                string newPath = string.Join(Path.DirectorySeparatorChar, prevDirModel.Path, dir);
                DirectoryItemModel newDir = new(newPath);

                prevDirModel.Items.Add(newDir);
                prevDirModel = newDir;
            }

            return directories.Any() ? prevDirModel : null;
        }*/

        private void OpenVPK(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            string fileName = Path.GetFileName(filePath);
            // If the file isn't a VPK directory, it might be a VPK archive. Let's try and find the directory file from here.
            if (!DirectoryFile.DirectoryFileNameRegex.IsMatch(fileName))
            {
                if (DirectoryFile.ArchiveFileNameRegex.IsMatch(fileName))
                {
                    Logger.LogWarning($"'{fileName}' seems to be a VPK archive file, not a VPK directory. Looking for the matching directory file...");
                    // We found the directory!... maybe. If not, try with the original path.
                    string? possibleArchivePath = Utils.ArchivePathToDirectoryPath(filePath);
                    if (possibleArchivePath is not null)
                    {
                        filePath = possibleArchivePath;
                        Logger.LogMessage("Matching directory file found!" + " ");
                    }
                    else
                    {
                        Logger.LogWarning("Couldn't find directory file.");
                    }
                }
                // It's not an archive file either. Just try and read it as a directory file below.
            }

            try
            {
                // We don't want to assign directly in case we want to keep any previous file opened if the new file turns out to be invalid.
                var vpkFile = new DirectoryFile(filePath);
                if (vpkFile is null) throw new VPKException($"The given file was not a valid VPK directory file.");

                OpenedVPK = vpkFile;
                BuildTreeView();

                Logger.LogMessage($"Loaded VPK: '{filePath}'");
            }
            catch (InvalidVPKFileException ex)
            {
                string message = $"Couldn't open '{filePath}' as it was not a valid VPK directory file.";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.LogError(message);
                Logger.LogError(ex);
            }
            catch (UnsupportedVPKFileException ex)
            {
                string message = $"Couldn't open '{filePath}' as the VPK is of an unsupported version.";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.LogError(message);
                Logger.LogError(ex);
            }
            catch (Exception ex)
            {
                string message = $"Couldn't open '{filePath}': {ex.Message}";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.LogError(message);
                Logger.LogError(ex);
            }
        }
    }
}