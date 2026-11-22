terraform {
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
    }
  }
}

resource "azurerm_cognitive_account" "service" {
  name                  = "oai-${var.name_suffix}"
  location              = var.location
  resource_group_name   = var.resource_group_name
  kind                  = "OpenAI"
  custom_subdomain_name = "oai-${var.name_suffix}"
  local_auth_enabled    = var.local_auth_enabled

  sku_name = "S0"

  tags = var.tags
}

resource "azurerm_cognitive_deployment" "model" {
  name                 = "gpt-4.1-nano"
  cognitive_account_id = azurerm_cognitive_account.service.id

  model {
    format = "OpenAI"
    name   = "gpt-4.1-nano"
  }

  sku {
    name     = "DataZoneStandard" # Pay per token, processed within the EU data zone
    capacity = 10                 # Thousands of tokens per minute
  }
}
