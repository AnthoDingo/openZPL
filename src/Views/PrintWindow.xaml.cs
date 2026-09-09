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
using openZPL.Services;
using Wpf.Ui.Controls;

namespace openZPL.Views;

/// <summary>Demande l'imprimante et le nombre d'exemplaires avant d'envoyer
/// l'etiquette.</summary>
public partial class PrintWindow : FluentWindow
{
    /// <summary>Imprimante retenue, valable seulement si ShowDialog a rendu true.</summary>
    public PrinterProfile? SelectedPrinter { get; private set; }

    /// <summary>Nombre d'exemplaires demande, au moins 1.</summary>
    public int Copies { get; private set; } = 1;

    public PrintWindow(IEnumerable<PrinterProfile> printers, PrinterProfile? current)
    {
        InitializeComponent();

        List<PrinterProfile> list = [.. printers];
        PrinterComboBox.ItemsSource = list;
        PrinterComboBox.SelectedItem = current is not null && list.Contains(current)
            ? current
            : list.FirstOrDefault(p => p.IsDefault) ?? list.FirstOrDefault();

        if (list.Count != 0) return;

        // rien a imprimer tant qu'aucune imprimante n'est configuree
        FormPanel.Visibility = Visibility.Collapsed;
        NoPrinterText.Visibility = Visibility.Visible;
        PrintButton.IsEnabled = false;
    }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        if (PrinterComboBox.SelectedItem is not PrinterProfile printer) return;

        SelectedPrinter = printer;
        Copies = (int)(CopiesNumberBox.Value ?? 1);
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
