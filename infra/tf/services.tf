data "azurerm_client_config" "current" {}

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

  depends_on = [
    azurerm_resource_group.instalike_resource_group # See global.tf
  ]
}

resource "azurerm_cognitive_account" "instalike_autotag_service" {
  name                = "InstaLike-Images-AutoTag"
  location            = azurerm_resource_group.instalike_resource_group.location
  resource_group_name = azurerm_resource_group.instalike_resource_group.name
  kind                = "ComputerVision"
  sku_name            = "F0"

  depends_on = [
    azurerm_resource_group.instalike_resource_group # See global.tf
  ]
}