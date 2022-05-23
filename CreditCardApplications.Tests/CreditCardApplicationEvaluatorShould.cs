using Moq;
using Xunit;

namespace CreditCardApplications.Tests;

public class CreditCardApplicationEvaluatorShould
{
    [Fact]
    public void AcceptHighIncomeApplications()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication() {GrossAnnualIncome = 100_000};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
    }

    [Fact]
    public void ReferYoungApplications()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(true);

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication() {Age = 19};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
    }

    [Fact]
    public void DeclineLowIncomeApplications()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        // # If the frequentFlyerNumber passed to IsValid is equal to "x"
        // mockValidator
        //     .Setup(x => x.IsValid("x"))
        //     .Returns(true);

        // # If the frequentFlyerNumber passed to IsValid is any string
        // mockValidator
        //     .Setup(x => x.IsValid(It.IsAny<string>()))
        //     .Returns(true);

        // # If the frequentFlyerNumber passed to IsValid starts with "y"
        // mockValidator
        //     .Setup(x => x.IsValid(It.Is<string>(number => number.StartsWith("y"))))
        //     .Returns(true);

        // # If the frequentFlyerNumber passed to IsValid is between "a" and "z" (inclusive)
        // mockValidator
        //     .Setup(x => x.IsValid(It.IsInRange("a", "z", Range.Inclusive)))
        //     .Returns(true);

        // # If the frequentFlyerNumber passed to IsValid is "x", "y", or "z"
        // mockValidator
        //     .Setup(x => x.IsValid(It.IsIn("x", "y", "z")))
        //     .Returns(true);

        // # If the frequentFlyerNumber passed to IsValid matches the regular expression "[a-z]"
        mockValidator
            .Setup(x => x.IsValid(It.IsRegex("[a-z]")))
            .Returns(true);

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication()
        {
            GrossAnnualIncome = 19_999,
            Age = 42,
            FrequentFlyerNumber = "y"
        };

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
    }

    [Fact]
    public void ReferInvalidFrequentFlyerApplication()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Loose);

        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(false);

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication();

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
    }

    [Fact]
    public void DeclineLowIncomeApplicationsOutDemo()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        // Value we want to return from the mocked method that has the out parameter.
        var isValid = true;
        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>(), out isValid));

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication()
        {
            GrossAnnualIncome = 19_999,
            Age = 42
        };

        var decision = sut.EvaluateUsingOut(application);

        Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
    }

    [Fact]
    public void ReferWhenLicenceKeyExpired()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(true);

        // mockValidator
        //     .Setup(x => x.LicenseKey)
        //     .Returns("EXPIRED");

        mockValidator
            .Setup(x => x.LicenseKey)
            .Returns(GetLicenseKeyExpiryString);

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication {Age = 42};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
    }

    string GetLicenseKeyExpiryString()
    {
        // Ex. Read from vendor-supplied constants file
        return "EXPIRED";
    }
}