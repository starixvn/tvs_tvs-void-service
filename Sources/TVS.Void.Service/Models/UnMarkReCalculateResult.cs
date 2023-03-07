using TicketVoidingSystem.Models;

namespace TVS.Void.Service.Models;

public class UnMarkReCalculateResult : Result
{
    public string NegativeBalanceUsers { get; set; }
}