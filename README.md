# WinFileSearch

<p align="center">
  <img src="docs/logo.png" alt="WinFileSearch Logo" width="128"/>
</p>

<p align="center">
  <b>Hızlı ve Modern Windows Dosya Arama Uygulaması</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet"/>
  <img src="https://img.shields.io/badge/Platform-Windows-0078D6?style=flat-square&logo=windows"/>
  <img src="https://img.shields.io/badge/UI-WPF-68217A?style=flat-square"/>
  <img src="https://img.shields.io/badge/License-MIT-green?style=flat-square"/>
</p>

---

## 📖 Hakkında

**WinFileSearch**, bilgisayarınızdaki dosyaları hızlıca bulmanızı sağlayan modern bir masaüstü uygulamasıdır. SQLite FTS5 (Full-Text Search) teknolojisi kullanarak milyonlarca dosya içinde bile milisaniyeler içinde arama yapabilirsiniz.

## ✨ Özellikler

- 🔍 **Hızlı Arama** - FTS5 ile anlık full-text arama
- 📁 **Klasör İndeksleme** - İstediğiniz klasörleri indeksleyin
- 🏷️ **Kategori Filtreleri** - Documents, Images, Media, Code
- 👁️ **Quick Preview** - Dosya detaylarını hızlıca görüntüleyin
- 🌙 **Dark Theme** - Göz yormayan modern koyu tema
- 📊 **Gerçek Zamanlı İzleme** - FileSystemWatcher ile otomatik güncelleme
- ⚡ **Async İşlemler** - UI donmadan arka plan indeksleme

## 📸 Ekran Görüntüleri

### Ana Sayfa
![Home Page](docs/screenshots/home.png)

### Arama Sonuçları
![Search Results](docs/screenshots/search.png)

### Ayarlar
![Settings](docs/screenshots/settings.png)

## 🚀 Kurulum

### Gereksinimler
- Windows 10/11
- .NET 8.0 Runtime

### Derlemek İçin
git clone https://github.com/CevdetTufan/WinFileSearch.git 
cd WinFileSearch dotnet build dotnet run --project src/WinFileSearch.UI

### Release İndirme
[Releases](https://github.com/CevdetTufan/WinFileSearch/releases) sayfasından son sürümü indirebilirsiniz.

## 📖 Kullanım

1. **Uygulamayı başlatın**
2. **Settings** sayfasına gidin
3. **Add Folder** ile indekslemek istediğiniz klasörleri ekleyin
4. İndeksleme tamamlanana kadar bekleyin
5. **Search** sayfasından dosyalarınızı arayın
6. Dosyaya çift tıklayarak açın veya **Open Location** ile konumuna gidin

## 🏗️ Mimari
WinFileSearch/ ├── src/ │   ├── WinFileSearch.Data/        # Veri Katmanı │   │   ├── Models/                # Entity modelleri │   │   ├── Repositories/          # Repository pattern │   │   └── FileSearchDbContext.cs # SQLite + FTS5 │   │ │   ├── WinFileSearch.Core/        # İş Mantığı │   │   └── Services/              # Servis implementasyonları │   │ │   └── WinFileSearch.UI/          # Sunum Katmanı (WPF) │       ├── Themes/                # Tema kaynakları │       ├── Converters/            # Value converters │       ├── ViewModels/            # MVVM ViewModels │       └── Views/                 # XAML sayfaları

## 🛠️ Teknolojiler

| Teknoloji | Kullanım Amacı |
|-----------|----------------|
| **.NET 8** | Uygulama framework |
| **WPF** | UI framework |
| **SQLite + FTS5** | Veritabanı ve full-text search |
| **CommunityToolkit.Mvvm** | MVVM pattern |
| **Microsoft.Extensions.DI** | Dependency Injection |

## 📊 Performans

| Metrik | Değer |
|--------|-------|
| İndeksleme hızı | ~10,000 dosya/saniye |
| Arama süresi | <50ms (100K+ dosyada) |
| Bellek kullanımı | ~50-100 MB |
| Veritabanı boyutu | ~1 MB / 10,000 dosya |

## 🤝 Katkıda Bulunma

1. Bu repoyu fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add amazing feature'`)
4. Branch'i push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 👤 Geliştirici

**Cevdet Tufan**

- GitHub: [@CevdetTufan](https://github.com/CevdetTufan)

---

<p align="center">
  ⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!
</p>