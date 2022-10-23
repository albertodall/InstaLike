locals {
  common_tags = {
    ApplicationName    = "InstaLike"
    Env                = "prd"
  }
}

resource "azurerm_resource_group" "instalike_resource_group" {
  name     = "InstaLike"
  location = "West Europe"

  tags = local.common_tags
}
