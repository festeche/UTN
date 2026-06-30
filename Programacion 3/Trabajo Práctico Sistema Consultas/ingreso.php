<?php
// Iniciar el sistema de gestión de sesiones globales de PHP
session_start();

// Importar la configuración centralizada de conexión a la base de datos (mi_banco_db)
require 'conexion.php';

// Verificar que la petición provenga del formulario mediante el botón "ingresar"
if (isset($_POST['ingresar'])) {
    
    // Capturar y limpiar los datos enviados desde el formulario de ingreso.html
    $tipo_doc  = $_POST['tipo_doc'];
    $documento = trim($_POST['documento']);
    $usuario   = trim($_POST['usuario']);
    $password  = $_POST['password']; // Se procesa en texto plano según especificaciones del TP

    try {
        // Preparar la consulta SQL para validar las credenciales provistas por el usuario
        // Se seleccionan los datos personales necesarios para utilizarlos en el Panel del Cliente (resumen.php)
        $stmt = $pdo->prepare("
            SELECT documento, nombre, apellido, usuario, password 
            FROM usuarios 
            WHERE tipo_doc = ? 
              AND documento = ? 
              AND usuario = ? 
              AND password = ?
        ");
        
        // Ejecutar pasando las variables ordenadas para blindar la app contra Inyección SQL
        $stmt->execute([$tipo_doc, $documento, $usuario, $password]);
        $user = $stmt->fetch(PDO::FETCH_ASSOC);

        // Validar si la base de datos devolvió un registro coincidente y si la cuenta web está activa (no es NULL)
        if ($user && $user['usuario'] !== null) {
            
            // --- INICIAR Y GESTIONAR LA SESIÓN DEL USUARIO ---
            // Guardamos el documento (clave primaria) para rastrear sus tarjetas y liquidaciones asociadas
            $_SESSION['documento'] = $user['documento'];
            
            // Guardamos el nombre y apellido formateados para mostrar la bienvenida personalizada en resumen.php
            $_SESSION['nombre_completo'] = $user['nombre'] . ' ' . $user['apellido'];
            
            // --- REDIRECCIÓN OBLIGATORIA ---
            // Al ser credenciales válidas, se lo redirige de inmediato al panel interactivo
            header("Location: resumen.php");
            exit(); // Finalizar la ejecución del script actual
            
        } else {
            // Si los datos no coinciden o el usuario aún no completó su onboarding digital (usuario/password en NULL)
            echo "<script>
                alert('Error: Credenciales inválidas o cuenta web aún no activada.');
                window.location.href = 'ingreso.html';
            </script>";
            exit();
        }

    } catch (PDOException $e) {
        // En caso de fallas de infraestructura en el motor MySQL
        die("Error crítico en el módulo de autenticación: " . $e->getMessage());
    }
} else {
    // Si intentan ingresar al archivo directamente por URL sin pasar por ingreso.html, se los redirige al login
    header("Location: ingreso.html");
    exit();
}
?>