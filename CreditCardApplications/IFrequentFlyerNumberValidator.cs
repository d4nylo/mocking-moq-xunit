namespace CreditCardApplications;

public interface ILicenseData
{
    string LicenseKey { get; }
}

public interface IServiceInformation
{
    ILicenseData License { get; }
}

public interface IFrequentFlyerNumberValidator
{
    bool IsValid(string frequentFlyerNumber);

    void IsValid(string frequentFlyerNumber, out bool isValid);

    //string LicenseKey { get; }

    IServiceInformation ServiceInformation { get; }
}