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

variable "web_application_domain" {
  description = "Web application root domain"
  type        = string
}

variable "dns_cname_record_value" {
  description = "The DNS CNAME record value for the application"
  type        = string
}

variable "cloudflare_api_token" {
  description = "CloudFlare API Token"
  type        = string
}

variable "cloudflare_dns_zone_id" {
  description = "CloudFlare DNS zone ID of albertodallagiacoma.it"
  type        = string
}