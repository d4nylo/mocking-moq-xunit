using System;
using System.Collections.Generic;
using Moq;
using Moq.Protected;
using Xunit;

namespace CreditCardApplications.Tests;

public class CreditCardApplicationEvaluatorShould
{
    private Mock<IFrequentFlyerNumberValidator> mockValidator;
    private CreditCardApplicationEvaluator sut;

    public CreditCardApplicationEvaluatorShould()
    {
        mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator.SetupAllProperties();

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(true);

        sut = new CreditCardApplicationEvaluator(mockValidator.Object);
    }

    [Fact]
    public void AcceptHighIncomeApplications()
    {
        var application = new CreditCardApplication() {GrossAnnualIncome = 100_000};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
    }

    [Fact]
    public void ReferYoungApplications()
    {
        mockValidator.DefaultValue = DefaultValue.Mock;

        var application = new CreditCardApplication() {Age = 19};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
    }

    [Fact]
    public void DeclineLowIncomeApplications()
    {
        mockValidator
            .Setup(x => x.IsValid(It.IsRegex("[a-z]")))
            .Returns(true);

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
        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(false);

        var application = new CreditCardApplication();

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
    }

    [Fact]
    public void ReferWhenLicenceKeyExpired()
    {
        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("EXPIRED");

        var application = new CreditCardApplication {Age = 42};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
    }

    string GetLicenseKeyExpiryString()
    {
        // Ex. Read from vendor-supplied constants file
        return "EXPIRED";
    }

    [Fact]
    public void UseDetailedLookupForOlderApplications()
    {
        var application = new CreditCardApplication {Age = 30};

        sut.Evaluate(application);

        Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
    }

    [Fact]
    public void ValidateFrequentFlyerNumberForLowIncomeApplications()
    {
        var application = new CreditCardApplication()
        {
            FrequentFlyerNumber = "q"
        };

        sut.Evaluate(application);

        mockValidator.Verify(
            x => x.IsValid(It.IsAny<string>()),
            Times.Once,
            "Frequent flyer numbers should be validated."
        );
    }

    [Fact]
    public void NotValidateFrequentFlyerNumberForHighIncomeApplicants()
    {
        var application = new CreditCardApplication {GrossAnnualIncome = 100_000};

        sut.Evaluate(application);

        mockValidator.Verify(
            x => x.IsValid(It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public void CheckLicenseKeyForLowIncomeApplications()
    {
        var application = new CreditCardApplication {GrossAnnualIncome = 99_000};

        sut.Evaluate(application);

        mockValidator.VerifyGet(
            x => x.ServiceInformation.License.LicenseKey,
            Times.Once
        );
    }

    [Fact]
    public void SetDetailedLookupForOlderApplications()
    {
        var application = new CreditCardApplication {Age = 30};

        sut.Evaluate(application);

        mockValidator.VerifySet(
            x => x.ValidationMode = It.IsAny<ValidationMode>(),
            Times.Once
        );
    }

    [Fact]
    public void ReferWhenFrequentFlyerValidationError()
    {
        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Throws(new Exception("Custom message")); // .Throws<Exception>();

        var application = new CreditCardApplication {Age = 42};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
    }

    [Fact]
    public void IncrementLookupCount()
    {
        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(true)
            .Raises(
                x => x.ValidatorLookupPerformed += null,
                EventArgs.Empty
            );

        var application = new CreditCardApplication {FrequentFlyerNumber = "x", Age = 25};

        sut.Evaluate(application);

        Assert.Equal(1, sut.ValidatorLookupCount);
    }

    [Fact]
    public void ReferInvalidFrequentFlyerApplications_ReturnValuesSequence()
    {
        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        mockValidator
            .SetupSequence(x => x.IsValid(It.IsAny<string>()))
            .Returns(false)
            .Returns(true);

        var application = new CreditCardApplication {Age = 25};

        var firstDecision = sut.Evaluate(application);
        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);

        var secondDecision = sut.Evaluate(application);
        Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);
    }

    [Fact]
    public void ReferInvalidFrequentFlyerApplications_MultipleCallsSequence()
    {
        var frequentFlyerNumbersPassed = new List<string>();

        mockValidator.Setup(x => x.IsValid(Capture.In(frequentFlyerNumbersPassed)));

        var application1 = new CreditCardApplication {Age = 25, FrequentFlyerNumber = "aa"};
        var application2 = new CreditCardApplication {Age = 25, FrequentFlyerNumber = "bb"};
        var application3 = new CreditCardApplication {Age = 25, FrequentFlyerNumber = "cc"};

        sut.Evaluate(application1);
        sut.Evaluate(application2);
        sut.Evaluate(application3);

        // Assert that IsValid was called 3 times with "aa", "bb", and "cc"
        Assert.Equal(new List<string> {"aa", "bb", "cc"}, frequentFlyerNumbersPassed);
    }

    [Fact]
    public void ReferFraudRisk()
    {
        var mockFraudLookup = new Mock<FraudLookup>();

        mockFraudLookup
            .Protected()
            .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>())
            .Returns(true);

        // "Override" sut in the class field to create a version with fraud lookup
        var sut = new CreditCardApplicationEvaluator(mockValidator.Object, mockFraudLookup.Object);

        var application = new CreditCardApplication();

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, decision);
    }

    [Fact]
    public void LinqToMocks()
    {
        // var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
        //
        // mockValidator
        //     .Setup(x => x.ServiceInformation.License.LicenseKey)
        //     .Returns("OK");
        //
        // mockValidator
        //     .Setup(x => x.IsValid(It.IsAny<string>()))
        //     .Returns(true);

        var mockValidator = Mock.Of<IFrequentFlyerNumberValidator>(
            validator =>
                validator.ServiceInformation.License.LicenseKey == "OK" &&
                validator.IsValid(It.IsAny<string>()) == true
        );

        // "Override" sut in class field to demonstrate LINQ setup
        var sut = new CreditCardApplicationEvaluator(mockValidator);

        var application = new CreditCardApplication {Age = 25};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
    }
}