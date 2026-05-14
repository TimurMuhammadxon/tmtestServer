namespace OnlineTesting.Domain.Tests;

public class BiletQuestion
{
    public Guid BiletId { get; set; }
    public Guid QuestionId { get; set; }
    public int OrderIndex { get; set; }

    public Bilet? Bilet { get; set; }
    public Question? Question { get; set; }
}