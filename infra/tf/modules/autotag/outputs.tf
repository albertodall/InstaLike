output "service_id" {
  description = "Scope for the role assignments of the callers."
  value       = azurerm_cognitive_account.service.id
}

output "endpoint" {
  description = "OpenAI-compatible v1 endpoint."
  value       = "${azurerm_cognitive_account.service.endpoint}openai/v1/"
}

output "api_key" {
  description = "Primary API key; empty when API keys are not accepted."
  value       = var.local_auth_enabled ? azurerm_cognitive_account.service.primary_access_key : ""
  sensitive   = true
}

output "model" {
  description = "Name of the deployed model."
  value       = azurerm_cognitive_deployment.model.name
}
