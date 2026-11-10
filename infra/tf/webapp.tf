resource "azurerm_linux_web_app" "instalike_app" {
  name                = "app-${local.name_suffix}"
  resource_group_name = azurerm_resource_group.instalike_resource_group.name
  location            = azurerm_service_plan.instalike_appservice_plan.location
  service_plan_id     = azurerm_service_plan.instalike_appservice_plan.id

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.instalike_app_identity.id]
  }

  site_config {
    always_on           = true
    ftps_state          = "Disabled"
    minimum_tls_version = "1.2"
    websockets_enabled  = false

    application_stack {
      dotnet_version = "10.0"
    }
  }

  app_settings = {
    AZURE_CLIENT_ID                               = azurerm_user_assigned_identity.instalike_app_identity.client_id
    AZURE_TOKEN_CREDENTIALS                       = "ManagedIdentityCredential"
    ConnectionStrings__InstalikeDatabase          = "Server=tcp:${azurerm_mssql_server.instalike_sql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.instalike_database.name};Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    ExternalStorage__AzureBlobStorage__ServiceUri = azurerm_storage_account.instalike_storage_account.primary_blob_endpoint
    ImageAnalysis__Endpoint                       = "${azurerm_cognitive_account.instalike_autotag_service.endpoint}openai/v1/"
    ImageAnalysis__Model                          = azurerm_cognitive_deployment.instalike_autotag_model.name
    ApplicationInsights__ConnectionString         = azurerm_application_insights.instalike_appinsights.connection_string
  }

  tags = local.common_tags

  depends_on = [
    azurerm_service_plan.instalike_appservice_plan,
    azurerm_mssql_database.instalike_database,
    azurerm_mssql_firewall_rule.instalike_sql_allow_azure_services,
    azurerm_storage_container.instalike_posts_container,
    azurerm_storage_container.instalike_profiles_container,
    azurerm_role_assignment.instalike_app_blob_access,
    azurerm_role_assignment.instalike_app_openai_access,
    azurerm_role_assignment.instalike_app_telemetry_access,
    azurerm_cognitive_deployment.instalike_autotag_model,
    azurerm_application_insights.instalike_appinsights
  ]
}

# App Service imports the origin certificate (and its private key, stored as a secret) from the key vault.
resource "azurerm_role_assignment" "web_app_resource_provider_certificate_access" {
  for_each = toset(["Key Vault Certificate User", "Key Vault Secrets User"])

  scope                = azurerm_key_vault.instalike_key_vault.id
  role_definition_name = each.value
  principal_id         = data.azuread_service_principal.web_app_resource_provider.object_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_app_service_certificate" "cloudflare_origin_server_cert" {
  name                = "${azurerm_key_vault.instalike_key_vault.name}-cloudflare-origin-server"
  resource_group_name = azurerm_resource_group.instalike_resource_group.name
  location            = azurerm_linux_web_app.instalike_app.location
  key_vault_secret_id = data.azurerm_key_vault_certificate.cloudflare_origin_server_certificate.secret_id

  depends_on = [
    azurerm_role_assignment.web_app_resource_provider_certificate_access,
    azurerm_linux_web_app.instalike_app
  ]
}