using EMS.Application.DTOs.Organisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Application.Interfaces.Services.Organisation
{
    internal interface IOrganisationService
    {
        Task<int> CreateOrganisationAsync(CreateOrganisationDto dto);
    }
}
