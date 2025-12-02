namespace Api.Hub
{
    public class UserResponseDto
    {
        public Guid SessionId { get; set; } = Guid.Empty;

        public string Message { get; set; } = string.Empty;

        public string TaskId { get; set; } = string.Empty;

        public bool HasError { get; set; } = false;

        public Guid VacationPlanId { get; set; } = Guid.Empty;

        public string Source { get; set; } = string.Empty;

        public bool IsEndOfStream { get; set; } = false;
    }
}
