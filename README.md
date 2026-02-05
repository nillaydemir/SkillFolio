# 🎓 SkillFolio – Career & Event Management Platform

SkillFolio, üniversite öğrencilerinin kariyer ve eğitim odaklı etkinlikleri tek bir platformda keşfetmesini, takip etmesini ve kişisel gelişimlerini yönetmesini amaçlayan bir web uygulamasıdır.  

Proje; öğrenciler, yöneticiler (admin) ve sistem arasındaki etkileşimi kapsayan **rol tabanlı** bir yapıda tasarlanmıştır.

---

## 🚀 Projenin Amacı

- Öğrencilerin kariyer, eğitim ve kişisel gelişim etkinliklerini tek bir platformdan takip edebilmesi  
- Etkinlik keşfi, filtreleme, favorileme ve katılım takibi  
- Yöneticilerin (admin) etkinlikleri ve içerikleri merkezi olarak yönetebilmesi  
- Kullanıcı profilleri üzerinden kişiselleştirilmiş bir deneyim sunulması  

---

## 🧩 Temel Özellikler

### 👤 Kullanıcı (Student)
- Kayıt olma & giriş yapma  
- Kişisel profil oluşturma ve düzenleme  
- Etkinlikleri listeleme ve filtreleme  
- Etkinlikleri favorilere ekleme  
- Katılımı onaylanan / sertifikalı etkinlikleri görüntüleme  
- Kendi etkinlik takvimini görüntüleme  
- Profil bilgilerine göre önerilen eğitimleri görme  

### 🛠️ Yönetici (Admin)
- Etkinlik ekleme, düzenleme ve silme  
- Tüm etkinlikleri yönetme  
- Sistem istatistiklerini görüntüleme  
  - Toplam kullanıcı sayısı  
  - Toplam etkinlik sayısı  
  - Sertifika bilgileri (genişletilebilir)  

---

## ⚙️ Proje Nasıl Çalışır?

SkillFolio, **ASP.NET Core MVC** mimarisi üzerine kurulmuş, rol tabanlı bir web uygulamasıdır.

### 🔁 Genel Akış
1. Kullanıcı sisteme kayıt olur veya giriş yapar  
2. Kimlik doğrulama sonrası rolüne göre yönlendirilir  
   - **Student** → Ana sayfa, etkinlikler, profil  
   - **Admin** → Yönetim paneli  
3. Kullanıcılar etkinlikleri keşfeder, filtreler ve favorilerine ekler  
4. Admin kullanıcılar etkinlikleri sistem üzerinden yönetir  
5. Tüm veriler Entity Framework Core aracılığıyla veritabanında saklanır  

---

## 👤 Kullanıcı Akışı (Student)

- Kayıt ve giriş işlemleri `AccountController` üzerinden yürütülür  
- Kullanıcıya ait bilgiler `User` modeli ile temsil edilir  
- Profil sayfasında:
  - Kişisel bilgiler  
  - Favorilenmiş etkinlikler  
  - Katılımı onaylanan / sertifikalı etkinlikler görüntülenir  
- Etkinlik işlemleri `EventsController` üzerinden gerçekleştirilir  
- Yetkilendirme, authentication mekanizması ile kontrol edilir  

---

## 🛠️ Yönetici Akışı (Admin)

- Admin kullanıcılar rol tabanlı yetkilendirme ile korunur  
- Yönetim paneli üzerinden:
  - Etkinlik ekleme, düzenleme ve silme  
  - Tüm etkinlikleri görüntüleme  
  - Sistem istatistiklerini takip etme  
- CRUD işlemleri veritabanı ile senkron çalışır  

---

## ⚙️ Kullanılan Teknolojiler

- ASP.NET Core MVC  
- Entity Framework Core (Code First)  
- Microsoft SQL Server / LocalDB  
- Razor Pages  
- Bootstrap  
- HTML / CSS / JavaScript  
- LINQ  
- Authentication & Authorization (Role-based)  

---

## 🗂️ Proje Mimarisi

Proje **MVC (Model–View–Controller)** mimarisi kullanılarak geliştirilmiştir.

```text
SkillFolio
│
├── Controllers
│   ├── AccountController
│   ├── ProfileController
│   ├── EventsController
│   ├── AnnouncementsController
│   └── AdminController
│
├── Models
│   ├── User
│   ├── Event
│   ├── Announcement
│   └── Comment
│
├── Data
│   └── SkillFolioDbContext
│
├── Views
│   ├── Account
│   ├── Profile
│   ├── Events
│   └── Admin
│
└── Migrations

```

## ▶️ Projeyi Kurma ve Çalıştırma

1. Repository’yi klonlayın:

- git clone https://github.com/username/SkillFolio.git

2. Gerekli bağımlılıkları yükleyin:

- dotnet restore

3. Veritabanını oluşturun (migration’ları uygulayın):

- dotnet ef database update

4. Uygulamayı çalıştırın:

- dotnet run

5. Tarayıcıdan erişin:

- https://localhost:xxxx

---

## 🖥️ Ekran Görüntüleri

<img width="1660" alt="Ana Sayfa" src="LINK_1" />

<img width="1656" alt="Etkinlikler" src="LINK_2" />

<img width="1652" alt="Admin Paneli" src="LINK_3" />

<img width="1652" alt="Kayıt Sayfası" src="LINK_4" />

<img width="1654" alt="Giriş Sayfası" src="LINK_5" />

<img width="1668" alt="Profil Sayfası" src="LINK_6" />


---

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.
Ticari kullanım için uygun değildir.
