variable "azure_sql_database_admin_username" {
  description = "Azure SQL Database Administrator Username"
  type        = string
}

variable "azure_sql_database_admin_password" {
  description = "Azure SQL Database Administrator Password"
  type        = string
}

variable "origin_server_certificate_password" {
  description = "Cloudflare Origin Server Certificate Password"
  type        = string
}

variable "web_application_endpoint" {
  description = "Endpoint on which the application will accept requests"
  type        = string
}

variable "cloudflare_user_email" {
  description = "CloudFlare user E-Mail"
  type        = string
}

variable "cloudflare_api_key" {
  description = "CloudFlare API Key"
  type        = string
}

variable "cloudflare_dns_zone_id" {
  description = "CloudFlare DNS zone ID"
  type        = string
}