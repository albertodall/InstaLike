resource "azurerm_key_vault" "instalike_key_vault" {
  name                        = "kv-${local.name_suffix}"
  location                    = azurerm_resource_group.instalike_resource_group.location
  resource_group_name         = azurerm_resource_group.instalike_resource_group.name
  enabled_for_disk_encryption = false
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  soft_delete_retention_days  = 90
  purge_protection_enabled    = false
  rbac_authorization_enabled  = true

  tags = local.common_tags

  depends_on = [
    azurerm_resource_group.instalike_resource_group
  ]
}

resource "azurerm_storage_account" "instalike_storage_account" {
  name                      = "st${replace(local.name_suffix, "-", "")}"
  resource_group_name       = azurerm_resource_group.instalike_resource_group.name
  location                  = azurerm_resource_group.instalike_resource_group.location
  account_kind              = "StorageV2"
  account_tier              = "Standard"
  account_replication_type  = "LRS"
  access_tier               = "Hot"
  min_tls_version           = "TLS1_2"
  shared_access_key_enabled = false

  allow_nested_items_to_be_public = false

  tags = local.common_tags

  depends_on = [
    azurerm_resource_group.instalike_resource_group # See global.tf
  ]
}

resource "azurerm_storage_container" "instalike_posts_container" {
  name                  = "posts"
  storage_account_id    = azurerm_storage_account.instalike_storage_account.id
  container_access_type = "private"

  depends_on = [
    azurerm_storage_account.instalike_storage_account
  ]
}

resource "azurerm_storage_container" "instalike_profiles_container" {
  name                  = "profiles"
  storage_account_id    = azurerm_storage_account.instalike_storage_account.id
  container_access_type = "private"

  depends_on = [
    azurerm_storage_account.instalike_storage_account
  ]
}

resource "azurerm_service_plan" "instalike_appservice_plan" {
  name                   = "asp-${local.name_suffix}"
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
  name                  = "oai-${local.name_suffix}"
  location              = azurerm_resource_group.instalike_resource_group.location
  resource_group_name   = azurerm_resource_group.instalike_resource_group.name
  kind                  = "OpenAI"
  custom_subdomain_name = "oai-${local.name_suffix}"
  local_auth_enabled    = false # Entra ID only: the web app calls it with its managed identity

  sku_name = "S0"

  tags = local.common_tags

  depends_on = [
    azurerm_resource_group.instalike_resource_group # See global.tf
  ]
}

resource "azurerm_cognitive_deployment" "instalike_autotag_model" {
  name                 = "gpt-4.1-nano"
  cognitive_account_id = azurerm_cognitive_account.instalike_autotag_service.id

  model {
    format = "OpenAI"
    name   = "gpt-4.1-nano"
  }

  sku {
    name     = "DataZoneStandard" # Pay per token, processed within the EU data zone
    capacity = 10                 # Thousands of tokens per minute
  }
}
# Lets Terraform read the origin certificate (see data.tf).
resource "azurerm_role_assignment" "terraform_certificate_access" {
  scope                = azurerm_key_vault.instalike_key_vault.id
  role_definition_name = "Key Vault Certificate User"
  principal_id         = data.azurerm_client_config.current.object_id
}
