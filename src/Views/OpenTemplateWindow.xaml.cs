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

using System.Collections.Generic;
using System.Windows.Input;
using openZPL.Models;
using Wpf.Ui.Controls;

namespace openZPL.Views;

public partial class OpenTemplateWindow : FluentWindow
{
    public LabelTemplate? SelectedTemplate { get; private set; }

    public OpenTemplateWindow(IEnumerable<LabelTemplate> templates)
    {
        InitializeComponent();
        TemplatesList.ItemsSource = templates;
        TemplatesList.SelectedIndex = TemplatesList.Items.Count > 0 ? 0 : -1;
    }

    private void TemplatesList_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Confirm();

    private void Open_Click(object sender, System.Windows.RoutedEventArgs e) => Confirm();

    private void Confirm()
    {
        if (TemplatesList.SelectedItem is not LabelTemplate template) return;
        SelectedTemplate = template;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
