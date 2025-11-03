using System.Data.SQLite;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Habit_Tracker
{
    internal class Program
    {
        static string connectionString = @"Data Source=habit-Tracker.db";

        static void Main(string[] args)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    @"CREATE TABLE IF NOT EXISTS habits (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        HabitName TEXT NOT NULL,
                        Unit TEXT NOT NULL)";
                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
            CreateMockDatabase();

            GetUserInput();
        }

        static void GetUserInput()
        {
            Console.Clear();

            bool closeApp = false;
            while (closeApp == false)
            {
                Console.WriteLine("\n\nMAIN MENU");
                Console.WriteLine("\nWhat would you like to do?");
                Console.WriteLine("\nType 0 to Close Application");
                Console.WriteLine("Type 1 to View All Records.");
                Console.WriteLine("Type 2 to Insert Record.");
                Console.WriteLine("Type 3 to Update Record.");
                Console.WriteLine("Type 4 to Delete Record.");
                Console.WriteLine("Type 5 to View All Habit Types");
                Console.WriteLine("Type 6 to Add New Habit Type");
                Console.WriteLine("-----------------------------\n");

                string command = Console.ReadLine();


                switch (command)
                {
                    case "0":
                        Console.WriteLine("\nGoodbye!");
                        closeApp = true;
                        Environment.Exit(0);
                        break;
                    case "1":
                        GetAllRecords();
                        break;
                    case "2":
                        Insert();
                        break;
                    case "3":
                        Update();
                        break;
                    case "4":
                        Delete();
                        break;
                    case "5":
                        PrintHabitTypes();
                        break;
                    case "6":
                        AddNewHabitType();
                        break;
                    default:
                        Console.WriteLine("\nInvalid Command. Please type a number from 0 to 6.\n");
                        break;
                }

            }

        }
        private static void GetAllRecords()
        {
            List<Habit> habits = GetAllHabitTypes();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                foreach(var habit in habits)
                {
                    PrintRecords(habit.HabitName, habit.Unit, connection);
                }
                connection.Close();
            }
        }

        private static void Insert()
        {
            PrintHabitTypes();
            List<Habit> tableData = GetAllHabitTypes();

            if (tableData.Count == 0)
            {
                Console.WriteLine("\nThe Habit types data table is empty. You can add new habit types from the Main Menu.");
                Console.WriteLine("Enter 0 to retun...");
                Console.ReadKey();
                return;
            }
            else
            {
                var possibleIds = tableData.Select(x => x.Id);
                int chosenId = 0;
                Console.WriteLine("Choose habit type's Id u want to moderate: ");
                while (true)
                {
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out chosenId) && possibleIds.Contains(chosenId))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a proper id number.: ");
                    }
                }
                

                string date = GetDateInput();

                int quantity = GetNumberInput("\n\nPlease insert number of units (no decimals allowed). Enter 0 if you want to go back to Main Menu");

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string tableName = tableData.FirstOrDefault(x => x.Id == chosenId).HabitName;
                    string sqlCommand = $"INSERT INTO {tableName}(date, quantity) VALUES(@Date, @Quantity)";
                    var tableCmd = connection.CreateCommand();
                    tableCmd.CommandText = sqlCommand;
                    tableCmd.Parameters.AddWithValue("@Date", date);
                    tableCmd.Parameters.AddWithValue("@Quantity", quantity);
                    tableCmd.ExecuteNonQuery();

                    connection.Close();
                }
            }
        }

        private static void Delete()
        {

            PrintHabitTypes();
            List<Habit> tableData = GetAllHabitTypes();

            if (tableData.Count == 0)
            {
                Console.WriteLine("\nThe Habit types data table is empty. You can add new habit types from the Main Menu.");
                Console.WriteLine("Enter 0 to retun...");
                Console.ReadKey();
                return;
            }
            else
            {
                var possibleIds = tableData.Select(x => x.Id);
                int chosenId = 0;
                Console.WriteLine("Choose habit type's Id u want to moderate: ");
                while (true)
                {
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out chosenId) && possibleIds.Contains(chosenId))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a proper id number.: ");
                    }
                }

                Console.Clear();
                Habit chosenHabit = tableData.FirstOrDefault(x => x.Id == chosenId);
                string tableName = chosenHabit.HabitName;
                string unit = chosenHabit.Unit;
                PrintRecords(tableName, unit);

                var recordId = GetNumberInput("\n\nPlease type the Id of the record you want to delete or type 0 to go back to Main Menu");

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string sqlCommand = $"DELETE FROM {tableName} WHERE Id = @Id";
                    var tableCmd = connection.CreateCommand();
                    tableCmd.CommandText = sqlCommand;
                    tableCmd.Parameters.AddWithValue("@Id", recordId);
                    int rowCount = tableCmd.ExecuteNonQuery();

                    if (rowCount == 0)
                    {
                        Console.WriteLine($"\n\nRecord with Id {recordId} does'n exist. \n\n");
                        Delete();
                    }
                    connection.Close();
                }

                Console.WriteLine($"\n\nRecord with Id {recordId} was deleted. \n");
                Console.WriteLine("Press Enter to back to Main Menu...");
                Console.ReadKey();
            }

        }

        private static void Update()
        {
            PrintHabitTypes();
            List<Habit> tableData = GetAllHabitTypes();

            if (tableData.Count == 0)
            {
                Console.WriteLine("\nThe Habit types data table is empty. You can add new habit types from the Main Menu.");
                Console.WriteLine("Enter 0 to retun...");
                Console.ReadKey();
                return;
            }
            else
            {
                var possibleIds = tableData.Select(x => x.Id);
                int chosenId = 0;
                Console.WriteLine("Choose habit type's Id u want to moderate: ");
                while (true)
                {
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out chosenId) && possibleIds.Contains(chosenId))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a proper id number: ");
                    }
                }

                Console.Clear();
                Habit chosenHabit = tableData.FirstOrDefault(x => x.Id == chosenId);
                string tableName = chosenHabit.HabitName;
                string unit = chosenHabit.Unit;
                PrintRecords(tableName, unit);

                var recordId = GetNumberInput("\n\nPlease type Id of the record you would like to update. Type 0 to return to Main Menu.\n\n");

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    var selectCmd = connection.CreateCommand();
                    string sqlSelectCommand = $"SELECT EXISTS(SELECT 1 FROM {tableName} WHERE Id = @Id)";
                    selectCmd.CommandText = sqlSelectCommand;
                    selectCmd.Parameters.AddWithValue("@Id", recordId);
                    int checkQuery = Convert.ToInt32(selectCmd.ExecuteScalar());

                    if (checkQuery == 0)
                    {
                        Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist. \n\n");
                        connection.Close();
                        Update();
                    }

                    string date = GetDateInput();

                    int quantity = GetNumberInput("\n\nPlease insert the number units (no decimals allowed)\n\n");

                    string sqlUpdateCommand = $"UPDATE {tableName} SET date = @Date, quantity = @Quantity WHERE Id = @Id";

                    var updateCmd = connection.CreateCommand();
                    updateCmd.CommandText = sqlUpdateCommand;
                    updateCmd.Parameters.AddWithValue("@Date", date);
                    updateCmd.Parameters.AddWithValue("@Quantity", quantity);
                    updateCmd.Parameters.AddWithValue(@"Id", recordId);
                    updateCmd.ExecuteNonQuery();

                    connection.Close();
                }
            }
        }

        internal static string GetDateInput()
        {
            Console.WriteLine("\n\nPlease insert the date: (Format dd-mm-yy). Type 'today' for today's date \nType 0 to return to main menu");

            string dateInput = Console.ReadLine();

            if (dateInput == "0")
                GetUserInput();

            if (dateInput == "today")
            {
                dateInput = DateTime.Now.ToString("dd-MM-yy");
            }

            while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
            {
                Console.WriteLine("\n\nInvalid date. (Format: dd-MM-yy). Type 0 to return to Main Menu or try again:\n\n");
                dateInput = Console.ReadLine();
            }

            return dateInput;
        }

        internal static int GetNumberInput(string message)
        {
            Console.WriteLine(message);

            string numberInput = Console.ReadLine();

            if (numberInput == "0")
                GetUserInput();

            int finalInput = 0;
            if (int.TryParse(numberInput, out int result))
            {
                finalInput = result;
            }
            else
            {
                GetNumberInput("\nInvalid input. Please insert a number greater than 0.\n");
            }

            return finalInput;
        }

        private static List<Habit> GetAllHabitTypes()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                List<Habit> tableData = new();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"SELECT * FROM habits";
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableData.Add(
                            new Habit
                            {
                                Id = reader.GetInt32(0),
                                HabitName = reader.GetString(1),
                                Unit = reader.GetString(2)
                            });
                    }
                }
                return tableData;
            }
        }

        private static void PrintHabitTypes()
        {
            List<Habit> tableData = GetAllHabitTypes();
            if (tableData.Count > 0)
            {
                Console.Clear();
                Console.WriteLine("------------------------------\n");
                foreach (var field in tableData)
                {
                    Console.WriteLine($"{field.Id} - Habit: {field.HabitName} - Unit: {field.Unit}");
                }
                Console.WriteLine("------------------------------\n");
            }
            else
            {
                Console.WriteLine("\nThe Habit types data table is empty. You can add new habit types from the Main Menu.");
                Console.WriteLine("Enter 0 to retun...");
                Console.ReadKey();
            }
        }

        private static void PrintRecords(string tableName, string unit, SQLiteConnection conn = null)
        {
            bool createdLocally = false;
            if (conn == null)
            {
                conn = new SQLiteConnection(connectionString);
                conn.Open();
                createdLocally = true;
            }
            var cmd = conn.CreateCommand();

            string sqlCommand = $"SELECT * FROM {tableName}";
            cmd.CommandText = sqlCommand;
            List<DataModel> tableData = new();

            SQLiteDataReader reader = cmd.ExecuteReader();

            Console.WriteLine($"\n{tableName}");

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    tableData.Add(
                        new DataModel
                        {
                            Id = reader.GetInt32(0),
                            Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                            Quantity = reader.GetInt32(2)
                        });
                }
            }
            else
            {
                Console.WriteLine("No rows found");
            }

            Console.WriteLine("------------------------------");
            foreach (var field in tableData)
            {
                Console.WriteLine($"{field.Id} - {field.Date.ToString("dd-MM-yy")} - {field.Quantity} {unit}");
            }
            Console.WriteLine("------------------------------");
            if(createdLocally)
            {
                conn.Dispose();
            }
        }
        private static void AddNewHabitType()
        {
            string tableName;
            Console.WriteLine("Please insert a new habit type: ");
            tableName = Console.ReadLine();
            var habits = GetAllHabitTypes();
            while (!System.Text.RegularExpressions.Regex.IsMatch(tableName, @"^[A-Za-z_][A-Za-z0-9_]*$") //letters, numbers, underscores not starting with a number validation
                || habits.Any(habit => habit.HabitName.Equals(tableName, StringComparison.OrdinalIgnoreCase))) 
                
            {
                if (habits.Any(habit=>habit.HabitName.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine("This type of habit already exists.");
                }
                Console.WriteLine("Insert type's name again: (name must start with a letter and can not contain any spaces)");
                tableName = Console.ReadLine();
            };
            tableName = tableName.ToLower();
            string sqlCommand = $"CREATE TABLE IF NOT EXISTS {tableName}(" +
                $"Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                $"Date TEXT," +
                $"Quantity INTEGER)";

            Console.WriteLine("Please insert a unit of measurement:");
            string unit = Console.ReadLine();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = sqlCommand;
                createTableCmd.ExecuteNonQuery();

                var addTableCmd = connection.CreateCommand();
                addTableCmd.CommandText =
                    @"INSERT INTO habits(HabitName, Unit) VALUES(@HabitName, @Unit)";
                addTableCmd.Parameters.AddWithValue("@HabitName", tableName);
                addTableCmd.Parameters.AddWithValue("@Unit", unit);
                addTableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        private static void CreateMockDatabase()
        {
            var rng = new Random();

            var list = new List<Habit>() {
                new Habit() { HabitName = "jogging", Unit = "km" },
                new Habit() { HabitName = "reading_books", Unit = "pages"},
                new Habit() { HabitName = "drinking_water", Unit = "glasses"}
            };

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                var createHabitsCmd = connection.CreateCommand();
                createHabitsCmd.CommandText =
                    @"CREATE TABLE IF NOT EXISTS habits (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    HabitName TEXT NOT NULL,
                    Unit TEXT NOT NULL)";
                createHabitsCmd.ExecuteNonQuery();

                foreach (var x in list)
                {
                    string sqlCreateTableCommand = $"CREATE TABLE IF NOT EXISTS {x.HabitName}(" +
                                        $"Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                        $"Date TEXT," +
                                        $"Quantity INTEGER)";
                    var createTableCmd = connection.CreateCommand();
                    createTableCmd.CommandText = sqlCreateTableCommand;
                    createTableCmd.ExecuteNonQuery();

                    string sqlInsertTableCommand = $"INSERT INTO habits(HabitName, Unit) VALUES(@HabitName, @Unit)";
                    var insertTableCmd = connection.CreateCommand();
                    insertTableCmd.CommandText = sqlInsertTableCommand;
                    insertTableCmd.Parameters.AddWithValue("@HabitName", x.HabitName);
                    insertTableCmd.Parameters.AddWithValue("@Unit", x.Unit);
                    insertTableCmd.ExecuteNonQuery();

                    for (int i = 0; i < 100; i++)
                    {
                        string sqlInsertCommand = $"INSERT INTO {x.HabitName}(date, quantity) VALUES(@Date, @Quantity)";
                        var insertCmd = connection.CreateCommand();
                        insertCmd.CommandText = sqlInsertCommand;
                        insertCmd.Parameters.AddWithValue("@Date", DateHelper.RandomDate().ToString("dd-MM-yy"));
                        insertCmd.Parameters.AddWithValue("@Quantity", rng.Next(10, 50)); 
                        insertCmd.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }
    }

    public class DataModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }

    public class Habit
    {
        public int Id { get; set; }
        public string HabitName { get; set; }
        public string Unit { get; set; }
    }

    public static class DateHelper
    {
        private static readonly Random _rng = new Random();

        public static DateTime RandomDate()
        {
            DateTime startDate = new DateTime(2000, 1, 1);
            DateTime endDate = DateTime.Now;
            int range = (endDate.Date - startDate.Date).Days + 1;

            return startDate.Date.AddDays(_rng.Next(range));
        }
    }
}
