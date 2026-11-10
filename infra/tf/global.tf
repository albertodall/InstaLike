locals {
  # Azure naming convention: {type}-{workload}-{environment}-{region}
  workload    = "instalike"
  environment = "prd"
  region      = "weu"
  name_suffix = "${local.workload}-${local.environment}-${local.region}"

  common_tags = {
    ApplicationName = "InstaLike"
    Environment     = local.environment
  }
}

resource "azurerm_resource_group" "instalike_resource_group" {
  name     = "rg-${local.name_suffix}"
  location = "West Europe"

  tags = local.common_tags
}
