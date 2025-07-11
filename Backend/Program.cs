// Gerekli kütüphaneleri ve sınıfları içeri aktarıyoruz
using Backend.Data;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Servisleri Konteynera Ekleme ---

// Tarayıcıdan gelen API isteklerine izin vermek için CORS (Cross-Origin Resource Sharing) servisini ekliyoruz.
// Bu ayar, HTML dosyanızın API'nizle konuşabilmesi için gereklidir.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


// Projemize Controller'ları kullanacağımızı söylüyoruz.
builder.Services.AddControllers();

// appsettings.json dosyasından PostgreSQL bağlantı dizesini alıyoruz.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Entity Framework DbContext'i PostgreSQL kullanacak şekilde yapılandırıyoruz.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Kendi yazdığımız ProductService'i Dependency Injection için kaydediyoruz.
// Bu sayede Controller içinde IProductService istediğimizde, ASP.NET Core bize bir ProductService nesnesi verecek.
builder.Services.AddScoped<IProductService, ProductService>();


// Swagger/OpenAPI (API dokümantasyonu) servislerini ekliyoruz.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --- 2. HTTP İstek Hattını (Pipeline) Yapılandırma ---
var app = builder.Build();

// Sadece geliştirme ortamındaysak Swagger'ı kullan.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP isteklerini otomatik olarak HTTPS'e yönlendir.
app.UseHttpsRedirection();

// Yukarıda tanımladığımız CORS politikasını aktif hale getiriyoruz.
app.UseCors("AllowAllOrigins");

// Yetkilendirme (Authorization) ara katmanını ekle.
app.UseAuthorization();

// Gelen istekleri ilgili Controller'a yönlendir.
app.MapControllers();

// Uygulamayı çalıştır.
app.Run();