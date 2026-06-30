<?php
// 1. SEGURIDAD: Restringir el acceso a usuarios no autenticados
session_start();

if (!isset($_SESSION['documento'])) {
    header("Location: ingreso.html");
    exit();
}

// Importar la conexión configurada previamente
require 'conexion.php';
$documento = $_SESSION['documento'];

try {
    // 2. JOIN CORRESPONDIENTE: Traer todos los datos del cliente y de su única tarjeta vinculada
    // Al ser una relación estricta 1:1, se asume que el registro de la tarjeta siempre existe.
    $stmt = $pdo->prepare("
        SELECT u.documento, u.tipo_doc, u.nombre, u.apellido, u.fecha_nacimiento, u.email, u.usuario,
               t.num_cuenta, t.numero_tarjeta, t.banco_emisor, t.estado, t.saldo
        FROM usuarios u
        INNER JOIN tarjetas t ON u.documento = t.dni_titular
        WHERE u.documento = ?
    ");
    $stmt->execute([$documento]);
    
    // Al no requerir manejo con rows múltiples para la tarjeta, se hace un fetch directo de la fila única
    $datos = $stmt->fetch(PDO::FETCH_ASSOC);

    // 3. CONSULTA DE LIQUIDACIONES: Traer el historial completo ordenado por período de forma descendente
    // De esta manera, el primer elemento que retorne el motor será el período más reciente.
    $stmtLiq = $pdo->prepare("
        SELECT periodo, fecha_vencimiento, total_a_pagar, pago_minimo
        FROM liquidaciones
        WHERE num_cuenta = ?
        ORDER BY periodo DESC
    ");
    $stmtLiq->execute([$datos['num_cuenta']]);

    // LIQUIDACIÓN ACTUAL: Extraemos la primera fila devuelta por la consulta (el período más reciente)
    $liquidacion_actual = $stmtLiq->fetch(PDO::FETCH_ASSOC);

} catch (PDOException $e) {
    die("Error en la base de datos: " . $e->getMessage());
}
?>
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Panel del Cliente - Resumen</title>
    <script src="https://cdn.tailwindcss.com"></script>    <!-- Aplicado el estilo que le puso el profe a ingreso.html (claro, con ayuda de ciertas herramientas.) -->
</head>
<body class="bg-gray-100 font-sans min-h-screen flex flex-col justify-between">

    <header class="bg-[#004691] text-white py-4 px-6 shadow-md flex justify-between items-center">
        <h1 class="text-xl font-semibold">Mis <span class="font-bold">Tarjetas</span></h1>
        <a href="logout.php" class="bg-red-600 hover:bg-red-700 text-white text-xs font-medium py-2 px-4 rounded-full transition duration-200">
            Cerrar Sesión
        </a>
    </header>

    <main class="flex-grow max-w-6xl w-full mx-auto p-4 md:p-8 space-y-6">
        <div class="bg-white rounded-lg shadow p-6 border-l-4 border-blue-500">
            <p class="text-gray-500 text-sm">Panel de Consultas</p>
            <h2 class="text-2xl font-bold text-gray-800">¡Bienvenido/a, <?php echo htmlspecialchars($_SESSION['nombre_completo']); ?>!</h2>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div class="md:col-span-2 space-y-6">
                <div class="bg-white rounded-lg shadow p-6">
                    <h3 class="text-lg font-bold text-[#004691] border-b pb-2 mb-4">Información de la Cuenta</h3>
                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 text-sm text-gray-700">
                        <div><span class="font-semibold text-gray-500">Titular:</span> <?php echo htmlspecialchars($datos['nombre'] . ' ' . $datos['apellido']); ?></div>
                        <div><span class="font-semibold text-gray-500">Documento:</span> <?php echo htmlspecialchars($datos['tipo_doc'] . ' ' . $datos['documento']); ?></div>
                        <div><span class="font-semibold text-gray-500">Fecha de Nacimiento:</span> <?php echo htmlspecialchars($datos['fecha_nacimiento']); ?></div>
                        <div><span class="font-semibold text-gray-500">Email:</span> <?php echo htmlspecialchars($datos['email']); ?></div>
                        <div><span class="font-semibold text-gray-500">Usuario Web:</span> <?php echo htmlspecialchars($datos['usuario']); ?></div>
                        <div><span class="font-semibold text-gray-500">N° Cuenta Relacional:</span> <?php echo htmlspecialchars($datos['num_cuenta']); ?></div>
                    </div>
                </div>

                <div class="bg-gradient-to-br from-[#004691] to-blue-800 text-white rounded-2xl shadow-xl p-6 relative overflow-hidden max-w-sm mx-auto md:mx-0">
                    <div class="absolute right-4 bottom-4 opacity-10 font-bold text-6xl">Progra3</div>
                    <div class="flex justify-between items-start mb-8">
                        <div>
                            <p class="text-[10px] uppercase tracking-wider opacity-75">Banco Emisor</p>
                            <p class="font-bold text-lg"><?php echo htmlspecialchars($datos['banco_emisor']); ?></p>
                        </div>
                        <span class="px-3 py-1 text-[10px] font-bold uppercase rounded-full <?php echo $datos['estado'] === 'Activa' ? 'bg-green-500' : 'bg-red-500'; ?>">
                            <?php echo htmlspecialchars($datos['estado']); ?>
                        </span>
                    </div>
                    <div class="mb-6">
                        <p class="text-[10px] uppercase tracking-wider opacity-75 mb-1">Número de Tarjeta</p>
                        <p class="text-xl font-mono tracking-widest"><?php echo implode(' ', str_split(htmlspecialchars($datos['numero_tarjeta']), 4)); ?></p>
                    </div>
                    <div class="flex justify-between items-end">
                        <div>
                            <p class="text-[10px] uppercase tracking-wider opacity-75">Saldo Consolidado</p>
                            <p class="text-2xl font-bold">$<?php echo number_format($datos['saldo'], 2, ',', '.'); ?></p>
                        </div>
                    </div>
                </div>
            </div>

            <div class="bg-white rounded-lg shadow p-6 flex flex-col justify-between border-t-4 border-[#004691]">
                <div>
                    <h3 class="text-lg font-bold text-[#004691] border-b pb-2 mb-4">Liquidación Actual</h3>
                    <?php if ($liquidacion_actual): ?>
                        <div class="space-y-4">
                            <div class="flex justify-between border-b pb-2">
                                <span class="text-sm font-semibold text-gray-500">Período:</span>
                                <span class="text-sm font-bold text-gray-800"><?php echo htmlspecialchars($liquidacion_actual['periodo']); ?></span>
                            </div>
                            <div class="flex justify-between border-b pb-2">
                                <span class="text-sm font-semibold text-gray-500">Vencimiento:</span>
                                <span class="text-sm font-bold text-red-600"><?php echo htmlspecialchars($liquidacion_actual['fecha_vencimiento']); ?></span>
                            </div>
                            <div class="flex justify-between border-b pb-2">
                                <span class="text-sm font-semibold text-gray-500">Pago Mínimo:</span>
                                <span class="text-sm font-bold text-gray-800">$<?php echo number_format($liquidacion_actual['pago_minimo'], 2, ',', '.'); ?></span>
                            </div>
                            <div class="pt-2">
                                <span class="block text-xs font-semibold text-gray-500 uppercase mb-1">Total a Pagar:</span>
                                <span class="text-3xl font-extrabold text-[#004691]">$<?php echo number_format($liquidacion_actual['total_a_pagar'], 2, ',', '.'); ?></span>
                            </div>
                        </div>
                    <?php else: ?>
                        <p class="text-sm text-gray-500 text-center py-8">No se registran liquidaciones emitidas para esta cuenta.</p>
                    <?php endif; ?>
                </div>
            </div>
        </div>

        <div class="bg-white rounded-lg shadow p-6">
            <h3 class="text-lg font-bold text-[#004691] border-b pb-2 mb-4">Historial de Resúmenes Anteriores</h3>
            <div class="overflow-x-auto">
                <table class="w-full text-left border-collapse text-sm">
                    <thead>
                        <tr class="bg-gray-50 text-gray-500 uppercase text-xs border-b">
                            <th class="p-3">Período Histórico</th>
                            <th class="p-3">Vencimiento Pasado</th>
                            <th class="p-3">Total Liquidado</th>
                            <th class="p-3">Mínimo Exigido</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y text-gray-700">
                        <?php
                        $contador_historial = 0;
                        while ($historial = $stmtLiq->fetch(PDO::FETCH_ASSOC)) {
                            $contador_historial++;
                            echo "<tr class='hover:bg-gray-50 transition'>";
                            echo "<td class='p-3 font-semibold'>" . htmlspecialchars($historial['periodo']) . "</td>";
                            echo "<td class='p-3'>" . htmlspecialchars($historial['fecha_vencimiento']) . "</td>";
                            echo "<td class='p-3 font-bold'>$" . number_format($historial['total_a_pagar'], 2, ',', '.') . "</td>";
                            echo "<td class='p-3'>$" . number_format($historial['pago_minimo'], 2, ',', '.') . "</td>";
                            echo "</tr>";
                        }
                        ?>
                    </tbody>
                </table>
            </div>
            <?php if ($contador_historial === 0): ?>
                <p class="text-sm text-gray-500 text-center py-6">No existen períodos anteriores registrados en el archivo histórico.</p>
            <?php endif; ?>
        </div>
    </main>

    <footer class="bg-gray-50 text-[10px] text-gray-500 text-center p-4 border-t border-gray-200 mt-8">
        Portal Oficial de Consultas de Liquidaciones Progra3card.
    </footer>
</body>
</html>