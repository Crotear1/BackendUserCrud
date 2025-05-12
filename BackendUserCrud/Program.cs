// Program.cs (Auszug)
using UserManagementAPI.Services; // Hinzufügen

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// UserService als Singleton oder Scoped registrieren
builder.Services.AddSingleton<UserService>(); // Oder AddScoped, abhängig von deinen Bedürfnissen

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS-Konfiguration (wichtig für MAUI App Zugriff)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMauiApp",
        builder =>
        {
            builder.WithOrigins("http://localhost", "https://localhost") // Ggf. anpassen für MAUI
                   .AllowAnyHeader()
                   .AllowAnyMethod();
            // Für MAUI Apps, die nicht über einen Browser laufen, ist die Origin-Prüfung weniger relevant,
            // aber für Web-basierte Blazor-Anteile oder Tests mit dem Browser kann es nützlich sein.
            // Für mobile Apps ist AllowAnyOrigin oft einfacher, aber spezifiziere es, wenn möglich.
            // Wenn deine MAUI App als WebAssembly im Browser läuft, musst du die Origin der App hier eintragen.
            // Für native MAUI Apps ist AllowAnyOrigin() sicherer als "*" wenn du spezifische Header brauchst.
            // In diesem Fall ist es für den lokalen Entwicklungsbetrieb wahrscheinlich in Ordnung, großzügiger zu sein.
            // builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); // Alternativ für breitere Akzeptanz
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Stelle sicher, dass die Entwickler-Zertifikate für HTTPS vertrauenswürdig sind
    // Führe ggf. 'dotnet dev-certs https --trust' in der Kommandozeile aus
}

app.UseHttpsRedirection();

app.UseRouting(); // Sicherstellen, dass Routing vor CORS kommt, wenn spezifische Policies genutzt werden

app.UseCors("AllowMauiApp"); // CORS-Policy anwenden

app.UseAuthorization();

app.MapControllers();

app.Run();