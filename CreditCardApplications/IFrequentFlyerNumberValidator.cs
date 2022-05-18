namespace CreditCardApplications;

public interface IFrequentFlyerNumberValidator
{
    bool IsValid(string frequentFlyerNumber);

    void IsValid(string frequentFlyerNumber, out bool isValid);
}