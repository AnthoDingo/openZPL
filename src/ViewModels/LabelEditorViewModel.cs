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
using openZPL.Localization;
using openZPL.Models;
using openZPL.Resources;
using openZPL.Services;
using openZPL.Views;
using Wpf.Ui.Controls;

namespace openZPL.ViewModels;

public partial class LabelEditorViewModel : ObservableObject
{
    private readonly PrinterSettingsStore _printerStore = new();
    private readonly AppSettingsStore _appSettingsStore = new();
    private readonly AppSettingsData _appSettings;

    public ObservableCollection<PrinterProfile> Printers { get; } = [];

    public Array BarcodeTypeValues => Enum.GetValues(typeof(LabelBarcodeType));
    public int[] DpiValues => [203, 300, 600];

    [ObservableProperty] private LabelTemplate _currentTemplate = new();
    [ObservableProperty] private LabelElement? _selectedElement;
    [ObservableProperty] private PrinterProfile? _selectedPrinter;
    [ObservableProperty] private double _scale = 1.0;

    /// <summary>Fichier .ozpl ouvert, null tant que l'etiquette n'a jamais ete
    /// enregistree (Enregistrer demande alors ou la mettre).</summary>
    [ObservableProperty] private string? _currentFilePath;

    public bool HasSelection => SelectedElement is not null;
    public bool NoSelection => SelectedElement is null;
    public int ZoomPercent => (int)Math.Round(Scale * 100);
    /// <summary>Le fichier ouvert identifie l'etiquette ; tant qu'elle n'est pas
    /// enregistree, on affiche son nom.</summary>
    public string WindowTitle => string.IsNullOrEmpty(CurrentFilePath)
        ? $"{CurrentTemplate.Name} - openZPL"
        : $"{Path.GetFileName(CurrentFilePath)} - openZPL";

    public LabelEditorViewModel()
    {
        _appSettings = _appSettingsStore.Load();
        CurrentTemplate.Name = Strings.LabelDefaultName;
        LoadPrinters();
    }

    // ── Langue ────────────────────────────────────────────────────────────────

    public bool IsFrench => Localization.Language.Current == Localization.Language.French;
    public bool IsEnglish => Localization.Language.Current == Localization.Language.English;

    /// <summary>Change la langue de l'interface et retient le choix. Les textes
    /// deja affiches se retraduisent, sans redemarrage.</summary>
    [RelayCommand]
    private void SetLanguage(string code)
    {
        if (Localization.Language.Current == code) return;

        Localization.Language.Apply(code);

        _appSettings.Language = code;
        _appSettingsStore.Save(_appSettings);

        // le nom par defaut suit la langue tant que l'etiquette n'a pas de fichier
        if (CurrentFilePath is null && CurrentTemplate.Elements.Count == 0)
            CurrentTemplate.Name = Strings.LabelDefaultName;

        OnPropertyChanged(nameof(IsFrench));
        OnPropertyChanged(nameof(IsEnglish));
        OnPropertyChanged(nameof(WindowTitle));
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
        OnPropertyChanged(nameof(WindowTitle));
    }

    partial void OnScaleChanged(double value) => OnPropertyChanged(nameof(ZoomPercent));

    partial void OnCurrentFilePathChanged(string? value) => OnPropertyChanged(nameof(WindowTitle));

    // ── Fichier ───────────────────────────────────────────────────────────────

    [RelayCommand]
    private void New()
    {
        CurrentFilePath = null;
        CurrentTemplate = new LabelTemplate { Name = Strings.LabelDefaultName };
    }

    /// <summary>Enregistre dans le fichier .ozpl ouvert, ou demande ou le creer
    /// si l'etiquette n'en a pas encore.</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentTemplate.Name)) return;

        if (string.IsNullOrEmpty(CurrentFilePath))
        {
            await SaveAsAsync();
            return;
        }

        await WriteFileAsync(CurrentFilePath);
    }

    [RelayCommand]
    private async Task SaveAsAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentTemplate.Name)) return;

        var dialog = new SaveFileDialog
        {
            Title = Strings.DialogSaveLabel,
            Filter = LabelFile.SaveFilter,
            DefaultExt = LabelFile.Extension,
            AddExtension = true,
            // une etiquette deja enregistree se repropose a sa place, sinon on
            // repart du dernier dossier utilise
            FileName = string.IsNullOrEmpty(CurrentFilePath)
                ? LabelFile.SuggestFileName(CurrentTemplate.Name)
                : CurrentFilePath,
            InitialDirectory = LastDirectory(),
        };
        if (dialog.ShowDialog() != true) return;

        await WriteFileAsync(dialog.FileName);
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = Strings.DialogOpenLabel,
            Filter = LabelFile.OpenFilter,
            DefaultExt = LabelFile.Extension,
            InitialDirectory = LastDirectory(),
        };
        if (dialog.ShowDialog() != true) return;

        await LoadFileAsync(dialog.FileName);
    }

    /// <summary>Ouvre un fichier .ozpl deja designe — par la boite de dialogue,
    /// ou par un double-clic dans l'Explorateur.</summary>
    public async Task LoadFileAsync(string path)
    {
        LabelTemplate? template;
        try
        {
            template = LabelFile.Load(path);
        }
        catch (IOException ex)
        {
            await ShowErrorAsync(Strings.ErrorOpenTitle, ex.Message);
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowErrorAsync(Strings.ErrorOpenTitle, ex.Message);
            return;
        }

        if (template is null)
        {
            await ShowErrorAsync(Strings.ErrorOpenTitle,
                string.Format(Strings.ErrorNotOpenZpl, Path.GetFileName(path)));
            return;
        }

        CurrentTemplate = template;
        CurrentFilePath = path;
        RememberDirectory(path);
    }

    /// <summary>Dossier ou rouvrir les boites de dialogue : celui du fichier
    /// courant, sinon le dernier utilise. Vide si le dossier memorise a disparu
    /// (cle USB retiree, dossier reseau hors ligne) — Windows choisit alors.</summary>
    private string LastDirectory()
    {
        string? dir = string.IsNullOrEmpty(CurrentFilePath)
            ? _appSettings.LastDirectory
            : Path.GetDirectoryName(CurrentFilePath);

        return dir is not null && Directory.Exists(dir) ? dir : string.Empty;
    }

    /// <summary>Memorise le dossier du fichier ouvert ou enregistre, pour la
    /// prochaine boite de dialogue et pour les prochains lancements.</summary>
    private void RememberDirectory(string path)
    {
        string? dir = Path.GetDirectoryName(path);
        if (dir is null || dir == _appSettings.LastDirectory) return;

        _appSettings.LastDirectory = dir;
        _appSettingsStore.Save(_appSettings);
    }

    private async Task WriteFileAsync(string path)
    {
        try
        {
            LabelFile.Save(path, CurrentTemplate);
        }
        catch (IOException ex)
        {
            await ShowErrorAsync(Strings.ErrorSaveTitle, ex.Message);
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            await ShowErrorAsync(Strings.ErrorSaveTitle, ex.Message);
            return;
        }

        CurrentFilePath = path;
        RememberDirectory(path);
    }

    private static async Task ShowErrorAsync(string title, string message)
    {
        var box = new MessageBox
        {
            Title = title,
            Content = message,
            CloseButtonText = Strings.ButtonClose,
        };
        await box.ShowDialogAsync();
    }

    // ── Elements ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void AddElement(LabelElementType type)
    {
        LabelElement el = type switch
        {
            LabelElementType.Text => new LabelElement { Type = type, Text = Strings.DefaultTextContent, Width = 300, Height = 40 },
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
            Title = Strings.DialogChooseImage,
            Filter = Strings.FilterImages,
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
        var dialog = new PrintWindow(Printers, SelectedPrinter)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        if (dialog.ShowDialog() != true) return;

        // le choix devient celui propose a la prochaine impression
        SelectedPrinter = dialog.SelectedPrinter;

        string zpl = ZplGenerator.Generate(CurrentTemplate, dialog.Copies);
        (bool ok, string? error) = await ZebraPrinterService.PrintZplAsync(SelectedPrinter, zpl);

        if (!ok)
            await ShowErrorAsync(Strings.ErrorPrintTitle, error ?? Strings.ErrorUnknown);
    }
}
