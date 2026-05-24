using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Application.DTOs.Organisation
{
    internal class OrganisationDTO
    {
        public OrganisationDTO() { }

        public string OrganisationID { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationDescription { get; set; }
        public string OrganisationType { get; set; }
        public string createtime { get; set; }
        public string CreateDate { get; set; }
        public string UpdateDate { get; set; }
        public string createdBy { get; set; }
        public string updatedBy { get; set; }
        public string updatedDate { get; set; }
        public string status { get; set; }

    }

    public class CreateOrganisationDto
    {
        public string LegalName { get; set; }
        public string DisplayName { get; set; }
        public string OrganisationCode { get; set; }
        public string OrganisationType { get; set; } // Trust, Society, PvtLtd

        public string TrustRegistrationNo { get; set; }
        public string SocietyRegistrationNo { get; set; }
        public string PAN { get; set; }
        public string TAN { get; set; }
        public string GSTIN { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; } = "India";
        public string Pincode { get; set; }

        public int AcademicYearStartMonth { get; set; } = 4;
        public string LogoUrl { get; set; }
        public string BoardLogoUrl { get; set; }
    }

    public class UpdateOrganisationDto : CreateOrganisationDto
    {
        public int OrganisationId { get; set; }
    }

    public class OrganisationDto
    {
        public int OrganisationId { get; set; }
        public string LegalName { get; set; }
        public string DisplayName { get; set; }
        public string OrganisationCode { get; set; }
        public string OrganisationType { get; set; }

        public string PAN { get; set; }
        public string TAN { get; set; }
        public string GSTIN { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pincode { get; set; }

        public int AcademicYearStartMonth { get; set; }
        public string LogoUrl { get; set; }
        public string BoardLogoUrl { get; set; }

        public bool IsActive { get; set; }
    }


}
