using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SSRd.Configuration;
using SSRd.Data;
using SSRd.DTOs;
using SSRd.Models;

namespace SSRd.Services;

public class MembershipService : IMembershipService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;
    private readonly RdSettings _rd;

    public MembershipService(AppDbContext db, IJwtService jwt, IOptions<RdSettings> rd)
    {
        _db = db;
        _jwt = jwt;
        _rd = rd.Value;
    }

    public async Task<TmMembership?> Getmemberdetails(string membershipNo)
    {
        return await _db.TmMemberships
            .FirstOrDefaultAsync(x => x.MembershipNo == membershipNo);
    }

    public async Task<List<TtRdDeposit>> Getrdaccdetails(string? membershipNo, string? rdaccno)
    {
        return await _db.TtRdDeposit
            .Where(x =>
                (!string.IsNullOrEmpty(membershipNo) && x.MEMBERSHIP_NO == membershipNo)
                ||
                (!string.IsNullOrEmpty(rdaccno) && x.Deposit_Account == rdaccno))
            .ToListAsync();
    }

    public async Task<RDResponse> RegisterAsync(RDCreateRequest request, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(request.MEMBERSHIP_NO))
            throw new InvalidOperationException("MEMBERSHIP_NO is required.");
        if (string.IsNullOrWhiteSpace(request.Deposit_Type))
            throw new InvalidOperationException("Deposit_Type is required.");
        if (!_rd.AllowedDepositTypes.Contains(request.Deposit_Type))
            throw new InvalidOperationException(
                $"Invalid Deposit_Type. Allowed: {string.Join(", ", _rd.AllowedDepositTypes)}.");
        if (request.Deposit_Amount is null or <= 0)
            throw new InvalidOperationException("Deposit_Amount must be greater than 0.");
        if (request.Tenor is null || !_rd.InterestRates.TryGetValue(request.Tenor.Value, out var rate))
            throw new InvalidOperationException(
                $"Unsupported Tenor. Allowed (years): {string.Join(", ", _rd.InterestRates.Keys)}.");

        var memberExists = await _db.TmMemberships
            .AnyAsync(x => x.MembershipNo == request.MEMBERSHIP_NO);
        if (!memberExists)
            throw new InvalidOperationException("Invalid Membership Number.");

        var now = DateTime.Now;
        var autoAuth = _rd.AutoAuthorize;

        var deposit = new TtRdDepositOnline
        {
            MEMBERSHIP_NO = request.MEMBERSHIP_NO,
            name = request.name,
            Deposit_Type = request.Deposit_Type,
            Tenor = request.Tenor,
            Deposit_Amount = request.Deposit_Amount,
            CreatedDate = now.Date,
            CreatedBy = createdBy,
            Nominee = request.Nominee,
            //CreatedTime = TimeOnly.FromDateTime(now),
            AuthStatus = autoAuth ? "A" : "U",
            AuthBy = autoAuth ? createdBy : null,
            AuthTime = autoAuth ? now : null
        };

        _db.TtRdDepositOnlines.Add(deposit);
        await _db.SaveChangesAsync();

        return new RDResponse
        {
            RdDepositsId = deposit.RdDepositsId,
            MEMBERSHIP_NO = deposit.MEMBERSHIP_NO ?? string.Empty,
            Deposit_Type = deposit.Deposit_Type ?? string.Empty,
            Deposit_Amount = deposit.Deposit_Amount ?? 0,
            Tenor = deposit.Tenor ?? 0,
            InterestRate = rate,
            AuthStatus = deposit.AuthStatus,
            CreatedBy = deposit.CreatedBy,
            CreatedDate = deposit.CreatedDate,
            //  CreatedTime = deposit.CreatedTime,
            AuthBy = deposit.AuthBy,
            AuthTime = deposit.AuthTime,
            Message = autoAuth
                ? "RD created and auto-authorized."
                : "RD created. Pending admin authorization."
        };
    }

    public async Task<RDResponse> AuthorizeAsync(int rdDepositsId, int adminUserId, string adminUserName, string? adminBranch)
    {
        var online = await _db.TtRdDepositOnlines
            .FirstOrDefaultAsync(x => x.RdDepositsId == rdDepositsId)
            ?? throw new InvalidOperationException("RD deposit not found.");

        if (online.AuthStatus == "A")
            throw new InvalidOperationException("RD already authorized.");

        var rate = online.Tenor.HasValue && _rd.InterestRates.TryGetValue(online.Tenor.Value, out var r) ? r : 0;
        var now = DateTime.Now;
        var today = now.Date;
        var tenorMonths = online.Tenor ?? 0;
        var liquidationDate = tenorMonths > 0 ? today.AddMonths(tenorMonths) : today;

        using var tx = await _db.Database.BeginTransactionAsync();

        var product = await _db.TmProductTypes
           .FirstOrDefaultAsync(x => x.Product_Code == online.Deposit_Type);

        var booked = new TtRdDeposit
        {
            BranchCode = adminBranch,
            MEMBERSHIP_NO = online.MEMBERSHIP_NO,
            Deposit_Type = "RD",
            Product_Code = online.Deposit_Type,
            ProductType = "RD",
            InterestRate = rate,
            Deposit_Amount = online.Deposit_Amount,
            InterestExpenseAccount = product.InterestExpenseAccount,
            CashGl = product.CashGl,
            LiabilityGl = product.LiabilityGl,
            Liquidation_Mode = "19",
            Tenor = online.Tenor,
            value_Date = today,
            Book_Date = today,
            Liquidation_Date = liquidationDate,
            Blocked = "N",
            CreatedBy = adminUserId,
            CreatedDate = online.CreatedDate ?? today,
            AuthorisedBy = adminUserId,
            AuthStatus = "A",
            AuthorisedDate = today,
            RecordStatus = "O",
            LiquidationStatus = "N",
            Penalty = 0,
            ProvisionAmount = 0,
            ExcessAmount = 0
        };
        _db.TtRdDeposit.Add(booked);
        await _db.SaveChangesAsync();

        var counter = await _db.TbMembernocounters
    .FirstOrDefaultAsync(x => x.BranchCode == adminBranch);

        if (counter == null)
            throw new InvalidOperationException("Counter details not found.");

        int nextRdCounter = (counter.RD_Ctr ?? 0) + 1;

        booked.RdRefNumber = $"{adminBranch}RD{nextRdCounter.ToString("D5")}";

        counter.RD_Ctr = nextRdCounter;


        // Deposit Account Logic


        if (product == null)
            throw new InvalidOperationException("Product details not found.");

        string bankRefId = counter.BankRef_ID ?? "";

        int nextProductAccNo = Convert.ToInt32(product.ProductBasedAccountNo ?? 0) + 1;

        string productCode = product.ProductBasedCode ?? "RD";

        booked.Deposit_Account = $"{bankRefId}{productCode}{nextProductAccNo.ToString("D5")}";

        product.ProductBasedAccountNo = nextProductAccNo;


        online.AuthStatus = "A";
        online.AuthBy = adminUserName;
        online.AuthTime = now;


        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        await _db.Database.ExecuteSqlRawAsync("CALL P_RDSCHEDULEgen({0})", booked.RdDepositsId);


        return new RDResponse
        {
            RdDepositsId = online.RdDepositsId,
            MEMBERSHIP_NO = online.MEMBERSHIP_NO ?? string.Empty,
            Deposit_Type = online.Deposit_Type ?? string.Empty,
            Deposit_Amount = online.Deposit_Amount ?? 0,
            Tenor = online.Tenor ?? 0,
            InterestRate = rate,
            AuthStatus = online.AuthStatus,
            CreatedBy = online.CreatedBy,
            CreatedDate = online.CreatedDate,
            //CreatedTime = online.CreatedTime,
            AuthBy = online.AuthBy,
            AuthTime = online.AuthTime,
            Message = $"Authorized by {adminUserName} (id {adminUserId}). Booked as {booked.RdRefNumber} / {booked.Deposit_Account}."
        };
    }


    public async Task<List<TtRdDepositOnline>> GetPendingAsync()
    {
        return await _db.TtRdDepositOnlines
            .Where(x => x.AuthStatus == "U")
            .OrderByDescending(x => x.RdDepositsId)
            .ToListAsync();
    }

    public async Task<List<Tmmebershipregistration>> GetMemPendingAsync()
    {
        return await _db.Tmmebershipregistrations
            .Where(x => x.AuthStatus == "U")
            .OrderByDescending(x => x.MemberId)
            .ToListAsync();
    }


    public async Task<MembersResponse> MemAuthorizeAsync(int memberId, int adminUserId, string adminUserName, string adminBranch)
    {
        var online = await _db.Tmmebershipregistrations
            .FirstOrDefaultAsync(x => x.MemberId == memberId)
            ?? throw new InvalidOperationException("Membership not found.");

        if (online.AuthStatus == "A")
            throw new InvalidOperationException("Membership details already authorized.");



        using var tx = await _db.Database.BeginTransactionAsync();

        var counter = await _db.TbMembernocounters
            .FirstOrDefaultAsync(x => x.BranchCode == adminBranch);

        if (counter == null)
            throw new InvalidOperationException("Member counter not found.");

        string bankRefId = counter.BankRef_ID ?? "";

        int nextCounter = (counter.NonMemberCtr ?? 0) + 1;

        string membershipNo = $"{bankRefId}NM{nextCounter.ToString("D4")}";

        var booked = new TmMembership
        {
            MembershipNo = membershipNo,
            PhoneNo = online.PhoneNo,
            Branchcode = adminBranch,
            Name = online.Name,
            FatherName = online.FatherName,
            Address = online.Address,
            Birthdate = online.Birthdate,
            Age = online.Age,
            Pancard = online.Pancard,
            AdharCard = online.AdharCard,
            AuthStatus = "A",
            AuthorisedBy = adminUserId,
            CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            AuthorisedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            FormDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Homebranch ="BR001"

        };
        booked.Memberstatus = (online.Age) >= 18 ? "Major" : "Minor";


        _db.TmMemberships.Add(booked);

        await _db.SaveChangesAsync();

        counter.NonMemberCtr = nextCounter;

        online.AuthStatus = "A";
        online.AuthorisedBy = adminUserId;
        online.AuthorisedDate = DateTime.Now;

        await _db.SaveChangesAsync();

        await tx.CommitAsync();



        return new MembersResponse
        {
            PhoneNo = booked.PhoneNo ?? string.Empty,
            Name = booked.Name ?? string.Empty,
            FatherName = booked.FatherName,
            Address = booked.Address,
            Age = booked.Age,
            Pancard = booked.Pancard,
            AdharCard = booked.AdharCard,
            Message = $"Authorized by {adminUserName} (id {adminUserId}). Membership No is {booked.MembershipNo}."
        };
    }

}
