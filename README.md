# 🚀 Guía de Ejecución - SubastaYa

Este proyecto consta de una API en .NET 8, un Frontend en React (Vite) y una base de datos SQL Server contenida en Docker. 
El sistema incluye un **Seeder automático** que poblará la base de datos con categorías, usuarios y subastas de prueba al iniciarse por primera vez.

## Pasos para ejecutar el entorno local:

### 1. Variables de Entorno y Base de Datos (Docker)

1. En la raíz del proyecto, duplicar el archivo `.env.example` y renombrarlo a `.env`. 
Los valores predeterminados (incluyendo la contraseña de SQL Server y la API Key de ImgBB) ya están configurados allí.
2. Abrir una terminal en la raíz del proyecto y levantar el contenedor ejecutando:

docker-compose up -d

(Asegurarse de tener el puerto 1433 libre para SQL Server).

### 2. Configuración de Secretos y Backend (.NET 8)

Para cumplir con las normativas de seguridad, la cadena de conexión no está versionada en el código fuente.
  
1. Abrir una terminal dentro de la carpeta SubastaYa/SubastaYa/ y ejecutar el siguiente comando para inyectar la configuración localmente en la bóveda de secretos:

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=SubastaYaDb;User Id=sa;Password=SubastaYa_2026!;TrustServerCertificate=True;"

2. Abrir la solución SubastaYa.slnx en Visual Studio 2026.  
3. Abrir la Consola del Administrador de Paquetes (Package Manager Console). 
En el menú desplegable "Proyecto predeterminado", seleccionar SubastaYa.Infrastructure (ya que allí residen las migraciones) y ejecutar:

Update-Database

4. Asegurarse de que el proyecto de inicio sea SubastaYa.Api y ejecutar con el perfil https.
(La API correrá en https://localhost:7109 y el Swagger se abrirá automáticamente, poblando la base de datos mediante el Seeder).

### 3. Frontend (React + Vite)

1. Abrir Visual Studio Code en la carpeta Frontend/
2. Abrir una terminal integrada e instalar las dependencias:

npm install

3. Levantar el servidor de desarrollo:

npm run dev

(La aplicación estará disponible en http://localhost:5173

🧪 Usuarios de Prueba

Todos los usuarios de prueba tienen como contraseña predeterminada: Clave123!

-> comprador1@test.com (Posee saldo en la billetera listo para pujar

-> vendedor@test.com (Posee subastas publicadas
