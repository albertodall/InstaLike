terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.17.0"
    }
    cloudflare = {
      source = "cloudflare/cloudflare"
      version = "3.20.0"
    }
  }
  backend "azurerm" {
    resource_group_name  = "SharedInfraServices"
    storage_account_name = "adinfrasharedstorage"
    container_name       = "tfstate"
    key                  = "instalike-terraform.tfstate"
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy    = true
      recover_soft_deleted_key_vaults = true
    }
  }
}

provider "cloudflare" {
  email   = var.cloudflare_user_email
  api_key = var.cloudflare_api_key
}

resource "azurerm_resource_group" "instalike_resource_group" {
  name     = "InstaLike"
  location = "West Europe"
}
