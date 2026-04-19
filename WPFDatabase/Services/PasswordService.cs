using System.Security.Cryptography;

namespace WPFDatabase.Services;

public static class PasswordService
{
    // Для каждого пароля создаем свою соль, чтобы одинаковые пароли не давали одинаковый хеш
    public static (string Hash, string Salt) CreateHash(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256);

        var hashBytes = pbkdf2.GetBytes(32);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    // При входе считаем хеш заново на основе сохраненной соли и сравниваем с тем, что лежит в БД
    public static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256);

        var hashBytes = pbkdf2.GetBytes(32);
        var computedHash = Convert.ToBase64String(hashBytes);

        return computedHash == storedHash;
    }

    // Проверка сложности пароля
    public static bool IsPasswordStrong(string password)
    {
        if (password.Length < 8)
        {
            return false;
        }

        if (!password.Any(char.IsUpper))
        {
            return false;
        }

        if (!password.Any(char.IsLower))
        {
            return false;
        }

        if (!password.Any(char.IsDigit))
        {
            return false;
        }

        return true;
    }
}
