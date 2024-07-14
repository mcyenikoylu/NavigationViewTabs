namespace App11.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
