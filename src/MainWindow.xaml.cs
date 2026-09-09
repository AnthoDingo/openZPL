// openZPL
// Copyright (C) 2026 AnthoDingo
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using openZPL.Views;
using Wpf.Ui.Controls;

namespace openZPL;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public ICommand ExitCommand { get; } = new RelayCommand(() => Application.Current.Shutdown());

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (App.StartupFilePath is { } path)
            await Editor.ViewModel.LoadFileAsync(path);
    }

    private void FileExit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Printers_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new PrinterSettingsWindow { Owner = this };
        dialog.ShowDialog();
        Editor.ViewModel.LoadPrinters();
    }
}
