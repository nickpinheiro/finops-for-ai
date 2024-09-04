# Authenticate and select the subscription and tenant
$tenantId = "00000000-0000-0000-0000-000000000000"
$subscriptionId = "00000000-0000-0000-0000-000000000000"

Connect-AzAccount -TenantId $tenantId -SubscriptionId $subscriptionId

# Variables
$resourceGroupName = "rg-budget-dev-001"
$budgetName = "Budget1"
$amount = 1000
$startDate = "2024-07-01T00:00:00Z"
$endDate = "2025-12-31T00:00:00Z"
$email = "your-email@example.com"

# Scope
$scope = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName"

# Notifications
$notifications = @{
    ActualGreaterThan80Percent = @{
        Enabled = $true
        Operator = "GreaterThan"
        Threshold = 80
        ThresholdType = "Actual"
        ContactEmails = @($email)
        ContactRoles = @("Contributor", "Reader")
    }
}

# Create Budget
New-AzConsumptionBudget -Name $budgetName -Amount $amount -TimeGrain Monthly -StartDate $startDate -EndDate $endDate -Category Cost