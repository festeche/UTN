/*===============================================================================
PROGRAMACIÓN III Conexión Lineal a MySQL - Búsqueda de Alumno
 
 ⚠️ Antes de correr el proyecto, se debe instalar el driver de MySQL.
 En VSCode ejecutar este comando por terminal:
 dotnet add package MySql.Data --source https://api.nuget.org/v3/index.json
===============================================================================*/
using System;
// Importamos los componentes del driver de MySQL.
using MySqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using MySqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using MySqlDataReader = MySql.Data.MySqlClient.MySqlDataReader;

namespace BuscaBDAlumnos
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===================================================================");
            Console.WriteLine("                       BÚSQUEDA DE ALUMNO                          ");
            Console.WriteLine("===================================================================");
            
            // 1. Pedir al usuario que ingrese el legajo
            Console.Write("Ingrese el legajo del alumno a buscar: ");
            string legajoBuscado = Console.ReadLine()??"";

            if (string.IsNullOrWhiteSpace(legajoBuscado))
            {
                Console.WriteLine("El legajo ingresado no es válido. Saliendo del programa...");
                return;
            }

            // Cadena de conexión.
            string connectionString = "Server=127.0.0.1;Port=3306;Database=mibd;Uid=root;Pwd=root;";
            
            // 2. Abrimos la conexión asegurando el cierre de recursos con 'using'.
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open(); 

                    // 3. Sentencia SQL con parámetro (@legajo) para evitar Inyección SQL
                    string consulta = "SELECT legajo, nombre, apellido, email, carrera, turno FROM alumnos WHERE legajo = @legajo";

                    using (MySqlCommand comando = new MySqlCommand(consulta, conexion))
                    {
                        // Vinculamos el valor ingresado por el usuario al parámetro de la consulta
                        comando.Parameters.AddWithValue("@legajo", legajoBuscado);

                        using (MySqlDataReader lector = comando.ExecuteReader())
                        {
                            // 4. Usamos un 'if' en lugar de un 'while' ya que esperamos un único resultado
                            if (lector.Read())
                            {
                                string legajo = lector["legajo"].ToString() ?? "";
                                string nombre = lector["nombre"].ToString() ?? "";
                                string apellido = lector["apellido"].ToString() ?? "";
                                string email = lector["email"].ToString() ?? "";
                                string carrera = lector["carrera"].ToString() ?? "";
                                string turno = lector["turno"].ToString() ?? "";

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("\n¡Alumno encontrado exitosamente!");
                                Console.ResetColor();
                                Console.WriteLine("-------------------------------------------------------------------");
                                Console.WriteLine($"Legajo:   {legajo}");
                                Console.WriteLine($"Nombre:   {nombre}");
                                Console.WriteLine($"Apellido: {apellido}");
                                Console.WriteLine($"Email:    {email}");
                                Console.WriteLine($"Carrera:  {carrera}");
                                Console.WriteLine($"Turno:    {turno}");
                                Console.WriteLine("-------------------------------------------------------------------\n");
                            }
                            else
                            {
                                // 5. Mensaje de error / no encontrado
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"\nNo se encontró ningún alumno registrado con el legajo: {legajoBuscado}\n");
                                Console.ResetColor();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Control de excepciones
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nOcurrió un error al intentar operar con la base de datos:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }

            Console.WriteLine("Presione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}