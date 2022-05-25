using System;
using System.Collections.Generic;
using Moq;
using Moq.Protected;
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

        mockValidator.DefaultValue = DefaultValue.Mock;

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

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

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
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(false);

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication();

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
    }

    // [Fact]
    // public void DeclineLowIncomeApplicationsOutDemo()
    // {
    //     var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
    //
    //     // Value we want to return from the mocked method that has the out parameter.
    //     var isValid = true;
    //     mockValidator
    //         .Setup(x => x.IsValid(It.IsAny<string>(), out isValid));
    //
    //     var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
    //
    //     var application = new CreditCardApplication()
    //     {
    //         GrossAnnualIncome = 19_999,
    //         Age = 42
    //     };
    //
    //     var decision = sut.EvaluateUsingOut(application);
    //
    //     Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
    // }

    [Fact]
    public void ReferWhenLicenceKeyExpired()
    {
        // var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
        //
        // mockValidator
        //     .Setup(x => x.IsValid(It.IsAny<string>()))
        //     .Returns(true);

        // mockValidator
        //     .Setup(x => x.LicenseKey)
        //     .Returns("EXPIRED");

        // mockValidator
        //     .Setup(x => x.LicenseKey)
        //     .Returns(GetLicenseKeyExpiryString);


        // var mockLicenseData = new Mock<ILicenseData>();
        //
        // mockLicenseData
        //     .Setup(x => x.LicenseKey)
        //     .Returns("EXPIRED");
        //
        // var mockServiceInfo = new Mock<IServiceInformation>();
        //
        // mockServiceInfo
        //     .Setup(x => x.License)
        //     .Returns(mockLicenseData.Object);
        //
        // var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
        //
        // mockValidator
        //     .Setup(x => x.ServiceInformation)
        //     .Returns(mockServiceInfo.Object);
        //
        // mockValidator
        //     .Setup(x => x.IsValid(It.IsAny<string>()))
        //     .Returns(true);

        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("EXPIRED");

        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(true);

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

    [Fact]
    public void UseDetailedLookupForOlderApplications()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        // mockValidator.SetupProperty(x => x.ValidationMode);

        // Gotcha: Call it before making any specific property setups (because this will override them).
        mockValidator.SetupAllProperties();

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication {Age = 30};

        sut.Evaluate(application);

        Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
    }

    [Fact]
    public void ValidateFrequentFlyerNumberForLowIncomeApplications()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication()
        {
            FrequentFlyerNumber = "q"
        };

        sut.Evaluate(application);

        mockValidator.Verify(
            x => x.IsValid(It.IsAny<string>()),
            "Frequent flyer numbers should be validated."
        );
    }

    [Fact]
    public void NotValidateFrequentFlyerNumberForHighIncomeApplicants()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

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
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

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
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication {Age = 30};

        sut.Evaluate(application);

        // mockValidator.VerifySet(
        //     x => x.ValidationMode = ValidationMode.Detailed
        // );

        mockValidator.VerifySet(
            x => x.ValidationMode = It.IsAny<ValidationMode>(),
            Times.Once
        );

        // mockValidator.Verify(
        //     x => x.IsValid(null),
        //     Times.Once
        // );
        //
        // mockValidator.VerifyNoOtherCalls();
    }

    [Fact]
    public void ReferWhenFrequentFlyerValidationError()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Throws(new Exception("Custom message")); // .Throws<Exception>();

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication {Age = 42};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
    }

    [Fact]
    public void IncrementLookupCount()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        mockValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(true)
            .Raises(
                x => x.ValidatorLookupPerformed += null,
                EventArgs.Empty
            );

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication {FrequentFlyerNumber = "x", Age = 25};

        sut.Evaluate(application);

        // mockValidator.Raise(
        //     x => x.ValidatorLookupPerformed += null,
        //     EventArgs.Empty
        // );

        Assert.Equal(1, sut.ValidatorLookupCount);
    }

    [Fact]
    public void ReferInvalidFrequentFlyerApplications_ReturnValuesSequence()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        mockValidator
            .SetupSequence(x => x.IsValid(It.IsAny<string>()))
            .Returns(false)
            .Returns(true);

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        var application = new CreditCardApplication {Age = 25};

        var firstDecision = sut.Evaluate(application);
        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);

        var secondDecision = sut.Evaluate(application);
        Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);
    }

    [Fact]
    public void ReferInvalidFrequentFlyerApplications_MultipleCallsSequence()
    {
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        mockValidator
            .Setup(x => x.ServiceInformation.License.LicenseKey)
            .Returns("OK");

        var frequentFlyerNumbersPassed = new List<string>();

        mockValidator.Setup(x => x.IsValid(Capture.In(frequentFlyerNumbersPassed)));

        var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

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
        var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

        var mockFraudLookup = new Mock<FraudLookup>();

        // mockFraudLookup
        //     .Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>()))
        //     .Returns(true);

        mockFraudLookup
            .Protected()
            .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>())
            .Returns(true);

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

        var sut = new CreditCardApplicationEvaluator(mockValidator);

        var application = new CreditCardApplication {Age = 25};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
    }
}