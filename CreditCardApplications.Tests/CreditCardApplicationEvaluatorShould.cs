using Xunit;

namespace CreditCardApplications.Tests;

public class CreditCardApplicationEvaluatorShould
{
    [Fact]
    public void AcceptHighIncomeApplications()
    {
        var sut = new CreditCardApplicationEvaluator();

        var application = new CreditCardApplication() {GrossAnnualIncome = 100_000};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
    }

    [Fact]
    public void ReferYoungApplications()
    {
        var sut = new CreditCardApplicationEvaluator();

        var application = new CreditCardApplication() {Age = 19};

        var decision = sut.Evaluate(application);

        Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
    }
}