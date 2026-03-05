# WinFileSearch Installer

Bu klasör Inno Setup installer script'ini içerir.

## Dosyalar

- `WinFileSearch.iss` - Inno Setup script

## Kullanım

### Lokal olarak build etmek için:

1. [Inno Setup](https://jrsoftware.org/isdl.php) yükleyin
2. Projeyi build edin ve publish edin:
   ```
   dotnet publish src/WinFileSearch.UI/WinFileSearch.UI.csproj -c Release -r win-x64 --self-contained -o ./publish/win-x64
   ```
3. `installer` klasöründe:
   ```
   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" WinFileSearch.iss
   ```

### GitHub Actions

Tag push edildiğinde otomatik olarak installer oluşturulur.

## Not

`WinFileSearch.iss` dosyasında tüm yollar göreceli (relative) olmalıdır:
- Icon: `..\src\WinFileSearch.UI\Resources\app.ico`
- Source: `..\publish\win-x64\*`
- OutputDir: `..`
