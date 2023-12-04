﻿using System.Net;
using Bit.Api.Models.Public.Response;
using Bit.Core.AdminConsole.Entities;
using Bit.Core.Context;
using Bit.Core.OrganizationFeatures.OrganizationSubscriptions.Interface;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrganizationSubscriptionUpdateRequestModel = Bit.Api.Billing.Public.Models.OrganizationSubscriptionUpdateRequestModel;

namespace Bit.Api.Billing.Public.Controllers;

[Route("public/billing-organization")]
[Authorize("Organization")]
public class OrganizationsController : Controller
{
    private readonly IOrganizationService _organizationService;
    private readonly ICurrentContext _currentContext;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUpdateSecretsManagerSubscriptionCommand _updateSecretsManagerSubscriptionCommand;

    public OrganizationsController(
        IOrganizationService organizationService,
        ICurrentContext currentContext,
        IOrganizationRepository organizationRepository,
        IUpdateSecretsManagerSubscriptionCommand updateSecretsManagerSubscriptionCommand)
    {
        _organizationService = organizationService;
        _currentContext = currentContext;
        _organizationRepository = organizationRepository;
        _updateSecretsManagerSubscriptionCommand = updateSecretsManagerSubscriptionCommand;
    }

    /// <summary>
    /// Update the organization's current subscription for Password Manager and/or Secrets Manager.
    /// </summary>
    /// <param name="model">The request model containing the updated subscription information.</param>
    [HttpPut("subscription")]
    [SelfHosted(NotSelfHostedOnly = true)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> PostSubscriptionAsync([FromBody] OrganizationSubscriptionUpdateRequestModel model)
    {
        var organization = await _organizationRepository.GetByIdAsync(_currentContext.OrganizationId.Value);

        if (model.PasswordManager != null)
        {
            await UpdatePasswordManagerSubscriptionAsync(organization, model);
        }

        if (model.SecretsManager != null)
        {
            var organizationUpdate = model.SecretsManager.ToSecretsManagerSubscriptionUpdate(organization);
            await _updateSecretsManagerSubscriptionCommand.UpdateSubscriptionAsync(organizationUpdate);
        }

        return new OkResult();
    }

    private async Task UpdatePasswordManagerSubscriptionAsync(Organization organization, OrganizationSubscriptionUpdateRequestModel model)
    {
        model.PasswordManager.MaxAutoScaleSeats ??= organization.MaxAutoscaleSeats;
        await _organizationService.UpdateSubscription(organization.Id, (int)model.PasswordManager.Seats,
            model.PasswordManager.MaxAutoScaleSeats);
        if (model.PasswordManager.Storage.HasValue)
        {
            await _organizationService.AdjustStorageAsync(organization.Id, (short)model.PasswordManager.Storage);
        }
    }
}
