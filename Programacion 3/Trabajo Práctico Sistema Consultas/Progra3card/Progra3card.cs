using System;
using MySql.Data.MySqlClient; 

using MySqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using MySqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using MySqlDataReader = MySql.Data.MySqlClient.MySqlDataReader;

namespace Progra3Card.Administrativo
{
    class Program
    {
        private static string connectionString = "Server=localhost;Port=3306;Database=mi_banco_db;Uid=root;Pwd=root;";
        
        static void Main(string[] args)
        {
            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("    SISTEMA ADMINISTRATIVO PROGRA3CARD   ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Emitir Nueva Tarjeta (Alta de Cliente)");
                Console.WriteLine("2. Listar Tarjetas");
                Console.WriteLine("3. Ver Detalle de una Tarjeta / Cliente");
                Console.WriteLine("4. Eliminar Tarjeta (Baja de Sistema)");
                Console.WriteLine("5. Emitir Nueva Liquidación Mensual");
                Console.WriteLine("6. Salir");
                Console.WriteLine("========================================");
                Console.Write("Seleccione una opción: ");

                switch (Console.ReadLine())
                {
                    case "1": MenuEmitirTarjeta(); break;
                    case "2": MenuListarTarjetas(); break;
                    case "3": MenuVerDetalleTarjeta(); break;
                    case "4": MenuEliminarTarjeta(); break;
                    case "5": MenuEmitirLiquidacion(); break;
                    case "6": salir = true; break;
                    default:
                        Console.WriteLine("Opción no válida. Presione una tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // Aca haciendo las 2 funciones a y b se piden en el tp

        // REQUERIMIENTO A: Cargar un nuevo cliente y emitir una tarjeta
        static void MenuEmitirTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- ALTA DE CLIENTE Y EMISIÓN DE TARJETA ---");
            
            // Recolección de Datos Personales del Cliente
            Console.Write("Ingrese el número de Documento (Sin puntos): ");
            string documento = Console.ReadLine()??"".Trim();

            // Selección del tipo de documento
            string tipoDoc = "DNI";
            Console.WriteLine("Seleccione Tipo de Documento:\n1. DNI\n2. PASAPORTE");
            if (Console.ReadLine() == "2") tipoDoc = "PASAPORTE";

            Console.Write("Nombre: ");
            string nombre = Console.ReadLine()??"".Trim();
            Console.Write("Apellido: ");
            string apellido = Console.ReadLine()??"".Trim();

            // VALIDACIÓN Y PARSEO DE FECHA DE NACIMIENTO (Esencial para evitar el Fatal Error)
            DateTime fechaNacParsed;
            while (true)
            {
                Console.Write("Fecha de Nacimiento (Formato AAAA-MM-DD, ej: 1995-08-25): ");
                string inputFecha = Console.ReadLine().Trim();
                if (DateTime.TryParse(inputFecha, out fechaNacParsed))
                {
                    break; // Fecha válida, salimos del bucle
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️ Formato de fecha inválido. Intente nuevamente.");
                Console.ResetColor();
            }

            Console.Write("Correo Electrónico: ");
            string email = Console.ReadLine()??"".Trim();

            // Recolección de Datos de la Tarjeta
            Console.Write("Ingrese el Número de Tarjeta (16 dígitos): ");
            string numeroTarjeta = Console.ReadLine()??"".Trim();

            // Selección estricta del Banco Emisor (ENUM)
            string bancoEmisor = "";
            bool bancoValido = false;
            while (!bancoValido)
            {
                Console.WriteLine("\nSeleccione el Banco Emisor de forma estricta:");
                Console.WriteLine("1. Banco Nación\n2. Banco Provincia\n3. Banco Galicia\n4. Banco Santander\n5. Banco BBVA\n6. Banco Macro");
                Console.Write("Opción numérica: ");
                switch (Console.ReadLine())
                {
                    case "1": bancoEmisor = "Banco Nación"; bancoValido = true; break;
                    case "2": bancoEmisor = "Banco Provincia"; bancoValido = true; break;
                    case "3": bancoEmisor = "Banco Galicia"; bancoValido = true; break;
                    case "4": bancoEmisor = "Banco Santander"; bancoValido = true; break;
                    case "5": bancoEmisor = "Banco BBVA"; bancoValido = true; break;
                    case "6": bancoEmisor = "Banco Macro"; bancoValido = true; break;
                    default: Console.WriteLine("⚠️ Opción inválida. Debe seleccionar una de las entidades permitidas."); break;
                }
            }

            // VALIDACIÓN Y PARSEO DEL SALDO
            decimal saldo = 0;
            while (true)
            {
                Console.Write("Ingrese el Saldo Inicial de la cuenta (ej: 1500,50): ");
                try
                {
                    saldo = Convert.ToDecimal(Console.ReadLine());
                    break;
                }
                catch
                {
                    Console.WriteLine("⚠️ Monto inválido. Use comas para los decimales si su sistema lo requiere.");
                }
            }

            // Impactar de forma segura en la base de datos
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // 1. Insertar el Usuario (Campos web 'usuario' y 'password' se envían explícitamente como NULL)
                    string sqlUsuario = "INSERT INTO usuarios (documento, tipo_doc, nombre, apellido, fecha_nacimiento, email, usuario, password) " +
                                        "VALUES (@doc, @tipo, @nom, @ape, @fNac, @email, NULL, NULL)";
                    
                    using (MySqlCommand cmdUser = new MySqlCommand(sqlUsuario, conn))
                    {
                        // Se define de forma estricta el tipo de columna (MySqlDbType) para evitar que el driver falle
                        cmdUser.Parameters.Add("@doc", MySql.Data.MySqlClient.MySqlDbType.VarChar, 20).Value = documento;
                        cmdUser.Parameters.Add("@tipo", MySql.Data.MySqlClient.MySqlDbType.VarChar, 20).Value = tipoDoc;
                        cmdUser.Parameters.Add("@nom", MySql.Data.MySqlClient.MySqlDbType.VarChar, 50).Value = nombre;
                        cmdUser.Parameters.Add("@ape", MySql.Data.MySqlClient.MySqlDbType.VarChar, 50).Value = apellido;
                        cmdUser.Parameters.Add("@fNac", MySql.Data.MySqlClient.MySqlDbType.Date).Value = fechaNacParsed; // Se pasa el objeto DateTime real
                        cmdUser.Parameters.Add("@email", MySql.Data.MySqlClient.MySqlDbType.VarChar, 100).Value = email;
                        
                        cmdUser.ExecuteNonQuery();
                    }

                    // 2. Insertar la Tarjeta asociada
                    string sqlTarjeta = "INSERT INTO tarjetas (numero_tarjeta, banco_emisor, estado, saldo, dni_titular) " +
                                        "VALUES (@numTar, @banco, 'Activa', @saldo, @doc)";
                    
                    using (MySqlCommand cmdTar = new MySqlCommand(sqlTarjeta, conn))
                    {
                        cmdTar.Parameters.Add("@numTar", MySql.Data.MySqlClient.MySqlDbType.VarChar, 16).Value = numeroTarjeta;
                        cmdTar.Parameters.Add("@banco", MySql.Data.MySqlClient.MySqlDbType.VarChar, 50).Value = bancoEmisor;
                        cmdTar.Parameters.Add("@saldo", MySql.Data.MySqlClient.MySqlDbType.Decimal).Value = saldo;
                        cmdTar.Parameters.Add("@doc", MySql.Data.MySqlClient.MySqlDbType.VarChar, 20).Value = documento;
                        
                        cmdTar.ExecuteNonQuery();
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n¡Éxito! Cliente registrado y plástico emitido correctamente sin errores de ejecución.");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nError al procesar el alta en la base de datos: " + ex.Message);
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        // Emitir liquidación mensual

        static void MenuEmitirLiquidacion()
        {
            Console.Clear();
            Console.WriteLine("--- EMITIR NUEVA LIQUIDACIÓN MENSUAL ---");
            
            Console.Write("Ingrese el Número de Cuenta del cliente: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());
            Console.Write("Ingrese Período comercial (Formato YYYY-MM): ");
            string periodo = Console.ReadLine().Trim();
            Console.Write("Fecha de Vencimiento (Formato YYYY-MM-DD): ");
            string fechaVenc = Console.ReadLine().Trim();
            Console.Write("Monto Total a Pagar: ");
            decimal totalPagar = Convert.ToDecimal(Console.ReadLine());
            Console.Write("Monto del Pago Mínimo Exigido: ");
            decimal pagoMinimo = Convert.ToDecimal(Console.ReadLine());

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sqlLiq = "INSERT INTO liquidaciones (num_cuenta, periodo, fecha_vencimiento, total_a_pagar, pago_minimo) " +
                                    "VALUES (@cuenta, @per, @venc, @total, @min)";

                    using (MySqlCommand liquidando = new MySqlCommand(sqlLiq, conn))
                    {
                        liquidando.Parameters.AddWithValue("@cuenta", numCuenta);
                        liquidando.Parameters.AddWithValue("@per", periodo);
                        liquidando.Parameters.AddWithValue("@venc", fechaVenc);
                        liquidando.Parameters.AddWithValue("@total", totalPagar);
                        liquidando.Parameters.AddWithValue("@min", pagoMinimo);
                        
                        liquidando.ExecuteNonQuery();
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n¡Resumen Impactado!");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nError al guardar la liquidación administrativa: " + ex.Message);
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        // Funciones a completar:
        //Cada una de las funciones esta abajo, completadas
        static void MenuListarTarjetas()
        {
            Console.Clear();
            Console.WriteLine("--- LISTADO GENERAL DE TARJETAS ---");
            Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}", "Nro Cuenta", "Nro Tarjeta", "Banco Emisor", "DNI Titular");
            Console.WriteLine("----------------------------------------------------------------------");

            // === A realizar ===
            // Aquí deben implementar un SELECT sobre la tabla 'tarjetas'
            // para recorrer las filas e imprimirlas en la consola.
            
            ObtenerYMostrarTarjetas();

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuVerDetalleTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- DETALLE DE TARJETA Y CLIENTE ---");
            Console.Write("Ingrese el Número de Cuenta a consultar: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            // === A realizar ===
            // Aquí deben realizar un SELECT con un JOIN entre 'tarjetas' y 'usuarios' 
            // filtrando por el numCuenta para traer todos los campos (Nombre, Apellido, Email, Saldo, etc.)
            
            MostrarDetalleCompleto(numCuenta);

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuEliminarTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- ELIMINAR TARJETA DEL SISTEMA ---");
            Console.Write("Ingrese el Número de Cuenta de la tarjeta a dar de baja: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n⚠️ ADVERTENCIA: Se eliminará la tarjeta, sus liquidaciones y los datos de acceso web vinculados.");
            Console.ResetColor();
            Console.Write("¿Está seguro de continuar? (S/N): ");
            
            if (Console.ReadLine().ToUpper() == "S")
            {
                // === A realizar ===
                // Aquí deben ejecutar un DELETE sobre la tabla 'tarjetas' donde num_cuenta = numCuenta.
                // Como definimos ON DELETE CASCADE en la base de datos, las liquidaciones se borrarán solas.
                // Opcional: Evaluar si también eliminan al usuario de la tabla 'usuarios' o si lo mantienen.
                
                bool exito = DarDeBajaTarjeta(numCuenta);

                if (exito)
                    Console.WriteLine("\nTarjeta eliminada correctamente del sistema.");
                else
                    Console.WriteLine("\nError al intentar eliminar la tarjeta. Verifique el número de cuenta.");
            }
            else
            {
                Console.WriteLine("\nOperación cancelada.");
            }

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }


        // =========================================================================
        // MÉTODOS BASE QUE DEBEN COMPLETAR CON LA LÓGICA 
        // =========================================================================

        static void ObtenerYMostrarTarjetas()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT num_cuenta, numero_tarjeta, banco_emisor, dni_titular FROM tarjetas";
                    
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}", 
                                reader["num_cuenta"], 
                                reader["numero_tarjeta"], 
                                reader["banco_emisor"], 
                                reader["dni_titular"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al leer el catálogo de tarjetas: " + ex.Message);
                }
            }
        }

        static void MostrarDetalleCompleto(int cuenta)
        {
            // Completar haciendo un SELECT con JOIN de usuarios y tarjetas WHERE num_cuenta = @cuenta
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Relación JOIN entre usuarios y tarjetas filtrado por la PK autoincremental de la cuenta
                    string query = "SELECT u.nombre, u.apellido, u.email, t.num_cuenta, t.numero_tarjeta, t.banco_emisor, t.saldo, t.estado " +
                                   "FROM usuarios u INNER JOIN tarjetas t ON u.documento = t.dni_titular " +
                                   "WHERE t.num_cuenta = @cuenta";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cuenta", cuenta);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Console.WriteLine("\n========================================");
                                Console.WriteLine($"Titular: {reader["apellido"]}, {reader["nombre"]}");
                                Console.WriteLine($"Email Declarado: {reader["email"]}");
                                Console.WriteLine("----------------------------------------");
                                Console.WriteLine($"Nro Cuenta: {reader["num_cuenta"]}");
                                Console.WriteLine($"Nro Tarjeta: {reader["numero_tarjeta"]}");
                                Console.WriteLine($"Banco Emisor: {reader["banco_emisor"]}");
                                Console.WriteLine($"Estado Operativo: {reader["estado"]}");
                                Console.WriteLine($"Saldo Consolidado: ${reader["saldo"]}");
                                Console.WriteLine("========================================");
                            }
                            else
                            {
                                Console.WriteLine("⚠️ No se encontró ninguna cuenta asociada al número ingresado.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al obtener detalles cruzados: " + ex.Message);
                }
            }
        }

        static bool DarDeBajaTarjeta(int cuenta)
        {
            // Completar usando un DELETE FROM tarjetas WHERE num_cuenta = @cuenta
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Al ejecutarse el DELETE, las liquidaciones asociadas se borran automáticamente en cascada (ON DELETE CASCADE)
                    string query = "DELETE FROM tarjetas WHERE num_cuenta = @cuenta";
                    
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cuenta", cuenta);
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        
                        // Retorna true si efectivamente se removió el registro de la tabla
                        return filasAfectadas > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error en la operación de eliminación: " + ex.Message);
                    return false;
                }
            }
        }
    }
}