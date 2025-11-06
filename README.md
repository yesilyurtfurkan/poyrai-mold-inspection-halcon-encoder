 Mold Inspection System (HALCON + Siemens PLC)

Bu sistem, üretim hattında hareketli kalıpların üzerine yerleştirilen parçaları
HALCON tabanlı görüntü işleme teknikleriyle kontrol eder.  
Sistem, *iemens PLC’den alınan encoder verisine göre tek kamerayı
iki farklı konumda tetikler, iki görüntüyü *tomatik olarak hizalar ve birleştirir  
Birleştirilen görüntü üzerinde, operatör tarafından arayüz üzerinden tanıtılan ROI’ler
(Region of Interest) kullanılarak parçaların varlığı, pozisyonu ve eksiklikleri denetlenir.

---

## ⚙️ Teknik Özellikler

| Bileşen | Açıklama |
|----------|-----------|
| **Kamera** | Endüstriyel area-scan (GigE/USB3) |
| **PLC** | Siemens S7-1200 / S7-1500 |
| **Veri İletişimi** | S7.Net TCP/IP üzerinden encoder pozisyonu alınır |
| **Trigger Mekanizması** | Encoder pozisyonuna göre iki ayrı tetik (ör. 0 mm ve +150 mm) |
| **Görüntü Birleştirme** | HALCON `align_image_rotation` veya `projective_trans_image` tabanlı |
| **Kontrol Tipi** | Parça var/yok, hizalama, eksiklik, deformasyon |
| **Kullanıcı Arayüzü** | C# WinForms – ROI tanıtımı, canlı görüntü ve sonuç ekranı |

---

##  Çalışma Akışı

1. Siemens PLC’den encoder değeri sürekli izlenir.  
2. Parça, belirli pozisyonlara geldiğinde PLC kamera tetiklerini gönderir.  
3. Kamera iki görüntüyü arka arkaya alır: `Image1` ve `Image2`.  
4. HALCON tarafında:
   - Görüntüler encoder ofsetine göre hizalanır,  
   - Birleştirilir (stitching / translation),  
   - Tek birleşik “inspection image” oluşturulur.  
5. Operatörün öğrettiği ROI bölgelerinde her parçanın kontrolü yapılır:  
   - Parça var mı?  
   - Boyutu doğru mu?  
   - Yanlış pozisyonda mı takılmış?  
6. Hatalı (NOK) sonuçlar ekranda kırmızıyla gösterilir ve PLC’ye bildirilir.  

---

## Öğretilebilir ROI Sistemi

- “Teach Mode” aktif edildiğinde:
  - Operatör fareyle ROI alanlarını çizer.  
  - Her ROI’ye isim ve kontrol tipi atanır.  
  - `roi_config.json` dosyasına kaydedilir.
- “Run Mode”da sistem otomatik olarak bu alanları işler.  
- ROI sınırları ve sonuçlar arayüzde renklerle (yeşil/kırmızı) gösterilir.
