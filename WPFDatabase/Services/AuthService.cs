using Microsoft.EntityFrameworkCore;
using WPFDatabase.Models;

namespace WPFDatabase.Services;

public static class AuthService
{
    // При входе проверяем логин, пароль и то, что пользователь не заблокирован
    public static (bool Success, string ErrorMessage, User? User) Authenticate(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            return (false, "Введите логин.", null);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "Введите пароль.", null);
        }

        var user = App.DbContext.Users.FirstOrDefault(u => u.Login == login.Trim());
        if (user is null)
        {
            return (false, "Пользователь не найден.", null);
        }

        if (!user.IsActive)
        {
            return (false, "Пользователь заблокирован.", null);
        }

        if (!PasswordService.VerifyPassword(password, user.PasswordHash, user.Salt))
        {
            return (false, "Неверный пароль.", null);
        }

        App.CurrentUser = user;
        LogService.Log(user, "Login", "User", user.Id, $"Пользователь {user.Login} вошел в систему.");

        return (true, string.Empty, user);
    }

    // Обычная регистрация создает пользователя, сразу хеширует пароль и сохраняет дату регистрации
    public static (bool Success, string ErrorMessage, User? User) Register(string login, string password, string confirmPassword)
    {
        var normalizedLogin = login.Trim();

        if (string.IsNullOrWhiteSpace(normalizedLogin))
        {
            return (false, "Введите логин.", null);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "Введите пароль.", null);
        }

        if (password != confirmPassword)
        {
            return (false, "Пароли не совпадают.", null);
        }

        if (!PasswordService.IsPasswordStrong(password))
        {
            return (false, "Пароль должен содержать минимум 8 символов, заглавную букву, строчную букву и цифру.", null);
        }

        if (!IsLoginAvailable(normalizedLogin))
        {
            return (false, "Этот логин уже занят.", null);
        }

        var (hash, salt) = PasswordService.CreateHash(password);
        var user = new User
        {
            Login = normalizedLogin,
            PasswordHash = hash,
            Salt = salt,
            RegisteredAt = DateTime.Now,
            IsActive = true
        };

        App.DbContext.Users.Add(user);
        App.DbContext.SaveChanges();

        LogService.Log(user, "Register", "User", user.Id, $"Пользователь {user.Login} зарегистрировался.");

        return (true, string.Empty, user);
    }

    // Создание пользователя из окна работы с пользователями
    public static (bool Success, string ErrorMessage, User? User) CreateUser(string login, string password, string confirmPassword, bool isActive)
    {
        var normalizedLogin = login.Trim();

        if (string.IsNullOrWhiteSpace(normalizedLogin))
        {
            return (false, "Введите логин.", null);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "Введите пароль.", null);
        }

        if (password != confirmPassword)
        {
            return (false, "Пароли не совпадают.", null);
        }

        if (!PasswordService.IsPasswordStrong(password))
        {
            return (false, "Пароль должен содержать минимум 8 символов, заглавную букву, строчную букву и цифру.", null);
        }

        if (!IsLoginAvailable(normalizedLogin))
        {
            return (false, "Этот логин уже занят.", null);
        }

        var (hash, salt) = PasswordService.CreateHash(password);
        var user = new User
        {
            Login = normalizedLogin,
            PasswordHash = hash,
            Salt = salt,
            RegisteredAt = DateTime.Now,
            IsActive = isActive
        };

        App.DbContext.Users.Add(user);
        App.DbContext.SaveChanges();

        LogService.Log(App.CurrentUser, "Create", "User", user.Id, $"Создан пользователь {user.Login}.");

        return (true, string.Empty, user);
    }

    // При редактировании снова проверяем уникальность логина и пересчитываем хеш для нового пароля
    public static (bool Success, string ErrorMessage) UpdateUser(User user, string login, string password, string confirmPassword, bool isActive)
    {
        var normalizedLogin = login.Trim();

        if (string.IsNullOrWhiteSpace(normalizedLogin))
        {
            return (false, "Введите логин.");
        }

        if (!IsLoginAvailable(normalizedLogin, user.Id))
        {
            return (false, "Этот логин уже занят.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "При редактировании необходимо ввести пароль.");
        }

        if (password != confirmPassword)
        {
            return (false, "Пароли не совпадают.");
        }

        if (!PasswordService.IsPasswordStrong(password))
        {
            return (false, "Пароль должен содержать минимум 8 символов, заглавную букву, строчную букву и цифру.");
        }

        var (hash, salt) = PasswordService.CreateHash(password);
        user.PasswordHash = hash;
        user.Salt = salt;

        user.Login = normalizedLogin;
        user.IsActive = isActive;

        App.DbContext.SaveChanges();

        LogService.Log(App.CurrentUser, "Update", "User", user.Id, $"Обновлен пользователь {user.Login}.");

        return (true, string.Empty);
    }

    // Перед удалением отвязываем все его логи, чтобы история действий в отдельной таблице не пропала
    public static (bool Success, string ErrorMessage) DeleteUser(User user)
    {
        if (App.CurrentUser?.Id == user.Id)
        {
            return (false, "Нельзя удалить текущего пользователя.");
        }

        var removedUserId = user.Id;
        var removedLogin = user.Login;

        var userLogs = App.DbContext.ActionLogs
            .Where(log => log.UserId == user.Id)
            .ToList();

        foreach (var log in userLogs)
        {
            // Убираем связь с пользователем, но саму запись журнала не удаляем
            log.UserId = null;
            log.User = null;

            if (string.IsNullOrWhiteSpace(log.UserLoginSnapshot))
            {
                // Если снимок логина пустой, сохраняем его прямо в логе до удаления пользователя
                log.UserLoginSnapshot = removedLogin;
            }
        }

        App.DbContext.Users.Remove(user);
        App.DbContext.SaveChanges();

        LogService.Log(App.CurrentUser, "Delete", "User", removedUserId, $"Удален пользователь {removedLogin}.");

        return (true, string.Empty);
    }

    // Временная блокировка сделана через флаг активности без физического удаления пользователя
    public static void ToggleUserActive(User user)
    {
        user.IsActive = !user.IsActive;
        App.DbContext.SaveChanges();

        var state = user.IsActive ? "активирован" : "заблокирован";
        LogService.Log(App.CurrentUser, "ToggleActive", "User", user.Id, $"Пользователь {user.Login} был {state}.");
    }

    public static List<User> GetUsers()
    {
        return App.DbContext.Users
            .OrderBy(u => u.Login)
            .ToList();
    }

    // Один и тот же логин нельзя использовать дважды ни при регистрации, ни при редактировании
    private static bool IsLoginAvailable(string login, int? excludedUserId = null)
    {
        return !App.DbContext.Users.Any(u => u.Login == login && (!excludedUserId.HasValue || u.Id != excludedUserId.Value));
    }
}
