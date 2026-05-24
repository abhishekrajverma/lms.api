using EMS.Application.DTOs.Organisation;
using EMS.Application.Interfaces.Repositories.Organisation;
using EMS.Shared.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Application.Services.Organisation
{
    public class OrganisationService
    {
        public readonly IOrganisationRepository _organisationRepository;
        public OrganisationService(IOrganisationRepository organisationRepository) {
            _organisationRepository = organisationRepository;
        }

        public async Task<int> CreateOrganisationAsync(CreateOrganisationDto dto)
        {
            // 🔹 BUSINESS RULES (Application Logic)

            if (string.IsNullOrWhiteSpace(dto.LegalName))
                throw new ArgumentException("Legal name is required.");

            if (string.IsNullOrWhiteSpace(dto.OrganisationCode))
                throw new ArgumentException("Organisation code is required.");

            if (dto.AcademicYearStartMonth < 1 || dto.AcademicYearStartMonth > 12)
                throw new ArgumentException("Academic year start month must be between 1 and 12.");

            // Example rule based on type
            if (dto.OrganisationType == "Trust" && string.IsNullOrWhiteSpace(dto.TrustRegistrationNo))
                throw new ArgumentException("Trust registration number is required for Trust organisations.");

            // 🔹 Call Repository (Infrastructure will handle SQL)
            var organisationId = await _organisationRepository.CreateOrganisationAsync(dto);

            return organisationId;
        }

    }
}
