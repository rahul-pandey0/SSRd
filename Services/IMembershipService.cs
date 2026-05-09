using SSRd.DTOs;
using SSRd.Models;

namespace SSRd.Services
{
    public interface IMembershipService
    {

        Task<TmMembership> Getmemberdetails(string membershipNo);
        Task<List<TtRdDeposit>> Getrdaccdetails(string? membershipNo, string? rdaccno );
        Task<RDResponse> RegisterAsync(RDCreateRequest request);

    }
}
