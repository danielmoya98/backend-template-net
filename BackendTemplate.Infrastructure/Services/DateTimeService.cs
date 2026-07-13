using BackendTemplate.Application.Common.Interfaces;

namespace BackendTemplate.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
