data "azurerm_client_config" "current" { }

data "azuread_service_principal" "web_app_resource_provider" {
  application_id = "abfa0a7c-a6b6-4736-8310-5855508787cd"
}

data "azurerm_key_vault_certificate" "cloudflare_origin_server_certificate" {
  # This certificate must have already been uploaded into the keyvault
  name         = "cloudflare-origin-server"
  key_vault_id = azurerm_key_vault.instalike_key_vault.id
}