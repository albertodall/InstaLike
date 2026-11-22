variable "name_suffix" {
  description = "Suffix of the resource names: {workload}-{environment}-{region}."
  type        = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "local_auth_enabled" {
  description = "Whether API keys are accepted besides Entra ID tokens."
  type        = bool
}

variable "tags" {
  type = map(string)
}
