<?php
require 'conexion.php';

if (isset($_POST['registrar'])) {
    // Captura de todos los campos provenientes de registro.html (el trim esta en algunos xq el usuario puede que ponga algun espacio y lo afecte)
    $tipo_doc = $_POST['tipo_doc'];
    $documento = trim($_POST['documento']);
    $nombre = trim($_POST['nombre']);
    $apellido = trim($_POST['apellido']);
    $fecha_nacimiento = $_POST['fecha_nacimiento'];
    $email = trim($_POST['email']);
    $usuario = trim($_POST['usuario']);
    $passwordA = $_POST['passwordA'];
    $passwordB = $_POST['passwordB'];

    // restrcicion del tipo de doc
    if ($tipo_doc !== 'DNI' && $tipo_doc !== 'PASAPORTE') {
        die("Error: Tipo de documento no permitido. <a href='registro.html'>Volver</a>");
    }

    // Que ambas contraseñas coincidan
    if ($passwordA !== $passwordB) {
        die("Error: Las contraseñas ingresadas no coinciden. <a href='registro.html'>Volver</a>");
    }

    try {
        // Evaluacion de los datos
        // Se hace un JOIN con tarjetas xq en los inner join significa que coindicen (tienen en comun, o sea la tarjeta asociada)
        $stmt = $pdo->prepare("
            SELECT u.usuario, t.num_cuenta 
            FROM usuarios u
            INNER JOIN tarjetas t ON u.documento = t.dni_titular
            WHERE u.tipo_doc = ? 
              AND u.documento = ? 
              AND u.nombre = ? 
              AND u.apellido = ? 
              AND u.fecha_nacimiento = ? 
              AND u.email = ?
        ");
        
        $stmt->execute([$tipo_doc, $documento, $nombre, $apellido, $fecha_nacimiento, $email]);
        $cliente = $stmt->fetch(PDO::FETCH_ASSOC);

        // Si la consulta no devuelve filas, significa que algún dato personal no coincide 
        // o que el cliente directamente no tiene una tarjeta emitida a su nombre.
        if (!$cliente) {
            echo "<div style='font-family:sans-serif; text-align:center; margin-top:50px;'>";
            echo "<h2 style='color:#dc2626;'>Validación Fallida</h2>";
            echo "<p>Los datos ingresados no coinciden con nuestros registros de clientes con tarjeta Progra3card activa.</p>";
            echo "<p style='color:gray; text-align:center; max-width:500px; margin: 10px auto;'>Por favor, verifique que su Nombre, Apellido, Correo y Fecha de Nacimiento sean exactamente los mismos declarados en la sucursal del banco.</p>";
            echo "<a href='registro.html' style='color:#004691; font-weight:bold;'>Intentar de nuevo</a>";
            echo "</div>";
            exit;
        }

        // esto es por si devuelve pero el usuario ya esta completado, por ende la cuenta ya se habia dado de alta antes
        if ($cliente['usuario'] !== null) {
            echo "<div style='font-family:sans-serif; text-align:center; margin-top:50px;'>";
            echo "<h2 style='color:#004691;'>Cuenta ya activa</h2>";
            echo "<p>Usted ya dispone de credenciales de acceso creadas para este documento.</p>";
            echo "<a href='ingreso.html' style='color:#004691; font-weight:bold;'>Ir al Inicio de Sesión</a>";
            echo "</div>";
            exit;
        }

        // Datos verificados. Se updatea el usuario del cliente y su contraseña a la BD
        try {
            $update = $pdo->prepare("UPDATE usuarios SET usuario = ?, password = ? WHERE documento = ? AND tipo_doc = ?");
            $update->execute([$usuario, $passwordA, $documento, $tipo_doc]);

            echo "<div style='font-family:sans-serif; text-align:center; margin-top:50px;'>";
            echo "<h2 style='color:#16a34a;'>¡Onboarding Digital Exitoso!</h2>";
            echo "<p>Tu cuenta web ha sido vinculada correctamente a tu tarjeta Nro Cuenta: <strong>" . $cliente['num_cuenta'] . "</strong>.</p>";
            echo "<a href='ingreso.html' style='color:#004691; font-weight:bold; font-size:18px;'>Ingresar al Portal</a>";
            echo "</div>";

        } catch (PDOException $e) {
            // Control de restricción UNIQUE si el Nombre de Usuario elegido ya existe en el sistema
            if ($e->getCode() == 23000) {
                echo "Error: El nombre de usuario web <strong>'$usuario'</strong> ya se encuentra registrado por otra persona. <a href='registro.html'>Elegir otro</a>";
            } else {
                throw $e;
            }
        }

    } catch (PDOException $e) {
        echo "Error crítico en el procesamiento de datos de la base: " . $e->getMessage();
    }
}
?>