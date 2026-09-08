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

using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using Wpf.Ui.Controls;

namespace openZPL.Views;

public partial class ZplPreviewWindow : FluentWindow
{
    public string ZplCode { get; }
    public string SizeText { get; }

    public ZplPreviewWindow(string zplCode, double widthMm, double heightMm, int dpi)
    {
        ZplCode = zplCode;
        SizeText = $"{widthMm:0.#} x {heightMm:0.#} mm ({dpi} dpi)";
        DataContext = this;
        InitializeComponent();
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://labelary.com/viewer.html") { UseShellExecute = true });
        }
        catch
        {
            // ignore — ouverture du navigateur non critique
        }
    }
}
