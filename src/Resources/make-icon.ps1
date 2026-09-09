# openZPL
# Copyright (C) 2026 AnthoDingo
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see <https://www.gnu.org/licenses/>.

# Regenere openZPL.ico : l'icone est dessinee, pas peinte, pour rester nette a
# chaque taille. A relancer apres toute retouche du dessin.
#   pwsh -File src\Resources\make-icon.ps1

param([string]$IcoOut = "$PSScriptRoot\openZPL.ico")
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase

function Rect([double]$x, [double]$y, [double]$w, [double]$h) {
    New-Object System.Windows.Rect $x, $y, $w, $h
}

function New-Brush([string]$hex) {
    $c = [System.Windows.Media.ColorConverter]::ConvertFromString($hex)
    $b = New-Object System.Windows.Media.SolidColorBrush $c
    $b.Freeze(); $b
}
$Blue  = New-Brush '#0D6EFD'   # fond, l'accent deja utilise dans le canvas
$White = New-Brush '#FFFFFF'   # l'etiquette
$Ink   = New-Brush '#0B1F3A'   # les barres
$Grey  = New-Brush '#93A3B8'   # la ligne de valeur sous le code-barres

function Render-Icon([int]$size) {
    $dv = New-Object System.Windows.Media.DrawingVisual
    $dc = $dv.RenderOpen()
    $s  = [double]$size

    # fond : carre a coins arrondis, plein cadre
    $dc.DrawRoundedRectangle($Blue, $null, (Rect 0 0 $s $s), (0.22*$s), (0.22*$s))

    # l'etiquette
    $lx = 0.17*$s; $ly = 0.23*$s; $lw = 0.66*$s; $lh = 0.54*$s
    $dc.DrawRoundedRectangle($White, $null, (Rect $lx $ly $lw $lh), (0.06*$s), (0.06*$s))

    # code-barres : moins de barres, plus epaisses, aux petites tailles
    $simple = $size -le 24
    $pattern = if ($simple) { @(2,1,2,1,2) } else { @(2,1,1,2,1,3,1,1,2) }
    $barTop = $ly + 0.14*$lh
    $barH   = if ($simple) { 0.72*$lh } else { 0.55*$lh }
    $inner  = 0.14*$lw
    $span   = $lw - 2*$inner
    $unit   = $span / ($pattern | Measure-Object -Sum).Sum
    $x = $lx + $inner
    for ($i = 0; $i -lt $pattern.Count; $i++) {
        $w = $pattern[$i] * $unit
        if ($i % 2 -eq 0) {
            $dc.DrawRectangle($Ink, $null, (Rect $x $barTop $w $barH))
        }
        $x += $w
    }

    # ligne de valeur sous les barres, seulement quand elle reste lisible
    if (-not $simple) {
        $ty = $barTop + $barH + 0.10*$lh
        $th = 0.11*$lh
        $dc.DrawRoundedRectangle($Grey, $null,
            (Rect ($lx + 0.26*$lw) $ty (0.48*$lw) $th), ($th/2), ($th/2))
    }
    $dc.Close()

    $rtb = New-Object System.Windows.Media.Imaging.RenderTargetBitmap(
        $size, $size, 96, 96, [System.Windows.Media.PixelFormats]::Pbgra32)
    $rtb.Render($dv)
    $rtb
}

function Get-PngBytes($bitmap) {
    $enc = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    # sans $null =, la valeur de retour de Add() se retrouve dans la sortie
    $null = $enc.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create([System.Windows.Media.Imaging.BitmapSource]$bitmap))
    $ms = New-Object IO.MemoryStream
    $enc.Save($ms)
    # la virgule empeche PowerShell de derouler le tableau d'octets
    return ,$ms.ToArray()
}

$sizes = 16, 24, 32, 48, 64, 128, 256
$images = @{}
foreach ($s in $sizes) { $images[$s] = Get-PngBytes (Render-Icon $s) }

# ── conteneur ICO : en-tete, repertoire, puis les PNG ────────────────────────
$fs = [IO.File]::Create($IcoOut)
$bw = New-Object IO.BinaryWriter($fs)
$bw.Write([uint16]0); $bw.Write([uint16]1); $bw.Write([uint16]$sizes.Count)
$offset = 6 + 16*$sizes.Count
foreach ($s in $sizes) {
    $bytes = $images[$s]
    $bw.Write([byte]($(if ($s -ge 256) { 0 } else { $s })))   # 0 = 256
    $bw.Write([byte]($(if ($s -ge 256) { 0 } else { $s })))
    $bw.Write([byte]0)            # palette
    $bw.Write([byte]0)            # reserve
    $bw.Write([uint16]1)          # plans
    $bw.Write([uint16]32)         # bits par pixel
    $bw.Write([uint32]$bytes.Length)
    $bw.Write([uint32]$offset)
    $offset += $bytes.Length
}
foreach ($s in $sizes) { $bw.Write($images[$s]) }
$bw.Flush(); $fs.Close()
"ico : $IcoOut ($((Get-Item $IcoOut).Length) octets, $($sizes.Count) tailles)"
