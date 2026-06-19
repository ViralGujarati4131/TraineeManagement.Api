namespace TraineeManagementApi.Models.TimestampInterface;

public interface ICreateTimestamp
{
    DateTime CreatedDate { get; set; }
}

public interface IUpdateTimestamp
{
    DateTime UpdatedDate { get; set; }
}