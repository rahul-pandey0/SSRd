using SSRd.DTOs;
using SSRd.Models;

namespace SSRd.Services
{
    public interface IMembershipService
    {
        Task<TmMembership?> Getmemberdetails(string membershipNo);
        Task<List<TtRdDeposit>> Getrdaccdetails(string? membershipNo, string? rdaccno);
        Task<RDResponse> RegisterAsync(RDCreateRequest request, string createdBy);
        Task<RDResponse> AuthorizeAsync(int rdDepositsId, int adminUserId, string adminUserName, string? adminBranch);
        Task<MembersResponse> MemAuthorizeAsync(int memberId, int adminUserId, string adminUserName, string adminBranch);
        Task<List<TtRdDepositOnline>> GetPendingAsync();
        Task<List<Tmmebershipregistration>> GetMemPendingAsync();

    }
}
