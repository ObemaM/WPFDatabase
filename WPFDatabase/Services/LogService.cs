using WPFDatabase.Models;

namespace WPFDatabase.Services;

public static class LogService
{
    // Каждое важное действие пользователя сохраняем в отдельную таблицу, чтобы потом показать историю операций
    public static void Log(User? actingUser, string actionType, string entityType, int entityId, string details)
    {
        if (actingUser is null)
        {
            return;
        }

        var actionLog = new ActionLog
        {
            UserId = actingUser.Id,
            // Логин сохраняем отдельно, чтобы запись журнала оставалась понятной даже после удаления пользователя
            UserLoginSnapshot = actingUser.Login,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId,
            CreatedAt = DateTime.Now,
            Details = details
        };

        App.DbContext.ActionLogs.Add(actionLog);
        App.DbContext.SaveChanges();
    }
}
