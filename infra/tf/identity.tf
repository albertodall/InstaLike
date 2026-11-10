resource "azurerm_user_assigned_identity" "instalike_app_identity" {
  name                = "id-${local.name_suffix}"
  location            = azurerm_resource_group.instalike_resource_group.location
  resource_group_name = azurerm_resource_group.instalike_resource_group.name

  tags = local.common_tags
}

resource "azurerm_role_assignment" "instalike_app_blob_access" {
  scope                = azurerm_storage_account.instalike_storage_account.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.instalike_app_identity.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_role_assignment" "instalike_app_openai_access" {
  scope                = azurerm_cognitive_account.instalike_autotag_service.id
  role_definition_name = "Cognitive Services OpenAI User"
  principal_id         = azurerm_user_assigned_identity.instalike_app_identity.principal_id
  principal_type       = "ServicePrincipal"
}
