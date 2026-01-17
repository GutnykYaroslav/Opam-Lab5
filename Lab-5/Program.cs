using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Це наша програма
namespace opam_lab5
{
    // --- ДОПОМІЖНИЙ КЛАС ---
    // Цей клас потрібен, щоб придумувати нові ID (номери) для товарів чи юзерів.
    // Щоб не було двох товарів з номером 1.
    public static class IdGenerator
    {
        // Метод, який шукає останній номер у файлі і повертає наступний
        public static int GetNewId(string filePath)
        {
            // Перевірка: якщо файлу взагалі нема, то це буде номер 1
            if (!File.Exists(filePath)) return 1;

            int maxId = 0; // Тут будемо зберігати найбільший знайдений номер

            // Читаємо всі рядки з текстового файлу
            var lines = File.ReadAllLines(filePath);

            // Проходимо по кожному рядку, але ПРОПУСКАЄМО перший (Skip(1)), бо там заголовок (Id,Name...)
            foreach (var line in lines.Skip(1))
            {
                // Розбиваємо рядок по комі, бо це CSV формат
                var parts = line.Split(',');

                // Пробуємо перетворити першу частину (ID) в число
                if (parts.Length > 0 && int.TryParse(parts[0], out int currentId))
                {
                    // Якщо цей номер більший за той, що ми бачили раніше - запам'ятовуємо його
                    if (currentId > maxId) maxId = currentId;
                }
            }
            // Повертаємо наступний номер (найбільший + 1)
            return maxId + 1;
        }
    }

    // --- КЛАС ТОВАРУ ---
    // Це просто шаблон, який описує, що таке "Товар"
    public class Product
    {
        public int Id { get; set; }            // Номер
        public string Name { get; set; } = ""; // Назва
        public double Price { get; set; }      // Ціна
        public int Quantity { get; set; }      // Скільки штук на складі
    }

    // --- КЛАС КЛІЄНТА ---
    // Шаблон для клієнтів
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public long Phone { get; set; } // Телефон (long, бо номер довгий)
    }

    // --- СЕРВІС ЮЗЕРІВ ---
    // Тут вся логіка для входу, реєстрації і видалення адмінів/користувачів
    public class UserService
    {
        private string filePath = "users.csv"; // Назва файлу, де лежать паролі

        // Конструктор: запускається один раз, коли створюємо цей сервіс
        public UserService()
        {
            // Якщо файлу ще нема - створюємо його і записуємо туди адміна
            if (!File.Exists(filePath))
            {
                // Записуємо заголовок і першого юзера (admin/12345)
                File.WriteAllText(filePath, "Email,Password\nadmin,12345\n", Encoding.UTF8);
            }
        }

        // Метод для логіну (перевірка пароля)
        public bool Login(string login, string password)
        {
            // Якщо файлу нема, то і зайти не можна
            if (!File.Exists(filePath)) return false;

            // Читаємо файл і зразу пропускаємо перший рядок (заголовок)
            var lines = File.ReadAllLines(filePath, Encoding.UTF8).Skip(1);

            // Перевіряємо кожен рядок
            foreach (var line in lines)
            {
                var parts = line.Split(','); // Розбиваємо рядок на логін і пароль
                // Якщо логін і пароль співпали з тим, що ввів юзер - пускаємо (true)
                if (parts.Length >= 2 && parts[0] == login && parts[1] == password) return true;
            }
            // Якщо нічого не знайшли - не пускаємо (false)
            return false;
        }

        // Метод реєстрації
        public bool Register(string login, string password)
        {
            // Спочатку перевіримо, чи такий логін вже зайнятий
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    // Якщо знайшли такий логін - відмовляємо
                    if (parts.Length > 0 && parts[0] == login) return false;
                }
            }
            // Якщо все ок - дописуємо в кінець файлу нового юзера
            File.AppendAllText(filePath, $"{login},{password}\n", Encoding.UTF8);
            return true; // Успіх
        }

        // Метод видалення юзера
        public bool DeleteUser(string login, string password)
        {
            if (!File.Exists(filePath)) return false;

            // Читаємо всі рядки у список
            var lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
            var newLines = new List<string>(); // Тут буде новий список без видаленого юзера
            bool found = false; // Прапорець, чи знайшли ми кого видаляти

            // Додаємо заголовок (перший рядок) назад у новий список
            if (lines.Count > 0) newLines.Add(lines[0]);

            // Проходимо по всіх інших рядках
            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(',');
                // Якщо це ТОЙ САМИЙ юзер, якого треба видалити
                if (parts.Length >= 2 && parts[0] == login && parts[1] == password)
                {
                    found = true; // Знайшли!
                    continue; // Пропускаємо цей крок циклу (НЕ додаємо його в новий список)
                }
                // Всіх інших додаємо
                newLines.Add(lines[i]);
            }

            // Якщо знайшли і видалили - перезаписуємо файл новим списком
            if (found) File.WriteAllLines(filePath, newLines, Encoding.UTF8);
            return found;
        }
    }

    // --- СЕРВІС ТОВАРІВ ---
    // Тут ми додаємо, видаляємо і читаємо ліки
    public class ProductService
    {
        private string filePath = "products.csv"; // Файл з ліками

        public ProductService()
        {
            // Якщо файлу нема - створюємо і наповнюємо тестовими даними
            if (!File.Exists(filePath))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Id,Name,Price,Quantity"); // Шапка
                sb.AppendLine("1,Нурофен,120,50");
                sb.AppendLine("2,Йод,40,100");
                sb.AppendLine("3,Едем,150,30");
                sb.AppendLine("4,Аспірин,60,80");
                sb.AppendLine("5,Вітамін С,200,40");
                // Записуємо все це у файл
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
        }

        // Метод, щоб отримати список всіх ліків з файлу
        public List<Product> GetAll()
        {
            var list = new List<Product>(); // Створюємо пустий список
            if (!File.Exists(filePath)) return list;

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            // Пропускаємо шапку (Skip 1)
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length < 4) continue; // Захист від битих рядків

                // Перетворюємо текст (string) у числа (int, double)
                if (int.TryParse(parts[0], out int id) &&
                    double.TryParse(parts[2], out double price) &&
                    int.TryParse(parts[3], out int qty))
                {
                    // Створюємо об'єкт Product і додаємо в список
                    list.Add(new Product { Id = id, Name = parts[1], Price = price, Quantity = qty });
                }
            }
            return list;
        }

        // Додавання нового товару
        public void Add(string name, double price, int quantity)
        {
            // Генеруємо новий ID
            int newId = IdGenerator.GetNewId(filePath);
            // Формуємо рядок для CSV
            string line = $"{newId},{name},{price},{quantity}";
            // Дописуємо в кінець файлу
            File.AppendAllText(filePath, line + Environment.NewLine, Encoding.UTF8);
        }

        // Видалення товару по ID
        public void Delete(int id)
        {
            var products = GetAll(); // Спочатку беремо всі товари в пам'ять

            // Шукаємо товар з таким ID
            var itemToRemove = products.FirstOrDefault(p => p.Id == id);

            if (itemToRemove != null)
            {
                products.Remove(itemToRemove); // Видаляємо зі списку в пам'яті

                // Тепер треба перезаписати файл
                var lines = new List<string> { "Id,Name,Price,Quantity" }; // Починаємо з шапки
                foreach (var p in products)
                {
                    // Додаємо кожен товар назад у список рядків
                    lines.Add($"{p.Id},{p.Name},{p.Price},{p.Quantity}");
                }
                // Записуємо все у файл
                File.WriteAllLines(filePath, lines, Encoding.UTF8);
            }
        }
    }

    // --- СЕРВІС КЛІЄНТІВ ---
    // Працює так само, як ProductService, але для людей
    public class ClientService
    {
        private string filePath = "clients.csv";

        public ClientService()
        {
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, "Id,Name,Phone\n", Encoding.UTF8);
        }

        // Отримати всіх клієнтів
        public List<Client> GetAll()
        {
            var list = new List<Client>();
            if (!File.Exists(filePath)) return list;

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                // Парсимо ID і телефон
                if (parts.Length >= 3 && int.TryParse(parts[0], out int id) && long.TryParse(parts[2], out long phone))
                {
                    list.Add(new Client { Id = id, Name = parts[1], Phone = phone });
                }
            }
            return list;
        }

        // Додати клієнта
        public void Add(string name, long phone)
        {
            int newId = IdGenerator.GetNewId(filePath);
            File.AppendAllText(filePath, $"{newId},{name},{phone}\n", Encoding.UTF8);
        }

        // Видалити клієнта
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

    // --- ГОЛОВНИЙ КЛАС ПРОГРАМИ ---
    class Program
    {
        // Створюємо об'єкти наших сервісів, щоб ними користуватися
        static ProductService productService = new ProductService();
        static ClientService clientService = new ClientService();
        static UserService userService = new UserService();

        // Точка входу (звідси стартує програма)
        public static void Main(string[] args)
        {
            // Вмикаємо українську мову в консолі, щоб не було знаків питання замість букв
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            bool isAuthenticated = false; // Змінна, яка показує, чи ми увійшли

            // Поки не увійдемо - крутимо цей цикл
            while (!isAuthenticated)
            {
                Console.Clear(); // Чистимо екран
                Console.WriteLine("=== ВІТАЄМО В СИСТЕМІ ===");
                Console.WriteLine("1. Вхід");
                Console.WriteLine("2. Реєстрація");
                Console.WriteLine("3. Видалити акаунт");
                Console.WriteLine("4. Вихід");

                // Питаємо вибір у користувача
                int choice = (int)GetUserInput("Ваш вибір:");

                if (choice == 1) // Вхід
                {
                    Console.Write("Логін: ");
                    string l = Console.ReadLine() ?? ""; // ?? "" означає: якщо null, то взяти пустий рядок
                    Console.Write("Пароль: ");
                    string p = Console.ReadLine() ?? "";

                    if (userService.Login(l, p)) isAuthenticated = true; // Ураааа, зайшли
                    else
                    {
                        Console.WriteLine("Помилка: Невірний логін або пароль.");
                        Console.ReadKey();
                    }
                }
                else if (choice == 2) // Реєстрація
                {
                    Console.Write("Новий логін: ");
                    string newLogin = Console.ReadLine() ?? "";
                    Console.Write("Новий пароль: ");
                    string newPass = Console.ReadLine() ?? "";

                    if (userService.Register(newLogin, newPass)) Console.WriteLine("Реєстрація успішна! Тепер увійдіть.");
                    else Console.WriteLine("Такий користувач вже існує!");
                    Console.ReadKey();
                }
                else if (choice == 3) // Видалення
                {
                    Console.Write("Логін для видалення: ");
                    string delLogin = Console.ReadLine() ?? "";
                    Console.Write("Пароль: ");
                    string delPass = Console.ReadLine() ?? "";

                    if (userService.DeleteUser(delLogin, delPass)) Console.WriteLine("Акаунт видалено.");
                    else Console.WriteLine("Помилка видалення.");
                    Console.ReadKey();
                }
                else if (choice == 4) ExitProgram(); // Вихід
            }

            // Якщо цикл закінчився (ми увійшли), показуємо головне меню
            RenderIntro();
            ShowMainMenu();
        }

        // Просто красива заставка
        public static void RenderIntro()
        {
            Console.Clear();
            Console.WriteLine("===========================================");
            Console.WriteLine("==== Ласкаво просимо до Аптеки Здоров'я ====");
            Console.WriteLine("===========================================");
            Console.WriteLine("Натисніть будь-яку клавішу...");
            Console.ReadKey();
        }

        // Дуже корисний метод!
        // Він зчитує число і не дає програмі впасти, якщо користувач ввів літери
        public static double GetUserInput(string prompt = "Введіть число:")
        {
            Console.Write(prompt + " ");
            // TryParse пробує перетворити текст у число. Повертає true якщо вийшло.
            bool isNumber = Double.TryParse(Console.ReadLine(), out double choice);

            if (!isNumber) // Якщо ввели не число
            {
                Console.WriteLine("Ви ввели не число! Спробуйте ще раз.");
                return GetUserInput(prompt); // Рекурсія: викликаємо цей же метод знову
            }
            return choice;
        }

        // Головне меню, де ми вибираємо, куди йти далі
        public static void ShowMainMenu()
        {
            while (true) // Безкінечний цикл, щоб меню не закривалось
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

                switch (choice) // Перемикач вибору
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

        // Підменю для роботи з товарами
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
                    case 6: return; // return тут викидає нас назад у попереднє меню
                    default: Console.WriteLine("Невірний вибір!"); Console.ReadKey(); break;
                }
            }
        }

        // Вивід таблиці товарів
        private static void DisplayProducts()
        {
            Console.Clear();
            Console.WriteLine("=== СПИСОК ТОВАРІВ ===");
            // {0,-5} означає: вставити нульовий аргумент, вирівняти вліво, ширина 5 символів
            Console.WriteLine("| {0,-5} | {1,-20} | {2,10} | {3,10} |", "ID", "Назва", "Ціна", "К-сть");
            Console.WriteLine(new string('-', 56)); // Малюємо лінію

            var products = productService.GetAll();
            foreach (var p in products)
            {
                // :F2 означає два знаки після коми (для ціни)
                Console.WriteLine("| {0,-5} | {1,-20} | {2,10:F2} | {3,10} |", p.Id, p.Name, p.Price, p.Quantity);
            }
            Console.WriteLine(new string('-', 56));
            Console.ReadKey();
        }

        // Додавання товару через консоль
        private static void AddProduct()
        {
            Console.Write("Назва: ");
            string name = Console.ReadLine() ?? "";
            double price = GetUserInput("Ціна:");
            int quantity = (int)GetUserInput("Кількість:");

            productService.Add(name, price, quantity);
            Console.WriteLine("Товар збережено у файл!");
            Console.ReadKey();
        }

        // Видалення товару
        private static void DeleteProduct()
        {
            Console.Clear();
            Console.WriteLine("--- Видалення товару ---");
            // Спочатку показуємо список, щоб знати ID
            var products = productService.GetAll();
            foreach (var p in products) Console.WriteLine($"{p.Id}. {p.Name}");

            int idToDelete = (int)GetUserInput("Введіть ID товару для видалення:");
            productService.Delete(idToDelete);
            Console.WriteLine("Операцію завершено.");
            Console.ReadKey();
        }

        // Меню сортування
        private static void ShowSortMenu()
        {
            Console.Clear();
            Console.WriteLine("\n--- Сортування ---");
            Console.WriteLine("1. Стандартне (List.Sort)");
            Console.WriteLine("2. Власне (Бульбашкове)");

            int choice = (int)GetUserInput("Виберіть метод:");
            var products = productService.GetAll(); // Беремо список

            // Варіант 1: Вбудоване сортування C#
            if (choice == 1) products.Sort((a, b) => a.Price.CompareTo(b.Price));

            // Варіант 2: Алгоритм "Бульбашка" 
            else if (choice == 2)
            {
                // Два цикли: один проходить по списку, інший штовхає елемент
                for (int i = 0; i < products.Count - 1; i++)
                {
                    for (int j = 0; j < products.Count - i - 1; j++)
                    {
                        // Якщо лівий елемент дорожчий за правий - міняємо місцями
                        if (products[j].Price > products[j + 1].Price)
                        {
                            var temp = products[j]; // Тимчасова змінна
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

        // Меню клієнтів (все аналогічно товарам)
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

        
        // Виведення списку клієнтів
        private static void DisplayClients()
        {
            Console.WriteLine("\nСписок клієнтів:");
            // Звертаємось до сервісу, щоб отримати список з файлу
            var clients = clientService.GetAll();

            // Перебираємо всіх клієнтів і друкуємо їх
            foreach (var c in clients)
            {
                Console.WriteLine($"{c.Id}. {c.Name} - {c.Phone}");
            }
            Console.ReadKey(); // Чекаємо, поки натиснуть кнопку
        }

        // Додавання нового клієнта
        private static void AddClient()
        {
            Console.Write("Ім'я: ");
            string name = Console.ReadLine() ?? "";

            // Тут питаємо телефон. GetUserInput повертає double, тому приводимо до long
            long phone = (long)GetUserInput("Телефон (тільки цифри, без +):");

            // Викликаємо метод сервісу для запису в файл
            clientService.Add(name, phone);
            Console.WriteLine("Клієнта додано!");
            Console.ReadKey();
        }

        // Видалення клієнта
        private static void DeleteClient()
        {
            Console.Clear();
            Console.WriteLine("--- Видалення клієнта ---");

            // Спочатку показуємо список, щоб користувач бачив ID
            DisplayClients();

            // Просимо ввести номер для видалення
            int id = (int)GetUserInput("Введіть ID:");

            // Видаляємо через сервіс
            clientService.Delete(id);
            Console.WriteLine("Виконано.");
            Console.ReadKey();
        }

        // Демонстрація розрахунку вартості (як на касі)
        private static void ShowOrderMenu()
        {
            Console.Clear();
            Console.WriteLine("=== ЗАМОВЛЕННЯ ===");
            Console.WriteLine("Доступні товари:");
            foreach (var p in productService.GetAll()) Console.WriteLine($"{p.Name} - {p.Price} грн");

            Console.WriteLine("\n*Розрахунок (демо)*");

            // Захардкоджені ціни для прикладу
            double priceNurofen = 120, priceIodine = 40, priceEdem = 150, priceAspirin = 60, priceVitC = 200;

            // Питаємо скільки чого треба
            double Nurofen = GetUserInput("Кількість Нурофену:");
            double Iodine = GetUserInput("Кількість Йоду:");
            double Edem = GetUserInput("Кількість Едему:");
            double Aspirin = GetUserInput("Кількість Аспірину:");
            double VitaminC = GetUserInput("Кількість Вітаміну С:");

            // Рахуємо загальну суму (кількість * ціна)
            double totalPrice = Nurofen * priceNurofen +
                                Iodine * priceIodine +
                                Edem * priceEdem +
                                Aspirin * priceAspirin +
                                VitaminC * priceVitC;

            // Робимо випадкову знижку від 10% до 100%
            int discount = new Random().Next(10, 101);

            // Виводимо кінцеву суму
            Console.WriteLine($"\nДо сплати: {totalPrice - (totalPrice * discount / 100)} грн (Знижка {discount}%)");
            Console.ReadKey();
        }

        // Пошук
        private static void SearchProductByNameStart()
        {
            Console.Clear();
            Console.WriteLine("=== ПОШУК ===");
            Console.Write("Введіть перші літери назви: ");
            string search = Console.ReadLine()?.ToLower() ?? ""; // Переводимо в малі букви

            var products = productService.GetAll();
            bool found = false;
            Console.WriteLine("\nРезультати пошуку:");
            foreach (var p in products)
            {
                // StartsWith перевіряє початок слова
                if (p.Name.ToLower().StartsWith(search))
                {
                    Console.WriteLine($"ID: {p.Id} | {p.Name} - {p.Price} грн");
                    found = true;
                }
            }
            if (!found) Console.WriteLine("Не знайдено");
            Console.ReadKey();
        }

        // Проста статистика
        private static void ShowStatistics()
        {
            Console.Clear();
            Console.WriteLine("=== СТАТИСТИКА ===");
            var products = productService.GetAll();
            if (products.Count == 0) { Console.WriteLine("Немає товарів"); Console.ReadKey(); return; }

            double totalValue = 0; // Загальна вартість складу
            double maxPrice = 0;   // Найдорожчий товар
            double minPrice = double.MaxValue; // Найдешевший товар (починаємо з макс. можливого)
            int totalQuantity = 0; // Всього штук
            int expensiveCount = 0; // Скільки дорогих товарів

            foreach (var p in products)
            {
                totalValue += p.Price * p.Quantity;
                totalQuantity += p.Quantity;

                if (p.Price > maxPrice) maxPrice = p.Price; // Якщо знайшли дорожчий - оновлюємо
                if (p.Price < minPrice) minPrice = p.Price; // Якщо знайшли дешевший - оновлюємо

                if (p.Price > 100) expensiveCount++;
            }

            Console.WriteLine($"Загальна вартість: {totalValue:F2} грн");
            Console.WriteLine($"Середня ціна: {(totalQuantity > 0 ? totalValue / totalQuantity : 0):F2} грн");
            Console.WriteLine($"Макс. ціна: {maxPrice} грн");
            Console.WriteLine($"Мін. ціна: {minPrice} грн");
            Console.WriteLine($"Товарів > 100 грн: {expensiveCount}");
            Console.ReadKey();
        }

        // Метод, щоб закрити програму
        private static void ExitProgram()
        {
            Environment.Exit(0);
        }
    }
}