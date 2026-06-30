<?php
$host     = 'localhost';
$port     = '3306';
$db       = 'mi_banco_db';
$user     = 'root';
$password = 'root';
$charset  = 'utf8mb4'; 

$dsn = "mysql:host=$host;port=$port;dbname=$db;charset=$charset";

$options = [
    // Activar el manejo de errores mediante Excepciones (requerido para los try-catch de altas e ingreso)
    PDO::ATTR_ERRMODE            => PDO::ERRMODE_EXCEPTION,
    // Configurar el modo de obtención por defecto como array asociativo
    PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
    // Desactivar emulación de sentencias preparadas para mayor seguridad real contra inyecciones SQL
    PDO::ATTR_EMULATE_PREPARES   => false,
];

try {
    // Inicialización del objeto global $pdo utilizado por el resto de los módulos
    $pdo = new PDO($dsn, $user, $password, $options);
    
} catch (\PDOException $e) {
    
    die("Error crítico de infraestructura: No se pudo conectar a la base de datos. Detalle: " . $e->getMessage());
}
?>