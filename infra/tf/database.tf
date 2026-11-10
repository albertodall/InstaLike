resource "azurerm_mssql_server" "instalike_sql_server" {
  name                = "sql-${local.name_suffix}"
  resource_group_name = azurerm_resource_group.instalike_resource_group.name
  location            = azurerm_resource_group.instalike_resource_group.location
  version             = "12.0"
  minimum_tls_version = "1.2"

  azuread_administrator {
    login_username              = data.azuread_group.sql_administrators.display_name
    object_id                   = data.azuread_group.sql_administrators.object_id
    azuread_authentication_only = true
  }

  tags = local.common_tags

  depends_on = [
    azurerm_resource_group.instalike_resource_group
  ]
}

resource "azurerm_mssql_database" "instalike_database" {
  name                 = "sqldb-${local.name_suffix}"
  server_id            = azurerm_mssql_server.instalike_sql_server.id
  collation            = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb          = 2
  read_scale           = false
  sku_name             = "GP_S_Gen5_2"
  storage_account_type = "Local"
  zone_redundant       = false

  tags = local.common_tags

  depends_on = [
    azurerm_mssql_server.instalike_sql_server
  ]
}

resource "azurerm_mssql_firewall_rule" "instalike_sql_allow_azure_services" {
  name             = "AllowAllWindowsAzureIps" # The web app and the Microsoft-hosted pipeline agents
  server_id        = azurerm_mssql_server.instalike_sql_server.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
