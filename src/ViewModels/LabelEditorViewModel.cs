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
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using openZPL.Models;
using openZPL.Services;
using openZPL.Views;
using Wpf.Ui.Controls;

namespace openZPL.ViewModels;

public partial class LabelEditorViewModel : ObservableObject
{
    private readonly TemplateStore _templateStore = new();
    private readonly PrinterSettingsStore _printerStore = new();

    public ObservableCollection<LabelTemplate> Templates { get; } = [];
    public ObservableCollection<PrinterProfile> Printers { get; } = [];

    public Array BarcodeTypeValues => Enum.GetValues(typeof(LabelBarcodeType));
    public int[] DpiValues => [203, 300, 600];

    [ObservableProperty] private LabelTemplate _currentTemplate = new();
    [ObservableProperty] private LabelElement? _selectedElement;
    [ObservableProperty] private PrinterProfile? _selectedPrinter;
    [ObservableProperty] private double _scale = 1.0;

    public bool HasSelection => SelectedElement is not null;
    public bool NoSelection => SelectedElement is null;
    public bool IsSaved => Templates.Contains(CurrentTemplate);
    public int ZoomPercent => (int)Math.Round(Scale * 100);
    public string WindowTitle => $"{CurrentTemplate.Name} - openZPL";

    public LabelEditorViewModel()
    {
        foreach (LabelTemplate t in _templateStore.Load().OrderByDescending(t => t.UpdatedAt))
            Templates.Add(t);

        if (Templates.Count > 0)
            CurrentTemplate = Templates[0];

        LoadPrinters();
    }

    /// <summary>Recharge la liste des imprimantes configurees depuis le disque —
    /// appele aussi apres fermeture de la fenetre Settings pour refleter les changements.</summary>
    public void LoadPrinters()
    {
        string? currentId = SelectedPrinter?.Id;
        PrinterSettingsData data = _printerStore.Load();

        Printers.Clear();
        foreach (PrinterProfile p in data.Printers)
            Printers.Add(p);

        SelectedPrinter = Printers.FirstOrDefault(p => p.Id == currentId)
            ?? Printers.FirstOrDefault(p => p.IsDefault)
            ?? Printers.FirstOrDefault();
    }

    partial void OnSelectedElementChanged(LabelElement? value)
    {
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(NoSelection));
    }

    partial void OnCurrentTemplateChanged(LabelTemplate value)
    {
        SelectedElement = null;
        OnPropertyChanged(nameof(IsSaved));
        OnPropertyChanged(nameof(WindowTitle));
    }

    partial void OnScaleChanged(double value) => OnPropertyChanged(nameof(ZoomPercent));

    // ── Templates ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void NewTemplate() => CurrentTemplate = new LabelTemplate();

    [RelayCommand]
    private void SelectTemplate(LabelTemplate template) => CurrentTemplate = template;

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(CurrentTemplate.Name)) return;

        CurrentTemplate.UpdatedAt = DateTime.Now;
        if (!Templates.Contains(CurrentTemplate))
            Templates.Insert(0, CurrentTemplate);

        _templateStore.Save(Templates);
        OnPropertyChanged(nameof(IsSaved));
    }

    [RelayCommand]
    private void Duplicate()
    {
        LabelTemplate copy = CurrentTemplate.Clone(CurrentTemplate.Name + " (copie)");
        Templates.Insert(0, copy);
        _templateStore.Save(Templates);
        CurrentTemplate = copy;
    }

    [RelayCommand]
    private void Activate(LabelTemplate? template)
    {
        template ??= CurrentTemplate;
        if (!Templates.Contains(template)) return;

        foreach (LabelTemplate t in Templates)
            t.IsDefault = t.Id == template.Id;

        if (template.Id == CurrentTemplate.Id) OnPropertyChanged(nameof(CurrentTemplate));

        _templateStore.Save(Templates);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (CurrentTemplate.IsDefault) return;

        var box = new MessageBox
        {
            Title = "Supprimer",
            Content = $"Supprimer « {CurrentTemplate.Name} » ? Cette action est irreversible.",
            PrimaryButtonText = "Supprimer",
            PrimaryButtonAppearance = ControlAppearance.Danger,
            CloseButtonText = "Annuler",
        };
        MessageBoxResult result = await box.ShowDialogAsync();
        if (result != MessageBoxResult.Primary) return;

        Templates.Remove(CurrentTemplate);
        _templateStore.Save(Templates);
        NewTemplate();
    }

    // ── Elements ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void AddElement(LabelElementType type)
    {
        LabelElement el = type switch
        {
            LabelElementType.Text => new LabelElement { Type = type, Text = "Texte", Width = 300, Height = 40 },
            LabelElementType.Barcode => new LabelElement { Type = type, BarcodeValue = "123456789", Width = 300, Height = 80 },
            LabelElementType.Image => new LabelElement { Type = type, Width = 100, Height = 100 },
            _ => new LabelElement { Type = type }
        };
        el.X = 20;
        el.Y = CurrentTemplate.Elements.Count * 50 + 20;
        CurrentTemplate.Elements.Add(el);
        SelectElement(el);
    }

    [RelayCommand]
    private void SelectElement(LabelElement element)
    {
        foreach (LabelElement el in CurrentTemplate.Elements)
            el.IsSelected = ReferenceEquals(el, element);
        SelectedElement = element;
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (LabelElement el in CurrentTemplate.Elements)
            el.IsSelected = false;
        SelectedElement = null;
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedElement is null) return;
        CurrentTemplate.Elements.Remove(SelectedElement);
        SelectedElement = null;
    }

    [RelayCommand]
    private void MoveUp()
    {
        if (SelectedElement is null) return;
        int i = CurrentTemplate.Elements.IndexOf(SelectedElement);
        if (i > 0) CurrentTemplate.Elements.Move(i, i - 1);
    }

    [RelayCommand]
    private void MoveDown()
    {
        if (SelectedElement is null) return;
        int i = CurrentTemplate.Elements.IndexOf(SelectedElement);
        if (i < CurrentTemplate.Elements.Count - 1) CurrentTemplate.Elements.Move(i, i + 1);
    }

    [RelayCommand]
    private void UploadImage()
    {
        if (SelectedElement is null) return;

        var dialog = new OpenFileDialog
        {
            Title = "Choisir une image",
            Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
        };
        if (dialog.ShowDialog() != true) return;

        var info = new FileInfo(dialog.FileName);
        if (info.Length > 2 * 1024 * 1024) return;

        byte[] bytes = File.ReadAllBytes(dialog.FileName);
        string mime = Path.GetExtension(dialog.FileName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
        SelectedElement.ImageData = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }

    // ── ZPL ───────────────────────────────────────────────────────────────────

    [RelayCommand]
    private void ShowZplPreview()
    {
        string zpl = ZplGenerator.Generate(CurrentTemplate);
        var window = new ZplPreviewWindow(zpl, CurrentTemplate.WidthMm, CurrentTemplate.HeightMm, CurrentTemplate.Dpi)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        window.ShowDialog();
    }

    [RelayCommand]
    private async Task PrintAsync()
    {
        string zpl = ZplGenerator.Generate(CurrentTemplate);
        await ZebraPrinterService.PrintZplAsync(SelectedPrinter, zpl);
    }
}
