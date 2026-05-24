using EMS.Application.DTOs.Organisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Application.Interfaces.Repositories.Organisation
{
    public interface IOrganisationRepository
    {
        Task<int> CreateOrganisationAsync(CreateOrganisationDto dto);

    }
}
