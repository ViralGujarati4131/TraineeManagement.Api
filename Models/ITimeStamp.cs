namespace MyApi.Models;

public interface ITimestamp
{
    DateTime CreatedDate { get; set; }
    DateTime UpdatedDate { get; set; }
}