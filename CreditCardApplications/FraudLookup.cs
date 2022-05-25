namespace CreditCardApplications;

public class FraudLookup
{
    virtual public bool IsFraudRisk(CreditCardApplication application)
    {
        if (application.LastName == "Smith")
        {
            return true;
        }

        return false;
    }
}