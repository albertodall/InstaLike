variable "sql_administrators_group" {
  description = "Entra ID group of the Azure SQL administrators (include the pipeline service principal)"
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