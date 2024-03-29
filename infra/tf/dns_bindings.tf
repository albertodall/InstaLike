resource "cloudflare_record" "instalike_dns_record" {
  zone_id = var.cloudflare_dns_zone_id
  name    = var.dns_cname_record_value
  value   = lower("${azurerm_linux_web_app.instalike_app.name}.azurewebsites.net")
  type    = "CNAME"
  proxied = true

  depends_on = [
    azurerm_linux_web_app.instalike_app
  ]
}

resource "cloudflare_record" "instalike_dns_txt_record" {
  zone_id = var.cloudflare_dns_zone_id
  name    = "asuid.${var.dns_cname_record_value}"
  value   = azurerm_linux_web_app.instalike_app.custom_domain_verification_id
  type    = "TXT"

  depends_on = [
    azurerm_linux_web_app.instalike_app
  ]
}

resource "azurerm_app_service_custom_hostname_binding" "instalike_app_binding" {
  hostname            = "${var.dns_cname_record_value}.${var.web_application_domain}"
  app_service_name    = azurerm_linux_web_app.instalike_app.name
  resource_group_name = azurerm_resource_group.instalike_resource_group.name

  # Ignore ssl_state and thumbprint as they are managed using
  # "azurerm_app_service_certificate_binding" resource
  lifecycle {
    ignore_changes = [ssl_state, thumbprint]
  }

  depends_on = [
    azurerm_linux_web_app.instalike_app,
    cloudflare_record.instalike_dns_record,
    cloudflare_record.instalike_dns_txt_record
  ]
}

resource "azurerm_app_service_certificate_binding" "instalike_ssl_binding" {
  hostname_binding_id = azurerm_app_service_custom_hostname_binding.instalike_app_binding.id
  certificate_id      = azurerm_app_service_certificate.cloudflare_origin_server_cert.id
  ssl_state           = "SniEnabled"

  depends_on = [
    azurerm_app_service_custom_hostname_binding.instalike_app_binding
  ]
}
