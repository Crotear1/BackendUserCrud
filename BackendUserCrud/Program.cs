// Program.cs (Auszug)
using UserManagementAPI.Services; // Hinzuf�gen

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// UserService als Singleton oder Scoped registrieren
builder.Services.AddSingleton<UserService>(); // Oder AddScoped, abh�ngig von deinen Bed�rfnissen

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS-Konfiguration (wichtig f�r MAUI App Zugriff)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMauiApp",
        builder =>
        {
            builder.WithOrigins("http://localhost", "https://localhost") // Ggf. anpassen f�r MAUI
                   .AllowAnyHeader()
                   .AllowAnyMethod();
            // F�r MAUI Apps, die nicht �ber einen Browser laufen, ist die Origin-Pr�fung weniger relevant,
            // aber f�r Web-basierte Blazor-Anteile oder Tests mit dem Browser kann es n�tzlich sein.
            // F�r mobile Apps ist AllowAnyOrigin oft einfacher, aber spezifiziere es, wenn m�glich.
            // Wenn deine MAUI App als WebAssembly im Browser l�uft, musst du die Origin der App hier eintragen.
            // F�r native MAUI Apps ist AllowAnyOrigin() sicherer als "*" wenn du spezifische Header brauchst.
            // In diesem Fall ist es f�r den lokalen Entwicklungsbetrieb wahrscheinlich in Ordnung, gro�z�giger zu sein.
            // builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); // Alternativ f�r breitere Akzeptanz
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Stelle sicher, dass die Entwickler-Zertifikate f�r HTTPS vertrauensw�rdig sind
    // F�hre ggf. 'dotnet dev-certs https --trust' in der Kommandozeile aus
}

app.UseHttpsRedirection();

app.UseRouting(); // Sicherstellen, dass Routing vor CORS kommt, wenn spezifische Policies genutzt werden

app.UseCors("AllowMauiApp"); // CORS-Policy anwenden

app.UseAuthorization();

app.MapControllers();

app.Run();