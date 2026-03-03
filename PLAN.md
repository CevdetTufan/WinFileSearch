# 📋 WinFileSearch Geliştirme Planı

> Son Güncelleme: Mart 2026  
> Durum: Aktif Geliştirme

---

## 🎯 Proje Vizyonu

Windows için en hızlı ve kullanıcı dostu dosya arama uygulaması olmak.

---

## ✅ Tamamlanan Özellikler (v1.0)

- [x] SQLite + FTS5 veritabanı altyapısı
- [x] Dosya indeksleme servisi
- [x] Full-text arama (kısmi eşleşme desteği)
- [x] Kategori filtreleri (Documents, Images, Media, Code)
- [x] Klasör yönetimi (dahil et / hariç tut)
- [x] Dark tema UI
- [x] MVVM mimarisi + DI
- [x] Quick Preview panel
- [x] İndeksleme progress gösterimi
- [x] FileSystemWatcher altyapısı

---

## 🚀 Geliştirme Yol Haritası

### Faz 1: Kritik İyileştirmeler (v1.1) 🔴

| # | Görev | Öncelik | Durum | Tahmini Süre |
|---|-------|---------|-------|--------------|
| 1.1 | FileWatcher → DB senkronizasyonu | Yüksek | ⏳ Bekliyor | 2 saat |
| 1.2 | Home → Search navigasyonu | Yüksek | ⏳ Bekliyor | 30 dk |
| 1.3 | Arama debounce (300ms) | Yüksek | ⏳ Bekliyor | 30 dk |
| 1.4 | RebuildIndex sırasında mevcut verileri koruma | Yüksek | ⏳ Bekliyor | 1 saat |

### Faz 2: Kullanıcı Deneyimi (v1.2) 🟡

| # | Görev | Öncelik | Durum | Tahmini Süre |
|---|-------|---------|-------|--------------|
| 2.1 | Keyboard shortcuts (Ctrl+F, Enter, Esc) | Orta | ⏳ Bekliyor | 1 saat |
| 2.2 | Arama geçmişi | Orta | ⏳ Bekliyor | 2 saat |
| 2.3 | Context menu (sağ tık menüsü) | Orta | ⏳ Bekliyor | 1 saat |
| 2.4 | Dosya önizleme (resim, metin) | Orta | ⏳ Bekliyor | 3 saat |
| 2.5 | Drag & Drop klasör ekleme | Orta | ⏳ Bekliyor | 1 saat |

### Faz 3: Gelişmiş Özellikler (v1.3) 🟢

| # | Görev | Öncelik | Durum | Tahmini Süre |
|---|-------|---------|-------|--------------|
| 3.1 | Favoriler / Starred dosyalar | Düşük | ⏳ Bekliyor | 2 saat |
| 3.2 | Sistem tray'e minimize | Düşük | ⏳ Bekliyor | 2 saat |
| 3.3 | Global hotkey (Win+Shift+F) | Düşük | ⏳ Bekliyor | 3 saat |
| 3.4 | Ayarları JSON'a kaydetme | Düşük | ⏳ Bekliyor | 1 saat |
| 3.5 | Otomatik başlatma seçeneği | Düşük | ⏳ Bekliyor | 30 dk |
| 3.6 | Dosya içeriğinde arama | Düşük | ⏳ Bekliyor | 4 saat |

### Faz 4: Kurumsal Özellikler (v2.0) 🔵

| # | Görev | Öncelik | Durum | Tahmini Süre |
|---|-------|---------|-------|--------------|
| 4.1 | Logging sistemi (Serilog) | Gelecek | ⏳ Bekliyor | 2 saat |
| 4.2 | Performans metrikleri | Gelecek | ⏳ Bekliyor | 3 saat |
| 4.3 | Ağ sürücüsü desteği | Gelecek | ⏳ Bekliyor | 4 saat |
| 4.4 | Multi-language desteği | Gelecek | ⏳ Bekliyor | 4 saat |
| 4.5 | Auto-update sistemi | Gelecek | ⏳ Bekliyor | 6 saat |
| 4.6 | Installer (MSIX/MSI) | Gelecek | ⏳ Bekliyor | 4 saat |

---

## 🐛 Bilinen Sorunlar

| # | Sorun | Önem | Durum |
|---|-------|------|-------|
| B1 | Filter toggle butonları görsel olarak senkronize değil | Düşük | Açık |
| B2 | Çok büyük klasörlerde UI donması olabilir | Orta | Açık |
| B3 | Empty state görünümü count=0 için çalışmıyor | Düşük | Açık |

---

## 📁 Dosya Yapısı (Mevcut)
WinFileSearch/ ├── src/ │   ├── WinFileSearch.Data/          ✅ Tamamlandı │   ├── WinFileSearch.Core/          ✅ Tamamlandı │   └── WinFileSearch.UI/            ✅ Tamamlandı ├── docs/                            ⏳ Oluşturulacak │   └── screenshots/ ├── README.md                        ⏳ Oluşturulacak ├── LICENSE                          ⏳ Oluşturulacak └── PLAN.md                          ✅ Bu dosya


---

## 🔧 Teknik Borç

- [ ] Unit testler yazılmalı
- [ ] Integration testler eklenmeli
- [ ] XML documentation tamamlanmalı
- [ ] Code coverage raporlaması
- [ ] CI/CD pipeline (GitHub Actions)

---

## 📊 Performans Hedefleri

| Metrik | Mevcut | Hedef |
|--------|--------|-------|
| İndeksleme hızı | ~10K dosya/sn | ~15K dosya/sn |
| Arama süresi | <100ms | <50ms |
| Bellek kullanımı | ~100MB | <80MB |
| Cold start süresi | ~2sn | <1sn |

---

## 🤝 Katkı Rehberi

1. Issue aç veya mevcut issue'yu sahiplen
2. Feature branch oluştur: `feature/issue-numarasi-kisa-aciklama`
3. Commit mesajları: `feat:`, `fix:`, `docs:`, `refactor:` prefix kullan
4. PR açmadan önce build'in başarılı olduğundan emin ol

---

## 📅 Sürüm Geçmişi

| Sürüm | Tarih | Notlar |
|-------|-------|--------|
| v1.0.0 | Mart 2026 | İlk sürüm - Temel özellikler |
| v1.1.0 | TBD | Kritik iyileştirmeler |
| v1.2.0 | TBD | UX geliştirmeleri |
| v2.0.0 | TBD | Kurumsal özellikler |

---

## 📝 Notlar

- 🤖 Bu proje GitHub Copilot yardımıyla geliştirilmiştir
- Geri bildirimler için: [Issues](https://github.com/CevdetTufan/WinFileSearch/issues)

---

<p align="center">
  <sub>Son güncelleme: Mart 2026</sub>
</p>