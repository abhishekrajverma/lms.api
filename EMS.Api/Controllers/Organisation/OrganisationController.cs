using EMS.Application.DTOs.Auth;
using EMS.Application.DTOs.Organisation;
using EMS.Application.Services.Organisation;
using EMS.Shared.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMS.Api.Controllers.Organisation
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganisationController : ControllerBase
    {
        private readonly OrganisationService _organisationService;
        //private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<OrganisationController> _logger;

        public OrganisationController(
            OrganisationService organisationService,
            ILogger<OrganisationController> logger)
        {
            _organisationService = organisationService;
            _logger = logger;
        }

        [HttpPost("InsertOrganisation")]
        public async Task<ActionResult<ApiResponse<CreateOrganisationDto>>> InsertOrganisation(
        [FromBody] CreateOrganisationDto request,
        CancellationToken cancellationToken = default)
        {
            await _organisationService.CreateOrganisationAsync(request);
            return Ok(ApiResponse<CreateOrganisationDto>.SuccessResponse(new CreateOrganisationDto { }, "Organisation created successfully.", 201));
          
        }
    }
}
