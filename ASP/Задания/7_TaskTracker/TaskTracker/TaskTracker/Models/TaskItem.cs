namespace TaskTracker.Models;

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string? Description { get; set; }

    // Когда задача была создана
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    // Выполнена ли задача
    public bool IsDone { get; set; } = false;
}