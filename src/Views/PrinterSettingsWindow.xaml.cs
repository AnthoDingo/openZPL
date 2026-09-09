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
using System.Windows.Controls;
using openZPL.Resources;
using openZPL.Services;
using Wpf.Ui.Controls;

namespace openZPL.Views;

public partial class PrinterSettingsWindow : FluentWindow
{
    private readonly PrinterSettingsStore _store = new();
    private PrinterSettingsData _data = new();

    public PrinterSettingsWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        _data = _store.Load();
        PrintersList.ItemsSource = _data.Printers;
        PrintersList.SelectedItem = _data.Printers.FirstOrDefault(p => p.IsDefault) ?? _data.Printers.FirstOrDefault();
    }

    private PrinterProfile? SelectedProfile => PrintersList.SelectedItem as PrinterProfile;

    private void ShowDetail(PrinterProfile? profile)
    {
        DetailPanel.IsEnabled = profile is not null;
        NameTextBox.Text = profile?.Name ?? string.Empty;
        IpTextBox.Text = profile?.IpAddress ?? string.Empty;
        PortNumberBox.Value = profile?.Port ?? 9100;
        DefaultToggle.IsChecked = profile?.IsDefault ?? false;
    }

    private void PrintersList_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        ShowDetail(SelectedProfile);

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var profile = new PrinterProfile { Name = Strings.NewPrinterName };
        _data.Printers.Add(profile);
        if (_data.Printers.Count == 1) _data.DefaultPrinterId = profile.Id;
        SaveAndRefresh(profile);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        PrinterProfile? profile = SelectedProfile;
        if (profile is null) return;

        profile.Name = NameTextBox.Text.Trim();
        profile.IpAddress = IpTextBox.Text.Trim();
        profile.Port = (int)(PortNumberBox.Value ?? 9100);

        if (DefaultToggle.IsChecked == true)
            _data.DefaultPrinterId = profile.Id;
        else if (_data.DefaultPrinterId == profile.Id)
            _data.DefaultPrinterId = _data.Printers.FirstOrDefault(p => p.Id != profile.Id)?.Id;

        SaveAndRefresh(profile);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        PrinterProfile? profile = SelectedProfile;
        if (profile is null) return;

        _data.Printers.Remove(profile);
        if (_data.DefaultPrinterId == profile.Id)
            _data.DefaultPrinterId = _data.Printers.FirstOrDefault()?.Id;

        SaveAndRefresh(_data.Printers.FirstOrDefault());
    }

    /// <summary>Persiste les changements puis rafraichit la liste et la selection
    /// (necessaire pour que le badge "defaut" et l'ordre restent a jour).</summary>
    private void SaveAndRefresh(PrinterProfile? toSelect)
    {
        _store.Save(_data);
        _data = _store.Load();
        PrintersList.ItemsSource = _data.Printers;
        PrintersList.SelectedItem = toSelect is null
            ? null
            : _data.Printers.FirstOrDefault(p => p.Id == toSelect.Id);

        if (PrintersList.SelectedItem is null)
            ShowDetail(null);
    }
}
