namespace CreditCardApplications;

/// <summary>
/// Contacts an external service to validate a frequent flyer number.
/// 
/// Some of the reasons we may want to mock this external service could include:
///     - Cost. An external company provides this service and bills per usage
///     - Slow. The call takes a long time to complete and will slow down unit tests
///     - No test version. The external company does not provide a test version, 
///       only a live "real" data production version
///     - Unreliable. The external service fails often, making our tests fail
///     - Effort. The effort (and/or complexity) to use the real service 
///       makes tests painful to write
/// </summary>
public class FrequentFlyerNumberValidatorService : IFrequentFlyerNumberValidator
{
    public bool IsValid(string frequentFlyerNumber)
    {
        throw new NotImplementedException("Simulate this real dependency being hard to use");
    }

    public void IsValid(string frequentFlyerNumber, out bool isValid)
    {
        throw new NotImplementedException("Simulate this real dependency being hard to use");
    }

    // public string LicenseKey
    // {
    //     get { throw new NotImplementedException("For demo purposes"); }
    // }

    public IServiceInformation ServiceInformation => throw new NotImplementedException();

    public ValidationMode ValidationMode
    {
        get => throw new NotImplementedException("For demo purposes");
        set => throw new NotImplementedException("For demo purposes");
    }

    public event EventHandler ValidatorLookupPerformed;
}