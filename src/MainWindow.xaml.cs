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
    public ICommand OpenTemplateCommand { get; }

    public MainWindow()
    {
        InitializeComponent();
        OpenTemplateCommand = new RelayCommand(OpenTemplate);
    }

    private void FileNew_Click(object sender, RoutedEventArgs e) =>
        Editor.ViewModel.NewTemplateCommand.Execute(null);

    private void FileOpen_Click(object sender, RoutedEventArgs e) => OpenTemplate();

    private void OpenTemplate()
    {
        var dialog = new OpenTemplateWindow(Editor.ViewModel.Templates) { Owner = this };
        if (dialog.ShowDialog() == true && dialog.SelectedTemplate is not null)
            Editor.ViewModel.SelectTemplateCommand.Execute(dialog.SelectedTemplate);
    }

    private void FileSave_Click(object sender, RoutedEventArgs e) =>
        Editor.ViewModel.SaveCommand.Execute(null);

    private void FileExit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new PrinterSettingsWindow { Owner = this };
        dialog.ShowDialog();
        Editor.ViewModel.LoadPrinters();
    }
}
