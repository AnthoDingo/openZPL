# openZPL

Éditeur d'étiquettes ZPL gratuit et open source pour Windows. Concevez visuellement vos
étiquettes, prévisualisez le code ZPL généré et imprimez directement sur une imprimante
Zebra du réseau.

[![Release](https://img.shields.io/github/v/release/AnthoDingo/openZPL?label=version)](https://github.com/AnthoDingo/openZPL/releases/latest)
[![Licence](https://img.shields.io/badge/licence-GPL--3.0-blue)](LICENSE)

Site du projet : <https://anthodingo.github.io/openZPL/>

## Fonctionnalités

- **Éditeur visuel** — placez et redimensionnez vos éléments sur l'étiquette, avec une liste
  de calques réordonnable par glisser-déposer.
- **Quatre types d'éléments** — texte (taille, gras, italique, alignements), code-barres,
  image et séparateur (trait horizontal ou vertical).
- **Codes-barres** — Code 128, QR Code et Data Matrix, avec ou sans texte lisible (HRI).
- **Dimensions en mm ou en dots** — la conversion suit la résolution de l'imprimante cible
  (203 dpi par défaut).
- **Aperçu du ZPL** — le code ZPL II généré est consultable à tout moment.
- **Impression réseau** — envoi direct en TCP/IP RAW (port 9100) vers toute imprimante
  ZPL II (ZD421, ZD620, ZT400…), avec choix de l'imprimante et du nombre d'exemplaires.
- **Fichiers `.ozpl`** — enregistrez et rouvrez vos étiquettes ; les images étant incluses
  dans le fichier, celui-ci est autonome et peut être copié ou envoyé tel quel.
- **Interface en français et en anglais**, suivant la langue de Windows par défaut.

## Installation

Téléchargez la dernière version depuis la [page des releases](https://github.com/AnthoDingo/openZPL/releases/latest) :

- `openZPL-Setup-X.Y.Z.exe` — installateur : crée les raccourcis, associe les fichiers
  `.ozpl` et gère les mises à jour. L'installation par utilisateur ne demande aucune
  élévation ; une installation pour tout le poste reste proposée.
- `openZPL-X.Y.Z-portable.zip` — archive à extraire dans un dossier, puis lancer
  `openZPL.exe` sans installation.

Les deux versions sont autonomes (*self-contained*) : aucun runtime .NET à installer.

**Prérequis** : Windows 10 version 1809 (build 17763) ou plus récent, 64 bits.

## Utilisation

1. Réglez les dimensions et la résolution de l'étiquette.
2. Ajoutez des éléments (texte, code-barres, image, séparateur) et positionnez-les.
3. Vérifiez le rendu avec l'aperçu ZPL.
4. Imprimez en choisissant l'imprimante et le nombre d'exemplaires, ou enregistrez
   l'étiquette au format `.ozpl` pour la réutiliser.

### Configuration des imprimantes

Les imprimantes se déclarent dans la fenêtre des paramètres : un nom, une adresse IP et un
port (9100 par défaut). L'une d'elles peut être définie comme imprimante par défaut.

L'impression se fait en TCP/IP RAW, sans passer par un pilote Windows : l'imprimante doit
donc être joignable sur le réseau depuis le poste.

### Emplacement des données

| Donnée | Emplacement | Portée |
| --- | --- | --- |
| Imprimantes (`printers.json`) | `%ProgramData%\openZPL` | tous les utilisateurs du poste |
| Préférences (`settings.json`) | `%AppData%\openZPL` | utilisateur courant |

## Compiler depuis les sources

Prérequis : [SDK .NET 10](https://dotnet.microsoft.com/download) et Windows.

```bash
dotnet build openZPL.slnx
```

Lancer l'application :

```bash
dotnet run --project src/openZPL.csproj
```

Publier l'application en mode autonome :

```bash
dotnet publish src/openZPL.csproj -c Release -r win-x64 --self-contained true
```

L'installateur se construit ensuite avec [Inno Setup](https://jrsoftware.org/isinfo.php) 6.3
ou supérieur, à partir du dossier publié :

```bash
iscc Setup/openZPL.iss
```

## Organisation du dépôt

```
src/           application WPF (.NET 10) — MVVM avec CommunityToolkit.Mvvm et WPF-UI
  Models/      modèle d'étiquette et de ses éléments
  Services/    génération ZPL, codes-barres, impression, fichiers .ozpl, préférences
  ViewModels/  logique de l'éditeur
  Views/       éditeur, aperçu ZPL, impression, paramètres imprimantes
  Resources/   icône et chaînes traduites (fr / en)
Setup/         script Inno Setup de l'installateur
pages/         site publié sur GitHub Pages
.github/       workflows et modèles d'issues
```

Une release est publiée automatiquement en poussant un tag de version `X.Y.Z` sur `main`.

## Contribuer

Les rapports de bogue et les propositions de fonctionnalités sont les bienvenus via les
[issues](https://github.com/AnthoDingo/openZPL/issues), en utilisant les modèles proposés.

## Licence

openZPL est distribué sous licence [GNU GPL v3](LICENSE) ou ultérieure.
