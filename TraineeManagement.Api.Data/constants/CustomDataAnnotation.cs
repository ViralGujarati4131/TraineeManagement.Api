using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TraineeManagement.Api.Data.CustomDataAnnotation;

public class RequiredField : RequiredAttribute
{
    public RequiredField() => ErrorMessage = "This field cannot be left blank.";
}

public class FieldMaxLength : MaxLengthAttribute 
{
    public FieldMaxLength(int length) : base(length) => 
        ErrorMessage = $"Value cannot exceed {length} characters.";
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class ValidDateRangeAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public ValidDateRangeAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
        ErrorMessage = "DueDate can not be before the assigned date";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        DateOnly currentValue = (DateOnly)value;

        PropertyInfo? property = validationContext.ObjectType.GetProperty(_comparisonProperty);
        if (property == null)
        {
            return new ValidationResult($"Unknown property: {_comparisonProperty}");
        }

        Object? comparisonValue = property.GetValue(validationContext.ObjectInstance);

        if (comparisonValue is DateOnly targetDate && currentValue < targetDate)
        {
            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class ValidEnumAttribute : ValidationAttribute
{
    private readonly Type _enumType;

    public ValidEnumAttribute(Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("The type provided must be an Enum.");
        }
        _enumType = enumType;
        ErrorMessage = $"The provided value is not a valid {_enumType.Name} status.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        bool isValidEnum = Enum.IsDefined(_enumType, value);

        if (!isValidEnum)
        {
            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}