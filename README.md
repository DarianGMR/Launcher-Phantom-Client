# 🚀 Launcher Phantom v0.1

Launcher de juegos cliente desarrollado en C# .NET con interfaz WPF moderna y futurista.

## 📋 Requisitos

- Windows 10 / Windows 11
- .NET 8.0 SDK o superior
- Visual Studio 2019 Build Tools o posterior
- Arquitectura: x64 o arm64

## 🔧 Dependencias NuGet
Newtonsoft.Json (13.0.3)
System.Data.SQLite (1.0.118)
BCrypt.Net-Next (4.0.3)
TagLibSharp (2.2.0)

## 📦 Compilación

### Modo Debug

```bash
dotnet build -c Debug
dotnet run
```
### Modo Release

```bash
dotnet build -c Release

Native AOT

dotnet publish -c Release -p:PublishAot=true -r win-x64
```
Esto genera un ejecutable standalone optimizado en:
bin/Release/net8.0-windows/win-x64/publish/LauncherPhantom.exe

## 🎨 Características - Phase 1 (Auth)
✅ Splash Screen

Animación de carga (5 segundos máximo)
Logo Phantom con fade-in/fade-out
Barra de progreso visual
Spinner de carga

✅ Panel de Login

Validación de credenciales local
Campo usuario/email
Campo contraseña
Campo dirección IP servidor
Checkbox "Recuérdame" (encriptado DPAPI)
Mensajes de error claros

✅ Panel de Registro

Validación completa de formulario
Indicador de fortaleza de contraseña
Términos y condiciones
Email y usuario únicos
Link a términos (browser)

✅ Autenticación

Comunicación HTTP con Launcher Server
JWT token encriptado localmente
Manejo de errores servidor
Retry automático

✅ Actualización

Verificación de versión servidor
Diálogo de actualización disponible
Descarga con barra de progreso
Aplicación de actualización

✅ Audio

Sonidos: click, success, error
Reproducción asíncrona

✅ Almacenamiento Local

Encriptación DPAPI de datos sensibles
Base de datos SQLite
Configuración JSON

## 🌐 Configuración del Servidor
El servidor Launcher debe proporcionar estos endpoints:
```bash
POST /api/auth/login
JSON
Request:
{
  "username": "usuario",
  "password": "contraseña"
}

Response (Success):
{
  "success": true,
  "token": "jwt_token",
  "user": {
    "id": "123",
    "username": "usuario",
    "email": "user@example.com"
  }
}

Response (Error):
{
  "success": false,
  "error": "Usuario o contraseña incorrectos"
}
POST /api/auth/register
JSON
Request:
{
  "username": "usuario",
  "email": "user@email.com",
  "password": "contraseña"
}

Response (Success):
{
  "success": true,
  "message": "Cuenta creada exitosamente",
  "userId": "123"
}

Response (Error):
{
  "success": false,
  "error": "El usuario ya existe"
}
GET /api/launcher/version
JSON
Response:
{
  "version": "0.2.0",
  "downloadUrl": "https://..../launcher-update.exe",
  "changes": "- Nueva función\n- Bug fixes",
  "required": false
}
GET /health
Response: 200 OK
```

## 🔐 Encriptación
Los datos sensibles (contraseñas, tokens JWT) se encriptan usando DPAPI de Windows:

C#
var encrypted = EncryptionManager.Instance.Encrypt(plainText);
var decrypted = EncryptionManager.Instance.Decrypt(encryptedText);

## 📁 Estructura de Carpetas
```bash
LauncherPhantom/
├── Views/
│   ├── SplashScreen.xaml
│   ├── LoginPage.xaml
│   ├── RegisterPage.xaml
│   ├── UpdateDialog.xaml
│   └── DownloadProgressWindow.xaml
├── Managers/
│   ├── AuthManager.cs
│   ├── ServerManager.cs
│   ├── UpdateManager.cs
│   ├── EncryptionManager.cs
│   ├── SoundManager.cs
│   ├── ConfigManager.cs
│   ├── ValidationManager.cs
│   └── DatabaseManager.cs
├── Models/
│   ├── Constants.cs
│   ├── AuthResponse.cs
│   ├── LoginRequest.cs
│   ├── RegisterRequest.cs
│   └── VersionInfo.cs
├── Resources/
│   ├── logo.png
│   ├── splash.png
│   ├── click.wav
│   ├── success.wav
│   ├── error.wav
│   └── config.json
└── App.xaml
```

## 🔊 Recursos de Audio
Requiere tres archivos .wav en Resources/:

click.wav - Sonido al hacer clic
success.wav - Sonido de éxito
error.wav - Sonido de error

## ⌨️ Atajos de Teclado
Enter - Enviar formulario (Login/Register)
Escape - Cerrar ventanas de diálogo

## 🐛 Debugging
Habilita el modo debug en config.json:
```bash
JSON
{
  "debug_mode": true
}
```
Ver logs en %APPDATA%\Phantom\launcher.db tabla Logs.

## 🚀 Próximas Fases
Phase 2: Dashboard y gestión de juegos
Phase 3: Descarga y ejecución de juegos
Phase 4: Sistema de mods
Phase 5: Social features (chat, amigos)

## 📝 Licencia
Privado - DarianGMR

## 👤 Autor
DarianGMR
Versión: 0.1.0 Última actualización: 2026-04-29

---

## 📥 DESCARGA

Aquí están **TODOS los archivos listos** para que los copies directamente a tu proyecto Visual Studio.

**Pasos para usar los archivos:**

1. **Crea un nuevo proyecto WPF** en Visual Studio 2019+ (`.NET 8.0`)
2. **Copia todos los archivos** en su estructura de carpetas correspondiente
3. **Instala los NuGet packages**:
Install-Package Newtonsoft.Json Install-Package System.Data.SQLite Install-Package BCrypt.Net-Next Install-Package TagLibSharp

4. **Agrega los recursos** (imágenes y sonidos) en la carpeta `Resources/`
5. **Compila** con `dotnet build`
6. **Ejecuta** con `dotnet run`
