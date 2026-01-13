using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace opam_lab5
{
    public static class IdGenerator
    {
        public static int GetNewId(string filePath)
        {
            if (!File.Exists(filePath)) return 1;

            int maxId = 0;
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');

                if (parts.Length > 0 && int.TryParse(parts[0], out int currentId))
                {
                    if (currentId > maxId) maxId = currentId;
                }
            }
            return maxId + 1;
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public int Quantity { get; set; }
    }

    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    public class UserService
    {
        private string filePath = "users.csv";

        public UserService()
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "Email,Password\nadmin,12345\n", Encoding.UTF8);
            }
        }

        public bool Login(string login, string password)
        {
            if (!File.Exists(filePath)) return false;
            var lines = File.ReadAllLines(filePath, Encoding.UTF8).Skip(1);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2 && parts[0] == login && parts[1] == password) return true;
            }
            return false;
        }

        public bool Register(string login, string password)
        {
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length > 0 && parts[0] == login) return false;
                }
            }
            File.AppendAllText(filePath, $"{login},{password}\n", Encoding.UTF8);
            return true;
        }

        public bool DeleteUser(string login, string password)
        {
            if (!File.Exists(filePath)) return false;
            var lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
            var newLines = new List<string>();
            bool found = false;

            if (lines.Count > 0) newLines.Add(lines[0]);

            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length >= 2 && parts[0] == login && parts[1] == password)
                {
                    found = true;
                    continue;
                }
                newLines.Add(lines[i]);
            }

            if (found) File.WriteAllLines(filePath, newLines, Encoding.UTF8);
            return found;
        }
    }

    public class ProductService
    {
        private string filePath = "products.csv";

        public ProductService()
        {
            if (!File.Exists(filePath))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Id,Name,Price,Quantity");
                sb.AppendLine("1,Нурофен,120,50");
                sb.AppendLine("2,Йод,40,100");
                sb.AppendLine("3,Едем,150,30");
                sb.AppendLine("4,Аспірин,60,80");
                sb.AppendLine("5,Вітамін С,200,40");
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
        }

        public List<Product> GetAll()
        {
            var list = new List<Product>();
            if (!File.Exists(filePath)) return list;

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length < 4) continue;

                if (int.TryParse(parts[0], out int id) &&
                    double.TryParse(parts[2], out double price) &&
                    int.TryParse(parts[3], out int qty))
                {
                    list.Add(new Product { Id = id, Name = parts[1], Price = price, Quantity = qty });
                }
            }
            return list;
        }

        public void Add(string name, double price, int quantity)
        {
            int newId = IdGenerator.GetNewId(filePath);
            string line = $"{newId},{name},{price},{quantity}";
            File.AppendAllText(filePath, line + Environment.NewLine, Encoding.UTF8);
        }

        public void Delete(int id)
        {
            var products = GetAll();
            var itemToRemove = products.FirstOrDefault(p => p.Id == id);
            if (itemToRemove != null)
            {
                products.Remove(itemToRemove);
                var lines = new List<string> { "Id,Name,Price,Quantity" };
                foreach (var p in products) lines.Add($"{p.Id},{p.Name},{p.Price},{p.Quantity}");
                File.WriteAllLines(filePath, lines, Encoding.UTF8);
            }
        }
    }

    public class ClientService
    {
        private string filePath = "clients.csv";

        public ClientService()
        {
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, "Id,Name,Phone\n", Encoding.UTF8);
        }

        public List<Client> GetAll()
        {
            var list = new List<Client>();
            if (!File.Exists(filePath)) return list;

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length >= 3 && int.TryParse(parts[0], out int id))
                {
                    list.Add(new Client { Id = id, Name = parts[1], Phone = parts[2] });
                }
            }
            return list;
        }

        public void Add(string name, string phone)
        {
            int newId = IdGenerator.GetNewId(filePath);
            File.AppendAllText(filePath, $"{newId},{name},{phone}\n", Encoding.UTF8);
        }

        public void Delete(int id)
        {
            var clients = GetAll();
            var itemToRemove = clients.FirstOrDefault(c => c.Id == id);
            if (itemToRemove != null)
            {
                clients.Remove(itemToRemove);
                var lines = new List<string> { "Id,Name,Phone" };
                foreach (var c in clients) lines.Add($"{c.Id},{c.Name},{c.Phone}");
                File.WriteAllLines(filePath, lines, Encoding.UTF8);
            }
        }
    }

    class Program
    {
        static ProductService _productService = new ProductService();
        static ClientService _clientService = new ClientService();
        static UserService _userService = new UserService();

        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            bool isAuthenticated = false;

            while (!isAuthenticated)
            {
                Console.Clear();
                Console.WriteLine("=== ВІТАЄМО В СИСТЕМІ ===");
                Console.WriteLine("1. Вхід");
                Console.WriteLine("2. Реєстрація");
                Console.WriteLine("3. Видалити акаунт");
                Console.WriteLine("4. Вихід");

                int choice = (int)GetUserInput("Ваш вибір:");

                if (choice == 1)
                {
                    Console.Write("Логін: ");
                    string l = Console.ReadLine() ?? "";
                    Console.Write("Пароль: ");
                    string p = Console.ReadLine() ?? "";

                    if (_userService.Login(l, p)) isAuthenticated = true;
                    else
                    {
                        Console.WriteLine("Помилка: Невірний логін або пароль.");
                        Console.ReadKey();
                    }
                }
                else if (choice == 2)
                {
                    Console.Write("Новий логін: ");
                    string newLogin = Console.ReadLine() ?? "";
                    Console.Write("Новий пароль: ");
                    string newPass = Console.ReadLine() ?? "";

                    if (_userService.Register(newLogin, newPass)) Console.WriteLine("Реєстрація успішна! Тепер увійдіть.");
                    else Console.WriteLine("Такий користувач вже існує!");
                    Console.ReadKey();
                }
                else if (choice == 3)
                {
                    Console.Write("Логін для видалення: ");
                    string delLogin = Console.ReadLine() ?? "";
                    Console.Write("Пароль: ");
                    string delPass = Console.ReadLine() ?? "";

                    if (_userService.DeleteUser(delLogin, delPass)) Console.WriteLine("Акаунт видалено.");
                    else Console.WriteLine("Помилка видалення.");
                    Console.ReadKey();
                }
                else if (choice == 4) ExitProgram();
            }

            RenderIntro();
            ShowMainMenu();
        }

        public static void RenderIntro()
        {
            Console.Clear();
            Console.WriteLine("===========================================");
            Console.WriteLine("==== Ласкаво просимо до Аптеки Здоров'я ====");
            Console.WriteLine("===========================================");
            Console.WriteLine("Натисніть будь-яку клавішу...");
            Console.ReadKey();
        }

        public static double GetUserInput(string prompt = "Введіть число:")
        {
            Console.Write(prompt + " ");
            bool isNumber = Double.TryParse(Console.ReadLine(), out double choice);
            if (!isNumber)
            {
                Console.WriteLine("Ви ввели не число! Спробуйте ще раз.");
                return GetUserInput(prompt);
            }
            return choice;
        }

        public static void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\nГоловне меню:");
                Console.WriteLine("1. Товари");
                Console.WriteLine("2. Клієнти");
                Console.WriteLine("3. Замовлення");
                Console.WriteLine("4. Пошук");
                Console.WriteLine("5. Статистика");
                Console.WriteLine("6. Вихід");

                int choice = (int)GetUserInput("Виберіть пункт меню:");

                switch (choice)
                {
                    case 1: ShowProductMenu(); break;
                    case 2: ShowClientsMenu(); break;
                    case 3: ShowOrderMenu(); break;
                    case 4: SearchProductByNameStart(); break;
                    case 5: ShowStatistics(); break;
                    case 6: ExitProgram(); break;
                    default: Console.WriteLine("Неправильний вибір."); Console.ReadKey(); break;
                }
            }
        }

        private static void ShowProductMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ТОВАРИ ===");
                Console.WriteLine("1. Перегляд товарів");
                Console.WriteLine("2. Додати товар");
                Console.WriteLine("3. Видалити товар");
                Console.WriteLine("4. Сортування товарів");
                Console.WriteLine("5. Пошук товару");
                Console.WriteLine("6. Назад");

                int choice = (int)GetUserInput("Виберіть дію:");

                switch (choice)
                {
                    case 1: DisplayProducts(); break;
                    case 2: AddProduct(); break;
                    case 3: DeleteProduct(); break;
                    case 4: ShowSortMenu(); break;
                    case 5: SearchProductByNameStart(); break;
                    case 6: return;
                    default: Console.WriteLine("Невірний вибір!"); Console.ReadKey(); break;
                }
            }
        }

        private static void DisplayProducts()
        {
            Console.Clear();
            Console.WriteLine("=== СПИСОК ТОВАРІВ ===");
            Console.WriteLine("| {0,-5} | {1,-20} | {2,10} | {3,10} |", "ID", "Назва", "Ціна", "К-сть");
            Console.WriteLine(new string('-', 56));

            var products = _productService.GetAll();
            foreach (var p in products)
            {
                Console.WriteLine("| {0,-5} | {1,-20} | {2,10:F2} | {3,10} |", p.Id, p.Name, p.Price, p.Quantity);
            }
            Console.WriteLine(new string('-', 56));
            Console.ReadKey();
        }

        private static void AddProduct()
        {
            Console.Write("Назва: ");
            string name = Console.ReadLine() ?? "";
            double price = GetUserInput("Ціна:");
            int quantity = (int)GetUserInput("Кількість:");

            _productService.Add(name, price, quantity);
            Console.WriteLine("Товар збережено у файл!");
            Console.ReadKey();
        }

        private static void DeleteProduct()
        {
            Console.Clear();
            Console.WriteLine("--- Видалення товару ---");
            var products = _productService.GetAll();
            foreach (var p in products) Console.WriteLine($"{p.Id}. {p.Name}");

            int idToDelete = (int)GetUserInput("Введіть ID товару для видалення:");
            _productService.Delete(idToDelete);
            Console.WriteLine("Операцію завершено.");
            Console.ReadKey();
        }

        private static void ShowSortMenu()
        {
            Console.Clear();
            Console.WriteLine("\n--- Сортування ---");
            Console.WriteLine("1. Стандартне (List.Sort)");
            Console.WriteLine("2. Власне (Бульбашкове)");

            int choice = (int)GetUserInput("Виберіть метод:");
            var products = _productService.GetAll();

            if (choice == 1) products.Sort((a, b) => a.Price.CompareTo(b.Price));
            else if (choice == 2)
            {
                for (int i = 0; i < products.Count - 1; i++)
                {
                    for (int j = 0; j < products.Count - i - 1; j++)
                    {
                        if (products[j].Price > products[j + 1].Price)
                        {
                            var temp = products[j];
                            products[j] = products[j + 1];
                            products[j + 1] = temp;
                        }
                    }
                }
            }

            Console.WriteLine("\n=== СПИСОК ТОВАРІВ (Відсортовано) ===");
            foreach (var p in products) Console.WriteLine("| {0,-5} | {1,-20} | {2,10:F2} | {3,10} |", p.Id, p.Name, p.Price, p.Quantity);
            Console.ReadKey();
        }

        private static void ShowClientsMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== КЛІЄНТИ ===");
                Console.WriteLine("1. Перегляд");
                Console.WriteLine("2. Додати");
                Console.WriteLine("3. Видалити");
                Console.WriteLine("4. Назад");

                int choice = (int)GetUserInput("Виберіть дію:");
                switch (choice)
                {
                    case 1: DisplayClients(); break;
                    case 2: AddClient(); break;
                    case 3: DeleteClient(); break;
                    case 4: return;
                    default: Console.WriteLine("Невірний вибір!"); Console.ReadKey(); break;
                }
            }
        }

        private static void DisplayClients()
        {
            Console.WriteLine("\nСписок клієнтів:");
            var clients = _clientService.GetAll();
            foreach (var c in clients) Console.WriteLine($"{c.Id}. {c.Name} - {c.Phone}");
            Console.ReadKey();
        }

        private static void AddClient()
        {
            Console.Write("Ім'я: ");
            string name = Console.ReadLine() ?? "";
            Console.Write("Телефон: ");
            string phone = Console.ReadLine() ?? "";

            _clientService.Add(name, phone);
            Console.WriteLine("Клієнта додано!");
            Console.ReadKey();
        }

        private static void DeleteClient()
        {
            Console.Clear();
            Console.WriteLine("--- Видалення клієнта ---");
            DisplayClients();
            int id = (int)GetUserInput("Введіть ID:");
            _clientService.Delete(id);
            Console.WriteLine("Виконано.");
            Console.ReadKey();
        }

        private static void ShowOrderMenu()
        {
            Console.Clear();
            Console.WriteLine("=== ЗАМОВЛЕННЯ ===");
            Console.WriteLine("Доступні товари:");
            foreach (var p in _productService.GetAll()) Console.WriteLine($"{p.Name} - {p.Price} грн");

            Console.WriteLine("\n*Розрахунок (демо)*");

            double priceNurofen = 120, priceIodine = 40, priceEdem = 150, priceAspirin = 60, priceVitC = 200;

            double Nurofen = GetUserInput("Кількість Нурофену:");
            double Iodine = GetUserInput("Кількість Йоду:");
            double Edem = GetUserInput("Кількість Едему:");
            double Aspirin = GetUserInput("Кількість Аспірину:");
            double VitaminC = GetUserInput("Кількість Вітаміну С:");

            double totalPrice = Nurofen * priceNurofen +
                                Iodine * priceIodine +
                                Edem * priceEdem +
                                Aspirin * priceAspirin +
                                VitaminC * priceVitC;

            int discount = new Random().Next(10, 101);

            Console.WriteLine($"\nДо сплати: {totalPrice - (totalPrice * discount / 100)} грн (Знижка {discount}%)");
            Console.ReadKey();
        }

        private static void SearchProductByNameStart()
        {
            Console.Clear();
            Console.WriteLine("=== ПОШУК ===");
            Console.Write("Введіть перші літери назви: ");
            string search = Console.ReadLine()?.ToLower() ?? "";

            var products = _productService.GetAll();
            bool found = false;
            Console.WriteLine("\nРезультати пошуку:");
            foreach (var p in products)
            {
                if (p.Name.ToLower().StartsWith(search))
                {
                    Console.WriteLine($"ID: {p.Id} | {p.Name} - {p.Price} грн");
                    found = true;
                }
            }
            if (!found) Console.WriteLine("Не знайдено");
            Console.ReadKey();
        }

        private static void ShowStatistics()
        {
            Console.Clear();
            Console.WriteLine("=== СТАТИСТИКА ===");
            var products = _productService.GetAll();
            if (products.Count == 0) { Console.WriteLine("Немає товарів"); Console.ReadKey(); return; }

            double totalValue = 0, maxPrice = 0, minPrice = double.MaxValue;
            int totalQuantity = 0, expensiveCount = 0;

            foreach (var p in products)
            {
                totalValue += p.Price * p.Quantity;
                totalQuantity += p.Quantity;
                if (p.Price > maxPrice) maxPrice = p.Price;
                if (p.Price < minPrice) minPrice = p.Price;
                if (p.Price > 100) expensiveCount++;
            }

            Console.WriteLine($"Загальна вартість: {totalValue:F2} грн");
            Console.WriteLine($"Середня ціна:        {(totalQuantity > 0 ? totalValue / totalQuantity : 0):F2} грн");
            Console.WriteLine($"Макс. ціна:          {maxPrice} грн");
            Console.WriteLine($"Мін. ціна:           {minPrice} грн");
            Console.WriteLine($"Товарів > 100 грн: {expensiveCount}");
            Console.ReadKey();
        }

        private static void ExitProgram()
        {
            Environment.Exit(0);
        }
    }
}