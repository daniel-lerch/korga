using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Korga.Extensions;

public static class DbUpdateExceptionExtensions
{
    public static bool IsUniqueConstraintViolation(this DbUpdateException exception)
    {
        return exception.InnerException is MySqlException sqlException && sqlException.Number == 1062;
    }

    public static bool IsForeignKeyConstraintViolation(this DbUpdateException exception)
    {
        return exception.InnerException is MySqlException sqlException && sqlException.Number == 1452;
    }
}
