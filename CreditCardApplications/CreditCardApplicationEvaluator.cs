namespace CreditCardApplications;

public class CreditCardApplicationEvaluator
{
    private readonly IFrequentFlyerNumberValidator _validator;

    private const int AutoReferralMaxAge = 20;
    private const int HighIncomeThreshold = 100_000;
    private const int LowIncomeThreshold = 20_000;

    public int ValidatorLookupCount { get; private set; }

    public CreditCardApplicationEvaluator(IFrequentFlyerNumberValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _validator.ValidatorLookupPerformed += ValidatorLookupPerformed;
    }

    private void ValidatorLookupPerformed(object sender, EventArgs e)
    {
        ValidatorLookupCount++;
    }

    public CreditCardApplicationDecision Evaluate(CreditCardApplication application)
    {
        if (application.GrossAnnualIncome >= HighIncomeThreshold)
        {
            return CreditCardApplicationDecision.AutoAccepted;
        }

        // if (_validator.LicenseKey == "EXPIRED")
        // {
        //     return CreditCardApplicationDecision.ReferredToHuman;
        // }

        if (_validator.ServiceInformation.License.LicenseKey == "EXPIRED")
        {
            return CreditCardApplicationDecision.ReferredToHuman;
        }

        _validator.ValidationMode = application.Age >= 30 ? ValidationMode.Detailed : ValidationMode.Quick;

        // var isValidFrequentFlyerNumber = _validator.IsValid(application.FrequentFlyerNumber);

        bool isValidFrequentFlyerNumber;

        try
        {
            isValidFrequentFlyerNumber = _validator.IsValid(application.FrequentFlyerNumber);
        }
        catch (Exception)
        {
            // Log
            return CreditCardApplicationDecision.ReferredToHuman;
        }

        if (!isValidFrequentFlyerNumber)
        {
            return CreditCardApplicationDecision.ReferredToHuman;
        }

        if (application.Age <= AutoReferralMaxAge)
        {
            return CreditCardApplicationDecision.ReferredToHuman;
        }

        if (application.GrossAnnualIncome < LowIncomeThreshold)
        {
            return CreditCardApplicationDecision.AutoDeclined;
        }

        return CreditCardApplicationDecision.ReferredToHuman;
    }

    // public CreditCardApplicationDecision EvaluateUsingOut(CreditCardApplication application)
    // {
    //     if (application.GrossAnnualIncome >= HighIncomeThreshold)
    //     {
    //         return CreditCardApplicationDecision.AutoAccepted;
    //     }
    //
    //     _validator.IsValid(application.FrequentFlyerNumber, out var isValidFrequentFlyerNumber);
    //
    //     if (!isValidFrequentFlyerNumber)
    //     {
    //         return CreditCardApplicationDecision.ReferredToHuman;
    //     }
    //
    //     if (application.Age <= AutoReferralMaxAge)
    //     {
    //         return CreditCardApplicationDecision.ReferredToHuman;
    //     }
    //
    //     if (application.GrossAnnualIncome < LowIncomeThreshold)
    //     {
    //         return CreditCardApplicationDecision.AutoDeclined;
    //     }
    //
    //     return CreditCardApplicationDecision.ReferredToHuman;
    // }
}