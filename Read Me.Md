this an intial project to start



//appsettings file
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=.;Database=project1;Trusted_Connection=True;TrustServerCertificate=True",
        "REDIS_URL": ""
    },
    "CacheSettings": {
        "Redis": {
            "Enabled": true,
            "DefaultExpirationMinutes": 1,
            "ConnectTimeoutMs": 10000,
            "SyncTimeoutMs": 10000
        }
    },
    "RateLimiting": {
        "Authenticated": {
            "Requests": 100,
            "WindowSeconds": 60
        },
        "Anonymous": {
            "Requests": 30,
            "WindowSeconds": 60
        },
        "Global": {
            "Requests": 1000,
            "WindowSeconds": 60
        },
        "ExcludedPaths": [
            "/health",
            "/swagger",
            "/.well-known",
            "/metrics"
        ],
        "ClientIdCookieName": "X-Client-Id",
        "ClientIdCookieExpirationDays": 30
    },
    "LoginRateLimiter": {
        "MaxAttempts": 5,
        "AttemptWindowMinutes": 15,
        "BanDurationHours": 3
    },
    "CloudinarySettings": {
        "CloudName": "",
        "ApiKey": "",
        "ApiSecret": ""
    },
    "JWT": {
        "Key": "Projectpasskeygaberemadgaberbadrthisisaverylongkeyabcdefghi",
        "Issuer": "https://localhost:7237/",
        "Audience": "project1",
        "AccessTokenExpirationMinutes": 15,
        "RefreshTokenExpirationDays": 7
    },
    "Email": {
        "Host": "smtp.gmail.com",
        "Port": 587,
        "SenderEmail": "gshevb9@gmail.com",
        "SenderPassword": "",
        "SenderDisplayName": "Gaber Company",
        "EnableSsl": true,
        "ExpirationMinutes": 3
    },
    "Authentication": {
        "Google": {
            "ClientId": "",
            "ClientSecret": ""
        }
    },
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*",
    "BaseURL": "https://localhost:7237/",
    "Frontend": {
        "AllowedBaseUrls": [
            "http://127.0.0.1:5500"
        ]
    }
}
// Add-Migration InitialCreate -OutputDir Persistence\Migrations