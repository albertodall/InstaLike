resource "azurerm_key_vault" "instalike_key_vault" {
  name                        = "InstaLike-secrets-vault"
  location                    = azurerm_resource_group.instalike_resource_group.location
  resource_group_name         = azurerm_resource_group.instalike_resource_group.name
  enabled_for_disk_encryption = false
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  soft_delete_retention_days  = 90
  purge_protection_enabled    = false

  tags = local.common_tags

  depends_on = [
    azurerm_resource_group.instalike_resource_group
  ]
}

resource "azurerm_storage_account" "instalike_storage_account" {
  name                      = "instalikepicturesstorage"
  resource_group_name       = azurerm_resource_group.instalike_resource_group.name
  location                  = azurerm_resource_group.instalike_resource_group.location
  account_kind              = "StorageV2"
  account_tier              = "Standard"
  account_replication_type  = "LRS"
  access_tier               = "Hot"
  min_tls_version           = "TLS1_2"
  shared_access_key_enabled = true

  tags = local.common_tags

  depends_on = [
    azurerm_resource_group.instalike_resource_group # See global.tf
  ]
}

resource "azurerm_storage_container" "instalike_posts_container" {
  name                  = "posts"
  storage_account_name  = azurerm_storage_account.instalike_storage_account.name
  container_access_type = "blob"

  depends_on = [
    azurerm_storage_account.instalike_storage_account
  ]
}

resource "azurerm_storage_container" "instalike_profiles_container" {
  name                  = "profiles"
  storage_account_name  = azurerm_storage_account.instalike_storage_account.name
  container_access_type = "blob"

  depends_on = [
    azurerm_storage_account.instalike_storage_account
  ]
}

resource "azurerm_service_plan" "instalike_appservice_plan" {
  name                   = "InstaLike-Plan"
  location               = azurerm_resource_group.instalike_resource_group.location
  resource_group_name    = azurerm_resource_group.instalike_resource_group.name
  os_type                = "Linux"
  sku_name               = "B1"
  zone_balancing_enabled = false

  tags = local.common_tags

  depends_on = [
    azurerm_resource_group.instalike_resource_group # See global.tf
  ]
}

resource "azurerm_cognitive_account" "instalike_autotag_service" {
  name                = "InstaLike-Images-AutoTag"
  location            = azurerm_resource_group.instalike_resource_group.location
  resource_group_name = azurerm_resource_group.instalike_resource_group.name
  kind                = "ComputerVision"

  sku_name = "F0"

  tags = local.common_tags

  depends_on = [
    azurerm_resource_group.instalike_resource_group # See global.tf
  ]
}