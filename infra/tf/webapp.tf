resource "azurerm_linux_web_app" "instalike_app" {
  name                = "InstaLike-WebApp"
  resource_group_name = azurerm_resource_group.instalike_resource_group.name
  location            = azurerm_service_plan.instalike_appservice_plan.location
  service_plan_id     = azurerm_service_plan.instalike_appservice_plan.id

  site_config {
    always_on           = true
    ftps_state          = "Disabled"
    minimum_tls_version = "1.2"
    websockets_enabled  = false

    application_stack {
      dotnet_version = "3.1"
    }
  }

  app_settings = {
    ConnectionStrings__DefaultDatabase                         = "Server=tcp:${azurerm_mssql_server.instalike_sql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.instalike_database.name};Persist Security Info=False;User ID=${var.azure_sql_database_admin_username};Password=${var.azure_sql_database_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    DeploymentType                                             = "AzureCloud"
    ExternalStorage__AzureBlobStorage__StorageConnectionString = azurerm_storage_account.instalike_storage_account.primary_connection_string
    ImageAnalysis__AzureComputerVision__ApiKey                 = azurerm_cognitive_account.instalike_autotag_service.primary_access_key
    ImageAnalysis__AzureComputerVision__ApiUrl                 = azurerm_cognitive_account.instalike_autotag_service.endpoint
    Logging__AppInsightsInstrumentationKey                     = azurerm_application_insights.instalike_appinsights.instrumentation_key
  }

  tags = local.common_tags
  
  depends_on = [
    azurerm_service_plan.instalike_appservice_plan,
    azurerm_mssql_database.instalike_database,
    azurerm_storage_container.instalike_posts_container,
    azurerm_storage_container.instalike_profiles_container,
    azurerm_cognitive_account.instalike_autotag_service,
    azurerm_application_insights.instalike_appinsights
  ]
}

resource "azurerm_key_vault_access_policy" "instalike_app_keyvault_access_policy" {
  key_vault_id = azurerm_key_vault.instalike_key_vault.id

  tenant_id = azurerm_key_vault.instalike_key_vault.tenant_id
  object_id = data.azuread_service_principal.web_app_resource_provider.id

  secret_permissions = [ "Get" ]

  certificate_permissions = [ "Get" ]

  depends_on = [
    azurerm_key_vault.instalike_key_vault,
    azurerm_linux_web_app.instalike_app
  ]
}

resource "azurerm_app_service_certificate" "cloudflare_origin_server_cert" {
  name                = "${azurerm_key_vault.instalike_key_vault.name}-cloudflare-origin-server"
  resource_group_name = azurerm_resource_group.instalike_resource_group.name
  location            = azurerm_linux_web_app.instalike_app.location
  key_vault_secret_id = data.azurerm_key_vault_certificate.cloudflare_origin_server_certificate.secret_id

  depends_on = [
    azurerm_key_vault_access_policy.instalike_app_keyvault_access_policy,
    azurerm_linux_web_app.instalike_app
  ]
}