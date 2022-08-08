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
    ConnectionStrings__DefaultDatabase                         = ""
    DeploymentType                                             = "AzureCloud"
    ExternalStorage__AzureBlobStorage__StorageConnectionString = azurerm_storage_account.instalike_storage_account.primary_connection_string
    ImageAnalysis__AzureComputerVision__ApiKey                 = azurerm_cognitive_account.instalike_autotag_service.primary_access_key
    ImageAnalysis__AzureComputerVision__ApiUrl                 = azurerm_cognitive_account.instalike_autotag_service.endpoint
    Logging__AppInsightsInstrumentationKey                     = azurerm_application_insights.instalike_appinsights.instrumentation_key
  }

  depends_on = [
    azurerm_service_plan.instalike_appservice_plan,
    azurerm_mssql_database.instalike_database,
    azurerm_storage_container.instalike_posts_container,
    azurerm_storage_container.instalike_profiles_container,
    azurerm_cognitive_account.instalike_autotag_service,
    azurerm_application_insights.instalike_appinsights
  ]
}