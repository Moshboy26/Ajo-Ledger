
using AjoLedger.Api.DTOs;

namespace AjoLedger.Tests;

public class UnitTest1
{
    [Theory]
    [InlineData(0)]
    [InlineData(299)]
    [InlineData(-1)]
    public void RecordContributionRequest_RejectsAmountsBelow300(
        decimal amount)
    {
        // Arrange
        var request = new RecordContributionRequest
        {
            Amount = amount,
            IdempotencyKey = Guid.NewGuid()
        };

        // Act
        var validationResults =
            new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid =
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                request,
                new System.ComponentModel.DataAnnotations.ValidationContext(request),
                validationResults,
                validateAllProperties: true);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData(300)]
    [InlineData(301)]
    [InlineData(500000)]
    public void RecordContributionRequest_AcceptsAmountsOf300OrMore(
        decimal amount)
    {
        // Arrange
        var request = new RecordContributionRequest
        {
            Amount = amount,
            IdempotencyKey = Guid.NewGuid()
        };

        // Act
        var validationResults =
            new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid =
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                request,
                new System.ComponentModel.DataAnnotations.ValidationContext(request),
                validationResults,
                validateAllProperties: true);

        // Assert
        Assert.True(isValid);
    }
}
