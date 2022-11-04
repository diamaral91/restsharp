
using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto;

namespace DepositAccount.APITest.Infrastructure.database
{
    public class AmazonFinancialEventGroupDB
    {
        private readonly ConnectionBase _connectBase;
        public AmazonFinancialEventGroupDB()
        {
            _connectBase = new ConnectionBase("ConnectionString_RawData");
        }

        public async Task<IEnumerable<FinancialEventGroupDto>> GetFinancialEventGroup(string companyId, string currency)
        {
            return await _connectBase.Get<FinancialEventGroupDto>($@"
                SELECT TOP 10
				    AFEG.OriginalTotalCurrencyCode as Currency
                    ,AFEG.OriginalTotalCurrencyAmount as Amount    
                    ,AFEG.FundTransferDate as FundTransferDate                
                    ,AC.ID as CompanyId
                    ,AFEG.AccountTail as AccountTail
                FROM Amazon.FinancialEventGroup AFEG
                INNER JOIN (
                    SELECT FinancialEventGroupId, MAX(ID) AS Id
                    FROM Amazon.FinancialEventGroup
                    WHERE FundTransferDate IS NOT NULL
                    GROUP BY FinancialEventGroupId) g ON AFEG.FinancialEventGroupId = g.FinancialEventGroupId AND AFEG.Id = g.Id
                INNER JOIN Amazon.UserTokenAuthorization ut on ut.SellerAuthorizationId = AFEG.SellerId
                INNER JOIN Account.Company AC on ut.CompanyId = AC.ID
                LEFT JOIN ApplicationInfo.CompanyInfo ACI on ACI.RegisterTypeId = 6 AND ACI.CompanyId = AC.id AND ACI.Status = 5
                WHERE
                    ut.CompanyId = @{nameof(companyId)} AND AFEG.OriginalTotalCurrencyCode = @{nameof(currency)}
                ORDER BY AFEG.FundTransferDate desc", new { companyId, currency });
        }
    }
}
