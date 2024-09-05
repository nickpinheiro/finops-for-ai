###############################################################################################################################
# Azure Cost Management Budget Creation Script                                                                                 #
#                                                                                                                              #
# This script creates a budget in Azure Cost Management using the REST API.                                                    #
#                                                                                                                              #
# Features:                                                                                                                    #
# - Creates a budget for a specific subscription and tenant.                                                                   #
# - Sets a specific amount, start date, and end date for the budget.                                                           #
# - Includes two notifications:                                                                                                #
#   - Forecasted_GreaterThan_80_Percent: Notifies when the forecasted amount is greater than 80% of the budget.                #
#   - Actual_GreaterThan_90_Percent: Notifies when the actual amount is greater than 90% of the budget.                        #
# - Notifications are sent to specified contact emails.                                                                        #
#                                                                                                                              #
# Requirements:                                                                                                                #
# - Azure PowerShell module for authentication and REST API calls.                                                             #
# - Input parameters: subscription ID, tenant ID, budget name, contact emails, start date, end date, and amount.               #
###############################################################################################################################

# Define input parameters
$tenantId = "00000000-0000-0000-0000-000000000000"
$subscriptionId = "00000000-0000-0000-0000-000000000000"
$contactEmails = @("your-email1@example.com", "your-email2@example.com")
$startDate = "2024-07-01T00:00:00Z"
$endDate = "2025-12-31T00:00:00Z"
$amount = 1000

# Authenticate and select the subscription and tenant
Connect-AzAccount -TenantId $tenantId -SubscriptionId $subscriptionId

# Fetch subscription details to get the subscription name
$subscription = Get-AzSubscription -SubscriptionId $subscriptionId
$subscriptionName = $subscription.Name

# Define naming convention for budget name
$budgetType = "MONTHLY"
$budgetName = "FINOPS-BUDGET-$budgetType-$subscriptionName"

# Convert budget name to uppercase
$budgetName = $budgetName.ToUpper()

# Prepare the request body
$body = @{
    "properties"= @{
        "category" = "Cost"
        "amount"= $amount
        "timeGrain" = "Monthly"
        "timePeriod"= @{
            "startDate" = $startDate
            "endDate" = $endDate
        }
        "notifications" = @{
            "Forecasted_GreaterThan_80_Percent" = @{
                "enabled" = $true
                "operator" = "GreaterThan"
                "threshold" = 80
                "locale" = "en-us"
                "contactEmails" = $contactEmails
                "thresholdType" = "Forecasted"
            }
            "Actual_GreaterThan_90_Percent" = @{
                "enabled" = $true
                "operator" = "GreaterThan"
                "threshold" = 90
                "locale" = "en-us"
                "contactEmails" = $contactEmails
                "thresholdType" = "Actual"
            }
        }
    }
}

# Get authentication token
$token=(Get-AzAccessToken).token

# Invoke REST API to create the budget
Invoke-RestMethod `
    -Method Put `
    -Headers @{"Authorization"="Bearer $token"} `
    -ContentType "application/json; charset=utf-8" `
    -Body (ConvertTo-Json $body -Depth 10) `
    -Uri "https://management.azure.com/subscriptions/$subscriptionId/providers/Microsoft.Consumption/budgets/$budgetName/?api-version=2021-10-01"