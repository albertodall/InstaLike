resource "azurerm_mssql_server" "instalike_sql_server" {
  name                         = "instalike-dbsrv"
  resource_group_name          = azurerm_resource_group.instalike_resource_group.name
  location                     = azurerm_resource_group.instalike_resource_group.location
  version                      = "12.0"
  minimum_tls_version          = "1.2"
  administrator_login          = var.azure_sql_database_admin_username
  administrator_login_password = var.azure_sql_database_admin_password

  tags = local.common_tags

  depends_on = [
    azurerm_resource_group.instalike_resource_group
  ]
}

resource "azurerm_mssql_database" "instalike_database" {
  name                 = "InstaLike"
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