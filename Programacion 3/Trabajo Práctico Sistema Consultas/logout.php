<?php
session_start();
// Destruir todas las variables de sesión activas en el servidor
session_destroy();
// Y devuelta al login crack
header("Location: ingreso.html");
exit();
?>