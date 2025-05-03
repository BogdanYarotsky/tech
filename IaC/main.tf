terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0" # Use a specific version range appropriate for your setup
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
  # Terraform will use your Azure CLI login context by default
}

# Generate a random suffix to help ensure unique resource names
resource "random_string" "unique_suffix" {
  length  = 6
  special = false
  upper   = false
}

# --- 1. Resource Group ---
# All resources will be placed in this group in Germany West Central
resource "azurerm_resource_group" "rg" {
  name     = "rg-freetier-simple-${random_string.unique_suffix.result}" # Hardcoded prefix + random suffix
  location = "germanywestcentral"                                     # Hardcoded location

  tags = {
    environment = "Demo"
    purpose     = "SimpleFreeTier"
  }
}

# --- 2. Static Web App (Free Tier) ---
resource "azurerm_static_web_app" "swa" {
  name                = "swa-simple-${random_string.unique_suffix.result}" # Hardcoded prefix + random suffix
  resource_group_name = azurerm_resource_group.rg.name
  location            = "westeurope"
  sku_tier            = "Free" # Explicitly set the Free tier
  sku_size            = "Free"

  # Note: A functional SWA usually needs repository details (repository_url, branch etc.)
  # This only provisions the Azure resource itself.

  tags = azurerm_resource_group.rg.tags # Use tags from the resource group
}

# --- 3. App Service (Container, Linux, Free Tier) ---

# App Service Plan (Defines the underlying compute - Free Tier F1)
resource "azurerm_service_plan" "appserviceplan" {
  name                = "asp-simple-${random_string.unique_suffix.result}" # Hardcoded prefix + random suffix
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"  # Specify Linux
  sku_name            = "F1"     # F1 is the Free tier for Linux App Service Plans

  tags = azurerm_resource_group.rg.tags # Use tags from the resource group
}

# App Service (The actual web app running the container)
# resource "azurerm_linux_web_app" "appservice" {
#   name                = "app-simple-${random_string.unique_suffix.result}" # Hardcoded prefix + random suffix
#   resource_group_name = azurerm_resource_group.rg.name
#   location            = azurerm_resource_group.rg.location
#   service_plan_id     = azurerm_service_plan.appserviceplan.id # Link to the free plan

#   # Configure the site to run a container
#   site_config {
#     application_stack {
#       # Hardcoded example public container image
#       docker_image     = "mcr.microsoft.com/azuredocs/aci-helloworld:latest"
#       docker_image_tag = "latest"
#     }
#     always_on = false # Always On is not available in the Free tier (F1)
#     http2_enabled = true
#   }

#   https_only = true # Recommended for security

#   tags = azurerm_resource_group.rg.tags # Use tags from the resource group
# }