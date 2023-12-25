using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public interface ICRUD<T>
{
    void Create(T item);
    T Read(string id);
    void Update(string id, T updatedItem);
    void Delete(string id);
    List<T> Search(string searchTerm);
}

public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public UserRole Role { get; set; }
}

public enum UserRole
{
    Administrator,
    Cashier,
    HRManager,
    WarehouseManager,
    Accountant
}

public class Employee
{
    public string Id { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public DateTime BirthDate { get; set; }
    public string PassportSeriesNumber { get; set; }
    public string Position { get; set; }
    public double Salary { get; set; }
    public string UserId { get; set; }
}

public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public double PricePerUnit { get; set; }
    public int QuantityInStock { get; set; }
}

public class SelectedProduct : Product
{
    public int SelectedQuantity { get; set; }
}

public class AccountingRecord
{
    public string Id { get; set; }
    public string Description { get; set; }
    public double Amount { get; set; }
    public DateTime RecordDate { get; set; }
    public bool IsIncome { get; set; }
}

public static class DataSerializer
{
    private static readonly string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "data.json");

    public static void Serialize<T>(List<T> data)
    {
        string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(filePath, jsonData);
    }

    public static List<T> Deserialize<T>()
    {
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<T>>(jsonData);
        }
        return new List<T>();
    }
}

public static class AuthManager
{
    public static User Authenticate()
    {
        Console.WriteLine("Выберите роль:");
        Console.WriteLine("1. Администратор");
        Console.WriteLine("2. Кассир");
        Console.WriteLine("3. Менеджер персонала");
        Console.WriteLine("4. Склад-менеджер");
        Console.WriteLine("5. Бухгалтер");

        ConsoleKeyInfo roleKey = Console.ReadKey();
        Console.WriteLine();

        UserRole selectedRole = GetUserRole(roleKey.KeyChar);

        Console.Write("Введите логин: ");
        string username = Console.ReadLine();

        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        // Ваш код проверки логина и пароля...

        return new User { Id = Guid.NewGuid().ToString(), Username = username, Password = password, Role = selectedRole };
    }

    private static UserRole GetUserRole(char key)
    {
        switch (key)
        {
            case '1':
                return UserRole.Administrator;
            case '2':
                return UserRole.Cashier;
            case '3':
                return UserRole.HRManager;
            case '4':
                return UserRole.WarehouseManager;
            case '5':
                return UserRole.Accountant;
            default:
                Console.WriteLine("Некорректный ввод. Установлено значение по умолчанию: Администратор.");
                return UserRole.Administrator;
        }
    }
}

public class Administrator : ICRUD<User>
{
    private List<User> users;

    public Administrator()
    {
        users = DataSerializer.Deserialize<User>();
        if (users == null)
        {
            users = new List<User>();
        }
    }

    public void Create(User item)
    {
        users.Add(item);
        Console.WriteLine("Пользователь успешно добавлен.");
    }

    public User Read(string id)
    {
        return users.Find(u => u.Id == id);
    }

    public void Update(string id, User updatedItem)
    {
        var user = users.Find(u => u.Id == id);
        if (user != null)
        {
            user.Username = updatedItem.Username;
            user.Password = updatedItem.Password;
            user.Role = updatedItem.Role;
            Console.WriteLine("Пользователь успешно обновлен.");
        }
        else
        {
            Console.WriteLine("Пользователь не найден.");
        }
    }

    public void Delete(string id)
    {
        var user = users.Find(u => u.Id == id);
        if (user != null)
        {
            users.Remove(user);
            Console.WriteLine("Пользователь успешно удален.");
        }
        else
        {
            Console.WriteLine("Пользователь не найден.");
        }
    }

    public List<User> Search(string searchTerm)
    {
        return users.FindAll(u => u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    // Добавленный метод
    public List<User> GetUsers()
    {
        return users;
    }
}

class Program
{
    private static Stack<Action> menuStack = new Stack<Action>();

    static void Main()
    {
        User authenticatedUser = AuthManager.Authenticate();

        if (authenticatedUser != null)
        {
            Console.WriteLine($"Добро пожаловать, {authenticatedUser.Username}!");

            switch (authenticatedUser.Role)
            {
                case UserRole.Administrator:
                    AdminMenu();
                    break;
                // Добавьте обработку других ролей...
                default:
                    Console.WriteLine("Роль не реализована.");
                    break;
            }
        }
        else
        {
            Console.WriteLine("Некорректные учетные данные.");
        }
    }

    static void AdminMenu()
    {
        Administrator admin = new Administrator();

        while (true)
        {
            Console.WriteLine("Меню администратора:");
            Console.WriteLine("1. Просмотреть всех пользователей");
            Console.WriteLine("2. Добавить пользователя");
            Console.WriteLine("3. Редактировать пользователя");
            Console.WriteLine("4. Удалить пользователя");
            Console.WriteLine("5. Поиск по пользователям");
            Console.WriteLine("6. Сохранить данные в файл");
            Console.WriteLine("7. Выйти");

            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();

            switch (key.KeyChar)
            {
                case '1':
                    menuStack.Push(() => ShowAllUsers(admin));
                    break;
                case '2':
                    menuStack.Push(() => AddUser(admin));
                    break;
                case '3':
                    menuStack.Push(() => EditUser(admin));
                    break;
                case '4':
                    menuStack.Push(() => DeleteUser(admin));
                    break;
                case '5':
                    menuStack.Push(() => SearchUsers(admin));
                    break;
                case '6':
                    menuStack.Push(() => SaveDataToFile(admin));
                    break;
                case '7':
                    return;
                case 'B':
                    // Возврат на предыдущий уровень
                    if (menuStack.Count > 0)
                    {
                        var previousMenu = menuStack.Pop();
                        previousMenu.Invoke();
                    }
                    break;
                default:
                    Console.WriteLine("Некорректный ввод. Повторите попытку.");
                    break;
            }
        }
    }

    static void ShowAllUsers(Administrator admin)
    {
        // Реализация вывода всех пользователей
    }

    static void AddUser(Administrator admin)
    {
        // Реализация добавления пользователя
    }

    static void EditUser(Administrator admin)
    {
        // Реализация редактирования пользователя
    }

    static void DeleteUser(Administrator admin)
    {
        // Реализация удаления пользователя
    }

    static void SearchUsers(Administrator admin)
    {
        // Реализация поиска пользователей
    }

    static void SaveDataToFile(Administrator admin)
    {
        DataSerializer.Serialize(admin.GetUsers());
        Console.WriteLine("Данные сохранены в файл.");
    }
}



