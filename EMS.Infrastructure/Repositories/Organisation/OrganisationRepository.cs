using Dapper;
using EMS.Application.DTOs.Organisation;
using EMS.Application.Interfaces.Repositories.Organisation;
using EMS.Shared.Interfaces.Repositories;
using System.Data;

namespace EMS.Infrastructure.Repositories.Organisation
{
    public class OrganisationRepository : IOrganisationRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public OrganisationRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int> CreateOrganisationAsync(CreateOrganisationDto dto)
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
INSERT INTO OrgStructure.Organisation
(
    LegalName, DisplayName, OrganisationCode, OrganisationType,
    TrustRegistrationNo, SocietyRegistrationNo, PAN, TAN, GSTIN,
    Email, Phone, Website,
    AddressLine1, AddressLine2, City, State, Country, Pincode,
    AcademicYearStartMonth, LogoUrl, BoardLogoUrl
)
VALUES
(
    @LegalName, @DisplayName, @OrganisationCode, @OrganisationType,
    @TrustRegistrationNo, @SocietyRegistrationNo, @PAN, @TAN, @GSTIN,
    @Email, @Phone, @Website,
    @AddressLine1, @AddressLine2, @City, @State, @Country, @Pincode,
    @AcademicYearStartMonth, @LogoUrl, @BoardLogoUrl
);

SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(sql, dto);
            return id;
        }
    }
}
