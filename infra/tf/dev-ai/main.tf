locals {
  # Azure naming convention: {type}-{workload}-{environment}-{region}
  name_suffix = "instalike-dev-we"

  common_tags = {
    ApplicationName = "InstaLike"
    Environment     = "dev"
  }
}

resource "azurerm_resource_group" "instalike_resource_group" {
  name     = "rg-${local.name_suffix}"
  location = "West Europe"

  tags = local.common_tags
}

module "autotag" {
  source = "../modules/autotag"

  name_suffix         = local.name_suffix
  location            = azurerm_resource_group.instalike_resource_group.location
  resource_group_name = azurerm_resource_group.instalike_resource_group.name
  local_auth_enabled  = true # The web application in its container cannot use the developer's Azure sign-in
  tags                = local.common_tags
}
