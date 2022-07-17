resource "azurerm_log_analytics_workspace" "instalike_loganalytics_ws" {
  name                = "InstaLike-Log-Workspace"
  location            = azurerm_resource_group.instalike_resource_group.location
  resource_group_name = azurerm_resource_group.instalike_resource_group.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  depends_on = [
    azurerm_resource_group.instalike_resource_group # See global.tf
  ]
}

resource "azurerm_application_insights" "instalike_appinsights" {
  name                = "InstaLike-AppInsights"
  location            = azurerm_resource_group.instalike_resource_group.location
  resource_group_name = azurerm_resource_group.instalike_resource_group.name
  workspace_id        = azurerm_log_analytics_workspace.instalike_loganalytics_ws.id
  application_type    = "web"

  depends_on = [
    azurerm_log_analytics_workspace.instalike_loganalytics_ws
  ]
}