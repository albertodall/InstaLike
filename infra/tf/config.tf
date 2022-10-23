terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.28.0"
    }
    azuread = {
      source = "hashicorp/azuread"
      version = "2.29.0"
    }
    cloudflare = {
      source = "cloudflare/cloudflare"
      version = "3.26.0"
    }
  }
  cloud {
    organization = "albertodallagiacoma"

    workspaces {
      name = "InstaLike"
    }
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
  api_token = var.cloudflare_api_token
}
