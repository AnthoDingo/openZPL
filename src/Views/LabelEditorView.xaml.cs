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

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using openZPL.Models;
using openZPL.ViewModels;

namespace openZPL.Views;

public partial class LabelEditorView : UserControl
{
    private const int MinElementSize = 10; // dots

    private readonly LabelEditorViewModel _viewModel;

    public LabelEditorViewModel ViewModel => _viewModel;

    public LabelEditorView()
    {
        InitializeComponent();
        DataContext = _viewModel = new LabelEditorViewModel();
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    /// <summary>Redonne le focus clavier au canvas des que la selection change,
    /// quelle qu'en soit l'origine (clic sur un element, liste des calques,
    /// ajout depuis la palette) — necessaire pour que Suppr fonctionne.</summary>
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LabelEditorViewModel.SelectedElement) && _viewModel.SelectedElement is not null)
            LabelSurface.Focus();
    }

    private void LayersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (((ListBox)sender).SelectedItem is LabelElement element)
            _viewModel.SelectElementCommand.Execute(element);
    }

    // ── Reordonnancement des calques (glisser/deposer) ──────────────────────────

    private Point _layerDragStart;

    private void LayersList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _layerDragStart = e.GetPosition(null);
    }

    private void LayersList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;

        Point position = e.GetPosition(null);
        if (Math.Abs(position.X - _layerDragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(position.Y - _layerDragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        if (FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource) is not { } item) return;
        if (item.DataContext is not LabelElement element) return;

        element.IsDragging = true;
        try
        {
            DragDrop.DoDragDrop(item, element, DragDropEffects.Move);
        }
        finally
        {
            element.IsDragging = false;
        }
    }

    private void LayersList_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(LabelElement)) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    /// <summary>Deplace le calque drague a la position de celui survole au
    /// relachement — en fin de liste si le depot tombe hors des elements.</summary>
    private void LayersList_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(LabelElement)) is not LabelElement dragged) return;

        ObservableCollection<LabelElement> elements = _viewModel.CurrentTemplate.Elements;
        int oldIndex = elements.IndexOf(dragged);
        if (oldIndex < 0) return;

        int newIndex = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource) is { } item
            && item.DataContext is LabelElement target
            ? elements.IndexOf(target)
            : elements.Count - 1;

        if (newIndex >= 0 && newIndex != oldIndex)
            elements.Move(oldIndex, newIndex);
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match) return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    // ── Zoom ──────────────────────────────────────────────────────────────────

    private bool _zoomEditCancelled;

    private void ZoomPercentText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2) return;

        _zoomEditCancelled = false;
        ZoomPercentBox.Text = _viewModel.ZoomPercent.ToString();
        ZoomPercentText.Visibility = Visibility.Collapsed;
        ZoomPercentBox.Visibility = Visibility.Visible;
        ZoomPercentBox.Focus();
        ZoomPercentBox.SelectAll();
    }

    private void ZoomPercentBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            LabelSurface.Focus();
        }
        else if (e.Key == Key.Escape)
        {
            _zoomEditCancelled = true;
            LabelSurface.Focus();
        }
    }

    /// <summary>Applique la valeur saisie des que le champ perd le focus (Entree,
    /// clic ailleurs...), sauf si l'utilisateur a annule avec Echap.</summary>
    private void ZoomPercentBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (!_zoomEditCancelled && double.TryParse(ZoomPercentBox.Text, out double percent))
            _viewModel.Scale = Math.Clamp(percent / 100.0, 0.25, 4.0);

        ZoomPercentBox.Visibility = Visibility.Collapsed;
        ZoomPercentText.Visibility = Visibility.Visible;
    }

    // ── Canvas ────────────────────────────────────────────────────────────────

    private void LabelSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ((UIElement)sender).Focus();
        _viewModel.DeselectAllCommand.Execute(null);
    }

    private void LabelSurface_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && _viewModel.SelectedElement is not null)
            _viewModel.DeleteSelectedCommand.Execute(null);
    }

    // ── Deplacement ───────────────────────────────────────────────────────────

    private void MoveThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is LabelElement element)
            _viewModel.SelectElementCommand.Execute(element);
    }

    private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not LabelElement el) return;
        LabelTemplate template = _viewModel.CurrentTemplate;

        int newX = (int)Math.Round(el.X + e.HorizontalChange);
        int newY = (int)Math.Round(el.Y + e.VerticalChange);

        el.X = Math.Max(0, Math.Min(newX, Math.Max(0, template.WidthDots - el.Width)));
        el.Y = Math.Max(0, Math.Min(newY, Math.Max(0, template.HeightDots - el.Height)));
    }

    // ── Redimensionnement (poignees façon Office : coins + cotes) ──────────────

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var thumb = (FrameworkElement)sender;
        if (thumb.DataContext is not LabelElement el) return;
        string handle = (string)thumb.Tag;
        LabelTemplate template = _viewModel.CurrentTemplate;

        int x = el.X, y = el.Y, w = el.Width, h = el.Height;
        int dx = (int)Math.Round(e.HorizontalChange);
        int dy = (int)Math.Round(e.VerticalChange);

        if (handle.Contains('w')) { x += dx; w -= dx; }
        if (handle.Contains('e')) { w += dx; }
        if (handle.Contains('n')) { y += dy; h -= dy; }
        if (handle.Contains('s')) { h += dy; }

        // Empeche de passer sous la taille minimale en gardant le bord oppose fixe
        if (w < MinElementSize)
        {
            if (handle.Contains('w')) x = el.X + el.Width - MinElementSize;
            w = MinElementSize;
        }
        if (h < MinElementSize)
        {
            if (handle.Contains('n')) y = el.Y + el.Height - MinElementSize;
            h = MinElementSize;
        }

        x = Math.Max(0, x);
        y = Math.Max(0, y);
        w = Math.Min(w, Math.Max(MinElementSize, template.WidthDots - x));
        h = Math.Min(h, Math.Max(MinElementSize, template.HeightDots - y));

        el.X = x;
        el.Y = y;
        el.Width = Math.Max(MinElementSize, w);
        el.Height = Math.Max(MinElementSize, h);
    }
}
